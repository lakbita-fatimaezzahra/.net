namespace LocationVoitures.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AjoutTableAlertes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.locations", "date_creation", c => c.DateTime(nullable: false));
            AddColumn("dbo.locations", "date_fin_reelle", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.locations", "date_fin_reelle");
            DropColumn("dbo.locations", "date_creation");
        }
    }
}
