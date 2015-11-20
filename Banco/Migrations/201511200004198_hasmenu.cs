namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasmenu : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "HasMenu", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Venues", "HasMenu");
        }
    }
}
