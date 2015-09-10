using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SGP.DBUtility
{
    public class DBUtility
    {
        private static string _dbConnectionString;
        public static string DBConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(_dbConnectionString))
                {
                    _dbConnectionString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
                }

                return _dbConnectionString;
            }
        }
    }
}
