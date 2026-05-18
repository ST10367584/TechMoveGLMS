using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Clients
                .Include(c => c.Contracts)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Region.Contains(search) ||
                    c.Email.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderBy(c => c.Name).ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(co => co.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();
            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create() => View(new Client());

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            client.CreatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();      // Persists to SQL Server immediately

            TempData["Success"] = $"Client '{client.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);

            try
            {
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();  // SQL Server updated immediately
                TempData["Success"] = $"Client '{client.Name}' updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clients.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Client deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
