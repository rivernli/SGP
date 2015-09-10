using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using SGP.DBUtility;
using System.IO;
using BI.SGP.BLL.WF;
using System.Collections.Specialized;
using BI.SGP.BLL.DataModels;

namespace BI.SGP.Web.Controllers
{
    public class CustomerProfileController : Controller
    {
        [AuthorityFilter(RolesCode = "Customer_Profile")]
        public ActionResult CustomerList()
        {
            ViewBag.Title = "Customer Profiles";
            ViewBag.GridGroup = "CustomerProfileGrid";
            ViewBag.SearchGroup = "CustomerProfileSearch";

            //权限
            ViewBag.Roles = 1;
            if (!AccessControl.CurrentLogonUser.IsCustomerAdmin())
            {
                PowerManager.LoadPower("22");
                if (!PowerManager.HasDel)//删除权限
                {
                    if (!PowerManager.HasEdit)//如没有编辑权限时
                    {
                        ViewBag.Roles = 0;
                    }
                }
            }

            return View();
        }

        [AuthorityFilter(RolesCode = "Customer_Profile")]
        public ActionResult CustomerEditor(string ID)
        {
            //if (Request.QueryString["ID"] != null)
            //{
            //    rf = CustomerProfileManager.GetDetail(Request.QueryString["ID"])[0];
            //}

            int dataId = ParseHelper.Parse<int>(ID);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_CUSTOMERPROFILE);
            if (dataId > 0)
            {
                CustomerProfileDetail detail = new CustomerProfileDetail();
                detail.FillCategoryData(lfc, dataId);
            }
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;

            //权限
            ViewBag.Roles = 1;
            if (!AccessControl.CurrentLogonUser.IsCustomerAdmin())
            {
                PowerManager.LoadPower("22");
                if (!PowerManager.HasDel)//删除权限
                {
                    if (!PowerManager.HasEdit)//如没有编辑权限时
                    {
                        ViewBag.Roles = 0;
                    }
                }
            }

            return View();
        }

        public ActionResult CustomerPrint(string ID)
        {
            //if (Request.QueryString["ID"] != null)
            //{
            //    rf = CustomerProfileManager.GetDetail(Request.QueryString["ID"])[0];
            //}

            int dataId = ParseHelper.Parse<int>(ID);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_CUSTOMERPROFILE);
            if (dataId > 0)
            {
                CustomerProfileDetail detail = new CustomerProfileDetail();
                detail.FillCategoryData(lfc, dataId);
            }
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            return View();
        }

        //Import
        public ActionResult CustomerImport()
        {
            return View();
        }


        //Delete
        public JsonResult DelData(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    CustomerProfileManager.DeleteCustomerData(ID);
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

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_CUSTOMERPROFILE);
               
                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                        //检查Customer Name 唯一
                        CheckCustomerName(dataId, data[fc.ID] as Dictionary<string, object>, sysMsg);
                    } 
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            CustomerProfileDetail dm = new CustomerProfileDetail(lfc, data);
                            if (id > 0)
                            {
                                dm.Update(id);
                            }
                            else
                            {
                                id = dm.Add();
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


        //检查是否供应商名称唯一
        public void CheckCustomerName(string dataId, Dictionary<string, object> data, SystemMessages sysMsg)
        {
            object value = data["Customer"];
            CustomerProfileManager.CheckNameOnly(dataId, value, sysMsg); 
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

        //Export To Excel
        public FileResult ToExcel()
        {
            string ID = Request.QueryString["id"];
            DataTable dt = CustomerProfileManager.GetCustomerData(ID);

            string tempFile = Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                ExcelHelper.CustomerToExcel(dt, fileStream);
            }

            return File(tempFile, "application/ms-excel", "Sales_Marketing_QBR.xlsx");
        }

        //合计People
        public ActionResult PeopleTotal(string ID)
        {
            int total = CustomerProfileManager.GetPeopleTotal(ID);
            return Json(total);
        }

        //合计New
        public ActionResult NewTotal(string ID)
        {
            int total = CustomerProfileManager.GetNewTotal(ID);
            return Json(total);
        }

        //读取Excel
        public static DataTable successdt = new DataTable();
        public ActionResult LoadExcel()
        {
            DataTable dt = new DataTable();
            string message = string.Empty;
            bool flag = SaveAndReadFile(out dt, out message);

            if (flag == true && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    CustomerProfileManager.SaveforCustomerProfileExcel(dr, out message);
                }

                DataView dv = dt.DefaultView;
                //dv.Sort = "Message Desc";
                ViewData["ExcelData"] = dv.ToTable();
                successdt = dt;
                //message = string.Empty;
            }

            ViewData["MSG"] = message;
            return View("CustomerImport");
        }

        private bool SaveAndReadFile(out DataTable dt, out string msg)
        {
            dt = null;
            msg = "";
            if (Request.Files.Count <= 0)
            {
                return false;
            }

            try
            {
                HttpPostedFileBase pf = Request.Files[0];
                if (string.IsNullOrEmpty(pf.FileName.Trim()) && pf.FileName.Trim() == "")
                {
                    msg = "Please select file and upload.";
                    return false;
                }

                string userid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;
                string parentpath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"].ToString();

                string excelpath = parentpath + @"\CustomerImport\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\" + userid + @"\";

                if (Directory.Exists(excelpath) == false)
                {
                    Directory.CreateDirectory(excelpath);
                }
                string filename = pf.FileName;
                string excelfullname = excelpath + DateTime.Now.ToString("HHmmssffff") + '_' + filename;
                pf.SaveAs(excelfullname);

                DataSet ds = ExcelHelper.ReadCustomerExcel(excelfullname);
                if (ds == null || ds.Tables.Count <= 0) return false;

                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                msg = string.Format("Error:"+ex.Message);
                return false;
            }
            return true;

        }

        //下载模板
        public FileResult DownloadTemplate()
        {
            string filename = "TemplateForCustomerProfiles.xlsx"; 
            string path = Server.MapPath(@"~/tmp/" + filename);
            return File(path, "application/ms-excel", filename);
        }

        //Customer Name Auto Complete
        public ActionResult GetAuotCompleteValue()
        {
            string strSql = "SELECT TOP 10 Customer FROM SGP_CustomerProfile_Data WHERE Customer LIKE @term order by Customer ";
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Customer"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Customer"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }
    }
}
