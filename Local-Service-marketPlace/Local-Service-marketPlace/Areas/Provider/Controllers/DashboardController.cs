using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Force setup if profile doesn't exist
            var profile = await _db.ProviderProfiles
                .Include(p => p.ProviderCategories)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            // Stats
            var activeBookings = await _db.Bookings
                .CountAsync(b => b.ProviderProfileId == profile.Id &&
                                 (b.BookingStatus == BookingStatus.Active ||
                                  b.BookingStatus == BookingStatus.InProgress));

            var pendingOffers = await _db.ServiceOffers
                .CountAsync(o => o.ProviderProfileId == profile.Id &&
                                 o.Status == OfferStatus.Pending);

            var pendingCompletion = await _db.Bookings
                .CountAsync(b => b.ProviderProfileId == profile.Id &&
                                 b.BookingStatus == BookingStatus.PendingCompletion);

            var totalEarnings = await _db.Bookings
                .Where(b => b.ProviderProfileId == profile.Id &&
                            b.BookingStatus == BookingStatus.Completed)
                .SumAsync(b => (decimal?)b.ProviderEarnings) ?? 0;

            // Latest bookings
            var latestBookings = await _db.Bookings
                .Include(b => b.ServiceRequest).ThenInclude(s => s.Category)
                .Include(b => b.Customer)
                .Where(b => b.ProviderProfileId == profile.Id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Latest reviews
            var latestReviews = await _db.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProviderProfileId == profile.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.Profile = profile;
            ViewBag.ActiveBookings = activeBookings;
            ViewBag.PendingOffers = pendingOffers;
            ViewBag.PendingCompletion = pendingCompletion;
            ViewBag.TotalEarnings = totalEarnings;
            ViewBag.LatestBookings = latestBookings;
            ViewBag.LatestReviews = latestReviews;

            return View();
        }
    }
}