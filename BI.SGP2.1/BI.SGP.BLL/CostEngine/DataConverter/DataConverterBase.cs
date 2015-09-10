using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.CostEngine.DataConverter
{
    public enum InputDataType
    {
        Json,
        Xml,
        DataRow
    }

    public abstract class DataConverterBase
    {
        public abstract Dictionary<string, object> ReadData(object value);
    }

    public class ConverterFactory
    {
        public static DataConverterBase GetInstance(InputDataType dataType)
        {
            switch (dataType)
            {
                case InputDataType.Json:
                    return new JsonDataConverter();
                case InputDataType.DataRow:
                    return new DataRowDataConverter();
            }
            throw new Exception("Unknow Data Type.");
        } 
    }
}
