using Microsoft.AspNetCore.Mvc;

namespace Talor_music.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
