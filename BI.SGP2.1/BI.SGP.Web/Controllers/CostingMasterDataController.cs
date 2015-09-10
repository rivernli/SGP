using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class CostingMasterDataController : Controller
    {
        //
        // GET: /CostingMasterData/

        public ActionResult Query()
        {
            return View();
        }

        public ActionResult Index()
        {
            string strSql = "SELECT * FROM SCS_TableParams ORDER BY Sort";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            ViewBag.TableData = dt;
            return View();
        }

        public ActionResult List(string ID)
        {
            TableParams tp = new TableParams(ID);
            ViewBag.Category = new FieldCategory(ID);
            ViewBag.TableParams = tp;
            ViewBag.Title = "<li class='active'>" + tp.DisplayName + "</li>";
            return View();
        }

        public ActionResult History(string ID)
        {
            TableParams tp = new TableParams(ID);
            ViewBag.Category = new FieldCategory(ID);
            ViewBag.TableParams = tp;
            ViewBag.Title = "<li>History</li>&nbsp;<li class='active'>" + tp.DisplayName + "</li>";
            ViewBag.History = "yes";
            return View("List");
        }

        public ActionResult Detail()
        {
            return View();
        }

        public ActionResult MasterData()
        {
            ViewBag.TableType = 1;
            ViewBag.Title = "<li class='active'>Master Data</li>";
            return View();
        }

        public ActionResult VersionData()
        {
            ViewBag.TableType = 2;
            ViewBag.Title = "<li class='active'>Version Data</li>";
            return View("MasterData");
        }

        public ActionResult PriceMaster()
        {
            ViewBag.TableType = 3;
            ViewBag.Title = "<li class='active'>Price Master</li>";
            return View("MasterData");
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

                using (TScope ts = new TScope())
                {
                    try
                    {
                        CostingMasterDetailData cmdd = new CostingMasterDetailData(category, data);
                        category.CheckDataType(data, sysMsg);
                        cmdd.CheckData(sysMsg);

                        if (sysMsg.isPass)
                        {
                            if (id > 0)
                            {
                                cmdd.Update(id, true);
                            }
                            else
                            {
                                id = cmdd.Add();
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

        public JsonResult SaveOtherData()
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

                using (TScope ts = new TScope())
                {
                    try
                    {
                        CostingOtherDetailData codd = new CostingOtherDetailData(category, data);
                        category.CheckDataType(data, sysMsg);
                        codd.CheckData(sysMsg);

                        if (sysMsg.isPass)
                        {
                            if (id > 0)
                            {
                                codd.Update(id);
                            }
                            else
                            {
                                id = codd.Add();
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
                    CostingMasterDetailData cmdd = new CostingMasterDetailData(category);

                    if (id > 0)
                    {
                        cmdd.Delete(id);
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

        public JsonResult DelOtherData()
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
                    CostingOtherDetailData codd = new CostingOtherDetailData(category);

                    if (id > 0)
                    {
                        codd.Delete(id);
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

        public JsonResult BatchDelete()
        {
            SystemMessages sysMsg = new SystemMessages();
            string categoryName = Request["categoryName"];
            FieldCategory category = new FieldCategory(categoryName);

            using (TScope ts = new TScope())
            {
                try
                {
                    CostingMasterDetailData cmdd = new CostingMasterDetailData(category);
                    cmdd.BatchDelete();
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

        public ActionResult ProcFlowGroup()
        {
            return View();
        }

        public string ProcFlowList()
        {
            string strWhere = "";
            List<SqlParameter> lstSearchParams = new List<SqlParameter>();
            string searchValue = Request["GroupName"];
            if (!String.IsNullOrEmpty(searchValue))
            {
                strWhere += " AND GroupName LIKE @GroupName";
                lstSearchParams.Add(new SqlParameter("@GroupName", "%" + searchValue + "%"));
            }

            searchValue = Request["Layer"];
            if (!String.IsNullOrEmpty(searchValue))
            {
                strWhere += " AND Layer LIKE @Layer";
                lstSearchParams.Add(new SqlParameter("@Layer", "%" + searchValue + "%"));
            }

            searchValue = Request["WorkCenter"];
            if (!String.IsNullOrEmpty(searchValue))
            {
                strWhere += " AND WorkCenter LIKE @WorkCenter";
                lstSearchParams.Add(new SqlParameter("@WorkCenter", "%" + searchValue + "%"));
            }

            searchValue = Request["Remark"];
            if (!String.IsNullOrEmpty(searchValue))
            {
                strWhere += " AND Remark LIKE @Remark";
                lstSearchParams.Add(new SqlParameter("@Remark", "%" + searchValue + "%"));
            }

            string strSql = String.Format("SELECT * FROM SCM_ProcFlowGroup WHERE 1=1{0} ", strWhere);

            GridData gridData = GridManager.GetGridData("GroupName,Sort,ID", "", Request["page"], Request["rows"], strSql, lstSearchParams.ToArray());

            return gridData.ToJson();
        }

        public string GetGridData()
        {
            FieldGroup fieldGroup = new FieldGroup(Request["groupName"]);
            TableParams tableParams = new TableParams(Request["groupName"]);
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

            formatString.Add(new TableFormatString("ExpiryDate", "{0:d-MMM-yyyy HH:mm:ss}"));

            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fields, extSqlColumns);

            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = Request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(Request, searchGroupName, listParames);
            }

            if (tableParams.TableType == TableParams.TableType_PriceMaster)
            {
                if (Request["historyPage"] == "yes")
                {
                    strSql += " WHERE ExpiryDate < GETDATE() " + strWhere;
                }
                else
                {
                    strSql += " WHERE EffectiveDate < GETDATE() AND ExpiryDate > GETDATE() " + strWhere;
                }
            }
            else
            {
                strSql += " WHERE 1=1" + strWhere;
            }

            GridData gridData = GridManager.GetGridData(Request, strSql, listParames.ToArray());

            return gridData.ToJson(formatString.ToArray());
        }

        public string GetTableParams(int ID)
        {
            string desc = Request["Description"];
            string strWhere = "";
            List<SqlParameter> lstSearchParams = new List<SqlParameter>();
            if (!String.IsNullOrEmpty(desc))
            {
                strWhere += "AND DisplayName LIKE @DisplayName";
                lstSearchParams.Add(new SqlParameter("@DisplayName", "%" + desc + "%"));
            }

            string strSql = String.Format("SELECT * FROM SCS_TableParams WHERE TableType={0} {1} ORDER BY Sort", ID, strWhere);

            DataTable dt = DbHelperSQL.Query(strSql, lstSearchParams.ToArray()).Tables[0];
            dt.Columns.Add("Version");
            dt.Columns.Add("RecordCount");
            dt.Columns.Add("LastUpdateTime");
            dt.Columns.Add("LastUpdateBy");

            string vop = "";

            if (ID == TableParams.TableType_VersionData || ID == TableParams.TableType_MasterData)
            {
                strSql = "SELECT TOP 1 Version FROM SCM_Version ORDER BY CASE WHEN Status = 'Active' THEN 1 ELSE 2 END, ID DESC";
                vop = Convert.ToString(DbHelperSQL.GetSingle(strSql));
            }
            else if (ID == TableParams.TableType_PriceMaster)
            {
                strSql = "SELECT TOP 1 Period FROM SCM_Period ORDER BY CASE WHEN Status = 'Active' THEN 1 ELSE 2 END, ID DESC";
                vop = Convert.ToString(DbHelperSQL.GetSingle(strSql));
            }

            SqlParameter vopps = new SqlParameter("@VersionOrPeriod", vop);

            DataTable dtLastUpdate = null;

            foreach (DataRow dr in dt.Rows)
            {
                dr["Version"] = vop;
                int tableType = ParseHelper.Parse<int>(dr["TableType"]);
                if (tableType == TableParams.TableType_MasterData)
                {
                    strSql = String.Format("SELECT COUNT(*) FROM {0}", dr["TableName"]);
                    dr["RecordCount"] = DbHelperSQL.GetSingle<int>(strSql);

                    strSql = String.Format(@"SELECT TOP 1 * FROM (
	                                            SELECT TOP 1 CreationTime,CreatorName FROM {0}
	                                            UNION ALL 
	                                            SELECT DateTime, UpdateBy FROM SCS_DataLog WHERE TableKey = '{1}' AND DataID IN(SELECT ID FROM {0})
                                            ) AS t ORDER BY CreationTime DESC", dr["TableName"], dr["TableKey"]);
                    dtLastUpdate = DbHelperSQL.Query(strSql).Tables[0];

                }
                else if (tableType == TableParams.TableType_VersionData)
                {
                    strSql = String.Format("SELECT COUNT(*) FROM {0} WHERE Version=@VersionOrPeriod", dr["TableName"]);
                    dr["RecordCount"] = DbHelperSQL.GetSingle<int>(strSql, vopps);

                    strSql = String.Format(@"SELECT TOP 1 * FROM (
	                                            SELECT TOP 1 CreationTime,CreatorName FROM {0} WHERE Version=@VersionOrPeriod
	                                            UNION ALL 
	                                            SELECT DateTime, UpdateBy FROM SCS_DataLog WHERE TableKey = '{1}' AND DataID IN(SELECT ID FROM {0} WHERE Version=@VersionOrPeriod)
                                            ) AS t ORDER BY CreationTime DESC", dr["TableName"], dr["TableKey"]);
                    dtLastUpdate = DbHelperSQL.Query(strSql, vopps).Tables[0];
                }
                else if (tableType == TableParams.TableType_PriceMaster)
                {
                    strSql = String.Format("SELECT COUNT(*) FROM {0} WHERE Period=@VersionOrPeriod AND ExpiryDate > GETDATE()", dr["TableName"]);
                    dr["RecordCount"] = DbHelperSQL.GetSingle<int>(strSql, vopps);
                    strSql = String.Format("SELECT TOP 1 EffectiveDate AS CreationTime,CreatorName FROM {0} WHERE Period=@VersionOrPeriod ORDER BY EffectiveDate DESC", dr["TableName"]);
                    dtLastUpdate = DbHelperSQL.Query(strSql, vopps).Tables[0];
                }

                if (dtLastUpdate != null && dtLastUpdate.Rows.Count > 0)
                {
                    dr["LastUpdateTime"] = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dtLastUpdate.Rows[0]["CreationTime"]);
                    dr["LastUpdateBy"] = String.Format("{0}", dtLastUpdate.Rows[0]["CreatorName"]);
                }
            }

            GridData gridData = new GridData();
            gridData.Total = dt.Rows.Count;
            gridData.Records = dt.Rows.Count;
            gridData.DataTable = dt;
            gridData.Page = "1";

            return gridData.ToJson();
        }

        public JsonResult GenrateCategory()
        {
            string html = "";
            string errMessage = "";
            int id = 0;
            string dataId = Request["dataId"];
            string categoryName = Request["categoryName"];
            Int32.TryParse(dataId, out id);

            try
            {
                FieldCategory category = new FieldCategory(categoryName);
                if (id > 0)
                {
                    CostingMasterDetailData cmdd = new CostingMasterDetailData(category);
                    cmdd.FillCategoryData(id);
                }
                
                html = BI.SGP.BLL.UIManager.CostingMasterDataDetailHelper.GenrateCategory(category);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                html = html,
                errMessage = errMessage
            };
            return Json(result);
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

        public FileResult DownloadExcel()
        {
            FieldGroup fieldGroup = new FieldGroup(Request.QueryString["excelList"]);
            TableParams tableParams = new TableParams(fieldGroup.GroupName);
            FieldGroupDetailCollection fields = fieldGroup.GetDefaultFields();
            string strSql = "SELECT ";
            string strWhere = " WHERE 1=1 ";
            List<SqlParameter> listParames = new List<SqlParameter>();

            if (tableParams.TableType == TableParams.TableType_PriceMaster)
            {
                strSql += "ID,";
                if (Request["historyPage"] == "yes")
                {
                    strSql += "ExpiryDate AS [Expiry Date], ";
                    strWhere += " AND ExpiryDate < GETDATE() ";
                }
                else
                {
                    strWhere += " AND EffectiveDate < GETDATE() AND ExpiryDate > GETDATE() ";
                }
            }

            foreach (FieldGroupDetail field in fields)
            {
                if (field.DataType == FieldInfo.DATATYPE_SUMMARY)
                {
                    strSql += String.Format("({0}) AS [{1}],", field.KeyValueSource, field.DisplayName);
                }
                else
                {
                    strSql += String.Format("[{0}] AS [{1}],", field.FieldName, field.DisplayName);
                }
            }

            if (tableParams.TableType == TableParams.TableType_PriceMaster && Request["historyPage"] == "yes")
            {
                strSql += "CreatorName AS Creator,";
            }

            strSql = strSql.TrimEnd(',') + String.Format(" FROM {0} AS t", fieldGroup.SourceName);
            
            string searchGroupName = Request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(Request, searchGroupName, listParames);
            }

            strSql = strSql + strWhere;

            DataSet ds = DbHelperSQL.Query(strSql, listParames.ToArray());

            RenderType rt = Request.QueryString["renderType"] == "2" ? RenderType.Vertical : RenderType.Horizontal;

            string tempFile = ExcelHelper.DataSetToExcel(ds, rt);
            return File(tempFile, "application/ms-excel", tableParams.DisplayName.Replace(" ", "_") + ".xlsx");
        }

        public FileResult DownloadTemplate(string ID) 
        {
            TableParams tp = new TableParams(ID);
            string tempFile = ExcelHelper.ExportSCMasterTemplate(ID);
            return File(tempFile, "application/ms-excel", tp.DisplayName.Replace(" ", "_") + ".xlsx");
        }

        public ActionResult UploadFile()
        {
            string tableKey = Request["tableKey"];
            HttpPostedFileBase file = Request.Files["Filedata"];
            SystemMessages sysMsg = new SystemMessages();
            using (TScope ts = new TScope())
            {
                try
                {
                    DataSet ds = ExcelHelper.ReadExcel(file.InputStream, true);
                    if (ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        FieldCategory fc = new FieldCategory(tableKey);
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
                                CostingMasterDetailData cmdd = new CostingMasterDetailData(fc, dicData);
                                fc.CheckDataType(dicData, sysMsg);
                                cmdd.CheckData(sysMsg);
                                if (sysMsg.isPass)
                                {
                                    int id = dt.Columns.Contains("ID") ? ParseHelper.Parse<int>(dr["ID"]) : 0;
                                    if (id > 0)
                                    {
                                        cmdd.Update(id);
                                    }
                                    else
                                    {
                                        cmdd.Add();
                                    }
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
            }
            var result = new
            {
                success = sysMsg.isPass,
                errMessage = sysMsg.MessageString
            };
            return Json(result);
        }
    }
}
