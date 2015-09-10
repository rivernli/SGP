using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.CostEngine;

namespace BI.SGP.Web.Controllers
{
    public class CostEngineController : Controller
    {
        //
        // GET: /CostEngine/

        public ActionResult ComputeCost()
        {
            string postData = Request["postData"];
            string costItem = Request["costItem"];

            string value = ""; ;
            string errMessage = "";
            bool success;

            try
            {
                CostBase cb = CostItemFactory.GetInstance(costItem, postData, BLL.CostEngine.DataConverter.InputDataType.Json);
                if (cb.DataReady)
                {
                    value = Convert.ToString(cb.Compute());
                }
                success = true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                success = false;
            }

            var jsonObject = new
            {
                success = success,
                value = value,
                errMessage = errMessage
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }
    }
}
