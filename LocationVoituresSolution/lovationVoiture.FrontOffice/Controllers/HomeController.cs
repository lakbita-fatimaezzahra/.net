using Microsoft.AspNetCore.Mvc;

namespace LocationVoitures.Controllers
{
    public class HomeController : Controller
    {
        // Page d'accueil
        public IActionResult Index()
        {
            return View();
        }
    }
}
