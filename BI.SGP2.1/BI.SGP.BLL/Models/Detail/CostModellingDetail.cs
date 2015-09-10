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
    public class CostModellingDetail :DetailModelBase
    {
        public const string OPERATION_SAVE = "Save";
        public const string OPERATION_CLONE = "Clone";
        public const string OPERATION_REQUOTE = "ReQuote";

        public override string MainTable
        {
            get
            {
                return "SCI_PRODUCTIONINFORMATION";
            }
        }

        public override Dictionary<string, string> TableKey
        {
            get
            {
                if (_tableKey == null)
                {
                    _tableKey = new Dictionary<string, string>();
                    _tableKey.Add("SCI_PRODUCTIONINFORMATION", "ID");
                    _tableKey.Add("SCI_BOM","SCIID");
                    _tableKey.Add("SCI_EDM","SCIID");
                    _tableKey.Add("SCI_PROCESSFLOW","SCIID");
                    _tableKey.Add("SCI_SPECIALPROCESSINFORMATION", "SCIID");
                    _tableKey.Add("SCI_TARGETYIELD","SCIID");
                }
                return _tableKey;
            }
        }

        public CostModellingDetail() { }

        public CostModellingDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public CostModellingDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
            : base(categories, mainData, subData)
        { }

  
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
    
    }
}
