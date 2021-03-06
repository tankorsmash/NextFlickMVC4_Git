﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Diagnostics;
using System.Web.Security;
using NextFlicksMVC4.Controllers;
using NextFlicksMVC4.Filters;
using WebMatrix.WebData;

using NextFlicksMVC4.Models;
using System.Data.Entity;

namespace NextFlicksMVC4
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);

           // Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MovieDbContext>());
            //add custom filter
            //GlobalFilters.Filters.Add(new TrackingActionFilterAttribute());
            //end customization filter

            //enable Simple Membership Provider database
            GlobalFilters.Filters.Add(new InitializeSimpleMembershipAttribute());
           //end simple role membership init
            

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            

        }

        ////from http://blog.davebouwman.com/2011/04/21/custom-404-pages-for-asp-net-mvc-3/
        //protected void Application_Error()
        //{
        //    var exception = Server.GetLastError();
        //    var httpException = exception as HttpException;
        //    Response.Clear();
        //    Server.ClearError();
        //    var routeData = new RouteData();
        //    routeData.Values["controller"] = "Errors";
        //    routeData.Values["action"] = "General";
        //    routeData.Values["exception"] = exception;
        //    Response.StatusCode = 500;
        //    if (httpException != null) {
        //        Response.StatusCode = httpException.GetHttpCode();
        //        switch (Response.StatusCode) {
        //            case 403:
        //                routeData.Values["action"] = "Http403";
        //                break;
        //            case 404:
        //                routeData.Values["action"] = "Http404";
        //                break;
        //        }
        //    }
        //    // Avoid IIS7 getting in the middle
        //    Response.TrySkipIisCustomErrors = true;
        //    IController errorsController = new ErrorsController();
        //    HttpContextWrapper wrapper = new HttpContextWrapper(Context);
        //    var rc = new RequestContext(wrapper, routeData);
        //    errorsController.Execute(rc);
        //}
    }
}
