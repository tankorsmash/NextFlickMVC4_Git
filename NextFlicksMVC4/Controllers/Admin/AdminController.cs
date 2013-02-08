using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.OMBD;

namespace NextFlicksMVC4.Controllers.Admin
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DbTools()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DbTools(string button)
        {
            if (button == "Drop Tables")
            {
                DatabaseTools.DropTables();
                ViewBag.Message = "Tables Dropped";
            }
            if (button == "Create Tables")
            {
                DatabaseTools.CreateTables();
                ViewBag.Message = "Tables Created";
            }
            if (button == "Drop And Create")
            {
                DatabaseTools.DropAndCreate();
                ViewBag.Message = "Tables dropped and Recreated";
            }
            return View();


        }

        public ActionResult Roles()
        {
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }

        public ActionResult Tags()
        {
            return View();
        }

        public ActionResult Movies()
        {
            return View();
        } 
        public ActionResult Full()
        {
           // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MovieDbContext>());
            Tools.TraceLine("In Full");
            //create a genres table in the DB
            PopulateGenres.PopulateGenresTable();


            Tools.BuildMoviesBoxartGenresTables(Server.MapPath("~/dbfiles/fixedAPI.NFPOX"));

            Tools.TraceLine("Out Full");

            return View();
        }
        
        /// <summary>
        /// Rebuild the serialized list of OmdbEntrys that were created in Movies/TSV, and adds them to the database
        /// </summary>
        /// <returns></returns>
        public ActionResult Regen()
        {
            //finds the file dump from the TSV to OmdbEntry read and adds it to the db
            Tools.RebuildOmdbsFromProtobufDump(@"\OMBD\omdb.DUMP");

            return View();
        }

        /// <summary>
        /// Combines /tsv /regen and /merge all together
        /// </summary>
        /// <returns></returns>
        public ActionResult Mutagen()
        {
            Tools.TraceLine("In Mutagen");

            var start = Tools.WriteTimeStamp();

            MovieDbContext db = new MovieDbContext();
            
            //read the Omdb.txt file and turn the resulting objects into a protobuf dump
            // to be read by the Tools.RebuildOmdbsFromProtobufDump method
            string entryDumpPath = Server.MapPath("~/dbfiles/omdbASD.DUMP");
            string imdbPath = Server.MapPath("~/dbfiles/omdb.txt");
            string tomPath =
                Server.MapPath("~/dbfiles/tomatoes.txt");

            Tools.SerializeOmdbTsv(
                entryDumpPath,
                imdbPath,
                tomPath);
            //finds the file dump from the TSV to OmdbEntry read and adds it to the db
            Tools.RebuildOmdbsFromProtobufDump(entryDumpPath);


            //loop over all the movies in Movies and find an omdb entry for it
            Tools.MarryMovieToOmdb(db);

            var end = Tools.WriteTimeStamp();
            Tools.TraceLine("Mutagen took {0}", end- start);

            Tools.TraceLine("Out Mutagen");

            return View();

        }

        /// <summary>
        /// Reads the OMDB API data txts and dumps the list of OMBD Entrys to file, use Movies/Regen to rebuild them
        /// </summary>
        /// <returns></returns>
        public ActionResult Tsv()
        {
            //read the Omdb.txt file and turn the resulting objects into a protobuf dump
            // to be read by the Tools.RebuildOmdbsFromProtobufDump method
            Tools.SerializeOmdbTsv(
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksMVC4\OMBD\omdb.DUMP",
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\omdb.txt",
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\tomatoes.txt");


            return View();
        }

        public ActionResult Zip()
        {
            Omdb.DownloadOmdbZipAndExtract(@"omdb.zip");
            return View();
        }


        public ActionResult FullDbBuild()
        {
            string netflixPosFilepath = @"catalogStreaming.NFPOX";
            MovieDbContext db = new MovieDbContext();

            //retrieve API .POX
            var data = OAuth1a.GetNextflixCatalogDataString( "catalog/titles/streaming", "", outputPath: netflixPosFilepath);

            //join the lines that don't match <catalog to the ones above it
            Tools.JoinLines(netflixPosFilepath);

            //build a genres txt file for all the genres in the NFPOX
            //ASSUMES GENRES.NFPOX IS THERE
            PopulateGenres.PopulateGenresTable();
            
            //parse the lines into a Title then Movie object, along with boxart data and genre
            Tools.BuildMoviesBoxartGenresTables(netflixPosFilepath);

            //download the omdbapi
            Omdb.DownloadOmdbZipAndExtract(@"omdb.zip");

            //parse it for omdbentrys, serialize it to file
            Tools.SerializeOmdbTsv(@"omdb.DUMP", @"omdb.txt", @"tomatoes.txt");

            //deserialize the file, turn it into omdb
            //  can't remember if it does it here or not, but marry the omdbs and movie
            Tools.RebuildOmdbsFromProtobufDump(@"omdb.DUMP");

            Tools.MarryMovieToOmdb(db);


            return View();
        }


        //loop over all the movies in Movies and find an omdb entry for it
        public ActionResult Merge()
        {

            MovieDbContext db = new MovieDbContext();

            Tools.MarryMovieToOmdb(db);


            return View();

        }
    }
}
