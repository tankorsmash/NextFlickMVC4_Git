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
            if (user == null)
            {
               return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                if (WebSecurity.UserExists(user.Username))
                {
                    ViewBag.Message = "Username already exists, please try another one";
                    TempData["validReCaptcha"] = "true";
                    return RedirectToAction("Index", "Register", user);
                }
                WebSecurity.CreateUserAndAccount(
                    user.Username, user.password,
                    propertyValues: new { username = user.Username, 
                        firstName = user.firstName, 
                        lastName = user.lastName, email = user.email},
                    requireConfirmationToken: true);
                
                string username = user.Username;
                Roles.AddUserToRole(username, "User");
                ViewBag.Title = "Success!";
                ViewBag.Message = "You have succesfully been registered!";
               
                return View("../Home/Index");
            }
            ViewBag.Title = "FAILED!";
            return View(user);
        }

       
        [HttpPost]
        public ActionResult SubmitCap()
        {
            if (ReCaptcha.Validate(privateKey: "6LdcQtwSAAAAACJPzqNPEoWp37-M-aUZi-6FgZNn"))
            {
                TempData["validReCaptcha"] = "true";
                return RedirectToAction("Index", "Register");
            }
            TempData["validReCaptcha"] = "false";
            return RedirectToAction("Index", "Register");
        }
    }
}
