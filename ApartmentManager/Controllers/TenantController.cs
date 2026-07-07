using ApartmentManager.Data;
using ApartmentManager.DTOs;
using ApartmentManager.Interfaces;
using ApartmentManager.Models;
using ApartmentManager.ViewModels;
using ApartmentManager.ViewModels.Tenant;
using ApartmentManager.ViewModels.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Controllers
{
    [Authorize]
    public class TenantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IPaymentService _paymentService;

        public TenantController(AppDbContext context, UserManager<User> userManager, IPaymentService paymentService)
        {
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants.Include(r=>r.Room).Where(t => t.UserId == _userManager.GetUserId(User) && (t.MoveOutDate == null || t.MoveOutDate > DateOnly.FromDateTime(DateTime.Now))).ToListAsync();
            var viewModel = new List<TenantsViewModel>();
            foreach(var tenant in tenants)
            {
                var dueDate = tenant.MoveInDate.AddMonths(tenant.MonthsPaid);
                var useAdvance = false;
                if (tenant.MoveOutDate != null)
                {
                    var monthBeforeMoveOut = tenant.MoveOutDate.Value.AddMonths(-1);
                    if (monthBeforeMoveOut < dueDate)
                    {
                        useAdvance = true;
                    }
                }
                var tenantViewModel = new TenantsViewModel
                {
                    TenantId = tenant.Id,
                    RoomName = tenant.Room?.Name ?? string.Empty,
                    TenantName = tenant.Name,
                    Deposit = tenant.Deposit,
                    Balance = tenant.Balance,
                    DueDate = dueDate,
                    DaysDue = dueDate.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber,
                    Paid = dueDate >= DateOnly.FromDateTime(DateTime.Now),
                    MovingOut = tenant.MoveOutDate != null,
                    UseAdvance = useAdvance,
                    MoveOutDate = tenant.MoveOutDate
                };
                viewModel.Add(tenantViewModel);
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddTenant(int id)
        {
            var viewModelWrapper = new AddTenantTransactionViewModelWrapper();
            var tenant = new CreateEditTenantViewModel();
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                tenant.MoveInDate = DateOnly.FromDateTime(DateTime.Now);
                viewModelWrapper.CreateEditTenantViewModel = tenant;
                viewModelWrapper.RoomName = room.Name;
                viewModelWrapper.Advance = room.Monthly * room.Advance;
                viewModelWrapper.Deposit = room.Monthly * room.Deposit;
                return View(viewModelWrapper);
            }

            return View(viewModelWrapper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTenant(int id, AddTenantTransactionViewModelWrapper viewModelWrapper)
        {
            ModelState.Remove("CreateTransactionViewModel.Purpose");
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var tenant = viewModelWrapper.CreateEditTenantViewModel;
                var transaction = viewModelWrapper.CreateTransactionViewModel;
                if (tenant is null || transaction is null || userId is null) return NotFound();

                var room = await _context.Rooms.FindAsync(id);
                if (room == null) return NotFound();
                var newTenant = new Tenant
                {
                    Name = tenant.Name,
                    Description = tenant.Description,
                    MoveInDate = tenant.MoveInDate,
                    RoomId = room.Id,
                    UserId = userId,
                };
                await _context.Tenants.AddAsync(newTenant);
                await _context.SaveChangesAsync();
                var handleDepositPayment = new ProcessPaymentDto
                {
                    TenantId = newTenant.Id,
                    Amount = tenant.DepositPaid,
                    Purpose = Purpose.Deposit,
                    MOP = (ModeOfPayment)transaction.ModeOfPayment,
                    DatePaid = transaction.PaymentDate,
                    //ReferenceNumber = transaction.ReferenceNumber,
                    UserId = userId
                };
                await _paymentService.ProcessPayment(handleDepositPayment);
                var handleAdvancePayment = handleDepositPayment;
                handleAdvancePayment.Amount = tenant.AdvancePaid;
                handleAdvancePayment.Purpose = Purpose.Advance;
                await _paymentService.ProcessPayment(handleAdvancePayment);
                
                return RedirectToAction("Index");
            }
            return View(viewModelWrapper);
        }

        [HttpGet]
        public async Task<IActionResult> ViewTenant(int id)
        {
            var tenant = await _context.Tenants.Include(r => r.Transactions).Include(r => r.Room).FirstOrDefaultAsync(t => t.Id == id && t.UserId == _userManager.GetUserId(User));
            if (tenant == null) return NotFound();

            var viewModel = new ViewTenantViewModel
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                Description = tenant.Description,
                RoomName = tenant.Room.Name,
                MoveInDate = tenant.MoveInDate,
                MoveOutDate = tenant.MoveOutDate,
                Balance = tenant.Balance,
                Deposit = tenant.Deposit,
                Transactions = tenant.Transactions.Select(t => new TransactionsViewModel
                {
                    Amount = t.Amount,
                    Date = t.Date,
                    MOP = t.ModeOfPayment,
                    Purpose = t.Purpose,
                    ReferenceNumber = t.ReferenceNumber?.ToString() ?? ""
                }).ToList()
            };
            var moveOutDateViewModel = new MoveOutDateViewModel
            {
                ResultMessage = TempData["ResultMessage"]?.ToString() ?? "",
                MoveOutDate = tenant.MoveOutDate ?? DateOnly.FromDateTime(DateTime.Now)
            };
            var viewModelWrapper = new ViewTenantViewModelWrapper
            {
                ViewTenantViewModel = viewModel,
                MoveOutDateViewModel = moveOutDateViewModel
            };
            return View(viewModelWrapper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMoveOutDate(int id, ViewTenantViewModelWrapper viewModel)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            var moveOutDate = viewModel.MoveOutDateViewModel.MoveOutDate;
            if(moveOutDate < DateOnly.FromDateTime(DateTime.Now))
            {
                TempData["ResultMessage"] = "Move out date cannot be in the past.";
                return RedirectToAction("ViewTenant", new { id = id });
            }
            tenant.MoveOutDate = moveOutDate;
            await _context.SaveChangesAsync();

            TempData["ResultMessage"] = "Move out date updated successfully.";
            return RedirectToAction("ViewTenant", new { id = id });
        }
    }
}
