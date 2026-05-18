using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Models.ViewModels;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyService _currencyService;
    public ServiceRequestsController(ApplicationDbContext context, ICurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    // GET: ServiceRequests (all or filtered by contract)
    public async Task<IActionResult> Index(int? contractId)
    {
        var query = _context.ServiceRequests.Include(sr => sr.Contract).ThenInclude(c => c!.Client).AsQueryable();
        if (contractId.HasValue) query = query.Where(sr => sr.ContractId == contractId.Value);
        return View(await query.OrderByDescending(sr => sr.CreatedAt).ToListAsync());
    }

    // GET: ServiceRequests/Create?contractId=5
    public async Task<IActionResult> Create(int contractId)
    {
        var contract = await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == contractId);
        if (contract == null) return NotFound();

        // WORKFLOW RULE: cannot create if Expired or OnHold
        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
        {
            TempData["Error"] = $"Cannot add service request: contract is {contract.Status}.";
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }

        var rate = await _currencyService.GetUsdToZarRateAsync();
        var vm = new ServiceRequestCreateViewModel
        {
            ContractId = contractId,
            ContractInfo = $"Contract #{contractId} — {contract.Client?.Name} ({contract.ServiceLevel}, {contract.Status})",
            ExchangeRate = rate
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
    {
        var contract = await _context.Contracts.FindAsync(vm.ContractId);
        if (contract == null) return NotFound();

        // Double‑check workflow rule in POST
        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
        {
            TempData["Error"] = $"Cannot add request: contract is {contract.Status}.";
            return RedirectToAction("Details", "Contracts", new { id = vm.ContractId });
        }

        if (!ModelState.IsValid)
        {
            vm.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
            vm.ContractInfo = $"Contract #{vm.ContractId} — {contract.Client?.Name}";
            return View(vm);
        }

        var rate = await _currencyService.GetUsdToZarRateAsync();
        var zar = _currencyService.ConvertUsdToZar(vm.CostUSD, rate);

        var request = new ServiceRequest
        {
            ContractId = vm.ContractId,
            Description = vm.Description,
            CostUSD = vm.CostUSD,
            CostZAR = zar,
            ExchangeRateUsed = rate,
            Status = vm.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Service request added. USD ${vm.CostUSD} = R{zar} (rate {rate:N4})";
        return RedirectToAction("Details", "Contracts", new { id = vm.ContractId });
    }

    // GET: ServiceRequests/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var sr = await _context.ServiceRequests.FindAsync(id);
        return sr == null ? NotFound() : View(sr);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequest sr)
    {
        if (id != sr.Id) return BadRequest();
        var existing = await _context.ServiceRequests.FindAsync(id);
        if (existing == null) return NotFound();

        if (existing.CostUSD != sr.CostUSD)
        {
            var rate = await _currencyService.GetUsdToZarRateAsync();
            existing.CostUSD = sr.CostUSD;
            existing.CostZAR = _currencyService.ConvertUsdToZar(sr.CostUSD, rate);
            existing.ExchangeRateUsed = rate;
        }
        existing.Description = sr.Description;
        existing.Status = sr.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Service request updated.";
        return RedirectToAction("Details", "Contracts", new { id = existing.ContractId });
    }

    // GET: ServiceRequests/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var sr = await _context.ServiceRequests.Include(sr => sr.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(sr => sr.Id == id);
        return sr == null ? NotFound() : View(sr);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sr = await _context.ServiceRequests.FindAsync(id);
        if (sr != null)
        {
            var contractId = sr.ContractId;
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request deleted.";
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }
        return NotFound();
    }

    // AJAX endpoint for live exchange rate
    [HttpGet]
    public async Task<IActionResult> GetRate()
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();
        return Ok(new { rate });
    }
}