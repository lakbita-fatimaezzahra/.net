using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("voitures")]
    public class Voiture
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("matricule")]
        public string Matricule { get; set; }

        [Column("marque")]
        public string Marque { get; set; }

        [Column("modele")]
        public string Modele { get; set; }

        [Column("annee")]
        public DateTime Annee { get; set; }

        [Column("prix_jour")]
        public double PrixJour { get; set; }

        [Column("statut")]
        public string Statut { get; set; }

        [Column("kilometrage")]
        public double Kilometrage { get; set; }

        [Column("derniere_maintenance")]
        public DateTime DerniereMaintenance { get; set; }

        [Column("type_id")]
        public int TypeId { get; set; }


        [ForeignKey("TypeId")]
        public virtual TypeVoiture TypeVoiture { get; set; }

        public virtual ICollection<ImageVoiture> Images { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<Maintenance> Maintenances { get; set; }
    }
}
