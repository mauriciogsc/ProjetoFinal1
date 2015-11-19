namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SquareIds : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tips", "SquareId", c => c.String());
            AddColumn("dbo.Users", "SquareId", c => c.String());
            AddColumn("dbo.Venues", "SquareId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Venues", "SquareId");
            DropColumn("dbo.Users", "SquareId");
            DropColumn("dbo.Tips", "SquareId");
        }
    }
}
