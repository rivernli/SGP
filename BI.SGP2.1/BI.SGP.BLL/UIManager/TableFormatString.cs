using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.UIManager
{
    public class TableFormatString
    {
        public string ColumnName { get; set; }
        public string DataFormatString { get; set; }
        public bool IsCreateNew { get; set; }
        public string NewColumnName { get; set; }

        public TableFormatString(string columnName, string dataFormatString)
        {
            this.ColumnName = columnName;
            this.DataFormatString = dataFormatString;
            this.IsCreateNew = false;
            this.NewColumnName = columnName;
        }

        public TableFormatString(string columnName, string dataFormatString, string newColumnName)
        {
            this.ColumnName = columnName;
            this.DataFormatString = dataFormatString;
            this.IsCreateNew = true;
            this.NewColumnName = newColumnName;
        }
    }
}
