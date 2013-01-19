using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace NextFlicksMVC4.Controllers.userAccount
{
    public class LogOffController : Controller
    {
        //
        // GET: /LogOff/

        public ActionResult Index()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
            //FormsAuthentication.SignOut();
            //return View();

        }

    }
}
