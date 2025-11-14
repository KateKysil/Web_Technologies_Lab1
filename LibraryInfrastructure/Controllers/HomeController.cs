using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LibraryDomain.Model;

namespace LibraryInfrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect("/Identity/Account/Login");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToAction("Details_Admin", "Users", new { id = currentUser.Id });
            }
            else
            {
                return RedirectToAction("Details_User", "Users", new { id = currentUser.Id });
            }
        }
    }
}
