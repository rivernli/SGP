using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.WF;
using SGP.DBUtility;
using System.Text;

namespace BI.SGP.Web.Controllers
{
    public class SettingController : Controller
    {
        //
        // GET: /Setting/
        [MyAuthorize]
        public ActionResult Setting()
        {
            return View();
        }
        [MyAuthorize]
        public ActionResult FieldSetting()
        {
            return View();
        }
        [MyAuthorize]
        public ActionResult KeyValueSetting()
        {
            return View();
        }
        [MyAuthorize]
        public ActionResult ListSetting(string ID)
        {
            ViewBag.ListName = ID;
            return View();
        }
       

        public string EditFieldContent()
        {
            string sqlfieldname = "select distinct FieldName from SYS_FieldInfo";

            StringBuilder fieldname = new StringBuilder();

            fieldname.Append(@"<select role='select' id='FieldName' name='FieldName' size='1' class='FormElement ui-widget-content ui-corner-all'>");

            DataTable dtfieldname = DbHelperSQL.Query(sqlfieldname).Tables[0];

            foreach (DataRow dr in dtfieldname.Rows)
            {

                fieldname.Append(@"<option role='option' value='" + dr["FieldName"].ToString() + "'>" + dr["FieldName"].ToString() + "</option>");

            }

            fieldname.Append(@"</select>");
            string sqlcategoryname = "select distinct CategoryName from SYS_FieldCategory";

            StringBuilder categoryname = new StringBuilder();

            categoryname.Append(@"<select role='select' id='CategoryName' name='CategoryName' size='1' class='FormElement ui-widget-content ui-corner-all'>");

            DataTable dtcategoryname = DbHelperSQL.Query(sqlcategoryname).Tables[0];

            foreach (DataRow dr in dtcategoryname.Rows)
            {

                categoryname.Append(@"<option role='option' value='" + dr["CategoryName"].ToString() + "'>" + dr["CategoryName"].ToString() + "</option>");

            }
            categoryname.Append(@"</select>");
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<div class='ui-jqdialog-content ui-widget-content' id='editcntgrid-table'><div>");
            sb.Append(@"<form name='FormPost' id='FrmGrid_grid-table' class='FormGrid' onsubmit='return false;' style='width:auto;overflow:auto;position:relative;height:auto;'>");
            sb.Append(@"<table id='TblGrid_grid-table' class='EditTable' cellspacing='0' cellpadding='0' border='0'>");
            sb.Append(@" <tbody>");
            sb.Append(@"<tr id='FormError' style='display:none'><td class='ui-state-error' colspan='2'></td></tr>");
            sb.Append(@"<tr style='display:none' class='tinfo'><td class='topinfo' colspan='2'></td></tr>");
            sb.Append(@"<tr rowpos='1' class='FormData' id='tr_FieldName'><td class='CaptionTD'>FieldName</td><td class='DataTD'>&nbsp;" + fieldname.ToString() + "</td></tr>");
            sb.Append(@"<tr rowpos='2' class='FormData' id='tr_DisplayName'><td class='CaptionTD'>DisplayName</td><td class='DataTD'>&nbsp;<input type='text' id='DisplayName' name='DisplayName' role='textbox' class='FormElement ui-widget-content ui-corner-all'></td></tr>");
            sb.Append(@"<tr rowpos='3' class='FormData' id='tr_Sort'><td class='CaptionTD'>Sort</td><td class='DataTD'>&nbsp;<input type='text' id='Sort' name='Sort' role='textbox' class='FormElement ui-widget-content ui-corner-all'></td></tr>");
            sb.Append(@"<tr rowpos='4' class='FormData' id='tr_CategoryName'><td class='CaptionTD'>CategoryName</td><td class='DataTD'>&nbsp;" + categoryname.ToString() + "</td></tr>");
            sb.Append(@"<tr class='FormData' style='display:none'><td class='CaptionTD'></td><td colspan='1' class='DataTD'><input class='FormElement' id='id_g' type='text' name='grid-table_id' value='_empty'></td></tr>");
            sb.Append(@"</tbody></table></form></div></div>");
            return sb.ToString();
        }

        public ActionResult EditFieldSetting()
        {
            string sort = "";
            string displayname = "";
            string id = "";
            string categoryname = "";
            string fieldname = "";

            if (Request.Params["Sort"] != null)
            {
                sort = Request.Params["Sort"].ToString();
            }
            if (Request.Params["DisplayName"] != null)
            {
                displayname = Request.Params["DisplayName"].ToString();
            }
            if (Request.Params["ID"] != null)
            {
                id = Request.Params["ID"].ToString();
            }
            if (Request.Params["CategoryName"] != null)
            {
                categoryname = Request.Params["CategoryName"].ToString();
            }

            SystemMessages sysmgs = new SystemMessages();
            sysmgs.isPass = true;
            try
            {
                string strsql = "select a.FieldName,a.DisplayName,a.Sort,b.CategoryName from SYS_FieldInfo a inner join SYS_FieldCategory b on a.CategoryID=b.id where a.ID=" + id + "";
                fieldname = DbHelperSQL.GetSingle<string>("select [FieldName] from SYS_FieldInfo where ID=" + id + "");
                DataTable dt = DbHelperSQL.Query(strsql).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    if (fieldname == dt.Rows[0]["FieldName"].ToString() && displayname == dt.Rows[0]["DisplayName"].ToString() && sort == dt.Rows[0]["Sort"].ToString() && categoryname == dt.Rows[0]["CategoryName"].ToString())
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add(fieldname + " " + displayname + " ", "no change");
                    }
                    else
                    {
                        strsql = "select * from SYS_FieldInfo where [FieldName]='" + fieldname + "' and DisplayName='" + displayname + "'";

                        bool issamerow = DbHelperSQL.Exists(strsql);

                        if (issamerow == false)
                        {
                            string sqlupdate = "update SYS_FieldInfo set FieldName='" + fieldname + "',DisplayName='" + displayname + "', CategoryID=(select id from SYS_FieldCategory where CategoryName='" + categoryname + "'), Sort='" + sort + "'  where ID=" + id + " ";
                            DbHelperSQL.ExecuteSql(sqlupdate);
                            sysmgs.isPass = true;

                        }
                        else
                        {

                            sysmgs.isPass = false;
                            sysmgs.Messages.Add(fieldname + " " + displayname + " ", " must not duplicate");
                        }

                    }
                }
                else
                {
                    sysmgs.isPass = false;
                    sysmgs.Messages.Add(fieldname + " " + displayname + " ", "null value, insert fail");
                }
            }
            catch (Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("Insert Error", ex.Data.ToString());
            }





        

            var jsonObject = new
            {

                SysMgs=sysmgs

            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddFieldSetting()
        {
            string sort = "";
            string fieldname = "";
            string displayname = "";
            string id = "";
            string categoryname = "";

            string strsql = "select MAX(ID)+1 from [SYS_FieldInfo]";


            id = DbHelperSQL.GetSingle<string>(strsql);


            if (Request.Params["Sort"] != null)
            {
                sort = Request.Params["Sort"].ToString();
            }
            if (Request.Params["FieldName"] != null)
            {
                fieldname = Request.Params["FieldName"].ToString();
            }
            if (Request.Params["DisplayName"] != null)
            {
                displayname = Request.Params["DisplayName"].ToString();
            }
            if (Request.Params["CategoryName"] != null)
            {
                categoryname = Request.Params["CategoryName"].ToString();
            }

           



            

            SystemMessages sysmgs = new SystemMessages();


            try
            {
                string sqlinsert = @"insert into [SYS_FieldInfo](ID,CategoryID,FieldName,DisplayName,Sort,[Status]) 
                                    values(" + id + @",
                                           (select id from SYS_FieldCategory where CategoryName='"+categoryname+@"'),
                                           '"+fieldname+@"',
                                           '"+displayname+@"',
                                            '"+sort+@"',
                                            1);SELECT @@IDENTITY";
                int ID = DbHelperSQL.GetSingle<int>(sqlinsert);

                if (ID > 0)
                {
                    sysmgs.isPass = true;
                }
                else
                {
                    sysmgs.isPass = false;
                    sysmgs.Messages.Add("Insert Error","Insert Error");
                    sysmgs.MessageType = "error";
                }
            }
            catch(Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("",ex.Data.ToString());
                sysmgs.MessageType = "error";
            
            }
            


            var jsonObject = new
            {

                SysMsg= sysmgs,
                ID=id,


            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }


        public string EditKeyValueContent()
        {
            string sqlKEY = "select distinct [KEY] from SGP_KeyValue";

            StringBuilder Key = new StringBuilder();

            Key.Append(@"<select role='select' id='Key' name='Key' size='1' class='FormElement ui-widget-content ui-corner-all'>");

            DataTable dtfieldname = DbHelperSQL.Query(sqlKEY).Tables[0];

            foreach (DataRow dr in dtfieldname.Rows)
            {

                Key.Append(@"<option role='option' value='" + dr["Key"].ToString() + "'>" + dr["Key"].ToString() + "</option>");

            }

            Key.Append(@"</select>");

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<div class='ui-jqdialog-content ui-widget-content' id='editcntgrid-table'><div>");
            sb.Append(@"<form name='FormPost' id='FrmGrid_grid-table' class='FormGrid' onsubmit='return false;' style='width:auto;overflow:auto;position:relative;height:auto;'>");
            sb.Append(@"<table id='TblGrid_grid-table' class='EditTable' cellspacing='0' cellpadding='0' border='0'>");
            sb.Append(@" <tbody>");
            sb.Append(@"<tr id='FormError' style='display:none'><td class='ui-state-error' colspan='2'></td></tr>");
            sb.Append(@"<tr style='display:none' class='tinfo'><td class='topinfo' colspan='2'></td></tr>");
            sb.Append(@"<tr rowpos='1' class='FormData' id='tr_KEY'><td class='CaptionTD'>Key</td><td class='DataTD'>&nbsp;" + Key.ToString() + "</td></tr>");
            sb.Append(@"<tr rowpos='2' class='FormData' id='tr_Value'><td class='CaptionTD'>Value</td><td class='DataTD'>&nbsp;<input type='text' id='Value' name='Value' role='textbox' class='FormElement ui-widget-content ui-corner-all'></td></tr>");
            sb.Append(@"<tr rowpos='3' class='FormData' id='tr_Sort'><td class='CaptionTD'>Sort</td><td class='DataTD'>&nbsp;<input type='text' id='Sort' name='Sort' role='textbox' class='FormElement ui-widget-content ui-corner-all'></td></tr>");
            sb.Append(@"<tr class='FormData' style='display:none'><td class='CaptionTD'></td><td colspan='1' class='DataTD'><input class='FormElement' id='id_g' type='text' name='grid-table_id' value='_empty'></td></tr>");
            sb.Append(@"</tbody></table></form></div></div>");
            return sb.ToString();
        }


        public ActionResult EditKeyValueSetting()
        {
            string sort = "";
            string Value = "";
            string ID = "";
            string Key = "";

            if (Request.Params["Sort"] != null)
            {
                sort = Request.Params["Sort"].ToString();
            }
            if (Request.Params["Value"] != null)
            {
                Value = Request.Params["Value"].ToString();
            }
            if (Request.Params["id"] != null)
            {
                ID = Request.Params["id"].ToString();
            }
          

            
            SystemMessages sysmgs = new SystemMessages();



            try
            {
                string strsql = "select [Key],Value,Sort from SGP_KeyValue where ID="+ID+"";
                Key = DbHelperSQL.GetSingle<string>("select [Key] from SGP_KeyValue where ID=" + ID + "");
                DataTable dt = DbHelperSQL.Query(strsql).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    if (Key == dt.Rows[0]["Key"].ToString() && Value == dt.Rows[0]["Value"].ToString() && sort == dt.Rows[0]["Sort"].ToString())
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add(Key + " " + Value + " ", "no change");
                    }
                    else
                    {
                        strsql = "select * from SGP_KeyValue where [Key]='" + Key + "' and Value='" + Value + "'";

                        bool issamerow = DbHelperSQL.Exists(strsql);

                        if (issamerow == false)
                        {
                            string sqlupdate = "update SGP_KeyValue set Value='" + Value + "', Sort='" + sort + "'  where ID=" + ID + " ";
                            DbHelperSQL.ExecuteSql(sqlupdate);
                        }
                        else
                        {

                            sysmgs.isPass = false;
                            sysmgs.Messages.Add(Key + " " + Value + " ", " must not duplicate");
                        }
                    
                    }
                }
                else
                {
                    sysmgs.isPass = false;
                    sysmgs.Messages.Add(Key + " " + Value + " ", "null value, insert fail");
                }
            }
            catch (Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("Insert Error", ex.Data.ToString());
            }
            var jsonObject = new
            {
                SysMgs=sysmgs
               
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddKeyValueSetting()
        {
            string sort = "";
            string key = "";
            string value = "";

            if (Request.Params["Sort"] != null)
            {
                sort = Request.Params["Sort"].ToString();
            }
            if (Request.Params["Key"] != null)
            {
                key = Request.Params["Key"].ToString();
            }
            if (Request.Params["Value"] != null)
            {
                value = Request.Params["Value"].ToString();
            }
            if (Request.Params["Sort"] != null)
            {
                sort = Request.Params["Sort"].ToString();
            }
            SystemMessages sysmgs = new SystemMessages();
            int NewID = 0;
            try
            {
                string strsql = "select * from SGP_KeyValue where [Key]='" + key + "' and Value='" + value + "'";

                bool issamerow = DbHelperSQL.Exists(strsql);

                if (issamerow == false)
                {
                    string sqlinsert = @"insert into [SGP_KeyValue]([Key], Value, Sort, [Status]) 
                                    values('" + key + @"',
                                           '" + value + @"',
                                            '" + sort + @"',
                                            1) ; ;SELECT @@IDENTITY";
                    NewID = DbHelperSQL.GetSingle<int>(sqlinsert);
                    if (NewID > 0)
                    {
                        sysmgs.isPass = true;

                    }
                    else
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add(key + " " + value + " ", "Insert Error");
                    }
                }
                else
                {

                    sysmgs.isPass = false;
                    sysmgs.Messages.Add(key + " " + value + " ", " must not duplicate");
                }
            }
            catch (Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("Insert Error", ex.Data.ToString());
            }
            var jsonObject = new
            {
                SysMgs = sysmgs,
                ID=NewID
            };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DelKeyValuSetting()
        {
            string ID = "";

            if (Request.Params["id"] != null)
            {
                ID = Request.Params["id"].ToString();
            }
            SystemMessages sysmgs = new SystemMessages();

            try
            {
                string strsql = "delete from SGP_KeyValue where id=" + ID + "";
                DbHelperSQL.ExecuteSql(strsql);
                sysmgs.isPass = true;
            }
            catch(Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.Messages.Add("Delete Error", ex.Data.ToString());
            }
            var jsonObject = new
            {
                SysMgs = sysmgs,

            };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        public string GetAllFPCCategoryAndFields(string ID)
        {
            List<FieldCategory> list = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_FPC, FieldCategory.Category_TYPE_WORKFLOW);
            FieldGroup fieldGroup = new FieldGroup(ID);
            FieldGroupDetailCollection fgdc = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);

            string strJson = "[";

            foreach (FieldCategory fc in list)
            {
                string strCategory = "";
                string strFields = "";
                FieldInfoCollecton fic = fc.Fields;

                foreach (FieldInfo fi in fic)
                {
                    if (fgdc[fi.FieldName] == null && fi.Enable == 1 && String.IsNullOrEmpty(fi.SubDataType))
                    {
                        strFields += "{\"id\":\"" + fi.ID + "\",\"text\":\"" + fi.DisplayName.Replace("<br />", " ") + "\", \"iconCls\":\"icon-ok\"},";
                    }
                }

                strFields = strFields.TrimEnd(',');

                if (!String.IsNullOrEmpty(strFields))
                {
                    strCategory = "{\"id\":\"" + fc.CategoryName + "\", \"IsCategory\":true, \"text\":\"" + fc.CategoryName + "\", \"iconCls\":\"icon-ok\", \"children\":[" + strFields + "]},";
                }

                strJson += strCategory;
            }

            strJson = strJson.TrimEnd(',');

            strJson += "]";

            return strJson;
        } 

        public string GetAllCategoryAndFields(string ID) 
        {
            List<FieldCategory> list = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SGP, FieldCategory.Category_TYPE_WORKFLOW);
            FieldGroup fieldGroup = new FieldGroup(ID);
            FieldGroupDetailCollection fgdc = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);

            string strJson = "[";

            foreach (FieldCategory fc in list)
            {
                string strCategory = "";
                string strFields = "";
                FieldInfoCollecton fic = fc.Fields;

                foreach (FieldInfo fi in fic)
                {
                    if (fgdc[fi.FieldName] == null && fi.Enable == 1 && String.IsNullOrEmpty(fi.SubDataType))
                    {
                        strFields += "{\"id\":\"" + fi.ID + "\",\"text\":\"" + fi.DisplayName.Replace("<br />", " ") + "\", \"iconCls\":\"icon-ok\"},";
                    }
                }

                strFields = strFields.TrimEnd(',');

                if (!String.IsNullOrEmpty(strFields))
                {
                    strCategory = "{\"id\":\"" + fc.CategoryName + "\", \"IsCategory\":true, \"text\":\"" + fc.CategoryName + "\", \"iconCls\":\"icon-ok\", \"children\":[" + strFields + "]},";
                }

                strJson += strCategory;
            }

            strJson = strJson.TrimEnd(',');

            strJson += "]";

            return strJson;
        }

        public string GetUserListFields(string ID)
        {
            string strJson = "[";

            FieldGroup fieldGroup = new FieldGroup(ID);

            FieldGroupDetailCollection fgdc = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
            if (fgdc.Count == 0)
            {
                fgdc = fieldGroup.GetDefaultFields();
            }

            foreach (FieldGroupDetail fgd in fgdc)
            {
                strJson += "{\"id\":\"" + fgd.ID + "\",\"text\":\"" + fgd.DisplayName.Replace("<br />", " ") + "\", \"iconCls\":\"icon-ok\"},";
            }

            strJson = strJson.TrimEnd(',');

            strJson += "]";
            return strJson;
        }

        [HttpPost]
        public JsonResult SaveListFields(FieldsSettingModel postModel)
        {
            string errMessage = "";
            try
            {
                FieldGroup fg = new FieldGroup(postModel.ListName);
                fg.SaveUserFieldGroup(AccessControl.CurrentLogonUser.Uid, postModel);
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
        [MyAuthorize]
        public ActionResult UserMappingSetting()
        {
            return View();
        }
        [MyAuthorize]
        public ActionResult FieldMappingSetting()
        {
            return View();
        }

        public ActionResult GetPreviewUser()
        {
            string actId = Request["actId"];
            int activityId = 0;
            if (int.TryParse(actId, out activityId))
            {
                WFActivity activity = new WFActivity(activityId);

                var previewUser = new
                {
                    Specified = activity.GetEntityPreviewUser(),
                    Fixed = activity.GetStaticUsers(),
                    Role = activity.GetRolePreviewUser()
                };
                return Json(previewUser);
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetPreviewFields()
        {
            string actId = Request["actId"];
            int activityId = 0;
            if (int.TryParse(actId, out activityId))
            {
                WFActivity activity = new WFActivity(activityId);
                List<WFActivityField> lstFields = activity.GetCheckFields();
                if (lstFields != null)
                {
                    return Json(lstFields);
                }
            }
            return Content("[]");
        }

        public ActionResult AddUserMapping()
        {
            bool addSuccess = false;
            string errMessage = "";
            string userName = Request["userName"];
            int isKeyUser = Request["userType"] == "Owner" ? 1 : 0;
            string actId = Request["activityId"];
            int activityId = 0;

            if (!String.IsNullOrWhiteSpace(userName) && int.TryParse(actId, out activityId))
            {
                try
                {
                    string strSql = @"SELECT TOP 1 UID,UserType FROM (
                                SELECT Name, t1.UID, 'Static' AS UserType FROM Access_User t1, Access_UserPermission_App t2 WHERE t1.Uid = t2.UID AND t2.AppID = 2
                                UNION 
                                SELECT DisplayName + '(' + ISNULL((SELECT CategoryName FROM SYS_FieldCategory WHERE ID = SYS_FieldInfo.CategoryID),'') + ')', CAST(ID AS VARCHAR), 'Entity' FROM SYS_FieldInfo WHERE ISNULL(TableName,'') <> '' AND DisplayName <> ''
                                UNION 
                                SELECT NAME, NAME, 'Role' FROM Access_Role t1, Access_RolePermission_App t2 WHERE t1.RoleID = t2.RoleID AND t2.AppID = 2
                                ) AS T WHERE Name = @Name";

                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", userName.Trim())).Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        string uid = Convert.ToString(dt.Rows[0]["UID"]);
                        string userType = Convert.ToString(dt.Rows[0]["UserType"]);

                        SqlParameter[] ps = 
                        {
                            new SqlParameter("@UID", uid),
                            new SqlParameter("@ActivityID", activityId),
                            new SqlParameter("@UserType", userType),
                        };

                        strSql = "SELECT COUNT(*) FROM SYS_WFActivityUser WHERE UID = @UID AND ActivityID = @ActivityID AND UserType = @UserType";
                        if (DbHelperSQL.GetSingle<int>(strSql, ps) > 0)
                        {
                            if (isKeyUser == 1)
                            {
                                strSql = "UPDATE SYS_WFActivityUser SET IsKeyUser = 1 WHERE UID = @UID AND ActivityID = @ActivityID AND UserType = @UserType";
                                DbHelperSQL.ExecuteSql(strSql, ps);
                                addSuccess = true;
                            }
                        }
                        else
                        {
                            strSql = String.Format("INSERT INTO SYS_WFActivityUser(UID,ActivityID,UserType,IsKeyUser) VALUES(@UID,@ActivityID,@UserType,{0})", isKeyUser);
                            DbHelperSQL.ExecuteSql(strSql, ps);
                            addSuccess = true;
                        }

                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }

            var jsonResult = new
            {
                Success = addSuccess,
                errMessage = errMessage
            };

            return Json(jsonResult);
        }

        public ActionResult AddFieldMapping()
        {
            bool addSuccess = false;
            string errMessage = "";
            string fieldName = Request["fieldName"];
            int isRequired = Request["fieldType"] == "Required" ? 1 : 0;
            string actId = Request["activityId"];
            int activityId = 0;

            if (!String.IsNullOrWhiteSpace(fieldName) && int.TryParse(actId, out activityId))
            {
                try
                {
                    string strSql = "SELECT TOP 1 ID FROM SYS_FieldInfo WHERE ISNULL(TableName,'') <> '' AND DisplayName <> '' AND (DisplayName + '(' + ISNULL((SELECT CategoryName FROM SYS_FieldCategory WHERE ID = SYS_FieldInfo.CategoryID),'') + ')') = @Name";

                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", fieldName.Trim())).Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        int fieldID = Convert.ToInt32(dt.Rows[0]["ID"]);

                        SqlParameter[] ps = 
                        {
                            new SqlParameter("@FieldID", fieldID),
                            new SqlParameter("@ActivityID", activityId),
                        };

                        strSql = "SELECT COUNT(*) FROM SYS_WFActivityField WHERE FieldID = @FieldID AND ActivityID = @ActivityID";
                        if (DbHelperSQL.GetSingle<int>(strSql, ps) > 0)
                        {
                            if (isRequired == 1)
                            {
                                strSql = "UPDATE SYS_WFActivityField SET IsRequired = 1 WHERE FieldID = @FieldID AND ActivityID = @ActivityID";
                                DbHelperSQL.ExecuteSql(strSql, ps);
                                addSuccess = true;
                            }
                        }
                        else
                        {
                            strSql = String.Format("INSERT INTO SYS_WFActivityField(FieldID,ActivityID,IsRequired,Sort) VALUES(@FieldID,@ActivityID,{0},9999)", isRequired);
                            DbHelperSQL.ExecuteSql(strSql, ps);
                            addSuccess = true;
                        }

                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }

            var jsonResult = new
            {
                Success = addSuccess,
                errMessage = errMessage
            };

            return Json(jsonResult);
        }

        public ActionResult DelUserMapping()
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
                    string strSql = "DELETE FROM SYS_WFActivityUser WHERE ID = @ID";
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

        public ActionResult DelFieldMapping()
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
                    string strSql = "DELETE FROM SYS_WFActivityField WHERE ID = @ID";
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

        public ActionResult SaveFieldSort()
        {
            bool success = true;
            string errMessage = "";
            string actId = Request["actId"];
            int activityId = 0;
            if (int.TryParse(actId, out activityId) && activityId > 0)
            {
                try
                {
                    WFActivity activity = new WFActivity(activityId);
                    List<WFActivityField> lstFields = activity.GetCheckFields();
                    if (lstFields != null)
                    {
                        string strSql = "UPDATE SYS_WFActivityField SET Sort = @Sort WHERE ID = @ID";

                        foreach (WFActivityField field in lstFields)
                        {
                            string fieldSort = Request[field.ID.ToString()];
                            if (!String.IsNullOrWhiteSpace(fieldSort))
                            {
                                int iSort = 0;
                                if (int.TryParse(fieldSort, out iSort) && iSort > 0)
                                {
                                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter[] { 
                                        new SqlParameter("@Sort",iSort),
                                        new SqlParameter("@ID",field.ID),
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
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

        public ActionResult GetUserMappingList()
        {
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            string strSql = @"SELECT TOP 10 * FROM (
                                SELECT Name FROM Access_User t1, Access_UserPermission_App t2 WHERE t1.Uid = t2.UID AND t2.AppID = 2
                                UNION 
                                SELECT DisplayName + '(' + ISNULL((SELECT CategoryName FROM SYS_FieldCategory WHERE ID = SYS_FieldInfo.CategoryID),'') + ')' FROM SYS_FieldInfo WHERE ISNULL(TableName,'') <> ''
                                UNION 
                                SELECT NAME FROM Access_Role t1, Access_RolePermission_App t2 WHERE t1.RoleID = t2.RoleID AND t2.AppID = 2
                                ) AS T WHERE Name LIKE @term";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["NAME"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["NAME"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult GetFieldMappingList()
        {
            string templateId = Request.QueryString["templateId"] == null ? "" : Request.QueryString["templateId"].Trim();
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            string strSql = "SELECT TOP 10 t1.DisplayName + '(' + ISNULL(t2.CategoryName,'') + ')' AS DisplayName FROM SYS_FieldInfo t1 INNER JOIN SYS_FieldCategory t2 ON t1.CategoryID = t2.ID WHERE ISNULL(t1.TableName,'') <> '' AND t1.DisplayName LIKE @term AND t2.TemplateID = @templateId";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@templateId", templateId),
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["DisplayName"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["DisplayName"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult GetAuotCompleteValue()
        {
            string strSql = "SELECT TOP 10 [Key], [Value] FROM SGP_KeyValue WHERE [KEY]='OEM' and [Value] LIKE @term order by [Value]";
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult GetAuotCompleteValueForShipmentTerms()
        {
            string strSql = "SELECT TOP 10 [Key], [Value] FROM SGP_KeyValue WHERE [KEY]='ShipmentTerms' and [Value] LIKE @term order by [Value]";
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult GetAuotCompleteValueForLocation()
        {
            string strSql = "SELECT TOP 10 [Key], [Value] FROM SGP_KeyValue WHERE [KEY]='Location' and [Value] LIKE @term order by [Value]";
            string term = Request.QueryString["term"] == null ? "" : Request.QueryString["term"].TrimStart();
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@term", "%" + term + "%")
            }).Tables[0];
            string strJson = "[";
            foreach (DataRow dr in dt.Rows)
            {
                strJson += "{\"label\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "," + "\"value\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr["Value"]) + "},";
            }
            strJson = strJson.TrimEnd(',');
            strJson += "]";
            return Content(strJson);
        }

        public ActionResult GetCostModuel()
        {
            string ProjectNumber = Request.Form["ProjectNumber"].ToString() == null ? "" : Request.Form["ProjectNumber"].ToString().TrimStart();
            DataTable dt = new DataTable();
            try
            {
                string strSql = "exec Mcnnt803.multek_180.dbo.Cost_sheet_Information @Item ";
                dt = DbHelperSQL.Query(strSql, new SqlParameter("@Item", ProjectNumber)).Tables[0];
            }
            catch
            {

            }

            string strJson = "{";
            foreach (DataColumn field in dt.Columns)
            { 
                if(dt.Rows.Count>0)
                {
                    strJson += "\"" + field.ColumnName + "\":" +  Newtonsoft.Json.JsonConvert.SerializeObject(dt.Rows[0][field.ColumnName]) + ",";
                }
                else
                {
                     strJson += "\"" + field.ColumnName + "\":" + Newtonsoft.Json.JsonConvert.SerializeObject("") + ",";
                }

            }
            strJson = strJson.TrimEnd(',');
            strJson += "}";
            
            return Content(strJson);

        }

        public ActionResult GetOEMInvolvedValue()
        {
            string OEM = Request.Form["OEM"].ToString() == null ? "" : Request.Form["OEM"].ToString().TrimStart();

            string strSql = "SELECT * FROM SGP_CustomerOEMBasedDefaults WHERE Customer_OEM = @OEM ";
            DataTable dt = DbHelperSQL.Query(strSql,new SqlParameter("@OEM",OEM)).Tables[0];

            string strJson = "{";
            foreach (DataColumn field in dt.Columns)
            { 
                if(dt.Rows.Count>0)
                {
                    strJson += "\"" + field.ColumnName + "\":" +  Newtonsoft.Json.JsonConvert.SerializeObject(dt.Rows[0][field.ColumnName]) + ",";
                }
                else
                {
                     strJson += "\"" + field.ColumnName + "\":" + Newtonsoft.Json.JsonConvert.SerializeObject("") + ",";
                }

            }
            strJson = strJson.TrimEnd(',');
            strJson += "}";
            
            return Content(strJson);
        }

     


    }
}
