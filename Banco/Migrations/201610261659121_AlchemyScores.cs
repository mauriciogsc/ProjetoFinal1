namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlchemyScores : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tips", "AlchemyScore", c => c.Single(nullable: false));
            AddColumn("dbo.Tips", "AlchemyPredict", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tips", "AlchemyPredict");
            DropColumn("dbo.Tips", "AlchemyScore");
        }
    }
}
