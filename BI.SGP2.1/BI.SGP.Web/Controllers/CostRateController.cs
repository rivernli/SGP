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
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Export;
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class CostRateController : Controller
    {
        //
        // GET: /CostRate/

        public ActionResult List()
        {
            ViewBag.ExcelView = "~/Views/Shared/_ExportExcel.cshtml";
            return View();
        }

        public ActionResult GenerateQuery()
        {
            string groupName = Request["listName"];
            StringBuilder strResult = new StringBuilder();
            strResult.Append("<table style='width:100%' id='query-" + groupName + "'><tr>");

            FieldGroup fieldGroup = new FieldGroup(groupName, "Search");

            FieldGroupDetailCollection fieldDetails = fieldGroup.GetDefaultFields();

            int detailCount = fieldDetails.Count < 3 ? 3 : fieldDetails.Count;

            for (int i = 0; i < detailCount; i++)
            {
                if (i > 1 && (i % 3) == 0)
                {
                    strResult.Append("</tr><tr>");
                }
                if (i >= fieldDetails.Count)
                {
                    strResult.Append("<td style='width:10%' align='right'>&nbsp;</td><td style='width:23%'></td>");
                }
                else
                {
                    strResult.AppendFormat("<td style='width:10%;{1}' align='right'>&nbsp;{0}&nbsp;</td><td style='width:23%'>", fieldDetails[i].DisplayName, fieldDetails[i].FieldName == "Version" ? "color:red" : "");
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
                        case BLL.DataModels.FieldInfo.DATATYPE_LIST_SQL:
                            strResult.Append(UIManager.GenerateQueryListSql(fieldDetails[i]));
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
            FieldGroup fieldGroup = new FieldGroup(Request["searchGroup"]);
            FieldGroupDetailCollection fields = fieldGroup.GetDefaultFields();
            List<TableFormatString> formatString = new List<TableFormatString>();
            foreach (FieldGroupDetail field in fields)
            {
                if (!String.IsNullOrEmpty(field.Format))
                {
                    formatString.Add(new TableFormatString(field.FieldName, field.Format));
                }
            }

            string[] versions = Request["Version"].Split(';', ',');
            foreach (string vs in versions)
            {
                formatString.Add(new TableFormatString(vs, "{0:F10}"));
            }

            string strSql = GetQuerySql(Request);

            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = Request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(Request, searchGroupName, listParames);
            }

            strSql += " WHERE 1=1" + strWhere;

            GridData gridData = GridManager.GetGridData(Request, strSql, listParames.ToArray());

            return gridData.ToJson(formatString.ToArray());
        }

        public static string GetQuerySql(HttpRequestBase request)
        {
            string versions = request["Version"].TrimEnd(';', ',').Replace(";", ",");

            return String.Format("SELECT * FROM (SELECT * FROM (SELECT * FROM V_SC_AllCostRate WHERE Version IN({0})) AS tb PIVOT(MAX(CostRate) FOR Version IN ([{1}])) AS pt) AS t", "'" + versions.Replace(",", "','") + "'", versions);
        }

        public ActionResult GetGridFields()
        {
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            string groupName = Request["searchGroup"];
            
            GridColumns gcs = GridManager.GetGridFields(groupName, "default");

            string versions = Request["Version"];
            if (!String.IsNullOrWhiteSpace(versions))
            {
                string[] vs = versions.Split(';', ',');
                if (vs.Length > 0)
                {
                    for (int i = vs.Length - 1; i >= 0; i--)
                    {
                        if (!String.IsNullOrWhiteSpace(vs[i]))
                        {
                            string v = vs[i].Trim();
                            gcs.colNames.Insert(gcs.colNames.Count, v);

                            GridColumnModel model = new GridColumnModel();
                            model.name = v;
                            model.index = v;
                            model.width = 80;
                            model.align = "right";
                            gcs.colModel.Insert(gcs.colModel.Count, model);
                        }
                    }
                }
            }

            json.Data = gcs;

            return json;
        }


        public FileResult DownExcel()
        {
            string groupName = Request.QueryString["excelList"];

            string strSql = GetQuerySql(Request);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strWhere = GridManager.GetWhereSql(Request, groupName, lstParams);

            strSql += " WHERE 1=1" + strWhere + " ORDER BY Plant,CostMainItem,CostSecondItem,CostSubItem,MainWorkCenter";

            DataTable dt = DbHelperSQL.Query(strSql, lstParams.ToArray()).Tables[0];

            RenderType rt = Request.QueryString["renderType"] == "2" ? RenderType.Vertical : RenderType.Horizontal;
            string tempFile = ExcelHelper.DataTableToExcel(dt, rt);

            return File(tempFile, "application/ms-excel", groupName.Replace(" ", "_") + ".xlsx");
        }
    }
}
