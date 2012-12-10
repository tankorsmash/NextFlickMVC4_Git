using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using System.Timers;
using NextFlicksMVC4.Helpers;
using System.Data.SqlClient;

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


        public ActionResult Test()
        {
            return View(@"~/Views/Home/About.cshtml");
        }

        public ActionResult Title()
        {
            Title title = new Title
                              {
                                  TitleString = "Terminator",
                                  ReleaseYear = "1995",
                              };

            return View(title);
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
        public ActionResult Genres(string genre_params = "action")
        {
            MovieDbContext db = new MovieDbContext();

            ////find the genre_id of genre
            //string gen_qry = "select genre_id from Genres where genre_string = {0}";
            //var gen_res = db.Database.SqlQuery<int>(gen_qry, genre);
            //var genre_id =  gen_res.ElementAt(0);

            ////select all movie_id that have a match for genre_id
            //string match_qry = "select movie_id from MovieToGenres where genre_id = {0}";
            //var match_res = db.Database.SqlQuery<List<int>>(match_qry, genre_id);
            //var match_list = match_res.ToList();

            ////select all movies from Movies where the movie_ids are found 


            ////select all movies that have the genre_id in them


            //select movies that match a genre_string
//            string qry = "" +
//"SELECT        Movies.* " +
//"FROM            Movies INNER JOIN " +
//"MovieToGenres ON Movies.movie_ID = MovieToGenres.movie_ID INNER JOIN " +
//"Genres ON MovieToGenres.genre_ID = Genres.genre_ID " +
//"WHERE        (Genres.genre_string LIKE N'{0}%')";

            ///Grab all the movies from the db
            //string qry =
            //    "SELECT Movies.* FROM  Movies INNER JOIN MovieToGenres ON Movies.movie_ID = MovieToGenres.movie_ID INNER JOIN Genres ON MovieToGenres.genre_ID = Genres.genre_ID WHERE (Genres.genre_string LIKE N'adventure') ";
            string qry;
            qry = @"
 SELECT distinct short_title, Movies.*
 FROM Movies INNER JOIN
 MovieToGenres ON Movies.movie_ID = MovieToGenres.movie_ID INNER JOIN Genres ON MovieToGenres.genre_ID = Genres.genre_ID 
 WHERE Genres.genre_string LIKE {0}+'%' ";
            var res = db.Movies.SqlQuery(qry, genre_params);
            var movie_list = res.ToList();

            //loop over Movies and create a MovieWithGenreViewModel for each
            var MwG_list = new List<MovieWithGenreViewModel>();
            foreach (Movie movie in movie_list) {
                MwG_list.Add(new MovieWithGenreViewModel {movie = movie,
                genre_strings = new List<string>()});
            }

            //Trace.WriteLine(res.to);

            ///Grab all the genres and movie_id
            string genres_qry;
            genres_qry = @"
 SELECT MovieToGenres.movie_ID, MovieToGenres.genre_ID, Genres.genre_string
 FROM Genres 
 INNER JOIN MovieToGenres ON Genres.genre_ID = MovieToGenres.genre_ID
 WHERE MovieToGenres.movie_ID in ({0}) ";
// WHERE Genres.genre_string LIKE {0}+'%' ";
            //List<int> movie_id_list = MwG_list.FindAll(item => item.movie.movie_ID)
            List<int> movie_id_list = movie_list.Select(movie => movie.movie_ID).ToList();
            string movie_id_list_string = String.Join(", ", movie_id_list.GetRange(0,10));
            var genres_res =
                db.Database.SqlQuery<MovieToGenreViewModel>(genres_qry);
            var genres_list = genres_res.ToList();

            ViewBag.Params = genre_params;

            ViewBag.Count = 0;
            ViewBag.Start = 0;
            ViewBag.TotalMovies = 0;




            //go through each genre and add it to the proper movie
            foreach (MovieToGenreViewModel movieToGenreViewModel in genres_list) {
                //LINQ FIND the movieWithgenre's movieid that matched the current mTOgenre movie_id and then
                // add the string to the found mWg instance
                MovieWithGenreViewModel movieWithGenreViewModel = MwG_list.Find(
                    item =>
                    item.movie.movie_ID == movieToGenreViewModel.movie_id);
                //add the genre_string to the movieWithGenre
                movieWithGenreViewModel.genre_strings.Add(movieToGenreViewModel.genre_string);
            }
            

            return View(MwG_list);

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
            public List<string> genre_strings { get; set; } 
        }

        public ActionResult Index(int start = 0, int count = 10)
        {
            MovieDbContext db = new MovieDbContext();

            //create a query string to full the proper count of movies from db
            Trace.WriteLine("Creating a query string");
            string qry = "select * from" +
                         " ( select " +
                         "  ROW_NUMBER() over (order by movie_id) as rownum," +
                         " *" +
                         " from Movies) foo" +
                         " where rownum  between {0} and {1}";
            var res = db.Movies.SqlQuery(qry, start, start + count);
            Trace.WriteLine("creating a list from the query");
            var fullList = res.ToList();

            //total movies in DB
            string count_qry = "select count(movie_id) from Movies";
            var count_res = db.Database.SqlQuery<int>(count_qry);
            int count_count = count_res.ElementAt(0);
            ViewBag.TotalMovies = count_count;

            //misc numbers
            ViewBag.Start = start;
            ViewBag.Count = count;

            //make sure there's not a outofbounds
            if (count > fullList.Count)
            {
                count = fullList.Count;
                Trace.WriteLine("had to shorten the returned results");
            }

            Trace.WriteLine("Get ranging");
            var full_range = fullList.GetRange(0, count);

            Trace.WriteLine("Returning View");
            return View(full_range);
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

        public static void AddBoxartsAndMovieToGenreData(Dictionary<Movie, Title> dictOfMoviesTitles, MovieDbContext db)
        {
            //get a list of ints that we can test against to save progress of adding to db
            List<int> checkpoints = ProgressList.CreateListOfCheckpointInts(dictOfMoviesTitles.Count, 55);
            
            //MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

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
                            "done saving MtG mtg_id = {0}\n movie_id = {1}\n genre_id = {2}",
                            movieToGenre.movie_to_genre_ID,
                            movieToGenre.movie_ID,
                            movieToGenre.genre_ID);

                    Trace.WriteLine(save_msg);
                }

                //if at a certain amount of adds, save changes, to avoid memory error hopefully
                if (checkpoints.Contains(index)) {
                    db.SaveChanges();

                    string msg =
                        String.Format("Just saved changes at checkpoint {0}",
                                      index);
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
    }
}
