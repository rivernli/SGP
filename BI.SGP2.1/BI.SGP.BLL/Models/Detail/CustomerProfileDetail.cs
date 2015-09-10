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
    public class CustomerProfileDetail : CustomerDetailBase
    {
        public const string OPERATION_SAVE = "Save";
        public override string MainTable
        {
            get
            {
                return "SGP_CUSTOMERPROFILE_DATA";
            }
        }

        public override Dictionary<string, string> TableKey
        {
            get
            {
                if (_tableKey == null)
                {
                    _tableKey = new Dictionary<string, string>();
                    _tableKey.Add("SGP_CUSTOMERPROFILE_DATA", "ID");

                }
                return _tableKey;
            }
        }

        public CustomerProfileDetail() { }

        public CustomerProfileDetail(List<FieldCategory> categories, Dictionary<string, object> data)
            : base(categories, data)
        { }

        public CustomerProfileDetail(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
            : base(categories, mainData, subData)
        { }

    }
}
