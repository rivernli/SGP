using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using SGP.DBUtility;
using System.IO;


namespace BI.SGP.Web.Controllers
{
    public class B2F_bakController : Controller
    {

        public ActionResult PricingView()
        {
            ViewBag.ExcelView = "~/Views/Export/ExportExcel.cshtml";
            ViewBag.GridGroup = "DefaultGrid";
            ViewBag.SearchGroup = "DefaultSearch";
            return View();
        }
        //
        // GET: /B2F/
        public ActionResult Detail(string RFQID)
        {
            int dataId = ParseHelper.Parse<int>(RFQID);

            WFTemplate template = new WFTemplate(2, dataId);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_B2F);

            if (dataId > 0)
            {
                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                b2Detail.FillCategoryData(lfc, dataId);
            }

            ViewBag.WFTemplate = template;
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            return View();
        }


        public ActionResult ImportExcel()
        {

            return View();
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

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_B2F);

                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                    }
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            BI.SGP.BLL.Models.B2FComputedField aftercomputeddata = new B2FComputedField();
                            aftercomputeddata.Data = data;

                            B2FQuotationDetail dm = new B2FQuotationDetail(lfc, aftercomputeddata.SetComputedValue());
                            if (operation == B2FQuotationDetail.OPERATION_REQUOTE)
                            {
                                id = dm.ReQuote();
                            }
                            else if (operation == B2FQuotationDetail.OPERATION_CLONE)
                            {
                                id = dm.Clone();
                            }
                            else
                            {
                                if (id > 0)
                                {
                                    dm.Update(id);
                                }
                                else
                                {
                                    id = dm.Add();
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
            }

            return id;
        }

        public ActionResult SaveData()
        {
            SystemMessages sysMsg = new SystemMessages();
            string html = "";
            string wfStatus = "";
            int id = 0;
            string postData = Request["postData"];
            id = Save(postData, sysMsg);
            
            if (id > 0 && sysMsg.isPass)
            {
                WFTemplate wfTemplate = new WFTemplate(2, id);
                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_B2F);
                b2Detail.FillCategoryData(lfc, id);
                html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
                wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            }
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }

        public ActionResult WFSkip()
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();
            string html = "";
            string wfStatus = "";

            string postData = Request["postData"];
            int id = Save(postData, sysMsg);

            if (sysMsg.isPass && id > 0)
            {
                List<int> lstToChildIds = new List<int>();
                string templateName = Request["templateName"];
                string toChildIds = Request["toChildIds"];
                int entityId = id;
                int fromId = ParseHelper.Parse<int>(Request["fromId"]);
                int toId = ParseHelper.Parse<int>(Request["toId"]);
                bool checkData = !(Request["checkData"] == "false");
                bool waitAllChildComplated = !(Request["waitAllChildComplated"] == "false");

                if (!String.IsNullOrWhiteSpace(toChildIds))
                {
                    foreach (string stcid in toChildIds.Split(','))
                    {
                        int tcid = ParseHelper.Parse<int>(stcid);
                        if (tcid > 0)
                        {
                            lstToChildIds.Add(Convert.ToInt32(tcid));
                        }
                    }
                }

                WFTemplate wfTemplate = new WFTemplate(templateName, entityId);

                using (TScope ts = new TScope())
                {
                    if (toId == 0)
                    {
                        sysMsg.Merge(wfTemplate.Run(fromId, checkData));
                    }
                    else
                    {
                        sysMsg.Merge(wfTemplate.Skip(toId, fromId, checkData, waitAllChildComplated, lstToChildIds.ToArray()));
                    }

                    if (!sysMsg.isPass)
                    {
                        ts.Rollback();
                    }
                }

                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_B2F);
                b2Detail.FillCategoryData(lfc, id);
                html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
                wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            }
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }


        public ActionResult GetComputedValue(string postData)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            List<double> UnitPrice = DetailUIHelper.SubDataToList<double>(jsonData["UnitPrice"]);
            List<string> UnitType = DetailUIHelper.SubDataToList<string>(jsonData["UnitType"]);
            List<double> LayerCount = DetailUIHelper.SubDataToList<double>(jsonData["LayerCount"]);
            List<double> UnitPerArray = DetailUIHelper.SubDataToList<double>(jsonData["UnitPerArray"]);
            List<double> ArraySizeWidth = DetailUIHelper.SubDataToList<double>(jsonData["ArraySizeWidth"]);
            List<double> ArraySizeLength = DetailUIHelper.SubDataToList<double>(jsonData["ArraySizeLength"]);
            List<double> PanelSizeWidth = DetailUIHelper.SubDataToList<double>(jsonData["PanelSizeWidth"]);
            List<double> PanelSizeLength = DetailUIHelper.SubDataToList<double>(jsonData["PanelSizeLength"]);
            List<double> UnitPerWorkingPanel = DetailUIHelper.SubDataToList<double>(jsonData["UnitPerWorkingPanel"]);
            List<double> VariableCost = DetailUIHelper.SubDataToList<double>(jsonData["VariableCost"]);
            List<double> FixedCost = DetailUIHelper.SubDataToList<double>(jsonData["FixedCost"]);
            List<double> TotalCost = DetailUIHelper.SubDataToList<double>(jsonData["TotalCost"]);

            for(int i=0; i<TotalCost.Count;i++)
            {

                TotalCost[i] = FixedCost[i] + VariableCost[i];
                
            }


            var jsonObject = new
            {
                TotalCost=TotalCost
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult SaveExcel(DataSet ds)
        //{
        //   // DataSet ds = ExcelHelper.ReadExcel(@"D:\Files\SGP\Technical_Costing_template.xlsx");
        //    DataTable mainTable = ds.Tables["Primary"];
        //    List<FieldCategory> categories = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_B2F);
        //    foreach (DataRow dr in mainTable.Rows)
        //    {
        //        int dataId = 0;
        //        string relationValue = "";
        //        Dictionary<string, DataRow[]> dicSubData = new Dictionary<string, DataRow[]>();

        //        foreach (FieldCategory fc in categories)
        //        {
        //            fc.ClearFieldsData();
        //        }
                
        //        if (mainTable.Columns.Contains("RFQ Number"))
        //        {
        //            relationValue = Convert.ToString(dr["RFQ Number"]);
        //        }
                
        //        for (int i = 0; i < ds.Tables.Count; i++)
        //        {
        //            string tableName = ds.Tables[i].TableName;
        //            if (String.Compare(tableName, "DataSource", true) != 0 && String.Compare(tableName, "Primary", true) != 0)
        //            {
        //                DataTable dt = ds.Tables[i];
        //                DataRow[] subDrs = null;
        //                if (!String.IsNullOrEmpty(relationValue))
        //                {
        //                    subDrs = dt.Select(String.Format("[RFQ Number]='{0}'", relationValue));
        //                }
        //                else
        //                {
        //                    subDrs = dt.Select("1=1");
        //                }

        //                if (subDrs != null && subDrs.Length > 0)
        //                {
        //                    dicSubData.Add(tableName, subDrs);
        //                }
        //            }
        //        }

        //        B2FQuotationDetail dm = new B2FQuotationDetail(categories, dr, dicSubData);

        //        if (!String.IsNullOrEmpty(relationValue))
        //        {
        //            string sSql = "SELECT ID FROM SGP_RFQ WHERE ExtNumber = @ExtNumber";
        //            dataId = DbHelperSQL.GetSingle<int>(sSql, new SqlParameter("@ExtNumber", dr["RFQ Number"]));
        //        }
                
        //        if (dataId > 0)
        //        {
        //            dm.Update(dataId);
        //        }
        //        else
        //        {
        //            dataId = dm.Add();
        //        }
        //    }

        //    return View();
        //}
        public FileResult DownloadPDF()
        {
            string KeyValues = Request.QueryString["RFQID"];
            string fileName = "";
            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/" + Guid.NewGuid() + ".tmp");
            MemoryStream mem = new MemoryStream();
            if (!String.IsNullOrEmpty(KeyValues))
            {
                KeyValues = KeyValues.TrimEnd(',');
                string[] ids = KeyValues.Split(',');
                if (ids.Length == 1)
                {
                    int id = Int32.Parse(ids[0]);
                    B2FPDFDownLoad pdf = new B2FPDFDownLoad();
                 
                    if (pdf.WriterPDF(ref mem, id, out fileName))
                    {

                        if (fileName == "")
                        {
                            fileName = "multek_" + DateTime.Now.ToString("mmssffff") + ".pdf";
                        }
                        else
                        {
                            fileName += ".pdf";
                        }
                        using (var fileStream = FileHelper.CreateFile(tempFile))
                        {
                            fileStream.Write(mem.GetBuffer(), 0, mem.GetBuffer().Length);
                        }
                    }
                }
                else
                {
                    ZipHelper.B2FPDFToZip(ids, mem);
                    fileName = "PDF_Packages_" + DateTime.Now.ToString("mmss") + ".zip";
                    using (var fileStream = FileHelper.CreateFile(tempFile))
                    {
                        mem.WriteTo(fileStream);
                    }
                }
            }

            return File(tempFile, "application/octet-stream", fileName);
        }
    }
}
