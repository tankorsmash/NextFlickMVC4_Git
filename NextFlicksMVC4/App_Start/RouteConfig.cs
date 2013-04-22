using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NextFlicksMVC4
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

           /* 
            * had to comment these out because things like /user/changePassword were trying to return a user
            * 
            * routes.MapRoute(
               "ShortUserProfile",
               "U/{username}",
               new { controller = "User", action = "Index", username = String.Empty }
           );
            routes.MapRoute(
                "UserProfile",
                "User/{username}",
                new { controller = "User", action = "Index", username = String.Empty }
            ); */
            routes.MapRoute(
                name: "Admin",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = ""}
                );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}