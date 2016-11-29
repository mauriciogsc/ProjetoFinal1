namespace Banco.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userweight : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "weight", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "weight");
        }
    }
}
