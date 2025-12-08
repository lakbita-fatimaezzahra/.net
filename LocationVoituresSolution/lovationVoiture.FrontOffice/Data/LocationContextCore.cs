using LocationVoitures.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LocationVoitures.FrontOffice.Data
{
    public class LocationContextCore : DbContext
    {
        public LocationContextCore(DbContextOptions<LocationContextCore> options)
            : base(options)
        {
        }

        // Utiliser les modèles du projet LocationVoitures.Data
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Spécifier les noms de tables (si nécessaire)
            modelBuilder.Entity<Voiture>().ToTable("voitures");
            modelBuilder.Entity<Client>().ToTable("clients");
            modelBuilder.Entity<Location>().ToTable("locations");
            modelBuilder.Entity<TypeVoiture>().ToTable("types_voitures");
            modelBuilder.Entity<ImageVoiture>().ToTable("images_voitures");
            modelBuilder.Entity<Maintenance>().ToTable("maintenances");
            modelBuilder.Entity<Alerte>().ToTable("alertes");
            modelBuilder.Entity<Paiement>().ToTable("paiements");
            modelBuilder.Entity<Utilisateur>().ToTable("utilisateurs");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Employe>().ToTable("employes");
            modelBuilder.Entity<LogAction>().ToTable("log_actions");

            // Configuration des relations

            // Voiture -> TypeVoiture
        {
            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.TypeVoiture)
                .WithMany(t => t.Voitures)
                .HasForeignKey(v => v.TypeId); // <-- indique la colonne exacte
        }


        // Voiture -> Images
        modelBuilder.Entity<ImageVoiture>()
                .HasOne(i => i.Voiture)
                .WithMany(v => v.Images)
                .HasForeignKey(i => i.VoitureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Location -> Voiture
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Voiture)
                .WithMany(v => v.Locations)
                .HasForeignKey(l => l.VoitureId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location -> Client
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Client)
                .WithMany(c => c.Locations)
                .HasForeignKey(l => l.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Maintenance -> Voiture
            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Voiture)
                .WithMany(v => v.Maintenances)
                .HasForeignKey(m => m.VoitureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Alerte -> Voiture
            modelBuilder.Entity<Alerte>()
                .HasOne(a => a.Voiture)
                .WithMany()
                .HasForeignKey(a => a.VoitureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Paiement -> Location
            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Location)
                .WithMany(l => l.Paiements)
                .HasForeignKey(p => p.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // employe -> Role
            modelBuilder.Entity<Employe>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}