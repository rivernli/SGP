using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.UIManager
{
    public class CustomerUIComponent
    {
        public static string CreateDetailComponentAndLable(FieldInfo field, string pageType)
        {
            int catetoryColSpan = field.Category == null ? 8 : field.Category.ColSpan;
            string lableWidth = "";
            string comWidth = "";
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
                    lableWidth = "13%";
                    comWidth = "20%";
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

            string fontColor = field.WFRequiredOption == RequiredOption.Required ? "color:red" : "color:#336199";

            StringBuilder sb = new StringBuilder();
            if (pageType == "Detail")
            {
                sb.AppendFormat("<td style=\"background-color:#edf3f4;border:1px solid #dcebf7;width:{1};{2} \" >{0}</td>", field.DisplayName, lableWidth, fontColor);
                sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;width:{1}\" colspan={0}>", field.ColSpan, comWidth);
                sb.Append(CreateDetailComponent(field));
                
            }
            else
            {
                sb.AppendFormat("<td style=\"width:{1} \" >{0}</td>", field.DisplayName, lableWidth);
                sb.AppendFormat("<td style=\"width:{1}\" colspan={0}>", field.ColSpan, comWidth);
                
                sb.Append(CreatePrintComponent(field));
            }

            sb.Append("</td>");

            if (field.ExtEmptyCol == 1)
            {
                if (pageType == "Detail")
                {
                    for (int i = 0; i < field.ExtEmptyColSpan; i++)
                    {
                        sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;\">&nbsp;</td>");
                    }
                }
                else
                {
                    for (int i = 0; i < field.ExtEmptyColSpan; i++)
                    {
                        sb.AppendFormat("<td>&nbsp;</td>");
                    }
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
                case FieldInfo.DATATYPE_LIST_SQL:
                    return CreateDropdownListSql(field);
                case FieldInfo.DATATYPE_FLOAT:
                case FieldInfo.DATATYPE_INT:
                case FieldInfo.DATATYPE_DOUBLE:
                    return CreateNumberBox(field);
                //case FieldInfo.DATATYPE_BOOLEAN:
                //    return UIManager.GenerateCheckBox(field);
            }
            return "";
        }

        //Print
        public static string CreatePrintComponent(FieldInfo field)
        {
            if (field.DataType == "float")
            {
                return String.Format("&nbsp;<label style=\"width:100%\" id=\"{0}\" name=\"{0}\"{2}>&nbsp;{1}</label>", field.FieldName, string.Format("{0:C0}", field.DataValue), GetVisible(field));
            }
            else
            {
                return String.Format("&nbsp;<label style=\"width:100%\" id=\"{0}\" name=\"{0}\"{2}>&nbsp;{1}</label>", field.FieldName, GetComponentValue(field), GetVisible(field));
            }
        }
        
        public static string CreateTextBox(FieldInfo field)
        {
            return String.Format("<input style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" type=\"text\" value=\"{1}\"{2}>", field.FieldName, GetComponentValue(field), GetVisible(field));
        }

        public static string CreateNumberBox(FieldInfo field)
        {
            return String.Format("<input style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" type=\"text\" class=\"NumberType1\" value=\"{1}\"{2}>", field.FieldName, GetComponentValue(field), GetVisible(field));
        }

        public static string CreateTextArea(FieldInfo field)
        {
            return String.Format("<textarea class=\"form-control\" style=\"width:100% !important\" id=\"{0}\" name=\"{0}\" value=\"{1}\"{2}>{1}</textarea>", field.FieldName, GetComponentValue(field), GetVisible(field));
        }

        public static string CreateDatePicker(FieldInfo field)
        {
            return String.Format("<div class=\"input-group\"><input style=\"height:28px !important\" id=\"{0}\" name=\"{0}\" value=\"{1}\"{2} type=\"text\" class=\"form-control date-picker\" data-date-format=\"mm/dd/yyyy\"><span style=\"height:24px !important\" class=\"input-group-addon\"><i class=\"icon-calendar bigger-110\"></i></span></div>", field.FieldName, GetComponentDateTimeValue(field), GetVisible(field));
        }

        public static string CreateCheckBox(FieldInfo field)
        {
            return String.Format("<label><input id=\"{0}\" name=\"{0}\" type=\"checkbox\" class=\"ace\"><span class=\"lbl\">{1}</span></label>", field.FieldName, field.DisplayName);
        }

        public static string CreateDropdownList(FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = SqlText.ExecuteDataset("SELECT [Key],[Value] FROM SGP_KeyValue WHERE [Key]=@Key AND Status=1 AND ISNULL([Value],'')<>'' ORDER BY [Sort]", new SqlParameter("@Key", field.KeyValueSource)).Tables[0];
            strControl.AppendFormat("<select class=\"form-control\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\"{1}>", field.FieldName, GetVisible(field));

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
            DataTable dt = SqlText.ExecuteDataset(field.KeyValueSource.ToString()).Tables[0];
            strControl.AppendFormat("<select class=\"form-control\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\"{1}>", field.FieldName, GetVisible(field));

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

        public static bool IsList(string key, object value)
        {
            if (value != null)
            {
                string strVal = Convert.ToString(value);
                if (!String.IsNullOrWhiteSpace(strVal))
                {
                    return DbHelperSQL.Exists("SELECT COUNT(*) FROM SGP_KeyValue WHERE [Key]=@Key AND [Value]=@Value AND Status=1", new SqlParameter("@Key", key), new SqlParameter("@Value", strVal));
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
                    return DbHelperSQL.Exists("SELECT COUNT(*) FROM (" + listSql + ") AS T WHERE [Value]=@Value", new SqlParameter("@Value", strVal));
                }
            }
            return true;
        }
    }
}
