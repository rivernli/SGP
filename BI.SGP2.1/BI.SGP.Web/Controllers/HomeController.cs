using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BI.SGP.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Main/

        public RedirectResult Home()
        {

            return new RedirectResult("~/");
        }

        public ActionResult Msg(string id)
        {
            switch (id)
            {
                case "AccessIssue":
                    ViewBag.Message = "You have no permission to access.";
                    break;
                default:
                    ViewBag.Message = Session["_errorMessage"];
                    break;
            }
            return View();
        }
    }
}
