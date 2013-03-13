using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using System.Web.Security;
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;
using WebMatrix.WebData;







namespace NextFlicksMVC4.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            //the commented lines below are a tiem check for the speed of this check at every action from .001 - .002 MS per call
            //var start = Tools.WriteTimeStamp("\n*** WebSecurity Init Start ***");
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
            //var done = Tools.WriteTimeStamp("done at");
            //Tools.TraceLine("took: {0}", done - start);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
               
                //TODO: Hod to change this to my DB context to get the db to autocreate!
                Database.SetInitializer<MovieDbContext>(null);

                try
                {
                    using (var context = new MovieDbContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }
                    //TODO: had to change this to match my database conneciton name, DB name and identify user id and username for simple role membership providers.
                    WebSecurity.InitializeDatabaseConnection("MovieDbContext", "UserProfile", "UserID", "Username", autoCreateTables: true);
                    SeedUserDatabaseTables();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }

            public void SeedUserDatabaseTables()
            {
                if (!Roles.RoleExists("Admin"))
                    Roles.CreateRole("Admin");
                if (!Roles.RoleExists("Mod"))
                    Roles.CreateRole(("Mod"));
                if (!Roles.RoleExists("User"))
                    Roles.CreateRole(("User"));

               if (!WebSecurity.UserExists("Admin"))
                    WebSecurity.CreateUserAndAccount("Admin", "Admin",
                    propertyValues:
                    new
                    {
                        Username = "Admin",
                        email = "Admin@thenextflick.com"
                    });
                if (!Roles.GetRolesForUser("Admin").Contains("Admin"))
                    Roles.AddUserToRole("Admin", "Admin");
            
            }
        }
    }
}
