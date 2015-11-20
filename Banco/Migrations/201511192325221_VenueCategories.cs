namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VenueCategories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SquareId = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VenueCategories",
                c => new
                    {
                        Venue_Id = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Venue_Id, t.Category_Id })
                .ForeignKey("dbo.Venues", t => t.Venue_Id, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .Index(t => t.Venue_Id)
                .Index(t => t.Category_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VenueCategories", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.VenueCategories", "Venue_Id", "dbo.Venues");
            DropIndex("dbo.VenueCategories", new[] { "Category_Id" });
            DropIndex("dbo.VenueCategories", new[] { "Venue_Id" });
            DropTable("dbo.VenueCategories");
            DropTable("dbo.Categories");
        }
    }
}
