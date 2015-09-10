using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.DataModels
{
    public class CostItem
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string TableKey { get; set; }
        public string CostClass { get; set; }

        public CostItem(DataRow dr)
        {
            this.Key = Convert.ToString(dr["CostItem"]);
            this.DisplayName = Convert.ToString(dr["DisplayName"]);
            this.TableKey = Convert.ToString(dr["TableKey"]);
            this.CostClass = Convert.ToString(dr["CostClass"]);
        }

        public CostItem(string costItem)
        {
            string strSql = "SELECT CostItem,DisplayName,TableKey,CostClass FROM SCS_CostInputGroup WHERE CostItem = @CostItem";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@CostItem", costItem)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.Key = Convert.ToString(dt.Rows[0]["CostItem"]);
                this.DisplayName = Convert.ToString(dt.Rows[0]["DisplayName"]);
                this.TableKey = Convert.ToString(dt.Rows[0]["TableKey"]);
                this.CostClass = Convert.ToString(dt.Rows[0]["CostClass"]);
            }
        }
    }
}
