using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Controllers;

public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ClientsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Clients.Include(c => c.Contracts).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || c.Region.Contains(search) || c.Email.Contains(search));
        return View(await query.OrderBy(c => c.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = await _context.Clients.Include(c => c.Contracts).ThenInclude(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);
        return client == null ? NotFound() : View(client);
    }

    public IActionResult Create() => View(new Client());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid) return View(client);
        client.CreatedAt = DateTime.UtcNow;
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Client '{client.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        return client == null ? NotFound() : View(client);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id) return BadRequest();
        if (!ModelState.IsValid) return View(client);
        _context.Update(client);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Client updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var client = await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
        return client == null ? NotFound() : View(client);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client != null) _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Client deleted.";
        return RedirectToAction(nameof(Index));
    }
}