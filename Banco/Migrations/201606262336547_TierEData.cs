namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TierEData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "updated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Venues", "tier", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Venues", "tier");
            DropColumn("dbo.Venues", "updated");
        }
    }
}
