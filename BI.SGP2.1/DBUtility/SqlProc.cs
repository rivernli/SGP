using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SGP.DBUtility
{
    public class SqlProc
    {
        private SqlProc() { }

        public static int ExecuteNonQuery(string cmdProc)
        {
            return ExecuteNonQuery(cmdProc, null);
        }

        public static int ExecuteNonQuery(string cmdProc, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteNonQuery(cmdProc, sqlParams);
        }

        public static int ExecuteNonQuery(string cmdProc, params SqlParameter[] sqlParams)
        {
            int retval = 0;

            TScope ts = TScope.Current;
            if (ts != null)
            {
                retval = ExecuteNonQuery((SqlConnection)ts.Transaction.Connection, (SqlTransaction)ts.Transaction, cmdProc, sqlParams);
            }
            else
            {
                using (SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString))
                {
                    retval = ExecuteNonQuery(cn, null, cmdProc, sqlParams);
                }
            }

            return retval;
        }

        private static int ExecuteNonQuery(SqlConnection cn, SqlTransaction trans, string cmdProc, params SqlParameter[] sqlParams)
        {
            int retval = 0;

            if (TScope.ContextHasErrors)
            {
                return retval;
            }

            try
            {
                SqlCommand cmd = new SqlCommand(cmdProc, cn);
                cmd.CommandType = CommandType.StoredProcedure;
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

        public static object ExecuteScalar(string cmdProc)
        {
            return ExecuteScalar(cmdProc, null);
        }

        public static object ExecuteScalar(string cmdProc, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteScalar(cmdProc, sqlParams);
        }

        public static object ExecuteScalar(string cmdProc, params SqlParameter[] sqlParams)
        {
            object retval = null;

            TScope ts = TScope.Current;
            if (ts != null)
            {
                retval = ExecuteScalar((SqlConnection)ts.Transaction.Connection, (SqlTransaction)ts.Transaction, cmdProc, sqlParams);
            }
            else
            {
                using (SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString))
                {
                    retval = ExecuteScalar(cn, null, cmdProc, sqlParams);
                }
            }

            return retval;
        }

        private static object ExecuteScalar(SqlConnection cn, SqlTransaction trans, string cmdProc, params SqlParameter[] sqlParams)
        {
            object retval = null;

            if (TScope.ContextHasErrors)
            {
                return retval;
            }

            try
            {
                SqlCommand cmd = new SqlCommand(cmdProc, cn);
                cmd.CommandType = CommandType.StoredProcedure;
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
            }
            catch (Exception err)
            {
                TScope.HandlError(err);
            }

            return retval;
        }

        public static DataSet ExecuteDataset(string cmdProc)
        {
            return ExecuteDataset(cmdProc, (string)null);
        }

        public static DataSet ExecuteDataset(string cmdProc, string paramName, object paramValue)
        {
            SqlParameter[] sqlParams = { new SqlParameter(paramName, paramValue) };

            return ExecuteDataset(cmdProc, sqlParams);
        }

        public static DataSet ExecuteDataset(string cmdProc, string tableName)
        {
            return ExecuteDataset(cmdProc, tableName, null);
        }

        public static DataSet ExecuteDataset(string cmdProc, params SqlParameter[] sqlParams)
        {
            return ExecuteDataset(cmdProc, null, sqlParams);
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

            try
            {
                SqlCommand cmd = new SqlCommand(cmdText, cn);
                cmd.CommandType = CommandType.StoredProcedure;
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
            }
            catch (Exception err)
            {
                TScope.HandlError(err);
            }

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
    }
}

