using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.DataModels;
using SGP.DBUtility;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.WF;
using System.IO;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.Utils;

namespace BI.SGP.Web.Controllers
{
    public class ImportController : Controller
    {
        //
        // GET: /Import/

        public static DataTable successdt = new DataTable();
        [MyAuthorize]
        public ActionResult ImportExcel()
        {

            return View();
        }

        private bool SaveAndReadFile(out DataTable dt, out string msg)
        {
            dt = null;
            msg = "Please select file and upload.";
            if (Request.Files.Count <= 0) return false;
           
            try
            {
                HttpPostedFileBase pf = Request.Files[0];
                if (string.IsNullOrEmpty(pf.FileName.Trim()) && pf.FileName.Trim() == "") return false;
                if (pf == null) return false;

                string userid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;
                string parentpath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"].ToString();

                string excelpath = parentpath + @"\ImportExcel\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\" + userid+@"\";

                if (Directory.Exists(excelpath) == false)
                {

                    Directory.CreateDirectory(excelpath);
                }
                string filename = pf.FileName;
                string excelfullname = excelpath + DateTime.Now.ToString("HHmmssffff") + '_' + filename;
                pf.SaveAs(excelfullname);

                DataSet ds = ExcelHelper.ReadExcel(excelfullname);
                if (ds == null || ds.Tables.Count <= 0) return false;

                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                msg = string.Format("Upload exception, please select the xls or xlsx file to upload.");
                return false;
            }
            return true;

        }
        private void SaveData(DataTable dt)
        {

        }

        public ActionResult LoadExcel()
        {
            DataTable dt = new DataTable();
            string message = string.Empty;
            bool flag = SaveAndReadFile(out dt, out message);


            if (flag==true && dt != null)
            {

                FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);
                ModelHandler<object>.ColumnNameToFieldName(dt, fields);

                if (!dt.Columns.Contains("Message")) dt.Columns.Add("Message", typeof(string));
                if (!dt.Columns.Contains("RFQID")) dt.Columns.Add("RFQID", typeof(string));
                if (!dt.Columns.Contains("Number")) dt.Columns.Add("Number", typeof(string));
                if (!dt.Columns.Contains("ExtNumber")) dt.Columns.Add("ExtNumber", typeof(string));

                foreach (DataRow dr in dt.Rows)
                {
                    BI.SGP.BLL.Models.RFQManager.SaveforExcel(dr);
                }
                dt.Columns["RFQID"].SetOrdinal(0);
                dt.Columns["Message"].SetOrdinal(1);
                dt.Columns["Number"].SetOrdinal(2);
                dt.Columns["ExtNumber"].SetOrdinal(3);
                ModelHandler<object>.ColumnNameToDisplayName(dt, fields);

                DataView dv = dt.DefaultView;
                dv.Sort = "Message Desc";
                ViewData["ExcelData"] = dv.ToTable();
                successdt = dt;
                message = string.Empty;
            }

            ViewData["MSG"] = message;
            return View("ImportExcel");
        }

        public ActionResult B2FLoadExcel()
        {
            DataTable dt = new DataTable();
          
            string message = string.Empty;
            bool flag = SaveAndReadFile(out dt, out message);
            DataSet ds = dt.DataSet;

            if (flag == true && dt != null)
            {

                DataTable mainTable = ds.Tables["Primary"];

                FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_FPC);
                ModelHandler<object>.ColumnNameToFieldName(dt, fields);

                if (!dt.Columns.Contains("Message")) dt.Columns.Add("Message", typeof(string));
                if (!dt.Columns.Contains("RFQID")) dt.Columns.Add("RFQID", typeof(string));
                if (!dt.Columns.Contains("Number")) dt.Columns.Add("Number", typeof(string));
                if (!dt.Columns.Contains("ExtNumber")) dt.Columns.Add("ExtNumber", typeof(string));

                List<FieldCategory> categories = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_FPC);

                foreach (DataRow dr in mainTable.Rows)
                {
                    try
                    {
                        int dataId = 0;
                        string relationValue = "";
                        Dictionary<string, DataRow[]> dicSubData = new Dictionary<string, DataRow[]>();

                        foreach (FieldCategory fc in categories)
                        {
                            fc.ClearFieldsData();
                        }

                        if (mainTable.Columns.Contains("Number"))
                        {
                            relationValue = Convert.ToString(dr["Number"]);
                        }

                        for (int i = 0; i < ds.Tables.Count; i++)
                        {
                            string tableName = ds.Tables[i].TableName;
                            if (String.Compare(tableName, "DataSource", true) != 0 && String.Compare(tableName, "Primary", true) != 0)
                            {
                                DataTable subdt = ds.Tables[i];
                                DataRow[] subDrs = null;
                                if (!String.IsNullOrEmpty(relationValue))
                                {
                                    subDrs = subdt.Select(String.Format("[Internal RFQ Number]='{0}'", relationValue));
                                }
                                else
                                {
                                    subDrs = subdt.Select("1=1");
                                }

                                if (subDrs != null && subDrs.Length > 0)
                                {
                                    dicSubData.Add(tableName, subDrs);
                                }
                            }
                        }

                        B2FQuotationDetail dm = new B2FQuotationDetail(categories, dr, dicSubData);

                        if (!String.IsNullOrEmpty(relationValue))
                        {
                            string sSql = "SELECT ID FROM SGP_RFQ WHERE ExtNumber = @ExtNumber and TemplateID=2";
                            dataId = DbHelperSQL.GetSingle<int>(sSql, new SqlParameter("@ExtNumber", dr["Number"]));
                        }

                        string sql = "select ID,Number,ExtNumber from SGP_RFQ where ID=@ID";
                      

                        if (dataId > 0)
                        {
                            DataSet sgpds = DbHelperSQL.Query(sql, new SqlParameter("@ID", dataId));
                            if (sgpds.Tables.Count>0 && dt.Rows.Count > 0)
                            {
                                dr["RFQID"] = sgpds.Tables[0].Rows[0]["ID"];
                                dr["Number"] = sgpds.Tables[0].Rows[0]["Number"];
                                dr["ExtNumber"] = sgpds.Tables[0].Rows[0]["ExtNumber"];
                            }
                            
                            dm.Update(dataId);
                            

                        }
                        else
                        {
                            dataId = dm.Add();
                            DataSet sgpds = DbHelperSQL.Query(sql, new SqlParameter("@ID", dataId));
                            if (sgpds.Tables.Count > 0 && dt.Rows.Count > 0)
                            {
                                dr["RFQID"] = sgpds.Tables[0].Rows[0]["ID"];
                                dr["Number"] = sgpds.Tables[0].Rows[0]["Number"];
                                dr["ExtNumber"] = sgpds.Tables[0].Rows[0]["ExtNumber"];
                            }
                            

                        }
                    }
                    catch (Exception ex)
                    {
                        dr["Message"] = ex.Message;
                    }
                }
                   
             

                dt.Columns["RFQID"].SetOrdinal(0);
                dt.Columns["Message"].SetOrdinal(1);
                dt.Columns["Number"].SetOrdinal(2);
                dt.Columns["ExtNumber"].SetOrdinal(3);
                ModelHandler<object>.ColumnNameToDisplayName(dt, fields);

                DataView dv = dt.DefaultView;
                dv.Sort = "Message Desc";
                ViewData["ExcelData"] = dv.ToTable();
                successdt = dt;
                message = string.Empty;
            }

            ViewData["MSG"] = message;
            return View("~/Views/B2F/ImportExcel.cshtml");
        }

        public ActionResult SupplierRFQLoadExcel()
        {
            DataTable dt = new DataTable();

            string message = string.Empty;
            bool flag = SaveAndReadFile(out dt, out message);
            DataSet ds = dt.DataSet;
            List<string> numbers = new List<string>();

            if (flag == true && dt != null)
            {
                DataTable mainTable = ds.Tables["Primary"];

                FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGPFORSUPPLIER);
                ModelHandler<object>.ColumnNameToFieldName(dt, fields);

                if (!dt.Columns.Contains("Message")) dt.Columns.Add("Message", typeof(string));

                List<FieldCategory> categories = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SGPFORSUPPLIER);

                foreach (DataRow dr in mainTable.Rows)
                {
                    try
                    {
                        SupplierRFQDetail sr = new SupplierRFQDetail();
                        SystemMessages sysMsg = WFHelper.CreateMessages();
                        int i = sr.UpdateSupplierRFQByExcel(dr, categories);
                        if(i > 0)
                        {
                            string number = dr["NVARCHAR1"].ToString();
                            numbers.Add(number);
                            int id = sr.GetSupplierRFQId(number);
                            sr.AddSupplierRFQHistory(id, number, "Upload", sysMsg);
                            if (!sysMsg.isPass)
                            {
                                throw new Exception(sysMsg.Messages[0].ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dr["Message"] = ex.Message;
                    }
                }

                dt.Columns["Message"].SetOrdinal(0);
                ModelHandler<object>.ColumnNameToDisplayName(dt, fields);

                DataView dv = dt.DefaultView;
                dv.Sort = "Message Desc";
                ViewData["ExcelData"] = dv.ToTable();
                successdt = dt;
                message = string.Empty;
            }

            ViewData["MSG"] = message;
            ViewData["NUMBER"] = string.Join(",", numbers.ToArray());
            return View("~/Views/VVI/SupplierRFQImportExcel.cshtml");
        }

        public ActionResult SubmitData()
        {


            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();
            try
            {
                WFTemplate wfTemplate = new WFTemplate("DefaultWF", rfdetail.RFQID);
                sysmgs.Merge(wfTemplate.Run());
            }
            catch (Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("System Exception", ex.Message);
            }

            var returnData = new
            {
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadData()
        {


            //for (int i = successdt.Rows.Count - 1; i >= 0; i--)
            //{
            //    string ss = successdt.Rows[i]["Message"].ToString().Trim();
            //    if (string.IsNullOrEmpty(successdt.Rows[i]["Message"].ToString().Trim()) == false)
            //    {
            //        successdt.Rows.Remove(successdt.Rows[i]);
            //    }

            //}

            string filename = SGP.BLL.Export.ExcelHelper.DataTableToExcel(successdt);

            return File(filename, "application/ms-excel", "BatchUpload_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx");


        }




    }
}
