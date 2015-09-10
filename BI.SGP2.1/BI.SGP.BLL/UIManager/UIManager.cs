using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BI.SGP.BLL.DataModels;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.Utils;
using System.Web;

namespace BI.SGP.BLL.UIManager
{
    /// <summary>
    /// 自动生成页面控件，根据Field的配置信息
    /// </summary>
    public static class UIManager
    {
        /// <summary>
        /// 生成整个页面
        /// </summary>
        /// <returns></returns>
        private static string GenrateModel(object data)
        {
            //所有需要显示的字段，从Field Mnager中读出
            List<FieldCategory> allGategory = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SGP);
            StringBuilder sb = new StringBuilder();

            //将data实体的数据赋值到FIELD的DATAVALUE中
            if (data != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
                    //if (currentRole != fc.AllowedRoles) continue;
                    foreach (BI.SGP.BLL.DataModels.FieldInfo field in fc.Fields)
                    {
                        PropertyInfo propertyInfo = data.GetType().GetProperty(field.FieldName);
                        if (propertyInfo != null)
                        {
                            field.DataValue = propertyInfo.GetValue(data);
                        }
                    }
                }
            }

            int ID = 1;
            foreach (FieldCategory fc in allGategory)
            {
                String GRole = fc.AllowedRoles;

                string[] GategoryRoles = GRole.Split(',');

                if (!GategoryRoles.Contains("ALL"))
                {
                    AccessServiceReference.Role[] role = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Roles;
                    List<string> CurrRole = new List<string>();
                    if (role != null)
                    {
                        foreach (AccessServiceReference.Role r in role)
                        {
                            CurrRole.Add(r.Name);
                        }
                    }

                    List<string> ExcepCurrRole = GategoryRoles.Except(CurrRole).ToList();
                    if (ExcepCurrRole.Count > 0)
                    {
                        continue;
                    }
                }

                ID++;

                if (fc.CategoryName == "Request Information")
                {
                    sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
                    sb.Append(@"<div class=""panel-collapse in"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");
                }
                else
                {
                    sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle collapsed"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
                    sb.Append(@"<div class=""panel-collapse collapse"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");
                }

                if (fc.CategoryName == "Price Detail")
                {
                    sb.AppendLine();
                    sb.Append(GenerateCategoryForVerticalDetail(fc));
                }
                else
                {
                    double RowCount = fc.Fields.Count;
                    int rowsize = 4;
                    if (fc.CategoryName == "Closure Status" || fc.CategoryName == "Request Information")
                    {
                        rowsize = 3;
                        sb.Append(@"<table>");
                        sb.Append("<thead style='width:100%'><tr><th style='width:15%'></th><th style='width:18.3333%'></th><th style='width:15%'></th><th style='width:18.3333%'></th><th style='width:15%'></th><th style='width:18.3333%'></th></tr></thead>");
                        sb.Append("<tdbody  style='width:100%'>");
                    }
                    else
                    {

                        sb.Append(@"<table>");
                        sb.Append("<thead style='width:100%'><tr><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th></tr></thead>");
                        sb.Append("<tdbody  style='width:100%'>");
                    }

                    double Rows = Math.Ceiling(RowCount / rowsize);

                    for (int i = 1; i <= (int)Rows; i++)
                    {
                        int startIndex = i * rowsize - rowsize;
                        int endIndex = i * rowsize;
                        sb.Append("<tr>");
                        for (int j = startIndex; j < endIndex && j < RowCount; j++)
                        {
                            if (fc.Fields[j].ColSpan >= 5)
                            {
                                sb.Append("<tr>");
                                sb.Append(GenerateField(fc.Fields[j]));
                                sb.Append("</tr>");
                            }
                            else
                            {
                                sb.Append(GenerateField(fc.Fields[j]));
                            }
                        }
                        sb.Append("</tr>");

                    }
                    sb.Append("</tdbody>");
                    sb.Append("</table>");
                }
                sb.Append(@"</div></div></div>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 初始化必填字段
        /// </summary>
        /// <param name="data"></param>
        /// <param name="allGategory"></param>
        private static void InitRequiredConfg(object data, List<FieldCategory> allGategory, out List<string> fieldsJustDisplay)
        {
            fieldsJustDisplay = new List<string>();
            BI.SGP.BLL.Models.RFQDetail detail = data as BI.SGP.BLL.Models.RFQDetail;

            int rfqId = 0; if (detail != null) rfqId = detail.RFQID;
            WF.WFTemplate wf = new WF.WFTemplate("DefaultWF", rfqId);

            List<WF.WFActivityField> wfFields = null; 
            if (wf.CurrentActivity != null)
            {
                wfFields = wf.CurrentActivity.GetCheckFields();
            }
            else
            {
                wfFields = wf.FirstActivity.GetCheckFields();
            }

            //取得必填项
            if (wfFields != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
                    //if (currentRole != fc.AllowedRoles) continue;
                    foreach (BI.SGP.BLL.DataModels.FieldInfo field in fc.Fields)
                    {
                        PropertyInfo propertyInfo = data.GetType().GetProperty(field.FieldName);
                        if (propertyInfo != null)
                        {
                            WF.WFActivityField wfFld = wfFields.Find(t => t.FieldName.CompareTo(field.FieldName) == 0);
                            if (wfFld != null)
                            {
                                field.WFRequiredOption = wfFld.IsRequired == true ? RequiredOption.Required : RequiredOption.Optional;
                                //加入到仅仅需要显示的列表中
                            }
                        }
                        if (field.WFRequiredOption == RequiredOption.None) fieldsJustDisplay.Add(field.FieldName);
                    }
                }
            }
        }

        private static string GenrateCategory(ref int ID, List<FieldCategory> allGategory, FieldCategory fc, object data, string ActivityID, bool needHeader)
        {
            StringBuilder sb = new StringBuilder();

            FieldCategory priceDetailCategroy = allGategory.Find(t => t.ID == "7");
            FieldCategory termsConditionsCategory = allGategory.Find(t => t.ID == "8");
            FieldCategory VVItermsConditionsCategory = allGategory.Find(t => t.ID == "9");
            FieldCategory VVIpriceDetailCategroy = allGategory.Find(t => t.ID == "15");

            String GRole = fc.AllowedRoles;

            string[] GategoryRoles = GRole.Split(',');

            if (!GategoryRoles.Contains("ALL"))
            {
                AccessServiceReference.Role[] role = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Roles;
                List<string> CurrRole = new List<string>();
                if (role != null)
                {
                    foreach (AccessServiceReference.Role r in role)
                    {
                        CurrRole.Add(r.Name);
                    }
                }

                List<string> ExcepCurrRole = GategoryRoles.Except(CurrRole).ToList();
                if (ExcepCurrRole.Count > 0)
                {
                    return string.Empty;
                }
            }
            ID++;
            string fcactivity = fc.ActivityID;
            if (needHeader)
            {
                if (fcactivity != null)
                {
                    string[] curractivitys = fcactivity.Split(',');
                    if (curractivitys.Contains(ActivityID))
                    {
                        sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle collapsed"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
                        sb.Append(@"<div class=""panel-collapse collapse"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");
                    }
                    else
                    {
                        sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
                        sb.Append(@"<div class=""panel-collapse in"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");
                    }
                }
                else
                {
                    sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
                    sb.Append(@"<div class=""panel-collapse in"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");
                }
            }

            sb.Append(@"<table>");
            {

                FieldInfoCollecton enablefields = new FieldInfoCollecton();

                foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fc.Fields)
                {
                    if (fi.Enable == 1)
                    {

                        enablefields.Add(fi);
                    }

                }


                double RowCount = enablefields.Count;
                int colSize = 8;
                if (fc.CategoryName == "Closure Status")
                {
                    colSize = 6;
                    sb.Append("<thead style='width:100%'><tr><th style='width:15%'></th><th style='width:18.3333%'></th><th style='width:15%'></th><th style='width:18.3333%'></th><th style='width:15%'></th><th style='width:18.3333%'></th></tr></thead>");
                }
                else if (fc.ID == "5") //价格区域
                {
                    colSize = 6;
                    sb.Append("<thead style='width:100%'><tr><th style='width:20%'></th><th style='width:13%'></th><th style='width:20%'></th><th style='width:13%'></th><th style='width:20%'></th><th style='width:14%'></th></tr></thead>");
                }
                else if (fc.ID == "9")
                {
                    colSize = 10;
                    sb.Append("<thead style='width:100%'><tr><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th><th style='width:10% !important'></th></tr></thead>");
                }
                else{

                    sb.Append("<thead style='width:100%'><tr><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th><th style='width:10%'></th><th style='width:15%'></th></tr></thead>");
                }

                sb.Append("<tbody  style='width:100%'>");

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
                    else if (enablefields[i].ColSpan > (colSize - 1))
                    {
                        curSpan = colSize - 1;
                    }
                    else
                    {
                        curSpan = enablefields[i].ColSpan;
                    }
                    colSpanTotal += curSpan;

                    if (colSpanTotal <= colSize)
                    {
                        sb.Append(GenerateField(enablefields[i]));
                    }

                    if (colSpanTotal == colSize)
                    {
                        sb.Append("</tr>");
                        colSpanTotal = 0;
                    }

                    if (colSpanTotal > colSize)
                    {
                        sb.Append("</tr>");
                        colSpanTotal = 0;
                        i--;
                    }
                }

                sb.Append("</tbody>");
                sb.Append("</table>");

                //价格区域
                if (fc.ID == "5")
                {
                    //绘制价格表格
                    //sb.AppendLine("<div style='padding:5px;'>");
                    sb.Append(GenerateCategoryForPriceDetail(priceDetailCategroy));
                    //sb.AppendLine("</div>");

                    //绘制Terms & Conditions头部
                    sb.AppendFormat(@"
<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>
{0}
</div>"
                , termsConditionsCategory.CategoryName);

                    //绘制Terms & Conditions主体
                    string s = GenrateCategory(ref ID, allGategory, termsConditionsCategory, data, ActivityID, false);
                    sb.AppendLine(s);
                }


                if (fc.ID == "15")
                {
                    //绘制价格表格
                    //sb.AppendLine("<div style='padding:5px;'>");
                    sb.Append(GenerateCategoryForVerticalDetail(VVIpriceDetailCategroy));
                    //sb.AppendLine("</div>");

                

                    
                }



                if (fc.ID == "4")
                {
                    //绘制Terms & Conditions头部
                    sb.AppendFormat(@"
<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>
{0}
</div>", VVItermsConditionsCategory.CategoryName);
                    //绘制Terms & Conditions主体
                    string s = GenrateCategory(ref ID, allGategory, VVItermsConditionsCategory, data, ActivityID, false);
                    sb.AppendLine(s);
                }
            }
            if (needHeader)
            {
                sb.Append(@"</div></div></div>");
            }

            return sb.ToString();
        }


     
        public static string GenrateModelforRFQDetail(object data, string ActivityID)
        {
            //所有需要显示的字段，从Field Mnager中读出
            List<FieldCategory> allGategory = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SGP);

            //如果传入的ACTIVITY ID为空，则找到当前步骤
            if (string.IsNullOrEmpty(ActivityID))
            {
                BLL.Models.RFQDetail rfqDetail = data as BLL.Models.RFQDetail;
                WF.WFTemplate wf = new WF.WFTemplate("DefaultWF", rfqDetail == null ? 0 : rfqDetail.RFQID);
                if (wf != null)
                {
                    if (wf.CurrentActivity != null)
                    {
                        ActivityID = wf.CurrentActivity.ID.ToString();
                    }
                    else
                    {
                        ActivityID = wf.FirstActivity.ID.ToString();
                    }
                }
            }

            List<string> fieldsJustDisplay = null;
            //初始化必填字段
            InitRequiredConfg(data, allGategory, out fieldsJustDisplay);

            StringBuilder sb = new StringBuilder();
            //将data实体的数据赋值到FIELD的DATAVALUE中
            if (data != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
                    //if (currentRole != fc.AllowedRoles) continue;
                    foreach (BI.SGP.BLL.DataModels.FieldInfo field in fc.Fields)
                    {
                        if (field.Enable == 1)
                        {
                            PropertyInfo propertyInfo = data.GetType().GetProperty(field.FieldName);
                            if (propertyInfo != null)
                            {
                                field.DataValue = propertyInfo.GetValue(data);
                            }
                        }
                    }
                }
            }

            FieldCategory priceDetailCategroy = allGategory.Find(t => t.ID == "7");
            FieldCategory termsConditionsCategory = allGategory.Find(t => t.ID == "8");
            FieldCategory VVItermsConditionsCategory = allGategory.Find(t => t.ID == "9");

            int ID = 1;
            foreach (FieldCategory fc in allGategory)
            {
                if (fc.ID == "8" || fc.ID == "7" || fc.ID == "9")
                {
                    continue;
                }
                string s = GenrateCategory(ref ID, allGategory, fc, data, ActivityID, true);
                sb.AppendLine(s);

            }
           AccessServiceReference.Role[] role = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Roles;

            sb.Append("<script>");

            if (!(role.Contains("SGP_BDMGAM") || role.Contains("SGP_Management") || role.Contains("SGP_RFQPrimaryContact")))
            {
                foreach (string fieldName in fieldsJustDisplay)
                {
                    sb.AppendFormat("$('#{0}').attr('disabled','disabled');", fieldName);
                }
            }

            sb.AppendLine("</script>");
            sb.AppendFormat("<script>$.rfqDetail.Init();</script>");
            return sb.ToString();
        }

        private static string GenerateCategoryForPriceDetail(FieldCategory fc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"
<div class='panel-heading' style='background:#f7f7f7;font-weight:bold;text-align:center'>
{0}
                </div>", fc.CategoryName);
            sb.AppendFormat(@"
                    <table class='table table-bordered table-striped' style='margin-bottom:0px;'>
                    <thead class='thin-border-bottom'>
                    <tr>
                    <td>#</td>
                    <td>Price Qty</td>
                    <td>Unit Price</td>
                    <td style='width:16%'>Price Type</td>
                    <td style='width:7%'>MP%</td>
                    <td style='width:7%'>OP%</td>
                    <td style='width:40%'>Remarks</td>
                    </tr></thead><tbody style='width:100%;'>"
                );
            for (int i = 1; i <= 5; i++)
            {
                sb.AppendFormat(@"<tr>
                                <td>{0}</td>
                                <td><input style=""width:100% !important; height:25px !important "" class='form-control' type='text' name='Price{0}Qty' id='Price{0}Qty' value='{1}' /></td>
                                <td><input style=""width:100% !important; height:25px !important ""  class='form-control NumberType1' type='text' name='UnitPrice{0}' id='UnitPrice{0}' value='{2}'/></td>
                                <td>{3}</td>
                                <td><input style=""width:100% !important; height:25px !important ""  class='form-control NumberType1' disabled=""disabled"" type='text' name='MP{0}' id='MP{0}' value='{5}' /></td>
                                <td><input style=""width:100% !important; height:25px !important ""  class='form-control NumberType1' disabled=""disabled"" type='text' name='OP{0}' id='OP{0}' value='{4}' /></td>
                               <td><input style=""width:100% !important; height:25px !important ""   type='text' name='Remark{0}' style='width:100%' id='Remark{0}' value='{6}'/></td>
                                </tr>", i
                                      , fc.Fields[(i - 1) * 6 + 0].DataValue
                                      , fc.Fields[(i - 1) * 6 + 1].DataValue
                                      , GenerateDropdownList(fc.Fields[(i - 1) * 6 + 2])
                                      , fc.Fields[(i - 1) * 6 + 3].DataValue
                                      , fc.Fields[(i - 1) * 6 + 4].DataValue
                                      , fc.Fields[(i - 1) * 6 + 5].DataValue
                                      );
            }
            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private static string GenerateCategoryForVerticalDetail(FieldCategory fc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<table id=""sample-table-1"" class=""table table-striped table-bordered table-hover"">");

            sb.Append(@"<thead><tr>");
            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fc.Fields)
            {

                sb.AppendFormat("<td>{0}</td>",fi.DisplayName);
              
            }
            sb.AppendFormat("<td></td>");
            sb.Append(@"</tr></thead>");
            sb.Append(@"<tbody><tr>");
            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fc.Fields)
            {

                sb.AppendFormat("<td>{0}</td>", GenerateFieldforVertical(fi));
            
            }
            sb.AppendFormat(@"		<td>
															<div class=""visible-md visible-lg hidden-sm hidden-xs btn-group"">
																<button class=""btn btn-xs btn-danger"" onclick=""return RemoveRow(this);"">
																	<i class=""icon-minus bigger-120""></i>
																</button>

																<button class=""btn btn-xs btn-success"" onclick=""return AddRow(this);"">
																	<i class=""icon-plus bigger-120""></i>
																</button>
															</div>
															</div>
														</td>");
            sb.Append(@"</tr></tbody></table>");



           
	
            return sb.ToString();
        }



        public static string GenrateModelforRFQVVIDetail(object data, string ActivityID)
        {
            //所有需要显示的字段，从Field Mnager中读出
            List<FieldCategory> allGategory = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_VVI);

            //如果传入的ACTIVITY ID为空，则找到当前步骤
            if (string.IsNullOrEmpty(ActivityID))
            {
                BLL.Models.RFQDetail rfqDetail = data as BLL.Models.RFQDetail;
                WF.WFTemplate wf = new WF.WFTemplate("DefaultWF", rfqDetail == null ? 0 : rfqDetail.RFQID);
                if (wf != null)
                {
                    if (wf.CurrentActivity != null)
                    {
                        ActivityID = wf.CurrentActivity.ID.ToString();
                    }
                    else
                    {
                        ActivityID = wf.FirstActivity.ID.ToString();
                    }
                }
            }

            List<string> fieldsJustDisplay = null;
            //初始化必填字段
            InitRequiredConfg(data, allGategory, out fieldsJustDisplay);

            StringBuilder sb = new StringBuilder();
            //将data实体的数据赋值到FIELD的DATAVALUE中
            if (data != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
                    //if (currentRole != fc.AllowedRoles) continue;
                    foreach (BI.SGP.BLL.DataModels.FieldInfo field in fc.Fields)
                    {
                        if (field.Enable == 1)
                        {
                            PropertyInfo propertyInfo = data.GetType().GetProperty(field.FieldName);
                            if (propertyInfo != null)
                            {
                                field.DataValue = propertyInfo.GetValue(data);
                            }
                        }
                    }
                }
            }

            FieldCategory priceDetailCategroy = allGategory.Find(t => t.ID == "7");
            FieldCategory termsConditionsCategory = allGategory.Find(t => t.ID == "8");
            FieldCategory VVItermsConditionsCategory = allGategory.Find(t => t.ID == "9");

            int ID = 1;
            foreach (FieldCategory fc in allGategory)
            {
                if (fc.ID == "8" || fc.ID == "7" || fc.ID == "9")
                {
                    continue;
                }
                string s = GenrateCategory(ref ID, allGategory, fc, data, ActivityID, true);
                sb.AppendLine(s);

            }
            AccessServiceReference.Role[] role = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Roles;

            sb.Append("<script>");

            if (!(role.Contains("SGP_BDMGAM") || role.Contains("SGP_Management") || role.Contains("SGP_RFQPrimaryContact")))
            {
                foreach (string fieldName in fieldsJustDisplay)
                {
                    sb.AppendFormat("$('#{0}').attr('disabled','disabled');", fieldName);
                }
            }

            sb.AppendLine("</script>");
            sb.AppendFormat("<script>$.rfqDetail.Init();</script>");
            return sb.ToString();
        }

       
        public static string GenrateModelforSupplier(object data)
        {
            //所有需要显示的字段，从Field Mnager中读出
            List<FieldCategory> allGategory = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_Supplier);

          
            //初始化必填字段
      
            StringBuilder sb = new StringBuilder();
            //将data实体的数据赋值到FIELD的DATAVALUE中
            if (data != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
                    //if (currentRole != fc.AllowedRoles) continue;
                    foreach (BI.SGP.BLL.DataModels.FieldInfo field in fc.Fields)
                    {
                        if (field.Enable == 1)
                        {
                            PropertyInfo propertyInfo = data.GetType().GetProperty(field.FieldName);
                            if (propertyInfo != null)
                            {
                                field.DataValue = propertyInfo.GetValue(data);
                            }
                        }
                    }
                }
            }

      

            int ID = 1;
            foreach (FieldCategory fc in allGategory)
            {
              
                string s = GenrateCategory(ref ID, allGategory, fc, data, "1", true);
                sb.AppendLine(s);

            }
          
            return sb.ToString();
        }
        /// <summary>
        /// 生成单个Field，返回生成的HTML代码
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string GenerateField(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<td style=\"background-color:#edf3f4;border:1px solid #dcebf7;width:10%;{1} \" >{0}</td>"
                , field.DisplayName
                , field.WFRequiredOption == RequiredOption.Required ? "color:red" : "color:#336199"

                );
            if (field.CategoryID == 9)
            {
                sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;width:10%\" colspan={0}>", field.ColSpan);
            }
            else
            {
                sb.AppendFormat("<td style=\"border:1px dotted #dcebf7;width:15%\" colspan={0}>", field.ColSpan);
            }
            
            switch (field.DataType)
            {
                case "string":
                    {
                        if (field.ColSpan >= 5)
                        {
                            sb.Append(GenerateTextArea(field)); break;
                        }
                        else
                        {
                            sb.Append(GenerateTextBox(field)); break;
                        }
                    }
                case "date":
                case "datetime":
                    {
                        sb.Append(GenerateDatePicker(field)); break;
                    }
                case "time":
                    {
                        sb.Append(GenerateTimePicker(field)); break;
                    }
                case "list":
                    {
                        sb.Append(GenerateDropdownList(field)); break;
                    }
                case "listsql":
                    {
                        sb.Append(GenerateDropdownListSQL(field)); break;
                    }
                case "float":
                    {
                        sb.Append(GenerateTextBoxForCurrency(field)); break;
                    }

                case "int":
                case "double":
                    {
                        sb.Append(GenerateTextBoxForNumber(field)); break;
                    }
                case "bool":
                    {
                        sb.Append(GenerateCheckBox(field)); break;
                    }
            }

            sb.Append("</td>");

            return sb.ToString();
        }


        public static string GenerateFieldforVertical(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
       
       

            switch (field.DataType)
            {
                case "string":
                    {
                        if (field.ColSpan >= 5)
                        {
                            sb.Append(GenerateTextArea(field)); break;
                        }
                        else
                        {
                            sb.Append(GenerateTextBox(field)); break;
                        }
                    }
                case "date":
                case "datetime":
                    {
                        sb.Append(GenerateDatePicker(field)); break;
                    }
                case "time":
                    {
                        sb.Append(GenerateTimePicker(field)); break;
                    }
                case "list":
                    {
                        sb.Append(GenerateDropdownList(field)); break;
                    }
                case "listsql":
                    {
                        sb.Append(GenerateDropdownListSQL(field)); break;
                    }
                case "float":
                case "int":
                case "double":
                    {
                        sb.Append(GenerateTextBoxForNumber(field)); break;
                    }
                case "bool":
                    {
                        sb.Append(GenerateCheckBox(field)); break;
                    }
            }

           

            return sb.ToString();
        }

        public static string GenerateTextBoxForCurrency(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
                sb.Append(@"<div style=""width:100% !important""  class=""input-group""><input style=""width:100% !important""  type=""text"" class=""NumberType1"" id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + field.DataValue + @"""  ></div>");            
            return sb.ToString();
        }


        /// <summary>
        /// 生成文本框的HTML代码
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        /// 
        public static string GenerateTextBox(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();

            if (field.FieldName == "OEM" || field.FieldName == "ShipmentTerms" || field.FieldName == "Location")
            {
                sb.AppendLine(@"<input style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @"""  Value=""" + field.DataValue + @""" type=""text"" autocomplete=""off"">");
            }
            else if ((field.FieldName == "CustomerContact")&&(field.DataValue == null || string.IsNullOrEmpty(field.DataValue.ToString().Trim())))
            {
                if (field.Visible == 0)
                {
                    sb.AppendLine(@"<input style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""Whom it may concern"" disabled=""disabled""  type=""text""  placeholder="""" autocomplete=""off"">");
                }
                else
                {
                    sb.AppendLine(@"<input style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""Whom it may concern""  type=""text""  placeholder="""" autocomplete=""off"">");
                }  
            }
            else
            {
                if (field.Visible == 0)
                {
                    sb.AppendLine(@"<input style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @"""  Value=""" + field.DataValue + @""" disabled=""disabled""  type=""text""  placeholder="""" autocomplete=""off"">");
                }
                else
                {
                    sb.AppendLine(@"<input style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @"""  Value=""" + field.DataValue + @"""  type=""text""  placeholder="""" autocomplete=""off"">");
                }
            }

            return sb.ToString();
        }

        public static string GenerateTextBoxForNumber(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            string val;
            double v = 0;
            if(field.DataValue==null)
            {
                field.DataValue = "";
            }
            double.TryParse(field.DataValue.ToString(), out v);
            
            if (v == 0)
            {
                val = "";
                if (field.FieldName == "DueDate")
                {
                    val = "48";
                }
            }
            else
            {
                val = v.ToString();
            }

            string[] hover =  {"PanelSizeWidth","PanelSizeLength"};
            StringBuilder sb = new StringBuilder();

            if (field.Visible == 0)
            {
                if (hover.Contains(field.FieldName))
                {
                    sb.Append(@"<div style=""width:100% !important""  class=""input-group""><input style=""width:100% !important""  type=""text""  disabled=""disabled"" class=""NumberType1"" id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + val + @""" maxlength=""8"" data-original-title=""Inches!"" data-placement=""bottom"" data-rel=""tooltip"" ></div>");
                }
                else
                {
                    sb.Append(@"<div style=""width:100% !important""  class=""input-group""><input style=""width:100% !important""  type=""text""  disabled=""disabled"" class=""NumberType1"" id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + val + @""" maxlength=""8""  ></div>");
                
                }
            }
            else
            {
                if (hover.Contains(field.FieldName))
                {
                    sb.Append(@"<div style=""width:100% !important""  class=""input-group""><input style=""width:100% !important""  type=""text"" class=""NumberType1"" id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + val + @""" maxlength=""8"" data-original-title=""Inches!"" data-placement=""bottom"" data-rel=""tooltip""></div>");
                }
                else
                {
                    sb.Append(@"<div style=""width:100% !important""  class=""input-group""><input style=""width:100% !important""  type=""text"" class=""NumberType1"" id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + val + @""" maxlength=""8""  ></div>");
                }
            }

            return sb.ToString();
        }

        public static string GenerateTextArea(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"<textarea class=""form-control"" style=""width:100% !important""   id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" Value=""" + field.DataValue + @""" placeholder="""">" + field.DataValue + "</textarea>");

            return sb.ToString();
        }

        public static string GenerateDropdownList(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = DbHelperSQL.Query("select * from SGP_KeyValue where [Key]=@Key and status=1 order by [Sort]", new SqlParameter("@Key", field.KeyValueSource)).Tables[0];
            sb.AppendLine(@"<select class=""form-control""  style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" >");

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (field.DataValue != null)
                    {

                        if (field.DataValue.ToString().Trim().ToLower() == dt.Rows[i]["Value"].ToString().Trim().ToLower())
                        {
                            sb.AppendLine(@"<option selected = ""selected"" value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                        }
                        else
                        {
                            sb.AppendLine(@"<option value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                        }
                    }
                    else
                    {
                        sb.AppendLine(@"<option value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                    }
                }
            }
            else
            {
                sb.AppendLine(@"<option value="""">&nbsp;</option>");
            }

            sb.AppendLine(@"</select>");

            return sb.ToString();
        }

        public static string GenerateDropdownListSQL(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();

            DataTable dt = DbHelperSQL.Query(field.KeyValueSource.ToString()).Tables[0];
            if (field.DataValue == null && (field.FieldName == "Initiator" || field.FieldName == "PricingAnalyst" || field.FieldName == "PrimaryContact"))
            {
                field.DataValue = SGP.BLL.Utils.AccessControl.CurrentLogonUser.Name;
            }
            sb.AppendLine(@"<select class=""form-control"" style=""width:100% !important""  id=""" + field.FieldName + @""" name=""" + field.FieldName + @""">");

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (field.DataValue != null)
                    {
                        if (field.DataValue.ToString().Trim().ToLower() == dt.Rows[i]["Value"].ToString().Trim().ToLower())
                        {
                            sb.AppendLine(@"<option selected = ""selected"" value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                        }
                        else
                        {
                            sb.AppendLine(@"<option value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                        }
                    }
                    else
                    {
                        sb.AppendLine(@"<option value=""" + dt.Rows[i]["Value"].ToString() + @""">" + dt.Rows[i]["Value"] + "</option>");
                    }
                }
            }
            else
            {
                sb.AppendLine(@"<option value="""">&nbsp;</option>");
            }

            sb.AppendLine(@"</select>");

            return sb.ToString();
        }

        public static string GenerateDatePicker(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            string val = string.Empty;
            DateTime dt = DateTime.MinValue;

            if (field.DataValue != null)
            {
                if (field.DataValue is DateTime)
                {
                    dt = (DateTime)field.DataValue;
                }
                else
                {
                    string dtStr = field.DataValue.ToString();
                    DateTime.TryParse(dtStr, out dt);
                }
            }

            //判断是否是空日期，空日期不需要显示
            if (dt.Year > 1900)
            {
                val = dt.ToString("MM/dd/yyyy");

                string[] formatdate = { "RFQDateIn", "RFQDateOut", "QuoteDateIn", "QuoteDateOut","PriceDateOut" };

                if (formatdate.Contains(field.FieldName))
                {
                    val = dt.ToString("MM/dd/yyyy HH:mm:ss");
                }
            }
            else if (field.FieldName == "CustomerQuoteDate")
            {
                val = DateTime.Now.ToString("MM/dd/yyyy");

            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"<div class=""input-group"">
							<input ");
            if (field.Visible == 0)
            {
                sb.AppendLine(@" disabled=""disabled""");
            }

            sb.AppendLine(@" style=""height:25px !important""  class=""form-control date-picker"" id=""" + field.FieldName + @"""  name=""" + field.FieldName + @""" Value=""" + val + @""" type=""text"" data-date-format=""mm/dd/yyyy"">
							<span  style=""height:24px !important"" class=""input-group-addon"">
								<i class=""icon-calendar bigger-110""></i>
							</span>
						</div>");

            return sb.ToString();
        }

        public static string GenerateTimePicker(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            string val = string.Empty;
            DateTime dt = DateTime.MinValue;

            if (field.DataValue != null)
            {
                if (field.DataValue is DateTime)
                {
                    dt = (DateTime)field.DataValue;
                }
                else
                {
                    string dtStr = field.DataValue.ToString();
                    DateTime.TryParse(dtStr, out dt);
                }
            }

            //判断是否是空日期，空日期不需要显示
            if (dt.Year > 0000)
            {
                val = dt.ToString("hh:mm:ss");
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"<div class=""input-group bootstrap-timepicker"">
                            <div class=""bootstrap-timepicker-widget dropdown-menu close"">
                                  <table>
                                    <tbody>
                                      <tr>
                                          <td><a href=""#"" data-action=""incrementHour""><i class=""icon-chevron-up""></i></a></td>
                                          <td class=""separator"">&nbsp;</td>
                                          <td><a href=""#"" data-action=""incrementMinute""><i class=""icon-chevron-up""></i></a></td>
                                          <td class=""separator"">&nbsp;</td>
                                          <td><a href=""#"" data-action=""incrementSecond""><i class=""icon-chevron-up""></i></a></td>
                                     </tr>
                                     <tr>
                                          <td><input type=""text"" name=""hour"" class=""bootstrap-timepicker-hour"" maxlength=""2""></td>
                                          <td class=""separator"">:</td><td><input type=""text"" name=""minute"" class=""bootstrap-timepicker-minute"" maxlength=""2""></td>
                                          <td class=""separator"">:</td><td><input type=""text"" name=""second"" class=""bootstrap-timepicker-second"" maxlength=""2""></td>
                                     </tr>
                                     <tr>
                                         <td><a href=""#"" data-action=""decrementHour""><i class=""icon-chevron-down""></i></a></td>
                                         <td class=""separator""></td>
                                        <td><a href=""#"" data-action=""decrementMinute""><i class=""icon-chevron-down""></i></a></td>
                                        <td class=""separator"">&nbsp;</td>
                                        <td><a href=""#"" data-action=""decrementSecond""><i class=""icon-chevron-down""></i></a></td>
                                     </tr>
                                    </tbody>
                                 </table>
                            </div>
							<input class=""form-control time-picker"" class=""form-control"" id=""" + field.FieldName + @"""  name=""" + field.FieldName + @"""   type=""text"">
                            <span class=""input-group-addon"">
                                <i class=""icon-time bigger-110""></i>
                            </span>
                           </div>");

            return sb.ToString();
        }

        public static string GenerateLabel(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<input id='"+ field.FieldName +"' name='"+ field.FieldName+ @"'/>");
            return sb.ToString();
        }
        public static string GenerateCheckBox(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<label><input id="""+field.FieldName+@""" name="""+field.FieldName+@""" type=""checkbox"" class=""ace"">");
            sb.AppendLine(@"<span class=""lbl"">"+field.FieldName+@" </span></label>");

            return sb.ToString();
        }

        public static string GenerateQuery(string groupName)
        {
            StringBuilder strResult = new StringBuilder();
            strResult.Append("<table style='width:100%' id='query-" + groupName + "'><tr>");

            FieldGroup fieldGroup = new FieldGroup(groupName, "Search");

            FieldGroupDetailCollection fieldDetails = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);

            int detailCount = fieldDetails.Count < 4 ? 4 : fieldDetails.Count;

            for (int i = 0; i < detailCount; i++)
            {
                if (i > 1 && (i % 4) == 0)
                {
                    strResult.Append("</tr><tr>");
                }
                if (i >= fieldDetails.Count)
                {
                    strResult.Append("<td style='width:8%' align='right'>&nbsp;</td><td style='width:17%'></td>");
                }
                else
                {
                    strResult.AppendFormat("<td style='width:8%' align='right'>&nbsp;{0}&nbsp;</td><td style='width:17%'>", fieldDetails[i].DisplayName);
                    switch (fieldDetails[i].DataType)
                    {
                        case BLL.DataModels.FieldInfo.DATATYPE_DATETIME:
                        case BLL.DataModels.FieldInfo.DATATYPE_DATE:
                            strResult.Append(GenerateQueryDateRange(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_INT:
                        case BLL.DataModels.FieldInfo.DATATYPE_FLOAT:
                        case BLL.DataModels.FieldInfo.DATATYPE_DOUBLE:
                            strResult.Append(GenerateQueryNumberBox(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_LIST:
                            strResult.Append(GenerateQueryList(fieldDetails[i]));
                            break;
                        case BLL.DataModels.FieldInfo.DATATYPE_ACTIVITY:
                            strResult.Append(GenerateQueryActivity(fieldDetails[i]));
                            break;
                        case "ext":
                            strResult.Append(GenerateQueryExt(fieldDetails[i]));
                            break;
                        default:
                            strResult.Append(GenerateQueryTextBox(fieldDetails[i]));
                            break;
                    }
                    strResult.Append("</td>");
                }
            }

            strResult.AppendFormat("</tr></table><input type='hidden' id='searchGroup' name='searchGroup' value='{0}' />", fieldGroup.GroupName);
            return strResult.ToString();
        }

        public static string GenerateQueryTextBox(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            return String.Format("<input style='width:100% !important' id='{0}' name='{0}' type='text' onpaste='return $.bi.form.pasteJoin(this)'>", field.FieldName);
        }

        public static string GenerateQueryNumberBox(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            return String.Format("<input style='width:100% !important'  class='NumberType1' id='{0}' name='{0}' type='text' onpaste='return $.bi.form.pasteJoinNumber(this)'>", field.FieldName); ;
        }

        public static string GenerateQueryDateRange(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            return String.Format("<div style='width:100% !important' class='input-group'><span class='input-group-addon'><i class='icon-calendar bigger-110'></i></span><input style='width:100% !important; height:28px' class='date-range-picker' type='text' name='{0}' id='{0}' style='cursor:pointer !important;'></div>", field.FieldName);
        }

        public static string GenerateQueryList(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = DbHelperSQL.Query("SELECT [Key],[Value] FROM SGP_KeyValue WHERE [Key]=@Key AND Status=1 AND ISNULL([Value],'')<>'' ORDER BY [Sort]", new SqlParameter("@Key", field.KeyValueSource)).Tables[0];
            strControl.AppendFormat("<input type='hidden' name='{0}'><select style='width:100% !important' class='chosen-select1 tag-input-style' name='{0}' id='{0}' multiple='' data-placeholder=' '><option value=''></option>", field.FieldName);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strControl.AppendFormat("<option value='{0}'>{0}</option>", dr["Value"]);
                }
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string GenerateQueryListSql(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = DbHelperSQL.Query(field.KeyValueSource).Tables[0];
            strControl.AppendFormat("<input type='hidden' name='{0}'><select style='width:100% !important' class='chosen-select1 tag-input-style' name='{0}' id='{0}' multiple='' data-placeholder=' '><option value=''></option>", field.FieldName);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strControl.AppendFormat("<option value='{0}'>{0}</option>", dr["Value"]);
                }
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string GenerateQueryActivity(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = SqlText.ExecuteDataset("SELECT ID,Name FROM SYS_WFActivity WHERE Status=1 AND TemplateID=@TemplateID ORDER BY Sort", "@TemplateID", field.KeyValueSource).Tables[0];
            strControl.AppendFormat("<input type='hidden' name='{0}'><select style='width:100% !important' class='chosen-select1 tag-input-style' multiple='' data-placeholder=' ' name='{0}' id='{0}'><option value=''></option>", field.FieldName);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strControl.AppendFormat("<option value='{0}'>{0}</option>", dr["Name"]);
                }
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static string GenerateQueryExt(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            if (field.FieldName == "StatusID")
            {
                return "<select class='form-control' name='StatusID' id='StatusID'><option value=''>All</option><option value='1'>New</option><option value='9'>Priced</option></select>";
            }
            return "";
        }


        /// <summary>
        /// 生成打印Lable，返回生成的HTML代码
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string GeneratePrintField(BI.SGP.BLL.DataModels.FieldInfo field)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<td style=\"width:10%\">&nbsp;{0}</td>"
                , field.DisplayName
                );
            
                sb.AppendFormat("<td style=\"width:15%\" colspan={0}>", field.ColSpan);
                if (field.DataType=="float")
                {
                    sb.AppendFormat(@"&nbsp;<label id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" >{0}</label>", string.Format("{0:C0}", field.DataValue));                     
                }else
                {
                    sb.AppendFormat(@"&nbsp;<label id=""" + field.FieldName + @""" name=""" + field.FieldName + @""" >{0}</label>", field.DataValue); 
                }
                
            sb.Append("</td>");

            return sb.ToString();
        }
    }
}
