using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Models.Detail;

namespace BI.SGP.BLL.UIManager
{
    public class CustomerUIManager
    {
        public static string GenrateCategories(string categoryType, string pageType)
        {
            return GenrateCategories(FieldCategory.GetMasterCategorys(categoryType), pageType);
        }

        public static string GenrateCategories(List<FieldCategory> categories, string pageType)
        {
            StringBuilder html = new StringBuilder();
            foreach (FieldCategory fc in categories)
            {
                html.Append(GenrateCategory(fc, pageType));
            }
            return html.ToString();
        }

        public static string GenrateCategory(FieldCategory category, string pageType)
        {

            foreach (FieldInfo f in category.MasterFields)
            {
                if (",Market,PurchasingManager,SupplierQualityEngineer,VPofSupplyChainPurchasing,President,FY18Forecast,".IndexOf("," + f.FieldName + ",") != -1)
                {
                    f.ExtEmptyCol = 1;
                    f.ExtEmptyColSpan = 2;
                }
                if (",FY18Forecast,".IndexOf("," + f.FieldName + ",") != -1)
                {
                    f.ExtEmptyCol = 1;
                    f.ExtEmptyColSpan = 4;
                }
            }

            int i = 0;

            string[] collapseds = category.ActivityID.Split(',');


            string collapsed = "";

            if (i > 0)
            {
                collapsed = collapseds.Contains(i.ToString()) ? "collapse" : "in";
            }
            StringBuilder html = new StringBuilder();
            if (pageType == "Detail")
            {
                string upload = category.Upload == 1 ? String.Format("<a href='javascript:void(0)' onclick='return showFilesDialog(\"{0}\",\"{1}\");' title='Upload Attachment'><i class='pull-right icon-paper-clip'></i></a>", category.ID, category.Description) : "";
                html.AppendFormat(@"<div class='panel panel-default'>
                                    <div class='panel-heading'>
                                        <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle'>
                                            <i class='pull-right icon-chevron-down' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                            <i class='icon-user bigger-130'></i>&nbsp; {1}:
                                        </a>{4}
                                    </div>
                                    <div class='panel-collapse {3}' id='faq-1-{0}' style='height: auto;'>
                                        <div class='panel-body'>
                                            <form id='fm-{0}' class='fm-category'>
                                                <table style='width:100%'>
                                                    {2}
                                                </table>
                                            </form>", category.ID, category.Description, CustomerUIManager.GenrateCategoryFields(category, pageType), collapsed, upload);

                foreach (FieldCategory fc in category.SubCategory)
                {
                    html.AppendFormat("<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>{0}</div><form id='fm-{1}' class='fm-category' style='overflow:auto'><table style='width:100%'>", fc.Description, fc.ID);
                    html.Append(CustomerUIManager.GenrateCategoryFields(fc, pageType));
                    html.Append("</table></form>");
                }

                html.Append("</div></div></div>");
            }
            else
            {
                html.AppendFormat(@"<div class='panel'>                                    
                                    <div><h3 align='center'>{1}</h3></div>
                                            <form id='fm-{0}' class='fm-category'>
                                                <table style='width:100%' border='1' align='center' cellpadding='0' cellspacing='0' bordercolor='#000000' >
                                                    {2}
                                                </table>
                                            </form>", category.ID, category.Description, CustomerUIManager.GenrateCategoryFields(category, pageType), collapsed);

                foreach (FieldCategory fc in category.SubCategory)
                {
                    html.AppendFormat("<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>{0}</div><form id='fm-{1}' class='fm-category' style='overflow:auto'><table style='width:100%'>", fc.Description, fc.ID);
                    html.Append(CustomerUIManager.GenrateCategoryFields(fc, pageType));
                    html.Append("</table></form>");
                }

                html.Append("</div>");
            }

            return html.ToString();
        }

        public static string GenrateCategoryFields(FieldCategory category, string pageType)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GenrateCategoryMasterFields(category, pageType));
            sb.Append(GenrateCategorySubFields(category));
            return sb.ToString();
        }

        public static string GenrateCategoryMasterFields(FieldCategory category, string pageType)
        {
            StringBuilder sb = new StringBuilder();
            FieldInfoCollecton enablefields = category.MasterFields;
            int colSpan = category.ColSpan;
            double RowCount = enablefields.Count;

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

                if (enablefields[i].ExtEmptyCol == 1 && enablefields[i].ExtEmptyColSpan > 0)
                {
                    colSpanTotal += enablefields[i].ExtEmptyColSpan;
                }

                if (colSpanTotal <= colSpan)
                {
                    sb.Append(CustomerUIComponent.CreateDetailComponentAndLable(enablefields[i], pageType));
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

            return sb.ToString();
        }
        public static string GenrateCategoryDetail(FieldCategory category)
        {
            int colSpan = category.ColSpan;
            StringBuilder sb = new StringBuilder();
            if (category.MasterFields.Count > 0)
            {

                sb.AppendFormat("<table class='table table-bordered table-striped' style='margin-bottom:0px;'>");
                sb.Append("<thead><tr>");
                List<string> FieldDisplayName = new List<string>();
                FieldInfoCollecton fic = new FieldInfoCollecton();
                StringBuilder sbSubLine = new StringBuilder();
                foreach (FieldInfo f in category.MasterFields)
                {
                    if (!FieldDisplayName.Contains(f.DisplayName))
                    {
                        FieldDisplayName.Add(f.DisplayName);
                        string fontColor = f.WFRequiredOption == RequiredOption.Required ? ";color:red" : "";
                        sbSubLine.AppendFormat("<th nowrap style='padding:8px;width:{0}px{1}'>{2}</th>", f.Width == 0 ? 100 : f.Width, fontColor, f.DisplayName);
                    }
                }
                sb.Append(sbSubLine);
                sb.Append("</tr></thead>");
                sb.Append("<tbody>");

                for (int i = 1; i <= 5; i++)
                {
                    foreach (FieldInfo f in category.MasterFields)
                    {
                        if (f.FieldName.EndsWith(i.ToString()))
                        {
                            fic.Add(f);
                        }
                    }
                    sb.Append(GenrateTableBody(fic));
                    fic.Clear();
                }

                sb.Append("</tbody>");
                sb.Append("</table>");
            }
            return sb.ToString();
        }

        public static string GenrateTableBody(FieldInfoCollecton Fields)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<tr>");

            foreach (FieldInfo f in Fields)
            {
                sb.AppendFormat("<td>{0}</td>", CustomerUIComponent.CreateDetailComponent(f));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string GenrateCategorySubFields(FieldCategory category)
        {
            int colSpan = category.ColSpan;
            StringBuilder sb = new StringBuilder();
            if (category.SubFields.Count > 0)
            {
                int subCount = 0;
                bool canEdit = false;
                sb.AppendFormat("<tr><td colspan='{0}'><div style='overflow:auto;' class='detail-subdata-list'><table id='tb-{1}' style='width:100%;margin-bottom:0px;' class='table table-striped table-bordered table-hover'>", colSpan, category.SubFields[0].SubDataType);
                sb.Append("<thead><tr style='background-image:-webkit-gradient(linear,left 0,left 100%,from(#f8f8f8),to(#ececec))'>");
                StringBuilder sbSubLine = new StringBuilder();
                foreach (FieldInfo f in category.SubFields)
                {
                    if (subCount == 0 && f.DataValue is ArrayList)
                    {
                        subCount = ((ArrayList)f.DataValue).Count;
                    }
                    string fontColor = f.WFRequiredOption == RequiredOption.Required ? ";color:red" : "";
                    if (!canEdit && f.Visible != 0)
                    {
                        canEdit = true;
                    }
                    sbSubLine.AppendFormat("<th nowrap style='padding:8px;width:{0}px{1}'>{2}</th>", f.Width == 0 ? 100 : f.Width, fontColor, f.DisplayName);
                }

                sb.AppendFormat("<th style='padding:8px;width:30px'>{0}</th>", canEdit ? "<a href='javascript:void(0)' onclick='addDetail(\"" + category.SubFields[0].SubDataType + "\")'><i class='green icon-plus bigger-130'></i></a>" : "");
                sb.Append(sbSubLine);
                sb.Append("</tr></thead>");
                sb.Append("<tbody>");

                if (subCount == 0)
                {
                    sb.Append(GenrateSubLine(category.SubFields, -1, false));
                }
                else
                {
                    for (int i = 0; i < subCount; i++)
                    {
                        sb.Append(GenrateSubLine(category.SubFields, i, canEdit && i > 0));
                    }
                }

                sb.Append("</tbody>");
                sb.Append("</div></table>");
                sb.AppendFormat("<script language='javascript'>var subdata_{0} = {1}</script>", category.SubFields[0].SubDataType, Newtonsoft.Json.JsonConvert.SerializeObject(GenrateSubLine(category.SubFields, -1, true)));
                sb.Append("</td></tr>");
            }
            return sb.ToString();
        }

        public static string GenrateSubLine(FieldInfoCollecton subFields, int dataIndex, bool delButton)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<tr><td nowrap style='padding:8px;'>{0}</td>", delButton ? "<a href='javascript:void(0)' onclick='removeDetail(this)'><i class='red icon-remove bigger-130'></i></a>" : "");
            foreach (FieldInfo f in subFields)
            {
                f.DataIndex = dataIndex;
                sb.AppendFormat("<td>{0}</td>", CustomerUIComponent.CreateDetailComponent(f));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        private static void MarkRequiredFields(FieldInfoCollecton fields, WFTemplate template)
        {
            if (fields != null)
            {
                WFActivity curAct = template.CurrentActivity == null ? template.FirstActivity : template.CurrentActivity;
                AccessServiceReference.Role[] role = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Roles;

                List<WFActivityField> checkFields = curAct.GetCheckFields();
                foreach (WFActivity wca in curAct.CurrentUserChildActivities)
                {
                    List<WFActivityField> ccf = wca.GetCheckFields();
                    if (ccf != null)
                    {
                        if (checkFields == null)
                        {
                            checkFields = ccf;
                        }
                        else
                        {
                            checkFields.AddRange(ccf);
                        }
                    }
                }

                foreach (FieldInfo f in fields)
                {
                    if (checkFields != null)
                    {
                        WFActivityField wff = checkFields.Find(t => t.FieldID == f.ID);
                        if (wff != null)
                        {
                            f.WFRequiredOption = wff.IsRequired == true ? RequiredOption.Required : RequiredOption.Optional;
                        }
                        else
                        {
                            f.WFRequiredOption = RequiredOption.None;
                            //if (!(role.Contains("SGP_BDMGAM") || role.Contains("SGP_Management") || role.Contains("SGP_RFQPrimaryContact")))
                            //{
                            //    f.Visible = 0;
                            //}
                        }
                    }
                    //else
                    //{
                    //    f.Visible = 0;
                    //}
                }
            }
        }

        public static List<T> SubDataToList<T>(object data)
        {
            List<T> list = new List<T>();
            if (data is ArrayList)
            {
                ArrayList arrData = (ArrayList)data;
                for (int i = 0; i < arrData.Count; i++)
                {
                    list.Add(ParseHelper.Parse<T>(arrData[i]));
                }
            }
            else
            {
                list.Add(ParseHelper.Parse<T>(data));
            }
            return list;
        }
    }
}
