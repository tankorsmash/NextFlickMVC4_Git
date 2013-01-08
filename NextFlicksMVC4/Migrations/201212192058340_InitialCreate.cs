namespace NextFlicksMVC4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Movies",
                c => new
                    {
                        movie_ID = c.Int(nullable: false, identity: true),
                        short_title = c.String(),
                        year = c.String(),
                        runtime = c.Time(nullable: false),
                        avg_rating = c.String(),
                        tv_rating = c.String(),
                        web_page = c.String(),
                        current_season = c.String(),
                        is_movie = c.Boolean(nullable: false),
                        maturity_rating = c.String(),
                    })
                .PrimaryKey(t => t.movie_ID);
            
            CreateTable(
                "dbo.BoxArts",
                c => new
                    {
                        boxart_ID = c.Int(nullable: false, identity: true),
                        movie_ID = c.Int(nullable: false),
                        boxart_38 = c.String(),
                        boxart_64 = c.String(),
                        boxart_110 = c.String(),
                        boxart_124 = c.String(),
                        boxart_150 = c.String(),
                        boxart_166 = c.String(),
                        boxart_88 = c.String(),
                        boxart_197 = c.String(),
                        boxart_176 = c.String(),
                        boxart_284 = c.String(),
                        boxart_210 = c.String(),
                    })
                .PrimaryKey(t => t.boxart_ID);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        genre_ID = c.Int(nullable: false),
                        genre_string = c.String(),
                    })
                .PrimaryKey(t => t.genre_ID);
            
            CreateTable(
                "dbo.MovieToGenres",
                c => new
                    {
                        movie_to_genre_ID = c.Int(nullable: false, identity: true),
                        movie_ID = c.Int(nullable: false),
                        genre_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.movie_to_genre_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MovieToGenres");
            DropTable("dbo.Genres");
            DropTable("dbo.BoxArts");
            DropTable("dbo.Movies");
        }
    }
}
