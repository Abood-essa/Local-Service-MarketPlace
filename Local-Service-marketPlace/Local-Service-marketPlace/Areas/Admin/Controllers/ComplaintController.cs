using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var complaints = await _context.Complaints
                .Include(c => c.Booking)
                .Include(c => c.ReportedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ProviderProfile)
                        .ThenInclude(p => p.User)
                .Include(c => c.ReportedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null) return NotFound();

            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ComplaintStatus status, string? adminNotes)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = status;
            complaint.AdminNotes = adminNotes;

            if (status == ComplaintStatus.Resolved || status == ComplaintStatus.Dismissed)
                complaint.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Complaint status updated!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}