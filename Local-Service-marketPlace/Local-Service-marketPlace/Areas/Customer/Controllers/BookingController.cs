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
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var userId = _userManager.GetUserId(User);

            var query = _db.Bookings
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Category)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Where(b => b.CustomerId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<BookingStatus>(status, out var parsedStatus))
                query = query.Where(b => b.BookingStatus == parsedStatus);

            ViewBag.Status = status;

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Category)
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Images)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Include(b => b.Review)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

            if (booking == null) return NotFound();

            return View(booking);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCompletion(int id)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ProviderProfile)
                .Include(b => b.ServiceRequest)
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);

            if (booking == null) return NotFound();

            if (booking.BookingStatus != BookingStatus.PendingCompletion)
            {
                TempData["Error"] = "This booking is not waiting for your confirmation.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 1 — Mark booking complete
            booking.BookingStatus = BookingStatus.Completed;
            booking.CompletedAt = DateTime.Now;
            booking.ServiceRequest.Status = ServiceRequestStatus.Completed;
            booking.ServiceRequest.UpdatedAt = DateTime.Now;

            // 2 — Update provider completed jobs count
            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.Id == booking.ProviderProfileId);

            if (profile != null)
            {
                profile.CompletedJobsCount++;

                // 3 — Deduct commission from wallet
                profile.WalletBalance -= booking.CommissionAmount;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    ProviderProfileId = profile.Id,
                    Type = TransactionType.CommissionDeduction,
                    Amount = booking.CommissionAmount,
                    BalanceAfter = profile.WalletBalance,
                    BookingId = booking.Id,
                    Description = $"Commission for job: {booking.ServiceRequest.Title}",
                    CreatedAt = DateTime.Now
                });
            }

            // 4 — Create payment record
            _db.Payments.Add(new Payment
            {
                BookingId = booking.Id,
                Amount = booking.FinalConfirmedPrice,
                CommissionAmount = booking.CommissionAmount,
                PaymentMethod = PaymentMethod.Cash,
                PaymentStatus = PaymentStatus.Paid,
                TransactionDate = DateTime.Now
            });

            // 5 — Notify provider
            _db.Notifications.Add(new Notification
            {
                UserId = booking.ProviderProfile.UserId,
                Title = "Job Completed & Commission Deducted!",
                Message = $"'{booking.ServiceRequest.Title}' confirmed complete. {booking.CommissionAmount} JD commission deducted from your wallet.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Job confirmed as completed! Please leave a review.";
            return RedirectToAction("Create", "Review", new { area = "Customer", bookingId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RaiseComplaint(int bookingId, string reason, string description)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null) return NotFound();

            var existing = await _db.Complaints
                .AnyAsync(c => c.BookingId == bookingId &&
                               c.ReportedByUserId == userId &&
                               c.Status == ComplaintStatus.Open);

            if (existing)
            {
                TempData["Error"] = "You already have an open complaint for this booking.";
                return RedirectToAction(nameof(Details), new { id = bookingId });
            }

            _db.Complaints.Add(new Complaint
            {
                BookingId = bookingId,
                ReportedByUserId = userId,
                Reason = reason,
                Description = description,
                Status = ComplaintStatus.Open,
                CreatedAt = DateTime.Now
            });

            booking.BookingStatus = BookingStatus.Disputed;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Your complaint has been submitted. Admin will review it shortly.";
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}
