using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using NextFlicksMVC4.Models.userAccount;
using WebMatrix.WebData;

namespace NextFlicksMVC4
{
    public static class SeedDB
    {

        public static void MembershipRoles()
        {
            //don't o anythign if the admin role exists already
            if (!Roles.RoleExists("Admin"))
            {
                Users user = new Users();

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
                                                                 firstName = "Admin",
                                                                 lastName = "Admin",
                                                                 email = "Admin@phall.us"
                                                             });
                if (!Roles.GetRolesForUser("Admin").Contains("Admin"))
                    Roles.AddUserToRole("Admin", "Admin");
            }
        }

        public static void FullDbBuild()
        {
            //put the full and mutagen methods here? then we can add to global.asax, and it will run when the app starts?
        }
            
    }
}