using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;

namespace NextFlicksMVC4.Controllers.userAccount
{
    //[RequireHttps]
    //TODO: need to figure out how to test https ont he local before deploying
    public class LogonController : Controller
    {
        //Users.UserDbContext _userDb = new Users.UserDbContext();
        MovieDbContext _userDb = new MovieDbContext();
        //
        // GET: /Logon/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LogonModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                var userList = _userDb.Users.Where(u => u.Username.Equals(model.Username));
                //Users user = _userDb.Users.Select() (model.Username); //TODO: need to figure out how to select a user row by ID based on username
                if (userList != null)
                {
                    userList.Cast<Users>();
                    foreach (Users user in userList)
                    {
                        string dbPwd = "";
                        //if (user.ValidatePassword(model.Password))
                        if(user.CheckHash(model.Password, user.password))
                        {
                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, model.Username,
                                DateTime.Now, DateTime.Now.AddMinutes(60), false, user.admin.ToString(),
                                FormsAuthentication.FormsCookiePath);
                            string encTicket = FormsAuthentication.Encrypt(ticket);
                            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

                            //FormsAuthentication.SetAuthCookie(model.Username, false); //set non persistant cookie,
                            //return RedirectToAction("Index", "Home"); //return to home page
                            if (returnUrl != null)
                                return Redirect(returnUrl);
                            else
                                return View("../Home/Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid Username or Password"); //if not valid or not authenticated return error
                        }
                    }
                }
               /* if (model.Username == "bob" && model.Password == "bob") //simulate DB call where username and password valid
                {
                    FormsAuthentication.SetAuthCookie(model.Username, false); //set non persistant cookie,
                    return RedirectToAction("Index", "Home"); //return to home page
                }*/
               
            }
           
            return View();
        }
    }
}
