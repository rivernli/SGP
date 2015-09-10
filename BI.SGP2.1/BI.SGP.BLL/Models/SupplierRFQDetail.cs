using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models
{
    public class SupplierRFQDetail : DetailModelBase
    {
        public const string OPERATION_SAVE = "Save";
        public const string OPERATION_CLONE = "Clone";
        public const string OPERATION_REQUOTE = "ReQuote";

        public override string MainTable
        {
            get
            {
                return "SGP_RFQFORVVI";
            }
        }

        public override Dictionary<string, string> TableKey
        {
            get
            {
                if (_tableKey == null)
                {
                    _tableKey = new Dictionary<string, string>();
                    _tableKey.Add("SGP_RFQFORVVI", "ID");
                    _tableKey.Add("SGP_SUBDATA", "EntityID");
                }
                return _tableKey;
            }
        }

        public SupplierRFQDetail() { }

        public SupplierRFQDetail(List<FieldCategory> categories, int id, string number)
        {
            FillCategoryData(categories, id, number);
            InitAutoCalculateColumns(categories);
        }

        public SupplierRFQDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public SupplierRFQDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
            : base(categories, mainData, subData)
        { }

        public override int Add()
        {
            CreateNewNumber();
            AddWFFields();
            return base.Add();
        }

        public virtual int ReQuote()
        {
            CreateReQuoteNumber();
            AddWFFields();
            return base.Add();
        }

        public virtual int Clone()
        {
            return Add();
        }

        public virtual void CreateNewNumber()
        {
            string uid = AccessControl.CurrentLogonUser.Uid;
            int lastNumberIndex = 0;
            int numberLen = 7;
            string prefix = string.Format("GP{0}", uid.Substring(0, 3));
            prefix = prefix.ToUpper();
            string strSql = string.Format("SELECT TOP 1 [ExtNumber] FROM SGP_RFQ WHERE [ExtNumber] LIKE @Prefix ORDER BY [ExtNumber] DESC", prefix);
            string lastNumber = DbHelperSQL.GetSingle<string>(strSql, new SqlParameter("@Prefix", prefix + "%"));

            if (string.IsNullOrEmpty(lastNumber))
            {
                lastNumberIndex = 0;
            }
            else
            {
                int p = lastNumber.IndexOf('-');
                if (p > 0)
                {
                    lastNumber = lastNumber.Substring(0, p);
                }
                string s = lastNumber.Substring(prefix.Length);
                Int32.TryParse(s, out lastNumberIndex);
                lastNumberIndex++;
            }

            string newNumber = String.Format("{0}{1}-000", prefix, lastNumberIndex.ToString().PadLeft(numberLen, '0'));

            MainTableData.Add("Number", newNumber);
            MainTableData.Add("ExtNumber", newNumber);
        }

        public virtual void CreateReQuoteNumber()
        {
            FieldInfo fiExtNumber = AllMasterFields["ExtNumber"];
            string extNumber = fiExtNumber == null ? "" : string.Format("{0}", fiExtNumber.DataValue);
            if (String.IsNullOrEmpty(extNumber))
            {
                CreateNewNumber();
            }
            else
            {
                string[] arrPrefix = extNumber.Split('-');
                string prefix = arrPrefix.Length > 0 ? arrPrefix[0] : extNumber;
                string strSql = string.Format("SELECT ExtNumber FROM SGP_RFQ WHERE ExtNumber LIKE @Prefix ORDER BY ExtNumber DESC", prefix);
                string lastExtNumber = DbHelperSQL.GetSingle<string>(strSql, new SqlParameter("@Prefix", prefix + "%"));
                string[] lastArrPrefix = lastExtNumber.Split('-');
                int lastIndex = 0;
                if (lastArrPrefix.Length > 1)
                {
                    Int32.TryParse(lastArrPrefix[1], out lastIndex);
                }
                lastIndex++;
                string newSeq = lastIndex.ToString().PadLeft(3, '0');
                string newNumber = String.Format("{0}-{1}", prefix, newSeq);
                MainTableData.Add("Number", newNumber);
                MainTableData.Add("ExtNumber", newNumber);
            }
        }

        protected virtual void AddWFFields()
        {
            MainTableData.Add("TemplateID", "3");
            MainTableData.Add("ActivityID", "101");
            MainTableData.Add("EmployeeID", AccessControl.CurrentLogonUser.Uid);
        }

        public override void UpdateOther()
        {

        }

        public override void AddHistory()
        {
            string strSql = "INSERT INTO SGP_RFQHistory SELECT *,GETDATE() FROM V_SGP WHERE RFQID=@RFQID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", this.ID));
        }

        /// <summary>
        /// Add history of Supplier RFQ operate
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <param name="operationType"></param>
        /// Lance Chen 20150206
        public void AddSupplierRFQHistory(int id, string number, string operationType, SystemMessages sysMsg)
        {
            try
            {
                string strSql = "INSERT INTO SGP_SupplierRFQHistory SELECT *, GETDATE(), @OPERATIONTYPE, @USER FROM V_SGPForSupplier WHERE RFQID=@RFQID AND NVARCHAR1=@NUMBER";
                DbHelperSQL.ExecuteSql(strSql,
                    new SqlParameter("@OPERATIONTYPE", operationType),
                    new SqlParameter("@USER", AccessControl.CurrentLogonUser.Uid),
                    new SqlParameter("@RFQID", id),
                    new SqlParameter("@NUMBER", number));
            }
            catch (Exception ex)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add("Error", ex.Message);
            }
        }

        /// <summary>
        /// Update supplier RFQ data to SGP_SUBDATA
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// Lance Chen 20150122
        public void UpdateSubDataForSupplierRfq(int id, string number, out bool isOutOfCapability)
        {
            isOutOfCapability = false;
            foreach (string tableName in MasterTableData.Keys)
            {
                string strField = "";
                List<SqlParameter> listParames = new List<SqlParameter>();
                foreach (KeyValuePair<string, string> tableFields in MasterTableData[tableName])
                {
                    if (InitColumnValue(tableFields.Key, tableFields.Value))
                    {
                        continue;
                    }

                    if (tableFields.Key.ToUpper() == "NVARCHAR40" && tableFields.Value == "Out of Capability")
                    {
                        isOutOfCapability = true;
                    }

                    strField += tableFields.Key + "=@" + tableFields.Key + ",";
                    listParames.Add(String.IsNullOrEmpty(tableFields.Value.Trim()) ?
                        new SqlParameter("@" + tableFields.Key, DBNull.Value) :
                        new SqlParameter("@" + tableFields.Key, tableFields.Value));
                }

                string panelUtilization = PanelUtilization;
                string sqInchPriceUSD = SqInchPriceUSD;
                if (!string.IsNullOrEmpty(panelUtilization))
                {
                    strField += "NVARCHAR16=@NVARCHAR16,";
                    listParames.Add(new SqlParameter("@NVARCHAR16", panelUtilization));
                }

                if (!string.IsNullOrEmpty(sqInchPriceUSD))
                {
                    strField += "NVARCHAR37=@NVARCHAR37,";
                    listParames.Add(new SqlParameter("@NVARCHAR37", sqInchPriceUSD));
                }

                if (!string.IsNullOrEmpty(strField))
                {
                    strField = strField.TrimEnd(',');
                    string strSql = "UPDATE " + tableName + " SET " + strField + " WHERE EntityId=@EntityId AND NVARCHAR1=@Number";
                    listParames.Add(new SqlParameter("@EntityId", id));
                    listParames.Add(new SqlParameter("@Number", number));
                    DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                }
            }
        }

        public void InitAutoCalculateColumns(List<FieldCategory> categories)
        {
            foreach (FieldCategory category in categories)
            {
                if (category.CategoryName == "Supplier Product Information")
                {
                    UnitSizeWidth = ParseHelper.Parse<double>(category.MasterFields["FLOAT5"].DataValue);
                    UnitSizeLength = ParseHelper.Parse<double>(category.MasterFields["FLOAT6"].DataValue);
                    UnitArea = ParseHelper.Parse<double>(category.MasterFields["FLOAT19"].DataValue);
                    ArrayPerWorkingPanel = ParseHelper.Parse<double>(category.MasterFields["FLOAT18"].DataValue);
                    UnitPerArray = ParseHelper.Parse<double>(category.MasterFields["FLOAT11"].DataValue);
                    ArraySizeWidth = ParseHelper.Parse<double>(category.MasterFields["FLOAT7"].DataValue);
                    ArraySizeLength = ParseHelper.Parse<double>(category.MasterFields["FLOAT8"].DataValue);
                    PanelSizeWidth = ParseHelper.Parse<double>(category.MasterFields["FLOAT16"].DataValue);
                    PanelSizeLength = ParseHelper.Parse<double>(category.MasterFields["FLOAT17"].DataValue);
                    VariableCost = ParseHelper.Parse<double>(category.MasterFields["FLOAT12"].DataValue);

                    if (!string.IsNullOrEmpty(PanelUtilization))
                    {
                        category.MasterFields["NVARCHAR16"].DataValue = PanelUtilization;
                    }

                    if (!string.IsNullOrEmpty(SqInchPriceUSD))
                    {
                        category.MasterFields["NVARCHAR37"].DataValue = SqInchPriceUSD;
                    }
                }
            }
        }

        private bool InitColumnValue(string fieldName, string fieldValue)
        {
            bool skip = false;
            switch (fieldName)
            {
                case "NVARCHAR16":
                    skip = true;break;
                case "NVARCHAR37":
                    skip = true;break;
                case "FLOAT19":
                    UnitArea = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT11":
                    UnitPerArray = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT18":
                    ArrayPerWorkingPanel = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT16":
                    PanelSizeWidth = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT17":
                    PanelSizeLength = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT7":
                    ArraySizeWidth = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT8":
                    ArraySizeLength = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                case "FLOAT12":
                    VariableCost = string.IsNullOrEmpty(fieldValue) ?
                        0 :
                        Convert.ToDouble(fieldValue); break;
                default:break;
            }
            return skip;
        }

        public int UpdateSupplierRFQByExcel(DataRow dr, List<FieldCategory> categories)
        {
            int i = 0;
            foreach (FieldCategory category in categories)
            {
                if (category.CategoryName == "Supplier Product Information")
                {
                    string strField = string.Empty;
                    List<SqlParameter> listParames = new List<SqlParameter>();
                    FieldInfoCollecton masterField = category.MasterFields;
                    string number = dr["NVARCHAR1"].ToString();
                    foreach (FieldInfo fi in masterField)
                    {
                        if (!dr.Table.Columns.Contains(fi.FieldName))
                        {
                            continue;
                        }

                        switch (fi.FieldName.ToUpper())
                        {
                            case "INT1":
                                continue;
                            case "NVARCHAR1":
                                continue;
                            case "NVARCHAR16":
                                continue;
                            case "NVARCHAR37":
                                continue;
                            case "FLOAT19":
                                UnitArea = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT11":
                                UnitPerArray = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT18":
                                ArrayPerWorkingPanel = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT16":
                                PanelSizeWidth = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT17":
                                PanelSizeLength = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT7":
                                ArraySizeWidth = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT8":
                                ArraySizeLength = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            case "FLOAT12":
                                VariableCost = string.IsNullOrEmpty(dr[fi.FieldName].ToString()) ?
                                    0 :
                                    Convert.ToDouble(dr[fi.FieldName]); break;
                            default: break;
                        }

                        strField += fi.FieldName + "=@" + fi.FieldName + ",";
                        string fieldValue = dr[fi.FieldName].ToString();
                        if (!string.IsNullOrEmpty(fieldValue) &&
                            !string.IsNullOrEmpty(fieldValue.Trim()))
                        {
                            listParames.Add(new SqlParameter("@" + fi.FieldName, fieldValue));
                        }
                        else
                        {
                            listParames.Add(new SqlParameter("@" + fi.FieldName, DBNull.Value));
                        }
                    }

                    string panelUtilization = PanelUtilization;
                    string sqInchPriceUSD = SqInchPriceUSD;
                    if (!string.IsNullOrEmpty(panelUtilization))
                    {
                        strField += "NVARCHAR16=@NVARCHAR16,";
                        listParames.Add(new SqlParameter("@NVARCHAR16", PanelUtilization));
                    }

                    if (!string.IsNullOrEmpty(sqInchPriceUSD))
                    {
                        strField += "NVARCHAR37=@NVARCHAR37,";
                        listParames.Add(new SqlParameter("@NVARCHAR37", SqInchPriceUSD));
                    }

                    if (!string.IsNullOrEmpty(strField))
                    {
                        strField = strField.TrimEnd(',');
                        string strSql = "UPDATE SGP_SubData SET " + strField + " WHERE NVARCHAR1=@Number AND EntityName = 'VVIDETAIL' AND (INT1 = 0 OR INT1 IS NULL)";
                        listParames.Add(new SqlParameter("@Number", number));
                        i = DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                    }
                }
            }
            return i;
        }

        /// <summary>
        /// Update supplier RFQ submit status to submit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// Lance Chen 20150122
        public void UpdateSupplierRfqStatus(int id, string number)
        {
            string strSql = "UPDATE SGP_SUBDATA SET INT1=9,DATETIME3=GetDate() WHERE EntityId=@EntityId AND NVARCHAR1=@Number";
            DbHelperSQL.ExecuteSql(strSql,
                                    new SqlParameter("@EntityId", id),
                                    new SqlParameter("@Number", number));

        }

        public int GetSupplierRFQId(string number)
        {
            int id = 0;
            string sqlStr = "select EntityID FROM SGP_SUBDATA WHERE NVARCHAR1 = @NUMBER";
            DataSet ds = DbHelperSQL.Query(sqlStr, new SqlParameter("@NUMBER", number));
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                Int32.TryParse(ds.Tables[0].Rows[0][0].ToString(), out id);
            }
            return id;
        }

        public double UnitSizeWidth
        {
            get;
            set;
        }

        public double UnitSizeLength
        {
            get;
            set;
        }

        public double UnitArea
        {
            get;
            set;
        }

        public double ArrayPerWorkingPanel
        {
            get;
            set;
        }

        public double UnitPerArray
        {
            get;
            set;
        }

        public double ArraySizeWidth
        {
            get;
            set;
        }

        public double ArraySizeLength
        {
            get;
            set;
        }

        public double PanelSizeWidth
        {
            get;
            set;
        }

        public double PanelSizeLength
        {
            get;
            set;
        }

        public string UnitType
        {
            get;
            set;
        }

        public double VariableCost
        {
            get;
            set;
        }

        public string PanelUtilization
        {
            get
            {
                double d = 0;
                if (UnitArea > 0 && UnitPerArray > 0 && ArrayPerWorkingPanel > 0 && PanelSizeWidth > 0 && PanelSizeLength > 0)
                {
                    double p = 100;
                    p = p * UnitArea * UnitPerArray * ArrayPerWorkingPanel;
                    p = p / PanelSizeLength / PanelSizeWidth;

                    if (string.IsNullOrEmpty(UnitType) == false && UnitType.ToLower() == "mm")
                    {
                        p = p / 25.4 / 25.4;
                    }
                    d = p;
                    return Math.Round(d, 6).ToString() + "%";
                }
                return Math.Round(d, 6).ToString() + "%";
            }
            set { }
        }

        public string SqInchPriceUSD
        {
            get
            {
                double d = 0;
                if (ArraySizeWidth > 0 && ArraySizeLength > 0 && UnitPerArray > 0 && VariableCost > 0)
                {
                    d = VariableCost / (ArraySizeWidth * ArraySizeLength / UnitPerArray);
                }
                return Math.Round(d, 6).ToString();
            }
            set { }
        }
    }
}
