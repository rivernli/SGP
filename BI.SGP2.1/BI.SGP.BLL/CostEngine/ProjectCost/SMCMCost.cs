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
    public class SMCMCost : CostBase
    {
        protected override string[] RequiredInputKey
        {
            get
            {
                return new String[] { "Plant", "MainWorkCenter", "MaterialID" };
            }
        }

        protected override string Formula
        {
            get 
            {
                string plant = Convert.ToString(Data["Plant"]);
                string mainWorkcenter = Convert.ToString(Data["MainWorkCenter"]);
                if (PCBType == PCBTYPE_FPC)
                {
                    if (mainWorkcenter == "COM")
                    {
                        return "{ManualPrice}*({PanelArea}/2/1000*{CPK}*{SidePanel}*{ConsRateFactor})";
                    }
                    else
                    {
                        return "{ManualPrice}*({Area}*0.00064516*{Thickness}*10-6*{SMCMDensity})/(1-0.4)/{AreaComFactor}";
                    }
                }
                else
                {
                    return "{ManualPrice}*({PanelArea}/2/1000*{CPK}*{SideAConsRateFactor}+{PanelArea}/2/1000*{CPK}*{SideBConsRateFactor})";
                }
            }
        }

        public SMCMCost(object data, InputDataType dataType)
            : base(data, dataType)
        {

        }

        protected override void FillAdditionalData()
        {
            if (PCBType == PCBTYPE_FPC)
            {
                FillFPCData();
            }
            else
            {
                FillRigidData();
            }
        }

        private void FillRigidData()
        {
            string plant = Convert.ToString(Data["Plant"]);
            string materialID = Convert.ToString(Data["MaterialID"]);
            string costVersion = Convert.ToString(Data["CostVersion"]);
            string strSql = "SELECT StdCons FROM SCD_SMCMStdCons WHERE ID = @ID";
            DataTable dt = SqlText.ExecuteDataset(strSql, new SqlParameter("@ID", materialID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                Data["CPK"] = ParseHelper.Parse<double>(dt.Rows[0]["StdCons"]);
                double sideA = ParseHelper.Parse<double>(Data["SideA"]);
                double sideB = ParseHelper.Parse<double>(Data["SideB"]);
                strSql = "SELECT ThicknessFactor FROM SCD_SMCMThicknessFactor WHERE Plant=@Plant AND Version=@CostVersion AND @Thickness >= ThicknessFrom AND @Thickness<=ThicknessTo";
                Data["SideAConsRateFactor"] = ParseHelper.Parse<double>(SqlText.ExecuteScalar(strSql, new SqlParameter("@Plant", plant), new SqlParameter("@Thickness", sideA), new SqlParameter("@CostVersion", costVersion)));
                Data["SideBConsRateFactor"] = ParseHelper.Parse<double>(SqlText.ExecuteScalar(strSql, new SqlParameter("@Plant", plant), new SqlParameter("@Thickness", sideB), new SqlParameter("@CostVersion", costVersion)));
            }
        }

        private void FillFPCData()
        {
            string mainWorkCenter = Convert.ToString(Data["MainWorkCenter"]);
            string plant = Convert.ToString(Data["Plant"]);
            string materialID = Convert.ToString(Data["MaterialID"]);
            string costVersion = Convert.ToString(Data["CostVersion"]);
            if (mainWorkCenter == "COM")
            {
                string strSql = "SELECT StdCons FROM SCD_SMCMStdCons WHERE ID = @ID";
                DataTable dt = SqlText.ExecuteDataset(strSql, new SqlParameter("@ID", materialID)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    Data["CPK"] = ParseHelper.Parse<double>(dt.Rows[0]["StdCons"]);
                    double thickness = 1;
                    strSql = "SELECT ThicknessFactor FROM SCD_SMCMThicknessFactor WHERE Plant=@Plant AND Version=@CostVersion AND @Thickness >= ThicknessFrom AND @Thickness<=ThicknessTo";
                    Data["ConsRateFactor"] = ParseHelper.Parse<double>(SqlText.ExecuteScalar(strSql, new SqlParameter("@Plant", plant), new SqlParameter("@CostVersion", costVersion), new SqlParameter("@Thickness", thickness)));
                }
            }
            else
            {
                string strSql = "SELECT SMCMDensity,AreaComFactor FROM SCD_FPCComMarkParams WHERE Plant=@Plant AND Version=@CostVersion AND MainWorkCenter=@MainWorkCenter";
                DataTable dt = SqlText.ExecuteDataset(strSql, new SqlParameter("@Plant", plant), new SqlParameter("@CostVersion", costVersion), new SqlParameter("@MainWorkCenter", mainWorkCenter)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    Data["SMCMDensity"] = ParseHelper.Parse<double>(dt.Rows[0]["SMCMDensity"]);
                    Data["AreaComFactor"] = ParseHelper.Parse<double>(dt.Rows[0]["AreaComFactor"]);
                }
            }
        }
    }
}
