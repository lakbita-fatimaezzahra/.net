namespace LocationVoitures.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class firstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.alertes",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    type_alerte = c.String(),
                    message = c.String(),
                    statut = c.String(),
                    voiture_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.voitures", t => t.voiture_id, cascadeDelete: true)
                .Index(t => t.voiture_id);

            CreateTable(
                "dbo.voitures",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    matricule = c.String(),
                    marque = c.String(),
                    modele = c.String(),
                    annee = c.DateTime(nullable: false),
                    prix_jour = c.Double(nullable: false),
                    statut = c.String(),
                    kilometrage = c.Double(nullable: false),
                    derniere_maintenance = c.DateTime(nullable: false),
                    type_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.types_voitures", t => t.type_id, cascadeDelete: true)
                .Index(t => t.type_id);

            CreateTable(
                "dbo.images_voitures",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    chemin = c.String(),
                    voiture_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.voitures", t => t.voiture_id, cascadeDelete: true)
                .Index(t => t.voiture_id);

            CreateTable(
                "dbo.locations",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    date_debut = c.DateTime(nullable: false),
                    date_fin = c.DateTime(nullable: false),
                    prix_total = c.Double(nullable: false),
                    statut = c.String(),
                    client_id = c.Int(nullable: false),
                    voiture_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.clients", t => t.client_id)
                .ForeignKey("dbo.voitures", t => t.voiture_id, cascadeDelete: true)
                .Index(t => t.client_id)
                .Index(t => t.voiture_id);

            CreateTable(
                "dbo.Utilisateurs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Nom = c.String(),
                    Prenom = c.String(),
                    Email = c.String(),
                    password = c.String(),
                    role_id = c.Int(nullable: false),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.role_id, cascadeDelete: true)
                .Index(t => t.role_id);

            CreateTable(
                "dbo.log_actions",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    action = c.String(),
                    date_action = c.DateTime(nullable: false),
                    ip_adresse = c.String(),
                    utilisateur_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Utilisateurs", t => t.utilisateur_id, cascadeDelete: true)
                .Index(t => t.utilisateur_id);

            CreateTable(
                "dbo.Roles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Nom = c.String(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.paiements",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    montant = c.Double(nullable: false),
                    mode_paiement = c.String(),
                    statut = c.String(),
                    date_paiement = c.DateTime(nullable: false),
                    location_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.locations", t => t.location_id, cascadeDelete: true)
                .Index(t => t.location_id);

            CreateTable(
                "dbo.maintenances",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    date_maintenance = c.DateTime(nullable: false),
                    cout = c.Double(nullable: false),
                    description = c.String(),
                    voiture_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.voitures", t => t.voiture_id, cascadeDelete: true)
                .Index(t => t.voiture_id);

            CreateTable(
                "dbo.types_voitures",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    nom = c.String(),
                    description = c.String(),
                })
                .PrimaryKey(t => t.id);

            CreateTable(
                "dbo.clients",
                c => new
                {
                    Id = c.Int(nullable: false),
                    cin = c.String(),
                    telephone = c.String(),
                    adresse = c.String(),
                    date_naissance = c.DateTime(nullable: false),
                    permis_conduire = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Utilisateurs", t => t.Id)
                .Index(t => t.Id);

            CreateTable(
                "dbo.employes",
                c => new
                {
                    Id = c.Int(nullable: false),
                    poste = c.String(),
                    salaire = c.Double(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Utilisateurs", t => t.Id)
                .Index(t => t.Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.employes", "Id", "dbo.Utilisateurs");
            DropForeignKey("dbo.clients", "Id", "dbo.Utilisateurs");
            DropForeignKey("dbo.alertes", "voiture_id", "dbo.voitures");
            DropForeignKey("dbo.voitures", "type_id", "dbo.types_voitures");
            DropForeignKey("dbo.maintenances", "voiture_id", "dbo.voitures");
            DropForeignKey("dbo.locations", "voiture_id", "dbo.voitures");
            DropForeignKey("dbo.paiements", "location_id", "dbo.locations");
            DropForeignKey("dbo.Utilisateurs", "role_id", "dbo.Roles");
            DropForeignKey("dbo.log_actions", "utilisateur_id", "dbo.Utilisateurs");
            DropForeignKey("dbo.locations", "client_id", "dbo.clients");
            DropForeignKey("dbo.images_voitures", "voiture_id", "dbo.voitures");
            DropIndex("dbo.employes", new[] { "Id" });
            DropIndex("dbo.clients", new[] { "Id" });
            DropIndex("dbo.maintenances", new[] { "voiture_id" });
            DropIndex("dbo.paiements", new[] { "location_id" });
            DropIndex("dbo.log_actions", new[] { "utilisateur_id" });
            DropIndex("dbo.Utilisateurs", new[] { "role_id" });
            DropIndex("dbo.locations", new[] { "voiture_id" });
            DropIndex("dbo.locations", new[] { "client_id" });
            DropIndex("dbo.images_voitures", new[] { "voiture_id" });
            DropIndex("dbo.voitures", new[] { "type_id" });
            DropIndex("dbo.alertes", new[] { "voiture_id" });
            DropTable("dbo.employes");
            DropTable("dbo.clients");
            DropTable("dbo.types_voitures");
            DropTable("dbo.maintenances");
            DropTable("dbo.paiements");
            DropTable("dbo.Roles");
            DropTable("dbo.log_actions");
            DropTable("dbo.Utilisateurs");
            DropTable("dbo.locations");
            DropTable("dbo.images_voitures");
            DropTable("dbo.voitures");
            DropTable("dbo.alertes");
        }
    }
}
