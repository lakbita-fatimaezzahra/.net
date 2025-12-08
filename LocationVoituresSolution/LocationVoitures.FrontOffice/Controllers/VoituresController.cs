using LocationVoitures.Data.models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Data.Entity;

namespace LocationVoitures.FrontOffice.Controllers
{
    public class VoituresController : Controller
    {
        private readonly LocationContext db = new LocationContext();

        public IActionResult Index()
        {
            var voitures = db.Voitures
                .Include(v => v.Images)
                .Include(v => v.TypeVoiture)
                .ToList();

            return View(voitures);
        }
    }
}
