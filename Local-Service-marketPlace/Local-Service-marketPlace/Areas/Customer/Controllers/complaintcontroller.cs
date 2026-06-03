using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Customer/Complaint/Create?bookingId=X
        public async Task<IActionResult> Create(int bookingId)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null) return NotFound();

            // Only allow complaints on active or completed bookings
            if (booking.BookingStatus == BookingStatus.Cancelled)
            {
                TempData["Error"] = "You cannot file a complaint on a cancelled booking.";
                return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
            }

            // Prevent duplicate complaints
            var existing = await _db.Complaints
                .AnyAsync(c => c.BookingId == bookingId && c.ReportedByUserId == userId);

            if (existing)
            {
                TempData["Error"] = "You have already filed a complaint for this booking.";
                return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
            }

            ViewBag.Booking = booking;
            return View();
        }

        // POST: Customer/Complaint/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, string reason, string description)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null) return NotFound();

            if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(description))
            {
                TempData["Error"] = "Please fill in all required fields.";
                ViewBag.Booking = booking;
                return View();
            }

            var complaint = new Complaint
            {
                BookingId = bookingId,
                ReportedByUserId = userId,
                Reason = reason,
                Description = description,
                Status = ComplaintStatus.Open,
                CreatedAt = DateTime.Now
            };

            _db.Complaints.Add(complaint);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your complaint has been submitted. We will review it shortly.";
            return RedirectToAction(nameof(MyComplaints));
        }

        // GET: Customer/Complaint/MyComplaints
        public async Task<IActionResult> MyComplaints()
        {
            var userId = _userManager.GetUserId(User);

            var complaints = await _db.Complaints
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ServiceRequest)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Where(c => c.ReportedByUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }
    }
}