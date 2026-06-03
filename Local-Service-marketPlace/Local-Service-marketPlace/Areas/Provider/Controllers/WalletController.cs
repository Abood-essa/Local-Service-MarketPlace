using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WalletController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Provider/Wallet/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("Setup", "Profile", new { area = "Provider" });

            var transactions = await _db.WalletTransactions
                .Include(w => w.Booking)
                    .ThenInclude(b => b.ServiceRequest)
                .Where(w => w.ProviderProfileId == profile.Id)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            ViewBag.Profile = profile;
            ViewBag.Transactions = transactions;

            return View();
        }

        // POST: Provider/Wallet/TopUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp(decimal amount)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid amount.";
                return RedirectToAction(nameof(Index));
            }

            profile.WalletBalance += amount;

            _db.WalletTransactions.Add(new WalletTransaction
            {
                ProviderProfileId = profile.Id,
                Type = TransactionType.TopUp,
                Amount = amount,
                BalanceAfter = profile.WalletBalance,
                Description = $"Top-up of {amount} JD",
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{amount} JD added to your wallet successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}