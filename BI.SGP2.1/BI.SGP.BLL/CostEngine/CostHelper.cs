using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.CostEngine
{
    public class CostHelper
    {
        public const string COSTITEM_GROUP_BOM = "BOM";
        public const string COSTITEM_GROUP_EDM = "EDM";
        private Dictionary<string, DataTable> _costRateTable = new Dictionary<string, DataTable>();

        public DataTable GetCostRateTable(string Version, string Plant)
        {
            string key = Version + Plant;
            if (!_costRateTable.ContainsKey(key))
            {
                string strSql = "SELECT CostKey,MainWorkCenter,CostRate FROM V_SC_AllCostRate WHERE @Version=@Version AND Plant=@Plant";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Version", Version), new SqlParameter("@Plant", Plant)).Tables[0];
                _costRateTable.Add(key, dt);
            }

            return _costRateTable[key];
        }

        public void GenerateCosting(int dataId)
        {
            UpdateCycleTime(dataId);

            string strSql = "SELECT * FROM SCI_ProductionInformation WHERE ID=@DataID";
            SqlParameter idps = new SqlParameter("DataID", dataId);
            DataTable mainData = DbHelperSQL.Query(strSql, idps).Tables[0];

            if (mainData.Rows.Count > 0)
            {
                string costVersion = Convert.ToString(mainData.Rows[0]["CostVersion"]);
                double targetYield = ParseHelper.Parse<double>(mainData.Rows[0]["TargetYield"]);

                strSql = @"SELECT ID,MainWorkCenter,WorkCenter,Step,Layer,PanelArea,SubContractPlant FROM SCI_ProcessFlow WHERE SCIID=@DataID UNION ALL SELECT 99999,'NPW','NPW',0,'',NULL,NULL";
                DataTable procData = DbHelperSQL.Query(strSql, idps).Tables[0];

                strSql = "SELECT * FROM SCI_BOM WHERE SCIID=@DataID";
                DataTable bomData = DbHelperSQL.Query(strSql, idps).Tables[0];

                strSql = "SELECT * FROM SCI_EDM WHERE SCIID=@DataID";
                DataTable edmData = DbHelperSQL.Query(strSql, idps).Tables[0];

                strSql = "SELECT * FROM SCS_CostItem WHERE Status = 1 ORDER BY Sort,ID";
                DataTable costItemData = DbHelperSQL.Query(strSql).Tables[0];

                MergerData(mainData, bomData);
                MergerData(mainData, edmData);
                MergerData(mainData, procData, edmData);

                strSql = "DELETE FROM SCO_ProcBreakdown WHERE SCIID=@DataID";
                DbHelperSQL.ExecuteSql(strSql, idps);

                foreach (DataRow pfdr in procData.Rows)
                {
                    string plant = Convert.ToString(pfdr["Plant"]);
                    string mainWorkCenter = Convert.ToString(pfdr["MainWorkCenter"]);
                    foreach (DataRow cidr in costItemData.Rows)
                    {
                        string costItemGroup = Convert.ToString(cidr["InputGroup"]);
                        string costItem = Convert.ToString(cidr["CostItem"]);
                        int pfID = ParseHelper.Parse<int>(pfdr["ID"]);
                        double sumValue = 0;
                        if (costItemGroup == COSTITEM_GROUP_BOM || costItemGroup == COSTITEM_GROUP_EDM)
                        {
                            DataRow[] drs = null;
                            if (costItemGroup == COSTITEM_GROUP_BOM)
                            {
                                drs = bomData.Select(String.Format("PFID={0} AND MaterialType='{1}'", pfdr["ID"], costItem));
                            }
                            else if (costItemGroup == COSTITEM_GROUP_EDM)
                            {
                                drs = edmData.Select(String.Format("PFID={0} AND DataType='{1}'", pfdr["ID"], costItem));
                            }
                            
                            foreach (DataRow dr in drs)
                            {
                                CostBase cb = CostItemFactory.GetInstance(costItem, dr, BLL.CostEngine.DataConverter.InputDataType.DataRow);
                                if (cb != null && cb.DataReady)
                                {
                                    double cbValue = cb.Compute();
                                    sumValue += cbValue;
                                    UpdateGroupDetailData(dataId, dr, costItemGroup, cbValue);
                                }
                            }
                        }
                        else
                        {
                            DataTable costRateData = GetCostRateTable(costVersion, plant);
                            DataRow[] costRateDrs = costRateData.Select(String.Format("CostKey='{0}' AND MainWorkCenter='{1}'", costItem, mainWorkCenter));
                            if(costRateDrs != null && costRateDrs.Length > 0) 
                            {
                                double costRate = ParseHelper.Parse<double>(costRateDrs[0]["CostRate"]);
                                if (costRate != 0)
                                {
                                    pfdr["CostRate"] = costRateDrs[0]["CostRate"];
                                    CostBase cb = CostItemFactory.GetInstance(costItem, pfdr, BLL.CostEngine.DataConverter.InputDataType.DataRow);
                                    if (cb != null && cb.DataReady)
                                    {
                                        sumValue += cb.Compute();
                                    }
                                }
                            }
                        }

                        if (sumValue != 0)
                        {
                            AddProcBreakdown(dataId, pfID, costItem, sumValue / targetYield);
                        }
                    }
                }

                strSql = @"UPDATE SCI_ProductionInformation 
                            SET Status = 2, 
                            VariableCost=(SELECT SUM(CostValue) FROM SCS_CostItem t1, SCO_ProcBreakdown t2 WHERE t1.CostItem = t2.CostItem AND DisplayMainGroup='Variable Cost' AND t2.SCIID=@DataID),
                            FixedCost=(SELECT SUM(CostValue) FROM SCS_CostItem t1, SCO_ProcBreakdown t2 WHERE t1.CostItem = t2.CostItem AND DisplayMainGroup='Fixed Cost' AND t2.SCIID=@DataID),
                            TotalCost=(SELECT SUM(CostValue) FROM SCS_CostItem t1, SCO_ProcBreakdown t2 WHERE t1.CostItem = t2.CostItem AND t2.SCIID=@DataID),
                            MaterialCost=(SELECT SUM(CostValue) FROM SCS_CostItem t1,SCO_ProcBreakdown t2 WHERE t1.CostItem = t2.CostItem AND t2.SCIID=@DataID AND (t1.DisplaySecondGroup='Project BOM Material' OR t1.DisplaySecondGroup='Project EDM' OR t1.DisplaySecondGroup='Process EDM'))
                            WHERE ID = @DataID";
                DbHelperSQL.ExecuteSql(strSql, idps);
            }
        }

        private void MergerData(DataTable mainData, DataTable procData, DataTable edmData)
        {
            string[] mergerMainColumns = { "CostVersion", "CostPeriod", "LayerCount" };
            string[] mergerOtherColumns = { "CostRate", "Plant", "TotalCycleTime" };
            foreach (string c in mergerMainColumns)
            {
                procData.Columns.Add(c);
            }
            foreach (string c in mergerOtherColumns)
            {
                procData.Columns.Add(c);
            }

            foreach (DataRow dr in procData.Rows)
            {
                foreach (string c in mergerMainColumns)
                {
                    dr[c] = mainData.Rows[0][c];
                }
            }

            string costVersion = Convert.ToString(mainData.Rows[0]["CostVersion"]);
            foreach (DataRow dr in procData.Rows)
            {
                string mainWorkCenter = Convert.ToString(dr["MainWorkCenter"]);

                string plant = Convert.ToString(dr["SubcontractPlant"]);
                if(String.IsNullOrEmpty(plant)) {
                    plant = Convert.ToString(mainData.Rows[0]["CostingBasedOn"]);
                }
                dr["Plant"] = plant;

                double panelArea = ParseHelper.Parse<double>(dr["PanelArea"]);
                if (panelArea == 0)
                {
                    dr["PanelArea"] = mainData.Rows[0]["PanelArea"];
                }

                if (CostBase.IsComputeByCycleTime(plant, mainWorkCenter))
                {
                    double sumVal = 0;
                    DataRow[] edmdrs = edmData.Select(String.Format("PFID={0}", dr["ID"]));
                    if (edmdrs != null)
                    {
                        foreach (DataRow edmdr in edmdrs)
                        {
                            sumVal += ParseHelper.Parse<double>(edmdr["TotalCycleTime"]);
                        }
                    }
                    dr["TotalCycleTime"] = sumVal;
                }
            }
        }

        private void MergerData(DataTable mainData, DataTable costData)
        {
            string[] mergerColumns = { "PanelArea", "CostingBasedOn", "CostVersion", "CostPeriod", "UnitPerWorkingPanel", "UnitPerArray" };
            foreach (string c in mergerColumns)
            {
                costData.Columns.Add(c);
            }

            foreach (DataRow dr in costData.Rows)
            {
                foreach (string c in mergerColumns)
                {
                    dr[c] = mainData.Rows[0][c];
                }
            }
        }

        private void AddProcBreakdown(int SCIID, int PFID, string CostItem, double CostValue)
        {
            string strSql = "INSERT INTO SCO_ProcBreakdown(SCIID,PFID,CostItem,CostValue) VALUES(@SCIID,@PFID,@CostItem,@CostValue)";
            DbHelperSQL.ExecuteSql(strSql, 
                new SqlParameter("@SCIID", SCIID),
                new SqlParameter("@PFID", PFID),
                new SqlParameter("@CostItem", CostItem),
                new SqlParameter("@CostValue", CostValue)
            );
        }

        private void UpdateGroupDetailData(int dataId, DataRow dr, string costItemGroup, double cbValue)
        {
            string tableName = "";
            if (costItemGroup == COSTITEM_GROUP_BOM)
            {
                tableName = "SCI_BOM";
            }
            else if (costItemGroup == COSTITEM_GROUP_EDM)
            {
                tableName = "SCI_EDM";
            }

            if (!String.IsNullOrEmpty(tableName))
            {
                string strSql = "UPDATE " + tableName + " SET Amount=@Amount WHERE PFID=@PFID AND ID=@ID AND SCIID=@SCIID";
                DbHelperSQL.ExecuteSql(strSql,
                    new SqlParameter("@Amount", cbValue),
                    new SqlParameter("@PFID", dr["PFID"]),
                    new SqlParameter("@ID", dr["ID"]),
                    new SqlParameter("@SCIID", dataId));
            }
        }

        private void UpdateCycleTime(int dataId)
        {
            SqlParameter idps = new SqlParameter("@SCIID", dataId);
            string strSql = "UPDATE SCI_EDM SET TotalCycleTime=(SELECT LUTime+ASTime FROM SCD_LoadUnloadTime WHERE Plant=SCI_EDM.Plant AND MainWorkCenter=SCI_EDM.MainWorkCenter AND Type=SCI_EDM.MaterialType AND Version=(SELECT CostVersion FROM SCI_ProductionInformation WHERE ID=SCI_EDM.SCIID)) WHERE MainWorkCenter IN('DRL','ROU') AND SCIID = @SCIID;";
            strSql += "UPDATE SCI_EDM SET TotalCycleTime=(SELECT LUTime+ASTime FROM SCD_LoadUnloadTime WHERE Plant=SCI_EDM.Plant AND MainWorkCenter=SCI_EDM.MainWorkCenter AND Type=(SELECT Step1 FROM SCD_LaserProcParams WHERE ID = SCI_EDM.MaterialID) AND Version=(SELECT CostVersion FROM SCI_ProductionInformation WHERE ID=SCI_EDM.SCIID)) WHERE MainWorkCenter = 'LAS' AND SCIID = @SCIID;";
            strSql += "UPDATE SCI_EDM SET TotalCycleTime=(SELECT LUTime+ASTime FROM SCD_LaserCutingParams WHERE ID=SCI_EDM.MaterialID) WHERE MainWorkCenter = 'LAC' AND SCIID = @SCIID;";
            DbHelperSQL.ExecuteSql(strSql, idps);
        }
    }
}
