using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("paiements")]
    public class Paiement
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("montant")]
        public double Montant { get; set; }

        [Column("mode_paiement")]
        public string ModePaiement { get; set; }

        [Column("statut")]
        public string Statut { get; set; }

        [Column("date_paiement")]
        public DateTime DatePaiement { get; set; }

        [Column("location_id")]
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
    }
}
