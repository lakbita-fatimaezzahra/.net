using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("clients")]
    public class Client : Utilisateur
    {
        [Column("cin")]
        public string CIN { get; set; }

        [Column("telephone")]
        public string Telephone { get; set; }

        [Column("adresse")]
        public string Adresse { get; set; }

        [Column("date_naissance")]
        public DateTime DateNaissance { get; set; }

        [Column("permis_conduire")]
        public string PermisConduire { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
    }
}
