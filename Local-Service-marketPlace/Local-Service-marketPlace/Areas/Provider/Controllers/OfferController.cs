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
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfferController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Provider/Offer/Index
        // Shows all offers the provider has submitted
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var offers = await _db.ServiceOffers
                .Include(o => o.ServiceRequest)
                    .ThenInclude(r => r.Customer)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(r => r.Category)
                .Where(o => o.ProviderProfileId == profile.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Profile = profile;
            return View(offers);
        }

        // POST: Provider/Offer/SubmitFinalPrice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFinalPrice(int offerId, decimal finalPrice)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            var offer = await _db.ServiceOffers
                .Include(o => o.ServiceRequest)
                .FirstOrDefaultAsync(o => o.Id == offerId &&
                                          o.ProviderProfileId == profile.Id);

            if (offer == null) return NotFound();

            if (finalPrice <= 0)
            {
                TempData["Error"] = "Please enter a valid final price.";
                return RedirectToAction(nameof(Index));
            }

            if (offer.Status != OfferStatus.Pending)
            {
                TempData["Error"] = "This offer cannot be updated at this stage.";
                return RedirectToAction(nameof(Index));
            }

            offer.FinalPrice = finalPrice;
            offer.Status = OfferStatus.FinalPriceSubmitted;
            offer.UpdatedAt = DateTime.Now;

            offer.ServiceRequest.Status = ServiceRequestStatus.WaitingForConfirmation;
            offer.ServiceRequest.UpdatedAt = DateTime.Now;

            // Notify customer
            _db.Notifications.Add(new Notification
            {
                UserId = offer.ServiceRequest.CustomerId,
                Title = "Final Price Submitted!",
                Message = $"A provider has submitted a final price of {finalPrice} JD for '{offer.ServiceRequest.Title}'. Please review and accept or reject.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Final price submitted successfully. Waiting for customer approval.";
            return RedirectToAction(nameof(Index));
        }
    }
}