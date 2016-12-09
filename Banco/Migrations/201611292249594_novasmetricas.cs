namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class novasmetricas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Venues", "ratePesoInterno", c => c.Double(nullable: false));
            AddColumn("dbo.Venues", "rateMediaAritmetica", c => c.Double(nullable: false));
            AddColumn("dbo.Users", "pesoInterno", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "pesoInterno");
            DropColumn("dbo.Venues", "rateMediaAritmetica");
            DropColumn("dbo.Venues", "ratePesoInterno");
        }
    }
}
