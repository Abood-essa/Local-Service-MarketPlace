using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
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
            var vm = new DashboardViewModel
            {
                // Users
                TotalUsers = await _context.Users.CountAsync(),
                TotalProviders = await _context.ProviderProfiles.CountAsync(),
                VerifiedProviders = await _context.ProviderProfiles.CountAsync(p => p.IsVerified),
                PendingVerification = await _context.ProviderProfiles.CountAsync(p => !p.IsVerified),

                // Requests & Bookings
                TotalRequests = await _context.ServiceRequests.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                ActiveBookings = await _context.Bookings.CountAsync(b =>
                                        b.BookingStatus == BookingStatus.Active ||
                                        b.BookingStatus == BookingStatus.InProgress),
                CompletedBookings = await _context.Bookings.CountAsync(b =>
                                        b.BookingStatus == BookingStatus.Completed),

                // Complaints
                PendingComplaints = await _context.Complaints
                                        .CountAsync(x => x.Status == ComplaintStatus.Open),

                // Financial
                TotalRevenue = await _context.Payments
                                   .SumAsync(x => (decimal?)x.CommissionAmount) ?? 0,

                TotalWalletCommissions = await _context.WalletTransactions
                                             .Where(w => w.Type == TransactionType.CommissionDeduction)
                                             .SumAsync(w => (decimal?)w.Amount) ?? 0,

                TotalNegativeBalances = await _context.ProviderProfiles
                                            .Where(p => p.WalletBalance < 0)
                                            .SumAsync(p => (decimal?)p.WalletBalance) ?? 0,

                ProvidersInDebt = await _context.ProviderProfiles
                                      .CountAsync(p => p.WalletBalance < 0),

                // Latest
                LatestRequests = await _context.ServiceRequests
                                     .Include(x => x.Customer)
                                     .OrderByDescending(x => x.CreatedAt)
                                     .Take(5)
                                     .ToListAsync(),

                LatestBookings = await _context.Bookings
                                     .Include(x => x.Customer)
                                     .Include(x => x.ProviderProfile).ThenInclude(p => p.User)
                                     .OrderByDescending(x => x.CreatedAt)
                                     .Take(5)
                                     .ToListAsync()
            };

            return View(vm);
        }
    }
}