using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.WF
{
    public class WFActivityCollection : List<WFActivity>
    {
        public WFActivityCollection() { }

        public WFActivityCollection(List<WFActivity> list)
        {
            if (list != null)
            {
                this.AddRange(list);
            }
        }

        public WFActivity Get(int activityId)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].ID == activityId) return this[i];
            }
            return null;
        }
    }
}
