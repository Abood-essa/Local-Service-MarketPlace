using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.Enums;
using Local_Service_marketPlace.Models.ViewModels;
using Local_Service_marketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public ServiceRequestController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration config,
            IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _config = config;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var userId = _userManager.GetUserId(User);

            var query = _db.ServiceRequests
                .Include(r => r.Category)
                .Include(r => r.Offers)
                .Where(r => r.CustomerId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<ServiceRequestStatus>(status, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            ViewBag.Status = status;

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new ServiceRequestCreateViewModel
            {
                Categories = await GetCategoriesSelectList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesSelectList();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var request = new ServiceRequest
            {
                CustomerId = userId,
                CategoryId = model.CategoryId,
                Title = model.Title,
                Description = model.Description,
                Address = model.Address,
                City = model.City,
                PreferredDate = model.PreferredDate,
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();

            if (model.Images != null && model.Images.Any())
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "requests");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in model.Images)
                {
                    if (file.Length == 0) continue;

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _db.ServiceRequestImages.Add(new ServiceRequestImage
                    {
                        ServiceRequestId = request.Id,
                        ImageUrl = "/uploads/requests/" + fileName,
                        CreatedAt = DateTime.Now
                    });
                }

                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Your service request has been posted!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var request = await _db.ServiceRequests
                .Include(r => r.Category)
                .Include(r => r.Images)
                .Include(r => r.Offers)
                    .ThenInclude(o => o.ProviderProfile)
                        .ThenInclude(p => p.User)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);

            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);

            var request = await _db.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);

            if (request == null) return NotFound();

            if (request.Status == ServiceRequestStatus.Confirmed ||
                request.Status == ServiceRequestStatus.InProgress)
            {
                TempData["Error"] = "Cannot cancel a request that is already in progress.";
                return RedirectToAction(nameof(Details), new { id });
            }

            request.Status = ServiceRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Request cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptFinalPrice(int offerId)
        {
            var userId = _userManager.GetUserId(User);

            var offer = await _db.ServiceOffers
                .Include(o => o.ServiceRequest)
                .Include(o => o.ProviderProfile).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(o => o.Id == offerId &&
                                          o.ServiceRequest.CustomerId == userId);

            if (offer == null) return NotFound();

            if (offer.FinalPrice == null)
            {
                TempData["Error"] = "Provider has not submitted a final price yet.";
                return RedirectToAction(nameof(Details), new { id = offer.ServiceRequestId });
            }

            if (offer.Status != OfferStatus.FinalPriceSubmitted)
            {
                TempData["Error"] = "This offer is not in a state that can be accepted.";
                return RedirectToAction(nameof(Details), new { id = offer.ServiceRequestId });
            }

            var commissionPct = _config.GetValue<double>("MarketplaceSettings:CommissionPercentage");
            var finalPrice = offer.FinalPrice.Value;
            var commissionAmount = finalPrice * (decimal)(commissionPct / 100);
            var providerEarnings = finalPrice - commissionAmount;

            var booking = new Booking
            {
                ServiceRequestId = offer.ServiceRequestId,
                ServiceOfferId = offer.Id,
                CustomerId = userId,
                ProviderProfileId = offer.ProviderProfileId,
                FinalConfirmedPrice = finalPrice,
                CommissionPercentage = commissionPct,
                CommissionAmount = commissionAmount,
                ProviderEarnings = providerEarnings,
                BookingStatus = BookingStatus.Active,
                CreatedAt = DateTime.Now
            };

            _db.Bookings.Add(booking);

            offer.Status = OfferStatus.AcceptedByCustomer;
            offer.ServiceRequest.Status = ServiceRequestStatus.Confirmed;
            offer.ServiceRequest.UpdatedAt = DateTime.Now;

            var otherOffers = await _db.ServiceOffers
                .Where(o => o.ServiceRequestId == offer.ServiceRequestId &&
                             o.Id != offer.Id &&
                             o.Status == OfferStatus.Pending)
                .ToListAsync();

            foreach (var other in otherOffers)
                other.Status = OfferStatus.RejectedByCustomer;

            _db.Notifications.Add(new Notification
            {
                UserId = offer.ProviderProfile.UserId,
                Title = "Offer Accepted!",
                Message = $"Your offer for '{offer.ServiceRequest.Title}' has been accepted. The job is now active.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            // ── Email: booking created → Customer ─────────────────────────
            var customer = await _userManager.FindByIdAsync(userId);
            var customerName = $"{customer.FirstName} {customer.LastName}";
            _ = _emailService.SendBookingCreatedEmailAsync(
                customer.Email!,
                customerName,
                offer.ServiceRequest.Title,
                booking.Id,
                finalPrice);

            // ── Email: offer accepted → Provider ──────────────────────────
            var providerName = $"{offer.ProviderProfile.User.FirstName} {offer.ProviderProfile.User.LastName}";
            _ = _emailService.SendOfferAcceptedEmailAsync(
                offer.ProviderProfile.User.Email!,
                providerName,
                offer.ServiceRequest.Title,
                booking.Id,
                finalPrice);
            // ──────────────────────────────────────────────────────────────

            TempData["Success"] = "Offer accepted! Your booking is now active.";
            return RedirectToAction("Details", "Booking", new { area = "Customer", id = booking.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOffer(int offerId)
        {
            var userId = _userManager.GetUserId(User);

            var offer = await _db.ServiceOffers
                .Include(o => o.ServiceRequest)
                .Include(o => o.ProviderProfile)
                .FirstOrDefaultAsync(o => o.Id == offerId &&
                                          o.ServiceRequest.CustomerId == userId);

            if (offer == null) return NotFound();

            offer.Status = OfferStatus.RejectedByCustomer;
            offer.ServiceRequest.Status = ServiceRequestStatus.Pending;
            offer.ServiceRequest.UpdatedAt = DateTime.Now;

            _db.Notifications.Add(new Notification
            {
                UserId = offer.ProviderProfile.UserId,
                Title = "Offer Rejected",
                Message = $"Your offer for '{offer.ServiceRequest.Title}' was rejected by the customer.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Offer rejected.";
            return RedirectToAction(nameof(Details), new { id = offer.ServiceRequestId });
        }

        private async Task<List<SelectListItem>> GetCategoriesSelectList()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
