namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CamposUsuario : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Sexo", c => c.String());
            AddColumn("dbo.Users", "countAmigos", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "countCheckin", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "countTip", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "cidadeNatal", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "cidadeNatal");
            DropColumn("dbo.Users", "countTip");
            DropColumn("dbo.Users", "countCheckin");
            DropColumn("dbo.Users", "countAmigos");
            DropColumn("dbo.Users", "Sexo");
        }
    }
}
