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
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int bookingId)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null) return NotFound();

            if (booking.BookingStatus != BookingStatus.Completed)
            {
                TempData["Error"] = "You can only review a completed booking.";
                return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
            }

            if (booking.Review != null)
            {
                TempData["Error"] = "You have already reviewed this booking.";
                return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
            }

            ViewBag.Booking = booking;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, int rating, string? comment)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _db.Bookings
                .Include(b => b.ServiceRequest)
                .Include(b => b.ProviderProfile)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

            if (booking == null) return NotFound();

            if (booking.Review != null)
            {
                TempData["Error"] = "You have already reviewed this booking.";
                return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return RedirectToAction(nameof(Create), new { bookingId });
            }

            _db.Reviews.Add(new Review
            {
                BookingId = bookingId,
                CustomerId = userId,
                ProviderProfileId = booking.ProviderProfileId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            });

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.Id == booking.ProviderProfileId);

            if (profile != null)
            {
                var allRatings = await _db.Reviews
                    .Where(r => r.ProviderProfileId == profile.Id)
                    .Select(r => r.Rating)
                    .ToListAsync();

                allRatings.Add(rating);
                profile.AverageRating = allRatings.Average();
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("Details", "Booking", new { area = "Customer", id = bookingId });
        }
    }
}
