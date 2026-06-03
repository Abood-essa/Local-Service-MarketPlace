using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVerification(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profile == null)
            {
                TempData["Error"] = "This user does not have a provider profile yet.";
                return RedirectToAction(nameof(Index));
            }

            profile.IsVerified = !profile.IsVerified;
            await _db.SaveChangesAsync();

            // Notify the provider
            _db.Notifications.Add(new Notification
            {
                UserId = id,
                Title = profile.IsVerified ? "Account Verified!" : "Verification Revoked",
                Message = profile.IsVerified
                    ? "Your provider account has been verified. You can now submit offers."
                    : "Your provider verification has been revoked. Please contact support.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Provider verification {(profile.IsVerified ? "granted" : "revoked")} successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}