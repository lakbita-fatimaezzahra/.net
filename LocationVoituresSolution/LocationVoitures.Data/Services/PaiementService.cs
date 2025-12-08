using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class PaiementService
    {
        private readonly LocationContext _context;
        private const double TAUX_TAXE = 0.20; // 20% de TVA
        private const double TAUX_ASSURANCE = 0.10; // 10% d'assurance

        public PaiementService()
        {
            _context = new LocationContext();
        }

        // 🔹 Créer un paiement
        public Paiement CreerPaiement(int locationId, double montant, string modePaiement)
        {
            try
            {
                var location = _context.Locations
                    .Include(l => l.Paiements)
                    .FirstOrDefault(l => l.Id == locationId);

                if (location == null)
                    throw new Exception("Location introuvable.");

                // Vérifier que le montant n'excède pas le reste à payer
                double totalPaye = location.Paiements?.Sum(p => p.Montant) ?? 0;
                double resteAPayer = location.PrixTotal - totalPaye;

                if (montant > resteAPayer)
                    throw new Exception($"Le montant ({montant:F2} MAD) dépasse le reste à payer ({resteAPayer:F2} MAD).");

                // Création du paiement
                var paiement = new Paiement
                {
                    LocationId = locationId,
                    Montant = montant,
                    ModePaiement = modePaiement,
                    DatePaiement = DateTime.Now,
                    Statut = (montant >= resteAPayer) ? "Payé" : "Partiel"
                };

                _context.Paiements.Add(paiement);
                _context.SaveChanges();

                // Recharger avec les relations
                return GetById(paiement.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur création paiement : {ex.Message}");
            }
        }

        // 🔹 Calculer le montant total avec taxes et assurance
        public double CalculerMontantTotal(double prixBase)
        {
            double taxes = prixBase * TAUX_TAXE;
            double assurance = prixBase * TAUX_ASSURANCE;
            return prixBase + taxes + assurance;
        }

        // 🔹 Obtenir le détail des frais
        public (double prixBase, double taxes, double assurance, double total) GetDetailsFrais(double prixBase)
        {
            double taxes = prixBase * TAUX_TAXE;
            double assurance = prixBase * TAUX_ASSURANCE;
            double total = prixBase + taxes + assurance;
            return (prixBase, taxes, assurance, total);
        }

        // 🔹 Récupérer un paiement par ID
        public Paiement GetById(int id)
        {
            return _context.Paiements
                .Include(p => p.Location)
                .Include(p => p.Location.Client)
                .Include(p => p.Location.Voiture)
                .FirstOrDefault(p => p.Id == id);
        }

        // 🔹 Récupérer tous les paiements d'une location
        public List<Paiement> GetPaiementsByLocation(int locationId)
        {
            return _context.Paiements
                .Where(p => p.LocationId == locationId)
                .OrderBy(p => p.DatePaiement)
                .ToList();
        }

        // 🔹 Calculer le reste à payer pour une location
        public double GetResteAPayer(int locationId)
        {
            var location = _context.Locations
                .Include(l => l.Paiements)
                .FirstOrDefault(l => l.Id == locationId);

            if (location == null)
                return 0;

            double totalPaye = location.Paiements?.Sum(p => p.Montant) ?? 0;
            return Math.Max(0, location.PrixTotal - totalPaye);
        }

        // 🔹 Vérifier si une location est entièrement payée
        public bool IsLocationPayee(int locationId)
        {
            return GetResteAPayer(locationId) == 0;
        }

        // 🔹 Historique des paiements d'un client
        public List<Paiement> GetPaiementsByClient(int clientId)
        {
            return _context.Paiements
                .Include(p => p.Location)
                .Include(p => p.Location.Voiture)
                .Where(p => p.Location.ClientId == clientId)
                .OrderByDescending(p => p.DatePaiement)
                .ToList();
        }

        // 🔹 Historique des paiements d'un véhicule
        public List<Paiement> GetPaiementsByVehicule(int voitureId)
        {
            return _context.Paiements
                .Include(p => p.Location)
                .Include(p => p.Location.Client)
                .Where(p => p.Location.VoitureId == voitureId)
                .OrderByDescending(p => p.DatePaiement)
                .ToList();
        }

        // 🔹 Pagination
        public PagedResult<Paiement> GetPagedPaiements(int page, int pageSize, string searchTerm = "")
        {
            var query = _context.Paiements
                .Include(p => p.Location)
                .Include(p => p.Location.Client)
                .Include(p => p.Location.Voiture)
                .AsQueryable();

            // Filtrage
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Location.Client.Nom.ToLower().Contains(searchTerm) ||
                    p.Location.Client.Prenom.ToLower().Contains(searchTerm) ||
                    p.ModePaiement.ToLower().Contains(searchTerm));
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .OrderByDescending(p => p.DatePaiement)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Paiement>
            {
                Items = items,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // 🔹 Statistiques
        public double GetRevenuTotal()
        {
            return _context.Paiements.Sum(p => (double?)p.Montant) ?? 0;
        }

        public Dictionary<string, double> GetRevenuParModePaiement()
        {
            return _context.Paiements
                .GroupBy(p => p.ModePaiement)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Montant));
        }

        public double GetRevenuMoisActuel()
        {
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return _context.Paiements
                .Where(p => p.DatePaiement >= debutMois)
                .Sum(p => (double?)p.Montant) ?? 0;
        }

        // 🔹 Annuler un paiement
        public void AnnulerPaiement(int paiementId)
        {
            try
            {
                var paiement = _context.Paiements.Find(paiementId);
                if (paiement == null)
                    throw new Exception("Paiement introuvable.");

                paiement.Statut = "Annulé";
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur annulation paiement : {ex.Message}");
            }
        }
    }
}