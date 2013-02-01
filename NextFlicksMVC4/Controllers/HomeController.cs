using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.Models;
using WebMatrix.WebData;
using System.Web.Security;
using System.Net.Mail;

namespace NextFlicksMVC4.Controllers
{
   // [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //since Home/Index is considered startup location, trace where the server is headed. 
            //Should be localhost:port at the moment
            string msg = String.Format("current address: {0}", Request.Url.OriginalString.ToString());
            Trace.WriteLine(msg);

            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Feedback()
        {
            MovieDbContext db = new MovieDbContext();
            FeedbackModel feedback = new FeedbackModel();
            

            if (WebSecurity.IsAuthenticated)
            {
                int userID = WebSecurity.CurrentUserId;
                var email = from user in db.Users
                        where user.userID == userID
                        select user.email;
                feedback.Email = email.FirstOrDefault();
            }
            return View("Feedback", feedback);
        }

        [HttpPost]
        public ActionResult Feedback(FeedbackModel feedback)
        {
            if (ModelState.IsValid)
            {
                string from = feedback.Email;
                string to = "joshua.tilson@yahoo.com";
                string subject = "Feedback from WATW";
                string body = feedback.Message;
                MailMessage message = new MailMessage(from, to, subject, body);
 
                SmtpClient client = new SmtpClient(host: "mail.wearethewatchers.com", port: 25);
                client.Send(message);

                //TODO: http://stackoverflow.com/questions/10022498/setting-up-email-settings-in-appsettings-web-config
            }
            return View();
        }
    }
}
