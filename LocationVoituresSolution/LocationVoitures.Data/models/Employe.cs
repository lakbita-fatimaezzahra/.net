using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("employes")]
    public class Employe : Utilisateur
    {
        [Column("poste")]
        public string Poste { get; set; }

        [Column("salaire")]
        public double Salaire { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}
