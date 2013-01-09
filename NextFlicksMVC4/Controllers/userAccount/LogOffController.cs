using System.Web.Mvc;
using System.Web.Security;

namespace NextFlicksMVC4.Controllers.userAccount
{
    public class LogOffController : Controller
    {
        //
        // GET: /LogOff/

        public ActionResult Index()
        {
            FormsAuthentication.SignOut();
            return View();

        }

    }
}
