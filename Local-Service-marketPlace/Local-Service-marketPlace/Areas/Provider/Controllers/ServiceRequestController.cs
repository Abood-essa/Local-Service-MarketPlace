using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.Enums;
using Local_Service_marketPlace.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Provider/ServiceRequest/Index
        // Browse open requests matching provider's categories
        public async Task<IActionResult> Index(string? city, int? categoryId)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .Include(p => p.ProviderCategories)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var myCategoryIds = profile.ProviderCategories.Select(pc => pc.CategoryId).ToList();

            // Already offered requests — hide them
            var myOfferedRequestIds = await _db.ServiceOffers
                .Where(o => o.ProviderProfileId == profile.Id)
                .Select(o => o.ServiceRequestId)
                .ToListAsync();

            var query = _db.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Category)
                .Include(r => r.Images)
                .Where(r => myCategoryIds.Contains(r.CategoryId) &&
                             r.Status == ServiceRequestStatus.Pending &&
                             !myOfferedRequestIds.Contains(r.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(r => r.City.Contains(city));

            if (categoryId.HasValue)
                query = query.Where(r => r.CategoryId == categoryId.Value);

            ViewBag.City = city;
            ViewBag.CategoryId = categoryId;
            ViewBag.Profile = profile;
            ViewBag.Categories = await _db.Categories
                .Where(c => myCategoryIds.Contains(c.Id))
                .ToListAsync();

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: Provider/ServiceRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var request = await _db.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Category)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            // Check if provider already submitted an offer
            var existingOffer = await _db.ServiceOffers
                .FirstOrDefaultAsync(o => o.ServiceRequestId == id &&
                                          o.ProviderProfileId == profile.Id);

            ViewBag.Profile = profile;
            ViewBag.ExistingOffer = existingOffer;

            return View(request);
        }

        // POST: Provider/ServiceRequest/SubmitOffer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOffer(SubmitOfferViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            // Must be verified
            if (!profile.IsVerified)
            {
                TempData["Error"] = "Your profile must be verified by an admin before you can submit offers.";
                return RedirectToAction(nameof(Details), new { id = model.ServiceRequestId });
            }

            if (profile.WalletBalance < 0)
            {
                TempData["Error"] = "Your wallet balance is negative. Please top up your wallet before submitting offers.";
                return RedirectToAction(nameof(Details), new { id = model.ServiceRequestId });
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your offer details.";
                return RedirectToAction(nameof(Details), new { id = model.ServiceRequestId });
            }

            if (model.EstimatedMinPrice > model.EstimatedMaxPrice)
            {
                TempData["Error"] = "Minimum price cannot be greater than maximum price.";
                return RedirectToAction(nameof(Details), new { id = model.ServiceRequestId });
            }

            // Check for duplicate offer
            var duplicate = await _db.ServiceOffers
                .AnyAsync(o => o.ServiceRequestId == model.ServiceRequestId &&
                               o.ProviderProfileId == profile.Id);

            if (duplicate)
            {
                TempData["Error"] = "You have already submitted an offer for this request.";
                return RedirectToAction(nameof(Details), new { id = model.ServiceRequestId });
            }

            var request = await _db.ServiceRequests
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == model.ServiceRequestId);

            if (request == null) return NotFound();

            _db.ServiceOffers.Add(new ServiceOffer
            {
                ServiceRequestId = model.ServiceRequestId,
                ProviderProfileId = profile.Id,
                EstimatedMinPrice = model.EstimatedMinPrice,
                EstimatedMaxPrice = model.EstimatedMaxPrice,
                Notes = model.Notes,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.Now
            });

            // Update request status
            request.Status = ServiceRequestStatus.OfferReceived;
            request.UpdatedAt = DateTime.Now;

            // Notify customer
            _db.Notifications.Add(new Notification
            {
                UserId = request.CustomerId,
                Title = "New Offer Received!",
                Message = $"A provider has submitted an offer for your request '{request.Title}'.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Your offer has been submitted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Provider/ServiceRequest/SubmitFinalPrice
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
                return RedirectToAction("Details", "Booking",
                    new { area = "Provider", id = offer.ServiceRequest.Id });
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
                Message = $"A provider has submitted a final price of {finalPrice} JD for '{offer.ServiceRequest.Title}'. Please review and accept.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Final price submitted. Waiting for customer approval.";
            return RedirectToAction("Index", "Booking", new { area = "Provider" });
        }
    }
}