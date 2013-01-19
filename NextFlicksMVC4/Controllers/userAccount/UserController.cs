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
        private FormsIdentity TicketId;// use this to get the user identity.
        private FormsAuthenticationTicket ticket; //use this ot pull acopy of the ticket 
        

        public ViewResult Seed()
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
            ViewBag.Message = " Admin Accout Seeded";
            return View();
        }
        // GET: /User/
        [Authorize(Roles="Admin")]
        public ViewResult Index(string returnUrl)
        {
            //var usernames = Roles.GetUsersInRole(UserRole.Admin.ToString());
            //if (IsAdmin())
                return View(_userDb.Users.ToList());
            //else not an admin
            //ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            //return View("Error");
        }

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

        private bool IsAdmin()
        {
            TicketId = (FormsIdentity)User.Identity;
            ticket = TicketId.Ticket;
            if (ticket.UserData == "1")
                return true;
            return false;
        }
    }
}