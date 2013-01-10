namespace NextFlicksMVC4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMovieIDtoOmdbEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OmdbEntries", "movie_ID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OmdbEntries", "movie_ID");
        }
    }
}
