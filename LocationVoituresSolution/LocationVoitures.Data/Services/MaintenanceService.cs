using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class MaintenanceService
    {
        private readonly LocationContext _context;
        private readonly AlerteService _alerteService;

        public MaintenanceService()
        {
            _context = new LocationContext();
            _alerteService = new AlerteService();
        }

        // Récupérer toutes les maintenances
        public List<Maintenance> GetAllMaintenances()
        {
            return _context.Maintenances
                .Include(m => m.Voiture)
                .Include(m => m.Voiture.TypeVoiture)
                .OrderByDescending(m => m.DateMaintenance)
                .ToList();
        }

        // Récupérer les maintenances d'une voiture
        public List<Maintenance> GetMaintenancesByVoiture(int voitureId)
        {
            return _context.Maintenances
                .Where(m => m.VoitureId == voitureId)
                .OrderByDescending(m => m.DateMaintenance)
                .ToList();
        }

        // Ajouter une maintenance
        public void Add(Maintenance maintenance)
        {
            // 1. Corriger problème datetime SQL
            if (maintenance.DateMaintenance < new DateTime(1900, 1, 1))
                maintenance.DateMaintenance = DateTime.Now;

            maintenance.DateCreation = DateTime.Now;
            _context.Maintenances.Add(maintenance);

            // 2. Mise à jour de la voiture liée
            var voiture = _context.Voitures.Find(maintenance.VoitureId);
            if (voiture != null)
            {
                voiture.DerniereMaintenance = maintenance.DateMaintenance;
                voiture.Kilometrage = maintenance.KilometrageMaintenance;

                // 3. Changer le statut de la voiture
                voiture.Statut = "Maintenance";
            }

            _context.SaveChanges();

            // 4. Vérifier les alertes
            VerifierEtCreerAlertes(maintenance.VoitureId);
        }

        public List<Maintenance> GetMaintenancesPlanifiees()
        {
            return _context.Maintenances
                .Where(m => m.Statut == "Planifiée")
                .OrderBy(m => m.DateMaintenance)
                .ToList();
        }
        public List<Maintenance> GetAllMaintenancesWithVoiture()
        {
            return _context.Maintenances.Include(m => m.Voiture)
                       .OrderByDescending(m => m.DateCreation)
                       .ToList();
        }


        // Mettre à jour une maintenance
        public void Update(Maintenance maintenance)
        {
            var existing = _context.Maintenances.Find(maintenance.Id);
            if (existing == null) return;

            existing.TypeMaintenance = maintenance.TypeMaintenance;
            existing.DateMaintenance = maintenance.DateMaintenance;
            existing.DateProchaineMaintenance = maintenance.DateProchaineMaintenance;
            existing.KilometrageMaintenance = maintenance.KilometrageMaintenance;
            existing.KilometrageProchain = maintenance.KilometrageProchain;
            existing.Cout = maintenance.Cout;
            existing.Garage = maintenance.Garage;
            existing.Description = maintenance.Description;
            existing.Statut = maintenance.Statut;

            _context.SaveChanges();

            // Mettre à jour les alertes
            VerifierEtCreerAlertes(maintenance.VoitureId);
        }

        // Supprimer une maintenance
        public void DeleteMaintenance(int id)
        {
            var maintenance = _context.Maintenances.Find(id);
            if (maintenance != null)
            {
                _context.Maintenances.Remove(maintenance);
                _context.SaveChanges();
            }
        }

       
        // Calculer la prochaine date de maintenance (par défaut 6 mois)
        public DateTime CalculerProchaineDateMaintenance(DateTime derniereMaintenance, int moisIntervalle = 6)
        {
            return derniereMaintenance.AddMonths(moisIntervalle);
        }

        // Calculer le prochain kilométrage de maintenance (par défaut 10 000 km)
        public double CalculerProchainKilometrage(double kilometrageActuel, double intervalleKm = 10000)
        {
            return kilometrageActuel + intervalleKm;
        }

        // Déterminer le niveau d'urgence
        private string DeterminerUrgence(int joursRestants, double kmRestants)
        {
            if (joursRestants < 0 || kmRestants < 0)
                return "🔴 CRITIQUE";
            if (joursRestants <= 3 || kmRestants <= 300)
                return "🔴 URGENT";
            if (joursRestants <= 7 || kmRestants <= 500)
                return "🟠 ATTENTION";
            if (joursRestants <= 15 || kmRestants <= 1000)
                return "🟡 À PRÉVOIR";
            return "🟢 INFO";
        }

        // Calculer les jours restants avant la prochaine maintenance
        public int CalculerJoursRestants(DateTime prochaineMaintenance)
        {
            return (prochaineMaintenance - DateTime.Now).Days;
        }

        // Calculer les km restants avant la prochaine maintenance
        public double CalculerKmRestants(double kilometrageActuel, double kilometrageProchain)
        {
            return kilometrageProchain - kilometrageActuel;
        }

        // ✅ CORRIGÉ: Tuple nommé pour éviter les erreurs
        public (bool Necessaire, string Urgence, string Message) VerifierMaintenanceAvecUrgence(int voitureId)
        {
            var voiture = _context.Voitures.Find(voitureId);
            if (voiture == null) return (false, "🟢 INFO", "Voiture introuvable");

            var derniereMaintenance = GetMaintenancesByVoiture(voitureId)
                .OrderByDescending(m => m.DateMaintenance)
                .FirstOrDefault();

            if (derniereMaintenance == null)
            {
                // Pas de maintenance enregistrée
                var moisDepuisMaintenance = (DateTime.Now - voiture.DerniereMaintenance).TotalDays / 30;
                if (moisDepuisMaintenance >= 6)
                {
                    return (true, "🔴 CRITIQUE",
                        $"Aucune maintenance enregistrée depuis {moisDepuisMaintenance:N0} mois");
                }
                return (false, "🟢 INFO", "Maintenance à jour");
            }

            // Calculer les jours et km restants
            int joursRestants = CalculerJoursRestants(derniereMaintenance.DateProchaineMaintenance);
            double kmRestants = CalculerKmRestants(voiture.Kilometrage, derniereMaintenance.KilometrageProchain);

            // Déterminer l'urgence
            string urgence = DeterminerUrgence(joursRestants, kmRestants);

            // Construire le message
            string message;
            if (joursRestants < 0)
                message = $"Maintenance en retard de {Math.Abs(joursRestants)} jours";
            else if (kmRestants < 0)
                message = $"Kilométrage dépassé de {Math.Abs(kmRestants):N0} km";
            else
                message = $"Maintenance dans {joursRestants} jours ou {kmRestants:N0} km";

            bool necessaire = urgence != "🟢 INFO";

            return (necessaire, urgence, message);
        }

        // Vérifier si une voiture nécessite une maintenance (version simple)
        public bool NecessiteMaintenance(int voitureId)
        {
            var result = VerifierMaintenanceAvecUrgence(voitureId);
            return result.Necessaire;
        }

        // Obtenir les voitures nécessitant une maintenance
        public List<Voiture> GetVoituresNecessitantMaintenance()
        {
            var voitures = _context.Voitures
                .Include(v => v.TypeVoiture)
                .ToList();

            return voitures.Where(v => NecessiteMaintenance(v.Id)).ToList();
        }

        // ✅ CORRIGÉ: Classe dédiée pour les résultats avec urgence
        public class VoitureAvecUrgence
        {
            public Voiture Voiture { get; set; }
            public string Urgence { get; set; }
            public string Message { get; set; }
        }

        // ✅ CORRIGÉ: Retourner une liste d'objets typés
        public List<VoitureAvecUrgence> GetVoituresAvecUrgence()
        {
            var voitures = _context.Voitures
                .Include(v => v.TypeVoiture)
                .ToList();

            var result = new List<VoitureAvecUrgence>();

            foreach (var voiture in voitures)
            {
                var (necessaire, urgence, message) = VerifierMaintenanceAvecUrgence(voiture.Id);
                if (necessaire || urgence != "🟢 INFO")
                {
                    result.Add(new VoitureAvecUrgence
                    {
                        Voiture = voiture,
                        Urgence = urgence,
                        Message = message
                    });
                }
            }

            // Trier par urgence (critique d'abord)
            return result.OrderBy(r =>
            {
                if (r.Urgence.Contains("CRITIQUE")) return 1;
                if (r.Urgence.Contains("URGENT")) return 2;
                if (r.Urgence.Contains("ATTENTION")) return 3;
                if (r.Urgence.Contains("À PRÉVOIR")) return 4;
                return 5;
            }).ToList();
        }

        // Vérifier et créer des alertes pour une voiture
        public void VerifierEtCreerAlertes(int voitureId)
        {
            var (necessaire, urgence, message) = VerifierMaintenanceAvecUrgence(voitureId);

            if (necessaire)
            {
                var voiture = _context.Voitures
                    .Include(v => v.TypeVoiture)
                    .FirstOrDefault(v => v.Id == voitureId);

                if (voiture == null) return;

                string messageComplet = $"{urgence} - {voiture.Marque} {voiture.Modele} ({voiture.Matricule}): {message}";

                // Créer l'alerte avec le niveau d'urgence
                _alerteService.CreerAlerteMaintenanceSiNecessaire(voitureId, messageComplet, urgence);
            }
            else
            {
                // Résoudre les alertes existantes si la maintenance n'est plus nécessaire
                _alerteService.ResoudreAlertesVoiture(voitureId);
            }
        }

        // Vérifier toutes les voitures et créer des alertes
        public void VerifierToutesLesVoitures()
        {
            var voitures = _context.Voitures.ToList();
            foreach (var voiture in voitures)
            {
                VerifierEtCreerAlertes(voiture.Id);
            }
            
            // ✅ NOUVEAU: Vérifier les maintenances planifiées
            VerifierMaintenancesPlanifiees();
        }

        // ✅ NOUVEAU: Vérifier les maintenances planifiées et créer des alertes
        public void VerifierMaintenancesPlanifiees()
        {
            var maintenancesPlanifiees = _context.Maintenances
                .Include(m => m.Voiture)
                .Where(m => m.Statut == "Planifiée")
                .ToList();

            foreach (var maintenance in maintenancesPlanifiees)
            {
                DateTime dateMaintenance = maintenance.DateMaintenance;
                DateTime maintenant = DateTime.Now;
                int joursRestants = (dateMaintenance - maintenant).Days;

                // Mettre à jour le statut si la date est dépassée
                if (joursRestants < 0 && maintenance.Statut == "Planifiée")
                {
                    maintenance.Statut = "En retard";
                    _context.SaveChanges();
                }

                // Créer une alerte si la date est proche ou dépassée
                string urgence = "";
                string message = "";

                if (joursRestants < 0)
                {
                    urgence = "🔴 CRITIQUE";
                    message = $"Maintenance planifiée DÉPASSÉE de {Math.Abs(joursRestants)} jour(s) - {maintenance.Voiture.Marque} {maintenance.Voiture.Modele} ({maintenance.Voiture.Matricule}) - Type: {maintenance.TypeMaintenance}";
                }
                else if (joursRestants <= 1)
                {
                    urgence = "🔴 URGENT";
                    message = $"Maintenance planifiée DEMAIN - {maintenance.Voiture.Marque} {maintenance.Voiture.Modele} ({maintenance.Voiture.Matricule}) - Type: {maintenance.TypeMaintenance}";
                }
                else if (joursRestants <= 3)
                {
                    urgence = "🔴 URGENT";
                    message = $"Maintenance planifiée dans {joursRestants} jour(s) - {maintenance.Voiture.Marque} {maintenance.Voiture.Modele} ({maintenance.Voiture.Matricule}) - Type: {maintenance.TypeMaintenance}";
                }
                else if (joursRestants <= 7)
                {
                    urgence = "🟠 ATTENTION";
                    message = $"Maintenance planifiée dans {joursRestants} jour(s) - {maintenance.Voiture.Marque} {maintenance.Voiture.Modele} ({maintenance.Voiture.Matricule}) - Type: {maintenance.TypeMaintenance}";
                }

                // Créer l'alerte si nécessaire
                if (!string.IsNullOrEmpty(urgence))
                {
                    _alerteService.CreerAlerteMaintenancePlanifieeSiNecessaire(
                        maintenance.VoitureId,
                        maintenance.Id,
                        message,
                        urgence
                    );
                }
            }
        }

        // Obtenir les statistiques de maintenance
        public Dictionary<string, int> GetStatistiquesMaintenance()
        {
            return new Dictionary<string, int>
            {
                ["Total"] = _context.Maintenances.Count(),
                ["Effectuées"] = _context.Maintenances.Count(m => m.Statut == "Effectuée"),
                ["Planifiées"] = _context.Maintenances.Count(m => m.Statut == "Planifiée"),
                ["En retard"] = _context.Maintenances.Count(m => m.Statut == "En retard")
            };
        }
        public Maintenance CreateMaintenance(Maintenance maintenance)
        {
            if (maintenance == null)
                throw new ArgumentNullException(nameof(maintenance));

            // Vérification : la voiture existe ?
            var voiture = _context.Voitures.Find(maintenance.VoitureId);
            if (voiture == null)
                throw new Exception("Voiture introuvable");

            // Ajouter la maintenance
            _context.Maintenances.Add(maintenance);
            _context.SaveChanges();

            return maintenance;
        }

        // Obtenir les statistiques d'urgence
        public Dictionary<string, int> GetStatistiquesUrgence()
        {
            var voituresAvecUrgence = GetVoituresAvecUrgence();

            return new Dictionary<string, int>
            {
                ["Critique"] = voituresAvecUrgence.Count(v => v.Urgence.Contains("CRITIQUE")),
                ["Urgent"] = voituresAvecUrgence.Count(v => v.Urgence.Contains("URGENT")),
                ["Attention"] = voituresAvecUrgence.Count(v => v.Urgence.Contains("ATTENTION")),
                ["À Prévoir"] = voituresAvecUrgence.Count(v => v.Urgence.Contains("À PRÉVOIR"))
            };
        }
    }
}