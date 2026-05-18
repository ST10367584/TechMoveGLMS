using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Models.ViewModels;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    public ContractsController(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    // GET: Contracts with search/filter (LINQ)
    public async Task<IActionResult> Index(ContractSearchViewModel search)
    {
        var query = _context.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).AsQueryable();

        if (search.StartDateFrom.HasValue)
            query = query.Where(c => c.StartDate >= search.StartDateFrom.Value);
        if (search.StartDateTo.HasValue)
            query = query.Where(c => c.StartDate <= search.StartDateTo.Value);
        if (search.StatusFilter.HasValue)
            query = query.Where(c => c.Status == search.StatusFilter.Value);
        if (search.ClientFilter.HasValue)
            query = query.Where(c => c.ClientId == search.ClientFilter.Value);
        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            query = query.Where(c => c.Client!.Name.Contains(search.SearchTerm) || (c.Notes != null && c.Notes.Contains(search.SearchTerm)));

        search.Results = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        search.AllClients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        return View(search);
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);
        return contract == null ? NotFound() : View(contract);
    }

    // GET: Contracts/Create
    public async Task<IActionResult> Create()
    {
        return View(new ContractCreateViewModel
        {
            Contract = new Contract { StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1) },
            Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateViewModel vm)
    {
        ModelState.Remove("Contract.Client");
        if (!ModelState.IsValid)
        {
            vm.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            return View(vm);
        }

        vm.Contract.CreatedAt = DateTime.UtcNow;
        _context.Contracts.Add(vm.Contract);
        await _context.SaveChangesAsync();

        if (vm.SignedAgreement != null)
        {
            try
            {
                var (path, name) = await _fileService.SaveContractPdfAsync(vm.SignedAgreement, vm.Contract.Id);
                vm.Contract.SignedAgreementPath = path;
                vm.Contract.SignedAgreementFileName = name;
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("SignedAgreement", ex.Message);
                vm.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }
        }

        TempData["Success"] = "Contract created.";
        return RedirectToAction(nameof(Details), new { id = vm.Contract.Id });
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();
        return View(new ContractCreateViewModel
        {
            Contract = contract,
            Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractCreateViewModel vm)
    {
        if (id != vm.Contract.Id) return BadRequest();
        ModelState.Remove("Contract.Client");
        if (!ModelState.IsValid)
        {
            vm.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            return View(vm);
        }

        var existing = await _context.Contracts.FindAsync(id);
        if (existing == null) return NotFound();

        existing.ClientId = vm.Contract.ClientId;
        existing.StartDate = vm.Contract.StartDate;
        existing.EndDate = vm.Contract.EndDate;
        existing.Status = vm.Contract.Status;
        existing.ServiceLevel = vm.Contract.ServiceLevel;
        existing.Notes = vm.Contract.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        if (vm.SignedAgreement != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(existing.SignedAgreementPath))
                    _fileService.DeleteFile(existing.SignedAgreementPath);
                var (path, name) = await _fileService.SaveContractPdfAsync(vm.SignedAgreement, id);
                existing.SignedAgreementPath = path;
                existing.SignedAgreementFileName = name;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("SignedAgreement", ex.Message);
                vm.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Contract updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);
        return contract == null ? NotFound() : View(contract);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract != null)
        {
            if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                _fileService.DeleteFile(contract.SignedAgreementPath);
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Contracts/DownloadAgreement/5
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            return NotFound();

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementPath.TrimStart('/'));
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        var fileName = contract.SignedAgreementFileName ?? "agreement.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}