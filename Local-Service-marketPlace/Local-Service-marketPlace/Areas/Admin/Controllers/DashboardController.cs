using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models.Enums;
using Local_Service_marketPlace.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            DashboardViewModel vm = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalProviders = await _context.ProviderProfiles.CountAsync(),

                TotalRequests = await _context.ServiceRequests.CountAsync(),

                TotalBookings = await _context.Bookings.CountAsync(),

                PendingComplaints = await _context.Complaints
                    .CountAsync(x => x.Status == ComplaintStatus.Open),

                TotalRevenue = await _context.Payments
                    .SumAsync(x => (decimal?)x.CommissionAmount) ?? 0,

                LatestRequests = await _context.ServiceRequests
                    .Include(x => x.Customer)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                LatestBookings = await _context.Bookings
                    .Include(x => x.Customer)
                    .Include(x => x.ProviderProfile)
                    .ThenInclude(p => p.User)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}