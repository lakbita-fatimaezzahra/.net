namespace LocationVoitures.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.maintenances", "type_maintenance", c => c.String());
            AddColumn("dbo.maintenances", "date_prochaine_maintenance", c => c.DateTime(nullable: false));
            AddColumn("dbo.maintenances", "kilometrage_maintenance", c => c.Double(nullable: false));
            AddColumn("dbo.maintenances", "kilometrage_prochain", c => c.Double(nullable: false));
            AddColumn("dbo.maintenances", "garage", c => c.String());
            AddColumn("dbo.maintenances", "statut", c => c.String());
            AddColumn("dbo.maintenances", "date_creation", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.maintenances", "date_creation");
            DropColumn("dbo.maintenances", "statut");
            DropColumn("dbo.maintenances", "garage");
            DropColumn("dbo.maintenances", "kilometrage_prochain");
            DropColumn("dbo.maintenances", "kilometrage_maintenance");
            DropColumn("dbo.maintenances", "date_prochaine_maintenance");
            DropColumn("dbo.maintenances", "type_maintenance");
        }
    }
}
