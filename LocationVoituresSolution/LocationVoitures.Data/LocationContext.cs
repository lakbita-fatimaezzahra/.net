using LocationVoitures.Data.Models;
using System.Data.Entity;

namespace LocationVoitures.Data.models
{
    public class LocationContext : DbContext
    {
        public LocationContext() : base("name=locationVoitures") { }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employe> Employes { get; set; }
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<TypeVoiture> TypesVoitures { get; set; }
        public DbSet<ImageVoiture> ImagesVoitures { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Alerte> Alertes { get; set; }
        public DbSet<LogAction> LogActions { get; set; }
    }
}
