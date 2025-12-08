using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("images_voitures")]
    public class ImageVoiture
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("chemin")]
        public string Chemin { get; set; }

        [Column("voiture_id")]
        public int VoitureId { get; set; }

        [ForeignKey("VoitureId")]
        public virtual Voiture Voiture { get; set; }
    }
}
