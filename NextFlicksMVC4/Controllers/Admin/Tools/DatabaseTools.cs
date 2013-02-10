using System.IO;
using System.Web.Mvc;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.OMBD;

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

        public static void MakeDb()
        {
            Full();
            Mutagen();
        }

        public static void Full()
        {
            // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MovieDbContext>());
            Tools.TraceLine("In Full");
            //create a genres table in the DB
            PopulateGenres.PopulateGenresTable();

            Tools.BuildMoviesBoxartGenresTables(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX"));

            Tools.TraceLine("Out Full");
        }

        public static void Mutagen()
        {
            Tools.TraceLine("In Mutagen");

            var start = Tools.WriteTimeStamp();

            MovieDbContext db = new MovieDbContext();

            //read the Omdb.txt file and turn the resulting objects into a protobuf dump
            // to be read by the Tools.RebuildOmdbsFromProtobufDump method
            string entryDumpPath = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdbASD.DUMP");
            string imdbPath = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.txt");
            string tomPath =
                 System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/tomatoes.txt");

            Tools.SerializeOmdbTsv(
                entryDumpPath,
                imdbPath,
                tomPath);
            //finds the file dump from the TSV to OmdbEntry read and adds it to the db
            Tools.RebuildOmdbsFromProtobufDump(entryDumpPath);

            //loop over all the movies in Movies and find an omdb entry for it
            Tools.MarryMovieToOmdb(db);

            var end = Tools.WriteTimeStamp();
            Tools.TraceLine("Mutagen took {0}", end - start);

            Tools.TraceLine("Out Mutagen");
        }

        /// <summary>
        /// Rebuild the serialized list of OmdbEntrys that were created in Movies/TSV, and adds them to the database
        /// </summary>
        /// <returns></returns>
        public static void Regen()
        {
            //finds the file dump from the TSV to OmdbEntry read and adds it to the db
            Tools.RebuildOmdbsFromProtobufDump(@"\OMBD\omdb.DUMP");
        }

        /// <summary>
        /// Reads the OMDB API data txts and dumps the list of OMBD Entrys to file, use Movies/Regen to rebuild them
        /// </summary>
        /// <returns></returns>
        public static void Tsv()
        {
            //read the Omdb.txt file and turn the resulting objects into a protobuf dump
            // to be read by the Tools.RebuildOmdbsFromProtobufDump method
            Tools.SerializeOmdbTsv(
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksMVC4\OMBD\omdb.DUMP",
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\omdb.txt",
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\tomatoes.txt");
        }

        public static void Zip()
        {
            Omdb.DownloadOmdbZipAndExtract(@"omdb.zip");
        }

        public static void FullDbBuild()
        {
            string netflixPosFilepath = @"catalogStreaming.NFPOX";
            MovieDbContext db = new MovieDbContext();

            //retrieve API .POX
            var data = OAuth1a.GetNextflixCatalogDataString("catalog/titles/streaming", "", outputPath: netflixPosFilepath);

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
        }


        //loop over all the movies in Movies and find an omdb entry for it
        public static void Merge()
        {

            MovieDbContext db = new MovieDbContext();

            Tools.MarryMovieToOmdb(db);

        }
    }
}