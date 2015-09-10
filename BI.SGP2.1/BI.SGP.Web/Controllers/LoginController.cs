using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BI.SGP.Web.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public RedirectResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return new RedirectResult("~/");
        }

    }
}
