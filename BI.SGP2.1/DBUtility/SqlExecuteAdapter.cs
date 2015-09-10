using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace SGP.DBUtility
{
    public class DbHelperSQL
    {
        public static int ExecuteSql(string cmdText)
        {
            return SqlText.ExecuteNonQuery(cmdText);
        }

        public static int ExecuteSql(string cmdText, params SqlParameter[] cmdParms)
        {
            return SqlText.ExecuteNonQuery(cmdText, cmdParms);
        }

        public static DataSet Query(string cmdText)
        {
            return SqlText.ExecuteDataset(cmdText);
        }

        public static DataSet Query(string cmdText, params SqlParameter[] cmdParms)
        {
            return SqlText.ExecuteDataset(cmdText, cmdParms);
        }

        public static object GetSingle(string cmdText)
        {
            return SqlText.ExecuteScalar(cmdText);
        }

        public static object GetSingle(string cmdText, params SqlParameter[] cmdParms)
        {
            return SqlText.ExecuteScalar(cmdText, cmdParms);
        }

        public static T GetSingle<T>(string cmdText)
        {
            return GetSingle<T>(cmdText, null);
        }

        public static T GetSingle<T>(string cmdText, params SqlParameter[] cmdParms)
        {
            object obj = GetSingle(cmdText, cmdParms);
            string objStr = string.Format("{0}", obj);

            if (typeof(T) == typeof(string))
            {
                return (T)(object)objStr;
            }
            if (typeof(T) == typeof(int))
            {
                if (obj == null) return (T)(object)0;
                int k = 0;
                Int32.TryParse(objStr, out k);
                return (T)(object)k;
            }
            if (typeof(T) == typeof(DateTime))
            {
                if (obj == null) return (T)(object)DateTime.MinValue;
                DateTime dt;
                DateTime.TryParse(objStr, out dt);
                return (T)(object)dt;
            }

            return default(T);
        }

        public static bool Exists(string cmdText)
        {
            return Exists(cmdText, null);
        }

        public static bool Exists(string cmdText, params SqlParameter[] cmdParms)
        {
            return GetSingle<int>(cmdText, cmdParms) > 0;
        }
    }
}