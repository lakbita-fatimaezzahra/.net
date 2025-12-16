using System;
using System.Collections.Generic;

namespace LocationVoitures.Data.Models
{
    /// <summary>
    /// Configuration des intervalles de maintenance selon le type
    /// </summary>
    public static class MaintenanceConfig
    {
        // Définition des intervalles standards
        public static readonly Dictionary<string, IntervallesMaintenance> Intervalles = new Dictionary<string, IntervallesMaintenance>
        {
            { "Vidange", new IntervallesMaintenance { Mois = 6, Kilometres = 10000 } },
            { "Révision", new IntervallesMaintenance { Mois = 12, Kilometres = 30000 } },
            { "Contrôle Technique", new IntervallesMaintenance { Mois = 24, Kilometres = 0 } },
            { "Pneus", new IntervallesMaintenance { Mois = 24, Kilometres = 50000 } },
            { "Freins", new IntervallesMaintenance { Mois = 0, Kilometres = 40000 } },
            { "Batterie", new IntervallesMaintenance { Mois = 36, Kilometres = 0 } },
            { "Climatisation", new IntervallesMaintenance { Mois = 24, Kilometres = 0 } },
            { "Courroie Distribution", new IntervallesMaintenance { Mois = 60, Kilometres = 100000 } },
            { "Filtre Air", new IntervallesMaintenance { Mois = 12, Kilometres = 15000 } },
            { "Liquide Refroidissement", new IntervallesMaintenance { Mois = 24, Kilometres = 40000 } }
        };

        // Seuils d'alerte préventive (jours avant échéance)
        public static readonly Dictionary<string, int> SeuilsAlerteJours = new Dictionary<string, int>
        {
            { "🔴 CRITIQUE", 0 },      // Dépassé
            { "🔴 URGENT", 3 },         // 3 jours ou moins
            { "🟠 ATTENTION", 7 },      // 7 jours ou moins
            { "🟡 À PRÉVOIR", 15 }      // 15 jours ou moins
        };

        // Seuils d'alerte par kilométrage (km avant échéance)
        public static readonly Dictionary<string, double> SeuilsAlerteKm = new Dictionary<string, double>
        {
            { "🔴 CRITIQUE", 0 },       // Dépassé
            { "🔴 URGENT", 300 },       // 300 km ou moins
            { "🟠 ATTENTION", 500 },    // 500 km ou moins
            { "🟡 À PRÉVOIR", 1000 }    // 1000 km ou moins
        };

        /// <summary>
        /// Calculer la prochaine date de maintenance
        /// </summary>
        public static DateTime CalculerProchaineDateMaintenance(string typeMaintenance, DateTime derniereDate)
        {
            if (Intervalles.ContainsKey(typeMaintenance))
            {
                int mois = Intervalles[typeMaintenance].Mois;
                return mois > 0 ? derniereDate.AddMonths(mois) : derniereDate.AddYears(10); // Si pas d'intervalle temps, mettre très loin
            }
            return derniereDate.AddMonths(6); // Défaut 6 mois
        }

        /// <summary>
        /// Calculer le prochain kilométrage de maintenance
        /// </summary>
        public static double CalculerProchainKilometrage(string typeMaintenance, double kilometrageActuel)
        {
            if (Intervalles.ContainsKey(typeMaintenance))
            {
                double km = Intervalles[typeMaintenance].Kilometres;
                return km > 0 ? kilometrageActuel + km : double.MaxValue; // Si pas d'intervalle km, mettre très haut
            }
            return kilometrageActuel + 10000; // Défaut 10,000 km
        }

        /// <summary>
        /// Déterminer le niveau d'urgence basé sur jours et km restants
        /// </summary>
        public static string DeterminerUrgence(int joursRestants, double kmRestants)
        {
            // Cas critique : dépassé
            if (joursRestants < 0 || kmRestants < 0)
                return "🔴 CRITIQUE";

            // Cas urgent
            if (joursRestants <= SeuilsAlerteJours["🔴 URGENT"] ||
                kmRestants <= SeuilsAlerteKm["🔴 URGENT"])
                return "🔴 URGENT";

            // Cas attention
            if (joursRestants <= SeuilsAlerteJours["🟠 ATTENTION"] ||
                kmRestants <= SeuilsAlerteKm["🟠 ATTENTION"])
                return "🟠 ATTENTION";

            // Cas à prévoir
            if (joursRestants <= SeuilsAlerteJours["🟡 À PRÉVOIR"] ||
                kmRestants <= SeuilsAlerteKm["🟡 À PRÉVOIR"])
                return "🟡 À PRÉVOIR";

            return "🟢 OK";
        }

        /// <summary>
        /// Générer un message d'alerte détaillé
        /// </summary>
        public static string GenererMessageAlerte(string typeMaintenance, Voiture voiture,
            int joursRestants, double kmRestants, string urgence)
        {
            string vehicule = $"{voiture.Marque} {voiture.Modele} ({voiture.Matricule})";

            if (joursRestants < 0 && kmRestants < 0)
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} en retard de {Math.Abs(joursRestants)} jour(s) ET {Math.Abs(kmRestants):N0} km dépassés";
            }
            else if (joursRestants < 0)
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} en retard de {Math.Abs(joursRestants)} jour(s)";
            }
            else if (kmRestants < 0)
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} - Kilométrage dépassé de {Math.Abs(kmRestants):N0} km";
            }
            else if (joursRestants == 0)
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} à faire AUJOURD'HUI";
            }
            else if (joursRestants == 1)
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} à faire DEMAIN";
            }
            else
            {
                return $"{urgence} - {vehicule}: {typeMaintenance} dans {joursRestants} jour(s) ou {kmRestants:N0} km";
            }
        }

        /// <summary>
        /// Obtenir la priorité selon l'urgence
        /// </summary>
        public static string ObtenirPriorite(string urgence)
        {
            if (urgence.Contains("CRITIQUE") || urgence.Contains("URGENT"))
                return "Haute";
            if (urgence.Contains("ATTENTION"))
                return "Moyenne";
            return "Basse";
        }
    }

    /// <summary>
    /// Structure pour les intervalles de maintenance
    /// </summary>
    public class IntervallesMaintenance
    {
        public int Mois { get; set; }
        public double Kilometres { get; set; }
    }
}