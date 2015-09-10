using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.DataModels
{
    public class FieldGroupDetail : FieldInfo
    {
        public string UID { get; set; }
        public int Width_Detail { get; set; }
        public string Format_Detail { get; set; }
        public int Sort_Detail { get; set; }
    }
}
