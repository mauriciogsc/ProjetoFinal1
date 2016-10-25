namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WekaPredict : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tips", "WekaPredict", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tips", "WekaPredict");
        }
    }
}
