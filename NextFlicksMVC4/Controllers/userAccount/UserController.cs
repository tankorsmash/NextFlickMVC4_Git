using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;
using WebMatrix.WebData;

namespace NextFlicksMVC4.Controllers.userAccount
{
    public class UserController : Controller
    {
      // private Users.UserDbContext _userDb = new Users.UserDbContext();
        MovieDbContext _userDb = new MovieDbContext();

       

        public ActionResult Index(string username)
        {
            var model = _userDb.Users.Where(user => user.Username == username).ToList();
            return View(model);
        }

        // GET: /Account/Password
        //for changing passwords
        public ActionResult Password(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";

            ViewBag.HasLocalPassword = false;
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            if (userId != -1)
                ViewBag.HasLocalPassword = true;

            ViewBag.ReturnUrl = Url.Action("Password");
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(Users.PasswordChangeModel model)
        {
            var localAccount = WebSecurity.GetUserId(User.Identity.Name);
            ViewBag.ReturnUrl = Url.Action("Password");

            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                }
                catch
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Password", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        /*
         * 
         * Old user controller stuff below here
         * auto geenrated by mvc
         */
        // GET: /User/
       /* [Authorize(Roles="Admin")]
        public ViewResult Index(string returnUrl)
        {
            //var usernames = Roles.GetUsersInRole(UserRole.Admin.ToString());
            //if (IsAdmin())
                return View(_userDb.Users.ToList());
            //else not an admin
            //ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            //return View("Error");
        }*/

        //
        // GET: /User/Details/5
        [Authorize(Roles="Admin")]
        public ViewResult Details(int id)
        {
           Users usermodel = _userDb.Users.Find(id);
                return View(usermodel);
        }

        //
        // GET: /User/Create
        [Authorize(Roles="Admin")]
        public ActionResult Create()
        {
            return View();
            
        } 

        //
        // POST: /User/Create
        [HttpPost, Authorize(Roles = "Admin")]
        public ActionResult Create(Users usermodel)
        {
            if (ModelState.IsValid)
            {
                _userDb.Users.Add(usermodel);
                _userDb.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Model Invalid";
            return View(usermodel);
        }
        
        //
        // GET: /User/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
                Users usermodel = _userDb.Users.Find(id);
                return View(usermodel);
        }

        //
        // POST: /User/Edit/5

        [HttpPost, Authorize(Roles = "Admin")]
        public ActionResult Edit(Users usermodel)
        {
            if (ModelState.IsValid)
            {
                _userDb.Entry(usermodel).State = EntityState.Modified;
                _userDb.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.error = "Invlaid model";
            return View(usermodel);
        }

        //
        // GET: /User/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Users usermodel = _userDb.Users.Find(id);
            return View(usermodel);
            
        }

        //
        // POST: /User/Delete/5

        [HttpPost, Authorize(Roles = "Admin"), ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
                Users usermodel = _userDb.Users.Find(id);
                _userDb.Users.Remove(usermodel);
                _userDb.SaveChanges();
                return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            _userDb.Dispose();
            base.Dispose(disposing);
        }

    }
}