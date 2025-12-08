using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("types_voitures")]
    public class TypeVoiture
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("nom")]
        public string Nom { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public virtual ICollection<Voiture> Voitures { get; set; }
    }
}
