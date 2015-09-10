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
    public class CostModelUIHelper
    {
        public static string GenrateCategories(string categoryType)
        {
            return GenrateCategories(FieldCategory.GetMasterCategorys(categoryType));
        }

        public static string GenrateCategories(List<FieldCategory> categories)
        {
            StringBuilder html = new StringBuilder();
            html.AppendFormat(@"<div class='widget-box transparent' id='recent-box'>");
            html.AppendFormat(@"<div class='widget-header'>");
            html.AppendFormat(@"<div class='widget-toolbar no-border' style='float:left !important'>");
            html.AppendFormat(@"<ul class='nav nav-tabs' id='recent-tab'>");
            foreach (FieldCategory fc in categories)
            {
               // if (fc.SubCategory.Count < 1)
               // {
                    html.Append(GenrateMasterCategory(fc));
     //           }
//                else
//                {
//                    html.AppendFormat(@"<li class='dropdown'>");
//                    html.AppendFormat(@" <a data-toggle='dropdown' class='dropdown-toggle' href='#'>
//                                                    {0} &nbsp;
//                                                    <i class='icon-caret-down bigger-110 width-auto'></i>
//                                                </a>", fc.CategoryName);
//                    html.AppendFormat(@" <ul class='dropdown-menu dropdown-info'>");
//                    html.Append(GenrateSubCategory(fc));
//                    html.AppendFormat(@"</ul>");
//                    html.AppendFormat(@"</li>");
//                }
            }
            html.AppendFormat(@"</ul></div></div>");
            html.AppendFormat(@"<div class='widget-body'>");
            html.AppendFormat(@" <div class='widget-main padding-4'>
                                        <div class='tab-content padding-8 overflow-visible'>");

            foreach (FieldCategory fc in categories)
            {
               // if (fc.SubCategory.Count < 1)
               // {
                    html.Append(GenrateMasterBody(fc));
               // }
                //else
                //{

                //    html.Append(GenrateSubBody(fc));
                   
                //}

            }

            html.AppendFormat(@"</div></div></div></div>");

            return html.ToString();
        }
        public static string GenrateMasterCategory(FieldCategory category)
        {
            StringBuilder html = new StringBuilder();
            if (category.ID == "101" || category.ID=="127")
            {
                html.AppendFormat(@"<li class='active'>");
            }
            else
            {
                html.AppendFormat(@"<li>");
            }
            html.AppendFormat(@"  <a data-toggle='tab' href='#task-tab{0}'>{1}</a>", category.ID, category.CategoryName);
            html.AppendFormat(@"</li>");
            return html.ToString();
        }
        //public static string GenrateSubCategory(FieldCategory category)
        //{
        //    StringBuilder html = new StringBuilder();
        //    foreach (FieldCategory fc in category.GetSubTypeCategories())
        //    {
        //        html.AppendFormat(@"<li>");
        //        html.AppendFormat(@" <a data-toggle='tab' href='#dropdown{0}'>{1}</a>", fc.ID, fc.CategoryName);
        //        html.AppendFormat(@"</li>");
        //    }
        //    return html.ToString();
        //}

        public static string GenrateMasterBody(FieldCategory category)
        {
            StringBuilder html = new StringBuilder();
            if (category.ID == "101"|| category.ID=="127" )
            {
                html.AppendFormat(@"  <div id='task-tab{0}' class='tab-pane active'>", category.ID);
            }
            else
            {
                html.AppendFormat(@"  <div id='task-tab{0}' class='tab-pane'>", category.ID);
            }
            html.Append(GenrateCategoryMasterDetail(category));


            html.AppendFormat(@"</div>");
            return html.ToString();

        }

        public static string GenrateSubBody(FieldCategory category)
        {
            StringBuilder html = new StringBuilder();
            //foreach (FieldCategory fc in category.SubCategory)
            //{
              //  html.AppendFormat(@"<div id='dropdown{0}' class='tab-pane'>", fc.ID);
            
            foreach (FieldCategory fc in category.SubCategory)
            {
                if(fc.CategoryType=="FSCSI")
                {
                    html.Append(@"<div ng-show={{Fvaltype}} class={{Fclass}}>");
                }
                else
                {
                    html.Append(@"<div ng-show={{Rvaltype}} class={{Rclass}}>");
                }
                html.Append(GenrateCategorySubDetail(fc));
                html.Append("</div>");
            }
            
         
              //  html.AppendFormat(@"</div>");
            //}
            return html.ToString();
        }

    

        public static string GenrateCategoryMasterDetail(FieldCategory category)
        {
         
            StringBuilder sb = new StringBuilder();

          

            if (category.MasterFields.Count>0)
            {

                sb.AppendFormat(@"<div class='panel panel-default'>
                                    <div class='panel-heading'>
                                        <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle'>
                                            <i class='pull-right icon-chevron-down' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                            <i class='icon-user bigger-130'></i>&nbsp; {1}:
                                        </a>
                                    </div>
                                    <div class='panel-collapse in' id='faq-1-{0}' style='height: auto;'>
                                        <div class='panel-body'>
                                            <form id='fm-{0}'  class='fm-category'>
                                                <table style='width:100%'>
                                                    {2}
                                                </table>
                                            </form>", category.ID,category.Description, GenrateCategoryMasterFields(category));
                
                sb.Append(GenrateSubBody(category));
               
                sb.AppendFormat("</div></div></div>");
            }
            else
            {

                string upload = category.Upload == 1 ? String.Format("<a href='javascript:void(0)' onclick='return showFilesDialog(\"{0}\",\"{1}\");' title='Upload Attachment'><i class='pull-right icon-cloud-upload  bigger-150'></i></a>", category.ID, category.Description) : "";
                
                if(category.SubCategory.Count>0)
                {

                    
                        sb.Append(GenrateSubBody(category));
                   

                }
                else
                {
                    sb.AppendFormat(@"<div  ng-controller=""ControllerID{0}"" class='panel panel-default'>
                                    <div class='panel-heading'>
                                        <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle'>
                                            <i class='pull-right icon-chevron-down' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                            <i class='icon-user bigger-130'></i>&nbsp; {1}:
                                        </a>{3}
                                    </div>
                                    <div class='panel-collapse {2}' id='faq-1-{0}' style='height: auto;'>
                                        <div>", category.ID, category.Description, "in", upload);
                    sb.AppendFormat(@" <form id='fm-{0}'  class='fm-category'>
                               
                                    {1}
                               
                          </form>", category.ID, GenrateCategorySubFields(category));

                    sb.AppendFormat("</div></div></div>");

                }
                
               
            }
            return sb.ToString();
        }
        public static string GenrateCategorySubDetail(FieldCategory category)
        {
            StringBuilder sb = new StringBuilder();
            
            //foreach(FieldCategory fc in FieldCategory.GetChildCategories(category))
            //{
            string upload = category.Upload == 1 ? String.Format("<a href='javascript:void(0)' onclick='return showFilesDialog(\"{0}\",\"{1}\");' title='Upload Attachment'><i class='pull-right icon-cloud-upload  bigger-150'></i></a>", category.ID, category.Description) : "";
                sb.AppendFormat(@"<div  ng-controller=""ControllerID{0}"" class='panel panel-default'>
                                    <div class='panel-heading'>
                                        <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle'>
                                            <i class='pull-right icon-chevron-down' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                            <i class='icon-user bigger-130'></i>&nbsp; {1}:
                                        </a>{3}
                                    </div>
                                    <div class='panel-collapse {2}' id='faq-1-{0}' style='height: auto;'>
                                        <div>", category.ID, category.Description, "in", upload);
                sb.AppendFormat(@" <form id='fm-{0}'  class='fm-category'>
                             
                                    {1}
                              
                          </form>", category.ID, GenrateCategorySubFields(category));

                sb.AppendFormat("</div></div></div>");
            
            //}

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
        public static string GenrateCategorySubFields(FieldCategory category)
        {
            int colSpan = category.ColSpan;
            bool canEdit = true;
            StringBuilder sb = new StringBuilder();
            if (category.SubFields.Count > 0)
            {
                sb.AppendFormat(@"<table  style=""width:100%"" ><thead><tr style='background-image:-webkit-gradient(linear,left 0,left 100%,from(#f8f8f8),to(#ececec))'>", category.ID);
                sb.AppendFormat("<th style='padding:8px;width:30px'>{0}</th>", canEdit ? "<a href='javascript:void(0)' ng-click='addrow()' ><i class='green icon-plus bigger-130'></i></a>" : "");
                foreach(FieldInfo f in category.SubFields)
                {
                    string fontColor = f.WFRequiredOption == RequiredOption.Required ? ";color:red" : "";
                    sb.AppendFormat("<th nowrap style='padding:8px;width:{0}px{1}'>{2}</th>", f.Width == 0 ? 100 : f.Width,fontColor, f.DisplayName);

                }
                sb.AppendFormat("</tr></thead>");
                sb.AppendFormat("<tbody>");
                sb.Append(GenrateSubLine(category.SubFields, -1, true));
                sb.AppendFormat("</tbody></table>");

                StringBuilder sbfi = new StringBuilder();
                StringBuilder sbnewrow = new StringBuilder();

                int subcount = 0;
                if(category.SubFields.Count>0)
                {
                    if(category.SubFields[0].DataValue is ArrayList)
                    {
                        subcount = ((ArrayList)category.SubFields[0].DataValue).Count;
                    }
                    else
                    {
                        subcount = 1;
                    }
                }

                sbnewrow.Append(@"{");
                foreach (FieldInfo fi in category.SubFields)
                {
                    sbnewrow.Append(fi.FieldName + ": \" \" ,");
                }
                sbnewrow = sbnewrow.Remove(sbnewrow.ToString().Length - 1, 1);
                sbnewrow.Append(@"}");

                if (subcount == 1)
                {
                    sbfi.Append(@"{");
                    foreach (FieldInfo fi in category.SubFields)
                    {
                        sbfi.Append(fi.FieldName + ":" + "\"" + fi.DataValue + "\"" + ",");
                    }
                    sbfi = sbfi.Remove(sbfi.ToString().Length - 1, 1);
                    sbfi.Append(@"}");
                }
                else
                {
                    for (int i = 0; i < subcount; i++)
                    {
                        sbfi.Append(@"{");
                        foreach (FieldInfo fi in category.SubFields)
                        {
                            if (fi.DataValue is ArrayList)
                            {
                                ArrayList dataval = (ArrayList)fi.DataValue;
                                sbfi.Append(fi.FieldName + ":" + "\"" + dataval[i].ToString() + "\"" + ",");
                            }
                            else
                            {
                                sbfi.Append(fi.FieldName + ":" + "\"\"" + ",");
                            }
                        }
                        sbfi = sbfi.Remove(sbfi.ToString().Length - 1, 1);
                        sbfi.Append(@"},");
                    }
                    sbfi = sbfi.Remove(sbfi.ToString().Length - 1, 1);
                }
           
               sb.Append(@"<script>");
               sb.Append(@" function ControllerID" + category.ID + "($scope) {");
               sb.Append(@"              $scope.items = [" + sbfi.ToString() + "];");
               sb.Append(@"                    $scope.removerow = function (index) {");
               sb.Append(@"                       $scope.items.splice(index, 1);");
               sb.Append(@"                   };");
               sb.Append(@"                   $scope.addrow= function () {");
               sb.Append(@"                      $scope.items.splice(0, 0, " + sbnewrow.ToString() + ");");
               sb.Append(@"                  };");
               sb.Append(@"                }");
               sb.Append(@"</script>");
               //  sb.AppendFormat("<script language='javascript'>var subdata_{0} = {1}</script>", category.SubFields[0].SubDataType, Newtonsoft.Json.JsonConvert.SerializeObject(GenrateSubLine(category.SubFields, -1, true)));
                

                 
            }
            return sb.ToString();
        }
        //public static string GenrateCategorySubFields(FieldCategory category)
        //{
        //    int colSpan = category.ColSpan;
        //    StringBuilder sb = new StringBuilder();
        //    if (category.SubFields.Count > 0)
        //    {
        //        int subCount = 0;
        //        bool canEdit = false;
        //        sb.AppendFormat("<tr><td colspan='{0}'><div style='overflow:auto;' class='detail-subdata-list'><table id='tb-{1}' style='width:100%;margin-bottom:0px;' class='table table-striped table-bordered table-hover'>", colSpan, category.SubFields[0].SubDataType);
        //        sb.Append("<thead><tr style='background-image:-webkit-gradient(linear,left 0,left 100%,from(#f8f8f8),to(#ececec))'>");
        //        StringBuilder sbSubLine = new StringBuilder();
        //        foreach (FieldInfo f in category.SubFields)
        //        {
        //            if (subCount == 0 && f.DataValue is ArrayList)
        //            {
        //                subCount = ((ArrayList)f.DataValue).Count;
        //            }
        //            string fontColor = f.WFRequiredOption == RequiredOption.Required ? ";color:red" : "";
        //            if (!canEdit && f.Visible != 0)
        //            {
        //                canEdit = true;
        //            }
        //            sbSubLine.AppendFormat("<th nowrap style='padding:8px;width:{0}px{1}'>{2}</th>", f.Width == 0 ? 100 : f.Width, fontColor, f.DisplayName);
        //        }

        //        sb.AppendFormat("<th style='padding:8px;width:30px'>{0}</th>", canEdit ? "<a href='javascript:void(0)' ng-click='addrow()' onclick='addDetail(\"" + category.SubFields[0].SubDataType + "\")'><i class='green icon-plus bigger-130'></i></a>" : "");
        //        sb.Append(sbSubLine);
        //        sb.Append("</tr></thead>");
        //        sb.Append("<tbody>");

        //        if (subCount == 0)
        //        {
        //            sb.Append(GenrateSubLine(category.SubFields, -1, false));
        //        }
        //        else
        //        {
        //            for (int i = 0; i < subCount; i++)
        //            {
        //                sb.Append(GenrateSubLine(category.SubFields, i, canEdit && i > 0));
        //            }
        //        }

        //        sb.Append("</tbody>");
        //        sb.Append("</div></table>");
        //        sb.AppendFormat("<script language='javascript'>var subdata_{0} = {1}</script>", category.SubFields[0].SubDataType, Newtonsoft.Json.JsonConvert.SerializeObject(GenrateSubLine(category.SubFields, -1, true)));
        //        sb.Append("</td></tr>");
        //    }
        //    return sb.ToString();
        //}

        public static string GenrateSubLine(FieldInfoCollecton subFields, int dataIndex, bool delButton)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<tr ng-repeat='item in items'>");
            sb.AppendFormat(@"<td nowrap style='padding:8px;'>{0}</td>", delButton ? "<a href='javascript:void(0)' ng-click='removerow($index)'><i class='red icon-remove bigger-130'></i></a>" : "");
            foreach (FieldInfo f in subFields)
            {

                sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
            }
            sb.AppendFormat(@"</tr>");
            return sb.ToString();
        }

        //public static string GenrateSubLine(FieldInfoCollecton subFields, int dataIndex, bool delButton)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat("<tr><td nowrap style='padding:8px;'>{0}</td>", delButton ? "<a href='javascript:void(0)'  onclick='removeDetail(this)'><i class='red icon-remove bigger-130'></i></a>" : "");
        //    foreach (FieldInfo f in subFields)
        //    {
        //        f.DataIndex = dataIndex;
        //        sb.AppendFormat("<td>{0}</td>", UIComponent.CreateDetailComponent(f));
        //    }
        //    sb.Append("</tr>");
        //    return sb.ToString();
        //}
    }
}
