using LocationVoitures.Data.models;
using LocationVoitures.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LocationVoitures.Data.Services
{
    public class AlerteService
    {
        private readonly LocationContext _context;

        public AlerteService()
        {
            _context = new LocationContext();
        }

        public AlerteService(LocationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<Alerte> GetAllAlertes()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .OrderByDescending(a => a.DateCreation)
                .ToList();
        }

        /// <summary>
        /// ✅ Créer une alerte de maintenance (évite les doublons par type + urgence)
        /// </summary>
        public void CreerAlerteMaintenanceSiNecessaire(int voitureId, string message, string urgence)
        {
            // ✅ Extraire le type de maintenance du message pour éviter les doublons
            string typeMaintenance = ExtraireTypeMaintenance(message);

            // Vérifier si une alerte similaire existe déjà pour ce type
            var alerteExistante = _context.Alertes
                .FirstOrDefault(a => a.VoitureId == voitureId &&
                                     a.Statut == "Active" &&
                                     a.TypeAlerte == "Maintenance" &&
                                     a.Message.Contains(typeMaintenance));

            if (alerteExistante == null)
            {
                // Créer une nouvelle alerte
                var alerte = new Alerte
                {
                    VoitureId = voitureId,
                    TypeAlerte = "Maintenance",
                    Message = message,
                    DateCreation = DateTime.Now,
                    Statut = "Active"
                };

                _context.Alertes.Add(alerte);
                _context.SaveChanges();
            }
            else
            {
                // ✅ Mettre à jour l'alerte existante si l'urgence a changé
                if (alerteExistante.Message != message)
                {
                    alerteExistante.Message = message;
                    alerteExistante.DateCreation = DateTime.Now; // Rafraîchir la date
                    _context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// ✅ Créer une alerte pour une maintenance planifiée
        /// </summary>
        public void CreerAlerteMaintenancePlanifieeSiNecessaire(int voitureId, int maintenanceId, string message, string urgence)
        {
            // Vérifier si une alerte similaire existe déjà pour cette maintenance
            var alerteExistante = _context.Alertes
                .FirstOrDefault(a => a.VoitureId == voitureId &&
                                     a.Statut == "Active" &&
                                     a.TypeAlerte == "Maintenance Planifiée" &&
                                     a.Message.Contains($"ID {maintenanceId}"));

            if (alerteExistante == null)
            {
                var alerte = new Alerte
                {
                    VoitureId = voitureId,
                    TypeAlerte = "Maintenance Planifiée",
                    Message = $"{message} [ID {maintenanceId}]", // ✅ Ajouter l'ID pour éviter les doublons
                    DateCreation = DateTime.Now,
                    Statut = "Active"
                };

                _context.Alertes.Add(alerte);
                _context.SaveChanges();
            }
            else
            {
                // Mettre à jour le message si l'alerte existe déjà
                if (alerteExistante.Message != message)
                {
                    alerteExistante.Message = $"{message} [ID {maintenanceId}]";
                    alerteExistante.DateCreation = DateTime.Now;
                    _context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// ✅ Résoudre une alerte et mettre la voiture en maintenance
        /// </summary>
        public void MarquerCommeResolue(int alerteId)
        {
            var alerte = _context.Alertes.Find(alerteId);
            if (alerte != null)
            {
                // Résoudre l'alerte
                alerte.Statut = "Résolue";
                alerte.DateResolution = DateTime.Now;

                // Récupérer la voiture liée
                var voiture = _context.Voitures.Find(alerte.VoitureId);
                if (voiture != null)
                {
                    // Mettre la voiture en statut maintenance
                    voiture.Statut = "En Maintenance";
                    voiture.DerniereMaintenance = DateTime.Now;
                }

                _context.SaveChanges();
            }
        }

        /// <summary>
        /// ✅ Résoudre toutes les alertes d'une voiture
        /// </summary>
        public void ResoudreAlertesVoiture(int voitureId)
        {
            var alertes = _context.Alertes
                .Where(a => a.VoitureId == voitureId && a.Statut == "Active")
                .ToList();

            foreach (var alerte in alertes)
            {
                alerte.Statut = "Résolue";
                alerte.DateResolution = DateTime.Now;
            }

            if (alertes.Any())
            {
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// ✅ NOUVEAU: Résoudre les alertes par type de maintenance
        /// </summary>
        public void ResoudreAlertesParType(int voitureId, string typeMaintenance)
        {
            var alertes = _context.Alertes
                .Where(a => a.VoitureId == voitureId &&
                           a.Statut == "Active" &&
                           a.Message.Contains(typeMaintenance))
                .ToList();

            foreach (var alerte in alertes)
            {
                alerte.Statut = "Résolue";
                alerte.DateResolution = DateTime.Now;
            }

            if (alertes.Any())
            {
                _context.SaveChanges();
            }
        }

        public void Delete(int alerteId)
        {
            var alerte = _context.Alertes.Find(alerteId);
            if (alerte != null)
            {
                _context.Alertes.Remove(alerte);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Compter les alertes par urgence
        /// </summary>
        public Dictionary<string, int> CompterAlertesParUrgence()
        {
            var alertesActives = _context.Alertes
                .Where(a => a.Statut == "Active")
                .ToList();

            return new Dictionary<string, int>
            {
                ["Critique"] = alertesActives.Count(a => a.Message.Contains("🔴 CRITIQUE") || a.Message.Contains("DÉPASSÉE")),
                ["Urgent"] = alertesActives.Count(a => a.Message.Contains("🔴 URGENT") || a.Message.Contains("DEMAIN") || a.Message.Contains("AUJOURD'HUI")),
                ["Attention"] = alertesActives.Count(a => a.Message.Contains("🟠 ATTENTION")),
                ["À Prévoir"] = alertesActives.Count(a => a.Message.Contains("🟡 À PRÉVOIR"))
            };
        }

        public int GetNombreAlertesActives()
        {
            return _context.Alertes.Count(a => a.Statut == "Active");
        }

        public List<Alerte> GetAlertesCritiques()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .Where(a => a.Statut == "Active" && a.Message.Contains("🔴 CRITIQUE"))
                .OrderByDescending(a => a.DateCreation)
                .ToList();
        }

        public List<Alerte> GetAlertesUrgentes()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .Where(a => a.Statut == "Active" &&
                           (a.Message.Contains("🔴 CRITIQUE") || a.Message.Contains("🔴 URGENT")))
                .OrderByDescending(a => a.DateCreation)
                .ToList();
        }

        public int CompterAlertesPrioritaires()
        {
            return _context.Alertes.Count(a =>
                a.Statut == "Active" &&
                (a.Message.Contains("🔴 CRITIQUE") || a.Message.Contains("🔴 URGENT"))
            );
        }

        /// <summary>
        /// ✅ Extraire le type de maintenance du message pour éviter les doublons
        /// </summary>
        private string ExtraireTypeMaintenance(string message)
        {
            var types = new[] { "Vidange", "Révision", "Contrôle Technique", "Pneus",
                              "Freins", "Batterie", "Climatisation", "Courroie Distribution",
                              "Filtre Air", "Liquide Refroidissement" };

            foreach (var type in types)
            {
                if (message.Contains(type))
                    return type;
            }

            return "Maintenance générale";
        }

        /// <summary>
        /// ✅ NOUVEAU: Obtenir les alertes groupées par voiture
        /// </summary>
        public Dictionary<int, List<Alerte>> GetAlertesGroupeesParVoiture()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .Where(a => a.Statut == "Active")
                .GroupBy(a => a.VoitureId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// ✅ NOUVEAU: Obtenir le nombre d'alertes par véhicule
        /// </summary>
        public Dictionary<int, int> CompterAlertesParVoiture()
        {
            return _context.Alertes
                .Where(a => a.Statut == "Active")
                .GroupBy(a => a.VoitureId)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}