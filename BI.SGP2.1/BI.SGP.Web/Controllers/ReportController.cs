using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using System.IO;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.Utils;

namespace BI.SGP.Web.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        public static DataTable dt{get;set;}
         [MyAuthorize]
        public ActionResult RFQHistory()
        {
            int RFQID = 0; Int32.TryParse(Request.QueryString["RFQID"], out RFQID);
            string RFQNumber = "";
            if (Request["RFQNumber"] != null)
            {
                RFQNumber = Request["RFQNumber"];
            }
            string strSQL = @"exec sp_SGP_RFQHistory @RFQID,@RFQNumber";

            List<SqlParameter> ParameterList = new List<SqlParameter>()
            {
                new SqlParameter ("@RFQID",RFQID),
                new SqlParameter ("@RFQNumber",RFQNumber)
            };

            dt = DbHelperSQL.Query(strSQL, ParameterList.ToArray()).Tables[0];

            ViewData["HistoryData"] = dt;
            return View();
        }

        public ActionResult DownloadReport() 
        {
            BI.SGP.BLL.DataModels.FieldInfoCollecton fields = BI.SGP.BLL.DataModels.FieldCategory.GetAllFieldsOrderbyCategoryID();

            DataTable fieldsdt = new DataTable();
            if (!fieldsdt.Columns.Contains("Name"))
            {
                DataColumn dcn = new DataColumn();
                dcn.ColumnName = "Name";
                fieldsdt.Columns.Add(dcn);
            }
            if (!fieldsdt.Columns.Contains("UpdateDate"))
            {
                DataColumn dcu = new DataColumn();
                dcu.ColumnName = "UpdateDate";
                fieldsdt.Columns.Add(dcu);
            }

            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fields)
            {
                DataColumn dc = new DataColumn();
                if (!fieldsdt.Columns.Contains(fi.FieldName))
                {
                    dc.ColumnName = fi.FieldName;
                    fieldsdt.Columns.Add(dc);
                }  
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = fieldsdt.NewRow();
                for (int j = 0; j < fieldsdt.Columns.Count; j++)
                {
                    if (dt.Columns.Contains(fieldsdt.Columns[j].ColumnName))
                    {
                        dr[fieldsdt.Columns[j].ColumnName] = dt.Rows[i][fieldsdt.Columns[j].ColumnName];
                    }
                }
                fieldsdt.Rows.Add(dr);
            }

            if (fieldsdt.Columns.Contains("Name"))
            {
                fieldsdt.Columns["Name"].ReadOnly = false;
                fieldsdt.Columns["Name"].ColumnName = "User Name";
            }

            if (fieldsdt.Columns.Contains("UpdateDate"))
            {
                fieldsdt.Columns["UpdateDate"].ReadOnly = false;
                fieldsdt.Columns["UpdateDate"].ColumnName = "Update On";
            }

            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fields)
            {
                if (fieldsdt.Columns.Contains(fi.FieldName))
                {
                    fieldsdt.Columns[fi.FieldName].ReadOnly = false;
                    fieldsdt.Columns[fi.FieldName].ColumnName = fi.DisplayName;
                }
            }

            fieldsdt.TableName = "RFQHistory";

            string filename = SGP.BLL.Export.ExcelHelper.DataTableToExcel(fieldsdt);

            return File(filename, "application/ms-excel", "ReportData_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx");
        }

        public ActionResult RFQHistory1()
        {
            return View();
        }
         [MyAuthorize]
        public ActionResult CostingCheckList()
        {
            ViewBag.Title = "Costing Checklist";
            ViewBag.GridGroup = "CostingChecklist";
            ViewBag.SearchGroup = "CostingCheckSearch";

            return View("Report");
        }
         [MyAuthorize]
        public ActionResult TATReport()
        {
            ViewBag.Title = "TAT Report";
            ViewBag.GridGroup = "TATGrid";
            ViewBag.SearchGroup = "TATSearch";

            return View("Report");
        }
        [MyAuthorize]
        public ActionResult FPCTATReport()
         {
             ViewBag.Title = "B2F TAT Report";
             ViewBag.GridGroup = "FPCTATGrid";
             ViewBag.SearchGroup = "FPCTATSearch";
            return View("Report");
         }
         [MyAuthorize]
        public ActionResult QuotesCompleted()
        {
            ViewBag.Title = "Quotes Completed";
            ViewBag.ProcName = "uspSGP_PricingAnalystQuotesReport";
            return View("StageSummary");
        }
         [MyAuthorize]
        public ActionResult TechnicalCostCompleted()
        {
            ViewBag.Title = "Technical Cost Completed";
            ViewBag.ProcName = "uspSGP_TechnicalQuotingQuotesReport";
            return View("StageSummary");
        }

        public string GetStageSummaryData()
        {
            StageSummary ss = new StageSummary(Request);
            return ss.ToJson();
        }

        public FileResult DownloadStageSumExcel()
        {
            StageSummary ss = new StageSummary(Request);
            RenderType renderType;
            if (Request["renderType"] == "2")
            {
                renderType = RenderType.Vertical;
            }
            else
            {
                renderType = RenderType.Horizontal;
            }
            string tempFile = ExcelHelper.DataTableToExcel(ss.Data, renderType);
            return File(tempFile, "application/ms-excel", "ExportFile.xlsx");
        }
    }
}
