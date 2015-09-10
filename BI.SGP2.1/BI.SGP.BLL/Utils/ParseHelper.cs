using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.Utils
{
    public class ParseHelper
    {
        public static T Parse<T>(object value)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)value;
            }
            else if (typeof(T) == typeof(int))
            {
                if (value == null) return (T)(object)0;
                int rv = 0;
                Int32.TryParse(Convert.ToString(value), out rv);
                return (T)(object)rv;
            }
            else if (typeof(T) == typeof(double))
            {
                if (value == null) return (T)(object)0.0;
                double rv = 0;
                Double.TryParse(Convert.ToString(value), out rv);
                return (T)(object)rv;
            }
            else if (typeof(T) == typeof(DateTime))
            {
                if (value == null) return (T)(object)DateTime.MinValue;
                DateTime dt;
                DateTime.TryParse(Convert.ToString(value), out dt);
                return (T)(object)dt;
            }

            return default(T);

            if(value == null || value == DBNull.Value) return default(T);
        }
    }
}
