using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models.Detail
{
    public class QuotationDetail : DetailModelBase
    {
        public const string OPERATION_SAVE = "Save";
        public const string OPERATION_CLONE = "Clone";
        public const string OPERATION_REQUOTE = "ReQuote";
        public const string OPERATION_CHANGEBUILDREV = "ChangeBuildREV";

        public override string MainTable
        {
            get
            {
                return "SGP_RFQ";
            }
        }

        public override Dictionary<string, string> TableKey
        {
            get
            {
                if (_tableKey == null)
                {
                    _tableKey = new Dictionary<string, string>();
                    _tableKey.Add("SGP_RFQ", "ID");
                    _tableKey.Add("SGP_RFQPRODUCTION", "RFQID");
                    _tableKey.Add("SGP_RFQGENERAL", "RFQID");
                    _tableKey.Add("SGP_RFQCOSTINGPROFITABILITY", "RFQID");
                    _tableKey.Add("SGP_RFQPRICING", "RFQID");
                    _tableKey.Add("SGP_RFQCLOSURE", "RFQID");
                    _tableKey.Add("SGP_TOOLINGSUMMARY", "RFQID");
                    _tableKey.Add("SGP_PRICINGFORFPC", "RFQID");

                }
                return _tableKey;
            }
        }

        public QuotationDetail() { }

        public QuotationDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public QuotationDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
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

        public virtual int ChangeBuildREV()
        {
            CreateNewRFQIDParamChange();
            AddWFFields();
            return base.Add();
        }
        public string REVFormat(string rev)
        {
            int i = 0;
            Int32.TryParse(rev, out i);
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
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
        public virtual void CreateNewRFQIDParamChange()
        {
            string extnumber = AllMasterFields["ExtNumber"].DataValue.ToString();
            string rev = AllMasterFields["InternalRevisionNumber"].DataValue.ToString();
            string building = AllMasterFields["Building"].DataValue.ToString();
            string newInternalNumber = string.Format("{0}-{1}{2}", extnumber, rev, building);

            MainTableData.Add("Number", newInternalNumber);
            MainTableData.Add("ExtNumber", extnumber);

            // Clone();
            // CloneWorkflowStatus(oldRFQId, rfqId);
            // CloneAttachments(oldRFQId, rfqId);
        }

        //        private static void CloneWorkflowStatus(int rfqIdFrom, int rfqIdTo)
        //        {
        //            string sql = string.Format(@" INSERT INTO SGP_CurrentUserTask(TemplateID,ActivityId,EntityId,UID) SELECT TemplateID,ActivityId,{0},UID from SGP_CurrentUserTask WHERE EntityId={1}"
        //                , rfqIdTo, rfqIdFrom);
        //            DbHelperSQL.ExecuteSql(sql);

        //            sql = string.Format("UPDATE SGP_RFQ SET ActivityId=(SELECT ActivityId FROM SGP_RFQ WHERE ID={0}) WHERE ID={1}"
        //                , rfqIdFrom, rfqIdTo);
        //            DbHelperSQL.ExecuteSql(sql);

        //            sql = string.Format(@"INSERT INTO SYS_WFProcessLog (EntityId,TemplateID,FromActivityId,ToActivityId,ActionUser,ActionTime,Comment) 
        //                                    SELECT {0},TemplateID,FromActivityId,ToActivityId,ActionUser,ActionTime,'CLONE' FROM SYS_WFProcessLog 
        //                                    WHERE EntityId={1} ORDER BY ID ASC", rfqIdTo, rfqIdFrom);
        //            DbHelperSQL.ExecuteSql(sql);

        //        }

        //        private static void CloneAttachments(int rfqIdFrom, int rfqIdTo)
        //        {
        //            string sql = string.Format(@" INSERT INTO SGP_Files([RelationKey],[FileName],[SourceName],[Folder],[FileSize],[CreateTime],[Creator],[Status])
        //                                          SELECT {1},[FileName],[SourceName],[Folder],[FileSize],[CreateTime],[Creator],[Status] FROM [SGP_Files] WHERE RelationKey={0}", rfqIdFrom, rfqIdTo);
        //            DbHelperSQL.ExecuteSql(sql);
        //        }
        public virtual void CreateReQuoteNumber()
        {
            BI.SGP.BLL.DataModels.FieldInfo fiExtNumber = AllMasterFields["ExtNumber"];
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
            MainTableData.Add("TemplateID", "2");
            MainTableData.Add("ActivityID", "201");
            MainTableData.Add("EmployeeID", AccessControl.CurrentLogonUser.Uid);
        }

        public override void UpdateOther()
        {
            FPCRFQDetail rfqdetail = new FPCRFQDetail();

            Type tp = typeof(FPCRFQDetail);

            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in AllMasterFields)
            {

                System.Reflection.PropertyInfo pi = tp.GetProperty(fi.FieldName);
                    if (pi != null && fi.DataValue != null)
                    {
                        if (fi.DataValue is string && string.IsNullOrEmpty(fi.DataValue as string)) continue;

                        if (fi.DataValue == DBNull.Value) continue;
                        //try
                        //{
                        object obj = fi.DataValue;
                        if (pi.PropertyType == typeof(double))
                        {
                            obj = ParseHelper.Parse<double>(fi.DataValue);
                        }
                        else if (pi.PropertyType == typeof(DateTime))
                        {
                            obj = ParseHelper.Parse<DateTime>(fi.DataValue);
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            obj = ParseHelper.Parse<int>(fi.DataValue);
                        }
                        else if (pi.PropertyType == typeof(float))
                        {
                            obj = ParseHelper.Parse<float>(fi.DataValue);
                        }
                        pi.SetValue(rfqdetail, obj);
                        //pi.SetValue(fi.FieldName, fi.DataValue, null);
                        //}
                        //catch (Exception ex) { 

                        //}
                    }
            }

            string strsqlforPricing = string.Format(@"update SGP_PRICINGFORFPC set 
                                                AssemblyCost1={1},AssemblyCost2={2},AssemblyCost3={3},AssemblyCost4={4},AssemblyCost5={5},
                                                AssemblyPrice1={6},AssemblyPrice2={7},AssemblyPrice3={8},AssemblyPrice4={9},AssemblyPrice5={10},
                                                BOMCost1={11},BOMCost2={12},BOMCost3={13},BOMCost4={14},BOMCost5={15},
                                                BOMPrice1={16},BOMPrice2={17},BOMPrice3={18},BOMPrice4={19},BOMPrice5={20},
                                                MP1={21},MP2={22},MP3={23},MP4={24},MP5={25},
                                                OP1={26},OP2={27},OP3={28},OP4={29},OP5={30},
                                                TotalPrice1={31},TotalPrice2={32},TotalPrice3={33},TotalPrice4={34},TotalPrice5={35},
                                                TargetASP={36},TargetSqIn={37},TargetCLsqin={38},TargetVSActucal={39},
                                                MinASP={40},MinSqInch={41},MinCLsqin={42}                                               
                                                where rfqid={0}"
                                              , this.ID
                                              , rfqdetail.AssemblyCost1, rfqdetail.AssemblyCost2, rfqdetail.AssemblyCost3, rfqdetail.AssemblyCost4, rfqdetail.AssemblyCost5
                                              , rfqdetail.AssemblyPrice1, rfqdetail.AssemblyPrice2, rfqdetail.AssemblyPrice3, rfqdetail.AssemblyPrice4, rfqdetail.AssemblyPrice5
                                              , rfqdetail.BOMCost1, rfqdetail.BOMCost2, rfqdetail.BOMCost3, rfqdetail.BOMCost4, rfqdetail.BOMCost5
                                              , rfqdetail.BOMPrice1, rfqdetail.BOMPrice2, rfqdetail.BOMPrice3, rfqdetail.BOMPrice4, rfqdetail.BOMPrice5
                                              , rfqdetail.MP1, rfqdetail.MP2, rfqdetail.MP3, rfqdetail.MP4, rfqdetail.MP5
                                              , rfqdetail.OP1, rfqdetail.OP2, rfqdetail.OP3, rfqdetail.OP4, rfqdetail.OP5
                                              , rfqdetail.TotalPrice1, rfqdetail.TotalPrice2, rfqdetail.TotalPrice3, rfqdetail.TotalPrice4, rfqdetail.TotalPrice5
                                              , rfqdetail.TargetASP, rfqdetail.TargetSqIn, rfqdetail.TargetCLsqin, rfqdetail.TargetVSActucal
                                              , rfqdetail.MinASP, rfqdetail.MinSqInch, rfqdetail.MinCLsqin);
            string strsqlforProduction = string.Format(@"update SGP_RFQProduction set
                                                PanelUtilization='{1}',
                                                UnitPerWorkingPanel='{2}'
                                                where rfqid={0}"
                                           , this.ID, rfqdetail.PanelUtilization,rfqdetail.UnitPerWorkingPanel);
            string strsqlforCosting = string.Format(@"update SGP_RFQCostingProfitability set
                                                TotalCost={1}, 
                                                OP={2},
                                                MP={3}
                                                where rfqid={0}"
                                           , this.ID, rfqdetail.TotalCost, rfqdetail.OP, rfqdetail.MP);
            string strsqlfortooling = string.Format(@"update SGP_TOOLINGSUMMARY set
                                                Total_1={1}, 
                                                Total_2={2},
                                                Total_3={3},
                                                Total_4={4}
                                                where rfqid={0}"
                                           , this.ID, rfqdetail.Total_1, rfqdetail.Total_2, rfqdetail.Total_3,rfqdetail.Total_4);
            DbHelperSQL.ExecuteSql(strsqlfortooling);
            DbHelperSQL.ExecuteSql(strsqlforPricing);
            DbHelperSQL.ExecuteSql(strsqlforProduction);
            DbHelperSQL.ExecuteSql(strsqlforCosting);

        }

//        public  void CloneAttachments(int rfqIdFrom, int rfqIdTo)
//        {
//            string sql = string.Format(@" INSERT INTO SGP_Files([RelationKey],[FileName],[SourceName],[Folder],[FileSize],[Category],[CategoryDesc],[CreateTime],[Creator],[Status])
//                                          SELECT {1},[FileName],[SourceName],[Folder],[FileSize],[Category],[CategoryDesc],[CreateTime],[Creator],[Status] FROM [SGP_Files] WHERE RelationKey={0}", rfqIdFrom, rfqIdTo);
//            DbHelperSQL.ExecuteSql(sql);
//        }

        public override void AddHistory()
        {
            string strSql = "INSERT INTO SGP_RFQHISTORYFORFPC SELECT *,GETDATE() FROM V_SGPForFPC WHERE RFQID=@RFQID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", this.ID));
        }

    }
}
