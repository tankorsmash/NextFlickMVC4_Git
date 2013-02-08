using System.IO;
using System.Web.Mvc;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.Controllers.Admin
{
    public static class DatabaseTools
    {
        static MovieDbContext db = new MovieDbContext();
        public static void DropTables()
        {
            FileInfo file = new FileInfo(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/DropTables.sql"));
            string script = file.OpenText().ReadToEnd();

            //var script = "DELETE FROM <DBO.MOVIES>";
            db.Database.ExecuteSqlCommand(script);




        }

        public static void CreateTables()
        {
            FileInfo file = new FileInfo( System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/CreateTables.sql"));
            string script = file.OpenText().ReadToEnd();
            db.Database.ExecuteSqlCommand(script);
        }

        public static void DropAndCreate()
        {
            DropTables();
            CreateTables();
        }
    }
}