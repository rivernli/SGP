using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models.Detail
{
    public class CostingPeriodDetail : CostingVersionDetail
    {
        public CostingPeriodDetail(FieldCategory category)
            : base(category)
        {

        }

        public CostingPeriodDetail(FieldCategory category, Dictionary<string, object> data)
            : base(category, data)
        {
            
        }

        public override void Delete(int ID)
        {
            AddDeletedLog(ID, "SCCostPeriod");
            string strSql = "DELETE FROM " + TableName + " WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
        }
    }
}
