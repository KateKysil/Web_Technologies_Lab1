using LibraryInfrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Model;
using LibraryInfrastructure.Models;

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

            var myLists = await _db.ReadingLists
                .Where(l => l.OwnerId == userId)
                .ToListAsync();

            var publicLists = await _db.ReadingLists
                .Where(l => l.isPublic && l.OwnerId != userId).Include(l=> l.Owner)
                .ToListAsync();

            var vm = new ReadingListsIndexViewModel
            {
                MyLists = myLists,
                PublicLists = publicLists
            };
            return View(vm);
        }


        public async Task<IActionResult> Details(int id)
        {
            var list = await _db.ReadingLists
                .Include(l => l.Themes)
                    .ThenInclude(t => t.Items)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
                return NotFound();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _db.ReadingLists
                .FirstOrDefaultAsync(l => l.OwnerId == userId);

            if (existing != null)
                return RedirectToAction("Details", new { id = existing.Id });

            var list = new ReadingList
            {
                Title = "My Reading List",
                OwnerId = userId,
                isPublic = false
            };

            _db.ReadingLists.Add(list);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = list.Id });
        }

    }

}
