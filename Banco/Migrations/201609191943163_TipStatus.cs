namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TipStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tips", "status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tips", "status");
        }
    }
}
