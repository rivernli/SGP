using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.Models
{
    public class MessageCollection : List<KeyValuePair<string, string>>
    {
        public void Add(string key, string value)
        {
            this.Add(new KeyValuePair<string, string>(key, value));
        }

        public void Add(MessageCollection coln)
        {
            if (coln != null)
            {
                this.AddRange(coln);
            }
        }
    }
}
