using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace BI.SGP.Web.Controllers
{
    public class InboxController : Controller
    {
        //
        // GET: /Inbox/

        public ActionResult MyTask()
        {
            return View();
        }
        public ActionResult MyTaskHistory()
        {
            return View();
        }
    }
}
