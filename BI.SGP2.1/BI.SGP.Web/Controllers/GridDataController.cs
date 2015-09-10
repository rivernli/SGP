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
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class GridDataController : Controller
    {
        public string GetGridData()
        {
            FieldGroup fieldGroup = new FieldGroup(Request["groupName"]);
            string[] extSqlColumns = String.IsNullOrEmpty(Request["extSqlColumns"]) ? null : Request["extSqlColumns"].Split(',');
            FieldGroupDetailCollection fields = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
            List<TableFormatString> formatString = new List<TableFormatString>();
            foreach (FieldGroupDetail field in fields)
            {
                if (!String.IsNullOrEmpty(field.Format))
                {
                    formatString.Add(new TableFormatString(field.FieldName, field.Format));
                }
            }

            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fields, extSqlColumns);

            GridData gridData;
            //Filter data for SupplierPricingView
            if (AccessControl.IsVendor() &&
                (Request["groupName"].ToString().ToUpper() == "SUPPLIERRFQGRID" ||
                Request["groupName"].ToString().ToUpper() == "VENDORRFQREPORTGRID"))
            {
                gridData = GridManager.GetGridData(Request, strSql, fieldGroup.Authority, fields);
            }
            else
            {
                if (Request["groupName"].ToString().ToUpper() == "CUSTOMERPROFILEGRID")
                {
                    gridData = GridManager.GetGridData(Request, strSql, fieldGroup);
                }
                else
                {
                    gridData = GridManager.GetGridData(Request, strSql, fieldGroup.Authority);
                }
            }

            return gridData.ToJson(formatString.ToArray());
        }

        //
        // GET: /GridData/
        
        public string GetData(string ID)
        {
            FieldGroup fieldGroup = new FieldGroup(ID);
            FieldGroupDetailCollection fields = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
            List<TableFormatString> formatString = new List<TableFormatString>();
            foreach (FieldGroupDetail field in fields)
            {
                if (!String.IsNullOrEmpty(field.Format))
                {
                    formatString.Add(new TableFormatString(field.FieldName, field.Format));
                }
            }
        
            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fields, "RFQID", "StatusID");
            if (fieldGroup.SourceName == "V_Supplier")
            {

                strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fields, "ID");
            }

            GridData gridData = GridManager.GetGridData(Request, strSql, fieldGroup.Authority);

            return gridData.ToJson(formatString.ToArray());
        }

        public string GetFieldSettingData()
        {
            string strsql=@"select ID=a.ID,FieldName=a.FieldName,DisplayName=a.DisplayName,Sort=a.Sort,CategoryName=b.CategoryName 
                            from SYS_FieldInfo a left join SYS_FieldCategory b on a.CategoryID=b.ID and a.[Status]=1";
            GridData gridData = GridManager.GetGridDataforSetting(Request, strsql);
            
            return gridData.ToJson();
        
        }

        public string SearchFieldSettingData()
        {
            string strsql = @"select ID=a.ID,FieldName=a.FieldName,DisplayName=a.DisplayName,Sort=a.Sort,CategoryName=b.CategoryName 
                            from SYS_FieldInfo a left join SYS_FieldCategory b on a.CategoryID=b.ID and a.[Status]=1";

            if (Request.Params.AllKeys.Contains("Search"))
            {
                string Search = Request.Params["Search"].ToString();
                strsql = @"select ID=a.ID,FieldName=a.FieldName,DisplayName=a.DisplayName,Sort=a.Sort,CategoryName=b.CategoryName 
                            from SYS_FieldInfo a left join SYS_FieldCategory b on a.CategoryID=b.ID and a.[Status]=1 where a.FieldName like '%" + Search + "%' or a.DisplayName like '%" + Search + "%' or  a.TableName like '%" + Search + "%'";   
  
            
            }
            GridData gridData = GridManager.GetGridDataforSetting(Request, strsql);

            return gridData.ToJson();

        }

      
        public string GetKeyValueSettingData()
        {
            string strsql = @"select ID=a.ID,  [Key]=a.[Key],FieldName=b.FieldName, Value=a.Value,Sort=a.Sort 
                                from SGP_KeyValue a inner join SYS_FieldInfo b on a.[Key]=b.FieldName where a.[Status]=1";
            GridData gridData = GridManager.GetGridDataforSetting(Request, strsql);

            return gridData.ToJson();

        }
        public string SearchKeyValueSettingData()
        {
            string strsql = @"select ID=a.ID,  [Key]=a.[Key],FieldName=b.FieldName, Value=a.Value,Sort=a.Sort 
                                from SGP_KeyValue a inner join SYS_FieldInfo b on a.[Key]=b.FieldName where a.[Status]=1";

            if (Request.Params.AllKeys.Contains("Search"))
            {
                string Search = Request.Params["Search"].ToString();
                strsql = @"select ID=a.ID,  [Key]=a.[Key],FieldName=b.FieldName, Value=a.Value,Sort=a.Sort 
                                from SGP_KeyValue a inner join SYS_FieldInfo b on a.[Key]=b.FieldName and a.[Status]=1 where a.[Key] like '%" + Search + "%' or a.Value like '%" + Search + "%'";


            }
            GridData gridData = GridManager.GetGridDataforSetting(Request, strsql);

            return gridData.ToJson();

        }


        public JsonResult DelData(string ID)
        {
            string errMessage= "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    string strSql = "UPDATE SGP_RFQ SET StatusID = 0,LastUpdate=GetDate() WHERE ID = @ID";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", dataId));
                    if (VVIRFQManager.IsExistInVVI(ID))
                    {
                        VVIRFQManager.DeleteVVIRFQData(ID);
                    }
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

        public ActionResult GetGridFields(string ID)
        {
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = GridManager.GetGridFields(ID, AccessControl.CurrentLogonUser.Uid);

            return json;
        }

        public ActionResult GetQueryTable()
        {
            string listName = Request["listName"];
            string strTable = UIManager.GenerateQuery(listName);
            return Content(strTable);
        }

        public ActionResult TransformGPToVI()
        {
            string id = Request["id"];
            string programName = Request["programName"];
            List<SqlParameter> listParames = new List<SqlParameter>();
            listParames.Add(new SqlParameter("@gpid",SqlDbType.Int));
            listParames.Add(new SqlParameter("@message", SqlDbType.VarChar, 100));
            listParames.Add(new SqlParameter("@works", SqlDbType.Bit));
            listParames.Add(new SqlParameter("@ispass", SqlDbType.Bit));
 
            listParames[0].Value = Int32.Parse(id);
            listParames[1].Direction = ParameterDirection.Output;
            listParames[2].Direction = ParameterDirection.Output;
            listParames[3].Direction = ParameterDirection.Output;

            DbHelperSQL.ExecuteSql("exec [sp_SGP_addToVI] @gpid,1,@message output,@works output,@ispass output", listParames.ToArray());

            SystemMessages mgs = new SystemMessages();
     
            if ((bool)listParames[3].Value == true)
            { 
                mgs.isPass = true;
                mgs.Messages.Add("Success",listParames[1].Value.ToString());
            }
            else
            {
                mgs.isPass = false;
                mgs.Messages.Add("Fail", listParames[1].Value.ToString());
            }

            var result = new
            {
                success = mgs.isPass,
                Message = mgs.MessageString
            };
            return Json(result);
        }
    }
}
