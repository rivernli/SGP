using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.WF
{
    public class WFActivityField
    {
        public int ID { get; set; }
        public bool IsRequired { get; set; }
        public int FieldID { get; set; }
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public string DataType { get; set; }
        public string SubDataType { get; set; }
        public string KeyValueSource { get; set; }
        public int Sort { get; set; }
    }
}
