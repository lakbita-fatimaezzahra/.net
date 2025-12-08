namespace LocationVoitures.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.alertes", "dateCreation", c => c.DateTime(nullable: false));
            AddColumn("dbo.alertes", "dateResolution", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.alertes", "dateResolution");
            DropColumn("dbo.alertes", "dateCreation");
        }
    }
}
