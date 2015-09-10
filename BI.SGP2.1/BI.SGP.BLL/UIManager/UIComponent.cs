using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.UIManager
{
    public class UIComponent
    {

        public static object GetSourceCache(string sourceName)
        {
            DataTable dt = (DataTable)HttpRuntime.Cache.Get(sourceName);
            if (dt == null)
            {
                dt = DbHelperSQL.Query(sourceName).Tables[0];
                dt.CaseSensitive = true;
                HttpRuntime.Cache.Insert(sourceName, dt, null, DateTime.Now.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return dt;
        }

        public static object GetKeyValueCache(string key)
        {
            DataTable dt = (DataTable)HttpRuntime.Cache.Get("KeyValue_" + key);
            if (dt == null)
            {
                dt = DbHelperSQL.Query("SELECT [Key], [Value] FROM SGP_KeyValue WHERE [Key] = @Key AND Status = 1 AND ISNULL([Value],'')<>'' ORDER BY [Sort],[ID]", new SqlParameter("@Key", key)).Tables[0];
                dt.CaseSensitive = true;
                HttpRuntime.Cache.Insert("KeyValue_" + key, dt, null, DateTime.Now.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return dt;
        }

        public static string CreateDetailComponentAndLable(FieldInfo field)
        {
            return CreateDetailComponentAndLable(field, null);
        }

        public static string CreateDetailComponentAndLable(FieldInfo field, Dictionary<int, Dictionary<string, string>> componentWidth)
        {
            return CreateDetailComponentAndLable(field, field.Category == null ? 8 : field.Category.ColSpan, componentWidth);
        }

        public static string CreateDetailComponentAndLable(FieldInfo field, int colSpan, Dictionary<int,Dictionary<string, string>> componentWidth)
        {
            int catetoryColSpan = colSpan;
            string lableWidth = "";
            string comWidth = "";

            if (componentWidth != null && componentWidth.ContainsKey(catetoryColSpan))
            {
                Dictionary<string, string> cw = componentWidth[catetoryColSpan];
                lableWidth = cw["lableWidth"];
                comWidth = cw["comWidth"];
            }
            else
            {
                switch (catetoryColSpan)
                {
                    case 2:
                        lableWidth = "20%";
                        comWidth = "80%";
                        break;
                    case 4:
                        lableWidth = "10%";
                        comWidth = "40%";
                        break;
                    case 6:
                        lableWidth = "10%";
                        comWidth = "23%";
                        break;
                    case 10:
                        lableWidth = "10%";
                        comWidth = "10%";
                        break;
                    default:
                        lableWidth = "10%";
                        comWidth = "15%";
                        break;
                }
            }

            string fontColor = field.WFRequiredOption == RequiredOption.Required ? "color:red" : "color:#336199";

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<td style=\"background-color:#edf3f4;border:1px solid #dcebf7;width:{1};{2} \" >{0}</td>", field.DisplayName, lableWidth, fontColor);
            sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;width:{1}\" colspan={0}>", field.ColSpan, comWidth);
            sb.Append(CreateDetailComponent(field));
            sb.Append("</td>");

            if (field.ExtEmptyColSpan > 0)
            {
                for (int i = 0; i < field.ExtEmptyColSpan; i++)
                {
                    sb.AppendFormat("<td style=\"background-color:#edf3f4;border:1px dotted #dcebf7;\">&nbsp;</td>");
                }
            }

            return sb.ToString();
        }

        public static string CreateDetailComponent(FieldInfo field)
        {
            switch (field.DataType)
            {
                case FieldInfo.DATATYPE_STRING:
                    return field.ColSpan > 4 ? CreateTextArea(field) : CreateTextBox(field);
                case FieldInfo.DATATYPE_DATE:
                case FieldInfo.DATATYPE_DATETIME:
                    return CreateDatePicker(field);
                //case FieldInfo.DATATYPE_TIME:
                //    return UIManager.GenerateTimePicker(field);
                case FieldInfo.DATATYPE_LIST:
                    return CreateDropdownList(field);
                case FieldInfo.DATATYPE_LIST2:
                    return CreateDropdownList(field, true);
                case FieldInfo.DATATYPE_LIST_SQL:
                    return CreateDropdownListSql(field);
                case FieldInfo.DATATYPE_LIST_SQL2:
                    return CreateDropdownListSql2(field);
                case FieldInfo.DATATYPE_FLOAT:
                case FieldInfo.DATATYPE_INT:
                case FieldInfo.DATATYPE_DOUBLE:
                case FieldInfo.DATATYPE_PERCENT:
                    return CreateNumberBox(field);
                case FieldInfo.DATATYPE_CHECKBOXLIST:
                    return CreateCheckBoxList(field);
                case FieldInfo.DATATYPE_QUERY:
                    return CreateQueryBox(field);
                //case FieldInfo.DATATYPE_BOOLEAN:
                //    return UIManager.GenerateCheckBox(field);
            }
            return "";
        }

        /// <summary>
        /// Create WorkFlow status icon for ProductInformation in VVIPricingDetail
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        /// Lance Chen 20150129
        public static string CreateWFStatusIconForVVI(FieldInfo field)
        {
            string fieldValue = GetComponentValue(field);
            string strreturn = "";
            if (!string.IsNullOrWhiteSpace(fieldValue) && fieldValue == "9")
            {
                strreturn=strreturn+ "<span class=\"label label-success arrowed arrowed-right\">Closed</span>";
            }
            else
            {
                strreturn=strreturn+ "<span class=\"label label-info arrowed arrowed-right\">Launch</span>";
            }
            return strreturn;
        }

        public static string CreateTextBox(FieldInfo field)
        {
            string comVal = GetComponentValue(field);
            if (comVal == "CurrentUser") 
            { 
                comVal = AccessControl.CurrentLogonUser.Name; 
            }
            return String.Format("<input class=\"form-field\" style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" type=\"text\" value=\"{1}\"{2}>", field.FieldName, HttpContext.Current.Server.HtmlEncode(comVal), GetVisible(field));
        }

        public static string CreateQueryBox(FieldInfo field)
        {
            string comVal = GetComponentValue(field);
            return String.Format("<span class=\"input-icon input-icon-right\" style=\"width:100% !important;\" onclick=\"{3}\"><input class=\"form-field\" style=\"cursor:pointer;width:100% !important\" id=\"{0}\" name=\"{0}\" type=\"text\" value=\"{1}\"{2}><i class=\"icon-search green\" style=\"cursor:pointer;\"></i></span>", field.FieldName, comVal, GetVisible(field), field.KeyValueSource);
        }

        public static string CreateNumberBox(FieldInfo field)
        {
            return String.Format("<input style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" type=\"text\" class=\"NumberType1 form-field\" value=\"{1}\"{2}>", field.FieldName, GetComponentValue(field), GetVisible(field));
        }

        public static string CreateTextArea(FieldInfo field)
        {
            return String.Format("<textarea class=\"form-control form-field\" style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" value=\"{1}\"{2}>{1}</textarea>", field.FieldName, GetComponentValue(field), GetVisible(field));
        }

        public static string CreateDatePicker(FieldInfo field)
        {
            return String.Format("<div class=\"input-group\"><input style=\"height:28px !important\" id=\"{0}\" name=\"{0}\" value=\"{1}\"{2} type=\"text\" class=\"form-control date-picker form-field\" data-date-format=\"mm/dd/yyyy\"><span style=\"height:24px !important\" class=\"input-group-addon\"><i class=\"icon-calendar bigger-110\"></i></span></div>", field.FieldName, GetComponentDateTimeValue(field), GetVisible(field));
        }

        public static string CreateCheckBox(FieldInfo field)
        {
            return String.Format("<label><input id=\"{0}\" name=\"{0}\" type=\"checkbox\" class=\"ace\"><span class=\"lbl\">{1}</span></label>", field.FieldName, field.DisplayName);
        }
        public static string CreateCheckBoxList(FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = (DataTable)GetKeyValueCache(field.KeyValueSource);
            strControl.AppendFormat("<div class='control-group' {1} ><label class='control-label bolder blue'>{0}</label>", field.FieldName, GetVisible(field));
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string val = Convert.ToString(dr["Value"]);
                    string selected = String.Compare(val, GetComponentValue(field)) == 0 ? " selected" : "";
                    strControl.AppendFormat(@"<div class='checkbox'><label>
														<input name='form-field-checkbox' class='ace ace-checkbox-2' type='checkbox'>
														<span class='lbl'>{0}</span>
													</label>
												</div>", dr["Value"], selected);
                }
            }
            strControl.Append("</div>");

            return strControl.ToString();

        }

        public static string CreateDropdownList(FieldInfo field)
        {
            return CreateDropdownList(field, false);
        }

        public static string CreateDropdownList(FieldInfo field, bool hasEmpty)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = (DataTable)GetKeyValueCache(field.KeyValueSource);
            strControl.AppendFormat("<select class=\"form-control form-field\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\"{1}>", field.FieldName, GetVisible(field));

            if (hasEmpty)
            {
                strControl.AppendFormat("<option value=\"\"></option>");
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string val = Convert.ToString(dr["Value"]);
                    string selected = String.Compare(val, GetComponentValue(field)) == 0 ? " selected" : "";
                    strControl.AppendFormat("<option value=\"{0}\"{1}>{0}</option>", dr["Value"], selected);
                }
            }
            else
            {
                strControl.Append("<option value=\"\"></option>");
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string CreateDropdownListSql(FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = (DataTable)GetSourceCache(field.KeyValueSource.ToString());
            strControl.AppendFormat("<select class=\"form-control form-field\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\"{1}>", field.FieldName, GetVisible(field));

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string val = Convert.ToString(dr["Value"]);
                    string comVal = GetComponentValue(field);
                    if (comVal == "CurrentUser") comVal = AccessControl.CurrentLogonUser.Name;
                    string selected = String.Compare(val, comVal) == 0 ? " selected" : "";
                    strControl.AppendFormat("<option value=\"{0}\"{1}>{0}</option>", dr["Value"], selected);
                }
            }
            else
            {
                strControl.Append("<option value=\"\"></option>");
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string CreateDropdownListSql2(FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = (DataTable)GetSourceCache(field.KeyValueSource.ToString());
            strControl.AppendFormat("<select class=\"form-control form-field\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\"{1}>", field.FieldName, GetVisible(field));

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string val = Convert.ToString(dr["Key"]);
                    string comVal = GetComponentValue(field);
                    if (comVal == "CurrentUser") comVal = AccessControl.CurrentLogonUser.Name;
                    string selected = String.Compare(val, comVal) == 0 ? " selected" : "";
                    strControl.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", dr["Key"], dr["Value"], selected);
                }
            }
            else
            {
                strControl.Append("<option value=\"\"></option>");
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string GetComponentValue(FieldInfo field)
        {
            object dataValue = field.GetFieldValue();
            string inpVal = dataValue == null ? "" : Convert.ToString(dataValue).Trim();
            string defVal = field.DefaultValue == null ? "" : Convert.ToString(field.DefaultValue).Trim();
            return inpVal == "" ? defVal : inpVal;
        }

        public static string GetComponentDateTimeValue(FieldInfo field)
        {
            object dataValue = field.GetFieldValue();
            DateTime inpVal = ParseHelper.Parse<DateTime>(dataValue);
            DateTime defVal = field.DefaultValue == "Now" ? DateTime.Now : (ParseHelper.Parse<DateTime>(field.DefaultValue));

            if (inpVal == DateTime.MinValue)
            {
                if (defVal != DateTime.MinValue)
                {
                    return GetComponentDateTimeValue(field, defVal);
                }
            }
            else
            {
                return GetComponentDateTimeValue(field, inpVal);
            }
            return "";
        }

        public static string GetComponentDateTimeValue(FieldInfo field, DateTime dt)
        {
            if (field.DataType == FieldInfo.DATATYPE_DATETIME)
            {
                return dt.ToString("MM/dd/yyyy HH:mm:ss");
            }
            else
            {
                return dt.ToString("MM/dd/yyyy");
            }
        }

        private static string GetVisible(FieldInfo field)
        {
            return field.Visible == 0 ? " disabled=\"disabled\"" : "";
        }

        public static bool IsInt(object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    int rv = 0;
                    return Int32.TryParse(strVal, out rv);
                }
            }
            return true;
        }

        public static bool IsDate(object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    DateTime rv;
                    return DateTime.TryParse(strVal, out rv);
                }
            }
            return true;
        }

        public static bool IsFloat(object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    float rv = 0;
                    return float.TryParse(strVal, out rv);
                }
            }
            return true;
        }

        public static bool IsPercent(object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value).Replace("%", "");
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    float rv = 0;
                    return float.TryParse(strVal, out rv);
                }
            }
            return true;
        }

        public static bool IsList(string key, object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    DataTable dt = (DataTable)GetKeyValueCache(key);

                    DataRow[] drs = dt.Select("Value = '" + strVal.Trim() + "'");

                    return drs.Length > 0;
                }
            }
            return true;
        }

        public static bool IsListSql(object value, string listSql)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    DataTable dt = (DataTable)GetSourceCache(listSql);

                    DataRow[] drs = dt.Select("Value = '" + strVal.Trim() + "'");

                    return drs.Length > 0;
                }
            }
            return true;
        }
    }
}
