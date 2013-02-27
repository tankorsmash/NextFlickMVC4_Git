using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web.Routing;
using Microsoft.Web.Helpers;
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


            //return View();
            return RedirectToRoute( new {controller = "Movies", action = "Index"});
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View("About");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View("Feedback");
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
            bool validRecaptcha = false;

            //check if user is logged in, if so set validRecaptcha to true as we don't want users having to use recap
            if (WebSecurity.IsAuthenticated)
            {
                validRecaptcha = true;
            }
                //else check the recap froma non logged in user and make sure it is ok before sending email.
                //TODO: reset recaptcha PRIVATE key here if domains change
            else if (ReCaptcha.Validate(privateKey: "6Ld_kt0SAAAAAAWadUrgyxHuqAlp2fjS5RXjdyn9"))
            {
                validRecaptcha = true;
            }

            if(validRecaptcha)
            {
                if (ModelState.IsValid)
                {
                    string from = feedback.Email;
                    string to = "feedback@wearethewatchers.com";
                    string subject = "Feedback from WATW";
                    string body = feedback.Message;
                    MailMessage message = new MailMessage(from, to, subject, body);

                    SmtpClient client = new SmtpClient();
                    client.Send(message);

                    //TODO: http://stackoverflow.com/questions/10022498/setting-up-email-settings-in-appsettings-web-config
                }
                return View();
            }
            return View(feedback);
        }
    }
}
