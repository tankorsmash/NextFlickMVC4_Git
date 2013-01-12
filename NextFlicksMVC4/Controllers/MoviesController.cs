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
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;
using LumenWorks.Framework.IO.Csv;
using ProtoBuf;

namespace NextFlicksMVC4.Controllers
{
    public class MoviesController : Controller
    {

        //Creates a cookie
        public ActionResult Cookies()
        {
            //create a cookie
            var cookie_name = "TestCookie";
            HttpCookie cookie = new HttpCookie(cookie_name);
            cookie.Value = "Test Cookie Value";

            //add the cookie
            Response.Cookies.Add(cookie);
            Trace.WriteLine("Cookie Added");

            //test for the cookie creation, change ViewBag
            if (Request.Cookies.Get(cookie_name) != null)
            { ViewBag.cookies = true; }

            return View();
        }

        //Expires a cookie
        public ActionResult Jar()
        {
            var cookie_name = "TestCookie";

            //if cookie exists, remove it
            if (Request.Cookies.AllKeys.Contains(cookie_name))
            {
                //get the cookie from the request, expire the time so it gets 
                // deleted
                var cookie = Request.Cookies.Get(cookie_name);
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);

                ViewBag.cookies = false;

                Trace.WriteLine("Cookie removed");
            }




            else
            {
                Trace.WriteLine("Cookie didn't exist, no action");
                ViewBag.cookies = true;
            }

            return View("Cookies");
        }


        //[HttpGet]
        //public ActionResult Filter()
        //{
        //    MovieDbContext db = new MovieDbContext();

        //    //grab the lowest year in Catalog
        //    var min_qry = "select  top(1) * from Movies where year != \'\' order by year ASC";
        //    var min_res = db.Movies.SqlQuery(min_qry);
        //    var min_list = min_res.ToList();
        //    string min_year = min_list[0].year;
        //    ViewBag.min_year = min_year;
        //    //grab the highest year in catalog
        //    var max_qry = "select  top(1) * from Movies order by year DESC ";
        //    var max_res = db.Movies.SqlQuery(max_qry);
        //    var max_list = max_res.ToList();
        //    string max_year = max_list[0].year;
        //    ViewBag.max_year = max_year;

        //    //Create a list of SelectListItems
        //    List<SelectListItem> years = new List<SelectListItem>();
        //    for (int i = Int32.Parse(min_year); i <= Int32.Parse(max_year); i++)
        //    {
        //        years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        //    }

        //    //Create a selectList, but since it's looking for an IEnumerable,
        //    // tell it what is the value and text parts
        //    SelectList slist = new SelectList(years, "Value", "Text");

        //    //give the Viewbag a property for the SelectList
        //    ViewBag.years = slist;

        //    return View();
        //}



        //[Obsolete("shouldn't be needed anymore", true)]
        //public ActionResult FilterHandler(string year_start, string year_end)
        //{
        //    ViewData["year_start"] = year_start;
        //    ViewData["year_end"] = year_end;
        //    Trace.WriteLine(ViewData["pet"]);

        //    return View("FilterHandler");
        //}

        /// <summary>
        /// Sloppy return random set of movies. redirects to Index with a random start and count of 10
        /// </summary>
        /// <returns></returns>
        public ActionResult Random()
        {
            MovieDbContext db = new MovieDbContext();

            int rand_title_int = new Random().Next(1, db.Movies.Count());
            return RedirectToAction("Index", new { count = 1, start = rand_title_int });
        }


        [TrackingActionFilter]
        public ActionResult Test(string title = "",
            int year_start = 1914, int year_end = 2012,
            int mpaa_start = 0, int mpaa_end = 200,

            bool? is_movie = null,

            int runtime_start = 0, int runtime_end = 9999999,
            string genre = "",
            int start = 0, int count = 25)
        {
            ViewBag.Params = Tools.GetAllParamNames("Test");

            //OMBD.Omdb.GetOmbdbTitleInfo("The Terminator", "1984");
            //OMBD.Omdb.GetOmdbEntryForMovie("The Terminator", "1984");

            MovieDbContext db = new MovieDbContext();

            //var movie_list = db.Movies.Take(100).Take(10).ToList();
            //var movie_list = db.Movies.Where(item => item.year == year).ToList();
            //List<MovieWithGenreViewModel> MwG_list = ModelBuilder.CreateListOfMwGVM(db, movie_list);
            //MwG_list = MwG_list.Where(item => item.movie.runtime.TotalSeconds == 5767).ToList();

            //Tools.TraceLine("TITS!");
            //throw  new Exception();

            var movie_list = db.Movies.ToList();

            var MwG_list = Tools.FilterMovies(db, movie_list,
            title: title, is_movie: is_movie,
            year_start: year_start, year_end: year_end,
            mpaa_start: mpaa_start, mpaa_end: mpaa_end,
            runtime_start: runtime_start, runtime_end: runtime_end,
            genre: genre,
            start: start, count: count);
            //start:start,
            //count:count,
            ////title: "kid");
            //genre: genre);

            ViewBag.Start = start;
            ViewBag.Count = count;

            //var completeVm_list = MatchListOfMwgvmWithOmdbEntrys(MwG_list, db);


            IEnumerable<MovieWithGenreViewModel> MwG_ienum = MwG_list;

            Trace.WriteLine(@"Returning /Test View");
            return View("Genres", MwG_ienum);
            //return View();
        }



        public ActionResult DetailsNit()
        {
            //create a VM
            MovieDbContext movieDb = new MovieDbContext();
            List<Movie> movie_list = movieDb.Movies.Take(1).ToList();



            var MwG_list = Tools.FilterMovies(movieDb, movie_list);
            NfImdbRtViewModel NitVm = Omdb.MatchListOfMwgvmWithOmdbEntrys(MwG_list, movieDb).First();

            return View(NitVm);


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
        public ActionResult Genres(string genre = "action",
                                   int count = 25,
                                   int start = 0)
        {
            MovieDbContext db = new MovieDbContext();

            //make sure params are set, because "" is a valid parameter
            if (genre == "")
            {
                genre = "nothing";
            }

            //get a movie list that matches genres
            var movie_list = Tools.GetMoviesMatchingGenre(db, genre);
            //creates the MwGVM for the movie list
            var ranged_movie_list = movie_list.GetRange(start, count);
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db, ranged_movie_list);

            //to show a given view what the user searched for
            ViewBag.SearchTerms = genre;
            //relectively get the list of parameters for this method and pass them to the view
            ViewBag.Params = Tools.GetAllParamNames("Genres");

            //if the count param is higher than the amount of MwG's in the list,
            // make count the upper limit
            if (count > MwG_list.Count)
            {
                count = MwG_list.Count;
            }

            ViewBag.Count = count;
            ViewBag.Start = start;
            ViewBag.TotalMovies = movie_list.Count;

            //var ret = MwG_list.GetRange(start, count);
            return View(MwG_list);

        }

        //-------------------------------------------------------

        public ActionResult Index(int start = 0, int count = 10)
        {
            MovieDbContext db = new MovieDbContext();

            //create a query string to full the proper count of movies from db
            Trace.WriteLine("Creating a query string");

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
            ViewBag.Params = Tools.GetAllParamNames("Index");

            //make sure there's not a outofbounds
            if (count > fullList.Count)
            {
                count = fullList.Count;
                Trace.WriteLine("had to shorten the returned results");
            }

            Trace.WriteLine("Get ranging");
            var full_range = fullList.GetRange(0, count);


            //turn all the movies into MovieWithGenresViewModel
            var MwG_list = ModelBuilder.CreateListOfMwGVM(db, full_range);

            IEnumerable<MovieWithGenreViewModel> MwG_ienum = MwG_list;

            Trace.WriteLine("Returning View");
            return View("Genres", MwG_ienum);
        }


        public ActionResult Full()
        {

            Trace.WriteLine("starting Full Action");
            string msg = DateTime.Now.ToShortTimeString();
            var start_time = DateTime.Now;
            Trace.WriteLine(msg);
            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            //create a genres table in the DB
            PopulateGenres.PopulateGenresTable();

            Tools.WriteTimeStamp("starting data read");

            // Go line by line, and parse it for Movie files
            Dictionary<Movie, Title> dictOfMoviesTitles = new Dictionary<Movie, Title>();
            string data;
            int count = 0;
            using (StreamReader reader = new StreamReader(@"C:\testUS.NFPOX"))
            {

                Trace.WriteLine("Starting to read");

                data = reader.ReadLine();
                try
                {
                    while (data != null)
                    {
                        if (!data.StartsWith("<catalog_title>"))
                        {
                            Trace.WriteLine(
                                "Invalid line of XML, probably CDATA or something");
                        }
                        else
                        {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                NetFlixAPI.Create.ParseXmlForCatalogTitles(data);
                            Movie movie =
                                NetFlixAPI.Create.CreateMovie(titles[0]);

                            //add to DB and dict
                            //listOfMovies.Add(movie);
                            dictOfMoviesTitles[movie] = titles[0];
                            db.Movies.Add(movie);


                            //log adding data
                            msg =
                                String.Format(
                                    "Added item {0} to database, moving to next one",
                                    count.ToString());
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
                    Trace.WriteLine(
                        "Done parsing the XML because of something happened. Probably the end of file:");
                    Trace.WriteLine(ex.Message);
                }

                db.SaveChanges();
                Trace.WriteLine("Adding Boxart and Genre");
                //add boxart and genre data to db before saving the movie 
                Tools.AddBoxartsAndMovieToGenreData(dictOfMoviesTitles, db);


                Trace.WriteLine("Saving Changes any untracked ones");
                db.SaveChanges();
                Trace.WriteLine("Done Saving! Check out Movies/index for a table of the stuff");

            }


            var end_time = Tools.WriteTimeStamp("Done everything");

            TimeSpan span = end_time - start_time;
            Trace.WriteLine("It took this long:");
            Trace.WriteLine(span);

            return View();
        }

        public ActionResult Api(string term = "Jim Carrey")
        {
            MovieDbContext db = new MovieDbContext();
            //grab new movies, turn one into a Movie and view it
            var data =
                OAuth1a.GetNextflixCatalogDataString(
                    "catalog/titles/streaming", term, max_results: "100",
                    outputPath: @"C:/testUS.NFPOX");
            var titles =
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

        /// <summary>
        /// Rebuild the serialized list of OmdbEntrys that were created in Movies/TSV, and adds them to the database
        /// </summary>
        /// <returns></returns>
        public ActionResult Regen()
        {
            //rebuild the serialized list of List<omdbentryies>
            string entry_dump_path =
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksMVC4\OMBD\omdb.DUMP";

            //deserialize the list of omdbentries saved the the file
            //TODO: add check to make sure the file exists and is not corrupted
            List<OmdbEntry> complete_list;
            using (var file = System.IO.File.OpenRead(entry_dump_path))
            {
                complete_list = Serializer.Deserialize<List<OmdbEntry>>(file);
            }

            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            int count = complete_list.Count;
            foreach (OmdbEntry omdbEntry in complete_list)
            {
                db.Omdb.Add(omdbEntry);

                int remaining = count - complete_list.IndexOf(omdbEntry);
                Trace.WriteLine(remaining);
            }

            Trace.WriteLine("saving changes");
            db.Configuration.AutoDetectChangesEnabled = true;
            db.SaveChanges();


            Tools.WriteTimeStamp("Done saving changes");

            return View();

        }

        /// <summary>
        /// Reads the OMDB API data txts and dumps the list of OMBD Entrys to file, use Movies/Regen to rebuild them
        /// </summary>
        /// <returns></returns>
        public ActionResult TSV()
        {
            //TODO:Make these paths more general
            string entry_dump_path =
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksMVC4\OMBD\omdb.DUMP";
            string imdb_path =
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\omdb.txt";
            string tom_path =
                @"C:\Users\Mark\Documents\Visual Studio 2010\Projects\NextFlicksMVC4\NextFlickMVC4_Git\NextFlicksTextFolder\OMDB\tomatoes.txt";

            var complete_list_of_entries =
                TSVParse.ParseTSVforOmdbData(imdb_filepath: imdb_path,
                                             tom_filepath: tom_path);

            Tools.WriteTimeStamp("Starting to serialize list");
            using (var file = System.IO.File.Create(entry_dump_path))
            {
                Serializer.Serialize(file, complete_list_of_entries);
            }
            Tools.WriteTimeStamp("Done serializing list");


            return View();
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
            var db = new MovieDbContext();
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

        //sort movies by t_meter
        public ActionResult Sort()
        {

            DateTime start = DateTime.Now;
            MovieDbContext db = new MovieDbContext();

            ////has never worked, but shows the idea
            //var result_list = db.Database.SqlQuery<NfImdbRtViewModel>(
            //"SELECT        Movies.short_title, OmdbEntries.t_Meter" +
            //"FROM            OmdbEntries INNER JOIN" +
            //"Movies ON OmdbEntries.movie_ID = Movies.movie_ID" +
            //"WHERE        (OmdbEntries.t_Meter <> 'n/a')" +
            //"ORDER BY OmdbEntries.t_Meter DESC");

            var result_list = db.Omdb.Where(omdb => omdb.movie_ID != 0).OrderByDescending(item => item.t_Meter);

            var qwe = result_list.ToList();

            Tools.TraceLine("count: {0}", qwe.Count);

            Tools.TraceLine("Top t_meter movie: {0}", qwe.First().title);

            DateTime end = DateTime.Now;

            var span = end - start;

            Tools.TraceLine("Took {0} seconds", span.TotalSeconds);

            return View();
        }


        //from a movie.movie_ID, find the matching omdbEntry
        public OmdbEntry MatchMovieIdToOmdbEntry(int movie_ID)
        {
            MovieDbContext db = new MovieDbContext();
            var omdbEntry = db.Omdb.FirstOrDefault(omdb => omdb.movie_ID == movie_ID);

            return omdbEntry;
        }

        //loop over all the movies in Movies and find an omdb entry for it
        public ActionResult Merge()
        {

            MovieDbContext db = new MovieDbContext();

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

            Tools.TraceLine("Starting LINQ query");

            var query = from movie in db.Movies
                        join omdbEntry in db.Omdb on
                            new {name = movie.short_title, year = movie.year}
                            equals
                            new {name = omdbEntry.title, year = omdbEntry.year}
                        select new {movie = movie, omdbEntry = omdbEntry};

            Tools.TraceLine("Done LINQ query");
            Tools.TraceLine("turning results into a list");
            var resultList = query.ToList();
            Tools.TraceLine("done turning results into a list, found {0} movies", resultList.Count);

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



            return View();

        }
    }

}


