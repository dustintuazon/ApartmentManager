using ApartmentManager.Data;
using ApartmentManager.DTOs;
using ApartmentManager.Interfaces;
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
        private readonly IPaymentService _paymentService;

        public TransactionController(AppDbContext context, UserManager<User> userManager, IPaymentService paymentService)
        {
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(DateOnly? filterDate, string searchName)
        {
            ViewData["FilterDate"] = filterDate?.ToString("yyyy-MM-dd");
            ViewData["SearchName"] = searchName;
            var transactions = await _context.Transactions.Include(t => t.Tenant).Where(t => t.UserId == _userManager.GetUserId(User)).OrderByDescending(t => t.Date).ToListAsync();

            if(!string.IsNullOrEmpty(searchName))
            {
                transactions = transactions.Where(t => t.Tenant.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (filterDate != null)
            {
                transactions = transactions.Where(t => t.Date == filterDate).ToList();
            }

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

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();
            var viewModel = new CreateTransactionViewModel
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int id, CreateTransactionViewModel viewModel)
        {
            var dto = new ProcessPaymentDto
            {
                TenantId = id,
                Amount = viewModel.Amount,
                Purpose = (Purpose)viewModel.Purpose,
                MOP = (ModeOfPayment)viewModel.ModeOfPayment,
                DatePaid = viewModel.PaymentDate,
                UserId = _userManager.GetUserId(User)
            };

            await _paymentService.ProcessPayment(dto);

            return RedirectToAction("Index","Tenant");
        }
    }
}
