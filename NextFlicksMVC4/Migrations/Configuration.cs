using System.Web.Security;
using NextFlicksMVC4.Models.userAccount;
using WebMatrix.WebData;

namespace NextFlicksMVC4.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<NextFlicksMVC4.Models.MovieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(NextFlicksMVC4.Models.MovieDbContext context)
        {
            WebSecurity.InitializeDatabaseConnection("MovieDbContext", "UserProfile", "userID", "Username", autoCreateTables: true );
            if (!Roles.RoleExists("Admin"))
                Roles.CreateRole("Admin");
            if (!Roles.RoleExists("Mod"))
                Roles.CreateRole(("Mod"));
            if(!Roles.RoleExists("User"))
                Roles.CreateRole(("User"));


            /*if (!WebSecurity.UserExists("sec_goat"))
                WebSecurity.CreateUserAndAccount("sec_goat", "password", firstName = "Josh", lastName = "tilson", email = "1@1.1.1" ); 
            if(!Roles.GetRolesForUser("sec_goat").Contains("Admin"))
                Roles.AddUserToRole("sec_goat", "Admin"); */
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}