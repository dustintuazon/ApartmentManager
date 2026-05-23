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
            var dashboardViewModel = new DashboardViewModel
            {
                YearIncome = await _context.Transactions.Where(t => t.Date.Year == DateTime.Now.Year && t.UserId == userId).SumAsync(t => t.Amount),
                MonthIncome = await _context.Transactions.Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year && t.UserId == userId).SumAsync(t => t.Amount),
                RoomCount = await _context.Rooms.Where(r=>r.UserId == userId).CountAsync(),
                TenantCount = await _context.Tenants.Where(t => t.UserId == userId && (t.Status == Status.Settled || t.Status == Status.MovingOut)).CountAsync(),
                AvailableRooms = await _context.Rooms.Where(r => r.IsAvailable && r.UserId == userId).CountAsync()
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
