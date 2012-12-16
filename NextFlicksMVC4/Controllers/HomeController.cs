using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace NextFlicksMVC4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //since Home/Index is considered startup location, trace where the server is headed. 
            //Should be localhost:port at the moment
            string msg = String.Format("current address: {0}", Request.Url.OriginalString.ToString());
            Trace.WriteLine(msg);

            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
