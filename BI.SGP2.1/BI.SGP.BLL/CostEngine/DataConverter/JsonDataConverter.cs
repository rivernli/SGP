using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.CostEngine.DataConverter
{
    public class JsonDataConverter : DataConverterBase
    {
        public override Dictionary<string, object> ReadData(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(Convert.ToString(value), typeof(Dictionary<string, object>)) as Dictionary<string, object>;
        }
    }
}
