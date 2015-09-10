using System;
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
    public class B2FQuotationDetail : QuotationDetail
    {
        public override Dictionary<string, string> TableKey
        {
            get
            {
                if(_tableKey == null)
                {
                    base.TableKey.Add("SGP_BOMASSEMBLY", "RFQID");
                }
                return _tableKey;
            }
        }

        public B2FQuotationDetail() { }

        public B2FQuotationDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public B2FQuotationDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
            : base(categories, mainData, subData)
        { }

        protected override void AddWFFields()
        {
            MainTableData.Add("TemplateID", "2");
            MainTableData.Add("ActivityID", "201");
            MainTableData.Add("EmployeeID", AccessControl.CurrentLogonUser.Uid);
        }
    }
}
