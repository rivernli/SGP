using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using System.Reflection;
using System.IO;
using BI.SGP.BLL.DataModels;


namespace BI.SGP.Web.Models
{
    public static class Generatehtml
    {

        public static MvcHtmlString GetHtmlContent(this HtmlHelper htmlhelper, DataTable dt)
        {


                StringBuilder strGrid = new StringBuilder();
                strGrid.Append(@"<table id=""GridData"" class=""table table-striped table-bordered"">");
                strGrid.Append("<thead>");
               
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        strGrid.Append("<th>");
                        strGrid.Append(dt.Columns[i].ColumnName.ToString());
                        strGrid.Append("</th>");
                    }
                    strGrid.Append("</thead>");
                    strGrid.Append("<tbody>");
                    for (int x = 0; x < dt.Rows.Count; x++)
                    {
                        if (dt.Columns.Contains("Message"))
                        {
                            if (string.IsNullOrEmpty(dt.Rows[x]["Message"].ToString()) == false)
                            {
                                strGrid.Append("<tr>");

                                for (int y = 0; y < dt.Columns.Count; y++)
                                {
                                    strGrid.Append("<td");

                                    if (dt.Columns[y].ColumnName == "Message")
                                    {
                                        strGrid.Append(" style='background-color:red;color:white;width:200px' ");
                                    }
                                    strGrid.Append(">");
                                    strGrid.Append(dt.Rows[x][y].ToString());
                                    strGrid.Append("</td>");
                                }
                                strGrid.Append("</tr>");
                            }
                            else
                            {

                                strGrid.Append("<tr>");
                               
                                for (int y = 0; y < dt.Columns.Count; y++)
                                {
                                    strGrid.Append("<td>");
                                    strGrid.Append(dt.Rows[x][y].ToString());
                                    strGrid.Append("</td>");
                                }
                                strGrid.Append("</tr>");
                            }
                        }
                        else
                        {
                            strGrid.Append("<tr>");
                           
                            for (int y = 0; y < dt.Columns.Count; y++)
                            {
                                strGrid.Append("<td>");
                                strGrid.Append(dt.Rows[x][y].ToString());
                                strGrid.Append("</td>");
                            }
                            strGrid.Append("</tr>");
                        }
                       
                    }
                }
                strGrid.Append("</tbody>");
                strGrid.Append("</table>");
            

            return new MvcHtmlString(strGrid.ToString());
           
        }
        public static MvcHtmlString GetHtmlContentforHistoryData(this HtmlHelper htmlhelper, DataTable dt)
        {

            FieldInfoCollecton fields = FieldCategory.GetAllFieldsOrderbyCategoryID();
            StringBuilder strGrid = new StringBuilder();
            strGrid.Append(@"<table id=""sample-table-2"" class=""table table-striped table-bordered"">");
            strGrid.Append("<thead>");
            List<string> cols=new List<string>();
            if (dt != null && dt.Rows.Count > 0)
            {
                if (!cols.Contains("User Name"))
                {
                    strGrid.Append("<th style=\"white-space:nowrap\">");
                    strGrid.Append("User Name");
                    strGrid.Append("</th>");
                }

                if (!cols.Contains("Update On"))
                {
                    strGrid.Append("<th style=\"white-space:nowrap\">");
                    strGrid.Append("Update On");
                    strGrid.Append("</th>");
                }

                for (int i = 0; i < fields.Count; i++)
                {
                    strGrid.Append("<th style=\"white-space:nowrap\">");
                    strGrid.Append(fields[i].DisplayName);
                    cols.Add(fields[i].DisplayName);
                    strGrid.Append("</th>");
                }

              
                strGrid.Append("</thead>");
                strGrid.Append("<tbody>");
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    strGrid.Append("<tr>");

                    if (!cols.Contains("User Name"))
                    {
                        strGrid.Append("<td nowrap='nowrap'>");
                        if (dt.Columns["Name"] != null)
                        {
                            strGrid.Append(dt.Rows[x]["Name"].ToString());
                        }
                        else
                        {
                            strGrid.Append("");
                        }

                        strGrid.Append("</td>");
                    }
                    if (!cols.Contains("Update On"))
                    {
                        strGrid.Append("<td nowrap='nowrap'>");
                        if (dt.Columns["UpdateDate"] != null)
                        {
                            strGrid.Append(dt.Rows[x]["UpdateDate"].ToString());
                        }
                        else
                        {
                            strGrid.Append("");
                        }

                        strGrid.Append("</td>");
                    }


                    for (int y = 0; y < fields.Count; y++)
                    {
                        strGrid.Append("<td nowrap='nowrap'>");
                        if (dt.Columns[fields[y].FieldName] != null)
                        {
                            strGrid.Append(dt.Rows[x][fields[y].FieldName].ToString());
                        }
                        else
                        {                           
                            strGrid.Append("");
                        }

                        strGrid.Append("</td>");
                    }


                  

                    strGrid.Append("</tr>");
                }
                cols.Add("User Name");
                cols.Add("Update On");
                
            }
            strGrid.Append("</tbody>");
            strGrid.Append("</table>");
            return new MvcHtmlString(strGrid.ToString());

        }
        public static  DataTable GetAll(string TableName)
        {


            StringBuilder strquery = new StringBuilder();
            strquery.Append("select * from "+TableName.ToString());
            DataTable dt = DbHelperSQL.Query(strquery.ToString()).Tables[0];
            return dt;
          
        }
        public static DataTable GetPagedData(string TableName, string order, int pageindex, int pagesize)
        {

            StringBuilder strquery = new StringBuilder();
            int startIndex = pageindex * pagesize - pagesize + 1;
            int endIndex = pageindex * pagesize;   
            strquery.Append(@" ;with a as (select [No.]=row_number() over(order by " + order + "),* from  " + TableName.ToString() + @") 
                             select * from a where [No.] between " + startIndex.ToString() + " and " + endIndex.ToString() + "");

            DataTable dt = DbHelperSQL.Query(strquery.ToString()).Tables[0];
            return dt;
        }

        public static string GetJavaScript()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append(@"<script type=""text/javascript"">
                            $(document).ready(function () {


                                    
                                    var url='~/Pricing/SaveData';
                                    $('#btnSave').click(function () {
                                            $.ajax({
                                            type: 'Post',
                                            url:  url,");

            sb.Append("data: {");
            

            BI.SGP.BLL.Models.RFQDetail rfqdetail=new BLL.Models.RFQDetail();
           

            PropertyInfo[] columns=rfqdetail.GetType().GetProperties();
            

            for(int i=0;i<columns.Length;i++)
            {
                sb.Append(columns[i].Name);
                sb.Append(":");
                sb.Append(@"$('#"+columns[i].Name+@"').val(),");
                
            }
            string strscript=sb.ToString().Substring(0, sb.ToString().Length - 1);
            sb.Clear();
            sb.Append(strscript);
            sb.Append("},");
            sb.Append(@"datatype:'json',
                                            success: function (data) {
                                                    //alert('test')
                                                    //bootbox.dialog({message:'test'});
                                                    $('#Number').val(data.Number);
                                                    if(data.isPass==false){
                                                        $.bi.dialog.show({ title: 'Message', content: data.MessageString,width:800 });
                                                    }else{
                                                        $.bi.dialog.show({ title: 'Save Success', content: 'Save Success!',width:800 });
                                                    }

                                            }
                                        });                     
                                    });
                                });
                            </script>");

            return sb.ToString();


        }
       
        

    }
}