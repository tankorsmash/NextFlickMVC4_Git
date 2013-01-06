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
    [Obsolete("",true)]
    public class ToolsController : Controller
    {
        //
        // GET: /Tools/

        public ActionResult Index()
        {


            //fill the Genres table
            PopulateGenres.PopulateGenresTable();


            return View();
        }

    }
}
