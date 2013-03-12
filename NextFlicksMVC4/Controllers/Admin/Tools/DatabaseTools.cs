using System.IO;
using System.Web.Mvc;
using System.Linq;
using System.Xml.Linq;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.OMBD;
using System.Collections.Generic;
using System;
using System.Xml;

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

            MovieDbContext db = new MovieDbContext();
            string netflixPosFilepath = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX");

            //retrieve API .POX
            //var netflixCatalogOutputPath = OAuth1a.GetNextflixCatalogDataString("catalog/titles/streaming", "", outputPath: netflixPosFilepath);

            var genresNFPOX = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/genres.NFPOX");
            var omdbZIP = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.zip");
            var omdbDUMP = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.DUMP");
            var omdbTXT = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.txt");
            var tomatoesTXT = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/tomatoes.txt");

            //join the lines that don't match <catalog to the ones above it
            //Tools.JoinLines(netflixPosFilepath);

            //Parse the netflix NFPOX and make sure the genres.nfpox exists and is up to dat
            //UpdateGenreList(netflixPosFilepath);
            //build a genres txt file for all the genres in the NFPOX
            //ASSUMES GENRES.NFPOX IS THERE
            //PopulateGenres.PopulateGenresTable();

            //parse the lines into a Title then Movie object, along with boxart data and genre
            //Tools.BuildMoviesBoxartGenresTables(netflixPosFilepath);

            //download the omdbapi
            //Omdb.DownloadOmdbZipAndExtract(omdbZIP);

            ////parse it for omdbentrys, serialize it to file
            //Tools.SerializeOmdbTsv(omdbDUMP, omdbTXT, tomatoesTXT);
            ////deserialize the file, turn it into omdb
            ////  can't remember if it does it here or not, but marry the omdbs and movie
            //Tools.RebuildOmdbsFromProtobufDump(omdbDUMP);

            //new way to turn the TSV into Omdb db
            TSVParse.OptimizedPopulateOmdbTableFromTsv(omdbTXT, tomatoesTXT);


            Tools.MarryMovieToOmdb(db);
        }


        //loop over all the movies in Movies and find an omdb entry for it
        public static void Merge()
        {
            MovieDbContext db = new MovieDbContext();
            Tools.MarryMovieToOmdb(db);
        }

        public static void Api(string term = "Jim Carrey")
        {
            MovieDbContext db = new MovieDbContext();
            //grab new movies, turn one into a Movie and view it
            var data =
                OAuth1a.GetNextflixCatalogDataString(
                    "catalog/titles/streaming", term, max_results: "100",
                    outputPath: System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX"));
            /*var titles =
                NetFlixAPI.Create.ParseXmlForCatalogTitles(data);

            List<Movie> movies = new List<Movie>();

            foreach (Title title in titles)
            {
                //create a  movie from the title, and add it to a list of movies and
                // the database
                Movie movie = NetFlixAPI.Create.CreateMovie(title);
                movies.Add(movie);
                db.Movies.Add(movie);

                //create a boxart object from the movie and title object
                BoxArt boxArt =
                    NetFlixAPI.Create.CreateMovieBoxartFromTitle(movie, title);
                db.BoxArts.Add(boxArt);

                //for all the genres in a title, create the linking MtG 
                // and then add that object to the db
                foreach (Genre genre in title.ListGenres)
                {
                    MovieToGenre movieToGenre =
                        NetFlixAPI.Create.CreateMovieMovieToGenre(movie,
                                                                  genre);
                    db.MovieToGenres.Add(movieToGenre);

                }
            }

            db.SaveChanges(); */
        }

        public static void NetflixGenres(string term = "Jim Carrey")
        {
            var data =
                OAuth1a.GetNextflixCatalogDataString(
                    "categories/genres", term, max_results: "900",
                    outputPath: System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/genres.NFPOX"));
        }

        /// <summary>
        /// Parse the Netflix Catalog (fixedAPI.nfpox) to get a list of all current genres
        /// </summary>
        /// <param name="filepath"></param>
        public static void UpdateGenreList(string filepath)
        {
            Tools.WriteTimeStamp("start update genres");
            Dictionary<int, string> genres = new Dictionary<int, string>();
            using (XmlReader xmlReader = XmlReader.Create(filepath))
            {
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "category"))
                    {

                        string possible_genre_url =
                            xmlReader.GetAttribute("scheme");

                        if (possible_genre_url.Contains("genres"))
                        {
                            //gotta split url for the id
                            int genre_id =
                                Convert.ToInt32(possible_genre_url.Split('/').Last());
                            string genre_string = xmlReader.GetAttribute("label");
                            //so long as the group isn't in the list, add it it
                            if (!genres.ContainsKey(genre_id))
                            {
                                genres.Add(genre_id, genre_string);
                            }
                           
                        }
                    }
                }
            }
            var genresPath = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/genres.NFPOX");
            using (StreamWriter writer = new StreamWriter(genresPath, append: false))
            {
                foreach (KeyValuePair<int, string> kvp in genres)
                {
                    writer.WriteLine(kvp.Key + " " + kvp.Value);
                }
            }
            Tools.WriteTimeStamp("end update genres");
                            
        }
    }
}
