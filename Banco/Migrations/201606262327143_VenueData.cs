namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VenueData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "checkincount", c => c.Int(nullable: false));
            AddColumn("dbo.Venues", "tipcount", c => c.Int(nullable: false));
            AddColumn("dbo.Venues", "rate", c => c.Double(nullable: false));
            AddColumn("dbo.Venues", "price", c => c.Int(nullable: false));
            AddColumn("dbo.Venues", "likes", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Venues", "likes");
            DropColumn("dbo.Venues", "price");
            DropColumn("dbo.Venues", "rate");
            DropColumn("dbo.Venues", "tipcount");
            DropColumn("dbo.Venues", "checkincount");
        }
    }
}
