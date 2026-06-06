using Local_Service_marketPlace.Models;
using Local_Service_marketPlace.Models.ViewModels;
using Local_Service_marketPlace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Local_Service_marketPlace.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                else if (roles.Contains("Customer"))
                    return RedirectToAction("Index", "ServiceRequest", new { area = "Customer" });
                else if (roles.Contains("Provider"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Provider" });
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var role = (model.Role == "Provider" || model.Role == "Customer")
                    ? model.Role
                    : "Customer";

                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // ── Welcome email ──────────────────────────────────────────
                var fullName = $"{user.FirstName} {user.LastName}";
                _ = _emailService.SendWelcomeEmailAsync(user.Email, fullName, role);
                // ──────────────────────────────────────────────────────────

                if (role == "Provider")
                    return RedirectToAction("Setup", "Profile", new { area = "Provider" });
                else
                    return RedirectToAction("Index", "ServiceRequest", new { area = "Customer" });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}