using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4;
using NextFlicksMVC4.DatabaseClasses;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using System.Timers;
using NextFlicksMVC4.Helpers;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NextFlicksMVC4.Controllers
{
    public class MoviesController : Controller
    {
        //public static MovieDbContext db = new MovieDbContext();


        [HttpGet]
        public ActionResult Filter()
        {
            MovieDbContext db = new MovieDbContext();

            //grab the lowest year in Catalog
            var min_qry = "select  top(1) * from Movies where year != \'\' order by year ASC";
            var min_res = db.Movies.SqlQuery(min_qry);
            var min_list = min_res.ToList();
            string min_year = min_list[0].year;
            ViewBag.min_year = min_year;
            //grab the highest year in catalog
            var max_qry = "select  top(1) * from Movies order by year DESC ";
            var max_res = db.Movies.SqlQuery(max_qry);
            var max_list = max_res.ToList();
            string max_year = max_list[0].year;
            ViewBag.max_year = max_year;

            //Create a list of SelectListItems
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = Int32.Parse(min_year); i <= Int32.Parse(max_year); i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            //Create a selectList, but since it's looking for an IEnumerable,
            // tell it what is the value and text parts
            SelectList slist = new SelectList(years, "Value", "Text");

            //give the Viewbag a property for the SelectList
            ViewBag.years = slist;

            return View();
        }



        public ActionResult FilterHandler(string year_start, string year_end)
        {
            ViewData["year_start"] = year_start;
            ViewData["year_end"] = year_end;
            Trace.WriteLine(ViewData["pet"]);

            return View("FilterHandler");
        }

        /// <summary>
        /// Sloppy return random set of movies. redirects to Index with a random start and count of 10
        /// </summary>
        /// <returns></returns>
        public ActionResult Random()
        {
            MovieDbContext db = new MovieDbContext();

            int rand_title_int = new Random().Next(1, db.Movies.ToList().Count);
            return RedirectToAction("Index", new { count = 1, start = rand_title_int });
        }


        public ActionResult Test(string year= "2000")
        {
            ViewBag.Params = GetAllParamNames("Test");


            MovieDbContext db = new MovieDbContext();

            //var movie_list = db.Movies.Take(100).Take(10).ToList();
            //var movie_list = db.Movies.Where(item => item.year == year).ToList();
            //List<MovieWithGenreViewModel> MwG_list = ModelBuilder.CreateListOfMwGVM(db, movie_list);
            //MwG_list = MwG_list.Where(item => item.movie.runtime.TotalSeconds == 5767).ToList();

            var movie_list = db.Movies.ToList();

            var MwG_list = FilterMovies(db, movie_list, 1990, 2012,
                                        mpaa_end: 100,
                                        is_movie: false,
                                        title: "kid");
                                        //genre: "action");


            

            IEnumerable<MovieWithGenreViewModel> MwG_ienum = MwG_list;

            Trace.WriteLine("Returning View");
            return View("Genres", MwG_ienum);
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
        /// <param name="count">Limits the amount of movies returned -at the very end- of the function instead of the start</param>
        /// <returns></returns>
        public List<MovieWithGenreViewModel> FilterMovies(MovieDbContext db,
                                                          List<Movie> movie_list,
            int year_start = 1914, int year_end = 2012,
            int mpaa_start = 0, int mpaa_end = 200,
            string title = "",
            bool? is_movie = null,
            
            int runtime_start = 0, int runtime_end = 9999999,
            string genre = "", int count =25)
        {
            Trace.WriteLine("Starting to filter...");

            ///gotta figure a way to filter all stuff down
            //year
            Trace.WriteLine("\tYear Start");
            movie_list = movie_list.Where(item => GetYearOr0(item) > year_start).ToList();
            Trace.WriteLine("\tYear End");
            movie_list = movie_list.Where(item => GetYearOr0(item) < year_end).ToList();
            ///title
            //specific
            if (title != "") {
            Trace.WriteLine("\tTitle");
                movie_list =
                    movie_list.Where(item => item.short_title.ToLower().StartsWith(title)).ToList();
            }
            //sort alphabetical

            //netflix rating

            //runtime
            if (runtime_start != 0 || runtime_end != 9999999) {
                Trace.WriteLine("\tRuntime Start");
                movie_list =
                    movie_list.Where(
                        item => item.runtime.TotalSeconds > runtime_start)
                              .ToList();
                Trace.WriteLine("\tRuntime End");
                movie_list =
                    movie_list.Where(
                        item => item.runtime.TotalSeconds < runtime_end)
                              .ToList();
            }
            //mpaa
            if (mpaa_start != 0 || mpaa_end != 200) {
                Trace.WriteLine("\tMPAA Start");
                movie_list =
                    movie_list.Where(
                        item =>
                        ReturnMaturityOrDefault(item.maturity_rating) >=
                        mpaa_start).ToList();
                Trace.WriteLine("\tMPAA End");
                movie_list =
                    movie_list.Where(
                        item =>
                        ReturnMaturityOrDefault(item.maturity_rating) <=
                        mpaa_end).ToList();
            }

            //genre
            if (genre != "") {
                Trace.WriteLine("\tGenres");

                Trace.WriteLine("\t\tFind movies that match genre_string");
                movie_list = ReduceMovieListToMatchingGenres(db, movie_list, genre);
            }

            //is_movie
            if (is_movie != null) {
                Trace.WriteLine("\tIs Movie");
                movie_list = movie_list.Where(item => item.is_movie == is_movie).ToList();
            }


            //pass the movie_list through a function to marry them to genres and boxarts
            Trace.WriteLine("\tCreating MtGVM");
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db, movie_list);

            //suppose we could use the netflix api to do searches like director, actor etc

            Trace.WriteLine("\tTaking Count");
            MwG_list= MwG_list.Take(count).ToList();
            return MwG_list;

        }

        public static int ReturnMaturityOrDefault(string maturity_rating)
        {
            try {
                return Convert.ToInt32(maturity_rating);
            }
            catch (Exception ex) {
                
                Trace.WriteLine(string.Format("maturity rating exception, probably not set:\r{0}", ex.Message));
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
            var movie_ids_for_genres = GetMovieIdsMatchingGenres(db, genre, movie_list);

            //execute the find movie_id finding by calling the list
            Trace.WriteLine("\t\tList the movies that match genre_string");
            var movie_ids_for_genres_list = movie_ids_for_genres.ToList();

            Trace.WriteLine("\t\tFind moves that match movie_id");

            //the iqueryable  for finding movies that match the movie_list
            var movie_iqry =
                movie_list.Where(
                    item => movie_ids_for_genres_list.Contains(item.movie_ID));

            Trace.WriteLine("\t\tTo List");
            movie_list = movie_iqry.ToList();
            return movie_list;
        }




        /// <summary>
        /// returns the integer of a year, or 0, if not found
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        public int GetYearOr0(Movie movie)
        {
            if (movie.year != "") {
                return Convert.ToInt32(movie.year);
            }
            else
                return 0;
        }


        public ActionResult Year(int year_start = 2001, int year_end = 2002, int start = 0, int count = 25, bool is_movie = true)
        {
            MovieDbContext db = new MovieDbContext();
            //returns all titles from year
            string qry = "select * from Movies where (year between {0} and {1}) and (is_movie = {2}) order by year";
            var res = db.Movies.SqlQuery(qry, year_start, year_end, is_movie);

            List<Movie> movie_list = res.ToList();

            ViewBag.TotalMovies = movie_list.Count - 1;
            ViewBag.year_start = year_start;
            ViewBag.year_end = year_end;
            ViewBag.start = start;
            ViewBag.count = count;

            if (count > movie_list.Count)
            {
                count = movie_list.Count - 1;
            }
            var results = movie_list.GetRange(start, start + count);
            return View("Index", results);
        }



        /// <summary>
        /// go through db, find id of genre param, go through db again for all movie Ids that match to a genre_id
        /// </summary>
        /// <returns></returns>
        public ActionResult Genres(string genre_params = "action", int count= 25, int start = 0)
        {
            MovieDbContext db = new MovieDbContext();

            //make sure params are set
            if (genre_params == "")
            {
                genre_params = "nothing";
            }

            //get a movie list that matches genres
            var movie_list = GetMoviesMatchingGenre(db, genre_params);
            //creates the MwGVM for the movie list
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db, movie_list);

            //to show a given view what the user searched for
            ViewBag.SearchTerms = genre_params;
            //relectively get the list of parameters for this method and pass them to the view
            ViewBag.Params = GetAllParamNames("Genres");

            if (count > MwG_list.Count)
                count = MwG_list.Count;

            ViewBag.Count = count;
            ViewBag.Start = start;
            ViewBag.TotalMovies = MwG_list.Count;

            return View(MwG_list.GetRange(start,count));

        }

        /// <summary>
        /// returns a queryable of movie_ids that match a genre_string
        /// </summary>
        /// <param name="db"></param>
        /// <param name="genre_params"></param>
        /// <param name="movie_list">if this is given, limit the search to </param>
        /// <returns></returns>
        public static IQueryable<Int32> GetMovieIdsMatchingGenres(
            MovieDbContext db, string genre_params, List<Movie> movie_list= null )
        {
            //finds the genre id matching the argument "genre_params"
            var genre_ids = db.Genres.Where(
                item => item.genre_string.StartsWith(genre_params))
                              .Select(item => item.genre_ID);

            //finds all movie_ids that have the genre_id from above
            var movies_ids_matching_the_genre_iq =
                db.MovieToGenres.Where(item => genre_ids.Contains(item.genre_ID))
                  .Select(item => item.movie_ID);

            //if movie_list was passed, select the movies from m_i_m_t_g_i that are in movie_list
            if (movie_list != null) {
                movies_ids_matching_the_genre_iq.Where(
                    movie_id =>
                    movie_list.Select(item => item.movie_ID).Contains(movie_id));
            }
            //var movies_ids_matching_the_genre =  movies_ids_matching_the_genre_iq.Select(item => item.movie_ID);

            return movies_ids_matching_the_genre_iq;
        }

        //-------------------------------------------------------
      
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


        public class MovieToGenreViewModel
        {
            public int movie_id { get; set; }
            public int genre_id { get; set; }
            public string genre_string { get; set; }
        }

        public  class MovieWithGenreViewModel
        {
            public Movie movie { get; set; }

            [DisplayName("List of Genres")]
            [DataType("CommaList")]
            public List<string> genre_strings { get; set; }

            public BoxArt boxart { get; set; }
        }

        public ActionResult Index(int start = 0, int count = 10)
        {
            MovieDbContext db = new MovieDbContext();

            //create a query string to full the proper count of movies from db
            Trace.WriteLine("Creating a query string");
            //string qry = "select * from" +
            //             " ( select " +
            //             "  ROW_NUMBER() over (order by movie_id) as rownum," +
            //             " *" +
            //             " from Movies) foo" +
            //             " where rownum  between {0} and {1}";
            //var res = db.Movies.SqlQuery(qry, start, start + count);
            //Trace.WriteLine("creating a list from the query");
            //var fullList = res.ToList();

            //select a range of items, with linq rather than with query
            var fullList = db.Movies.OrderBy(item => item.movie_ID).Skip(start).Take(count).ToList();

            //count the total movies in DB
            Trace.WriteLine("Counting movies");
            string count_qry = "select count(movie_id) from Movies";
            var count_res = db.Database.SqlQuery<int>(count_qry);
            int count_count = count_res.ElementAt(0);
            ViewBag.TotalMovies = count_count;

            //misc numbers
            ViewBag.Start = start;
            ViewBag.Count = count;

            //Param names
            Trace.WriteLine("getting param names");
            ViewBag.Params = GetAllParamNames("Index");

            //make sure there's not a outofbounds
            if (count > fullList.Count)
            {
                count = fullList.Count;
                Trace.WriteLine("had to shorten the returned results");
            }

            Trace.WriteLine("Get ranging");
            var full_range = fullList.GetRange(0, count);


            //turn all the movies into MovieWithGenresViewModel
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db,full_range);

            IEnumerable<MovieWithGenreViewModel> MwG_ienum = MwG_list;

            Trace.WriteLine("Returning View");
            return View("Genres",MwG_ienum);
        }

        public ActionResult Table(int start = 0, int count = 10)
        {

            MovieDbContext db = new MovieDbContext();

            Trace.WriteLine("To QRY");
            //var fullList = db.Movies.ToList();
            string qry = "select * from" +
                         " ( select " +
                         "  ROW_NUMBER() over (order by movie_ID) as rownum," +
                         " *" +
                         " from Movies) foo" +
                         " where rownum  between {0} and {1}";
            var res = db.Movies.SqlQuery(qry, start, start + count);
            Trace.WriteLine("To list");
            var fullList = res.ToList();

            //total movies in DB
            ViewBag.TotalMovies = fullList.Count;
            ViewBag.Start = start;
            ViewBag.Count = count;

            Trace.WriteLine("Get ranging");
            var full_range = fullList.GetRange(0, count);

            Trace.WriteLine("Returning View");
            return View(full_range);

        }

        public ActionResult MtG()
        {
            //need to find movie_id, its genres and its boxarts
            //could recreate all titles, find its matching webpage in the db, then the movie_id and then use that

            Trace.WriteLine("starting Full Action");
            string msg = DateTime.Now.ToShortTimeString();
            var start_time = DateTime.Now;
            Trace.WriteLine(msg);
            MovieDbContext db = new MovieDbContext();
            //------------------------------------------------------

            //need to have a Genre table first, so make sure that's there
            //int genre_count = db.Genres.Count();
            //if (genre_count == 0)
            //{

            //}

            //PopulateGenres.PopulateGenresTable();


            //------------------------------------------------------

            Trace.WriteLine("starting data read");
            msg = DateTime.Now.ToShortTimeString();
            Trace.WriteLine(msg);

            //get list of movies in db
            var movieDbList = db.Movies.ToList();

            // Go line by line, and parse it for Movie files
            //List<Movie> listOfMovies = new List<Movie>();
            Dictionary<Movie, Title> dictOfMoviesTitles = new Dictionary<Movie, Title>();
            string data;
            int count = 0;
            using (StreamReader reader = new StreamReader(@"C:\fixedAPI.NFPOX"))
            {

                Trace.WriteLine("Starting to read");

                data = reader.ReadLine();
                try
                {
                    while (data != null)
                    {
                        if (!data.StartsWith("<catalog_title>"))
                        {
                            Trace.WriteLine("Invalid line of XML, probably CDATA or something");
                        }
                        else
                        {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                NextFlicksMVC4.Create.ParseXmlForCatalogTitles(data);

                            //find movie with same webpage as the title
                            Movie movie;
                            foreach (Movie movie_model in movieDbList)
                            {

                                if (movie_model.web_page == titles[0].LinkToPage)
                                {
                                    movie = movie_model;
                                    movieDbList.Remove(movie_model);
                                    dictOfMoviesTitles[movie] = titles[0];
                                    break;
                                }

                            }
                            //Movie movie =
                            //    NextFlicksMVC4.Create.CreateMovie(titles[0]);

                            //add to DB and dict
                            //listOfMovies.Add(movie);

                            //db.Movies.Add(movie);


                            //log adding data
                            msg = String.Format("Added item {0} to dict, moving to next one", count.ToString());
                            Trace.WriteLine(msg);
                            count += 1;

                        }
                        data = reader.ReadLine();
                    }

                    //save the movies added to db
                    Trace.WriteLine("done making dict");
                    //db.SaveChanges();

                }

                catch (System.Xml.XmlException ex)
                {
                    Trace.WriteLine("Done parsing the XML because of something happened. Probably the end of file:");
                    Trace.WriteLine(ex.Message);
                }


                Trace.WriteLine("Adding Boxart and Genre");
                //add boxart and genre data to db before saving the movie 
                AddBoxartsAndMovieToGenreData(dictOfMoviesTitles, db);

                //Trace.WriteLine("Beginning Add to DB");

                //set up checkpoints for progress updates
                //int modulo =0;
                //List<int> checkpoints = new List<int>();
                //int total = listOfMovies.Count;
                //int start = total/25;
                //if (start == 0)
                //{
                //    start = 1;
                //}
                //while (modulo <= listOfMovies.Count)
                //{
                //   checkpoints.Add(modulo);
                //    modulo += start;
                //}

                //go through list of movies and add to database
                //int counter = 0;
                //if (listOfMovies.Count > 0)
                //{
                //    foreach (Movie movie in listOfMovies)
                //    {
                //        //db.Movies.Add(movie);


                //        //counting stuff, not logic essential
                //        counter += 1;
                //        if (checkpoints.Contains(counter))
                //        {
                //            string msg =
                //                String.Format(
                //                    "Done adding at least {0} movies", counter);
                //            Trace.WriteLine(msg);
                //        }
                //    }

                Trace.WriteLine("Saving Changes any untracked ones, anyways");
                db.SaveChanges();
                Trace.WriteLine("Done Saving! Check out Movies/index for a table of the stuff");

                //}
            }


            Trace.WriteLine("Done everything");
            msg = DateTime.Now.ToShortTimeString();
            Trace.WriteLine(msg);
            var end_time = DateTime.Now;

            TimeSpan span = end_time - start_time;
            Trace.WriteLine("It took this long:");
            Trace.WriteLine(span);

            return View();
        }

        public ActionResult Full()
        {

            Trace.WriteLine("starting Full Action");
            string msg = DateTime.Now.ToShortTimeString();
            var start_time = DateTime.Now;
            Trace.WriteLine(msg);
            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;
            //------------------------------------------------------

            //need to have a Genre table first, so make sure that's there
            //int genre_count = db.Genres.Count();
            //if (genre_count == 0)
            //{

            //}

            PopulateGenres.PopulateGenresTable();


            //------------------------------------------------------



            Trace.WriteLine("starting data read");
            msg = DateTime.Now.ToShortTimeString();
            Trace.WriteLine(msg);

            // Go line by line, and parse it for Movie files
            //List<Movie> listOfMovies = new List<Movie>();
            Dictionary<Movie, Title> dictOfMoviesTitles = new Dictionary<Movie, Title>();
            string data;
            int count = 0;
            using (StreamReader reader = new StreamReader(@"C:\fixedAPI.NFPOX"))
            {

                Trace.WriteLine("Starting to read");

                data = reader.ReadLine();
                try
                {
                    while (data != null)
                    {
                        if (!data.StartsWith("<catalog_title>"))
                        {
                            Trace.WriteLine("Invalid line of XML, probably CDATA or something");
                        }
                        else
                        {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                NextFlicksMVC4.Create.ParseXmlForCatalogTitles(data);
                            Movie movie =
                                NextFlicksMVC4.Create.CreateMovie(titles[0]);

                            //add to DB and dict
                            //listOfMovies.Add(movie);
                            dictOfMoviesTitles[movie] = titles[0];
                            db.Movies.Add(movie);


                            //log adding data
                            msg = String.Format("Added item {0} to database, moving to next one", count.ToString());
                            Trace.WriteLine(msg);
                            count += 1;

                        }
                        data = reader.ReadLine();
                    }

                    //save the movies added to db
                    Trace.WriteLine("Saving Movies");
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;

                }

                catch (System.Xml.XmlException ex)
                {
                    Trace.WriteLine("Done parsing the XML because of something happened. Probably the end of file:");
                    Trace.WriteLine(ex.Message);
                }

                db.SaveChanges();
                Trace.WriteLine("Adding Boxart and Genre");
                //add boxart and genre data to db before saving the movie 
                AddBoxartsAndMovieToGenreData(dictOfMoviesTitles, db);

                //Trace.WriteLine("Beginning Add to DB");

                //set up checkpoints for progress updates
                //int modulo =0;
                //List<int> checkpoints = new List<int>();
                //int total = listOfMovies.Count;
                //int start = total/25;
                //if (start == 0)
                //{
                //    start = 1;
                //}
                //while (modulo <= listOfMovies.Count)
                //{
                //   checkpoints.Add(modulo);
                //    modulo += start;
                //}

                //go through list of movies and add to database
                //int counter = 0;
                //if (listOfMovies.Count > 0)
                //{
                //    foreach (Movie movie in listOfMovies)
                //    {
                //        //db.Movies.Add(movie);


                //        //counting stuff, not logic essential
                //        counter += 1;
                //        if (checkpoints.Contains(counter))
                //        {
                //            string msg =
                //                String.Format(
                //                    "Done adding at least {0} movies", counter);
                //            Trace.WriteLine(msg);
                //        }
                //    }

                Trace.WriteLine("Saving Changes any untracked ones, anyways");
                db.SaveChanges();
                Trace.WriteLine("Done Saving! Check out Movies/index for a table of the stuff");

                //}
            }


            Trace.WriteLine("Done everything");
            msg = DateTime.Now.ToShortTimeString();
            Trace.WriteLine(msg);
            var end_time = DateTime.Now;

            TimeSpan span = end_time - start_time;
            Trace.WriteLine("It took this long:");
            Trace.WriteLine(span);

            return View();
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
                BoxArt boxArt = NextFlicksMVC4.Create.CreateMovieBoxartFromTitle(movie,
                                                                                 title);
                db.BoxArts.Add(boxArt);


                //genres to database
                foreach (Genre genre in title.ListGenres)
                {
                    MovieToGenre movieToGenre =
                        NextFlicksMVC4.Create.CreateMovieMovieToGenre(movie,
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

        public ActionResult API(string term = "Jim Carrey")
        {
            MovieDbContext db = new MovieDbContext();
            //grab new movies, turn one into a Movie and view it
            var data = OAuth1a.GetNextflixCatalogDataString("catalog/titles/streaming", term, max_results: "100", outputPath: @"C:/streamingAPI2.NFPOX");
            var titles =
                NextFlicksMVC4.Create.ParseXmlForCatalogTitles(data);

            List<Movie> movies = new List<Movie>();

            foreach (Title title in titles)
            {
                Movie movie = NextFlicksMVC4.Create.CreateMovie(title);
                movies.Add(movie);
                db.Movies.Add(movie);

                BoxArt boxArt = NextFlicksMVC4.Create.CreateMovieBoxartFromTitle(movie,
                                                                        title);
                db.BoxArts.Add(boxArt);
                foreach (Genre genre in title.ListGenres)
                {
                    MovieToGenre movieToGenre =
                        NextFlicksMVC4.Create.CreateMovieMovieToGenre(movie,
                                                                      genre);
                    db.MovieToGenres.Add(movieToGenre);

                }
            }

            db.SaveChanges();

            return View(movies.ToList());
        }

        //
        // GET: /Movies/Details/5

        public ActionResult Details(int movie_ID = 0)
        {
            MovieDbContext db = new MovieDbContext();

            Movie movie = db.Movies.Find(movie_ID);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        //
        // GET: /Movies/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Movies/Create

        [HttpPost]
        public ActionResult Create(Movie movie)
        {
            MovieDbContext db = new MovieDbContext();
            if (ModelState.IsValid)
            {
                db.Movies.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        //
        // GET: /Movies/Edit/5

        public ActionResult Edit(int id = 0)
        {
            MovieDbContext db = new MovieDbContext();
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        //
        // POST: /Movies/Edit/5

        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            MovieDbContext db = new MovieDbContext();
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        //
        // GET: /Movies/Delete/5

        public ActionResult Delete(int id = 0)
        {
            MovieDbContext db = new MovieDbContext();
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        //
        // POST: /Movies/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            MovieDbContext db = new MovieDbContext();
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            MovieDbContext db = new MovieDbContext();
            db.Dispose();
            base.Dispose(disposing);
        }
        

        ///TODO: Move these methods to a new file, since they're not a controller
        public static string GetParamName(System.Reflection.MethodInfo method, int index)
        {
            string retVal = string.Empty;

            if (method != null && method.GetParameters().Length > index)
                retVal = method.GetParameters()[index].Name;


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
    }
}
