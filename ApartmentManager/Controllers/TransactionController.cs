using ApartmentManager.Data;
using ApartmentManager.Models;
using ApartmentManager.ViewModels.Transaction;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Controllers
{
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public TransactionController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions.Include(t=>t.Tenant).Where(t=> t.UserId == _userManager.GetUserId(User)).OrderBy(t=>t.Date).ToListAsync();

            var viewModel = transactions.Select(t => new TransactionsViewModel
            {
                TenantName = t.Tenant.Name,
                Amount = t.Amount,
                Date = t.Date,
                MOP = t.ModeOfPayment,
                Purpose = t.Purpose
            });

            return View(viewModel);
        }
    }
}
