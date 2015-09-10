using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BI.SGP.BLL.UIManager
{
    public class TableFormat
    {
        public DataTable DataTable { get; set; }
        public List<TableFormatString> formatStrings { get; set; }
        public TableFormat(DataTable datatable, params TableFormatString[] formatstrings)
        {
            this.DataTable = datatable;
            this.formatStrings = new List<TableFormatString>();

            foreach (TableFormatString formatstring in formatstrings)
            {
                this.formatStrings.Add(formatstring);
            }
        }
    }
}
