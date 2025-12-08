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
        public string Statut { get; set; } // Effectuée, Planifiée, En retard

        [Column("date_creation")]
        public DateTime DateCreation { get; set; }

        // Relation
        [ForeignKey("VoitureId")]
        public virtual Voiture Voiture { get; set; }
    }
}