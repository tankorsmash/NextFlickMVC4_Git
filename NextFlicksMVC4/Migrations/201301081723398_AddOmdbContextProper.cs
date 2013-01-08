namespace NextFlicksMVC4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOmdbContextProper : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OmdbEntries",
                c => new
                    {
                        ombd_ID = c.Int(nullable: false, identity: true),
                        title = c.String(),
                        year = c.String(),
                        i_Rating = c.String(),
                        i_Votes = c.String(),
                        i_ID = c.String(),
                        t_Meter = c.String(),
                        t_Image = c.String(),
                        t_Rating = c.String(),
                        t_Reviews = c.String(),
                        t_Fresh = c.String(),
                        t_Rotten = c.String(),
                        t_Consensus = c.String(),
                        t_UserMeter = c.String(),
                        t_UserRating = c.String(),
                        t_UserReviews = c.String(),
                    })
                .PrimaryKey(t => t.ombd_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OmdbEntries");
        }
    }
}
