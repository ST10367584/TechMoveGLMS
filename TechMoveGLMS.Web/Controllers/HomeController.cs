using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalClients = await _context.Clients.CountAsync();
        ViewBag.TotalContracts = await _context.Contracts.CountAsync();
        ViewBag.ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
        ViewBag.PendingRequests = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Pending);
        var recentContracts = await _context.Contracts.Include(c => c.Client).OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
        return View(recentContracts);
    }
}