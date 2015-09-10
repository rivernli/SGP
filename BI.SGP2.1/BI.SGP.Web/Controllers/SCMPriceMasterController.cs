using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.WF;
using SGP.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BI.SGP.Web.Controllers
{
    public class SCMPriceMasterController : Controller
    {
        //
        // GET: /SCMPriceMaster/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SCMView()
        {
            ViewBag.PageType = "";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "SCMPriceMaster";
            ViewBag.GridGroup = "SCMPriceMasterGrid";
            ViewBag.SearchGroup = "SCMPriceMasterSearch";
            return View();
        }

        public ActionResult SCMViewLaminate()
        {
            ViewBag.PageType = "Laminate";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "<li class='active'>Laminate</li>";
            ViewBag.GridGroup = "SCMPMLaminateGrid";
            ViewBag.SearchGroup = "SCMPMLaminateSearch";
            return View("SCMView");
        }

        public ActionResult SCMViewPrepreg()
        {
            ViewBag.PageType = "Prepreg";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "<li class='active'>Prepreg</li>";
            ViewBag.GridGroup = "SCMPMPrepregGrid";
            ViewBag.SearchGroup = "SCMPMPrepregSearch";
            return View("SCMView");
        }

        public ActionResult QuoteApprove()
        {
            ViewBag.PageType = "Approval";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "<li class='active'>Approval</li>";
            ViewBag.GridGroup = "SCMPMApprovalGrid";
            ViewBag.SearchGroup = "SCMPMApprovalSearch";
            return View("SCMView");
        }

        public ActionResult SCMViewVendor()
        {
            ViewBag.PageType = "Vendor";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "<li class='active'>Quote</li>";
            ViewBag.GridGroup = "SCMPMVendorGrid";
            ViewBag.SearchGroup = "SCMPMVendorSearch";
            return View("SCMView");
        }

        public ActionResult QuoteReport()
        {
            ViewBag.PageType = "Report";
            ViewBag.CategoryName = "SCMPriceMaster";
            ViewBag.Title = "<li class='active'>Report</li>";
            ViewBag.GridGroup = "SCMPMReportGrid";
            ViewBag.SearchGroup = "SCMPMReportSearch";
            return View("SCMView");
        }

        public ActionResult GenerateQuery()
        {
            string groupName = Request["listName"];
            StringBuilder strResult = new StringBuilder();
            strResult.Append("<table style='width:100%' id='query-" + groupName + "'><tr>");

            FieldGroup fieldGroup = new FieldGroup(groupName, "Search");
            FieldGroupDetailCollection fieldDetails = fieldGroup.GetDefaultFields();
            int detailCount = fieldDetails.Count < 4 ? 4 : fieldDetails.Count;

            for (int i = 0; i < detailCount; i++)
            {
                if (i > 1 && (i % 4) == 0)
                {
                    strResult.Append("</tr><tr>");
                }
                if (i >= fieldDetails.Count)
                {
                    strResult.Append("<td style='width:10%' align='right'>&nbsp;</td><td style='width:15%'></td>");
                }
                else
                {
                    strResult.AppendFormat("<td style='width:10%' align='right'>&nbsp;{0}&nbsp;</td><td style='width:15%'>", fieldDetails[i].DisplayName);
                    switch (fieldDetails[i].DataType)
                    {
                        case BLL.DataModels.FieldInfo.DATATYPE_DATETIME:
                        case BLL.DataModels.FieldInfo.DATATYPE_DATE:
                            strResult.Append(UIManager.GenerateQueryDateRange(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_INT:
                        case BLL.DataModels.FieldInfo.DATATYPE_FLOAT:
                        case BLL.DataModels.FieldInfo.DATATYPE_DOUBLE:
                            strResult.Append(UIManager.GenerateQueryNumberBox(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_LIST:
                            strResult.Append(UIManager.GenerateQueryList(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_ACTIVITY:
                            strResult.Append(UIManager.GenerateQueryActivity(fieldDetails[i]));
                            break;
                        case "ext":
                            strResult.Append(UIManager.GenerateQueryExt(fieldDetails[i]));
                            break;
                        default:
                            strResult.Append(UIManager.GenerateQueryTextBox(fieldDetails[i]));
                            break;
                    }
                    strResult.Append("</td>");
                }
            }
            strResult.AppendFormat("</tr></table><input type='hidden' id='searchGroup' name='searchGroup' value='{0}' />", fieldGroup.GroupName);
            return Content(strResult.ToString());
        }

        public string GetGridData()
        {
            FieldGroup fieldGroup = new FieldGroup(Request["groupName"]);
            string pageType = Request["pageType"];
            string[] extSqlColumns = String.IsNullOrEmpty(Request["extSqlColumns"]) ? null : Request["extSqlColumns"].Split(',');
            FieldGroupDetailCollection fields = fieldGroup.GetDefaultFields();
            List<TableFormatString> formatString = new List<TableFormatString>();
            foreach (FieldGroupDetail field in fields)
            {
                if (!String.IsNullOrEmpty(field.Format))
                {
                    formatString.Add(new TableFormatString(field.FieldName, field.Format));
                }
            }
            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fields, extSqlColumns);

            string strWhere;
            List<SqlParameter> listParames;
            GenerateStrWhere(pageType, out strWhere, out listParames);
            strSql += " WHERE 1=1" + strWhere;

            GridData gridData = GridManager.GetGridData(Request, strSql, listParames.ToArray());
            return gridData.ToJson(formatString.ToArray());
        }

        private void GenerateStrWhere(string pageType, out string strWhere, out List<SqlParameter> listParames)
        {
            strWhere = "";
            listParames = new List<SqlParameter>();
            string searchGroupName = Request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(Request, searchGroupName, listParames);
            }

            if (pageType == "Vendor")
            {
                strWhere += " AND VendorCode=@VendorCode";
                listParames.Add(new SqlParameter("@VendorCode", "RZ0220"));
            }
        }

        /// <summary>
        /// 报价审批通过
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public ActionResult ApproveQuote(string keyValues)
        {
            SystemMessages sysMsg = new SystemMessages();
            if (!string.IsNullOrEmpty(keyValues))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
                        string[] ids = keyValues.Split(',');
                        if (ids != null && ids.Length > 0)
                        {
                            DateTime approveDate = DateTime.Now;
                            foreach (string id in ids)
                            {
                                smd.ChangeQuoteStatus(id, SCMPriceMasterDetail.QuoteStatus_Approved, approveDate);
                                UpdateMainDataStatus(id);
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
            var result = new
            {
                success = sysMsg.isPass,
                message = (sysMsg.isPass ? "" : sysMsg.Messages.ToString())
            };
            return Json(result);
        }

        private void UpdateMainDataStatus(string id)
        {
            SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
            int mainId = smd.GetMainId(id);
            if (mainId > 0)
            {
                smd.UpdateMainQuoteRecord(mainId, id);
                smd.ChangeRequestStatus(mainId.ToString(), SCMPriceMasterDetail.RequestStatus_Free);
            }
        }

        /// <summary>
        /// 退回报价
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public ActionResult RejectQuote(string keyValues)
        {
            SystemMessages sysMsg = new SystemMessages();
            if (!string.IsNullOrEmpty(keyValues))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
                        string[] ids = keyValues.Split(',');
                        if (ids != null && ids.Length > 0)
                        {
                            DateTime date = DateTime.Now;
                            foreach (string id in ids)
                            {
                                smd.ChangeQuoteStatus(id, SCMPriceMasterDetail.QuoteStatus_Reject, date);
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
            var result = new
            {
                success = sysMsg.isPass,
                message = (sysMsg.isPass ? "" : sysMsg.Messages.ToString())
            };
            return Json(result);
        }

        public ActionResult CancelQuote(string keyValues)
        {
            SystemMessages sysMsg = new SystemMessages();
            if (!string.IsNullOrEmpty(keyValues))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        string[] ids = keyValues.Split(',');
                        if (ids != null && ids.Length > 0)
                        {
                            SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
                            DateTime date = new DateTime();
                            foreach (string id in ids)
                            {
                                smd.DisableQuote(id, date);
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
            var result = new
            {
                success = sysMsg.isPass,
                message = (sysMsg.isPass ? "" : sysMsg.Messages.ToString())
            };
            return Json(result);
        }

        public ActionResult RequestToVendor(string KeyValues)
        {
            SystemMessages sysMsg = new SystemMessages();
            if (!string.IsNullOrEmpty(KeyValues))
            {
                using (TScope ts = new TScope())
                {
                    try
                    {
                        string[] ids = KeyValues.Split(',');
                        DateTime date = DateTime.Now;
                        if (ids != null && ids.Length > 0)
                        {
                            SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
                            foreach (string id in ids)
                            {
                                if (!smd.CheckIsFree(id))
                                {
                                    throw new Exception("Cannot request a data in process!");
                                }
                                smd.CreateNewRequestToVendor(id, date);
                                smd.ChangeRequestStatus(id, SCMPriceMasterDetail.RequestStatus_InProcess);
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
            var result = new
            {
                success = sysMsg.isPass,
                message = (sysMsg.isPass ? "" : sysMsg.Messages.ToString())
            };
            return Json(result);
        }

        public ActionResult UploadFile()
        {
            string tableKey = Request["tableKey"];
            string pageType = Request["pageType"];
            HttpPostedFileBase file = Request.Files["Filedata"];
            SystemMessages sysMsg = new SystemMessages();
            using (TScope ts = new TScope())
            {
                try
                {
                    DataSet ds = ExcelHelper.ReadExcel(file.InputStream, true);
                    if (ds.Tables.Count > 0)
                    {
                        DateTime date = DateTime.Now;
                        DataTable dt = ds.Tables[0];
                        FieldCategory fc = new FieldCategory(tableKey, pageType);
                        foreach (DataRow dr in dt.Rows)
                        {
                            fc.ClearAllFieldsData();
                            FieldInfoCollecton fields = fc.VisibleFields;
                            Dictionary<string, object> dicData = new Dictionary<string, object>();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                var fi = fields.Where(p => p.DisplayName == dc.ColumnName).SingleOrDefault();
                                if (fi != null)
                                {
                                    dicData.Add(fi.FieldName, dr[dc.ColumnName]);
                                }
                            }

                            if (dicData.Count == 0)
                            {
                                throw new Exception("Upload Error: can not match data.");
                            }
                            else
                            {
                                SCMPriceMasterDetail smd = new SCMPriceMasterDetail(fc, dicData, pageType);
                                fc.CheckDataType(dicData, sysMsg);
                                smd.CheckData(sysMsg);
                                if (sysMsg.isPass)
                                {
                                    smd.InsertUploadFile(date);
                                }
                                else
                                {
                                    ts.Rollback();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }
                var result = new
                {
                    success = sysMsg.isPass,
                    errMessage = sysMsg.MessageString
                };
                return Json(result);
            }
        }

        public FileResult DownloadExcel()
        {
            string groupName = Request.QueryString["groupName"];
            string pageType = Request.QueryString["pageType"];
            FieldGroup fieldGroup = new FieldGroup(groupName);
            FieldGroupDetailCollection fields = fieldGroup.GetDefaultFields();

            string strSql = "select ";
            string strWhere = " where 1=1 ";
            List<SqlParameter> parameters = new List<SqlParameter>();

            foreach (FieldGroupDetail field in fields)
            {
                strSql += String.Format("[{0}] AS [{1}],", field.FieldName, field.DisplayName);
            }

            strSql = strSql.TrimEnd(',') + String.Format(" FROM {0} AS t", fieldGroup.SourceName);

            string searchGroupName = Request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(Request, searchGroupName, parameters);
            }

            strSql = strSql + strWhere;

            DataSet ds = DbHelperSQL.Query(strSql, parameters.ToArray());

            RenderType rt = Request.QueryString["renderType"] == "2" ? RenderType.Vertical : RenderType.Horizontal;

            string tempFile = ExcelHelper.DataSetToExcel(ds, rt);
            return File(tempFile, "application/ms-excel", pageType + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xlsx");
        }

        public FileResult ExportReport()
        {
            string categoryName = Request.QueryString["categoryName"];
            SCMPriceMasterDetail smd = new SCMPriceMasterDetail();
            DataSet ds = smd.GetReportData(Request, categoryName);
            if (ds != null && ds.Tables.Count > 0)
            {
                string tempFile = ExcelHelper.DataSetToExcel(ds, RenderType.Horizontal);
                return File(tempFile, "application/ms-excel", "PriceMasterReport_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".xlsx");
            }
            return null;
        }
    }
}
