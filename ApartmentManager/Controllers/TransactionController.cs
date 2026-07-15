using ApartmentManager.Data;
using ApartmentManager.DTOs;
using ApartmentManager.Interfaces;
using ApartmentManager.Models;
using ApartmentManager.ViewModels;
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

        public async Task<IActionResult> Index(DateOnly? filterDate, string currentSearch, string searchString, int? pageNumber)
        {
            ViewData["FilterDate"] = filterDate?.ToString("yyyy-MM-dd");

            if(searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentSearch;
            }

            ViewData["CurrentSearch"] = searchString;

            var transactions = _context.Transactions.Where(t => t.UserId == _userManager.GetUserId(User)).OrderByDescending(t => t.Date).AsNoTracking();

            if(!string.IsNullOrEmpty(searchString))
            {
                transactions = transactions.Where(t => t.Tenant.Name.Contains(searchString));
            }

            if (filterDate != null)
            {
                transactions = transactions.Where(t => t.Date == filterDate);
            }

            var viewModel = transactions.Select(t => new TransactionsViewModel
            {
                TenantName = t.Tenant.Name,
                Amount = t.Amount,
                Date = t.Date,
                MOP = t.ModeOfPayment,
                Purpose = t.Purpose
            });

            int pageSize = 10;
            
            return View(await PaginatedList<TransactionsViewModel>.CreateAsync(viewModel, pageNumber ?? 1, pageSize));
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
            if(viewModel == null) return NotFound();
            if(viewModel.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than zero.");
            }

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
