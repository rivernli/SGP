using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.Models;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using SGP.DBUtility;
using System.IO;
using System.Collections.Specialized;
using BI.SGP.BLL.UIManager;
using System.Text;

namespace BI.SGP.Web.Controllers
{
    public class CustomerOrderInformationController : Controller
    {
        //
        // GET: /CustomerOrderInformation/
        public ActionResult CustomerOrderInformation()
        {
            return View();
        }

        public string CusomerOrderInformationResult()
        {
            string jsonData = CustomerOrderInformationManager.GetCustomerOrder(Request);
            return jsonData;
        }

        //Export To Excel
        public FileResult ToExcel()
        {
            DataTable dt = CustomerOrderInformationManager.GetCustomerOrderData(Request.QueryString["myrad"], Request.QueryString["site"], Request.QueryString["PartNo"], Request.QueryString["Item"]);

            string tempFile = Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                ExcelHelper.CustomerStateInfoToExcel(dt, fileStream, Request.QueryString["myrad"]);
            }

            return File(tempFile, "application/ms-excel", "Cisco_Hub_Management.xlsx");
        }
    }
}
