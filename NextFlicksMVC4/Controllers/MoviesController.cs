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

namespace NextFlicksMVC4.Controllers
{
    public class MoviesController : Controller
    {
        private MovieDBContext db = new MovieDBContext();

        //
        // GET: /Movies/

        [HttpGet]
        public ActionResult Filter()
        {//Create a list of SelectListItems   List<SelectListItem> years = new List<SelectListItem> { 
        new SelectListItem { Text = "1999", Value = "1999" },
        new SelectListItem { Text = "2000", Value = "2000" } 
   };

            //Create a selectList, but since it's looking for an IEnumerable,
            // tell it what is the value and text parts
            SelectList slist = new SelectList(years, "Value", "Text" );
            
            //give the Viewbag a property for the SelectList
            ViewBag.years = slist;


            return View();
        }

        //[HttpPost]
        //public ActionResult Filter(string name)
        //{
         

        //    return View("FilterHandler");
        //}


        public ActionResult FilterHandler(string name)
        {
            ViewData["name"] = name;
            Trace.WriteLine(ViewData["pet"]);

            return View("FilterHandler");
        }

        public ActionResult Year(int start = 2001, int end = 2002)
        {
            //returns all titles from year
            string qry = "select * from Movies where (year between {0} and {1}) AND (is_movie = 0) order by year";
            var res = db.Movies.SqlQuery(qry, start, end);

            ViewBag.TotalMovies = end;
            ViewBag.Start = start;
            ViewBag.Count = 12345;

            return View("Year", res.ToList());
        }

        public ActionResult Index(int start = 0, int count = 10)
        {
            var fullList = db.Movies.ToList();
            //total movies in DB
            ViewBag.TotalMovies = fullList.Count;
            ViewBag.Start = start;
            ViewBag.Count = count;

            return View(fullList.GetRange(start, count));
        }

        public ActionResult Full()
        {
            // Go line by line, and parse it for Movie files
            List<Movie> listOfMovies = new List<Movie>();

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
                            Trace.WriteLine("Invalid line");
                        }
                        else
                        {
                            //parse line for a title, which is what NF returns
                            List<Title> titles =
                                NextFlicksMVC4.Create.ParseXmlForCatalogTitles(data);
                            Movie movie =
                                NextFlicksMVC4.Create.CreateMovie(titles[0]);
                            listOfMovies.Add(movie);
                            string msg = String.Format("Added item {0}", count.ToString());
                            Trace.WriteLine(msg);
                            count += 1;

                        }
                        data = reader.ReadLine();
                    }
                }

                catch (System.Xml.XmlException ex)
                {
                    Trace.WriteLine("XML ERROR OH GOD:");
                    Trace.WriteLine(ex.Message);
                }

                Trace.WriteLine("Beginning Add to DB");

                int modulo =0;
                List<int> checkpoints = new List<int>();
                int total = listOfMovies.Count;
                int start = total/25;
                while (modulo <= listOfMovies.Count)
                {
                   checkpoints.Add(modulo);
                    modulo += start;
                }
                int counter = 0;
                if (listOfMovies.Count > 0)
                {
                    foreach (Movie movie in listOfMovies)
                    {
                        db.Movies.Add(movie);
                        counter += 1;
                        if (checkpoints.Contains(counter))
                        {
                            string msg =
                                String.Format(
                                    "Done adding at least {0} movies", counter);
                            Trace.WriteLine(msg);
                        }
                    }

                    Trace.WriteLine("Saving Changes");
                    db.SaveChanges();
                    Trace.WriteLine("Done Saving!");
                }
            }

            return View();
        }

        public  ActionResult API(string term="Jim Carrey")
        {
            //grab new movies, turn one into a Movie and view it
            var data = OAuth1a.GetNextflixCatalogDataString("catalog/titles/streaming", term, max_results:"100", outputPath:@"C:/streamingAPI2.NFPOX");
            var titles =
                NextFlicksMVC4.Create.ParseXmlForCatalogTitles(data);

            List<Movie> movies = new List<Movie>();

            foreach (Title title in titles)
            {
            Movie movie = NextFlicksMVC4.Create.CreateMovie(title);
                movies.Add(movie);
                db.Movies.Add(movie);
            }

            db.SaveChanges();

            return View(movies.ToList());
        }

        //
        // GET: /Movies/Details/5

        public ActionResult Details(int id = 0)
        {
            Movie movie = db.Movies.Find(id);
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
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}