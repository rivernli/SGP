using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models.Detail
{
    public class CostingVersionDetail : CostingMasterDetailData
    {
        public CostingVersionDetail(FieldCategory category)
            : base(category)
        {

        }

        public CostingVersionDetail(FieldCategory category, Dictionary<string, object> data)
            : base(category, data)
        {
            
        }

        public override int Add()
        {
            string strSql = "";
            string strField = "";
            string strValue = "";
            List<SqlParameter> listParames = new List<SqlParameter>();

            foreach (FieldInfo tableFields in Category.Fields)
            {
                if (tableFields.CurrentlyInUse)
                {
                    strField += tableFields.FieldName + ",";
                    strValue += "@" + tableFields.FieldName + ",";
                    string fieldValue = String.Format("{0}", tableFields.DataValue);
                    listParames.Add(String.IsNullOrEmpty(fieldValue.Trim()) ? new SqlParameter("@" + tableFields.FieldName, DBNull.Value) : new SqlParameter("@" + tableFields.FieldName, fieldValue));
                }
            }

            strField = strField.TrimEnd(',');
            strValue = strValue.TrimEnd(',');
            strSql = String.Format("INSERT INTO " + TableName + "(" + strField + ") VALUES(" + strValue + ");SELECT @@IDENTITY");
            this.ID = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
            return this.ID;
        }

        public override int Update(int ID)
        {
            this.ID = ID;
            string strField = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            foreach (FieldInfo tableFields in Category.Fields)
            {
                if (tableFields.CurrentlyInUse)
                {
                    strField += tableFields.FieldName + "=@" + tableFields.FieldName + ",";
                    string fieldValue = String.Format("{0}", tableFields.DataValue);
                    listParames.Add(String.IsNullOrEmpty(fieldValue.Trim()) ? new SqlParameter("@" + tableFields.FieldName, DBNull.Value) : new SqlParameter("@" + tableFields.FieldName, fieldValue));
                }
            }
            strField = strField.TrimEnd(',');
            string tk = "ID";
            listParames.Add(new SqlParameter("@" + tk, this.ID));
            string strSql = "UPDATE " + TableName + " SET " + strField + " WHERE " + tk + " = @" + tk + ";";
            DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
            return this.ID;
        }

        public override void Delete(int ID)
        {
            DeleteVersion(ID);
            AddDeletedLog(ID, "SCCostVersion");
            string strSql = "DELETE FROM " + TableName + " WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
        }

        public static void CopyVersion(int versionID)
        {
            string strSql = "SELECT * FROM SCM_Version WHERE ID = @ID";
            DataTable dtVersion = DbHelperSQL.Query(strSql, new SqlParameter("@ID", versionID)).Tables[0];
            if (dtVersion.Rows.Count > 0)
            {
                string baseOn = Convert.ToString(dtVersion.Rows[0]["BaseOn"]);
                string version = Convert.ToString(dtVersion.Rows[0]["Version"]);
                if (String.IsNullOrWhiteSpace(baseOn))
                {
                    strSql = "SELECT Version FROM SCM_Version WHERE Status = 'Active'";
                    baseOn = DbHelperSQL.GetSingle<string>(strSql);
                }

                if (!String.IsNullOrWhiteSpace(baseOn))
                {
                    strSql = "SELECT * FROM SCS_TableParams WHERE TableType = 2";
                    DataTable dtTableParams = DbHelperSQL.Query(strSql).Tables[0];
                    foreach (DataRow drTP in dtTableParams.Rows)
                    {
                        FieldCategory fc = new FieldCategory(Convert.ToString(drTP["TableKey"]));
                        string strField = "";
                        foreach (FieldInfo f in fc.Fields)
                        {
                            if (f.Visible != 0 && f.FieldName != "Version")
                            {
                                strField += f.FieldName + ",";
                            }
                        }

                        strField = strField.TrimEnd(',');

                        strSql = String.Format("INSERT INTO {0}(Version,{1}) SELECT @Version,{1} FROM {0} WHERE Version = @BaseOn;", drTP["TableName"], strField);
                        DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@Version", version), new SqlParameter("@BaseOn", baseOn));
                    }
                }
            }
        }

        private void DeleteVersion(int versionID)
        {
            string strSql = "SELECT * FROM SCM_Version WHERE ID = @ID";
            DataTable dtVersion = DbHelperSQL.Query(strSql, new SqlParameter("@ID", versionID)).Tables[0];
            if (dtVersion.Rows.Count > 0)
            {
                string version = Convert.ToString(dtVersion.Rows[0]["Version"]);
                strSql = "SELECT * FROM SCS_TableParams WHERE TableType = 2";
                DataTable dtTableParams = DbHelperSQL.Query(strSql).Tables[0];
                foreach (DataRow drTP in dtTableParams.Rows)
                {
                    strSql = String.Format("DELETE FROM {0} WHERE Version = @Version", drTP["TableName"]);
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@Version", version));
                }
            }
        }
    }
}
