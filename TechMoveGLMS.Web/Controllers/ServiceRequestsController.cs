using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Models.ViewModels;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(ApplicationDbContext context,
                                         ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index(int? contractId)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(sr => sr.ContractId == contractId.Value);

            ViewBag.ContractId = contractId;
            return View(await query.OrderByDescending(sr => sr.CreatedAt).ToListAsync());
        }

        // GET: ServiceRequests/Create?contractId=5
        public async Task<IActionResult> Create(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null) return NotFound();

            // ── WORKFLOW GUARD ────────────────────────────────────────────────
            // A ServiceRequest cannot be created if the Contract is Expired or OnHold
            if (contract.Status == ContractStatus.Expired ||
                contract.Status == ContractStatus.OnHold)
            {
                TempData["Error"] =
                    $"Cannot create a service request for a contract with status '{contract.Status}'. " +
                    "The contract must be Active or Draft.";
                return RedirectToAction("Details", "Contracts", new { id = contractId });
            }

            // Fetch live exchange rate (async I/O — good for async/await demonstration)
            var rate = await _currencyService.GetUsdToZarRateAsync();

            var vm = new ServiceRequestCreateViewModel
            {
                ContractId = contractId,
                ContractInfo = $"Contract #{contractId} — {contract.Client?.Name} " +
                               $"({contract.ServiceLevel}, {contract.Status})",
                ExchangeRate = rate
            };

            return View(vm);
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == vm.ContractId);

            if (contract == null) return NotFound();

            // ── WORKFLOW GUARD (also in POST — never trust only the GET guard) ─
            if (contract.Status == ContractStatus.Expired ||
                contract.Status == ContractStatus.OnHold)
            {
                TempData["Error"] =
                    $"Cannot create a service request: contract is '{contract.Status}'.";
                return RedirectToAction("Details", "Contracts", new { id = vm.ContractId });
            }

            if (!ModelState.IsValid)
            {
                vm.ContractInfo = $"Contract #{vm.ContractId} — {contract.Client?.Name}";
                vm.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
                return View(vm);
            }

            // Fetch latest rate for conversion
            var rate = await _currencyService.GetUsdToZarRateAsync();
            var zarAmount = _currencyService.ConvertUsdToZar(vm.CostUSD, rate);

            var serviceRequest = new ServiceRequest
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                CostUSD = vm.CostUSD,
                CostZAR = zarAmount,
                ExchangeRateUsed = rate,
                Status = vm.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();      // Persists to SQL Server immediately

            TempData["Success"] =
                $"Service request created. Cost: ${vm.CostUSD:N2} USD = R{zarAmount:N2} ZAR " +
                $"(rate: {rate:N4})";
            return RedirectToAction("Details", "Contracts", new { id = vm.ContractId });
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sr == null) return NotFound();
            return View(sr);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest sr)
        {
            if (id != sr.Id) return BadRequest();
            ModelState.Remove("Contract");

            if (!ModelState.IsValid) return View(sr);

            var existing = await _context.ServiceRequests.FindAsync(id);
            if (existing == null) return NotFound();

            // If USD cost changed, recalculate ZAR
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
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();

            var contractId = sr.ContractId;
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request deleted.";
            return RedirectToAction("Details", "Contracts", new { id = contractId });
        }

        // GET: ServiceRequests/GetRate — AJAX endpoint for live USD→ZAR preview
        [HttpGet]
        public async Task<IActionResult> GetRate()
        {
            var rate = await _currencyService.GetUsdToZarRateAsync();
            return Json(new { rate });
        }
    }
}
