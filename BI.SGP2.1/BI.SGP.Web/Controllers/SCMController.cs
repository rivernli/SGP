using BI.SGP.BLL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BI.SGP.Web.Controllers
{
    public class SCMController : Controller
    {

        [MyAuthorize]
        public ActionResult SCMView()
        {
            ViewBag.ExcelView = "~/Views/Shared/_ExportExcel.cshtml";
            ViewBag.GridGroup = "SGPForSCMGrid";
            ViewBag.SearchGroup = "SGPForSCMSearch";
            ViewBag.UserID = AccessControl.CurrentLogonUser.Uid;
            return View();
        }

    }
}
