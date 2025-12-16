using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("maintenances")]
    public class Maintenance
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("voiture_id")]
        public int VoitureId { get; set; }

        [Column("type_maintenance")]
        public string TypeMaintenance { get; set; } // Vidange, Révision, Contrôle Technique, etc.

        [Column("date_maintenance")]
        public DateTime DateMaintenance { get; set; }

        [Column("date_prochaine_maintenance")]
        public DateTime DateProchaineMaintenance { get; set; }

        [Column("kilometrage_maintenance")]
        public double KilometrageMaintenance { get; set; }

        [Column("kilometrage_prochain")]
        public double KilometrageProchain { get; set; }

        [Column("cout")]
        public double Cout { get; set; }

        [Column("garage")]
        public string Garage { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("statut")]
        public string Statut { get; set; } // Planifiée, Effectuée, En cours, Annulée, En retard

        [Column("date_creation")]
        public DateTime DateCreation { get; set; }

        // ✅ NOUVELLES COLONNES À AJOUTER
        [Column("date_execution")]
        public DateTime? DateExecution { get; set; } // Date réelle d'exécution

        [Column("notes_garage")]
        public string NotesGarage { get; set; } // Notes du garage après intervention

        [Column("cout_reel")]
        public double? CoutReel { get; set; } // Coût réel vs estimé

        [Column("employe_id")]
        public int? EmployeId { get; set; } // Qui a géré cette maintenance

        [Column("priorite")]
        public string Priorite { get; set; } // Haute, Moyenne, Basse

        // Relations
        [ForeignKey("VoitureId")]
        public virtual Voiture Voiture { get; set; }

        // ✅ Méthodes utilitaires
        public bool EstEnRetard()
        {
            return DateTime.Now > DateProchaineMaintenance && Statut != "Effectuée";
        }

        public int JoursRestants()
        {
            return (DateProchaineMaintenance - DateTime.Now).Days;
        }

        public double KilometragesRestants(double kilometrageActuel)
        {
            return KilometrageProchain - kilometrageActuel;
        }
    }
}