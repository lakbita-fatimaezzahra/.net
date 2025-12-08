using LocationVoitures.Data;
using LocationVoitures.Data.models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class DashboardService
    {
        private readonly LocationContext _context;

        public DashboardService()
        {
            _context = new LocationContext();
        }

        public int CountUsers() => _context.Employes.Count();
        public int CountClients() => _context.Clients.Count();
        public int CountVoitures() => _context.Voitures.Count();
        public int CountLocations() => _context.Locations.Count();
        public Dictionary<string, int> GetLocationsParMois()
        {
            // Récupérer toutes les locations depuis la base
            var locations = _context.Locations.ToList();

            // Grouper par année et mois
            var grouped = locations
                .GroupBy(l => new { l.DateDebut.Year, l.DateDebut.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            // Créer un dictionnaire avec le mois et l'année comme clé
            var result = grouped.ToDictionary(
                g => new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                g => g.Count()
            );

            return result;
        }


        public Dictionary<string, int> GetVoituresParType()
        {
            // ✅ Inclure TypeVoiture dans la requête pour éviter les erreurs de référence null
            var voitures = _context.Voitures
                .Include(v => v.TypeVoiture)
                .ToList();

            return voitures
                .GroupBy(v => v.TypeVoiture != null && !string.IsNullOrEmpty(v.TypeVoiture.Nom) 
                    ? v.TypeVoiture.Nom 
                    : "Non défini")
                .OrderByDescending(g => g.Count()) // Trier par nombre décroissant
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /* ✅ NOUVEAU: Obtenir les revenus par mois
        public Dictionary<string, double> GetRevenusParMois()
        {
            var locations = _context.Locations
                .Where(l => l.Statut == "Terminée")
                .ToList();

            var grouped = locations
                .GroupBy(l => new { l.DateFinReelle.HasValue ? l.DateFinReelle.Value.Year : l.DateFin.Year, 
                                   l.DateFinReelle.HasValue ? l.DateFinReelle.Value.Month : l.DateFin.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            return grouped.ToDictionary(
                g => new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                g => g.Sum(l => l.PrixTotal)
            );
        }*/

        // ✅ NOUVEAU: Obtenir les locations par statut
        public Dictionary<string, int> GetLocationsParStatut()
        {
            return _context.Locations
                .GroupBy(l => l.Statut ?? "Non défini")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // ✅ NOUVEAU: Obtenir les voitures par statut
        public Dictionary<string, int> GetVoituresParStatut()
        {
            return _context.Voitures
                .GroupBy(v => v.Statut ?? "Non défini")
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
