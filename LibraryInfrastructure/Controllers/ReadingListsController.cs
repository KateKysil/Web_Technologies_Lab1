using LibraryInfrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Model;

namespace LibraryInfrastructure.Controllers
{
    public class ReadingListsController : Controller
    {
        private readonly LibraryContext _db;
        private readonly UserManager<User> _userManager;

        public ReadingListsController(LibraryContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var lists = await _db.ReadingLists
                .Where(l => l.OwnerId == userId || l.IsPublic)
                .ToListAsync();

            return View(lists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var list = await _db.ReadingLists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null) return NotFound();

            return View(list);
        }
    }

}
