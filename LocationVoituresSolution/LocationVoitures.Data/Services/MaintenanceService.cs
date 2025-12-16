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

        #region CRUD de base

        public List<Maintenance> GetAllMaintenances()
        {
            return _context.Maintenances
                .Include(m => m.Voiture)
                .Include(m => m.Voiture.TypeVoiture)
                .OrderByDescending(m => m.DateMaintenance)
                .ToList();
        }

        public List<Maintenance> GetMaintenancesByVoiture(int voitureId)
        {
            return _context.Maintenances
                .Where(m => m.VoitureId == voitureId)
                .OrderByDescending(m => m.DateMaintenance)
                .ToList();
        }

        public List<Maintenance> GetAllMaintenancesWithVoiture()
        {
            return _context.Maintenances
                .Include(m => m.Voiture)
                .OrderByDescending(m => m.DateCreation)
                .ToList();
        }

        public List<Maintenance> GetMaintenancesPlanifiees()
        {
            return _context.Maintenances
                .Include(m => m.Voiture)
                .Where(m => m.Statut == "Planifiée")
                .OrderBy(m => m.DateMaintenance)
                .ToList();
        }

        public Maintenance CreateMaintenance(Maintenance maintenance)
        {
            if (maintenance == null)
                throw new ArgumentNullException(nameof(maintenance));

            var voiture = _context.Voitures.Find(maintenance.VoitureId);
            if (voiture == null)
                throw new Exception("Voiture introuvable");

            _context.Maintenances.Add(maintenance);
            _context.SaveChanges();

            return maintenance;
        }

        public void DeleteMaintenance(int id)
        {
            var maintenance = _context.Maintenances.Find(id);
            if (maintenance != null)
            {
                _context.Maintenances.Remove(maintenance);
                _context.SaveChanges();
            }
        }

        #endregion

        #region Ajout de Maintenance avec Logique Intelligente

        /// <summary>
        /// Ajouter une maintenance avec calcul automatique des prochaines échéances
        /// </summary>
        public void Add(Maintenance maintenance)
        {
            // 1. Valider la date
            if (maintenance.DateMaintenance < new DateTime(1900, 1, 1))
                maintenance.DateMaintenance = DateTime.Now;

            maintenance.DateCreation = DateTime.Now;

            // 2. ✅ Calculer automatiquement les prochaines échéances selon le type
            maintenance.DateProchaineMaintenance = MaintenanceConfig.CalculerProchaineDateMaintenance(
                maintenance.TypeMaintenance,
                maintenance.DateMaintenance);

            maintenance.KilometrageProchain = MaintenanceConfig.CalculerProchainKilometrage(
                maintenance.TypeMaintenance,
                maintenance.KilometrageMaintenance);

            // 3. ✅ Déterminer la priorité
            var voiture = _context.Voitures.Find(maintenance.VoitureId);
            if (voiture != null)
            {
                int joursRestants = (maintenance.DateProchaineMaintenance - DateTime.Now).Days;
                double kmRestants = maintenance.KilometrageProchain - voiture.Kilometrage;
                string urgence = MaintenanceConfig.DeterminerUrgence(joursRestants, kmRestants);

                maintenance.Priorite = MaintenanceConfig.ObtenirPriorite(urgence);

                // 4. Mise à jour de la voiture
                voiture.DerniereMaintenance = maintenance.DateMaintenance;
                voiture.Kilometrage = maintenance.KilometrageMaintenance;
                voiture.Statut = "Maintenance";
            }

            _context.Maintenances.Add(maintenance);
            _context.SaveChanges();

            // 5. Vérifier et créer des alertes
            VerifierEtCreerAlertes(maintenance.VoitureId);
        }

        /// <summary>
        /// Mettre à jour une maintenance
        /// </summary>
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
            existing.NotesGarage = maintenance.NotesGarage;
            existing.CoutReel = maintenance.CoutReel;
            existing.DateExecution = maintenance.DateExecution;

            _context.SaveChanges();

            // Mettre à jour les alertes
            VerifierEtCreerAlertes(maintenance.VoitureId);
        }

        #endregion

        #region Vérification et Création d'Alertes

        /// <summary>
        /// ✅ Vérifier TOUS les types de maintenance pour une voiture
        /// </summary>
        /// <summary>
        /// ✅ Vérifier TOUS les types de maintenance pour une voiture
        /// </summary>
        public void VerifierEtCreerAlertes(int voitureId)
        {
            var voiture = _context.Voitures
                .Include(v => v.TypeVoiture)
                .FirstOrDefault(v => v.Id == voitureId);

            if (voiture == null) return;

            System.Diagnostics.Debug.WriteLine($"[VerifierEtCreerAlertes] Vérification pour voiture {voitureId}");

            // ✅ Parcourir TOUS les types de maintenance configurés
            foreach (var typeConfig in MaintenanceConfig.Intervalles)
            {
                string typeMaintenance = typeConfig.Key;

                // Récupérer la dernière maintenance de ce type
                var derniereMaintenance = _context.Maintenances
                    .Where(m => m.VoitureId == voitureId &&
                                m.TypeMaintenance == typeMaintenance)
                    .OrderByDescending(m => m.DateMaintenance)
                    .FirstOrDefault();

                if (derniereMaintenance == null)
                {
                    // ✅ Pas de maintenance de ce type enregistrée
                    var moisDepuis = (DateTime.Now - voiture.DerniereMaintenance).TotalDays / 30;

                    if (moisDepuis >= typeConfig.Value.Mois && typeConfig.Value.Mois > 0)
                    {
                        string message = $"🔴 CRITIQUE - {voiture.Marque} {voiture.Modele} ({voiture.Matricule}): " +
                                       $"Aucune {typeMaintenance} enregistrée depuis {moisDepuis:N0} mois";

                        _alerteService.CreerAlerteMaintenanceSiNecessaire(
                            voitureId,
                            message,
                            "🔴 CRITIQUE");
                    }
                }
                else
                {
                    // ✅ IMPORTANT : Ignorer si la maintenance vient d'être effectuée (moins de 1 heure)
                    if (derniereMaintenance.Statut == "Effectuée" &&
                        derniereMaintenance.DateExecution.HasValue &&
                        (DateTime.Now - derniereMaintenance.DateExecution.Value).TotalHours < 1)
                    {
                        System.Diagnostics.Debug.WriteLine($"[VerifierEtCreerAlertes] {typeMaintenance} vient d'être effectuée, skip");
                        continue;
                    }

                    // ✅ Calculer les jours et km restants
                    int joursRestants = (derniereMaintenance.DateProchaineMaintenance - DateTime.Now).Days;
                    double kmRestants = derniereMaintenance.KilometrageProchain - voiture.Kilometrage;

                    System.Diagnostics.Debug.WriteLine($"[VerifierEtCreerAlertes] {typeMaintenance}: {joursRestants} jours, {kmRestants:N0} km");

                    // ✅ Déterminer l'urgence
                    string urgence = MaintenanceConfig.DeterminerUrgence(joursRestants, kmRestants);

                    // ✅ Créer alerte si nécessaire
                    if (urgence != "🟢 OK")
                    {
                        string message = MaintenanceConfig.GenererMessageAlerte(
                            typeMaintenance,
                            voiture,
                            joursRestants,
                            kmRestants,
                            urgence);

                        _alerteService.CreerAlerteMaintenanceSiNecessaire(
                            voitureId,
                            message,
                            urgence);

                        System.Diagnostics.Debug.WriteLine($"[VerifierEtCreerAlertes] Alerte créée: {urgence}");
                    }
                    else
                    {
                        // ✅ Résoudre les alertes existantes pour ce type si maintenance OK
                        _alerteService.ResoudreAlertesParType(voitureId, typeMaintenance);

                        // ✅ Mettre à jour le statut si en retard
                        if (joursRestants < 0 && derniereMaintenance.Statut == "Planifiée")
                        {
                            derniereMaintenance.Statut = "En retard";
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ✅ Vérifier toutes les voitures (à exécuter quotidiennement)
        /// </summary>
        public void VerifierToutesLesVoitures()
        {
            var voitures = _context.Voitures
                .Where(v => v.Statut != "Hors service") // ✅ Exclusion véhicules hors service
                .ToList();

            foreach (var voiture in voitures)
            {
                VerifierEtCreerAlertes(voiture.Id);
            }

            // ✅ Vérifier les maintenances planifiées
            VerifierMaintenancesPlanifiees();
        }

        /// <summary>
        /// ✅ Vérifier les maintenances planifiées et créer des alertes
        /// </summary>
        /// <summary>
        /// ✅ Vérifier les maintenances planifiées et créer des alertes
        /// VERSION CORRIGÉE - Sans String.Format dans LINQ
        /// </summary>
        public void VerifierMaintenancesPlanifiees()
        {
            // ✅ Récupérer d'abord TOUTES les maintenances en mémoire
            var maintenancesPlanifiees = _context.Maintenances
                .Include(m => m.Voiture)
                .Where(m => m.Statut == "Planifiée")
                .ToList(); // ✅ IMPORTANT: ToList() exécute la requête AVANT le traitement

            foreach (var maintenance in maintenancesPlanifiees)
            {
                // ✅ Maintenant on est en mémoire, pas dans LINQ to Entities
                int joursRestants = (maintenance.DateMaintenance - DateTime.Now).Days;

                // ✅ Mettre à jour le statut si la date est dépassée
                if (joursRestants < 0)
                {
                    maintenance.Statut = "En retard";
                    _context.SaveChanges();
                }

                // ✅ Créer une alerte si la date est proche ou dépassée
                string urgence = "";
                string message = "";

                // ✅ Construire les strings EN MÉMOIRE (pas dans LINQ)
                string marque = maintenance.Voiture.Marque ?? "";
                string modele = maintenance.Voiture.Modele ?? "";
                string matricule = maintenance.Voiture.Matricule ?? "";
                string typeMain = maintenance.TypeMaintenance ?? "";

                string vehiculeInfo = marque + " " + modele + " (" + matricule + ")";

                if (joursRestants < 0)
                {
                    urgence = "🔴 CRITIQUE";
                    int joursDepasses = Math.Abs(joursRestants);
                    message = "Maintenance planifiée DÉPASSÉE de " + joursDepasses.ToString() + " jour(s) - " +
                             vehiculeInfo + " - Type: " + typeMain;
                }
                else if (joursRestants <= 1)
                {
                    urgence = "🔴 URGENT";
                    if (joursRestants == 0)
                    {
                        message = "Maintenance planifiée AUJOURD'HUI - " + vehiculeInfo + " - Type: " + typeMain;
                    }
                    else
                    {
                        message = "Maintenance planifiée DEMAIN - " + vehiculeInfo + " - Type: " + typeMain;
                    }
                }
                else if (joursRestants <= 3)
                {
                    urgence = "🔴 URGENT";
                    message = "Maintenance planifiée dans " + joursRestants.ToString() + " jour(s) - " +
                             vehiculeInfo + " - Type: " + typeMain;
                }
                else if (joursRestants <= 7)
                {
                    urgence = "🟠 ATTENTION";
                    message = "Maintenance planifiée dans " + joursRestants.ToString() + " jour(s) - " +
                             vehiculeInfo + " - Type: " + typeMain;
                }

                // ✅ Créer l'alerte si nécessaire
                if (!string.IsNullOrEmpty(urgence))
                {
                    _alerteService.CreerAlerteMaintenancePlanifieeSiNecessaire(
                        maintenance.VoitureId,
                        maintenance.Id,
                        message,
                        urgence);
                }
            }
        }


        #endregion

        #region Exécution de Maintenance

        /// <summary>
        /// ✅ Marquer une maintenance comme effectuée
        /// </summary>
        /// <summary>
        /// ✅ Marquer une maintenance comme effectuée - VERSION CORRIGÉE
        /// </summary>
        public void MarquerCommeEffectuee(int maintenanceId, double coutReel, string notesGarage = "")
        {
            var maintenance = _context.Maintenances
                .Include(m => m.Voiture)
                .FirstOrDefault(m => m.Id == maintenanceId);

            if (maintenance == null)
                throw new Exception("Maintenance introuvable");

            // 1. Mettre à jour la maintenance
            maintenance.Statut = "Effectuée";
            maintenance.DateExecution = DateTime.Now;
            maintenance.CoutReel = coutReel;
            maintenance.NotesGarage = notesGarage;

            // 2. Mettre à jour la date de prochaine maintenance
            maintenance.DateProchaineMaintenance = MaintenanceConfig.CalculerProchaineDateMaintenance(
                maintenance.TypeMaintenance,
                DateTime.Now);

            maintenance.KilometrageProchain = MaintenanceConfig.CalculerProchainKilometrage(
                maintenance.TypeMaintenance,
                maintenance.Voiture.Kilometrage);

            // 3. Mettre à jour la voiture
            if (maintenance.Voiture != null)
            {
                maintenance.Voiture.DerniereMaintenance = DateTime.Now;
                maintenance.Voiture.Statut = "Disponible";

                System.Diagnostics.Debug.WriteLine($"[MarquerCommeEffectuee] Voiture {maintenance.VoitureId} mise à jour");
            }

            // 4. ✅ IMPORTANT : Sauvegarder AVANT de résoudre les alertes
            _context.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"[MarquerCommeEffectuee] Maintenance sauvegardée, résolution des alertes...");

            // 5. ✅ Résoudre TOUTES les alertes de maintenance pour cette voiture et ce type
            _alerteService.ResoudreAlertesVoiture(maintenance.VoitureId);
            _alerteService.ResoudreAlertesParType(maintenance.VoitureId, maintenance.TypeMaintenance);

            System.Diagnostics.Debug.WriteLine($"[MarquerCommeEffectuee] ✅ Alertes résolues");

            // 6. ✅ NE PAS RE-VÉRIFIER immédiatement - la maintenance vient d'être effectuée
            // La prochaine vérification se fera lors du prochain cycle automatique
        }
        #endregion

        #region Statistiques et Rapports

        public Dictionary<string, int> GetStatistiquesMaintenance()
        {
            return new Dictionary<string, int>
            {
                ["Total"] = _context.Maintenances.Count(),
                ["Effectuées"] = _context.Maintenances.Count(m => m.Statut == "Effectuée"),
                ["Planifiées"] = _context.Maintenances.Count(m => m.Statut == "Planifiée"),
                ["En retard"] = _context.Maintenances.Count(m => m.Statut == "En retard"),
                ["En cours"] = _context.Maintenances.Count(m => m.Statut == "En cours")
            };
        }

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

        /// <summary>
        /// ✅ Obtenir les maintenances à venir (7 prochains jours)
        /// </summary>
        public List<Maintenance> GetMaintenancesAVenir(int jours = 7)
        {
            DateTime limite = DateTime.Now.AddDays(jours);
            return _context.Maintenances
                .Include(m => m.Voiture)
                .Where(m => m.Statut == "Planifiée" &&
                           m.DateMaintenance <= limite &&
                           m.DateMaintenance >= DateTime.Now)
                .OrderBy(m => m.DateMaintenance)
                .ToList();
        }

        /// <summary>
        /// ✅ Coût total des maintenances sur une période
        /// </summary>
        public double GetCoutMaintenances(DateTime debut, DateTime fin)
        {
            return _context.Maintenances
                .Where(m => m.DateExecution >= debut &&
                           m.DateExecution <= fin &&
                           m.CoutReel.HasValue)
                .Sum(m => m.CoutReel.Value);
        }

        #endregion

        #region Classes et Méthodes Existantes

        public class VoitureAvecUrgence
        {
            public Voiture Voiture { get; set; }
            public string Urgence { get; set; }
            public string Message { get; set; }
        }

        public List<VoitureAvecUrgence> GetVoituresAvecUrgence()
        {
            var voitures = _context.Voitures
                .Include(v => v.TypeVoiture)
                .Where(v => v.Statut != "Hors service")
                .ToList();

            var result = new List<VoitureAvecUrgence>();

            foreach (var voiture in voitures)
            {
                var (necessaire, urgence, message) = VerifierMaintenanceAvecUrgence(voiture.Id);
                if (necessaire || urgence != "🟢 OK")
                {
                    result.Add(new VoitureAvecUrgence
                    {
                        Voiture = voiture,
                        Urgence = urgence,
                        Message = message
                    });
                }
            }

            return result.OrderBy(r =>
            {
                if (r.Urgence.Contains("CRITIQUE")) return 1;
                if (r.Urgence.Contains("URGENT")) return 2;
                if (r.Urgence.Contains("ATTENTION")) return 3;
                if (r.Urgence.Contains("À PRÉVOIR")) return 4;
                return 5;
            }).ToList();
        }

        public (bool Necessaire, string Urgence, string Message) VerifierMaintenanceAvecUrgence(int voitureId)
        {
            var voiture = _context.Voitures.Find(voitureId);
            if (voiture == null) return (false, "🟢 OK", "Voiture introuvable");

            // ✅ Vérifier la maintenance la plus urgente parmi tous les types
            string urgenceGlobale = "🟢 OK";
            string messageGlobal = "Toutes les maintenances sont à jour";
            bool necessaire = false;

            foreach (var typeConfig in MaintenanceConfig.Intervalles)
            {
                var derniereMaintenance = GetMaintenancesByVoiture(voitureId)
                    .Where(m => m.TypeMaintenance == typeConfig.Key)
                    .OrderByDescending(m => m.DateMaintenance)
                    .FirstOrDefault();

                if (derniereMaintenance != null)
                {
                    int joursRestants = (derniereMaintenance.DateProchaineMaintenance - DateTime.Now).Days;
                    double kmRestants = derniereMaintenance.KilometrageProchain - voiture.Kilometrage;

                    string urgence = MaintenanceConfig.DeterminerUrgence(joursRestants, kmRestants);

                    if (urgence != "🟢 OK")
                    {
                        necessaire = true;

                        // ✅ Garder l'urgence la plus critique
                        if (urgence.Contains("CRITIQUE"))
                        {
                            urgenceGlobale = urgence;
                            messageGlobal = $"{typeConfig.Key}: " +
                                (joursRestants < 0 ? $"en retard de {Math.Abs(joursRestants)} jours" :
                                 kmRestants < 0 ? $"kilométrage dépassé de {Math.Abs(kmRestants):N0} km" :
                                 $"dans {joursRestants} jours ou {kmRestants:N0} km");
                        }
                        else if (urgenceGlobale == "🟢 OK" ||
                                (urgence.Contains("URGENT") && !urgenceGlobale.Contains("CRITIQUE")))
                        {
                            urgenceGlobale = urgence;
                            messageGlobal = $"{typeConfig.Key}: dans {joursRestants} jours ou {kmRestants:N0} km";
                        }
                    }
                }
            }

            return (necessaire, urgenceGlobale, messageGlobal);
        }

        public List<Voiture> GetVoituresNecessitantMaintenance()
        {
            var voitures = _context.Voitures
                .Include(v => v.TypeVoiture)
                .ToList();

            return voitures.Where(v => {
                var (necessaire, _, _) = VerifierMaintenanceAvecUrgence(v.Id);
                return necessaire;
            }).ToList();
        }

        public bool NecessiteMaintenance(int voitureId)
        {
            var result = VerifierMaintenanceAvecUrgence(voitureId);
            return result.Necessaire;
        }

        public int CalculerJoursRestants(DateTime prochaineMaintenance)
        {
            return (prochaineMaintenance - DateTime.Now).Days;
        }

        public double CalculerKmRestants(double kilometrageActuel, double kilometrageProchain)
        {
            return kilometrageProchain - kilometrageActuel;
        }

        #endregion
    }
}