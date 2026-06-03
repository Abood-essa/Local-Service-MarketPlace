using Local_Service_marketPlace.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Controllers
{
    [Authorize]
    public class ProviderPublicController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProviderPublicController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /ProviderPublic/Profile/5
        public async Task<IActionResult> Profile(int id)
        {
            var profile = await _db.ProviderProfiles
                .Include(p => p.User)
                .Include(p => p.ProviderCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.ReviewsReceived).ThenInclude(r => r.Customer)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.ServiceRequest)
                        .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null) return NotFound();

            // Hide profile entirely if wallet is negative
            bool isHidden = profile.WalletBalance < 0;
            ViewBag.IsHidden = isHidden;

            return View(profile);
        }
    }
}