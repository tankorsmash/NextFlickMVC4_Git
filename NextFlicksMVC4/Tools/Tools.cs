using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
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
using System.Text.RegularExpressions;

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
        /// <param name="writeTime">whether or not to actually write to log, default true</param>
        /// <returns>Returns the DateTime that was written</returns>
        public static DateTime WriteTimeStamp(string msg= "The time is", DateTime? presetDateTime = null, bool writeTime = true)
        {
            DateTime time;
            if (presetDateTime == null) { time = DateTime.Now; }
            else { time = (DateTime) presetDateTime; } 

            string to_write = String.Format("{0} {1}: {2}", time.ToShortDateString(), time.ToShortTimeString(), msg);
            if (writeTime) {
                TraceLine(to_write);
            }

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
        [Obsolete("don't use anymore", true)]
        public static List<FullViewModel> FilterMovies(
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
                TraceLine("Starting to filter...");

            //year released
            if (verbose == true)
                TraceLine("\tYear Start");
            movie_list =
                movie_list.Where(item => item.year >= year_start)
                          .ToList();
            if (verbose == true)
                TraceLine("\tYear End");
            movie_list =
                movie_list.Where(item => item.year <= year_end).ToList();

            //title
            if (title != "")
            {
                if (verbose == true)
                    TraceLine("\tTitle");
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
                    TraceLine("\tRuntime Start");
                movie_list =
                    movie_list.Where(
                        item => item.runtime >= runtime_start)
                              .ToList();
                if (verbose == true)
                    TraceLine("\tRuntime End");
                movie_list =
                    movie_list.Where(
                        item => item.runtime <= runtime_end)
                              .ToList();
            }

            //mpaa rating       OR
            if (mpaa_start != 0 || mpaa_end != 200)
            {
                if (verbose == true)
                    TraceLine("\tMPAA Start");
                movie_list =
                    movie_list.Where(
                        item =>
                        item.maturity_rating >=
                        mpaa_start).ToList();
                if (verbose == true)
                    TraceLine("\tMPAA End");
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
                    TraceLine("\tGenres");
                movie_list = ReduceMovieListToMatchingGenres(db, movie_list,
                                                             genre);
            }

            //is_movie
            if (is_movie != null)
            {
                if (verbose == true)
                    TraceLine("\tIs Movie");
                movie_list =
                    movie_list.Where(item => item.is_movie == is_movie).ToList();
            }


            //pass the movie_list through a function to marry them to genres and boxarts
            if (verbose == true)
                TraceLine("\tCreating MtGVM from a range in movies_list");
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
                            .OrderByDescending(item => item.OmdbEntry.t_Meter)
                            .ToList();
            }
            return nit_list;

        }

        


        /// <summary>
        /// tries to convert a string to a double, returns 0.0 if failure
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
        public static List<FullViewModel> MatchMwGVMsWithOmdbEntrys(List<MovieWithGenreViewModel> MwG_list)
        {
            List<FullViewModel> nit_list = new List<FullViewModel>();
            foreach (var MwGVM in MwG_list) {
                //find the matching omdb entry based on the MwGVM's movie_ID
                OmdbEntry omdbEntry =
                    MatchMovieIdToOmdbEntry(MwGVM.movie.movie_ID);
                //create the NitVM based on the new omdb entry
                FullViewModel nitvm = new FullViewModel
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
            string msg = vals.Any()
                             ? String.Format(msg_string, vals)
                             : msg_string;
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
                //TraceLine(
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
            TraceLine("\t\tFind movies that match genre_string");
            var movie_ids_for_genres = GetMovieIdsMatchingGenres(db, genre);

            //execute the find movie_id finding by calling the list
            TraceLine("\t\tList the movies that match genre_string");
            var movie_ids_for_genres_list = movie_ids_for_genres.ToList();

            TraceLine("\t\tFind moves that match movie_id");

            TraceLine("\t\tmovie_list to Array");
            var movie_array = movie_list.ToArray();


            //the iqueryable  for finding movies that match the movie_list
            var movie_iqry =
                movie_array.Where(
                    item => movie_ids_for_genres_list.Contains(item.movie_ID));


            TraceLine("\t\tTo List");
            movie_list = movie_iqry.ToList();
            //movie_list = movie_array.ToList();
            return movie_list;
        }

        ///// <summary>
        ///// returns the integer of a year, or 0, if not found
        ///// </summary>
        ///// <param name="movie"></param>
        ///// <returns></returns>
        //[Obsolete("Movie Class doesn't have a string year property", true)]
        //public static int GetYearOr0(Movie movie)
        //{
        //    //if (movie.year != "") {
        //    //    return Convert.ToInt32(movie.year);
        //    //}
        //    //else
        //    //    return 0;
        //    return 0;
        //}

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
            TraceLine("In AddBoxartsAndMovieToGenreData");

            //get a list of ints that we can test against to save progress of adding to db
            List<int> checkpoints = ProgressList.CreateListOfCheckpointInts(dictOfMoviesTitles.Count, 55);
            
            //MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

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
                    MovieToGenre movieToGenre = Create.CreateMovieMovieToGenre(movie, genre);
                    db.MovieToGenres.Add(movieToGenre);
                    //db.SaveChanges();

                    //var save_msg =
                    //    String.Format(
                    //        "done adding MtG mtg_id = {0}\n movie_id = {1}\n genre_id = {2}",
                    //        movieToGenre.movie_to_genre_ID,
                    //        movieToGenre.movie_ID,
                    //        movieToGenre.genre_ID);

                    //TraceLine(save_msg);
                }

                //if at a certain amount of adds, save changes, to avoid memory error hopefully
                if (checkpoints.Contains(index))
                {
                    db.SaveChanges();

                    string msg =
                        String.Format("  Just saved changes at checkpoint {0}", index);
                    TraceLine(msg);
                }

                //incrememt index
                index ++;

            }
            TraceLine("  saving final boxarts and genres");
            db.SaveChanges();
            TraceLine("\t\tdone saving boxarts and genres");
            db.Configuration.AutoDetectChangesEnabled = true;

            TraceLine("Out AddBoxartsAndMovieToGenreData");
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
            TraceLine("In RebuildOmdbsFromProtobufDump");
//rebuild the serialized list of List<omdbentryies>

            //deserialize the list of omdbentries saved the the file
            //TODO: add check to make sure the file exists and is not corrupted
            List<OmdbEntry> complete_list;
            using (var file = File.OpenRead(entryDumpPath)) {
                complete_list = Serializer.Deserialize<List<OmdbEntry>>(file);
            }

            //takes a list of omdbentrys and then saves them to the db
            SaveListOfOmdbEntrys(complete_list);
            TraceLine("Out RebuildOmdbsFromProtobufDump");
        }

        public static void SaveListOfOmdbEntrys(List<OmdbEntry> complete_list)
        {
            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            //create a list of hashes for the omdbentrys in the db
            List<int> omdbHashes = new List<int>();
            foreach (OmdbEntry omdb in db.Omdb) {
                omdbHashes.Add(omdb.GetHashCode());
            }
            //create a list of hashes for the omdbentrys in the list 
            List<int> listHashes = new List<int>();
            foreach (OmdbEntry listOmdb in complete_list) {
                listHashes.Add(listOmdb.GetHashCode());
            }

            //find duplicates in the list of hashes for the db
            var dbDupes =
                omdbHashes.GroupBy(x => x)
                          .Where(group => @group.Count() > 1)
                          .Select(group => @group.Key)
                          .ToList();
            //find duplicates in the list of hashes for the list
            var fileDupes =
                listHashes.GroupBy(x => x)
                          .Where(group => @group.Count() > 1)
                          .Select(group => @group.Key)
                          .ToList();
            //print the hashes that are duplicated (?)
            foreach (var r in dbDupes) {
                TraceLine(r.ToString());
            }

            //total movines in the passed in list of omdbentrys
            int count = complete_list.Count;
            const int quarter_1 = 25000;
            const int quarter_2 = 50000;
            const int quarter_3 = 75000;
            const int quarter_4 = 100000;
            const int quarter_5 = 125000;
            const int quarter_6 = 150000;
            const int quarter_7 = 175000;
            const int quarter_8 = 200000;


            int index = 0;
            foreach (OmdbEntry omdbEntry in complete_list) {
                //if the current omdbentry hash is already in the db, remove it from the
                // list of omdb hashes. (for optimization I guess? I don't know why you're doing that,
                //  that means that once you find a single dupe, you'll never find a second third or fourth 
                //  dupe even if it exists, but I'm probably just misunderstanding)
                if (omdbHashes.Contains(omdbEntry.GetHashCode())) {
                    omdbHashes.Remove(omdbEntry.GetHashCode());
                }
                else {
                    db.Omdb.Add(omdbEntry);
                }

                switch (index) {
                    case quarter_1:
                    case quarter_2:
                    case quarter_3:
                    case quarter_4:
                    case quarter_5:
                    case quarter_6:
                    case quarter_7:
                    case quarter_8:
                        TraceLine("Saving changes at quarter {0}", index / 25000);
                        db.SaveChanges();
                        break;
                }
                index++;
                //int remaining = count - complete_list.IndexOf(omdbEntry);
                //TraceLine(remaining);
            }

            TraceLine("  saving changes");
            db.Configuration.AutoDetectChangesEnabled = true;
            db.SaveChanges();


            WriteTimeStamp("  Done saving changes");
        }

        public static void SerializeOmdbTsv(string entryDumpPath, string imdbPath, string tomPath)
        {

            TraceLine("In SerializeOmdbTsv");

            var complete_list_of_entries =
                TSVParse.ParseTSVforOmdbData(imdb_filepath: imdbPath,
                                             tom_filepath: tomPath);

            WriteTimeStamp("  Starting to serialize list");
            using (var file = File.Create(entryDumpPath)) {
                Serializer.Serialize(file, complete_list_of_entries);
            }
            WriteTimeStamp("  Done serializing list");
            TraceLine("Out SerializeOmdbTsv");
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
            TraceLine("In JoinLines");
            //keep track of how many malformed lines I have found in a row, 1 = record the line to join the next line to
            // 2 lines found means join lines, write file and recurse through again.
            bool found_broken_lines = false;
            //try my code here and comemnt out all the other stuff.
            //Tankorsmash's code is great and works well if you are not on 32bit, with 2GB memory cap, I can't use this code.
            //come up with a new function that checks each line to see if it matches the ^<catalog and if not join to the above line
            Regex startsWith = new Regex(@"^<catalog_title");
            Regex startRoot = new Regex(@"^<catalog_titles>");
            Regex endRoot = new Regex(@"^</catalog_titles>");
            Regex endsWith = new Regex(@"title>$");
            Regex startsWithXML = new Regex(@"^<\?xml");

            //keep a list of new lines that have been meregs. I think i can just cram em in at position 2 over and over again, not 
            //sure the overall order of titles in the xml matters.
            string combinedLines = "";
            
            string lastline = ""; //for keepign track of the last line in case we need to merge the two
            List<string> malformedLines = new List<string>(); //keep track of all malformed lines in a row and then add them together then write the line
            string fixedLine = "";
            string line = "";
            using (StreamReader reader = new StreamReader(filepath))
            {
                using (StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/dbfiles/temp.NFPOX")))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        var trimmedLine = line.Trim();

                        Match matchStart = startsWith.Match(trimmedLine);
                        Match matchXML = startsWithXML.Match(trimmedLine);
                        Match matchEnd = endsWith.Match(trimmedLine);
                        Match matchRootStart = startRoot.Match(trimmedLine);
                        Match matchRootEnd = endRoot.Match(trimmedLine);

                        if ((matchXML.Success || matchRootStart.Success || matchRootEnd.Success) ||
                            (matchStart.Success && matchEnd.Success))
                        {
                            if (malformedLines.Count > 0)
                            {
                                var fixedXml = String.Join(String.Empty, malformedLines.ToArray());
                                writer.WriteLine(fixedXml);
                                malformedLines.Clear();
                            }

                            writer.WriteLine(trimmedLine);
                            lastline = trimmedLine;
                        }

                        else //if (!matchStart.Success)
                        {
                            found_broken_lines = true;
                            malformedLines.Add(trimmedLine);
                            //combinedLines = lastline + trimmedLine;
                            //writer.WriteLine(combinedLines);
                            //lastline = ""; //do this so that we dont end up with some malformed aborted fetus of an xml code

                        }
                    }
                }
            }

            //change the files around, overwrite fixedAPI.nfpox so that we can go through the same process again if neccesary
            //if not it will jsut write temp to fixedapi so we are good to go next step.
            var fixedAPI = HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX");
            var temp = HttpContext.Current.Server.MapPath("~/dbfiles/temp.NFPOX");
            var backup = HttpContext.Current.Server.MapPath("~/dbfiles/backup.NFPOX");
            if (File.Exists(fixedAPI) && File.Exists(temp))
            {
                File.Replace(temp, fixedAPI, backup);
                File.Delete(temp);

            }
            if (found_broken_lines)
            {
                JoinLines(fixedAPI);
            }
           
            /*
            //read the file into a list of lines
            WriteTimeStamp("  Reading file");
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(filepath)) 
            {
                string line;
                while ((line = reader.ReadLine()) != null) 
                {
                    lines.Add(line);
                }
            }
            //MatchCollection NonMatchngLines = new MatchCollection();
           
                //remove the lines that start with the lines below
            if (skip_default_xml) 
            {
                WriteTimeStamp("  removing default xml");
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
            WriteTimeStamp("  removing lines that don't match the string");
            foreach (string line in lines) {
                if (line.StartsWith(start_string) != true) {
                    TraceLine(line);

                    //set found_matching_lines to true because we found a matching line and should check 
                    //another time to make sure there ar eno leftovers. Otherwise it will be false and the function will 
                    //exit at the end.
                    found_matching_lines = true; 

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
            
            //add the first two lines back to fixed lines and the last line so it is  avalid xml doc with root elements
            fixed_lines.Insert(0, "<?xml version=\"1.0\" standalone=\"yes\"?>"); //<?xml  version= 1.0
            fixed_lines.Insert(1, "<catalog_titles>"); // <catalog_titles>
            fixed_lines.Insert(fixed_lines.Count, "</catalog_titles>"); // </catalog_titles>
            //write file from fixed_lines
            WriteTimeStamp("  writing fixed file");
            using (StreamWriter writer = new StreamWriter(filepath)) {
                foreach (string fixedLine in fixed_lines) {
                    writer.WriteLine(fixedLine);
                }
            }
            if (found_matching_lines)
            {
                lines = new List<string>();
                fixed_lines = new List<string>();
                GC.Collect();
                JoinLines(filepath);
            }
             * */
            TraceLine("Out JoinLines");
        }

        public static void BuildMoviesBoxartGenresTables(string filepath)
        {
            TraceLine("In BuildMoviesBoxartGenresTables");
            TraceLine("  starting Full Action");

            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            var start_time = WriteTimeStamp("starting data read");
            List<int> dbHashes = new List<int>();//get alist of hashes for all movies in the db so we can make sure not to add a duplicate;
            //int[] dbHashes = new int[db.Movies.Count()];

            foreach (Movie dbMovie in db.Movies)
            {
                dbHashes.Add(dbMovie.GetHashCode());
            } 

            // Go line by line, and parse it for Movie files
            Dictionary<Movie, Title> dictOfMoviesTitles = new Dictionary<Movie, Title>();
            int count = 0;
            using (StreamReader reader = new StreamReader(filepath)) 
            {
                TraceLine("  Starting to read");

                string line = reader.ReadLine();
                line = line.Trim();
                //try {
                    while (line != null) 
                    {
                        if (!line.StartsWith("<catalog_title>")) 
                        {
                            TraceLine("  Invalid line of XML, probably CDATA or something, make sure all the lines start with '<cata' \n{0}", line);
                        }
                        else 
                        {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                Create.ParseXmlForCatalogTitles(line);

                            //if there was a title to parse add it to the db
                            if (titles != null) 
                            {
                                Movie movie =
                                    Create.CreateMovie(titles[0]);

                                if (dbHashes.Contains(movie.GetHashCode()))
                                {
                                    dbHashes.Remove(movie.GetHashCode());
                                }
                                else
                                {
                                    //add to DB and dict
                                    dictOfMoviesTitles[movie] = titles[0];
                                    db.Movies.Add(movie);
                                }
                            }
                            else 
                            {
                                TraceLine("  Failed on line {0}\n{1}", count, line);
                            }

                            count += 1;
                        }
                        line = reader.ReadLine();
                    }

                    //save the movies added to db

                    TraceLine("  Saving Movies");
                    TraceLine(dictOfMoviesTitles.Count().ToString());
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                //}

                //catch (XmlException ex) {
                //    TraceLine(
                //        "  Done parsing the XML because of something happened. Probably the end of file:");
                //    TraceLine(ex.Message);
                //    TraceLine(" Failed around item {0} to database",
                //              count.ToString());
                //}

                db.SaveChanges();
                TraceLine("  Adding Boxart and Genre");
                //add boxart and genre data to db before saving the movie 
                AddBoxartsAndMovieToGenreData(dictOfMoviesTitles, db);


                TraceLine("  Saving Changes any untracked ones");
                db.SaveChanges();
                TraceLine(
                    "  Done Saving! Check out Movies/index for a table of the stuff");
            }


            var end_time = WriteTimeStamp(    "  Done everything");

            TimeSpan span = end_time - start_time;
            TraceLine("  It took this long:");
            TraceLine(span.ToString());

            TraceLine("Out BuildMoviesBoxartGenresTables");
        }

        public static IQueryable<FullViewModel> GetFullDbQuery(MovieDbContext db, bool verbose = false)
        {

            if (verbose)
            {
                TraceLine("In GetFullDbQuery");
            }
            //pulls all the mtgs and joins the genre_strings to the appropriate movie_id
            // so the end result is something like {terminator's movie_id : ["action", "drama"]}
            // but is a IGrouping, so it handles a bit weird.
            if (verbose)
            {
                TraceLine("  Group the genres by movie ID");
            }
            var movieID_genreString_grouping = from mtg in db.MovieToGenres
                                               join genre in db.Genres on
                                                   mtg.genre_ID equals
                                                   genre.genre_ID
                                               group genre.genre_string by
                                                   mtg.movie_ID;


            //a default empty element
            OmdbEntry defaultOmdbEntry = new OmdbEntry
                                             {
                                             };

            //create the list of NITVMs
            if (verbose)
            {
                TraceLine("  Build the query for all the movies in the DB");
            }
            var nitvmQuery =
                //left outer join so that all movies get selected even if there's no omdb match
                from movie in db.Movies
                join omdb in db.Omdb on
                    movie.movie_ID equals omdb.movie_ID into mov_omdb_matches
                from mov_omdb_match in mov_omdb_matches.DefaultIfEmpty()
                //match the boxarts
                from boxart in db.BoxArts
                where movie.movie_ID == boxart.movie_ID
                //match the genres string
                from grp in movieID_genreString_grouping
                where grp.Key == movie.movie_ID
                //match the genre id



                //create the NITVM
                select new FullViewModel
                           {
                               Movie = movie,
                               Boxarts = boxart,
                               Genres = grp,
                               Genre_IDs = (List<int>)(from mtg in db.MovieToGenres
                                                       where mtg.movie_ID == movie.movie_ID
                                                       select mtg.genre_ID),
                               //OmdbEntry = (mov_omdb_match == null) ? mov_omdb_match.movie_ID= movie.movie_ID: mov_omdb_match
                               OmdbEntry = mov_omdb_match
                           };

            if (verbose)
            {
                TraceLine("Out GetFullDbQuery");
            }
            return nitvmQuery;
        }

        /// <summary>
        /// Looks through db.Movies and db.Omdb and finds the matching Movies and OmdbEntrys and assigns the movie_ids to the OmdbEntry
        /// </summary>
        /// <param name="db"></param>
        public static void MarryMovieToOmdb(MovieDbContext db)
        {
            //get list of movies
            //var movie_queryable = db.Movies.AsQueryable();
            TraceLine("In MarryMovieToOmdb");
            TraceLine("  Starting LINQ query to marry movies to omdb");

            //find the matches from the Movies and Omdb dbs and create a new 
            // anonymous object to hold each entry
            var query = from movie in db.Movies
                        join omdbEntry in db.Omdb on
                            new {name = movie.short_title, year = movie.year}
                            equals
                            new {name = omdbEntry.title, year = omdbEntry.year}
                        select new {movie = movie, omdbEntry = omdbEntry};

            TraceLine("  Done LINQ query");
            TraceLine("  turning results into a list");
            var resultList = query.ToList();
            TraceLine("  done turning results into a list, found {0} movies",
                            resultList.Count);

            //update movie_id for each of the matched omdbs

            TraceLine("  Looping through pairs");
            foreach (var pair in resultList) {
                OmdbEntry omdb = pair.omdbEntry;
                omdb.movie_ID = pair.movie.movie_ID;

                //Tools.TraceLine("  m: {0} ID: {2}\no: {1}", pair.movie.short_title,
                //                omdb.title, pair.movie.movie_ID);

                ////might not be needed since its being tracked along the same context
                //db.Entry(omdb).State = EntityState.Modified;
            }

            TraceLine("  done looping");

            TraceLine("  starting to save changes");
            db.SaveChanges();
            TraceLine("  done saving changes");
            TraceLine("Out MarryMovieToOmdb");

        }

        public static SortedDictionary<string, int> CreateSortedTagDictionary(MovieDbContext db, bool verbose=false)
        {
            //create a Dict<string,int> for all tag_string and ids, so that they 
            // can be enumerated in the filtermenu. So the user can select which tags 
            // they want to look for
            DateTime dict_start = new DateTime();
            if (verbose) {
                dict_start = WriteTimeStamp("  Start dict make");
            }
            Dictionary<string, int> tag_dict =
                db.MovieTags.Distinct().ToDictionary(tag => tag.Name,
                                                  tag => tag.TagId);
            //sort the dictionary, automatically does it by key, seems like
            SortedDictionary<string, int> sortedDictionary =
                new SortedDictionary<string, int>(tag_dict);

            DateTime dict_end;
            if (verbose) {
                dict_end = WriteTimeStamp("  End dict make");
                TraceLine("tag dict took {0}", dict_end - dict_start);
            }
            return sortedDictionary;
        }


        public static SortedDictionary<string, int> CreateSortedGenreDictionary(MovieDbContext db)
        {
//create a Dict<string,int> for all genre_string and ids, so that they 
            // can be enumerated in the filtermenu. So the user can select which genres 
            // they want to look for
            //var dict_start = WriteTimeStamp("  Start dict make");
            Dictionary<string, int> genre_dict =
                db.Genres.Distinct().ToDictionary(gen => gen.genre_string,
                                                  gen => gen.genre_ID);
            //sort the dictionary, automatically does it by key, seems like
            SortedDictionary<string, int> sortedDictionary =
                new SortedDictionary<string, int>(genre_dict);
            //var dict_end = WriteTimeStamp("  End dict make");
            //TraceLine("genre dict took {0}", dict_end - dict_start);
            return sortedDictionary;
        }

        public static IQueryable<FullViewModel> FilterTags(string tag_string,
                                                           MovieDbContext db)
        {

            //find the tag id for the string
            MovieTag searched_tag = db.MovieTags.First(tag => tag.Name == tag_string);

            //find all movies tagged by this tag
            var res = from umt in db.UserToMovieToTags
                      where umt.TagId == searched_tag.TagId
                      select umt.movie_ID;

            var distinct_movie_ids_qry = res.Distinct();

            //pull only the matching FullViews from  the db
            var total_qry = GetFullDbQuery(db);
            var taggedFullViews = from nit in total_qry
                                  from movie_id in distinct_movie_ids_qry
                                  where nit.Movie.movie_ID == movie_id
                                  select nit;

            return taggedFullViews;

        }

        public static IQueryable<FullViewModel> FilterMoviesAndGenres(
            string movie_title,
            MovieDbContext db,
            string genre_select = "0")
        {



            //get a full query with all data in db
            var total_qry = GetFullDbQuery(db);

            //filters the movie quickly enough
            // basic stuff 
            var res =
                from nit in total_qry
                where nit.Movie.short_title.Contains(movie_title)
                select nit;

            //if the genre isn't the default value, filter the results even more
            if (genre_select != "0") {
                res = res.Where(nit => nit.Genres.Any(item => item == genre_select));
            }
            return res;
        }

        /// <summary>
        /// Take  a IEnum and turns the items into SelectListItems
        /// </summary>
        /// <param name="all_years"></param>
        /// <returns></returns>
        public static List<SelectListItem> IEnumToSelectListItem(IEnumerable<int> all_years)
        {
            var a_list = new List<SelectListItem>();
            foreach (int year in all_years) {
                SelectListItem sli = new SelectListItem();
                sli.Text = year.ToString();
                sli.Value = year.ToString();

                a_list.Add(sli);
            }
            return a_list;
        }
    }


    
}
