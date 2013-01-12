using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NextFlicksMVC4.Controllers;
using System.Diagnostics;
using NextFlicksMVC4.DatabaseClasses;
using NextFlicksMVC4.Helpers;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.Views.Movies.ViewModels;
using NextFlicksMVC4.OMBD;

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
        public static DateTime WriteTimeStamp(string msg, DateTime? presetDateTime = null)
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
        public static List<MovieWithGenreViewModel> FilterMovies(
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
            bool verbose = false)
        {
            if (verbose == true)
                Trace.WriteLine("Starting to filter...");



            //year
            if (verbose == true)
                Trace.WriteLine("\tYear Start");
            movie_list =
                movie_list.Where(item => GetYearOr0(item) >= year_start)
                          .ToList();
            if (verbose == true)
                Trace.WriteLine("\tYear End");
            movie_list =
                movie_list.Where(item => GetYearOr0(item) <= year_end).ToList();
            ///title
            //specific
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

            //runtime
            if (runtime_start != 0 || runtime_end != 9999999)
            {
                if (verbose == true)
                    Trace.WriteLine("\tRuntime Start");
                movie_list =
                    movie_list.Where(
                        item => item.runtime.TotalSeconds >= runtime_start)
                              .ToList();
                if (verbose == true)
                    Trace.WriteLine("\tRuntime End");
                movie_list =
                    movie_list.Where(
                        item => item.runtime.TotalSeconds <= runtime_end)
                              .ToList();
            }
            //mpaa
            if (mpaa_start != 0 || mpaa_end != 200)
            {
                if (verbose == true)
                    Trace.WriteLine("\tMPAA Start");
                movie_list =
                    movie_list.Where(
                        item =>
                        ReturnMaturityOrDefault(item.maturity_rating) >=
                        mpaa_start).ToList();
                if (verbose == true)
                    Trace.WriteLine("\tMPAA End");
                movie_list =
                    movie_list.Where(
                        item =>
                        ReturnMaturityOrDefault(item.maturity_rating) <=
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

            //combine the remaining movies with omdbs and then filter from there too

            MatchMwGVMsWithOmdbEntrys(MwG_list);

            //return the count number of movies to return
                //if (verbose == true)
                //    TraceLine("Total results after Filter: {0}", movie_list.Count);
                //if (verbose == true)
                //    Trace.WriteLine("\tTaking Count");
                //MwG_list = MwG_list.Take(count).ToList();
                //return MwG_list;



            }

        public static void MatchMwGVMsWithOmdbEntrys(List<MovieWithGenreViewModel> MwG_list)
        {
            List<NfImdbRtViewModel> nit_list = new List<NfImdbRtViewModel>();
            foreach (var MwGVM in MwG_list) {
                OmdbEntry omdbEntry =
                    MatchMovieIdToOmdbEntry(MwGVM.movie.movie_ID);
                NfImdbRtViewModel nitvm = new NfImdbRtViewModel
                                              {
                                                  Boxarts = MwGVM .boxart,
                                                  Genres = MwGVM .genre_strings,
                                                  Movie = MwGVM .movie,
                                                  OmdbEntry = omdbEntry
                                              };
                nit_list.Add(nitvm);
            }
        }

        /// <summary>
        /// Wraps Trace.WriteLine, cannot accept category though, due to params 
        /// </summary>
        /// <param name="msg_string"></param>
        /// <param name="vals"></param>
        public static void TraceLine(string msg_string, params object[] vals)
        {
            string msg = String.Format(msg_string, vals);
            Trace.WriteLine(msg);
        }

        public static int ReturnMaturityOrDefault(string maturity_rating)
        {
            try {
                return Convert.ToInt32(maturity_rating);
            }
            catch (Exception ex) {
                
                Trace.WriteLine(String.Format("maturity rating exception, probably not set:\r{0}", ex.Message));
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
            var movie_ids_for_genres = GetMovieIdsMatchingGenres(db, genre
                );
            //OOM error if movie_list is passed
            //,movie_list);

            //execute the find movie_id finding by calling the list
            Trace.WriteLine("\t\tList the movies that match genre_string");
            var movie_ids_for_genres_list = Enumerable.ToList(movie_ids_for_genres);

            Trace.WriteLine("\t\tFind moves that match movie_id");

            Trace.WriteLine("\t\tmovie_list to Array");
            var movie_array = movie_list.ToArray();


            //the iqueryable  for finding movies that match the movie_list
            var movie_iqry =
                //movie_list.Where(
                //item => movie_ids_for_genres_list.Contains(item.movie_ID));

                ////tried using the iQry instead of list and it was slower
                //// so I'm keeping to above line instead of the below one
                // item => movie_ids_for_genres.Contains(item.movie_ID));

                movie_array.Where(
                    item => movie_ids_for_genres_list.Contains(item.movie_ID));

            //Trace.WriteLine("\t\tto array after array.Where");
            //var res_array = movie_iqry.ToArray();

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
    }


    
}
