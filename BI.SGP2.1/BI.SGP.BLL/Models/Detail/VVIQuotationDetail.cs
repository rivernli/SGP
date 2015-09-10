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
    public class VVIQuotationDetail : DetailModelBase
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
                    _tableKey.Add("SGP_RFQFORVVI", "RFQID");

                }
                return _tableKey;
            }
        }

        public VVIQuotationDetail() { }

        public VVIQuotationDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public VVIQuotationDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
            : base(categories, mainData, subData)
        { }
   
        protected virtual void AddWFFields()
        {
            MainTableData.Add("TemplateID", "3");
            MainTableData.Add("ActivityID", "101");
            MainTableData.Add("EmployeeID", AccessControl.CurrentLogonUser.Uid);
        }
        
        public override void UpdateOther()
        {
            string strsqlforPricing = string.Format(@"update ", AccessControl.CurrentLogonUser.Uid);

//            FPCRFQDetail rfqdetail = new FPCRFQDetail();

//            Type tp = typeof(FPCRFQDetail);

//            foreach (BI.SGP.BLL.DataModels.FieldInfo fi in AllMasterFields)
//            {

//                System.Reflection.PropertyInfo pi = tp.GetProperty(fi.FieldName);
//                if (pi != null && fi.DataValue != null)
//                {
//                    if (fi.DataValue is string && string.IsNullOrEmpty(fi.DataValue as string)) continue;

//                    if (fi.DataValue == DBNull.Value) continue;
//                    //try
//                    //{
//                    object obj = fi.DataValue;
//                    if (pi.PropertyType == typeof(double))
//                    {
//                        obj = ParseHelper.Parse<double>(fi.DataValue);
//                    }
//                    else if (pi.PropertyType == typeof(DateTime))
//                    {
//                        obj = ParseHelper.Parse<DateTime>(fi.DataValue);
//                    }
//                    else if (pi.PropertyType == typeof(int))
//                    {
//                        obj = ParseHelper.Parse<int>(fi.DataValue);
//                    }
//                    else if (pi.PropertyType == typeof(float))
//                    {
//                        obj = ParseHelper.Parse<float>(fi.DataValue);
//                    }
//                    pi.SetValue(rfqdetail, obj);
//                    //pi.SetValue(fi.FieldName, fi.DataValue, null);
//                    //}
//                    //catch (Exception ex) { 

//                    //}
//                }
//            }

//            string strsqlforPricing = string.Format(@"update SGP_PRICINGFORFPC set 
//                                                AssemblyCost1={1},AssemblyCost2={2},AssemblyCost3={3},AssemblyCost4={4},AssemblyCost5={5},
//                                                AssemblyPrice1={6},AssemblyPrice2={7},AssemblyPrice3={8},AssemblyPrice4={9},AssemblyPrice5={10},
//                                                BOMCost1={11},BOMCost2={12},BOMCost3={13},BOMCost4={14},BOMCost5={15},
//                                                BOMPrice1={16},BOMPrice2={17},BOMPrice3={18},BOMPrice4={19},BOMPrice5={20},
//                                                MP1={21},MP2={22},MP3={23},MP4={24},MP5={25},
//                                                OP1={26},OP2={27},OP3={28},OP4={29},OP5={30},
//                                                TotalPrice1={31},TotalPrice2={32},TotalPrice3={33},TotalPrice4={34},TotalPrice5={35},
//                                                TargetASP={36},TargetSqIn={37},TargetCLsqin={38},TargetVSActucal={39},
//                                                MinASP={40},MinSqInch={41},MinCLsqin={42}                                               
//                                                where rfqid={0}"
//                                              , this.ID
//                                              , rfqdetail.AssemblyCost1, rfqdetail.AssemblyCost2, rfqdetail.AssemblyCost3, rfqdetail.AssemblyCost4, rfqdetail.AssemblyCost5
//                                              , rfqdetail.AssemblyPrice1, rfqdetail.AssemblyPrice2, rfqdetail.AssemblyPrice3, rfqdetail.AssemblyPrice4, rfqdetail.AssemblyPrice5
//                                              , rfqdetail.BOMCost1, rfqdetail.BOMCost2, rfqdetail.BOMCost3, rfqdetail.BOMCost4, rfqdetail.BOMCost5
//                                              , rfqdetail.BOMPrice1, rfqdetail.BOMPrice2, rfqdetail.BOMPrice3, rfqdetail.BOMPrice4, rfqdetail.BOMPrice5
//                                              , rfqdetail.MP1, rfqdetail.MP2, rfqdetail.MP3, rfqdetail.MP4, rfqdetail.MP5
//                                              , rfqdetail.OP1, rfqdetail.OP2, rfqdetail.OP3, rfqdetail.OP4, rfqdetail.OP5
//                                              , rfqdetail.TotalPrice1, rfqdetail.TotalPrice2, rfqdetail.TotalPrice3, rfqdetail.TotalPrice4, rfqdetail.TotalPrice5
//                                              , rfqdetail.TargetASP, rfqdetail.TargetSqIn, rfqdetail.TargetCLsqin, rfqdetail.TargetVSActucal
//                                              , rfqdetail.MinASP, rfqdetail.MinSqInch, rfqdetail.MinCLsqin);
//            string strsqlforProduction = string.Format(@"update SGP_RFQProduction set
//                                                PanelUtilization='{1}'
//                                                where rfqid={0}"
//                                           , this.ID, rfqdetail.PanelUtilization);
//            string strsqlforCosting = string.Format(@"update SGP_RFQCostingProfitability set
//                                                TotalCost={1}, 
//                                                OP={2},
//                                                MP={3}
//                                                where rfqid={0}"
//                                           , this.ID, rfqdetail.TotalCost, rfqdetail.OP, rfqdetail.MP);
//            DbHelperSQL.ExecuteSql(strsqlforPricing);
//            DbHelperSQL.ExecuteSql(strsqlforProduction);
//            DbHelperSQL.ExecuteSql(strsqlforCosting);

        }

        public override void AddHistory()
        {
            string strSql = @"INSERT INTO SGP_RFQHistroyForVVI
                             select *, Getdate() from 
                             SGP_RFQForVVI a inner join SGP_SubData b on a.RFQID=b.EntityID and b.EntityName='VVIDETAIL'
                             where a.RFQID=@RFQID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", this.ID));
        }
        public  void AddHistoryForSubDetail(string VendorRFQNumber,int EntityID)
        {
            string strSql = @"INSERT INTO SGP_RFQHistroyForVVI
                             select * ,getdate() from SGP_RFQForVVI a inner join SGP_SubData b on a.RFQID=b.EntityID and b.EntityName='VVIDETAIL'
                             where a.RFQID=@RFQID and b.NVARCHAR1=@VendorRFQNumber";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", EntityID), new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
        }

        public void ReturnRFQ(string VendorRFQNumber, int EntityID)
        {
            string[] arrPrefix = VendorRFQNumber.Split('-');
            string prefix = arrPrefix.Length > 0 ? arrPrefix[0] : VendorRFQNumber;
            string vendorcode = arrPrefix.Length > 0 ? arrPrefix[2] : VendorRFQNumber;
            string strsql="update SGP_SubData set INT1=0 where INT1=9 and EntityName='VVIDETAIL' and NVARCHAR1=@VendorRFQNumber and EntityID=@EntityID";
            DbHelperSQL.ExecuteSql(strsql, new SqlParameter("@EntityID", EntityID), new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            AddHistoryForSubDetail(VendorRFQNumber,EntityID);
            SpecialSendMail(EntityID, VendorRFQNumber, vendorcode);
        }
        public void RedoRFQ(string VendorRFQNumber, int EntityID)
        {
            string[] arrPrefix = VendorRFQNumber.Split('-');
            string prefix = arrPrefix.Length > 0 ? arrPrefix[0] : VendorRFQNumber;
            string vendorcode = arrPrefix.Length > 0 ? arrPrefix[2] : VendorRFQNumber;
            string strSql = string.Format("SELECT NVARCHAR1 FROM SGP_SubData WHERE  EntityName='VVIDETAIL' and EntityID=@EntityID and NVARCHAR1 LIKE @Prefix ORDER BY NVARCHAR1 DESC", prefix);
            string lastExtNumber = DbHelperSQL.GetSingle<string>(strSql, new SqlParameter("@Prefix", prefix + "%"), new SqlParameter("@EntityID", EntityID), new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            string[] lastArrPrefix = lastExtNumber.Split('-');
            int lastIndex = 0;
            if (lastArrPrefix.Length > 1)
            {
                Int32.TryParse(lastArrPrefix[1], out lastIndex);
            }
            lastIndex++;
            string newSeq = lastIndex.ToString().PadLeft(3, '0');
            string newNumber = String.Format("{0}-{1}-{2}", prefix, newSeq,vendorcode);

            string strsql = "exec SP_VVIRFQRedo @EntityID,@VendorRFQNumber,@NewVendorRFQNumber";
            DbHelperSQL.ExecuteSql(strsql, new SqlParameter("@EntityID", EntityID), new SqlParameter("@VendorRFQNumber", VendorRFQNumber), new SqlParameter("@NewVendorRFQNumber", newNumber));
    
            AddHistoryForSubDetail(newNumber,EntityID);

            SpecialSendMail(EntityID,newNumber,vendorcode);

        }
        public void SpecialSendMail(int EntityID,string VendorRFQNumber,string VendorCode)
        {

            string vendormail = "";
            try
            {
                WF.WFUser user = new WF.WFUser(VendorCode);
                if (user != null)
                {
                    vendormail = user.Email;
                }
                else
                {
                    vendormail = "cfszy@163.com";
                }
            }
            catch
            {
                vendormail = "cfszy@163.com";
            }

            BI.SGP.BLL.WF.WFTemplate wftemplate = new WF.WFTemplate("SUPPLIERWF", EntityID, VendorRFQNumber);
            BI.SGP.BLL.WF.WFActivity wfactivity = wftemplate.FirstActivity;
            BI.SGP.BLL.WF.Action.SendMailAction sendmail = new WF.Action.SendMailAction();
            sendmail.DoActionForVVI(wfactivity, VendorCode, vendormail);


        }
        public bool CheckMainRFQStatusByID(int EntityID)
        {
            bool ispass = false;
            string strsql = "select * from SGP_RFQ where ID=@EntityID and ActivityID in(2,103) and StatusID=1";
            ispass=DbHelperSQL.Exists(strsql, new SqlParameter("@EntityID", EntityID));
            return ispass;
        }
        public bool CheckVendorRFQStatus(string VendorRFQNumber, int EntityID)
        {
            bool ispass = false;
            string strsql = "select * from SGP_SubData where EntityID=@EntityID and INT1=9 and EntityName='VVIDETAIL' and NVARCHAR1=@VendorRFQNumber";
            ispass = DbHelperSQL.Exists(strsql, new SqlParameter("@EntityID", EntityID), new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            return ispass;
        }
        public bool CheckRFQStatusByNumber(string VendorRFQNumber)
        {
            bool ispass = false;
            string strsql = "select * from SGP_RFQ where ActivityID in(2,104) and StatusID=1 and ID in (select distinct EntityID from SGP_SubData where EntityName='VVIDETAIL' and NVARCHAR1=@VendorRFQNumber)";
            ispass = DbHelperSQL.Exists(strsql,  new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            return ispass;
        }
        public bool CheckSubmitToVVIByNumber(string VendorRFQNumber)
        {
            bool ispass = false;
            string strsql = "select distinct EntityID from SGP_SubData where EntityName='VVIDETAIL' and NVARCHAR1=@VendorRFQNumber and NVARCHAR36='Yes'";
            ispass = DbHelperSQL.Exists(strsql, new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            return ispass;
        }
        public bool CheckClosedByNumber(string VendorRFQNumber)
        {
            bool ispass = false;
            string strsql = "select distinct EntityID from SGP_SubData where EntityName='VVIDETAIL' and NVARCHAR1=@VendorRFQNumber and INT1=9";
            ispass = DbHelperSQL.Exists(strsql, new SqlParameter("@VendorRFQNumber", VendorRFQNumber));
            return ispass;
        }


    }
}
