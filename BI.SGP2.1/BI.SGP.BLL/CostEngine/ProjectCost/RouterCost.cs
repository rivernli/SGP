using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.CostEngine.DataConverter;
using SGP.DBUtility;

namespace BI.SGP.BLL.CostEngine.ProjectCost
{
    public class RouterCost: CostBase
    {
        protected override string[] RequiredInputKey
        {
            get
            {
                return new String[] { "CostVersion", "Plant", "Size", "MaterialID", "MaterialType" };
            }
        }

        protected override string Formula
        {
            get
            {
                return "{ManualPrice}*{Distance}*{UnitPerWorkingPanel}/{UnitPerArray}/{LifeTime}*(1+{BreakageRate})/{Stack}";
            }
        }

        public RouterCost(object data, InputDataType dataType)
            : base(data, dataType)
        {

        }

        protected override void FillAdditionalData()
        {
            string plant = Convert.ToString(Data["Plant"]);
            string materialType = Convert.ToString(Data["MaterialType"]);
            string costVersion = Convert.ToString(Data["CostVersion"]);
            string size = Convert.ToString(Data["Size"]);
            string strSql = "SELECT LifeTime,BreakageRate FROM SCD_RouterProcParams WHERE RoutingType=@MaterialType AND Plant=@Plant AND Version=@CostVersion AND DrillSize=@Size";
            DataTable dt = SqlText.ExecuteDataset(strSql,
                new SqlParameter("@MaterialType", materialType),
                new SqlParameter("@Plant", plant),
                new SqlParameter("@CostVersion", costVersion),
                new SqlParameter("@Size", size)
                ).Tables[0];
            if (dt.Rows.Count > 0)
            {
                Data["LifeTime"] = ParseHelper.Parse<double>(dt.Rows[0]["LifeTime"]);
                Data["BreakageRate"] = ParseHelper.Parse<double>(dt.Rows[0]["BreakageRate"]);
            }
        }
    }
}