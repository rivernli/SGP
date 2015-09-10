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
    public class CostingOtherDetailData : CostingMasterDetailData
    {
        public CostingOtherDetailData(FieldCategory category)
            : base(category)
        {

        }

        public CostingOtherDetailData(FieldCategory category, Dictionary<string, object> data)
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
            string strSql = "SELECT * FROM " + TableName + " WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.BeforeData = dt.Rows[0];
            }

            if (HasChanged())
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
                strSql = "UPDATE " + TableName + " SET " + strField + " WHERE " + tk + " = @" + tk + ";";
                DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                AddChangedLog();
            }

            return this.ID;
        }

        public override void Delete(int ID)
        {
            AddDeletedLog(ID, Category.CategoryName);
            string strSql = "DELETE FROM " + TableName + " WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
        }
    }
}
