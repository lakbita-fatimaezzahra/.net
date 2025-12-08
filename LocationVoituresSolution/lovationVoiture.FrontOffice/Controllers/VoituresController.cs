using LocationVoitures.Data;
using LocationVoitures.Data.Models;
using LocationVoitures.FrontOffice.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LocationVoitures.Controllers
{
    public class VoituresController : Controller
    {
        private readonly LocationContextCore _context;

        public VoituresController(LocationContextCore context)
        {
            _context = context;
        }

        // Page qui liste toutes les voitures
        public IActionResult Index()
        {
            var voitures = _context.Voitures
        .Include(v => v.Images)             // Charger les images
        .Include(v => v.TypeVoiture)        // Charger le type
        .OrderBy(v => v.Marque)
        .ToList();

            return View("~/Views/Voitures/Index.cshtml", voitures);
        }
    }
}
