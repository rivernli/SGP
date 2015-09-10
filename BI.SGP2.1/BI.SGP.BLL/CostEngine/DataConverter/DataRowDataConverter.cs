using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BI.SGP.BLL.CostEngine.DataConverter
{
    public class DataRowDataConverter : DataConverterBase
    {
        public override Dictionary<string, object> ReadData(object value)
        {
            DataRow dr = value as DataRow;
            if (dr != null)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (DataColumn dc in dr.Table.Columns)
                {
                    dic.Add(dc.ColumnName, dr[dc.ColumnName]);
                }

                return dic;
            }

            return null;
        }
    }
}
