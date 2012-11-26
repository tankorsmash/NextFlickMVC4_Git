using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.Controllers
{
    public class MovieController : Controller
    {
        //
        // GET: /Movie/

        public ActionResult Index()
        {
            MovieDBContext db = new MovieDBContext();
            IEnumerable<Movie> model = db.Movies.ToList();
            return View(model);
        }

    }
}
