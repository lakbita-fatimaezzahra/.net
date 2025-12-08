using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class LocationService
    {
        private readonly LocationContext _context;

        public LocationService()
        {
            _context = new LocationContext();
        }

        // 🔹 Vérifier la disponibilité d'un véhicule
        public bool IsVehiculeDisponible(int voitureId, DateTime dateDebut, DateTime dateFin, int? locationIdExclure = null)
        {
            // 1. Vérifier le statut du véhicule
            var voiture = _context.Voitures.Find(voitureId);
            if (voiture == null)
                return false;

            // ❌ Véhicule indisponible si Loué, Maintenance, ou En Maintenance
            if (voiture.Statut == "Loué" || voiture.Statut == "Maintenance" || voiture.Statut == "En Maintenance")
                return false;

            // 2. Vérifier les maintenances planifiées pendant la période
            var maintenancesConflictuelles = _context.Maintenances
                .Where(m => m.VoitureId == voitureId
                       && (m.Statut == "Planifiée" || m.Statut == "En cours")
                       && m.DateMaintenance <= dateFin
                       && m.DateMaintenance >= dateDebut)
                .Any();

            if (maintenancesConflictuelles)
                return false; // ❌ Maintenance planifiée pendant cette période

            // 3. Vérifier les locations conflictuelles
            var locationsConflictuelles = _context.Locations
                .Where(l => l.VoitureId == voitureId
                       && l.Statut != "Terminée"
                       && l.Statut != "Annulée"
                       && (locationIdExclure == null || l.Id != locationIdExclure)
                       && ((l.DateDebut <= dateFin && l.DateFin >= dateDebut)))
                .ToList();

            return !locationsConflictuelles.Any();
        }

        // 🔹 Créer une nouvelle location
        public Location CreerLocation(int clientId, int voitureId, DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Vérification des dates
                if (dateDebut >= dateFin)
                    throw new Exception("La date de fin doit être après la date de début.");

                if (dateDebut < DateTime.Now)
                    throw new Exception("La date de début ne peut pas être dans le passé.");

                // Vérifier la disponibilité
                if (!IsVehiculeDisponible(voitureId, dateDebut, dateFin))
                    throw new Exception("Le véhicule n'est pas disponible aux dates sélectionnées.");

                // Récupérer le véhicule
                var voiture = _context.Voitures.Find(voitureId);
                if (voiture == null)
                    throw new Exception("Véhicule introuvable.");

                // Calcul du prix
                int nbJours = (dateFin - dateDebut).Days;
                if (nbJours == 0) nbJours = 1;
                double prixTotal = nbJours * voiture.PrixJour;

                // Création de la location
                var location = new Location
                {
                    ClientId = clientId,
                    VoitureId = voitureId,
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    PrixTotal = prixTotal,
                    Statut = "En cours",
                    DateCreation = DateTime.Now
                };

                _context.Locations.Add(location);

                // 🔥 Bloquer le véhicule
                voiture.Statut = "Loué";

                _context.SaveChanges();

                // Retourner la location complète
                return GetById(location.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur création location : {ex.Message}");
            }
        }


        // 🔹 Terminer une location
        public void TerminerLocation(int locationId, double kilometrageFinal, double? fraisSupplementaires = 0)
        {
            try
            {
                var location = _context.Locations
                    .Include(l => l.Voiture)
                    .FirstOrDefault(l => l.Id == locationId);

                if (location == null)
                    throw new Exception("Location introuvable.");

                if (location.Statut == "Terminée")
                    throw new Exception("Cette location est déjà terminée.");

                // Sauvegarder le kilométrage initial pour le calcul
                double kmInitial = location.Voiture.Kilometrage;

                // Mise à jour du kilométrage
                location.Voiture.Kilometrage = kilometrageFinal;

                // Calcul des frais supplémentaires
                int nbJours = (location.DateFin - location.DateDebut).Days;
                if (nbJours == 0) nbJours = 1;
                double kmParJour = 200; // Limite par jour
                double kmAutorise = nbJours * kmParJour;
                double kmParcouru = kilometrageFinal - kmInitial;

                if (kmParcouru > kmAutorise)
                {
                    double kmDepassement = kmParcouru - kmAutorise;
                    fraisSupplementaires += kmDepassement * 0.5; // 0.5 MAD par km dépassé
                }

                // Vérifier le retard
                if (DateTime.Now > location.DateFin)
                {
                    int joursRetard = (DateTime.Now - location.DateFin).Days;
                    fraisSupplementaires += joursRetard * location.Voiture.PrixJour * 1.5; // Pénalité 150%
                }

                // Mise à jour du prix total
                location.PrixTotal += fraisSupplementaires ?? 0;
                location.Statut = "Terminée";
                location.DateFinReelle = DateTime.Now; // ✅ AJOUTÉ: Enregistrer la date de fin réelle

                // Libérer le véhicule
                location.Voiture.Statut = "Disponible";

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur terminaison location : {ex.Message}");
            }
        }

        // 🔹 Annuler une location
        public void AnnulerLocation(int locationId)
        {
            try
            {
                var location = _context.Locations
                    .Include(l => l.Voiture)
                    .FirstOrDefault(l => l.Id == locationId);

                if (location == null)
                    throw new Exception("Location introuvable.");

                if (location.Statut == "Terminée")
                    throw new Exception("Impossible d'annuler une location terminée.");

                location.Statut = "Annulée";
                location.Voiture.Statut = "Disponible";

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur annulation location : {ex.Message}");
            }
        }

        // 🔹 Récupérer une location par ID
        public Location GetById(int id)
        {
            return _context.Locations
                .Include(l => l.Client)
                .Include(l => l.Voiture)
                .Include(l => l.Voiture.TypeVoiture)
                .Include(l => l.Paiements)
                .FirstOrDefault(l => l.Id == id);
        }

        // 🔹 Récupérer toutes les locations
        public List<Location> GetAll()
        {
            return _context.Locations
                .Include(l => l.Client)
                .Include(l => l.Voiture)
                .OrderByDescending(l => l.DateDebut)
                .ToList();
        }

        // 🔹 Pagination
        public PagedResult<Location> GetPagedLocations(int page, int pageSize, string searchTerm = "", string statutFilter = "Tous")
        {
            var query = _context.Locations
                .Include(l => l.Client)
                .Include(l => l.Voiture)
                .AsQueryable();

            // Filtrage par recherche
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(l =>
                    l.Client.Nom.ToLower().Contains(searchTerm) ||
                    l.Client.Prenom.ToLower().Contains(searchTerm) ||
                    l.Voiture.Marque.ToLower().Contains(searchTerm) ||
                    l.Voiture.Modele.ToLower().Contains(searchTerm) ||
                    l.Voiture.Matricule.ToLower().Contains(searchTerm));
            }

            // Filtrage par statut
            if (statutFilter != "Tous")
            {
                query = query.Where(l => l.Statut == statutFilter);
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .OrderByDescending(l => l.DateDebut)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Location>
            {
                Items = items,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // 🔹 Locations en cours
        public List<Location> GetLocationsEnCours()
        {
            return _context.Locations
                .Include(l => l.Client)
                .Include(l => l.Voiture)
                .Where(l => l.Statut == "En cours")
                .OrderBy(l => l.DateFin)
                .ToList();
        }

        // 🔹 Locations à venir (dans les 3 prochains jours)
        public List<Location> GetLocationsAVenir()
        {
            DateTime limite = DateTime.Now.AddDays(3);
            return _context.Locations
                .Include(l => l.Client)
                .Include(l => l.Voiture)
                .Where(l => l.Statut == "En cours" && l.DateFin <= limite)
                .OrderBy(l => l.DateFin)
                .ToList();
        }

        // 🔹 Historique d'un client
        public List<Location> GetLocationsByClient(int clientId)
        {
            return _context.Locations
                .Include(l => l.Voiture)
                .Where(l => l.ClientId == clientId)
                .OrderByDescending(l => l.DateDebut)
                .ToList();
        }

        // 🔹 Historique d'un véhicule
        public List<Location> GetLocationsByVehicule(int voitureId)
        {
            return _context.Locations
                .Include(l => l.Client)
                .Where(l => l.VoitureId == voitureId)
                .OrderByDescending(l => l.DateDebut)
                .ToList();
        }

        // 🔹 Statistiques
        public double GetRevenuTotal()
        {
            return _context.Locations
                .Where(l => l.Statut == "Terminée")
                .Sum(l => (double?)l.PrixTotal) ?? 0;
        }

        public int GetNombreLocationsActives()
        {
            return _context.Locations.Count(l => l.Statut == "En cours");
        }
    }
}