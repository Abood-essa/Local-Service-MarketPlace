using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.ViewModels;
using Local_Service_marketPlace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Local_Service_marketPlace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IEmailService emailService)
        {
            _logger = logger;
            _db = db;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _emailService.SendContactEmailAsync(model.Name, model.Email, model.Subject, model.Message);

            TempData["Success"] = "Thank you! Your message has been sent successfully to the admin.";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}