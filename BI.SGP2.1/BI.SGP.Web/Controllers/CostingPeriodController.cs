using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class CostingPeriodController : Controller
    {
        public ActionResult List()
        {
            FieldCategory fc = new FieldCategory("SCPeriod");
            ViewBag.Category = fc;
            return View();
        }

        public JsonResult SaveData()
        {
            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string postData = Request["postData"];

            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                Int32.TryParse(dataId, out id);
                FieldCategory category = new FieldCategory(Convert.ToString(jsonData["categoryName"]));
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;

                if (id == 0)
                {
                    data["CreatorName"] = AccessControl.CurrentLogonUser.Name;
                    data["CreationTime"] = DateTime.Now;
                }

                using (TScope ts = new TScope())
                {
                    try
                    {
                        CostingPeriodDetail cpd = new CostingPeriodDetail(category, data);
                        category.CheckDataType(data, sysMsg);
                        cpd.CheckData(sysMsg);

                        if (sysMsg.isPass)
                        {
                            if (id > 0)
                            {
                                cpd.Update(id);
                            }
                            else
                            {
                                id = cpd.Add();
                            }

                            if (Convert.ToString(data["Status"]) == "Active")
                            {
                                SetActiveToClose(id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ts.Rollback();
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add("Error", ex.Message);
                    }
                }
            }

            var jsonResult = new
            {
                DataId = id,
                success = sysMsg.isPass,
                errMessage = sysMsg.MessageString
            };
            return Json(jsonResult);
        }

        public JsonResult DelData()
        {
            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string dataId = Request["dataId"];
            string categoryName = Request["categoryName"];

            Int32.TryParse(dataId, out id);
            FieldCategory category = new FieldCategory(categoryName);

            using (TScope ts = new TScope())
            {
                try
                {
                    CostingPeriodDetail cpd = new CostingPeriodDetail(category);

                    if (id > 0)
                    {
                        cpd.Delete(id);
                    }
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }
            }

            var jsonResult = new
            {
                success = sysMsg.isPass,
                errMessage = sysMsg.MessageString
            };
            return Json(jsonResult);
        }

        private void SetActiveToClose(int dataId)
        {
            string strSql = "UPDATE SCM_Period SET Status = 'Close' WHERE Status = 'Active' AND ID <> @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", dataId));
        }
    }
}
