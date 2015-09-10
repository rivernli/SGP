using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.CostEngine.DataConverter;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.CostEngine.ProjectCost
{
    public class GoldCost : CostBase
    {
        protected override string[] RequiredInputKey
        {
            get
            {
                return new String[] { "Plant", "MainWorkCenter", "PanelArea", "Thickness", "PlatingAreaPercent", "AreaCompensation", "PlatingArea" };
            }
        }

        protected override string Formula
        {
            get 
            {
                if (PCBType == PCBTYPE_FPC)
                {
                    return "{Price}*{Thickness}*{ThicComFactor}*{PlatingArea}*0.0001*6.4516*19.32*{LossFactor}/0.683";
                }
                else
                {
                    return "{Price}*{Thickness}*{ThicComFactor}*{PanelArea}*2*({PlatingAreaPercent}/100+{AreaCompensation}/100)*0.000003280839895*28316.846592*19.32*{LossFactor}/0.683";
                }
            }
        }

        public GoldCost(object data, InputDataType dataType)
            : base(data, dataType)
        {

        }

        protected override void FillAdditionalData()
        {
            string plant = Convert.ToString(Data["Plant"]);
            string costVersion = Convert.ToString(Data["CostVersion"]);
            string thickness = Convert.ToString(Data["Thickness"]);
            string mainWorkCenter = Convert.ToString(Data["MainWorkCenter"]);
            Data["PlatingAreaPercent"] = ParseHelper.Parse<double>(Data["PlatingAreaPercent"]);
            Data["AreaCompensation"] = ParseHelper.Parse<double>(Data["AreaCompensation"]);
            string strSql = "SELECT ThicComFactor,LossFactor,Price FROM SCD_GoldSaltConsFactor WHERE MainWorkCenter=@MainWorkCenter AND Plant=@Plant AND Version=@CostVersion AND ThicknessFrom<=@Thickness AND ThicknessTo>=@Thickness";
            DataTable dt = SqlText.ExecuteDataset(strSql,
                new SqlParameter("@MainWorkCenter", mainWorkCenter),
                new SqlParameter("@Plant", plant),
                new SqlParameter("@CostVersion", costVersion),
                new SqlParameter("@Thickness", thickness)
                ).Tables[0];
            if (dt.Rows.Count > 0)
            {
                Data["ThicComFactor"] = ParseHelper.Parse<double>(dt.Rows[0]["ThicComFactor"]);
                Data["LossFactor"] = ParseHelper.Parse<double>(dt.Rows[0]["LossFactor"]);
                Data["Price"] = ParseHelper.Parse<double>(dt.Rows[0]["Price"]);
            }
        }
    }
}
