using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("locations")]
    public class Location
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("date_debut")]
        public DateTime DateDebut { get; set; }

        [Column("date_fin")]
        public DateTime DateFin { get; set; }

        [Column("prix_total")]
        public double PrixTotal { get; set; }

        [Column("statut")]
        public string Statut { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [Column("voiture_id")]
        public int VoitureId { get; set; }

        [ForeignKey("VoitureId")]
        public virtual Voiture Voiture { get; set; }

        // ✅ NOUVELLES PROPRIÉTÉS AJOUTÉES
        [Column("date_creation")]
        public DateTime DateCreation { get; set; }

        [Column("date_fin_reelle")]
        public DateTime? DateFinReelle { get; set; }

        public virtual ICollection<Paiement> Paiements { get; set; }
    }
}