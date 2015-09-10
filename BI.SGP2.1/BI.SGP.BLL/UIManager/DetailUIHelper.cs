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
    public class DetailUIHelper
    {
        public static string GenrateCategories(string categoryType, WFTemplate template)
        {
            return GenrateCategories(FieldCategory.GetMasterCategorys(categoryType), template);
        }

        public static string GenrateCategories(List<FieldCategory> categories, WFTemplate template)
        {
            StringBuilder html = new StringBuilder();
            foreach (FieldCategory fc in categories)
            {
                html.Append(GenrateCategory(fc, template));
            }
            return html.ToString();
        }

        public static string GenrateCategory(FieldCategory category, WFTemplate template)
        {
            WFActivity curAct = template.CurrentActivity == null ? template.FirstActivity : template.CurrentActivity;
            int i=0;
            foreach(WFActivity wf in curAct.CurrentChildActivities)
            {
                i = wf.ID;
            }

            string[] collapseds = category.ActivityID.Split(',');


            string collapsed = collapseds.Contains(curAct.ID.ToString())? "collapse": "in";

            if (i > 0)
            {
                collapsed = collapseds.Contains(i.ToString()) ? "collapse" : "in";
            }
            string upload = category.Upload == 1 ? String.Format("<a href='javascript:void(0)' onclick='return showFilesDialog(\"{0}\",\"{1}\");' title='Upload Attachment'><i class='pull-right icon-paper-clip'></i></a>", category.ID, category.Description) : "";
            StringBuilder html= new StringBuilder();
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
                                            </form>", category.ID, category.Description, DetailUIHelper.GenrateCategoryFields(category, template), collapsed, upload);

            foreach (FieldCategory fc in category.SubCategory)
            {
                html.AppendFormat("<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>{0}</div><form id='fm-{1}' class='fm-category' style='overflow:auto'><table style='width:100%'>", fc.Description, fc.ID);
                html.Append(DetailUIHelper.GenrateCategoryFields(fc, template));
                html.Append("</table></form>");
            }

            html.Append("</div></div></div>");

            return html.ToString();
        }

        public static string GenrateCategoryFields(FieldCategory category, WFTemplate template)
        {
            StringBuilder sb = new StringBuilder();
            MarkRequiredFields(category.MasterFields, template);
            MarkRequiredFields(category.SubFields, template);
            if (category.CategoryName == "FPCPriceDetail" || category.CategoryName == "FPCToolingSummary")
            {
                sb.Append(GenrateCategoryDetail(category));
            }
            else if (category.CategoryName == "VVI-Product Information")
            {
                sb.Append(GenrateCategoryMasterFields(category));
                sb.Append(GenrateCategorySubFieldsForVVI(category));
            }
            else
            {
                sb.Append(GenrateCategoryMasterFields(category));
                sb.Append(GenrateCategorySubFields(category));
            }
            return sb.ToString();
        }

        public static string GenrateCategoryMasterFields(FieldCategory category)
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

                if (colSpanTotal <= colSpan)
                {
                    sb.Append(UIComponent.CreateDetailComponentAndLable(enablefields[i]));
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
                sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
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

        public static string GenrateCategorySubFieldsForVVI(FieldCategory category)
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
                
                sb.AppendFormat("<th nowrap style='padding:8px;width:30px'></th>");
                sb.AppendFormat(@"<th nowrap style='padding:8px;width:50px'></th>");
                sb.Append(sbSubLine);
                sb.Append("</tr></thead>");
                sb.Append("<tbody>");

                //if (subCount == 0)
                //{
                //    sb.Append(GenrateSubLineForVVI(category.SubFields, -1, false));
                //}
                //else
                //{
                    for (int i = 0; i < subCount; i++)
                    {
                        sb.Append(GenrateSubLineForVVI(category.SubFields, i, canEdit && i > 0));
                    }
                //}

                sb.Append("</tbody>");
                sb.Append("</div></table>");
              //  sb.AppendFormat("<script language='javascript'>var subdata_{0} = {1}</script>", category.SubFields[0].SubDataType, Newtonsoft.Json.JsonConvert.SerializeObject(GenrateSubLine(category.SubFields, -1, true)));
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
                sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string GenrateSubLineForVVI(FieldInfoCollecton subFields, int dataIndex, bool delButton)
        {
            StringBuilder sb = new StringBuilder();
  
             
            foreach (FieldInfo f in subFields)
            {
                f.DataIndex = dataIndex;
                if (!string.IsNullOrWhiteSpace(f.SubDataType) && f.SubDataType.ToUpper() == "VVIDETAIL")
                {
                    object f19 = subFields["FLOAT19"].DataValue;
                    object f18 = subFields["FLOAT18"].DataValue;
                    object f17 = subFields["FLOAT17"].DataValue;
                    object n7 = subFields["NVARCHAR7"].DataValue;
                    FieldInfo nvarchar7 = subFields["NVARCHAR7"];
                    double d = 0;
                    double v = 0;
                    double w = 0;
                    if (f19 is ArrayList)
                    {
                        ArrayList arrayf19 = f19 as ArrayList;
                        ArrayList arrayf18 = f18 as ArrayList;
                        ArrayList arrayf17 = f17 as ArrayList;
                        ArrayList arrayn7=n7 as ArrayList;
                        double.TryParse(arrayf18[dataIndex].ToString(), out v);
                        double.TryParse(arrayf17[dataIndex].ToString(), out w);

                        if (v <= 0)
                        {
                            string vendorcode = arrayn7[dataIndex].ToString().Substring(0, arrayn7[dataIndex].ToString().IndexOf('['));
                            double xplan = (double)DbHelperSQL.GetSingle("select top 1 xplan from sys_supplier where supplycode=@supplycode", new SqlParameter("@supplycode", vendorcode));
                            v = xplan;
                            arrayf18[dataIndex] = xplan;
                            f18 = arrayf18;
                        }
                        if (v > 0 && w > 0)
                        {
                            d = v + w;
                            arrayf19[dataIndex] = d;
                            f19 = arrayf19;
                        }

                    }
                    else
                    {

                        double.TryParse(f18.ToString(), out v);
                        double.TryParse(f17.ToString(), out w);
                        if (v <= 0)
                        {
                            string vendorcode = nvarchar7.GetFieldValue().ToString().Substring(0, nvarchar7.GetFieldValue().ToString().IndexOf('['));
                            double xplan = (double)DbHelperSQL.GetSingle("select top 1 xplan from sys_supplier where supplycode=@supplycode", new SqlParameter("@supplycode", vendorcode));
                            v = xplan;
                            f18 = v;
                        }
                        if (v > 0 && w > 0)
                        {
                            d = v + w;
                            f19 = d;
                        }
                    }
                    if (f.FieldName == "FLOAT18" )
                    {
                        f.DataValue = f18;
                    }
                    if (f.FieldName == "FLOAT19" )
                    {
                        f.DataValue = f19;
                    }
                }

                if (f.FieldName == "NVARCHAR1" && !string.IsNullOrWhiteSpace(f.SubDataType) && f.SubDataType.ToUpper() == "VVIDETAIL")
                {
                    sb.AppendFormat(@"<tr>");

                    VVIQuotationDetail dm = new VVIQuotationDetail();

                    if (dm.CheckClosedByNumber(f.GetFieldValue().ToString()) == true)
                    {
                        if (dm.CheckSubmitToVVIByNumber(f.GetFieldValue().ToString()) == false)
                        {
                            sb.AppendFormat(@"<td nowrap style='padding:8px;'><label><input type='checkbox' name='SupplierRFQ' checkname='checkbox{0}' value='{0}'><span class='lbl'></span></label></td>", f.GetFieldValue().ToString());
                            sb.AppendFormat(@"<td nowrap style='padding:8px; width:40px '><div title=""return"" style=""float:left;cursor:pointer;"" class=""ui-pg-div ui-inline-edit"" onclick=""returnvendorrfq('{0}')"" onmouseover=""jQuery(this).addClass('ui-state-hover');"" onmouseout=""jQuery(this).removeClass('ui-state-hover')""><span class=""ui-icon icon-reply red""></span>&nbsp;</div>", f.GetFieldValue().ToString());
                            // sb.AppendFormat(@"<div title=""redo"" style=""float:left;cursor:pointer;""  onclick="""" ><span class=""ui-icon icon-refresh gray""></span></div></td>", f.GetFieldValue().ToString());
                        }
                        else if (dm.CheckRFQStatusByNumber(f.GetFieldValue().ToString()) == true)
                        {
                            sb.AppendFormat(@"<td nowrap style='padding:8px;'><label><input type='checkbox' name='SupplierRFQ' checkname='checkbox{0}' value='{0}'><span class='lbl'></span></label></td>", f.GetFieldValue().ToString());
                            sb.AppendFormat(@"<td nowrap style='padding:8px; width:40px '><div title=""return"" style=""float:left;cursor:pointer;"" class=""ui-pg-div ui-inline-edit"" onclick=""returnvendorrfq('{0}')"" onmouseover=""jQuery(this).addClass('ui-state-hover');"" onmouseout=""jQuery(this).removeClass('ui-state-hover')""><span class=""ui-icon icon-reply red""></span>&nbsp;</div>", f.GetFieldValue().ToString());
                        }
                        else
                        {

                            sb.AppendFormat(@"<td nowrap style='padding:8px;'><label><input type='checkbox' disabled='disabled' name='SupplierRFQ' checkname='checkbox{0}' value='{0}'><span class='lbl'></span></label></td>", f.GetFieldValue().ToString());
                            sb.AppendFormat(@"<td nowrap style='padding:8px; width:40px '>");
                            //sb.AppendFormat(@"<div title=""return"" disabled='disabled' style=""float:left;cursor:pointer; color:gray"" class=""ui-pg-div ui-inline-edit"" onclick="""" onmouseover="""" onmouseout=""""><span class=""ui-icon icon-reply gray""></span>&nbsp;</div>", f.GetFieldValue().ToString());
                            sb.AppendFormat(@"<div title=""redo"" style=""float:left;cursor:pointer;"" class=""ui-pg-div ui-inline-edit"" onclick=""redovendorrfq('{0}')"" onmouseover=""jQuery(this).addClass('ui-state-hover');"" onmouseout=""jQuery(this).removeClass('ui-state-hover')""><span class=""ui-icon icon-refresh green""></span></div></td>", f.GetFieldValue().ToString());
                        }
                    }
                    else
                    {
                        sb.AppendFormat(@"<td nowrap style='padding:8px;'><label><span class='lbl'></span></label></td>", f.GetFieldValue().ToString());
                        sb.AppendFormat(@"<td nowrap style='padding:8px; width:40px '>");

                    }
                   
                   

                }
                if (!string.IsNullOrWhiteSpace(f.SubDataType) && f.SubDataType.ToUpper() == "VVIDETAIL" && f.FieldName.ToUpper() == "INT1")
                {
                    sb.AppendFormat("<td>{0}</td>", UIComponent.CreateWFStatusIconForVVI(f));
                }
                else
                {
                    sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
                }
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
