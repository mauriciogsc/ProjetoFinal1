namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mediadoscomentariosusuarios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "mediaComentarios", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "mediaComentarios");
        }
    }
}
