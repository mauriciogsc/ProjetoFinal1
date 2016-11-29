namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AvaliacaoFinal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "rateWeka", c => c.Double(nullable: false));
            AddColumn("dbo.Tips", "WekaPredictFinal", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tips", "WekaPredictFinal");
            DropColumn("dbo.Venues", "rateWeka");
        }
    }
}
