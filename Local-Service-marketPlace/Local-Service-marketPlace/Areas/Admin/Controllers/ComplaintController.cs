using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models.Enums;
using Local_Service_marketPlace.Services;
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
        private readonly IEmailService _emailService;

        public ComplaintController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Admin/Complaint/Index
        public async Task<IActionResult> Index(string? filter)
        {
            var query = _context.Complaints
                .Include(c => c.ReportedByUser)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ServiceRequest)
                .AsQueryable();

            if (filter == "open")
                query = query.Where(c => c.Status == ComplaintStatus.Open);
            else if (filter == "underreview")
                query = query.Where(c => c.Status == ComplaintStatus.UnderReview);
            else if (filter == "resolved")
                query = query.Where(c => c.Status == ComplaintStatus.Resolved);
            else if (filter == "dismissed")
                query = query.Where(c => c.Status == ComplaintStatus.Dismissed);

            ViewBag.Filter = filter;
            ViewBag.TotalComplaints = await _context.Complaints.CountAsync();
            ViewBag.OpenCount = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Open);
            ViewBag.UnderReviewCount = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.UnderReview);
            ViewBag.ResolvedCount = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Resolved);
            ViewBag.DismissedCount = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Dismissed);

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        // GET: Admin/Complaint/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.ReportedByUser)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ServiceRequest)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ProviderProfile)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null) return NotFound();

            return View(complaint);
        }

        // POST: Admin/Complaint/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ComplaintStatus status, string? adminNotes)
        {
            var complaint = await _context.Complaints
                .Include(c => c.ReportedByUser)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ServiceRequest)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null) return NotFound();

            complaint.Status = status;
            complaint.AdminNotes = adminNotes;

            if (status == ComplaintStatus.Resolved || status == ComplaintStatus.Dismissed)
                complaint.ResolvedAt = DateTime.Now;
            else
                complaint.ResolvedAt = null;

            await _context.SaveChangesAsync();

            // ── Email: complaint status updated → user ─────────────────────
            var fullName = $"{complaint.ReportedByUser.FirstName} {complaint.ReportedByUser.LastName}";
            var serviceTitle = complaint.Booking?.ServiceRequest?.Title ?? "Your service";
            _ = _emailService.SendComplaintStatusUpdatedEmailAsync(
                complaint.ReportedByUser.Email!,
                fullName,
                serviceTitle,
                status.ToString(),
                adminNotes);
            // ──────────────────────────────────────────────────────────────

            TempData["Success"] = "Complaint updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}