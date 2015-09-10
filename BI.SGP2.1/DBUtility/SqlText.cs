using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace SGP.DBUtility
{

    public class SqlText
    {
        public SqlText()
        {

        }

        public static int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, null);
        }

        public static int ExecuteNonQuery(string cmdText, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteNonQuery(cmdText, sqlParams);
        }

        public static int ExecuteNonQuery(string cmdText, params SqlParameter[] sqlParams)
        {
            int retval = 0;
            TScope ts = TScope.Current;
            if (ts != null)
            {
                retval = ExecuteNonQuery((SqlConnection)ts.Transaction.Connection,
                    (SqlTransaction)ts.Transaction, cmdText, sqlParams);
            }
            else
            {
                using (SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString))
                {
                    retval = ExecuteNonQuery(cn, null, cmdText, sqlParams);
                }
            }

            return retval;
        }

        private static int ExecuteNonQuery(SqlConnection cn, SqlTransaction trans, string cmdText, params SqlParameter[] sqlParams)
        {
            int retval = 0;

            if (TScope.ContextHasErrors)
            {
                return retval;
            }

            try
            {
                SqlCommand cmd = new SqlCommand(cmdText, cn);
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = trans;

                if (sqlParams != null)
                {
                    AttachParameters(cmd, sqlParams);
                }
                if (cn.State != ConnectionState.Open)
                    cn.Open();
                retval = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Dispose();
            }
            catch (Exception err)
            {
                TScope.HandlError(err);
            }

            return retval;
        }

        public static object ExecuteScalar(string cmdText)
        {
            return ExecuteScalar(cmdText, null);
        }

        public static object ExecuteScalar(string cmdText, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteScalar(cmdText, sqlParams);
        }

        public static object ExecuteScalar(string cmdText, params SqlParameter[] sqlParams)
        {
            object retval = null;
            TScope ts = TScope.Current;
            if (ts != null)
            {
                retval = ExecuteScalar((SqlConnection)ts.Transaction.Connection,
                    (SqlTransaction)ts.Transaction, cmdText, sqlParams);
            }
            else
            {
                using (SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString))
                {
                    retval = ExecuteScalar(cn, null, cmdText, sqlParams);
                }
            }

            return retval;
        }

        public static object ExecuteScalar(SqlConnection cn, SqlTransaction trans, string cmdText, params SqlParameter[] sqlParams)
        {
            object retval = null;

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = trans;
            if (sqlParams != null)
            {
                AttachParameters(cmd, sqlParams);
            }
            if (cn.State != ConnectionState.Open)
                cn.Open();
            retval = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            cmd.Dispose();

            return retval;
        }

        public static DataSet ExecuteDataset(string cmdText)
        {
            return ExecuteDataset(cmdText, (string)null);
        }

        public static DataSet ExecuteDataset(string cmdText, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteDataset(cmdText, sqlParams);
        }

        public static DataSet ExecuteDataset(string cmdText, string tableName)
        {
            return ExecuteDataset(cmdText, tableName, null);
        }

        public static DataSet ExecuteDataset(string cmdText, params SqlParameter[] sqlParams)
        {
            return ExecuteDataset(cmdText, null, sqlParams);
        }

        public static DataSet ExecuteDataset(string cmdText, string tableName, params SqlParameter[] sqlParams)
        {
            DataSet data = null;
            TScope ts = TScope.Current;
            if (ts != null)
            {
                data = ExecuteDataset((SqlConnection)ts.Transaction.Connection,
                    (SqlTransaction)ts.Transaction, cmdText, tableName, sqlParams);
            }
            else
            {
                using (SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString))
                {
                    data = ExecuteDataset(cn, null, cmdText, tableName, sqlParams);
                }
            }

            return data;
        }
        public static DataSet ExecuteDataset(SqlConnection cn, SqlTransaction trans, string cmdText, string tableName, params SqlParameter[] sqlParams)
        {
            DataSet data = new DataSet();

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = trans;
            if (sqlParams != null)
            {
                AttachParameters(cmd, sqlParams);
            }
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            if (tableName != null && tableName != string.Empty)
            {
                adapter.Fill(data, tableName);
            }
            else
            {
                adapter.Fill(data);
            }
            adapter.Dispose();
            cmd.Parameters.Clear();
            cmd.Dispose();


            return data;
        }
        private static void AttachParameters(SqlCommand cmd, params SqlParameter[] sqlParams)
        {
            if (cmd == null || sqlParams == null || sqlParams.Length == 0)
            {
                return;
            }

            foreach (SqlParameter p in sqlParams)
            {
                cmd.Parameters.Add(p);
            }
        }
        public static void ExecuteDataset()
        {
            throw new NotImplementedException();
        }

        public static void ExecuteDataset(string p1, string p2, string p3, string p4, string p5)
        {
            throw new NotImplementedException();
        }
    }
}
