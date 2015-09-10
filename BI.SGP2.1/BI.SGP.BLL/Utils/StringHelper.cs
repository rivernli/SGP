using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BI.SGP.BLL.WF;

namespace BI.SGP.BLL.Utils
{
    public class StringHelper
    {
        public static string[] DateTimeSplit(string dateValue)
        {
            if (dateValue != null)
            {
                string[] values = dateValue.Split('-');
                if (values.Length == 2)
                {
                    DateTime dt;
                    string startDate = values[0].Trim();
                    string endDate = values[1].Trim();
                    if (DateTime.TryParse(startDate, out dt) && DateTime.TryParse(endDate, out dt))
                    {
                        return new string[] { startDate + " 00:00:00", endDate + " 23:59:59" };
                    }
                }
            }
            return null;
        }

        public static string WFActivityToJSON(WFActivity activity)
        {
            return "{\"id\":" + activity.ID + ",\"name\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activity.Name) + ", \"sort\":" + activity.Sort + ",\"activityType\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activity.ActivityType) + ",\"activityDeac\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activity.Description) + ",\"childCount\":" + activity.CurrentChildActivities.Count + "}";
        }

        public static string SelectReplace(string oldStr)
        {
            if (string.IsNullOrEmpty(oldStr))
            {
                return "";
            }
            string str2 = Regex.Replace(oldStr, @"[\[\+\\\|\(\)\^\*\""\]'%~#-&]", delegate(Match match)
            {
                if (match.Value == "'")
                {
                    return "''";
                }
                else
                {
                    return "[" + match.Value + "]";
                }
            });
            return str2;
        }
    }
}
