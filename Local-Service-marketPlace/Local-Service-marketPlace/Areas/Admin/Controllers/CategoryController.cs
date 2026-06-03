using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CategoryController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }


        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };

            if (model.ImageFile != null)
                category.ImageUrl = await SaveImageAsync(model.ImageFile);

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ExistingImageUrl = category.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = await _db.Categories.FindAsync(model.Id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;

            if (model.ImageFile != null)
            {
                DeleteImage(category.ImageUrl);
                category.ImageUrl = await SaveImageAsync(model.ImageFile);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            DeleteImage(category.ImageUrl);
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "categories");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/categories/" + fileName;
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}