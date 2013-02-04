using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.Web.Helpers;
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
        private MovieDbContext _userDb = new MovieDbContext();
        //
        // GET: /Register/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Users.RegistrationViewModel user)
        {
            if (user == null)
            {
                return View();
            }
            if (ReCaptcha.Validate(privateKey: "6LdcQtwSAAAAACJPzqNPEoWp37-M-aUZi-6FgZNn"))
            {
                if (ModelState.IsValid)
                {
                    if (WebSecurity.UserExists(user.Username))
                    {
                        ModelState.AddModelError("Username", "User Name has already been chosen, please try another.");
                        return View(user);

                    }
                    WebSecurity.CreateUserAndAccount(
                        user.Username, user.password,
                        new
                        {
                            user.Username,
                            user.email
                        });

                    string username = user.Username;
                    Roles.AddUserToRole(username, "User");
                    WebSecurity.Login(username, user.password, persistCookie: false);
                    ViewBag.Title = "Success!";
                    ViewBag.Message = "You have succesfully been registered!";

                    return RedirectToAction("Index", "Movies");
                }
                ViewBag.Title = "FAILED!";
                return View(user);
            }
            //Else: recaptcha failed!
            return View(user);
        }

    }
}