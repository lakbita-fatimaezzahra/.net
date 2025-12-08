using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("alertes")]
    public class Alerte
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("type_alerte")]
        public string TypeAlerte { get; set; }

        [Column("message")]
        public string Message { get; set; }

        // ✅ CORRIGÉ: DateTime au lieu de string
        [Column("dateCreation")]
        public DateTime DateCreation { get; set; }

        // ✅ AJOUTÉ: Date de résolution (nullable)
        [Column("dateResolution")]
        public DateTime? DateResolution { get; set; }

        [Column("statut")]
        public string Statut { get; set; }

        [Column("voiture_id")]
        public int VoitureId { get; set; }

        [ForeignKey("VoitureId")]
        public virtual Voiture Voiture { get; set; }
    }
}