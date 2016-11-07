namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ismixed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tips", "AlchemyMixed", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tips", "AlchemyMixed");
        }
    }
}
