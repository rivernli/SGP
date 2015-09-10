using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.Models
{
    public class StageSummary
    {
        public DataTable Data { get; set; }

        public StageSummary(HttpRequestBase Request)
        {
            SqlParameter[] parames = null;
            string spName = Encryption.Decrypto(Request["spName"]);

            if (!String.IsNullOrEmpty(spName))
            {
                string[] dateTime = StringHelper.DateTimeSplit(Request["QueryDateTime"]);
                if (dateTime != null)
                {
                    parames = new SqlParameter[] {
                        new SqlParameter("@StartDate", dateTime[0]),
                        new SqlParameter("@EndDate", dateTime[1])
                    };
                }
                Data = SqlProc.ExecuteDataset(spName, parames).Tables[0];
            }
        }

        public string ToJson()
        {
            if (Data != null)
            {
                string[] arrCols = new string[Data.Columns.Count];
                for (int i = 0; i < Data.Columns.Count; i++)
                {
                    arrCols[i] = Data.Columns[i].ColumnName;
                }
                StringBuilder jsonData = new StringBuilder();
                jsonData.Append("{\"columns\":");
                jsonData.Append(Newtonsoft.Json.JsonConvert.SerializeObject(arrCols));
                jsonData.Append(",\"rows\":");
                jsonData.Append(Newtonsoft.Json.JsonConvert.SerializeObject(Data));
                jsonData.Append("}");
                return jsonData.ToString();
            }
            else
            {
                return "";
            }
            
        }
    }
}
