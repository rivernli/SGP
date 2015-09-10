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
    public static class CustomerProfileUIManager
    {
        public static string GenrateModelCustomerProfileDetail(object data, string ActivityID, string PageType)
        {
            //所有需要显示的字段，从Field Mnager中读出
            List<FieldCategory> allGategory = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_CUSTOMERPROFILE);
            
            StringBuilder sb = new StringBuilder();
            //将data实体的数据赋值到FIELD的DATAVALUE中
            if (data != null)
            {
                foreach (FieldCategory fc in allGategory)
                {
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
                string s = string.Empty;
                switch (PageType)
                {
                    case "edit":
                        s = GenrateCategory(ref ID, allGategory, fc, data, ActivityID);
                    break;
                    case "print":
                        s = GenratePrintPage(ref ID, allGategory, fc, data, ActivityID);
                    break;
                }
                sb.AppendLine(s);
            }

            return sb.ToString();
        }

        private static string GenrateCategory(ref int ID, List<FieldCategory> allGategory, FieldCategory fc, object data, string ActivityID)
        {
            StringBuilder sb = new StringBuilder();           
            ID++;

            sb.Append(@"<div class=""panel panel-default"">
                           <div class=""panel-heading"">
                              <a href=""#faq-1-" + ID.ToString() + @""" data-parent=""#faq-list-" + ID.ToString() + @""" data-toggle=""collapse"" class=""accordion-toggle"">
                              <i class=""pull-right icon-chevron-down"" data-icon-hide=""icon-chevron-down"" data-icon-show=""icon-chevron-left""></i>
                              <i class=""icon-user bigger-130""></i>&nbsp; " + fc.CategoryName + @":</a>
                           </div>");
            sb.Append(@"<div class=""panel-collapse in"" id=""faq-1-" + ID.ToString() + @""" style=""height: auto;""><div class=""panel-body"">");

            sb.Append(@"<form id='fm' method='post'><table>");
            FieldInfoCollecton enablefields = new FieldInfoCollecton();

            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fc.Fields)
            {
                if (fi.Enable == 1)
                {

                    enablefields.Add(fi);
                }
            }

            double RowCount = enablefields.Count;
            int colSize = 4;            
            sb.Append(@"<input type=""hidden"" id=""ID"" name=""ID"" value=""" + ActivityID + @"""  /><tbody  style=""width:100%"">");

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
                    sb.Append(UIManager.GenerateField(enablefields[i]));
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
            sb.Append("</form></table>");
            sb.Append(@"</div></div></div>");

            return sb.ToString();
        }

        private static string GenratePrintPage(ref int ID, List<FieldCategory> allGategory, FieldCategory fc, object data, string ActivityID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<div><h3 align=""center"">" + fc.CategoryName + "</h3></div>");

            sb.Append(@"<table style=""width:98%"" id=""listTable"" border=""1"" align=""center"" cellpadding=""0"" cellspacing=""0"" bordercolor=""#000000"" >");
            FieldInfoCollecton enablefields = new FieldInfoCollecton();

            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in fc.Fields)
            {
                if (fi.Enable == 1)
                {
                    enablefields.Add(fi);
                }
            }

            double RowCount = enablefields.Count;
            int colSize = 4;
            int colSpanTotal = 0;
            for (int i = 0; i < RowCount; i++)
            {
                if (colSpanTotal == 0)
                {
                    sb.Append(@"<tr style=""height:25px"">");
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
                    sb.Append(UIManager.GeneratePrintField(enablefields[i]));
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
            sb.Append("</table>");

            return sb.ToString();
        }
    }        


}
