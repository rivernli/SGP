using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.DataModels
{
    public class FieldInfoCollecton : List<FieldInfo>
    {
        public FieldInfoCollecton() { }

        public FieldInfoCollecton(List<FieldInfo> list)
        {
            if (list != null)
            {
                this.AddRange(list);
            }
        }

        public FieldInfo this[string fieldName]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].FieldName == fieldName) return this[i];
                }
                return null;
            }
        }
    }
}
