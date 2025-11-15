using Microsoft.AspNetCore.Mvc;

namespace LibraryInfrastructure.Controllers
{
    public class WhiteboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
