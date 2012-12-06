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
    public class ToolsController : Controller
    {
        //
        // GET: /Tools/

        public ActionResult Index()
        {


            //returns a dict of id to genres
            Dictionary<string,string> dict = NetFlixAPI.PopulateGenres.CreateDictofGenres(@"c:\genres.NFPOx");
            //create all the genre models
            List<Genre> genres = new List<Genre>();
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                //create genres
                var id = keyValuePair.Key;
                var genre_string = keyValuePair.Value;
                Genre genre = NetFlixAPI.PopulateGenres.CreateGenreModel(id,
                                                                         genre_string);
                //add to list
                genres.Add(genre);
                
            }

            //add to and save table
            Trace.WriteLine("starting to add genres");
            //var db = new MovieDbContext();
            var db = NextFlicksMVC4.Controllers.MoviesController.db;
            foreach (Genre genre in genres)
            {
                db.Genres.Add(genre);
            }


            Trace.WriteLine("starting to save genre table");
            db.SaveChanges();


            return View();
        }

    }
}
