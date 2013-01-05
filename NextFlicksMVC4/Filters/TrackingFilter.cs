using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace NextFlicksMVC4.Filters
{
    public class TrackingActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            
            //save url, userId from session, etc...


            //Trace.WriteLine("raw > url > filepath");
            //Trace.WriteLine(context.HttpContext.Request.RawUrl);
            //Trace.WriteLine(context.HttpContext.Request.Url);
            //Trace.WriteLine(context.HttpContext.Request.FilePath);



            HttpCookie cookie = context.HttpContext.Request.Cookies.Get("TestCookie");
            if (cookie != null) {
                Trace.WriteLine(cookie.Value);
            }
            else {
                Trace.WriteLine("Cookie does not exist");
            }
        }
    }
}