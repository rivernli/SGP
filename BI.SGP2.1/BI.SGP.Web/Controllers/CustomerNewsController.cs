using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using SGP.DBUtility;
using System.IO;
using BI.SGP.BLL.WF;
using System.Collections.Specialized;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.WF.Action;

namespace BI.SGP.Web.Controllers
{
    public class CustomerNewsController : Controller
    {
        //
        // GET: /CustomerNews/
        public ActionResult News()
        {
            return View();
        }

        public ActionResult NewsEditor()
        {
            return View();
        }

        public ActionResult NewsView()
        {   
            return View();
        }

        public ActionResult NewsLetter()
        {
            return View();
        }

        public string GetNewsView(string id)
        {
            string sSql = @"SELECT (select name from Access_User where Uid=PostedBy) as UserName,
                            (select Customer from SGP_CustomerProfile_Data where id=CustomerId) as Customer,
                            (SELECT COUNT(ID) FROM SGP_CustomerNews_Vews WHERE NewsId=SGP_CustomerNews.ID) AS SumViews, 
                            (SELECT COUNT(ID) FROM SGP_CustomerNews_Comments WHERE NewsId=SGP_CustomerNews.ID) AS Replies, 
                            * FROM SGP_CustomerNews WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(sSql, new SqlParameter("@ID", id)).Tables[0];

            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("PostedDate", "{0:yyyy-MM-dd hh:mm}"));
        }

        //Customer People Grid Data
        public string GetCusomerNewsData()
        {
            string CustomerId = Request.QueryString["CustomerId"];
            string jsonData = CustomerNewsManager.GetCustomerNews(Request, CustomerId);
            return jsonData;
        }

        ///Get Customer People Detail
        public ActionResult GetCustomerNewsDetail(string id)
        {
            CustomerNewsManager CustomerEntity = null;
            string sSql = "SELECT * FROM SGP_CustomerNews WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(sSql, new SqlParameter("@ID", id)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                CustomerEntity = ModelHandler<CustomerNewsManager>.FillModel(dt.Rows[0]);
            }

            return Json(CustomerEntity);
        }

        //Delete Data
        public JsonResult DelData(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    CustomerNewsManager.DeleteCustomerNews(ID);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                errMessage = errMessage
            };
            return Json(result);
        }

        //Save Customer People Data
        public ActionResult SaveData(CustomerNewsManager curd)
        {
            //CustomerProfileManager curd
            string message = "";
            bool success = true;
            try
            {
                curd.Save();
                if (curd.CustomerId == 0)
                {
                    int newsId = (int)curd.ID;
                    SendNewsletterMail(newsId);
                }
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
                data = curd
            };

            return Json(jsonData);
        }

        public void SendNewsletterMail(int newsId)
        {
            string mailAddress = GetNewsletterRecipients();
            SendMailAction mail = new SendMailAction();
            mail.SendNewsletterMail(mailAddress, newsId);
        }

        public string GetNewsletterRecipients()
        {
            string sqlStr = "select * from SGP_KeyValue where [Key] = 'NewletterRecipients'";
            DataTable dt = DbHelperSQL.Query(sqlStr).Tables[0];
            string mailAddress = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                mailAddress += dr["Value"].ToString() + ";";
            }
            return mailAddress;
        }

        public string GetFiles()
        {
            string FileID = Request["FileID"];
            DataTable dt = null;
            if (!String.IsNullOrWhiteSpace(FileID))
            {
                dt = FileHelper.GetFilesData(FileID, null);
            }
            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("CreateTime", "{0:yyyy-MM-dd hh:mm}"));
        }

        //News Reply
        public ActionResult SaveReplyData(CustomerNewsManager curd)
        {
            string NewsId = Request["NewsId"];    //关联新闻表的ID
            string Comment = Request["Comment"];  //评论

            string message = "";
            bool success = true;
            try
            {                
                curd.SaveReply(NewsId, Comment);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            var jsonData = new
            {
                success = success,
                message = message
            };

            return Json(jsonData);
        }

        //Get Reply Data
        public string GetReply()
        {
            string NewsId = Request["NewsId"];
            DataTable dt = null;
            if (!String.IsNullOrWhiteSpace(NewsId))
            {
                dt = CustomerNewsManager.GetReplyData(NewsId);
            }
            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("PostedDate", "{0:yyyy-MM-dd hh:mm}"));
        }

        public ActionResult SaveNewsViews(string ID)
        {
            //点击数
            string errMessage = "";

                try
                {
                    CustomerNewsManager.SaveViews(ID);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }

            var result = new
            {
                success = (errMessage == "" ? true : false),
                errMessage = errMessage
            };
            return Json(result);            
        }
    }
}
