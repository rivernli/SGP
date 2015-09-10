using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;

namespace BI.SGP.Web.Controllers
{
    public class AccountsController : Controller
    {
        //
        // GET: /Management/
        public ActionResult PermissionGroup()
        {
            return View();
        }

        public ActionResult Employee()
        {
            return View();
        }

        public ActionResult Delegation()
        {
            return View();
        }

        public ActionResult GetAccountList()
        {
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            string strSql = "SELECT TOP 10 UID,NAME FROM Access_User WHERE UID <> @UID AND (UID LIKE @term OR NAME LIKE @term)";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@UID", AccessControl.CurrentLogonUser.Uid),
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["NAME"] + "(" + dr["UID"] + ")") + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["NAME"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult AddDelegation()
        {
            bool addSuccess = false;
            string errMessage = "";
            string userName = Request["userName"];
            if (!String.IsNullOrWhiteSpace(userName))
            {
                try
                {
                    string strSql = "SELECT TOP 1 UID,NAME FROM Access_User WHERE Name LIKE @Name";
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", "%" + userName.Trim() + "%")).Tables[0];
                    
                    if (dt.Rows.Count > 0)
                    {
                        string uid = Convert.ToString(dt.Rows[0]["UID"]);
                        string uName = Convert.ToString(dt.Rows[0]["NAME"]);
                        strSql = "IF NOT EXISTS (SELECT * FROM SGP_Delegation WHERE FromUser = @FromUser AND ToUser = @ToUser) INSERT INTO SGP_Delegation(FromUser,ToUser,FromUserName,ToUserName) VALUES(@FromUser,@ToUser,@FromUserName,@ToUserName)";
                            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[] {
                            new SqlParameter("@FromUser", AccessControl.CurrentLogonUser.Uid),
                            new SqlParameter("@ToUser", uid),
                            new SqlParameter("@FromUserName", AccessControl.CurrentLogonUser.Name),
                            new SqlParameter("@ToUserName", uName),
                        });
                        addSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }

            string strResult = "{\"success\":" + Newtonsoft.Json.JsonConvert.SerializeObject(addSuccess) + ",\"errMessage\":" + Newtonsoft.Json.JsonConvert.SerializeObject(errMessage) + ",\"data\":" + Newtonsoft.Json.JsonConvert.SerializeObject(GetDelegationData()) + "}";

            return Content(strResult);
        }

        public ActionResult GetDelegation()
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(GetDelegationData()));
        }

        public ActionResult DelDelegation()
        {
            bool success = false;
            string errMessage = "";
            string id = Request["id"];
            int dId = 0;
            int.TryParse(id, out dId);
            if (dId > 0)
            {
                try
                {
                    string strSql = "DELETE FROM SGP_Delegation WHERE ID = @ID";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", dId));
                    success = true;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }

            var jsonResult = new
            {
                success = success,
                errMessage = errMessage
            };
            return Json(jsonResult);
        }

        private DataTable GetDelegationData()
        {
            string strSql = "SELECT * FROM SGP_Delegation WHERE (FromUser = @UID OR ToUser = @UID)";
            return DbHelperSQL.Query(strSql, new SqlParameter("@UID", AccessControl.CurrentLogonUser.Uid)).Tables[0];
        }

        
    }
}
