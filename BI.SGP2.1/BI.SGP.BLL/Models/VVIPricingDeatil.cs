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
    public class VVIPricingDeatil : DetailModelBase
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
                   
                }
                return _tableKey;
            }
        }

        public VVIPricingDeatil() { }

        public VVIPricingDeatil(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public VVIPricingDeatil(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
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

    }
}
