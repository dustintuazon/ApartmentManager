using ApartmentManager.Data;
using ApartmentManager.Models;
using ApartmentManager.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ApartmentManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public HomeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var transactions = await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();
            var rooms = await _context.Rooms.Where(r => r.UserId == userId).ToListAsync();
            var tenants = await _context.Tenants.Where(t => t.UserId == userId && (t.MoveOutDate == null || t.MoveOutDate > DateOnly.FromDateTime(DateTime.Now))).ToListAsync();
            var dashboardViewModel = new DashboardViewModel
            {
                YearIncome = transactions.Where(t => t.Date.Year == DateTime.Now.Year && t.Purpose == Purpose.Monthly).Sum(t => t.Amount),
                MonthIncome = transactions.Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year && t.UserId == userId && t.Purpose == Purpose.Monthly).Sum(t => t.Amount),
                RoomCount = rooms.Count(),
                TenantCount = tenants.Count(),
                AvailableRooms = rooms.Where(r => r.IsAvailable).Count(),
                UnpaidTenants = tenants.Where(t => !transactions.Any(tr => tr.TenantId == t.Id && (tr.Purpose == Purpose.Monthly || tr.Purpose == Purpose.Advance) && t.MoveInDate.AddMonths(t.MonthsPaid) > DateOnly.FromDateTime(DateTime.Now))).Count()
            };
            return View(dashboardViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
