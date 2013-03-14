using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.OMBD;
using System.IO;

namespace NextFlicksMVC4.Controllers.Admin
{
   // [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        MovieDbContext db = new MovieDbContext();
        //
        // GET: /Admin/
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays a readout of the trace.log file
        /// </summary>
        /// <param name="linesToShow">the amount of lines to show on the page at once</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public ActionResult LogFile(int linesToShow = 1000)
        {
            ViewBag.Log = new List<string>();
            var tempList = new List<string>();
            var logfile = System.Web.HttpContext.Current.Server.MapPath("~/trace.log");
            using(var fs = new FileStream(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    tempList.Add(sr.ReadLine());
                }

                //count the lines read, then make sure the param isn't too high
                int logCount = tempList.Count;
                if (logCount < linesToShow) {
                    linesToShow = logCount;
                }
                //reverse ViewBag.Log so that the newest is at top
                tempList.Reverse();

                //after taking only the desired amount of items, set it to the viewbag so the view knows whats up
                ViewBag.Log = tempList.Take(linesToShow).ToList();

            }
           
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DbTools()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DbTools(string button)
        {
            if (button == "Drop Tables")
            {
                DatabaseTools.DropTables();
                ViewBag.Message = "Tables Dropped";
            }
            if (button == "Create Tables")
            {
                DatabaseTools.CreateTables();
                ViewBag.Message = "Tables Created";
            }
            if (button == "Drop And Create")
            {
                DatabaseTools.DropAndCreate();
                ViewBag.Message = "Tables dropped and Recreated";
            }
            if (button == "Full")
            {
                DatabaseTools.Full();
                ViewBag.Message = "Full Db Created";
            }
            if (button == "Api")
            {
                DatabaseTools.Api();
                ViewBag.Message = "Api Downloaded.";
            }
            if (button == "Get Genres From Netflix")
            {
                DatabaseTools.NetflixGenres();
                ViewBag.Message = "Genres downloaded fron Netflix";
            }
            if (button == "Update Genres In DB")
            {
                Tools.UpdateGenreList(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX"));
                ViewBag.Message = "Update Genres List";

            }
            if (button == "Full DB Build")
            {
                ViewBag.Message = "Full Db Build / Update Complete";
                DatabaseTools.FullDbBuild();
            }
            if (button == "Join Lines")
            {
                Tools.JoinLines(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX"));
            }
            if (button == "omdb")
            {
                MovieDbContext db = new MovieDbContext();
                var omdbZIP = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.zip");
                var omdbDUMP = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.DUMP");
                var omdbTXT = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/omdb.txt");
                var tomatoesTXT = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/tomatoes.txt");
                //download the omdbapi
                Omdb.DownloadOmdbZipAndExtract(omdbZIP);

                //parse it for omdbentrys, serialize it to file
                Tools.SerializeOmdbTsv(omdbDUMP, omdbTXT, tomatoesTXT);

                //deserialize the file, turn it into omdb
                //  can't remember if it does it here or not, but marry the omdbs and movie
                Tools.RebuildOmdbsFromProtobufDump(omdbDUMP);

                Tools.MarryMovieToOmdb(db);
                Tools.RebuildOmdbsFromProtobufDump(omdbDUMP);
            }
            
            return View();
        }
    }
}
