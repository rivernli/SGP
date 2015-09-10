using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BI.SGP.BLL.DataModels
{
    public class FieldGroupDetailCollection : List<FieldGroupDetail>
    {
        public FieldGroupDetailCollection() { }

        public FieldGroupDetailCollection(List<FieldGroupDetail> list)
        {
            if (list != null)
            {
                this.AddRange(list);
            }
        }

        public FieldGroupDetail this[string fieldName]
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
