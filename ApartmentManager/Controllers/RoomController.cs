using ApartmentManager.Data;
using ApartmentManager.Models;
using ApartmentManager.ViewModels.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public RoomController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.Where(r => r.UserId == _userManager.GetUserId(User)).OrderBy(r => r.Name).ToListAsync();
            var roomViewModels = rooms.Select(r => new ViewRoomViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsAvailable = r.IsAvailable,
                Monthly = r.Monthly,
                MonthsDeposit = r.Deposit,
                MonthsAdvance = r.Advance
            }).ToList();
            return View(roomViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEditRoom(int? id)
        {
            if (id != null)
            {
                var room = await _context.Rooms.FindAsync(id);
                var viewModel = new CreateEditRoomViewModel
                {
                    Id = room.Id,
                    Name = room.Name,
                    Description = room.Description,
                    Monthly = room.Monthly,
                    MonthsDeposit = room.Deposit,
                    MonthsAdvanced = room.Advance
                };
                return View(viewModel);
            }
            return View(new CreateEditRoomViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateEditRoom(int? id, CreateEditRoomViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingRoom = await _context.Rooms.AnyAsync(r=> r.Name == model.Name && r.UserId == _userManager.GetUserId(User) && r.Id != id);
            if(existingRoom)
            {
                ModelState.AddModelError("Name", "A room with this name already exists.");
                return View(model);
            }
            if (id != null)
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    return NotFound();
                }
                room.Name = model.Name;
                room.Description = model.Description;
                room.Monthly = model.Monthly;
                room.Deposit = model.MonthsDeposit;
                room.Advance = model.MonthsAdvanced;
            }
            else
            {
                var newRoom = new Room
                {
                    Name = model.Name,
                    Description = model.Description,
                    Monthly = model.Monthly,
                    Deposit = model.MonthsDeposit,
                    Advance = model.MonthsAdvanced,
                    UserId = _userManager.GetUserId(User)
                };
                _context.Rooms.Add(newRoom);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Room");
        }
    }
}
