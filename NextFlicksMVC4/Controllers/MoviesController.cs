using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using NextFlicksMVC4;
using NextFlicksMVC4.DatabaseClasses;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;
using NextFlicksMVC4.NetFlixAPI;
using System.Timers;
using NextFlicksMVC4.Helpers;
using System.Data.SqlClient;
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;
using LumenWorks.Framework.IO.Csv;
using ProtoBuf;
using WebMatrix.WebData;
using Ionic.Zip;

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
            if (Request.Cookies.Get(cookie_name) != null) {
                ViewBag.cookies = true;
            }

            return View();
        }

        //Expires a cookie
        public ActionResult Jar()
        {
            var cookie_name = "TestCookie";

            //if cookie exists, remove it
            if (Request.Cookies.AllKeys.Contains(cookie_name)) {
                //get the cookie from the request, expire the time so it gets 
                // deleted
                var cookie = Request.Cookies.Get(cookie_name);
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);

                ViewBag.cookies = false;

                Trace.WriteLine("Cookie removed");
            }




            else {
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
            return RedirectToAction("oldIndex",
                                    new {count = 1, start = rand_title_int});
        }


        //[TrackingActionFilter]
        //public ActionResult oldTest(string title = "",
        //                         int year_start = 1914,
        //                         int year_end = 2012,
        //                         int mpaa_start = 0,
        //                         int mpaa_end = 200,

        //                         bool? is_movie = null,

        //                         int runtime_start = 0,
        //                         int runtime_end = 9999999,
        //                         string genre = "",
        //                         int start = 0,
        //                         int count = 25,
        //                         string sort = "movie_ID")
        //{
        //    ViewBag.Params = Tools.GetAllParamNames("Test");


        //    MovieDbContext db = new MovieDbContext();



        //    var movie_list = db.Movies.ToList();

        //    var nitlist = Tools.FilterMovies(db, movie_list,
        //                                     title: title, is_movie: is_movie,
        //                                     year_start: year_start,
        //                                     year_end: year_end,
        //                                     mpaa_start: mpaa_start,
        //                                     mpaa_end: mpaa_end,
        //                                     runtime_start: runtime_start,
        //                                     runtime_end: runtime_end,
        //                                     genre: genre,
        //                                     start: start, count: count,
        //                                     sort: sort);


        //    ViewBag.Start = start;
        //    ViewBag.Count = count;



        //    return View("Genres", nitlist);
        //}

        //public ActionResult sort()
        //{

        //    MovieDbContext db = new MovieDbContext();

        //    int count = 25;

        //    var start = Tools.WriteTimeStamp("\n*** starting /sql ***");

        //    //returns a IQueryable populated with all the entries in the movies/etc 
        //    var nitvmQuery = Tools.GetFullDbQuery(db);


        //    Tools.TraceLine("Ordering the movies and taking {0}", count);
        //    //array instead list for performance
        //    var nitvmArray =
        //        nitvmQuery.OrderBy(item => item.OmdbEntry.t_Meter)
        //                  .Take(count)
        //                  .ToArray();


        //    var done = Tools.WriteTimeStamp("done at");
        //    Tools.TraceLine("took: {0}", done - start);
        //    //Tools.TraceLine("amount of results {0}", res.Count);


        //    return View();

        //}


        public ActionResult Index(string movie_title = "", string genre_select = "0", int page = 1)
        {


            var start = Tools.WriteTimeStamp("start");

            //if the titles are default print default message, otherwise print variables
            if (movie_title != "" || genre_select != "0")
            {
                Tools.TraceLine("testsorting with title: {0}, genre: {1}", movie_title, genre_select);
            }
            else { Tools.TraceLine("testsorting with blank title and genre");}

            int movie_count = 28;

            var db = new MovieDbContext();

            //create a Dict<string,int> for all genre_string and ids, so that they 
            // can be enumerated in the filtermenu. So the user can select which genres 
            // they want to look for
            var dict_start = Tools.WriteTimeStamp("  Start dict make");
            Dictionary<string, int> genre_dict =
                db.Genres.Distinct().ToDictionary(gen => gen.genre_string,
                                       gen => gen.genre_ID);
            //sort the dictionary, automatically does it by key, seems like
            SortedDictionary<string, int> sortedDictionary = new SortedDictionary<string, int>(genre_dict);
            var dict_end = Tools.WriteTimeStamp("  End dict make");
            Tools.TraceLine("dict took {0}", dict_end - dict_start);

            //Assign it to a ViewBag, so the Filtermenu can use it
            ViewBag.genre_dict = sortedDictionary;
            MultiSelectList msl = new MultiSelectList(ViewBag.genre_dict);
            ViewBag.msl = msl;


            //make sure the title isn't the default text set in the _FilterMenu
            if (movie_title == "Enter a title") {
                movie_title = "";
            }


            //TODO:create a FilterMenuInit() so I can just call this everytime. It'll be easier on us all


            //get a full query with all data in db
            var total_qry = Tools.GetFullDbQuery(db);

            //filters the movie quickly enough
            // basic stuff, hard coded for now, except for the title
            var res =
                from nit in total_qry
                where
                    //title
                nit.Movie.short_title.Contains(movie_title)
                    //runtime
                && nit.Movie.runtime > 0
                && nit.Movie.runtime < 100000
                    //year
                && nit.Movie.year >= 0
                && nit.Movie.year <= 3000
                    //maturity rating
                && nit.Movie.maturity_rating >= 0
                && nit.Movie.maturity_rating <= 200
                    //genre 


                select nit ;
                    
            //if the genre isn't default, filter more
            if (genre_select != "0") {
                res = res.Where(nit => nit.Genres.Any(item => item == genre_select));
            }
                //&& nit.Genres.Any(item => item == genre_select)

                      ////Rotten Tomatoes Meter
                      //&& nit.OmdbEntry.t_Meter >= 0
                      //&& nit.OmdbEntry.t_Meter <= 200

                      ////Rotten Tomatoes Fresh
                      //&& nit.OmdbEntry.t_Fresh >= 0
                      //&& nit.OmdbEntry.t_Fresh <= 200000

                      //Rotten Tomatoes Rotten
                      //&& nit.OmdbEntry.t_Rotten >= 0
                      //&& nit.OmdbEntry.t_Rotten <= 200000


            //sometimes the first call to the db times out. I can't reliably repro it, so I've just created a try catch for it.
            try {

                Tools.TraceLine(" Counting all possible results, before pagination");
                var count_start = Tools.WriteTimeStamp("  count start");
                //count all the movies possible
                int totalMovies = res.Count();
                //set it to the viewbag so the view can display it
                ViewBag.TotalMovies = totalMovies;
                Tools.TraceLine("  total possible results {0}", totalMovies);
                var count_end = Tools.WriteTimeStamp("count_start end");
                Tools.TraceLine("  counting took {0}", count_end - count_start);


                var page_start = Tools.WriteTimeStamp();

                //limit the amount of movies per page, and then multiply it by the current page
                //page 1 = 0-27, then 28- 55 or something. Math's not my forte
                Tools.TraceLine("  Retrieving paginated results");

                int movies_to_skip = movie_count*(page-1);

                //needed to sort the stuff before I could skip, so I chose alphabetically, then changed to ID for a bit of speed
                // it can be changed at any time, once we get some feedback.
                //IEnumerable<FullViewModel> nit_list =
                //    res.OrderBy(nit => nit.Movie.movie_ID)
                //       .Skip(movies_to_skip)
                //       .Take(movie_count)
                //       .ToArray();


                ///the Orderby didn't work,so I changed tactics a bit. Hacky as fuck
                //to avoid out of index errors, limit the range chosen. A limitation of doing it with lists, over Linq
                if (totalMovies < movie_count) {
                    movie_count = totalMovies;
                }
                
                //take all the pages up to and including the ones you'll show 
                // on page, then only take the last set of movies you'll show
                IEnumerable<FullViewModel> nit_list =
                    res.Take(movies_to_skip + movie_count)
                       .ToList().GetRange(movies_to_skip, movie_count);

                var page_end = Tools.WriteTimeStamp();
                Tools.TraceLine("  Taking first page of movies {0}", (page_end - page_start).ToString());



                var end = Tools.WriteTimeStamp("end");
                Tools.TraceLine((end - start).ToString());

                return View("Results", nit_list);
            }

            catch (System.Data.EntityCommandExecutionException ex) {
                Tools.TraceLine(
                    "The ToArray() call probably timed out, it happens on first call to db a lot, I don't know why:\n  ***{0}",
                    ex.GetBaseException().Message);

                return View("Error");
            }



        }


        public ActionResult DetailsNit()
        {
            //create a VM
            MovieDbContext movieDb = new MovieDbContext();
            List<Movie> movie_list = movieDb.Movies.Take(1).ToList();

            var MwG_list = Tools.FilterMovies(movieDb, movie_list);
            //FullViewModel NitVm = Omdb.MatchListOfMwgvmWithOmdbEntrys(MwG_list, movieDb).First();

            var NitVm = MwG_list.First();

            return View(NitVm);
        }

        
        //public ActionResult oldYear(int year_start = 2001, int year_end = 2002, int start = 0, int count = 25, bool is_movie = true)
        //{
        //    MovieDbContext db = new MovieDbContext();
        //    //returns all titles from year
        //    string qry = "select * from Movies where (year between {0} and {1}) and (is_movie = {2}) order by year";
        //    var res = db.Movies.SqlQuery(qry, year_start, year_end, is_movie);

        //    List<Movie> movie_list = res.ToList();

        //    ViewBag.TotalMovies = movie_list.Count - 1;
        //    ViewBag.year_start = year_start;
        //    ViewBag.year_end = year_end;
        //    ViewBag.start = start;
        //    ViewBag.count = count;

        //    if (count > movie_list.Count)
        //    {
        //        count = movie_list.Count - 1;
        //    }
        //    var results = movie_list.GetRange(start, start + count);
        //    return View("Index", results);
        //}


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

        //public ActionResult oldIndex(int start = 0, int count = 10)
        //{
        //    MovieDbContext db = new MovieDbContext();

        //    //create a query string to full the proper count of movies from db
        //    Trace.WriteLine("Creating a query string");

        //    //select a range of items, with linq rather than with query
        //    var fullList = db.Movies.OrderBy(item => item.movie_ID).Skip(start).Take(count).ToList();

        //    //count the total movies in DB
        //    Trace.WriteLine("Counting movies");
        //    string count_qry = "select count(movie_id) from Movies";
        //    var count_res = db.Database.SqlQuery<int>(count_qry);
        //    int count_count = count_res.ElementAt(0);
        //    ViewBag.TotalMovies = count_count;

        //    //misc numbers
        //    ViewBag.Start = start;
        //    ViewBag.Count = count;

        //    //Param names
        //    Trace.WriteLine("getting param names");
        //    ViewBag.Params = Tools.GetAllParamNames("Index");

        //    //make sure there's not a outofbounds
        //    if (count > fullList.Count)
        //    {
        //        count = fullList.Count;
        //        Trace.WriteLine("had to shorten the returned results");
        //    }

        //    Trace.WriteLine("Get ranging");
        //    var full_range = fullList.GetRange(0, count);


        //    //turn all the movies into MovieWithGenresViewModel
        //    var MwG_list = ModelBuilder.CreateListOfMwGVM(db, full_range);

        //    IEnumerable<MovieWithGenreViewModel> MwG_ienum = MwG_list;

        //    Trace.WriteLine("Returning View");
        //    return View("Genres", MwG_ienum);
        //}


        public ActionResult Full()
        {
            Tools.TraceLine("In Full");
            //create a genres table in the DB
            PopulateGenres.PopulateGenresTable();


            Tools.BuildMoviesBoxartGenresTables(@"C:\fixedAPI.NFPOX");

            Tools.TraceLine("Out Full");

            return View();
        }

        public ActionResult Api(string term = "Jim Carrey")
        {
            MovieDbContext db = new MovieDbContext();
            //grab new movies, turn one into a Movie and view it
            var data =
                OAuth1a.GetNextflixCatalogDataString(
                    "catalog/titles/streaming", term, max_results: "100",
                    outputPath: @"C:/testtesttest.NFPOX");
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

     /*   public ActionResult DetailsTag(int movie_ID = 0)
        {
            MovieDbContext db = new MovieDbContext();

            Movie movie = db.Movies.Find(movie_ID);
            if (movie == null)
            {
                return HttpNotFound();
            }

            TagViewModel tagViewModel = new TagViewModel();
            tagViewModel.movie = movie;
            tagViewModel.genre_strings = new List<string>();
           // TagViewModel.Tags = new List<String>();
            tagViewModel.TagAndCount = new Dictionary<String,int>();
            tagViewModel.Anon = false;
           

            var movieGenres = from mtg in db.MovieToGenres
                              join genre in db.Genres
                                  on mtg.genre_ID equals genre.genre_ID
                              group genre.genre_string by mtg.movie_ID
                                  into gs
                                  where gs.Key == movie_ID
                                  select gs;
            tagViewModel.genre_strings = movieGenres.First().ToList();

            var tagsForMovie = from MtT in db.UserToMovieToTags
                            where MtT.movie_ID == movie_ID
                            select MtT.TagId;
            var tagStrings = from tag in db.MovieTags
                             from tag_id in tagsForMovie
                             where tag.TagId == tag_id
                             select tag.Name;

            //if i actually don't set this part of the model and only use the part where i send
            // the count over with the tag as the key will that work?
            var movieTags = tagStrings.ToList();

            foreach (string tag in movieTags)
            {
                //select * from MtUtT where MtUtT.tag.id == TagsForMovie.tagID
                //select * from 
                var tagCount = from tagString in db.MovieTags
                               where tagString.Name == tag
                               from MtT in db.UserToMovieToTags
                               where MtT.TagId == tagString.TagId
                               select MtT.TagId;
                               
                if (!tagViewModel.TagAndCount.ContainsKey(tag))
                {
                    tagViewModel.TagAndCount.Add(tag, tagCount.Count());
                }
            }
            
            foreach (KeyValuePair<string, int> kvp in tagViewModel.TagAndCount)
            {
                Tools.TraceLine(kvp.ToString(), kvp.Value.ToString());
            }
           
            return View(tagViewModel); 
        }*/

        [HttpPost]
        public ActionResult DetailsTag(int movie_ID, List<string> tags, bool Anon)
        {
            if (ModelState.IsValid)
            {
                MovieDbContext db = new MovieDbContext();
                Movie taggedMovie = db.Movies.Find(movie_ID);
                MovieTag newTag = new MovieTag();

                //break the tags down by comma delimeter
                List<String> seperatedtags = UserInput.DeliminateStrings(tags);
                seperatedtags = UserInput.StripWhiteSpace(seperatedtags);
                seperatedtags = UserInput.SanitizeSpecialCharacters(seperatedtags);
                
                foreach (string tag in seperatedtags)
                {
                    var tagExists = db.MovieTags.First(t => t.Name == tag);
                    if (tagExists == null)
                    {
                        //tag doesn't exist, so create it
                        newTag = new MovieTag();
                        newTag.Name = tag;
                        db.MovieTags.Add(newTag);
                        db.SaveChanges();
                    }
                    else
                    {
                        //otherwise slecte the MovieTag where the names match and use that.
                        newTag = db.MovieTags.First(t => t.Name == tag);
                    }

                    UserToMovieToTags UtMtT = new UserToMovieToTags();
                    UtMtT.TagId = newTag.TagId;
                    UtMtT.UserID = WebSecurity.CurrentUserId;
                    UtMtT.movie_ID = movie_ID;

                    db.UserToMovieToTags.Add(UtMtT);
                    db.SaveChanges();

                    UtMtTisAnon anon = new UtMtTisAnon();
                    anon.UtMtT_ID = UtMtT.UtMtY_ID;
                    anon.IsAnon = Anon;

                    db.UtMtTisAnon.Add(anon);
                    db.SaveChanges();
                    /* MovieTag newTag = new MovieTag();
                    {

                        Tag = tag;
                         movie_ID = taggedMovie.movie_ID,
                        userID = WebSecurity.CurrentUserId
                    }; 
                    db.Tags.Add(newTag);
                    db.SaveChanges(); */
                }
                return RedirectToAction("DetailsTag", "Movies", movie_ID);
            }
            return RedirectToAction("DetailsTag", "Movies", movie_ID); 
        }

        public ActionResult Details(int movie_ID = 0)
        {
            MovieDbContext db = new MovieDbContext();
            FullViewModel fullView = new FullViewModel();

            Movie movie = db.Movies.Find(movie_ID);
            if (movie == null)
            {
                return HttpNotFound();
            }

            fullView.Movie = movie;
            fullView.Genres = PopulateFullView.Genres(movie_ID);
            fullView.Boxarts = PopulateFullView.BoxArts(movie_ID);
            fullView.OmdbEntry = PopulateFullView.Omdb(movie_ID);
            fullView.Tags = PopulateFullView.TagsAndCount(movie_ID);
            //added the plot to the fullView too, needs the whole movie, for the title and year
            fullView.Plot = PopulateFullView.Plot(movie);




            return View("Details2", fullView);
        }

        [HttpPost]
        public ActionResult Details(int movie_ID, List<String> tags, bool anon)
        {

            if (ModelState.IsValid)
            {
                MovieDbContext db = new MovieDbContext();
                Movie taggedMovie = db.Movies.Find(movie_ID);
                MovieTag newTag = new MovieTag();

                //break the tags down by comma delimeter
                List<String> seperatedtags = UserInput.DeliminateStrings(tags);
                seperatedtags = UserInput.StripWhiteSpace(seperatedtags);
                seperatedtags = UserInput.SanitizeSpecialCharacters(seperatedtags);

                foreach (string tag in seperatedtags)
                {
                    var tagExists = db.MovieTags.FirstOrDefault(t => t.Name == tag);
                    if (tagExists == null)
                    {
                        //tag doesn't exist, so create it
                        newTag.Name = tag;
                        db.MovieTags.Add(newTag);
                        db.SaveChanges();
                    }
                    else
                    {
                        //otherwise slecte the MovieTag where the names match and use that.
                        newTag = db.MovieTags.First(t => t.Name == tag);
                    }

                    UserToMovieToTags UtMtT = new UserToMovieToTags();
                    UtMtT.TagId = newTag.TagId;
                    UtMtT.UserID = WebSecurity.CurrentUserId;
                    UtMtT.movie_ID = movie_ID;

                    db.UserToMovieToTags.Add(UtMtT);
                    db.SaveChanges();

                    UtMtTisAnon anonTags = new UtMtTisAnon();
                    anonTags.UtMtT_ID = UtMtT.UtMtY_ID;
                    anonTags.IsAnon = anon;

                    db.UtMtTisAnon.Add(anonTags);
                    db.SaveChanges();
                  }
                return RedirectToAction("DetailsTag", "Movies", movie_ID);
                //return View(fullView);
            }
            return RedirectToAction("DetailsTag", "Movies", movie_ID); 
            //return View(fullView);
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
            string entryDumpPath = @"omdbASD.DUMP";
            string imdbPath = @"omdb.txt";
            string tomPath =
                @"tomatoes.txt";

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
                return RedirectToAction("oldIndex");
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
                return RedirectToAction("oldIndex");
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
            return RedirectToAction("oldIndex");
        }

        protected override void Dispose(bool disposing)
        {
            MovieDbContext db = new MovieDbContext();
            db.Dispose();
            base.Dispose(disposing);
        }

        ////sort movies by t_meter
        //public ActionResult Sort()
        //{

        //    DateTime start = DateTime.Now;
        //    MovieDbContext db = new MovieDbContext();

        //    ////has never worked, but shows the idea
        //    //var result_list = db.Database.SqlQuery<FullViewModel>(
        //    //"SELECT        Movies.short_title, OmdbEntries.t_Meter" +
        //    //"FROM            OmdbEntries INNER JOIN" +
        //    //"Movies ON OmdbEntries.movie_ID = Movies.movie_ID" +
        //    //"WHERE        (OmdbEntries.t_Meter <> 'n/a')" +
        //    //"ORDER BY OmdbEntries.t_Meter DESC");

        //    var result_list = db.Omdb.Where(omdb => omdb.movie_ID != 0).OrderByDescending(item => item.t_Meter);

        //    var qwe = result_list.ToList();

        //    Tools.TraceLine("count: {0}", qwe.Count);

        //    Tools.TraceLine("Top t_meter movie: {0}", qwe.First().title);

        //    DateTime end = DateTime.Now;

        //    var span = end - start;

        //    Tools.TraceLine("Took {0} seconds", span.TotalSeconds);

        //    return View();
        //}

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
