using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Models.Detail;
using SGP.DBUtility;
using System.Data;
using System.Data.SqlClient;

namespace BI.SGP.BLL.UIManager
{
    public class CostingMasterDataDetailHelper
    {
        public static string GenrateCategory(string categoryName)
        {
            return GenrateCategory(new FieldCategory(categoryName));
        }

        public static string GenrateCategory(FieldCategory category)
        {
            StringBuilder html = new StringBuilder();
            MarkRequiredFields(category.Fields);
            html.AppendFormat(@"<div class='panel panel-default' style='margin-bottom: 0px'>
                                    <div class='panel-collapse in' id='faq-1-{0}' style='height: auto;'>
                                        <div class='panel-body'>
                                            <form id='fm-{0}' class='fm-category'>
                                                <table style='width:100%'>
                                                    {1}
                                                </table>{2}
                                            </form>", category.ID, GenrateCategoryFields(category), CreateHiddenFields(category));

            html.Append("</div></div></div>");
            return html.ToString();
        }

        public static string GenrateYieldCategory(string pcbType, string version, Dictionary<string, string> data)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> dicYields = GetYieldFields(pcbType, version);

            int RowCount = dicYields.Count;

            int colSpan = 6;
            int colSpanTotal = 0;
            for (int i = 0; i < RowCount; i++)
            {
                if (colSpanTotal == 0)
                {
                    sb.Append("<tr height='28px'>");
                }

                colSpanTotal += 2;

                sb.Append(CreateDetailComponentAndLable(dicYields.ElementAt(i), data));

                if (colSpanTotal == colSpan)
                {
                    sb.Append("</tr>");
                    colSpanTotal = 0;
                }

                if (colSpanTotal > colSpan)
                {
                    sb.Append("</tr>");
                    colSpanTotal = 0;
                    i--;
                }
            }

            StringBuilder html = new StringBuilder();

            html.AppendFormat(@"<div class='panel panel-default' style='margin-bottom: 0px'>
                                    <div class='panel-collapse in' id='faq-1-yield' style='height: auto;'>
                                        <div class='panel-body'>
                                            <form id='fm-yield'>
                                                <table style='width:100%'>
                                                    {0}
                                                </table>
                                            </form>", sb);

            html.Append("</div></div></div>");
            return html.ToString();
        }

        public static Dictionary<string, object> GetYieldFields(string pcbType, string version)
        {
            Dictionary<string, object> dicYield = new Dictionary<string, object>();
            string strSql = "SELECT ItemName,ItemValue,Yield FROM SCD_AdderYield WHERE PCBType=@PCBType AND Version=@Version ORDER BY Sort, ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@PCBType", pcbType), new SqlParameter("@Version", version)).Tables[0];
            dicYield.Add("Adjust Yield(%)", "");
            foreach (DataRow dr in dt.Rows)
            {
                string itemName = Convert.ToString(dr["ItemName"]);
                string itemValue = Convert.ToString(dr["ItemValue"]);
                string yield = Convert.ToString(dr["Yield"]);
                if (String.IsNullOrWhiteSpace(itemValue))
                {
                    dicYield.Add(itemName, yield);
                }
                else
                {
                    if (dicYield.ContainsKey(itemName))
                    {
                        ((List<DataRow>)dicYield[itemName]).Add(dr);
                    }
                    else
                    {
                        List<DataRow> lstDrs = new List<DataRow>();
                        lstDrs.Add(dr);
                        dicYield.Add(itemName, lstDrs);
                    }
                }
            }
            return dicYield;
        }

        public static string CreateDetailComponentAndLable(KeyValuePair<string, object> kv, Dictionary<string, string> data)
        {
            StringBuilder strControl = new StringBuilder();

            if (kv.Value is List<DataRow>)
            {
                List<DataRow> drs = (List<DataRow>)kv.Value;
                strControl.AppendFormat("<select class=\"form-control form-field\" style=\"width:100% !important\" name=\"{0}\" id=\"{0}\">", kv.Key);

                strControl.Append("<option value=\"\"></option>");
                foreach (DataRow dr in drs)
                {
                    string val = Convert.ToString(dr["Yield"]);
                    string selected = (data != null && data.ContainsKey(kv.Key) && data[kv.Key] == val) ? " selected" : "";
                    strControl.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", dr["Yield"], dr["ItemValue"], selected);
                }

                strControl.Append("</select>");
            }
            else
            {
                if (Convert.ToString(kv.Value) == "")
                {
                    strControl.AppendFormat("<input id=\"{0}\" name=\"{0}\" type=\"text\" style=\"width:100% !important\" value=\"{1}\" class=\"NumberType1 form-field\">", kv.Key, data.ContainsKey(kv.Key) ? data[kv.Key] : "");
                }
                else
                {
                    strControl.AppendFormat("<input id=\"{0}\" name=\"{0}\" type=\"checkbox\" value=\"{1}\" {2} class=\"ace\"><span class=\"lbl\"></span>", kv.Key, kv.Value, data.ContainsKey(kv.Key) ? "checked" : "");
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<td style=\"background-color:#edf3f4;border:1px solid #dcebf7;width:{1}; \" >{0}</td>", kv.Key, "23%");
            sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;width:{0}\" align=\"center\">", "10%");
            sb.Append(strControl);
            sb.Append("</td>");
            return sb.ToString();
        }

        public static string GenrateCategoryFields(FieldCategory category)
        {
            return GenrateCategoryFields(category.VisibleFields, category.ColSpan, null);
        }

        public static string GenrateCategoryFields(FieldInfoCollecton fields, int colSpan, Dictionary<string, string> dcwi)
        {
            StringBuilder sb = new StringBuilder();
            FieldInfoCollecton enablefields = fields;
            int RowCount = enablefields.Count;

            Dictionary<int, Dictionary<string, string>> dcWidth = new Dictionary<int, Dictionary<string, string>>();

            if (dcwi != null)
            {
                dcWidth.Add(colSpan, dcwi);
            }

            if (!dcWidth.ContainsKey(4))
            {
                Dictionary<string, string> dcw = new Dictionary<string, string>();
                dcw.Add("lableWidth", "20%");
                dcw.Add("comWidth", "30%");
                dcWidth.Add(4, dcw);
            }

            if (!dcWidth.ContainsKey(6))
            {
                Dictionary<string, string> dcw = new Dictionary<string, string>();
                dcw.Add("lableWidth", "15%");
                dcw.Add("comWidth", "18%");
                dcWidth.Add(6, dcw);
            }

            int colSpanTotal = 0;
            for (int i = 0; i < RowCount; i++)
            {
                if (colSpanTotal == 0)
                {
                    sb.Append("<tr>");
                }

                colSpanTotal++;

                int curSpan;
                if (enablefields[i].ColSpan == 0)
                {
                    curSpan = 1;
                }
                else if (enablefields[i].ColSpan > (colSpan - 1))
                {
                    curSpan = colSpan - 1;
                }
                else
                {
                    curSpan = enablefields[i].ColSpan;
                }
                colSpanTotal += curSpan;

                if (enablefields[i].ExtEmptyColSpan > 0)
                {
                    colSpanTotal += enablefields[i].ExtEmptyColSpan;
                }

                if (colSpanTotal <= colSpan)
                {
                    sb.Append(UIComponent.CreateDetailComponentAndLable(enablefields[i], colSpan, dcWidth));
                }

                if (colSpanTotal == colSpan)
                {
                    sb.Append("</tr>");
                    colSpanTotal = 0;
                }

                if (colSpanTotal > colSpan)
                {
                    sb.Append("</tr>");
                    colSpanTotal = 0;
                    i--;
                }
            }

            if ((RowCount*2) < colSpan)
            {
                for (int i = (RowCount*2); i < colSpan; i++)
                {
                    sb.Append("<td>&nbsp;</td>");
                }
                sb.Append("</tr>");
            }
            
            return sb.ToString();
        }

        public static string GenrateTableBody(FieldInfoCollecton Fields)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<tr>");

            foreach (FieldInfo f in Fields)
            {
                sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string GenrateSubCategory(FieldCategory category, bool hasPanel, bool canEdit, bool headEdit)
        {
            string html = String.Format("<form id='fm-{0}' class='fm-category' categoryName='{1}' subDataType='{2}'>{3}</form>", category.ID, category.Description, category.Fields[0].SubDataType, GenrateCategorySubFields(category, canEdit, headEdit));
            if (hasPanel)
            {
                html = String.Format(@"<div class='panel panel-default'>
                                    <div class='panel-heading'>
                                        <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle'>
                                            <i class='pull-right icon-chevron-down' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                            <i class='icon-pencil bigger-130'></i>&nbsp; {1}:
                                        </a>
                                    </div>
                                    <div class='panel-collapse in' id='faq-1-{0}' style='height: auto;'>
                                        <div class='panel-body'>{2}</div></div></div>", category.ID, category.Description, html);
            }

            return html;
        }

        public static string GenrateCategorySubFields(FieldCategory category, bool canEdit, bool headEdit)
        {
            StringBuilder sb = new StringBuilder();
            if (category.VisibleFields.Count > 0)
            {
                int subCount = 0;
                sb.AppendFormat("<table id='tb-{0}' style='width:100%;margin-bottom:0px;' class='table table-striped table-bordered table-hover'>", category.VisibleFields[0].SubDataType);
                sb.Append("<thead><tr style='background-image:-webkit-gradient(linear,left 0,left 100%,from(#f8f8f8),to(#ececec))'>");
                StringBuilder sbSubLine = new StringBuilder();
                foreach (FieldInfo f in category.VisibleFields)
                {
                    if (subCount == 0 && f.DataValue is ArrayList)
                    {
                        subCount = ((ArrayList)f.DataValue).Count;
                    }
                    string fontColor = f.Options.Required ? ";color:red" : "";
                    sbSubLine.AppendFormat("<th nowrap style='padding:8px;width:{0}px{1}'>{2}</th>", f.Width == 0 ? 100 : f.Width, fontColor, f.DisplayName);
                }

                sb.AppendFormat("<th style='padding:8px;width:30px'>{0}</th>", (canEdit && headEdit) ? "<a href='javascript:void(0)' onclick='add" + category.SubFields[0].SubDataType + "Detail(\"" + category.SubFields[0].SubDataType + "\")'><i class='green icon-plus bigger-130'></i></a>" : "");
                //sb.AppendFormat("<th style='padding:8px;width:30px'></th>");
                sb.Append(sbSubLine);
                sb.Append("</tr></thead>");
                sb.Append("<tbody>");

                for (int i = 0; i < subCount; i++)
                {
                    sb.Append(GenrateSubLine(category, i, canEdit));
                }

                sb.Append("</tbody>");
                sb.Append("</table>");
                sb.AppendFormat("<script language='javascript'>var subdata_{0} = {1}</script>", category.VisibleFields[0].SubDataType, Newtonsoft.Json.JsonConvert.SerializeObject(GenrateSubLine(category, -1, canEdit)));
            }
            return sb.ToString();
        }

        public static string GenrateSubLine(FieldCategory category, int dataIndex, bool delButton)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<tr><td nowrap style='padding-top:8px;padding-left:8px'>{0}{1}</td>", delButton ? "<a href='javascript:void(0)' onclick='add" + category.SubFields[0].SubDataType + "Detail(\"" + category.VisibleFields[0].SubDataType + "\", this)'><i class='green icon-plus bigger-130'></i></a>&nbsp;<a href='javascript:void(0)' onclick='removeDetail(this, \"" + category.VisibleFields[0].SubDataType + "\")'><i class='red icon-remove bigger-130'></i></a>" : "", CreateSubHiddenFields(dataIndex, category));
            foreach (FieldInfo f in category.VisibleFields)
            {
                if (category.CategoryName == "SCIBOM" && f.FieldName == "MaterialType")
                {
                    string a = "1";
                }
                f.DataIndex = dataIndex;
                sb.AppendFormat("<td style='padding:0px'>{0}</td>", UIComponent.CreateDetailComponent(f));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string CreateHiddenFields(FieldCategory category)
        {
            StringBuilder sb = new StringBuilder();
            foreach (FieldInfo f in category.HiddenFields)
            {
                string comVal = UIComponent.GetComponentValue(f);
                sb.AppendFormat("<input class=\"form-field\" type=\"hidden\" id=\"{0}\" name=\"{0}\" value=\"{1}\" />", f.FieldName, comVal);
            }
            return sb.ToString();
        }

        public static string CreateSubHiddenFields(int dataIndex, FieldCategory category)
        {
            StringBuilder sb = new StringBuilder();
            foreach (FieldInfo f in category.HiddenFields)
            {
                f.DataIndex = dataIndex;
                string comVal = UIComponent.GetComponentValue(f);
                sb.AppendFormat("<input class=\"form-field\" type=\"hidden\" id=\"{0}\" name=\"{0}\" value=\"{1}\" />", f.FieldName, comVal);
            }
            return sb.ToString();
        }

        private static void MarkRequiredFields(FieldInfoCollecton fields)
        {
            foreach (FieldInfo f in fields)
            {
                f.WFRequiredOption = f.Options.Required == true ? RequiredOption.Required : RequiredOption.Optional;
            }
        }
    }
}
