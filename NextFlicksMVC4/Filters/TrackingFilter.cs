using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.Tracking;

namespace NextFlicksMVC4.Filters
{
    public class TrackingActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            //Trace.WriteLine("Starting filter");
            //save url, userId from session, etc...


            //create a Userlog for this request
            var request = context.HttpContext.Request;

            UserLog userLog = TrackingCreate.CreateUserLog(request);

            TrackingDbContext db = new TrackingDbContext();

            db.UserLogs.Add(userLog);
            db.SaveChanges();
            


            //Trace.WriteLine("raw > url > filepath");
            //Trace.WriteLine(context.HttpContext.Request.RawUrl);
            //Trace.WriteLine(context.HttpContext.Request.Url);
            //Trace.WriteLine(context.HttpContext.Request.FilePath);


            context.Controller.ViewBag.filtered = true;

            //write out all headers
            //var headers = context.HttpContext.Request.Headers;
            //string[] keys =
            //    context.HttpContext.Request.Headers.AllKeys;
            //foreach (string key in keys) {
            //    string msg = string.Format("{0} : {1}", key, headers[key]);
            //    Trace.WriteLine(msg);
            //}

            //write out the user agent
            //string userAgent = context.HttpContext.Request.UserAgent;
            //Trace.WriteLine(userAgent);


            //var model = context.Controller.ViewData.Model;
            //if (model.GetType() == typeof(List<NextFlicksMVC4.Controllers.MoviesController.MovieWithGenreViewModel>))
            //{
            //    Trace.WriteLine("List of models");
            //}

            //test for the custom cookie
            //HttpCookie cookie = context.HttpContext.Request.Cookies.Get("TestCookie");
            //if (cookie != null) {
            //    Trace.WriteLine(cookie.Value);
            //}
            //else {
            //    Trace.WriteLine("Cookie does not exist");
            //}

            //Trace.WriteLine("Ending filter");
        }
    }
}