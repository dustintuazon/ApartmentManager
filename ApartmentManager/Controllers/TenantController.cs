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

        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentSearch, int? pageNumber)
        {
            if(searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentSearch;
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = searchString;

            var sortOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Name (Asc)", Value = "name_asc" },
                new SelectListItem { Text = "Name (Desc)", Value = "name_desc" },
                new SelectListItem { Text = "Status (Paid)", Value = "status_asc" },
                new SelectListItem { Text = "Status (Unpaid)", Value = "status_desc" }
            };
            ViewBag.SortList = new SelectList(sortOptions, "Value", "Text", sortOrder);

            var tenants = _context.Tenants.Include(r=>r.Room).Where(t => t.UserId == _userManager.GetUserId(User) && (t.MoveOutDate == null || t.MoveOutDate > DateOnly.FromDateTime(DateTime.Now))).AsNoTracking();
            
            if(!string.IsNullOrEmpty(searchString))
            {
                tenants = tenants.Where(t => t.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tenants = tenants.OrderByDescending(t => t.Name);
                    break;
                case "status_desc":
                    tenants = tenants.OrderBy(t => t.MoveInDate.AddMonths(t.MonthsPaid).DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber);
                    break;
                case "status_asc":
                    tenants = tenants.OrderByDescending(t => t.MoveInDate.AddMonths(t.MonthsPaid).DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber);
                    break;
                default:
                    tenants = tenants.OrderBy(t => t.Name);
                    break;
            }

            var tenantsViewModel = tenants.Select(t => new TenantsViewModel
            {
                TenantId = t.Id,
                RoomName = t.Room.Name,
                TenantName = t.Name,
                Monthly = t.Room.Monthly,
                Deposit = t.Deposit,
                Balance = t.Balance,
                DueDate = t.MoveInDate.AddMonths(t.MonthsPaid),
                DaysDue = t.MoveInDate.AddMonths(t.MonthsPaid).DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber,
                Paid = t.MoveInDate.AddMonths(t.MonthsPaid) >= DateOnly.FromDateTime(DateTime.Now),
                MovingOut = t.MoveOutDate != null,
                UseAdvance = t.MoveOutDate.Value.AddMonths(-1) < t.MoveInDate.AddMonths(t.MonthsPaid) && t.Balance <= 0,
                MoveOutDate = t.MoveOutDate
            });

            int pageSize = 8;

            return View(await PaginatedList<TenantsViewModel>.CreateAsync(tenantsViewModel, pageNumber ?? 1, pageSize));
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
                Monthly = tenant.Room.Monthly,
                Balance = tenant.Balance,
                Deposit = tenant.Deposit
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

        [HttpGet]
        public async Task<PartialViewResult> GetTransactionsPartial(int id, int? pageIndex)
        {
            var transactions = _context.Transactions.Where(t => t.TenantId == id && t.UserId == _userManager.GetUserId(User)).AsNoTracking();

            var transactionsViewModel = transactions.Select(t => new TransactionsViewModel
            {
                Amount = t.Amount,
                Date = t.Date,
                MOP = t.ModeOfPayment,
                Purpose = t.Purpose,
                ReferenceNumber = t.ReferenceNumber.ToString() ?? ""
            }).OrderByDescending(t => t.Date);

            var pageSize = 10;

            return PartialView("_TenantTransactionsTablePartial", await PaginatedList<TransactionsViewModel>.CreateAsync(transactionsViewModel, pageIndex ?? 1, pageSize));
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
