using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.DataModels
{
    public class TableParams
    {
        public static readonly int TableType_MasterData = 1;
        public static readonly int TableType_VersionData = 2;
        public static readonly int TableType_PriceMaster = 3;
        public int ID { get; set; }
        public string TableKey { get; set; }
        public string TableName { get; set; }
        public int TableType { get; set; }
        public string DisplayName { get; set; }
        public List<string> UniqueKey { get; set; }
        public string OrderBy { get; set; }
        public int Sort { get; set; }

        public TableParams(string tableKey)
        {
            string strSql = "SELECT * FROM SCS_TableParams WHERE TableKey = @TableKey";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@TableKey", tableKey)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                this.TableKey = Convert.ToString(dt.Rows[0]["TableKey"]);
                this.TableName = Convert.ToString(dt.Rows[0]["TableName"]);
                this.TableType = Convert.ToInt32(dt.Rows[0]["TableType"]);
                this.DisplayName = Convert.ToString(dt.Rows[0]["DisplayName"]);
                this.Sort = Convert.ToInt32(dt.Rows[0]["Sort"]);
                this.OrderBy = Convert.ToString(dt.Rows[0]["OrderBy"]);
                string uks = Convert.ToString(dt.Rows[0]["UniqueKey"]);
                UniqueKey = new List<string>();
                if (!String.IsNullOrWhiteSpace(uks))
                {
                    string[] auks = uks.Split(',');
                    foreach (string uk in auks)
                    {
                        if (!String.IsNullOrWhiteSpace(uk))
                        {
                            UniqueKey.Add(uk.Trim());
                        }
                    }
                }
            }
        }
    }
}
