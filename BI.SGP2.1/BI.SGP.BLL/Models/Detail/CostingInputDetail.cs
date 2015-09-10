using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.CostEngine;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models.Detail
{
    public class CostingInputDetail
    {
        public int ID { get; set; }
        public FieldCategory CommonCategory { get; set; }
        public FieldCategory BasicInfoCategory { get; set; }
        public FieldCategory ProcessFlowCategory { get; set; }
        public FieldCategory BOMCategory { get; set; }
        public List<FieldCategory> EDMCategories { get; set; }
        public List<FieldCategory> SpecialCategory { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public Dictionary<string, object> YieldData { get; set; }

        public static readonly string BasicDataKey = "BasicDataKey";
        public static readonly string ProcFlowDataKey = "ProcFlowDataKey";
        public static readonly string BOMDataKey = "BomDataKey";
        public static readonly string EDMDataKey = "EdmDataKey";

        public CostingInputDetail()
        {

        }

        public static Dictionary<string, DataTable> GetData(int dataId)
        {
            Dictionary<string, DataTable> dicData = new Dictionary<string, DataTable>();
            SqlParameter ps = new SqlParameter("@DataID", dataId);
            string strSql = "SELECT * FROM SCI_ProductionInformation WHERE ID = @DataID";
            DataTable basicData = DbHelperSQL.Query(strSql, ps).Tables[0];
            strSql = "SELECT * FROM SCI_ProcessFlow WHERE SCIID = @DataID ORDER BY Step,ID";
            DataTable procFlowData = DbHelperSQL.Query(strSql, ps).Tables[0];
            strSql = "SELECT * FROM SCI_BOM WHERE SCIID = @DataID ORDER BY ID";
            DataTable bomData = DbHelperSQL.Query(strSql, ps).Tables[0];
            strSql = "SELECT * FROM SCI_EDM WHERE SCIID = @DataID ORDER BY DataType, ID";
            DataTable edmData = DbHelperSQL.Query(strSql, ps).Tables[0];
            dicData.Add(BasicDataKey, basicData);
            dicData.Add(ProcFlowDataKey, procFlowData);
            dicData.Add(BOMDataKey, bomData);
            dicData.Add(EDMDataKey, edmData);
            return dicData;
        }

        private void FillData(bool subData, params FieldCategory[] fcs)
        {
            foreach (FieldCategory fc in fcs)
            {
                if (Data.ContainsKey(fc.ID))
                {
                    Dictionary<string, object> data = Data[fc.ID] as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> kv in data)
                    {
                        fc.SetFieldValue(kv, fc.Fields, subData);
                    }
                }
            }
        }

        public void FillData(Dictionary<string, object> data)
        {
            this.Data = data;
            FillData(false, CommonCategory);
            FillData(false, BasicInfoCategory);
            FillData(true, ProcessFlowCategory);
            FillData(true, BOMCategory);
            FillData(true, EDMCategories.ToArray());
            FillData(true, SpecialCategory.ToArray());
        }

        private void CheckDataType(SystemMessages sysMsg, params FieldCategory[] fcs)
        {
            foreach (FieldCategory fc in fcs)
            {
                if (Data.ContainsKey(fc.ID))
                {
                    fc.CheckDataType(Data[fc.ID] as Dictionary<string, object>, sysMsg);
                }
            }
        }

        private void CheckRequired(SystemMessages sysMsg, params FieldCategory[] fcs)
        {
            foreach (FieldCategory fc in fcs)
            {
                foreach (FieldInfo fi in fc.Fields)
                {
                    fi.CheckRequired(sysMsg);
                }
            }
        }

        private void CheckProcessFlow(SystemMessages sysMsg)
        {
            ArrayList arrVal = ProcessFlowCategory.Fields["WorkCenter"].DataValue as ArrayList;
            if (arrVal == null || arrVal.Count == 0)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add("Process Flow Check", "\"Process Flow\" is required.");
            }
        }

        private void CheckCategoryWorkCenter(SystemMessages sysMsg)
        {

        }

        private void CheckSpecRequired(SystemMessages sysMsg)
        {
            if (CostBase.GetPCBType(Convert.ToString(CommonCategory.Fields["CostingBasedOn"].DataValue)) == CostBase.PCBTYPE_FPC)
            {
                foreach (FieldCategory fc in SpecialCategory)
                {
                    if (fc.CategoryName == "SMCM")
                    {
                        ArrayList arrMWC = fc.Fields["MainWorkCenter"].DataValue as ArrayList;
                        if (arrMWC != null)
                        {
                            for (int i = 0; i < arrMWC.Count; i++)
                            {
                                if (Convert.ToString(arrMWC[i]) == "COM")
                                {
                                    fc.Fields["SidePanel"].CheckRequired(sysMsg, i);
                                }
                                else
                                {
                                    fc.Fields["Thickness"].CheckRequired(sysMsg, i);
                                    fc.Fields["Area"].CheckRequired(sysMsg, i);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckDataType(SystemMessages sysMsg)
        {
            CheckDataType(sysMsg, CommonCategory);
            CheckDataType(sysMsg, BasicInfoCategory);
            CheckDataType(sysMsg, ProcessFlowCategory);
            CheckDataType(sysMsg, BOMCategory);
            CheckDataType(sysMsg, EDMCategories.ToArray());
            CheckDataType(sysMsg, SpecialCategory.ToArray());
        }

        public void CheckRequired(SystemMessages sysMsg)
        {
            CheckRequired(sysMsg, CommonCategory);
            CheckRequired(sysMsg, BasicInfoCategory);
            CheckProcessFlow(sysMsg);
            CheckRequired(sysMsg, ProcessFlowCategory);
            CheckRequired(sysMsg, BOMCategory);
            CheckRequired(sysMsg, EDMCategories.ToArray());
            CheckRequired(sysMsg, SpecialCategory.ToArray());
            CheckCategoryWorkCenter(sysMsg);
            CheckSpecRequired(sysMsg);
        }

        private void AddBasicInfo()
        {
            List<SqlParameter> listParames = new List<SqlParameter>();
            string strField = "";
            string strValue = "";

            foreach(FieldInfo f in CommonCategory.Fields) 
            {
                if (f.CurrentlyInUse == true)
                {
                    strField += f.FieldName + ",";
                    strValue += "@" + f.FieldName + ",";
                    string fieldValue = Convert.ToString(f.DataValue).Trim();
                    listParames.Add(String.IsNullOrEmpty(fieldValue) ? new SqlParameter("@" + f.FieldName, DBNull.Value) : new SqlParameter("@" + f.FieldName, fieldValue));
                }
            }
            foreach (FieldInfo f in BasicInfoCategory.Fields)
            {
                if (f.CurrentlyInUse == true)
                {
                    strField += f.FieldName + ",";
                    strValue += "@" + f.FieldName + ",";
                    string fieldValue = Convert.ToString(f.DataValue).Trim();
                    listParames.Add(String.IsNullOrEmpty(fieldValue) ? new SqlParameter("@" + f.FieldName, DBNull.Value) : new SqlParameter("@" + f.FieldName, fieldValue));
                }
            }

            strField = strField.TrimEnd(',');
            strValue = strValue.TrimEnd(',');
            string strSql = "INSERT INTO SCI_ProductionInformation(" + strField + ",Status) VALUES(" + strValue + ",1);SELECT @@IDENTITY";
            this.ID = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
        }

        private void UpdateBasicInfo()
        {
            string strField = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            listParames.Add(new SqlParameter("@ID", this.ID));
            foreach (FieldInfo f in CommonCategory.Fields)
            {
                strField += f.FieldName + "=@" + f.FieldName + ",";
                string fieldValue = Convert.ToString(f.DataValue).Trim();
                listParames.Add(String.IsNullOrEmpty(fieldValue) ? new SqlParameter("@" + f.FieldName, DBNull.Value) : new SqlParameter("@" + f.FieldName, fieldValue));
            }
            foreach (FieldInfo f in BasicInfoCategory.Fields)
            {
                strField += f.FieldName + "=@" + f.FieldName + ",";
                string fieldValue = Convert.ToString(f.DataValue).Trim();
                listParames.Add(String.IsNullOrEmpty(fieldValue) ? new SqlParameter("@" + f.FieldName, DBNull.Value) : new SqlParameter("@" + f.FieldName, fieldValue));
            }
            strField = strField.TrimEnd(',');
            string strSql = "UPDATE SCI_ProductionInformation SET " + strField + ",Status=1,LastUpdate=GETDATE() WHERE ID=@ID;";
            DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
        }

        private void UpdateSubData(FieldInfoCollecton fields, string tableName, string dataType)
        {
            string strSql = "DELETE FROM " + tableName + " WHERE SCIID=@SCIID AND DataType=@DataType";
            List<SqlParameter> listMainParames = new List<SqlParameter>();
            listMainParames.Add(new SqlParameter("@SCIID", this.ID));
            listMainParames.Add(new SqlParameter("@DataType", dataType));
            DbHelperSQL.ExecuteSql(strSql, listMainParames.ToArray());

            string strField = "ID,SCIID,DataType,", strValue = "@ID,@SCIID,@DataType,";
            int subCount = 0;

            foreach (FieldInfo f in fields)
            {
                ArrayList arrVal = f.DataValue as ArrayList;
                if (arrVal != null)
                {
                    if (f.CurrentlyInUse == true)
                    {
                        strField += f.FieldName + ",";
                        strValue += "@" + f.FieldName + ",";
                    }
                    if (subCount == 0)
                    {
                        subCount = arrVal.Count;
                    }
                } 
            }
            strField = strField.TrimEnd(',');
            strValue = strValue.TrimEnd(',');
            strSql = "INSERT INTO " + tableName + "(" + strField + ") VALUES(" + strValue + ")";

            for (int i = 0; i < subCount; i++)
            {
                List<SqlParameter> listParames = new List<SqlParameter>();
                listParames.AddRange(listMainParames);
                listParames.Add(new SqlParameter("@ID", i + 1));

                foreach (FieldInfo f in fields)
                {
                    ArrayList arrVal = f.DataValue as ArrayList;
                    listParames.Add(new SqlParameter("@" + f.FieldName, String.IsNullOrEmpty(arrVal[i].ToString()) ? DBNull.Value : arrVal[i]));
                }
                DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
            }
        }

        private void UpdateProcessFlow()
        {
            UpdateSubData(ProcessFlowCategory.Fields, "SCI_ProcessFlow", "ProcessFlow");
        }

        private void UpdateBOM()
        {
            UpdateSubData(BOMCategory.Fields, "SCI_BOM", "BOM");
        }

        private void UpdateEDM()
        {
            foreach (FieldCategory fc in EDMCategories)
            {
                UpdateSubData(fc.Fields, "SCI_EDM", fc.Fields[0].SubDataType);
            }
        }

        private void UpdateSpecial()
        {
            foreach (FieldCategory fc in SpecialCategory)
            {
                UpdateSubData(fc.Fields, "SCI_EDM", fc.Fields[0].SubDataType);
            }
        }

        private void UpdateYield()
        {
            string strSql = "DELETE FROM SCI_TargetYield WHERE SCIID = @SCIID";
            List<SqlParameter> lstParams = new List<SqlParameter>();
            SqlParameter idps = new SqlParameter("@SCIID", this.ID);
            DbHelperSQL.ExecuteSql(strSql, idps);

            strSql = "INSERT INTO SCI_TargetYield(SCIID,ItemName,ItemValue) VALUES(@SCIID,@ItemName,@ItemValue)";
            if (YieldData != null)
            {
                foreach (KeyValuePair<string, object> kv in YieldData)
                {
                    DbHelperSQL.ExecuteSql(strSql, idps, new SqlParameter("@ItemName", kv.Key), new SqlParameter("@ItemValue", kv.Value));
                }
            }

            strSql = "UPDATE SCI_ProductionInformation SET TargetYield=(ISNULL((SELECT Yield FROM SCD_BaseYield WHERE Version=SCI_ProductionInformation.CostVersion AND PCBType=@PCBType AND ProductType=SCI_ProductionInformation.ViaStructure AND LayerCount=SCI_ProductionInformation.LayerCount),1)-ISNULL((SELECT SUM(CASE WHEN ItemValue = '' THEN 0 WHEN ItemName = 'Adjust Yield(%)' THEN CAST(ItemValue AS FLOAT)/100 ELSE CAST(ItemValue AS FLOAT) END) FROM SCI_TargetYield WHERE SCIID=SCI_ProductionInformation.ID),0))  WHERE ID=@SCIID";
            DbHelperSQL.ExecuteSql(strSql, idps, new SqlParameter("@PCBType", CostBase.GetPCBType(Convert.ToString(CommonCategory.Fields["CostingBasedOn"].DataValue))));
        }

        private void UpdateMainWorkCenter()
        {
            SqlParameter[] ps = new SqlParameter[] { 
                new SqlParameter("@SCIID", this.ID),
                new SqlParameter("@Plant", Convert.ToString(CommonCategory.Fields["CostingBasedOn"].DataValue))
            };

            string strSql = "UPDATE SCI_ProcessFlow SET MainWorkCenter=ISNULL((SELECT MainWorkCenter FROM SCM_SubWorkCenter WHERE Plant=ISNULL(SCI_ProcessFlow.SubcontractPlant, @Plant) AND Name=SCI_ProcessFlow.WorkCenter), SCI_ProcessFlow.WorkCenter) WHERE SCIID = @SCIID;";
            string orgSql = @"UPDATE {0} SET MainWorkCenter=ISNULL((SELECT MainWorkCenter FROM SCM_SubWorkCenter WHERE Plant={0}.Plant AND Name={0}.WorkCenter), {0}.WorkCenter) WHERE SCIID = @SCIID;";

            strSql += String.Format(orgSql, "SCI_BOM");
            strSql += String.Format(orgSql, "SCI_EDM");

            DbHelperSQL.ExecuteSql(strSql, ps);
        }

        private void UpdateOther()
        {
            UpdateProcessFlow();
            UpdateBOM();
            UpdateEDM();
            UpdateSpecial();
            UpdateYield();
            UpdateMainWorkCenter();
        }

        public static void Delete(int id)
        {
            string strSql = "UPDATE SCI_ProductionInformation SET Status = 0, LastUpdate=GETDATE() WHERE ID=@ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        public int Add()
        {
            AddBasicInfo();
            UpdateOther();
            return this.ID;
        }

        public void Update(int id)
        {
            this.ID = id;
            UpdateBasicInfo();
            UpdateOther();
        }

        public int Clone()
        {
            string version = "";
            string period = "";
            string strSql = "SELECT (SELECT Version FROM SCM_Version WHERE Status='Active') AS Version,(SELECT Period FROM SCM_Period WHERE Status='Active') AS Period";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            if (dt.Rows.Count > 0)
            {
                version = Convert.ToString(dt.Rows[0]["Version"]);
                period = Convert.ToString(dt.Rows[0]["Period"]);
            }

            if (!String.IsNullOrWhiteSpace(version))
            {
                CommonCategory.Fields["CostVersion"].DataValue = version;
            }

            if (!String.IsNullOrWhiteSpace(period))
            {
                CommonCategory.Fields["CostPeriod"].DataValue = period;
            }

            AddBasicInfo();
            UpdateProcessFlow();
            UpdateYield();

            strSql = "UPDATE SCI_ProductionInformation SET FlowConfirm = 0 WHERE ID = @ID";

            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", this.ID));

            return this.ID;
        }

        public static int SubmitToRFQ(int dataID, SystemMessages sysMsg)
        {
            int rfqId = 0;
            string strSql = "SELECT * FROM SCI_ProductionInformation WHERE ID=@DataID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@DataID", dataID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                DataRow drData = dt.Rows[0];
                strSql = "SELECT ID FROM SGP_RFQ WHERE Number=@Number";
                rfqId = DbHelperSQL.GetSingle<int>(strSql, new SqlParameter("@Number", drData["RFQNumber"]));

                if (rfqId > 0)
                {
                    strSql = "UPDATE SGP_RFQ SET InternalRevisionNumber=@InternalRevisionNumber WHERE ID=@RFQID";
                    DbHelperSQL.Query(strSql, new SqlParameter("@InternalRevisionNumber", drData["InternalRevision"]), new SqlParameter("@RFQID", rfqId));

                    Dictionary<string, string> dicProd = new Dictionary<string, string>();
                    dicProd.Add("LayerCount", "LayerCount");
                    dicProd.Add("ViaStructure", "ViaStructure");
                    dicProd.Add("MaterialCategory", "MaterialType");
                    dicProd.Add("BoardThickness", "BoardThickness");
                    dicProd.Add("UnitType", "Measure");
                    dicProd.Add("UnitSizeWidth", "UnitSizeWidth");
                    dicProd.Add("UnitSizeLength", "UnitSizeLength");
                    dicProd.Add("UnitOrArray", "UnitOrArray");
                    dicProd.Add("UnitPerArray", "UnitPerArray");
                    dicProd.Add("ArraySizeWidth", "ArraySizeWidth");
                    dicProd.Add("ArraySizeLength", "ArraySizeLength");
                    dicProd.Add("UnitPerWorkingPanel", "UnitPerWorkingPanel");
                    dicProd.Add("PanelUtilization", "PanelUtilization");
                    dicProd.Add("Outline", "OutLine");
                    dicProd.Add("LnO", "LNO");
                    dicProd.Add("LnI", "LNI");
                    dicProd.Add("TechnicalRemarks", "TechnicalRemarks");
                    dicProd.Add("BoardConstruction", "Construction");
                    dicProd.Add("MicroViaSize", "MicroViaSize");
                   
                    UpdateRFQKeyValue("SGP_RFQProduction", rfqId, dicProd, drData);

                    dicProd = new Dictionary<string, string>();
                    dicProd.Add("MaterialCost", "MaterialCost");
                    dicProd.Add("VariableCost", "VariableCost");
                    dicProd.Add("FixedCost", "FixedCost");
                    dicProd.Add("TotalCost", "TotalCost");
                    dicProd.Add("Yield", "TargetYield");

                    UpdateRFQKeyValue("SGP_RFQCostingProfitability", rfqId, dicProd, drData);
                }
            }

            return rfqId;
        }

        private static void UpdateRFQKeyValue(string tableName, int RFQID, Dictionary<string, string> dicProd, DataRow drData)
        {
            string strSql = "UPDATE " + tableName + " SET ";
            List<SqlParameter> ps = new List<SqlParameter>();
            foreach (KeyValuePair<string, string> kv in dicProd)
            {
                strSql += kv.Key + "=@" + kv.Key + ",";
                ps.Add(new SqlParameter("@" + kv.Key, drData[kv.Value]));
            }

            strSql = strSql.TrimEnd(',');

            strSql += " WHERE RFQID=@RFQID";
            ps.Add(new SqlParameter("@RFQID", RFQID));
            DbHelperSQL.Query(strSql, ps.ToArray());
        }

        public void FillMainCategoryData(FieldCategory category, int dataId)
        {
            string strSql = "SELECT * FROM SCI_ProductionInformation WHERE ID=@ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", dataId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (FieldInfo fi in category.Fields)
                {
                    if (dt.Columns.Contains(fi.FieldName))
                    {
                        fi.DataValue = dt.Rows[0][fi.FieldName];
                    }
                }
            }
        }

        public void FillSubCategoryData(FieldCategory category, int dataId)
        {
            string tableName = category.Fields[0].TableName;
            string dataType = category.Fields[0].SubDataType;

            string strSql = "SELECT * FROM " + tableName + " WHERE SCIID=@SCIID AND DataType=@DataType ORDER BY ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@SCIID", dataId), new SqlParameter("@DataType", dataType)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (FieldInfo fi in category.Fields)
                {
                    if (dt.Columns.Contains(fi.FieldName))
                    {
                        ArrayList arr = new ArrayList();
                        foreach (DataRow dr in dt.Rows)
                        {
                            arr.Add(dr[fi.FieldName]);
                        }
                        fi.DataValue = arr;
                    }
                }
            }
        }

        public void FillYieldData(Dictionary<string, string> data, int dataId)
        {
            string strSql = "SELECT ItemName,ItemValue FROM SCI_TargetYield WHERE SCIID=@SCIID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@SCIID", dataId)).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                data.Add(Convert.ToString(dr["ItemName"]), Convert.ToString(dr["ItemValue"]));
            }
        }

        public static string CreateProcGroup(string ctrlName, string width) 
        {
            StringBuilder strControl = new StringBuilder();
            DataTable dt = DbHelperSQL.Query("SELECT DISTINCT ProcGroup AS Value FROM SCM_ProcFlowGroup ORDER BY ProcGroup").Tables[0];
            strControl.AppendFormat("<select class='form-control form-field' style='width:{1} !important' name='{0}' id='{0}'><option value=''></option>", ctrlName, width);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strControl.AppendFormat("<option value='{0}'>{0}</option>", dr["Value"]);
                }
            }

            strControl.Append("</select>");

            return strControl.ToString();
        }

        public static bool WorkCenterExists(string workCenter, string plant)
        {
            string strSql = "SELECT (SELECT COUNT(*) FROM SCM_MainWorkCenter WHERE Name=@WorkCenter AND Plant=@Plant)+(SELECT COUNT(*) FROM SCM_SubWorkCenter WHERE Name=@WorkCenter AND Plant=@Plant)";

            return DbHelperSQL.GetSingle<int>(strSql, new SqlParameter("@WorkCenter", workCenter), new SqlParameter("@Plant", plant)) > 0;
        }
    }
}
