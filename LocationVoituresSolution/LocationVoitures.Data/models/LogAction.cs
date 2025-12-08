using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    [Table("log_actions")]
    public class LogAction
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("action")]
        public string Action { get; set; }

        [Column("date_action")]
        public DateTime DateAction { get; set; }

        [Column("ip_adresse")]
        public string IpAdresse { get; set; }

        [Column("utilisateur_id")]
        public int UtilisateurId { get; set; }

        [ForeignKey("UtilisateurId")]
        public virtual Utilisateur Utilisateur { get; set; }
    }
}
