using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Category)
                .Include(r => r.Offers)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Category)
                .Include(r => r.Images)
                .Include(r => r.Offers)
                    .ThenInclude(o => o.ProviderProfile)
                        .ThenInclude(p => p.User)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ServiceRequestStatus status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request deleted!";
            return RedirectToAction(nameof(Index));
        }
    }
}