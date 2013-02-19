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
       
        ////Creates a cookie
        //public ActionResult Cookies()
        //{
        //    //create a cookie
        //    var cookie_name = "TestCookie";
        //    HttpCookie cookie = new HttpCookie(cookie_name);
        //    cookie.Value = "Test Cookie Value";

        //    //add the cookie
        //    Response.Cookies.Add(cookie);
        //    Trace.WriteLine("Cookie Added");

        //    //test for the cookie creation, change ViewBag
        //    if (Request.Cookies.Get(cookie_name) != null) {
        //        ViewBag.cookies = true;
        //    }

        //    return View();
        //}

        ////Expires a cookie
        //public ActionResult Jar()
        //{
        //    var cookie_name = "TestCookie";

        //    //if cookie exists, remove it
        //    if (Request.Cookies.AllKeys.Contains(cookie_name)) {
        //        //get the cookie from the request, expire the time so it gets 
        //        // deleted
        //        var cookie = Request.Cookies.Get(cookie_name);
        //        cookie.Expires = DateTime.Now.AddDays(-1);
        //        Response.Cookies.Add(cookie);

        //        ViewBag.cookies = false;

        //        Trace.WriteLine("Cookie removed");
        //    }




        //    else {
        //        Trace.WriteLine("Cookie didn't exist, no action");
        //        ViewBag.cookies = true;
        //    }

        //    return View("Cookies");
        //}


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


        public ActionResult TagSearch(string term="scary")
        {
            MovieDbContext db = new MovieDbContext();
    
           //validate tag
            if (term == "") {
                term = "scary";
            }

            //get the tag id for the term
            var tag_res = from tag in db.MovieTags
                      where tag.Name == term
                      select tag.TagId;
            int tag_id = tag_res.FirstOrDefault();

            //find the movies that are tagged with tag_id
            var movie_res = from umt in db.UserToMovieToTags
                            where umt.TagId == tag_id
                            select umt.movie_ID;

            //tODO: build a FullView for each of the movies in the movie_ids list,
            // should almost be done, but I can't quite add any tags to the db to test
            var res_db = Tools.GetFullDbQuery(db);
            var res = from nit in res_db
                      from movie_id in movie_res
                      where nit.Movie.movie_ID == movie_id
                      select nit;
            IEnumerable<FullViewModel> matched_list = res.ToList();


            //Assign it to a ViewBag, so the Filtermenu can use it
            ViewBag.genre_dict = Tools.CreateSortedGenreDictionary(db);

            //Assign it to a ViewBag, so the Filtermenu can use it
            ViewBag.tag_dict = Tools.CreateSortedTagDictionary(db);

            //Count the total movies and add that to the Viewbag
            ViewBag.TotalMovies = matched_list.Count();

            return View("Results", matched_list);


        }

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




        /// <summary>
        /// Main action for this controller. Offers searching by title, genre and tag
        /// </summary>
        /// <param name="movie_title">movie title to look for</param>
        /// <param name="genre_select">selected genre to search for</param>
        /// <param name="tag_string">selected tag to search for</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Index(string movie_title = "", string genre_select = "0",
                                    string tag_string = "0",
                                    int page = 1)
        {


            int movie_count = 28;
            int movies_to_skip = movie_count * (page - 1);

            var db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = true;

            //make sure there's movies in the db
            if (db.Movies.Count() < 1) {
                Tools.TraceLine("ERROR: No movies in DB, have you ran Full yet?");
                return View("Error");
            }


            var start = Tools.WriteTimeStamp("start");

            //if the titles are default, print default message, otherwise print variables
            if (movie_title != "" || genre_select != "0") {
                Tools.TraceLine("Sorting with title: {0}, genre: {1}",
                                movie_title, genre_select);
            }
            else {
                Tools.TraceLine("Sorting with blank title and genre");
            }


            //TODO:create a FilterMenuInit() so I can just call this everytime. It'll be easier on us all
            //Assign it to a ViewBag, so the Filtermenu can use it
            ViewBag.genre_dict = Tools.CreateSortedGenreDictionary(db);
            ViewBag.tag_dict = Tools.CreateSortedTagDictionary(db);

            //make sure the title isn't the default text set in the _FilterMenu
            if (movie_title == "Enter a title") {
                movie_title = "";
            }
            if (tag_string == "Enter a tag") {
                tag_string = "0";
            }


            IQueryable<FullViewModel> res;
            //if the movie title isn't null, search movies
            if (movie_title != "") {
                res = Tools.FilterMoviesAndGenres(movie_title, db, genre_select);
            }
            //if the tag string isn't empty, then search through tags
            else if (tag_string != "0") {
                res = Tools.FilterTags(tag_string, db);
            }
            //otherwise return the entire db and return that
            else {
                res = Tools.GetFullDbQuery(db);
            }


            //sometimes the first call to the db times out. I can't reliably repro it, so I've just created a try catch for it.
            try
            {
                //Tools.TraceLine("  Counting all possible results, before pagination");
                //var count_start = Tools.WriteTimeStamp("  count start");

                //count all the movies possible
                int totalMovies = res.Count();
                //set it to the viewbag so the view can display it
                ViewBag.TotalMovies = totalMovies;

                Tools.TraceLine("  total possible results {0}", totalMovies);
                //var count_end = Tools.WriteTimeStamp("  count_start end");
                //Tools.TraceLine("  counting took {0}", count_end - count_start);


                var page_start = Tools.WriteTimeStamp();

                //limit the amount of movies per page, and then multiply it by the current page
                //page 1 = 0-27, then 28- 55 or something. Math's not my forte
                Tools.TraceLine("  Retrieving paginated results");

                var ids = res.Select(nit => nit.Movie.movie_ID).ToArray();

                Tools.TraceLine("  sorting movie ids");
                var sorted_movie_ids = ids.OrderBy(movie_id => movie_id).Skip(movies_to_skip).Take(movie_count).ToArray();

                Tools.TraceLine("  grabbing matched movies");
                //take all the pages up to and including the ones you'll show 
                // on page, then only take the last set of movies you'll show
                IEnumerable<FullViewModel> nit_list =
                    Tools.GetFullDbQuery(db)
                         .Where(
                             fullViewModel =>
                             sorted_movie_ids.Any(
                                 movie_id =>
                                 movie_id == fullViewModel.Movie.movie_ID))
                         .ToList();
                Tools.TraceLine("  done matching movies, returning");

                //to avoid out of index errors, limit the range chosen. A limitation of doing it with lists, over Linq
                if (totalMovies < movie_count)
                {
                    movie_count = totalMovies;
                }

                var page_end = Tools.WriteTimeStamp();
                Tools.TraceLine("  Taking first page of movies {0}", (page_end - page_start).ToString());

                var end = Tools.WriteTimeStamp("end");
                Tools.TraceLine((end - start).ToString());
                Tools.TraceLine("*********************");

                return View("Results", nit_list);
            }

            catch (System.Data.EntityCommandExecutionException ex)
            {
                Tools.TraceLine(
                    "The ToList() call probably timed out, it happens on first call to db a lot, I don't know why:\n  ***{0}",
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

        

        public ActionResult Api(string term = "Jim Carrey")
        {
            MovieDbContext db = new MovieDbContext();
            //grab new movies, turn one into a Movie and view it
            var data =
                OAuth1a.GetNextflixCatalogDataString(
                    "catalog/titles/streaming", term, max_results: "100",
                    outputPath: Server.MapPath("~/dbfiles/testtesttest.NFPOX"));
            //var titles =
            //    NetFlixAPI.Create.ParseXmlForCatalogTitles(data);

            //List<Movie> movies = new List<Movie>();

            //foreach (Title title in titles)
            //{
            //    //create a  movie from the title, and add it to a list of movies and
            //    // the database
            //    Movie movie = NetFlixAPI.Create.CreateMovie(title);
            //    movies.Add(movie);
            //    db.Movies.Add(movie);

            //    //create a boxart object from the movie and title object
            //    BoxArt boxArt =
            //        NetFlixAPI.Create.CreateMovieBoxartFromTitle(movie, title);
            //    db.BoxArts.Add(boxArt);

            //    //for all the genres in a title, create the linking MtG 
            //    // and then add that object to the db
            //    foreach (Genre genre in title.ListGenres)
            //    {
            //        MovieToGenre movieToGenre =
            //            NetFlixAPI.Create.CreateMovieMovieToGenre(movie,
            //                                                      genre);
            //        db.MovieToGenres.Add(movieToGenre);

            //    }
            //}

            //db.SaveChanges();

            //return View(movies.ToList());
            return View();
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

                //break the tags down by comma delimiter
                List<String> separatedtags = UserInput.DeliminateStrings(tags);
                separatedtags = UserInput.StripWhiteSpace(separatedtags);
                separatedtags = UserInput.SanitizeSpecialCharacters(separatedtags);
                
                foreach (string tag in separatedtags)
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
                        //otherwise select the MovieTag where the names match and use that.
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

        [HttpGet]
        public ActionResult Details(int movie_ID = 0)
        {
            MovieDbContext db = new MovieDbContext();
            FullViewModel fullView = new FullViewModel();

            Movie movie = db.Movies.Find(movie_ID);
            if (movie == null) {
                return View("Error");
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

            //if (true) {

                Tools.TraceLine("Looking to add the tags {0}, to movie id {1}",
                                tags[0], movie_ID);

                MovieDbContext db = new MovieDbContext();
                Movie taggedMovie = db.Movies.Find(movie_ID);
                MovieTag newTag = new MovieTag();

                //break the tags down by comma delimeter
                List<String> seperatedtags = UserInput.DeliminateStrings(tags);
                seperatedtags = UserInput.StripWhiteSpace(seperatedtags);
                seperatedtags = UserInput.SanitizeSpecialCharacters(seperatedtags);

                foreach (string tag in seperatedtags)
                {
                    //JB note: not sure if FoD() here return null if there's nothing, 
                    // I think it return a new Tag() empty which'll mean that 
                    // it's never gonna hit null. Not sure though.
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
                        //otherwise select the MovieTag where the names match and use that.
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
                    
                    Tools.TraceLine("added tag {0} to movie_id {1}", UtMtT.TagId, movie_ID);
                  }
                return RedirectToAction("DetailsTag", "Movies", movie_ID);
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

    }
}
