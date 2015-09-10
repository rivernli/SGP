using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.Utils;


namespace BI.SGP.Web.Controllers
{
    public class HelperController : Controller
    {
        //
        // GET: /Helper/

        public ActionResult VideoIntroduction()
        {
            return View();
        }
        public ActionResult FAQ()
        {
            return View();
        
        }

    }
}
