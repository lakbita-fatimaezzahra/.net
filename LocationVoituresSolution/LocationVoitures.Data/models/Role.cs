using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationVoitures.Data.Models
{
    public class Role
    {
        public int Id { get; set; }          // Clé primaire
        public string Nom { get; set; }    

        // Navigation : un rôle peut avoir plusieurs utilisateurs
        public virtual ICollection<Employe> Employes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public Role()
        {
            Employes = new HashSet<Employe>();
        }
    }
}
