namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "lat", c => c.Double(nullable: false,defaultValue:0.0));
            AddColumn("dbo.Venues", "lon", c => c.Double(nullable: false,defaultValue:0.0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Venues", "lon");
            DropColumn("dbo.Venues", "lat");
        }
    }
}
