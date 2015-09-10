using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BI.SGP.BLL.UIManager;


namespace BI.SGP.BLL.Models
{
    public class GridData
    {
        public string Page { get; set; }
        public int Total { get; set; }
        public int Records { get; set; }
        public DataTable DataTable { get; set; }

        public string ToJson()
        {
            return ToJson(null);
        }

        public string ToJson(params TableFormatString[] formatString)
        {
            string tableData;
            if (formatString != null && formatString.Length > 0)
            {
                TableFormat tf = new TableFormat(DataTable, formatString);
                tableData = Newtonsoft.Json.JsonConvert.SerializeObject(tf, new TableFormatConverter());
            }
            else
            {
                tableData = Newtonsoft.Json.JsonConvert.SerializeObject(DataTable);
            }

            StringBuilder jsonData = new StringBuilder();
            jsonData.Append("{\"total\":");
            jsonData.Append(Total);
            jsonData.Append(",");
            jsonData.Append("\"page\":");
            jsonData.Append(Page);
            jsonData.Append(",");
            jsonData.Append("\"records\":");
            jsonData.Append(Records);
            jsonData.Append(",");
            jsonData.Append("\"rows\":");
            jsonData.Append(tableData);
            jsonData.Append("}");
            return jsonData.ToString();
        }


        public string ToJsonTotal(DataTable totalTableData, params TableFormatString[] formatString)
        {
            string tableData;
            string totalData;
            if (formatString != null && formatString.Length > 0)
            {
                TableFormat tf = new TableFormat(DataTable, formatString);
                tableData = Newtonsoft.Json.JsonConvert.SerializeObject(tf, new TableFormatConverter());
                //Total
                tf = new TableFormat(totalTableData, formatString);
                totalData = Newtonsoft.Json.JsonConvert.SerializeObject(tf, new TableFormatConverter());
            }
            else
            {
                tableData = Newtonsoft.Json.JsonConvert.SerializeObject(DataTable);
                //Total
                totalData = Newtonsoft.Json.JsonConvert.SerializeObject(totalTableData);
            }

            StringBuilder jsonData = new StringBuilder();
            jsonData.Append("{\"total\":");
            jsonData.Append(Total);
            jsonData.Append(",");
            jsonData.Append("\"page\":");
            jsonData.Append(Page);
            jsonData.Append(",");
            jsonData.Append("\"records\":");
            jsonData.Append(Records);
            jsonData.Append(",");
            jsonData.Append("\"rows\":");
            jsonData.Append(tableData);
            jsonData.Append(",");
            jsonData.Append("\"userdata\":");
            jsonData.Append(totalData.Replace("[","").Replace("]",""));
            jsonData.Append("}");
            return jsonData.ToString();
        }
    }
}
