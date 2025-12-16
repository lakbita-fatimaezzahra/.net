using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class VoitureService
    {
        private readonly LocationContext _context;
        private readonly MaintenanceService _maintenanceService;

        public VoitureService()
        {
            _context = new LocationContext();
            _maintenanceService = new MaintenanceService();
        }

        // ✅ AJOUTÉ: Classe PagedResult pour la pagination
        public class PagedResult<T>
        {
            public List<T> Items { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }

        // Récupérer toutes les voitures avec leurs relations
        public List<Voiture> GetAllVoitures()
        {
            return _context.Voitures
                .Include(v => v.TypeVoiture)
                .Include(v => v.Images)
                .OrderBy(v => v.Id)
                .ToList();
        }
        public List<Voiture> GetVoitureByType()
        {
            return _context.Voitures
              .Include(v => v.TypeVoiture)
              .ToList();

        }

        // Pagination
        public PagedResult<Voiture> GetPagedVoitures(int page, int pageSize)
        {
            var query = _context.Voitures
                .Include(v => v.TypeVoiture)
                .Include(v => v.Images)
                .OrderBy(v => v.Id);

            int totalItems = query.Count();
            int totalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Voiture>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        // Filtre par recherche
        public List<Voiture> Filter(string search, string criterium)
        {
            if (string.IsNullOrWhiteSpace(search))
                return GetAllVoitures();

            search = search.ToLower();
            var query = _context.Voitures
                .Include(v => v.TypeVoiture)
                .Include(v => v.Images)
                .AsQueryable();

            switch (criterium)
            {
                case "Matricule":
                    query = query.Where(v => v.Matricule != null && v.Matricule.ToLower().Contains(search));
                    break;
                case "Marque":
                    query = query.Where(v => v.Marque != null && v.Marque.ToLower().Contains(search));
                    break;
                case "Modele":
                    query = query.Where(v => v.Modele != null && v.Modele.ToLower().Contains(search));
                    break;
                case "Type":
                    query = query.Where(v => v.TypeVoiture != null && v.TypeVoiture.Nom.ToLower().Contains(search));
                    break;
                default:
                    query = query.Where(v =>
                        (v.Matricule != null && v.Matricule.ToLower().Contains(search)) ||
                        (v.Marque != null && v.Marque.ToLower().Contains(search)) ||
                        (v.Modele != null && v.Modele.ToLower().Contains(search)) ||
                        (v.TypeVoiture != null && v.TypeVoiture.Nom.ToLower().Contains(search)));
                    break;
            }

            return query.OrderBy(v => v.Id).ToList();
        }

        public Voiture GetById(int id)
        {
            return _context.Voitures
                .Include(v => v.TypeVoiture)
                .Include(v => v.Images)
                .FirstOrDefault(v => v.Id == id);
        }

        // Ajout avec vérification automatique de maintenance
        public void Add(Voiture voiture)
        {
            _context.Voitures.Add(voiture);
            _context.SaveChanges();

            // Vérifier automatiquement la maintenance pour la nouvelle voiture
            try
            {
                _maintenanceService.VerifierEtCreerAlertes(voiture.Id);
            }
            catch (System.Exception ex)
            {
                // Log l'erreur mais ne bloque pas l'ajout
                System.Diagnostics.Debug.WriteLine($"Erreur vérification maintenance : {ex.Message}");
            }
        }

        // Mise à jour avec vérification automatique si kilométrage change
        public void Update(Voiture voiture)
        {
            var existing = _context.Voitures.Find(voiture.Id);
            if (existing == null) return;

            // Vérifier si le kilométrage a changé
            bool kilometrageChange = existing.Kilometrage != voiture.Kilometrage;
            bool maintenanceChange = existing.DerniereMaintenance != voiture.DerniereMaintenance;

            existing.Matricule = voiture.Matricule;
            existing.Marque = voiture.Marque;
            existing.Modele = voiture.Modele;
            existing.Kilometrage = voiture.Kilometrage;
            existing.PrixJour = voiture.PrixJour;
            existing.Statut = voiture.Statut;
            existing.DerniereMaintenance = voiture.DerniereMaintenance;
            existing.TypeId = voiture.TypeId;

            _context.SaveChanges();

            // Vérifier automatiquement la maintenance si le kilométrage ou la date de maintenance a changé
            if (kilometrageChange || maintenanceChange)
            {
                try
                {
                    _maintenanceService.VerifierEtCreerAlertes(voiture.Id);
                }
                catch (System.Exception ex)
                {
                    // Log l'erreur mais ne bloque pas la mise à jour
                    System.Diagnostics.Debug.WriteLine($"Erreur vérification maintenance : {ex.Message}");
                }
            }
        }

        public void SupprimerVoiture(Voiture voiture)
        {
            var existing = _context.Voitures
                .Include(v => v.Images)
                .FirstOrDefault(v => v.Id == voiture.Id);

            if (existing == null) return;

            if (existing.Images != null)
                _context.ImagesVoitures.RemoveRange(existing.Images);

            _context.Voitures.Remove(existing);
            _context.SaveChanges();
        }

        // ✅ CORRIGÉ: Utiliser la classe VoitureAvecUrgence définie dans MaintenanceService
        public List<Voiture> GetVoituresAvecAlertes()
        {
            return _context.Voitures
                 .Include(v => v.TypeVoiture)
                 .Include(v => v.Images)
                 .OrderBy(v => v.Id)
                 .Where(v => v.Statut == "en maintenance")
                 .ToList();
        }

        // Obtenir le statut de maintenance d'une voiture
        public string GetStatutMaintenance(int voitureId)
        {
            var (necessaire, urgence, _) = _maintenanceService.VerifierMaintenanceAvecUrgence(voitureId);
            return necessaire ? urgence : "🟢 OK";
        }
    }
}