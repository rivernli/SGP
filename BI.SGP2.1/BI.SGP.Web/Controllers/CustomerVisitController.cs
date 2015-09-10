using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using SGP.DBUtility;
using System.IO;
using System.Collections;

namespace BI.SGP.Web.Controllers
{
    public class CustomerVisitController : Controller
    {
        //
        // GET: /CustomerVisit/

        public ActionResult CustomerVisitList()
        {
            return View();
        }

        public ActionResult CustomerVisitCalendar()
        {
            return View();
        }

        public ActionResult CustomerVisitDetail()
        {
            return View();
        }

        public ActionResult CustomerActionsTrackerList()
        {
            return View();
        }

        public ActionResult CustomerActionsTrackerDetail()
        {
            List<SelectListItem> select1 = new List<SelectListItem>();
            DataTable dt = SqlText.ExecuteDataset("SELECT [Key],[Value] FROM SGP_KeyValue WHERE [Key]=@Key AND Status=1 AND ISNULL([Value],'')<>'' ORDER BY [Sort]", new SqlParameter("@Key", "CustomerVisitTrackerStatus")).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    select1.Add(new SelectListItem
                    {
                        Text = Convert.ToString(dr["Value"]),
                        Value = Convert.ToString(dr["Value"])
                    });
                }
            }
            ViewData["Status"] = new SelectList(select1, "Value", "Text", "");

            return View();
        }
        
        //Customer Visit Grid Data
        public string CustomerVisitData()
        {
            string jsonData = CustomerVisitManager.GetCustomerVisitData(Request);
            return jsonData;
        }
        //Customer Visit Involved People
        public string CustomerVisitInvolvedPeople()
        {
            string VisitID = Request["VisitID"];
            DataTable dt = CustomerVisitManager.GetVisitInvolvedPeopleData(VisitID);

            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson();
        }


        public ActionResult GetCustomerVisitList()
        {            
            DataSet ds = CustomerVisitManager.GetVisit();

            string jsonstring = "[";
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                jsonstring += "{";

                jsonstring += "\"id\":" + "\"" + Convert.ToInt32(dr["ID"]) + "\",";
                jsonstring += "\"title\":" + "\"" + dr["Customer"].ToString() + "\",";
                jsonstring += "\"content\":" + "\"" + dr["VisitPurpose"].ToString() + "\",";
                jsonstring += "\"start\":" + "\"" + Convert.ToDateTime(dr["StartDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "\",";
                jsonstring += "\"end\":" + "\"" + Convert.ToDateTime(dr["EndDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "\"";

                jsonstring += "},";
            }
            if (jsonstring.Length > 1)
            {
                jsonstring = jsonstring.Substring(0, jsonstring.Length - 1);
            }
            jsonstring += "]";

            return Content(jsonstring, "application/json");
        }

        ///Get Customer Data
        public ActionResult GetCustomerVisitDetailData(string id)
        {
            CustomerVisitManager CustomerVisitEntity = null;
            string sSql = "SELECT * FROM SGP_CustomerVisit WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(sSql, new SqlParameter("@ID", id)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                CustomerVisitEntity = ModelHandler<CustomerVisitManager>.FillModel(dt.Rows[0]);
            }

            var dateConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "MM/dd/yyyy" };
            ViewData["json"] = Newtonsoft.Json.JsonConvert.SerializeObject(CustomerVisitEntity, dateConverter);
 
            return Content(Convert.ToString(ViewData["json"]), "application/json");
        }

        //Customer Visit Actions Tracker Grid Data
        public string CustomerVisitActionsTrackerData()
        {
            string VisitId = Request.QueryString["VisitId"];
            string jsonData = CustomerVisitActionsTrackerManager.GetCustomerVisitActionsTrackerData(Request, VisitId);
            return jsonData;
        }

        ///Get Customer Visit Actions Tracker Detail
        public ActionResult GetCustomerVisitActionTrackerDetail(string id)
        {
            CustomerVisitActionsTrackerManager visitEntity = null;
            string sSql = "SELECT * FROM SGP_CustomerVisit_ActionsTracker WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(sSql, new SqlParameter("@ID", id)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                visitEntity = ModelHandler<CustomerVisitActionsTrackerManager>.FillModel(dt.Rows[0]);
            }
            return Json(visitEntity);
        }

        [HttpPost]
        public ActionResult SaveData()
        {
            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string postData = Request["postData"];

            id = Save(postData, sysMsg);

            var jsonData = new
            {
                SysMsg = sysMsg,
                DataId = id
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        private int Save(string postData, SystemMessages sysMsg)
        {
            int id = 0;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string operation = Convert.ToString(jsonData["operation"]);
                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
               
                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            CustomerVisitManager dm = new CustomerVisitManager();
                            if (id > 0)
                            {
                                dm.Update(id, data);
                            }
                            else
                            {
                                id = dm.Add(data);
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
            }
            return id;
        }

        //Delete Visit List Data
        public JsonResult DelData(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        CustomerVisitManager.DeleteVisitListData(ID);
                    }
                    catch (Exception ex)
                    {
                        errMessage = ex.Message;
                    }
                }
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                errMessage = errMessage
            };
            return Json(result);
        }
                
        //Delete Visit List Actions Tracker Data
        public JsonResult DelActionsTrackerData(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        CustomerVisitActionsTrackerManager.DeleteVisitActionsTrackerData(ID);
                    }
                    catch (Exception ex)
                    {
                        errMessage = ex.Message;
                    }
                }
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                errMessage = errMessage
            };
            return Json(result);
        }

        //删除
        [HttpPost]
        public ActionResult DeleteInvolvedPeople()
        {
            SystemMessages sysMsg = new SystemMessages();
            string id = Request["ID"];

            using (TScope ts = new TScope())
            {
                try
                {
                    CustomerVisitManager.DeletePeople(id);
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }
            }

            var jsonData = new
            {
                SysMsg = sysMsg
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Save Visit Actions Tracker
        /// </summary>
        /// <param name="curd"></param>
        /// <returns></returns>
        public ActionResult SaveVisitActionsTrackerData(CustomerVisitActionsTrackerManager ActionsTrackerManager)
        {
            //CustomerProfileManager curd
            string message = "";
            bool success = true;
            try
            {
                ActionsTrackerManager.Save();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            var jsonData = new
            {
                success = success,
                message = message,
                data = ActionsTrackerManager
            };

            return Json(jsonData);
        }

        
    }
}
