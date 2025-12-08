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

        // ✅ CONSTRUCTEUR SANS PARAMÈTRES
        public AlerteService()
        {
            _context = new LocationContext();
        }

        // ✅ CONSTRUCTEUR AVEC CONTEXTE
        public AlerteService(LocationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Récupérer toutes les alertes
        public List<Alerte> GetAllAlertes()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .OrderByDescending(a => a.DateCreation)
                .ToList();
        }

        // Créer une alerte de maintenance si nécessaire
        public void CreerAlerteMaintenanceSiNecessaire(int voitureId, string message, string urgence)
        {
            // Vérifier si une alerte similaire existe déjà
            var alerteExistante = _context.Alertes
                .FirstOrDefault(a => a.VoitureId == voitureId &&
                                     a.Statut == "Active" &&
                                     a.Message.Contains(urgence));

            if (alerteExistante == null)
            {
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
                // Mettre à jour le message si l'alerte existe déjà
                alerteExistante.Message = message;
                alerteExistante.DateCreation = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void MarquerCommeResolue(int alerteId)
        {
            var alerte = _context.Alertes.Find(alerteId);
            if (alerte != null)
            {
                // Résoudre l'alerte
                alerte.Statut = "Résolue";
                alerte.DateResolution = DateTime.Now;

                // ✔ Récupérer la voiture liée
                var voiture = _context.Voitures.Find(alerte.VoitureId);
                if (voiture != null)
                {
                    // ✔ Mettre la voiture en statut maintenance
                    voiture.Statut = "En Maintenance";
                    voiture.DerniereMaintenance = DateTime.Now;
                }

                _context.SaveChanges();
            }
        }


        // Résoudre toutes les alertes d'une voiture
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

        // Supprimer une alerte
        public void Delete(int alerteId)
        {
            var alerte = _context.Alertes.Find(alerteId);
            if (alerte != null)
            {
                _context.Alertes.Remove(alerte);
                _context.SaveChanges();
            }
        }

        // Compter les alertes par urgence
        public Dictionary<string, int> CompterAlertesParUrgence()
        {
            var alertesActives = _context.Alertes
                .Where(a => a.Statut == "Active")
                .ToList();

            return new Dictionary<string, int>
            {
                ["Critique"] = alertesActives.Count(a => a.Message.Contains("🔴 CRITIQUE") || a.Message.Contains("DÉPASSÉE")),
                ["Urgent"] = alertesActives.Count(a => a.Message.Contains("🔴 URGENT") || a.Message.Contains("DEMAIN")),
                ["Attention"] = alertesActives.Count(a => a.Message.Contains("🟠 ATTENTION")),
                ["À Prévoir"] = alertesActives.Count(a => a.Message.Contains("🟡 À PRÉVOIR"))
            };
        }

        // Obtenir le nombre total d'alertes actives
        public int GetNombreAlertesActives()
        {
            return _context.Alertes.Count(a => a.Statut == "Active");
        }

        // Obtenir les alertes critiques
        public List<Alerte> GetAlertesCritiques()
        {
            return _context.Alertes
                .Include(a => a.Voiture)
                .Where(a => a.Statut == "Active" && a.Message.Contains("🔴 CRITIQUE"))
                .OrderByDescending(a => a.DateCreation)
                .ToList();
        }

        // Obtenir les alertes urgentes
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

        // ✅ NOUVEAU: Créer une alerte pour une maintenance planifiée
        public void CreerAlerteMaintenancePlanifieeSiNecessaire(int voitureId, int maintenanceId, string message, string urgence)
        {
            // Vérifier si une alerte similaire existe déjà pour cette maintenance
            var alerteExistante = _context.Alertes
                .FirstOrDefault(a => a.VoitureId == voitureId &&
                                     a.Statut == "Active" &&
                                     a.TypeAlerte == "Maintenance Planifiée" &&
                                     a.Message.Contains("Maintenance planifiée"));

            if (alerteExistante == null)
            {
                var alerte = new Alerte
                {
                    VoitureId = voitureId,
                    TypeAlerte = "Maintenance Planifiée",
                    Message = message,
                    DateCreation = DateTime.Now,
                    Statut = "Active"
                };

                _context.Alertes.Add(alerte);
                _context.SaveChanges();
            }
            else
            {
                // Mettre à jour le message si l'alerte existe déjà
                alerteExistante.Message = message;
                alerteExistante.DateCreation = DateTime.Now;
                _context.SaveChanges();
            }
        }
    }
}