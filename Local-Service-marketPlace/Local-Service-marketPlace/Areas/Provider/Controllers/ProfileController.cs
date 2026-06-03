using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApplicationDbContext db,
                                  UserManager<ApplicationUser> userManager,
                                  IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // GET: Provider/Profile/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .Include(p => p.ProviderCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.ReviewsReceived).ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction(nameof(Setup));

            return View(profile);
        }

        // GET: Provider/Profile/Setup
        public async Task<IActionResult> Setup()
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _db.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existing != null)
                return RedirectToAction("Index", "Dashboard", new { area = "Provider" });

            var vm = new ProviderProfileSetupViewModel
            {
                Categories = await GetCategoriesSelectList()
            };

            return View(vm);
        }

        // POST: Provider/Profile/Setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(ProviderProfileSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesSelectList();
                return View(model);
            }

            if (model.SelectedCategoryIds == null || !model.SelectedCategoryIds.Any())
            {
                ModelState.AddModelError("SelectedCategoryIds", "Please select at least one category.");
                model.Categories = await GetCategoriesSelectList();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var idImageUrl = await SaveImageAsync(model.NationalIdImage, "nationalids");

            var profile = new ProviderProfile
            {
                UserId = userId,
                Bio = model.Bio,
                YearsOfExperience = model.YearsOfExperience,
                AvailableFrom = model.AvailableFrom,
                AvailableTo = model.AvailableTo,
                NationalIdImage = idImageUrl,
                IsVerified = false,
                AverageRating = 0,
                CompletedJobsCount = 0,
                WalletBalance = 0,
                CreatedAt = DateTime.Now
            };

            _db.ProviderProfiles.Add(profile);
            await _db.SaveChangesAsync();

            foreach (var categoryId in model.SelectedCategoryIds)
            {
                _db.ProviderCategories.Add(new ProviderCategory
                {
                    ProviderProfileId = profile.Id,
                    CategoryId = categoryId
                });
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Profile created! Waiting for admin verification.";
            return RedirectToAction("Index", "Dashboard", new { area = "Provider" });
        }

        // GET: Provider/Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .Include(p => p.ProviderCategories)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction(nameof(Setup));

            var vm = new ProviderEditProfileViewModel
            {
                FirstName = profile.User.FirstName,
                LastName = profile.User.LastName,
                Bio = profile.Bio,
                YearsOfExperience = profile.YearsOfExperience,
                AvailableFrom = profile.AvailableFrom,
                AvailableTo = profile.AvailableTo,
                ExistingProfileImage = profile.ProfileImage,
                SelectedCategoryIds = profile.ProviderCategories
                    .Select(pc => pc.CategoryId).ToList(),
                Categories = await GetCategoriesSelectList()
            };

            return View(vm);
        }

        // POST: Provider/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProviderEditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesSelectList();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var profile = await _db.ProviderProfiles
                .Include(p => p.ProviderCategories)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            // Update user name
            profile.User.FirstName = model.FirstName;
            profile.User.LastName = model.LastName;
            await _userManager.UpdateAsync(profile.User);

            // Update profile fields
            profile.Bio = model.Bio;
            profile.YearsOfExperience = model.YearsOfExperience;
            profile.AvailableFrom = model.AvailableFrom;
            profile.AvailableTo = model.AvailableTo;

            // Update profile image
            if (model.ProfileImageFile != null)
            {
                if (!string.IsNullOrEmpty(profile.ProfileImage))
                    DeleteImage(profile.ProfileImage);

                profile.ProfileImage = await SaveImageAsync(model.ProfileImageFile, "profiles");
            }

            // Update categories
            var existing = _db.ProviderCategories
                .Where(pc => pc.ProviderProfileId == profile.Id);
            _db.ProviderCategories.RemoveRange(existing);

            if (model.SelectedCategoryIds != null)
            {
                foreach (var catId in model.SelectedCategoryIds)
                {
                    _db.ProviderCategories.Add(new ProviderCategory
                    {
                        ProviderProfileId = profile.Id,
                        CategoryId = catId
                    });
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ─────────────────────────────────────────────────
        private async Task<List<SelectListItem>> GetCategoriesSelectList()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}