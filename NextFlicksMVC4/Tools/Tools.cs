using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NextFlicksMVC4.Controllers;
using System.Diagnostics;
using NextFlicksMVC4.DatabaseClasses;
using NextFlicksMVC4.Helpers;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.Views.Movies.ViewModels;
using NextFlicksMVC4.OMBD;
using ProtoBuf;

namespace NextFlicksMVC4
{
    public static class Tools
    {
        public static string GetParamName(MethodInfo method, int index)
        {
            string retVal = String.Empty;

            if (method != null && method.GetParameters().Length > index) {
                retVal = method.GetParameters()[index].Name;
            }

            return retVal;
        }

        /// <summary>
        /// loop over params and save all the names in a list
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static List<string> GetAllParamNames(string methodName)
        {
            List<string> param_names = new List<string>();

            for (int i = 0; i < 15; i++) {
                var info = typeof (MoviesController).GetMethod(methodName);
                string param_name = GetParamName(info, i);
                
                //TODO: Improve method
                //if the param is unnamed, there's probably no more params. Improve it
                if (param_name == "")
                    break;

                param_names.Add(param_name);
            }

            return param_names;

        }

        //public static string GetXmlText(XmlDocument xmlDocument, string xpath, string error_msg = @"N/A")
        //{

        //    string result = xmlDocument.SelectSingleNode(xpath).InnerText;

        //    return result;
        //}

        /// <summary>
        /// Takes a message string and Trace.WriteLines it along with a DateTime, default Now
        /// </summary>
        /// <param name="msg">Message to write</param>
        /// <param name="presetDateTime">A custom time value</param>
        /// <returns>Returns the DateTime that was written</returns>
        public static DateTime WriteTimeStamp(string msg= "The time is", DateTime? presetDateTime = null)
        {
            DateTime time;
            if (presetDateTime == null) { time = DateTime.Now; }
            else { time = (DateTime) presetDateTime; } 

            string to_write = String.Format("{0}: {1}", msg,
                                            time.ToShortTimeString());
            Trace.WriteLine(to_write);

            return time;
        }

        /// <summary>
        /// Merges any two object together, modifying the primary object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primary">is modified in place</param>
        /// <param name="secondary"></param>
        public static void MergeWithSlow<T>(this T primary, T secondary)
        {
            foreach (var pi in typeof(T).GetProperties())
            {
                var priValue = pi.GetGetMethod().Invoke(primary, null);
                var secValue = pi.GetGetMethod().Invoke(secondary, null);
                if (priValue == null || (pi.PropertyType.IsValueType && priValue.Equals(Activator.CreateInstance(pi.PropertyType))))
                {
                    pi.GetSetMethod().Invoke(primary, new object[] { secValue });
                }
            }
        }

        /// <summary>
        /// Filters a list of movies down through a variety of optional params
        /// </summary>
        /// <param name="db"></param>
        /// <param name="movie_list"></param>
        /// <param name="year_start"></param>
        /// <param name="year_end"></param>
        /// <param name="mpaa_start"></param>
        /// <param name="mpaa_end"></param>
        /// <param name="title"></param>
        /// <param name="runtime_start"></param>
        /// <param name="runtime_end"></param>
        /// <param name="genre"></param>
        /// <param name="start"></param>
        /// <param name="count">Limits the amount of movies returned -at the very end- of the function instead of the start</param>
        /// <param name="verbose"> whether or not to print output trace lines</param>
        /// <returns></returns>
        public static List<NfImdbRtViewModel> FilterMovies(
            MovieDbContext db,
            List<Movie> movie_list,
            int year_start = 1914,
            int year_end = 2012,
            int mpaa_start = 0,
            int mpaa_end = 200,
            string title = "",
            bool? is_movie = null,

            int runtime_start = 0,
            int runtime_end = 9999999,
            string genre = "",
            int start = 0,
            int count = 25,

            string sort = "movie_id",
            bool verbose = false)
        {
            if (verbose == true)
                Trace.WriteLine("Starting to filter...");

            //year released
            if (verbose == true)
                Trace.WriteLine("\tYear Start");
            movie_list =
                movie_list.Where(item => GetYearOr0(item) >= year_start)
                          .ToList();
            if (verbose == true)
                Trace.WriteLine("\tYear End");
            movie_list =
                movie_list.Where(item => GetYearOr0(item) <= year_end).ToList();

            //title
            if (title != "")
            {
                if (verbose == true)
                    Trace.WriteLine("\tTitle");
                movie_list =
                    movie_list.Where(
                        item => item.short_title.ToLower().Contains(title))
                              .ToList();
            }
            //sort alphabetical

            //netflix rating

            //movie runtime
            if (runtime_start != 0 || runtime_end != 9999999)
            {
                if (verbose == true)
                    Trace.WriteLine("\tRuntime Start");
                movie_list =
                    movie_list.Where(
                        item => item.runtime >= runtime_start)
                              .ToList();
                if (verbose == true)
                    Trace.WriteLine("\tRuntime End");
                movie_list =
                    movie_list.Where(
                        item => item.runtime <= runtime_end)
                              .ToList();
            }

            //mpaa rating       OR
            if (mpaa_start != 0 || mpaa_end != 200)
            {
                if (verbose == true)
                    Trace.WriteLine("\tMPAA Start");
                movie_list =
                    movie_list.Where(
                        item =>
                        item.maturity_rating >=
                        mpaa_start).ToList();
                if (verbose == true)
                    Trace.WriteLine("\tMPAA End");
                movie_list =
                    movie_list.Where(
                        item =>
                        item.maturity_rating <=
                        mpaa_end).ToList();
            }

            //genre
            if (genre != "")
            {
                if (verbose == true)
                    Trace.WriteLine("\tGenres");
                movie_list = ReduceMovieListToMatchingGenres(db, movie_list,
                                                             genre);
            }

            //is_movie
            if (is_movie != null)
            {
                if (verbose == true)
                    Trace.WriteLine("\tIs Movie");
                movie_list =
                    movie_list.Where(item => item.is_movie == is_movie).ToList();
            }


            //pass the movie_list through a function to marry them to genres and boxarts
            if (verbose == true)
                Trace.WriteLine("\tCreating MtGVM from a range in movies_list");
            if (start > movie_list.Count)
            {
                start = movie_list.Count - 1;
            }
            if (count > movie_list.Count)
            {
                count = movie_list.Count;
            }
            var ranged_movie_list = movie_list.GetRange(start, count);
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db, ranged_movie_list);

            //TraceLine("creating list of mwgvm");
            //var MwG_list = ModelBuilder.CreateListOfMwGVM(db, movie_list);

            TraceLine("matching to omdb");
            //combine the remaining movies with omdbs and then filter from there too
            var nit_list = MatchMwGVMsWithOmdbEntrys(MwG_list);

            //return the count number of movies to return
                //if (verbose == true)
                //    TraceLine("Total results after Filter: {0}", movie_list.Count);
                //if (verbose == true)
                //    Trace.WriteLine("\tTaking Count");
                //MwG_list = MwG_list.Take(count).ToList();
                //return MwG_list;

            TraceLine("about to return");
            if (sort.ToLower() == "t_meter") {
                return
                    nit_list.Where(item => item.OmdbEntry != null)
                            .OrderByDescending(item => StringToDouble(item.OmdbEntry.t_Meter))
                            .ToList();
            }
            return nit_list;

        }

        


        public static double StringToDouble(string str)
        {
            double toDouble;
            try {
                toDouble = Convert.ToDouble(str);
            }

            catch (Exception ex) {
                TraceLine("decimal conversion error, returning 0.0\n{0}", ex.Message);
                toDouble = 0.0;
            }

            return toDouble;
        }

        /// <summary>
        /// Matches a list of MwGVMs to the appropriate OmdbEntry and returns a new list of NitVM
        /// </summary>
        /// <param name="MwG_list"></param>
        /// <returns>List of NitVMs</returns>
        public static List<NfImdbRtViewModel> MatchMwGVMsWithOmdbEntrys(List<MovieWithGenreViewModel> MwG_list)
        {
            List<NfImdbRtViewModel> nit_list = new List<NfImdbRtViewModel>();
            foreach (var MwGVM in MwG_list) {
                //find the matching omdb entry based on the MwGVM's movie_ID
                OmdbEntry omdbEntry =
                    MatchMovieIdToOmdbEntry(MwGVM.movie.movie_ID);
                //create the NitVM based on the new omdb entry
                NfImdbRtViewModel nitvm = new NfImdbRtViewModel
                                              {
                                                  Boxarts = MwGVM .boxart,
                                                  Genres = MwGVM .genre_strings,
                                                  Movie = MwGVM .movie,
                                                  OmdbEntry = omdbEntry
                                              };
                //add to the list
                nit_list.Add(nitvm);
            }

            return nit_list;
        }

        /// <summary>
        /// Wraps Trace.WriteLine, cannot accept category though, due to params 
        /// </summary>
        /// <param name="msg_string"></param>
        /// <param name="vals"></param>
        public static void TraceLine(string msg_string, params object[] vals)
        {
            //create a new string based on the params given in the arguments
            string msg = String.Format(msg_string, vals);
            //send the message to Stdout
            Trace.WriteLine(msg);
        }

        /// <summary>
        /// Gets the maturity rating, or returns 200 if none found so it's highly rated in case it's adult
        /// </summary>
        /// <param name="maturity_rating"></param>
        /// <returns></returns>
        public static int ReturnMaturityOrDefault(string maturity_rating)
        {
            try {
                return Convert.ToInt32(maturity_rating);
            }

            catch (Exception ex) {
                //Trace.WriteLine(
                //    String.Format(
                //        "maturity rating exception, probably not set:\r{0}",
                //        ex.Message));

                TraceLine(
                    "maturity rating exception, probably not set:\r{0}",
                    ex.Message);
                //200 is way overkill, but you don't want a porno sorted along kids movies
                return 200;
            }
        }

        /// <summary>
        /// takes a movie_list and removes the movies that don't match the genre
        /// </summary>
        /// <param name="db"></param>
        /// <param name="movie_list"></param>
        /// <param name="genre"></param>
        /// <returns></returns>
        public static List<Movie> ReduceMovieListToMatchingGenres(MovieDbContext db,
                                                                  List<Movie> movie_list,
                                                                  string genre)
        {
            //gets the movie_ids that match 'genre'
            Trace.WriteLine("\t\tFind movies that match genre_string");
            var movie_ids_for_genres = GetMovieIdsMatchingGenres(db, genre);

            //execute the find movie_id finding by calling the list
            Trace.WriteLine("\t\tList the movies that match genre_string");
            var movie_ids_for_genres_list = movie_ids_for_genres.ToList();

            Trace.WriteLine("\t\tFind moves that match movie_id");

            Trace.WriteLine("\t\tmovie_list to Array");
            var movie_array = movie_list.ToArray();


            //the iqueryable  for finding movies that match the movie_list
            var movie_iqry =
                movie_array.Where(
                    item => movie_ids_for_genres_list.Contains(item.movie_ID));


            Trace.WriteLine("\t\tTo List");
            movie_list = movie_iqry.ToList();
            //movie_list = movie_array.ToList();
            return movie_list;
        }

        /// <summary>
        /// returns the integer of a year, or 0, if not found
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        public static int GetYearOr0(Movie movie)
        {
            if (movie.year != "") {
                return Convert.ToInt32(movie.year);
            }
            else
                return 0;
        }

        /// <summary>
        /// returns a queryable of movie_ids that match a genre_string
        /// </summary>
        /// <param name="db"></param>
        /// <param name="genre_params"></param>
        /// <param name="movie_list">if this is given, limit the search to </param>
        /// <returns></returns>
        public static IQueryable<int> GetMovieIdsMatchingGenres(
            MovieDbContext db, string genre_params, List<Movie> movie_list= null )
        {
            //finds the genre id matching the argument "genre"
            var genre_ids = db.Genres.Where(
                item => item.genre_string.StartsWith(genre_params))
                              .Select(item => item.genre_ID);

            //finds all movie_ids that have the genre_id from above
            var movies_ids_matching_the_genre_iq =
                db.MovieToGenres.Where(item => genre_ids.Contains(item.genre_ID))
                  .Select(item => item.movie_ID);

            //if movie_list was passed, select the movies from m_i_m_t_g_i that are in movie_list
            if (movie_list != null) {
                var movie_list_ids =
                    movie_list.Select(movie => movie.movie_ID);
                var qwe = movies_ids_matching_the_genre_iq.Where(
                    movie_id => 
                    movie_list_ids.Contains(movie_id));
                return qwe;
            }
                //var movies_ids_matching_the_genre =  movies_ids_matching_the_genre_iq.Select(item => item.movie_ID);
            else {

                return movies_ids_matching_the_genre_iq;
            }
        }

        ///faster than  ReduceMovieListToMatchingGenres
        public static List<Movie> GetMoviesMatchingGenre(MovieDbContext db,
                                                         string genre_params =
                                                             "action")
        {

            //get all movie ids that match the genre
            var movies_ids_matching_the_genre =
                GetMovieIdsMatchingGenres(db, genre_params);

            //finds all movies based on the movie_ids
            List<Movie> movie_list =
                db.Movies.Where(
                    item =>
                    movies_ids_matching_the_genre.Contains(item.movie_ID))
                  .ToList();

            return movie_list;
        }

        /// <summary>
        /// adds the boxart and Genres to the database, based on the dict of Movies-Titles passed into it. MAKUE SURE TO SaveChanges() before, for some reason.
        /// </summary>
        /// <param name="dictOfMoviesTitles"></param>
        /// <param name="db"></param>
        public static void AddBoxartsAndMovieToGenreData(Dictionary<Movie, Title> dictOfMoviesTitles, MovieDbContext db)
        {
            //get a list of ints that we can test against to save progress of adding to db
            List<int> checkpoints = ProgressList.CreateListOfCheckpointInts(dictOfMoviesTitles.Count, 55);
            
            //MovieDbContext db = new MovieDbContext();
            //db.Configuration.AutoDetectChangesEnabled = false;

            //loop over dict and make Boxart and Genre
            int index = 0;
            foreach (KeyValuePair<Movie, Title> keyValuePair in dictOfMoviesTitles) {
                Movie movie = keyValuePair.Key;
                Title title = keyValuePair.Value;
                BoxArt boxArt = Create.CreateMovieBoxartFromTitle(movie,
                                                                             title);
                db.BoxArts.Add(boxArt);


                //genres to database
                foreach (Genre genre in title.ListGenres)
                {
                    MovieToGenre movieToGenre =
                        Create.CreateMovieMovieToGenre(movie,
                                                                  genre);
                    db.MovieToGenres.Add(movieToGenre);
                    //db.SaveChanges();

                    var save_msg =
                        String.Format(
                            "done adding MtG mtg_id = {0}\n movie_id = {1}\n genre_id = {2}",
                            movieToGenre.movie_to_genre_ID,
                            movieToGenre.movie_ID,
                            movieToGenre.genre_ID);

                    Trace.WriteLine(save_msg);
                }

                //if at a certain amount of adds, save changes, to avoid memory error hopefully
                if (checkpoints.Contains(index))
                {
                    db.SaveChanges();

                    string msg =
                        String.Format("Just saved changes at checkpoint {0}", index);
                    Trace.WriteLine(msg);
                }

                //incrememt index
                index ++;

            }
            Trace.WriteLine("saving final boxarts and genres");
            db.SaveChanges();
            Trace.WriteLine("\t\tdone saving boxarts and genres");
            db.Configuration.AutoDetectChangesEnabled = true;
        }

        //from a movie.movie_ID, find the matching omdbEntry
        public static OmdbEntry MatchMovieIdToOmdbEntry(int movie_ID)
        {
            MovieDbContext db = new MovieDbContext();
            var omdbEntry = db.Omdb.FirstOrDefault(omdb => omdb.movie_ID == movie_ID);

            return omdbEntry;
        }

        public static void RebuildOmdbsFromProtobufDump(string entryDumpPath)
        {
//rebuild the serialized list of List<omdbentryies>

            //deserialize the list of omdbentries saved the the file
            //TODO: add check to make sure the file exists and is not corrupted
            List<OmdbEntry> complete_list;
            using (var file = File.OpenRead(entryDumpPath)) {
                complete_list = Serializer.Deserialize<List<OmdbEntry>>(file);
            }

            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            int count = complete_list.Count;
            foreach (OmdbEntry omdbEntry in complete_list) {
                db.Omdb.Add(omdbEntry);

                //int remaining = count - complete_list.IndexOf(omdbEntry);
                //Trace.WriteLine(remaining);
            }

            Trace.WriteLine("saving changes");
            db.Configuration.AutoDetectChangesEnabled = true;
            db.SaveChanges();


            WriteTimeStamp("Done saving changes");
        }

        public static void SerializeOmdbTsv(string entryDumpPath, string imdbPath, string tomPath)
        {
//TODO:Make these paths more general

            var complete_list_of_entries =
                TSVParse.ParseTSVforOmdbData(imdb_filepath: imdbPath,
                                             tom_filepath: tomPath);

            WriteTimeStamp("Starting to serialize list");
            using (var file = File.Create(entryDumpPath)) {
                Serializer.Serialize(file, complete_list_of_entries);
            }
            WriteTimeStamp("Done serializing list");
        }

        /// <summary>
        /// Joins lines that don't start with a given string, default "<catalog"
        /// </summary>
        /// <param name="filepath"></param>
        ///<param name="start_string">string to match on the start of the line</param>
        /// <param name="skip_default_xml">whether or not to skip the fist two lines and the last one</param>
        public static void JoinLines(string filepath,
                                     string start_string = "<catalog",
                                     bool skip_default_xml = true)
        {

            //read the file into a list of lines
            WriteTimeStamp("Reading file");
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(filepath)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    lines.Add(line);
                }
            }

            //remove the lines that start with the lines below
            if (skip_default_xml) {
                WriteTimeStamp("removing default xml");
                lines =
                    lines.Where(
                        line =>
                        (!line.StartsWith(
                            "<?xml version=\"1.0\" standalone=\"yes\"?>") &&
                         !line.StartsWith("<catalog_titles>") &&
                         !line.StartsWith("</catalog_titles>"))).ToList();
            }

            List<string> fixed_lines = new List<string>();

            //find all lines that don't star with <catalog
            WriteTimeStamp("removing lines that don't match the string");
            foreach (string line in lines) {
                if (line.StartsWith(start_string) != true) {
                    TraceLine(line);
                    //System.Diagnostics.Debug.WriteLine(line);

                    //line above current line
                    int curr_line = lines.IndexOf(line);
                    string above = lines[curr_line - 1];

                    //join lines
                    string joined = String.Format("{0} {1}", above.TrimEnd(), line.TrimStart());

                    //add joined lines to list
                    fixed_lines.RemoveAt(fixed_lines.Count - 1);
                    fixed_lines.Add(joined);
                }

                    //no change, add current line
                else {
                    fixed_lines.Add(line);
                }
            }

            //write file from fixed_lines
            WriteTimeStamp("writing fixed file");
            using (StreamWriter writer = new StreamWriter(filepath)) {
                foreach (string fixedLine in fixed_lines) {
                    writer.WriteLine(fixedLine);
                }
            }
        }

        public static void BuildMoviesBoxartGenresTables(string filepath)
        {
            Trace.WriteLine("starting Full Action");
            string msg = DateTime.Now.ToShortTimeString();
            var start_time = DateTime.Now;
            Trace.WriteLine(msg);
            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;


            WriteTimeStamp("starting data read");

            // Go line by line, and parse it for Movie files
            Dictionary<Movie, Title> dictOfMoviesTitles = new Dictionary<Movie, Title>();
            string data;
            int count = 0;
            using (StreamReader reader = new StreamReader(filepath)) {
                Trace.WriteLine("Starting to read");

                data = reader.ReadLine();
                try {
                    while (data != null) {
                        if (!data.StartsWith("<catalog_title>")) {
                            Trace.WriteLine(
                                "Invalid line of XML, probably CDATA or something\n***{0}", data);
                        }
                        else {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                Create.ParseXmlForCatalogTitles(data);
                            Movie movie =
                                Create.CreateMovie(titles[0]);

                            //add to DB and dict
                            //listOfMovies.Add(movie);
                            dictOfMoviesTitles[movie] = titles[0];
                            db.Movies.Add(movie);


                            //log adding data
                            //msg =
                            //    String.Format(
                            //        "Added item {0} to database, moving to next one",
                            //        count.ToString());
                            //Trace.WriteLine(msg);
                            count += 1;
                        }
                        data = reader.ReadLine();
                    }

                    //save the movies added to db
                    Trace.WriteLine("Saving Movies");
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                catch (XmlException ex) {
                    Trace.WriteLine(
                        "Done parsing the XML because of something happened. Probably the end of file:");
                    Trace.WriteLine(ex.Message);
                    msg =
                        String.Format(
                            "Failed around item {0} to database",
                            count.ToString());
                    Trace.WriteLine(msg);
                }

                db.SaveChanges();
                Trace.WriteLine("Adding Boxart and Genre");
                //add boxart and genre data to db before saving the movie 
                AddBoxartsAndMovieToGenreData(dictOfMoviesTitles, db);


                Trace.WriteLine("Saving Changes any untracked ones");
                db.SaveChanges();
                Trace.WriteLine(
                    "Done Saving! Check out Movies/index for a table of the stuff");
            }


            var end_time = WriteTimeStamp("Done everything");

            TimeSpan span = end_time - start_time;
            Trace.WriteLine("It took this long:");
            Trace.WriteLine(span);
        }

        public static IQueryable<NfImdbRtViewModel> GetFullDbQuery(MovieDbContext db)
        {
//pulls all the mtgs and joins the genre_strings to the appropriate movie_id
            // so the end result is something like {terminator's movie_id : ["action", "drama"]}
            // but is a IGrouping, so it handles a bit weird.
            TraceLine("Group the genres by movie ID");
            var movieID_genreString_grouping = from mtg in db.MovieToGenres
                                               join genre in db.Genres on
                                                   mtg.genre_ID equals
                                                   genre.genre_ID
                                               group genre.genre_string by
                                                   mtg.movie_ID;


            //create the list of NITVMs
            TraceLine("Build the query for all the movies in the DB");
            var nitvmQuery =
                //left outer join so that all movies get selected even if there's no omdb match
                from movie in db.Movies
                join omdb in db.Omdb on
                    movie.movie_ID equals omdb.movie_ID into mov_omdb_matches
                from mov_omdb_match in mov_omdb_matches.DefaultIfEmpty()
                //match the boxarts
                from boxart in db.BoxArts
                where movie.movie_ID == boxart.movie_ID
                //match the genres
                from grp in movieID_genreString_grouping
                where grp.Key == movie.movie_ID
                //create the NITVM
                select new NfImdbRtViewModel
                           {
                               Movie = movie,
                               Boxarts = boxart,
                               Genres = grp,
                               OmdbEntry = mov_omdb_match
                           };

            return nitvmQuery;
        }

        public static void MarryMovieToOmdb(MovieDbContext db)
        {
//get list of movies
            //var movie_queryable = db.Movies.AsQueryable();

            //TODO: match better, curr. only finds 6k movies but there should be closer to 56k

            ////take only movies, since OMDB doesn't hold tv shows
            //var movie_list = db.Movies.ToList().Where(movie => movie.is_movie);
            //var omdb_list = db.Omdb.ToList();
            //Dictionary<Movie, OmdbEntry> matches_MtO =
            //    new Dictionary<Movie, OmdbEntry>();


            //pseudo code for what I want
            //where omdb.title == movie.short_title && omdb.year == movie.year select new {omdb_id, movie_id}

            TraceLine("Starting LINQ query");

            var query = from movie in db.Movies
                        join omdbEntry in db.Omdb on
                            new {name = movie.short_title, year = movie.year}
                            equals
                            new {name = omdbEntry.title, year = omdbEntry.year}
                        select new {movie = movie, omdbEntry = omdbEntry};

            TraceLine("Done LINQ query");
            TraceLine("turning results into a list");
            var resultList = query.ToList();
            TraceLine("done turning results into a list, found {0} movies",
                            resultList.Count);

            //update movie_id for each of the matched omdbs

            Trace.WriteLine("Looping through pairs");
            foreach (var pair in resultList) {
                OmdbEntry omdb = pair.omdbEntry;
                omdb.movie_ID = pair.movie.movie_ID;

                //Tools.TraceLine("m: {0} ID: {2}\no: {1}", pair.movie.short_title,
                //                omdb.title, pair.movie.movie_ID);

                ////might not be needed since its being tracked along the same context
                //db.Entry(omdb).State = EntityState.Modified;
            }

            Trace.WriteLine("done looping");

            Trace.WriteLine("starting to save changes");
            db.SaveChanges();
            Trace.WriteLine("done saving changes");
        }
    }


    
}
