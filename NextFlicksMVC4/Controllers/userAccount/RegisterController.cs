using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;
using WebMatrix.WebData;

namespace NextFlicksMVC4.Controllers.userAccount
{
    //[RequireHttps]
    //TODO: need to figure out how to test https ont he local before deploying
    public class RegisterController : Controller
    {
        //Users.UserDbContext _userDb = new Users.UserDbContext();
        MovieDbContext _userDb = new MovieDbContext();
        //
        // GET: /Register/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Users.RegistrationViewModel user)
        {
            ///
            /// This is where we check to make sure the modelmatches what we wrote in RegsirationViewModel
            /// and if it does we need to convert it back to the Users model so we can enter a new user into the database.
            /// Probbaly not the most ideal way of doing this, but a straight cast did not work.
            /// Also find another way of verifying of user is active: I.E: send out email with link to click to activate useraccount
            ///
            if (ModelState.IsValid)
            {
                WebSecurity.CreateUserAndAccount(user.Username, user.password, propertyValues: new { username = user.Username, firstName = user.firstName, lastName = user.lastName, email = user.email });
                string username = user.Username;
                Roles.AddUserToRole(username, "User");
                ViewBag.Title = "Success!";

                //Users newUser = new Users();
                //newUser.Username = user.Username;
                //newUser.firstName = user.firstName;
                //newUser.lastName = user.lastName;
                //newUser.email = user.email;
                //newUser.password = newUser.SetPassword(user.password);
               // newUser.password = newUser.SetHash((user.password));
                //var userData = new {{"firstName", user.firstName},{"lastName", user.lastName}, {"email", user.email}};
                // _userDb.Users.Add(newUser);
                //_userDb.SaveChanges();
                ViewBag.Message = "You have succesfully been registered!";
                return View("../Home/Index");
            }
            ViewBag.Title = "FAILED!";
            return View(user);
        }

    }
}
