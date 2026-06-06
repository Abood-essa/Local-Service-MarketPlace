using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.Enums;
using Local_Service_marketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public BookingController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Provider/Booking/Index
        public async Task<IActionResult> Index(string? status)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var query = _db.Bookings
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Category)
                .Include(b => b.Customer)
                .Where(b => b.ProviderProfileId == profile.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<BookingStatus>(status, out var parsed))
                query = query.Where(b => b.BookingStatus == parsed);

            ViewBag.Status = status;
            ViewBag.Profile = profile;

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Provider/Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Category)
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Images)
                .Include(b => b.Customer)
                .Include(b => b.ServiceOffer)
                .Include(b => b.Review)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id && b.ProviderProfileId == profile.Id);

            if (booking == null) return NotFound();

            ViewBag.Profile = profile;
            return View(booking);
        }

        // POST: Provider/Booking/MarkInProgress/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInProgress(int id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _db.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            var booking = await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.ServiceRequest)
                .FirstOrDefaultAsync(b => b.Id == id && b.ProviderProfileId == profile.Id);

            if (booking == null) return NotFound();

            if (booking.BookingStatus != BookingStatus.Active)
            {
                TempData["Error"] = "Booking must be Active to mark as In Progress.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.BookingStatus = BookingStatus.InProgress;
            booking.StartedAt = DateTime.Now;
            booking.ServiceRequest.Status = ServiceRequestStatus.InProgress;
            booking.ServiceRequest.UpdatedAt = DateTime.Now;

            _db.Notifications.Add(new Notification
            {
                UserId = booking.CustomerId,
                Title = "Job Started!",
                Message = $"Your provider has started working on '{booking.ServiceRequest.Title}'.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            // ── Email: booking status → InProgress ────────────────────────
            var customerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}";
            _ = _emailService.SendBookingStatusChangedEmailAsync(
                booking.Customer.Email!,
                customerName,
                booking.ServiceRequest.Title,
                booking.Id,
                "InProgress",
                "Your provider has started working on your service request. We'll notify you when it's done.");
            // ──────────────────────────────────────────────────────────────

            TempData["Success"] = "Booking marked as In Progress.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Provider/Booking/MarkDone/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(int id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _db.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            var booking = await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.ServiceRequest)
                .FirstOrDefaultAsync(b => b.Id == id && b.ProviderProfileId == profile.Id);

            if (booking == null) return NotFound();

            if (booking.BookingStatus != BookingStatus.InProgress)
            {
                TempData["Error"] = "Booking must be In Progress before marking as done.";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.BookingStatus = BookingStatus.PendingCompletion;

            _db.Notifications.Add(new Notification
            {
                UserId = booking.CustomerId,
                Title = "Job Completed by Provider!",
                Message = $"Your provider has marked '{booking.ServiceRequest.Title}' as done. Please confirm completion.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            // ── Email: booking status → PendingCompletion ─────────────────
            var customerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}";
            _ = _emailService.SendBookingStatusChangedEmailAsync(
                booking.Customer.Email!,
                customerName,
                booking.ServiceRequest.Title,
                booking.Id,
                "PendingCompletion",
                "Your provider has marked the job as done. Please log in and confirm completion to finalize the booking.");
            // ──────────────────────────────────────────────────────────────

            TempData["Success"] = "Job marked as done. Waiting for customer confirmation.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}