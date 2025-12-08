namespace LocationVoitures.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Utilisateurs", "role_id", "dbo.Roles");
            DropIndex("dbo.Utilisateurs", new[] { "role_id" });
            AddColumn("dbo.employes", "role_id", c => c.Int(nullable: false));
            AlterColumn("dbo.Utilisateurs", "Role_Id", c => c.Int());
            CreateIndex("dbo.Utilisateurs", "Role_Id");
            CreateIndex("dbo.employes", "role_id");
            AddForeignKey("dbo.Utilisateurs", "Role_Id", "dbo.Roles", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Utilisateurs", "Role_Id", "dbo.Roles");
            DropIndex("dbo.employes", new[] { "role_id" });
            DropIndex("dbo.Utilisateurs", new[] { "Role_Id" });
            AlterColumn("dbo.Utilisateurs", "Role_Id", c => c.Int(nullable: false));
            DropColumn("dbo.employes", "role_id");
            CreateIndex("dbo.Utilisateurs", "role_id");
            AddForeignKey("dbo.Utilisateurs", "role_id", "dbo.Roles", "Id", cascadeDelete: true);
        }
    }
}
