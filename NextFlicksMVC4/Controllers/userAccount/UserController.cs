using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using NextFlicksMVC4.Models.userAccount;

namespace NextFlicksMVC4.Controllers.userAccount
{ 
    public class UserController : Controller
    {
       private Users.UserDbContext _userDb = new Users.UserDbContext();
        private FormsIdentity TicketId;// use this to get the user identity.
        private FormsAuthenticationTicket ticket; //use this ot pull acopy of the ticket 
        
        // GET: /User/
        [Authorize]
        public ViewResult Index(string returnUrl)
        {
            if (IsAdmin())
                return View(_userDb.Users.ToList());
            //else not an admin
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }

        //
        // GET: /User/Details/5
        [Authorize]
        public ViewResult Details(int id)
        {
            if (IsAdmin())
            {
                Users usermodel = _userDb.Users.Find(id);
                return View(usermodel);
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }

        //
        // GET: /User/Create
        [Authorize]
        public ActionResult Create()
        {
            if(IsAdmin())
                return View();
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        } 

        //
        // POST: /User/Create
        [HttpPost, Authorize]
        public ActionResult Create(Users usermodel)
        {
            if (IsAdmin())
            {
                if (ModelState.IsValid)
                {
                    _userDb.Users.Add(usermodel);
                    _userDb.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(usermodel);
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }
        
        //
        // GET: /User/Edit/5
        [Authorize]
        public ActionResult Edit(int id)
        {
            if (IsAdmin())
            {
                Users usermodel = _userDb.Users.Find(id);
                return View(usermodel);
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }

        //
        // POST: /User/Edit/5

        [HttpPost, Authorize]
        public ActionResult Edit(Users usermodel)
        {
            if (IsAdmin())
            {
                if (ModelState.IsValid)
                {
                    _userDb.Entry(usermodel).State = EntityState.Modified;
                    _userDb.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(usermodel);
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }

        //
        // GET: /User/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            if (IsAdmin())
            {
                Users usermodel = _userDb.Users.Find(id);
                return View(usermodel);
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
        }

        //
        // POST: /User/Delete/5

        [HttpPost, Authorize, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (IsAdmin())
            {
                Users usermodel = _userDb.Users.Find(id);
                _userDb.Users.Remove(usermodel);
                _userDb.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Unathorized Access! I'm afraid I Can't Do that Dave!";
            return View("Error");
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