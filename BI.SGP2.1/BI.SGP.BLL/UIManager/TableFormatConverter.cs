using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BI.SGP.BLL.UIManager
{
    public class TableFormatConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TableFormat table = (TableFormat)value;
            writer.WriteStartArray();

            foreach (DataRow row in table.DataTable.Rows)
            {
                writer.WriteStartObject();
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (row[column] != null && row[column] != DBNull.Value)
                    {
                        bool hasWrite = false;
                        foreach (TableFormatString tfs in table.formatStrings)
                        {
                            if (tfs.ColumnName == column.ColumnName)
                            {
                                writer.WritePropertyName(tfs.NewColumnName);
                                serializer.Serialize(writer, String.Format(tfs.DataFormatString, row[column]));
                                if (!tfs.IsCreateNew)
                                {
                                    hasWrite = true;
                                }
                            }
                        }

                        if (!hasWrite)
                        {
                            writer.WritePropertyName(column.ColumnName);
                            serializer.Serialize(writer, row[column]);
                        }
                    }
                }
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type valueType)
        {
            return (valueType == typeof(TableFormat));
        }
    }
}
