using Local_Service_marketPlace.Data;
using Local_Service_marketPlace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Local_Service_marketPlace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Wallet/Index
        // Full transaction log across all providers
        public async Task<IActionResult> Index(string? filter)
        {
            var query = _context.WalletTransactions
                .Include(w => w.ProviderProfile).ThenInclude(p => p.User)
                .Include(w => w.Booking).ThenInclude(b => b.ServiceRequest)
                .AsQueryable();

            if (filter == "topup")
                query = query.Where(w => w.Type == TransactionType.TopUp);
            else if (filter == "commission")
                query = query.Where(w => w.Type == TransactionType.CommissionDeduction);

            ViewBag.Filter = filter;

            // Provider wallet summary
            var providerWallets = await _context.ProviderProfiles
                .Include(p => p.User)
                .OrderBy(p => p.WalletBalance)
                .ToListAsync();

            ViewBag.ProviderWallets = providerWallets;
            ViewBag.TotalCommissions = await _context.WalletTransactions
                .Where(w => w.Type == TransactionType.CommissionDeduction)
                .SumAsync(w => (decimal?)w.Amount) ?? 0;
            ViewBag.TotalTopUps = await _context.WalletTransactions
                .Where(w => w.Type == TransactionType.TopUp)
                .SumAsync(w => (decimal?)w.Amount) ?? 0;
            ViewBag.ProvidersInDebt = providerWallets.Count(p => p.WalletBalance < 0);

            var transactions = await query
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(transactions);
        }
    }
}