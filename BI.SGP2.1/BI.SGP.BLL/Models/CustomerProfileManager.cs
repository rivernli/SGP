using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using System.Web;
using System.Reflection;
using BI.SGP.BLL.DataModels;
using System.Transactions;
using System.Collections;
using System.Globalization;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;

namespace BI.SGP.BLL.Models
{
    public class CustomerProfileManager
    {       

        public static DataTable GetCustomerData(string id)
        {
            string strSql = "SELECT * FROM SGP_CustomerProfile_Data WHERE DeleteState = 0 AND ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", id)).Tables[0];
            return dt;
        }

        //删除 把字段DeleteState标为1
        public static void DeleteCustomerData(string id)
        {
            string strSql = "UPDATE SGP_CustomerProfile_Data SET DeleteState = 1 WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));

            Addlog(id);
        }
        
        //记录日志
        public static void Addlog(string id)
        {
            string strSql = "INSERT INTO SGP_CustomerProfile_Data_History SELECT * FROM SGP_CustomerProfile_Data WHERE ID=@ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        public static object ConvertValue(object strValue)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(strValue)))
            {
                return strValue;
            }
            return DBNull.Value;
        }

        public static int ConvertToInt32(object Value)
        {
            if (Value == null || Convert.ToString(Value).Trim() == String.Empty || Value == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(Value);
            }
        }
        
        /// <summary>
        /// 合计People人数
        /// </summary>
        public static int GetPeopleTotal(string id)
        {
            string strSql = "select count(id) from SGP_CustomerPeople where CustomerId='" + id + "' ";
            int PeopleTotal = DbHelperSQL.GetSingle<int>(strSql);            
            return PeopleTotal;
        }

        /// <summary>
        /// 未查看过的总数
        /// </summary>
        public static int GetNewTotal(string id)
        {
            List<SqlParameter> ps = new List<SqlParameter>()
                {
                    new SqlParameter("@CustomerId", id),
                    new SqlParameter("@Uid", AccessControl.CurrentLogonUser.Uid)
                };

            string strSql = "select count(id) from SGP_CustomerNews WHERE CustomerId=@CustomerId AND ID NOT IN (select NewsId from SGP_CustomerNews_Vews where Uid=@Uid)";
            int PeopleTotal = DbHelperSQL.GetSingle<int>(strSql,ps.ToArray());
            return PeopleTotal;
        }

      
        /// <summary>
        /// check customer name only
        /// </summary>
        public static void CheckNameOnly(string id, object value, SystemMessages sysMsg)
        {
            List<SqlParameter> ps = new List<SqlParameter>()
                {
                    new SqlParameter("@CustomerId", id),
                    new SqlParameter("@CustomerName", Convert.ToString(value))
                };

            string strSql = "SELECT COUNT(ID) FROM SGP_CustomerProfile_Data WHERE DeleteState = 0 AND Customer=@CustomerName AND ID<>@CustomerId ";
            if (DbHelperSQL.GetSingle<int>(strSql, ps.ToArray())>0)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add("Customer", String.Format("[{0}] already exists,please try again", value));
            }
        }

        /// <summary>
        /// 检查Customer Name, 已有的名称Update, 没有Insert
        /// </summary>
        public static bool CheckNameOnly(string customer)
        {
            bool checkCustomerName = true;
            List<SqlParameter> ps = new List<SqlParameter>()
                {
                    new SqlParameter("@customer", customer)
                };

            string strSql = "SELECT COUNT(ID) FROM SGP_CustomerProfile_Data WHERE DeleteState = 0 AND Customer=@customer ";
            if (DbHelperSQL.GetSingle<int>(strSql, ps.ToArray()) > 0)
            {
                checkCustomerName = false; ;
            }
            return checkCustomerName;
        }

        public static bool SaveforCustomerProfileExcel(DataRow dr, out string msg)
        {
            msg = "";
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_CUSTOMERPROFILE);

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string columnName = dr.Table.Columns[i].ColumnName;
                BI.SGP.BLL.DataModels.FieldInfo fi = fields.Find(t => string.Compare(t.FieldName, columnName, true) == 0);
                fi.DataValue = dr[i];

                if (fi != null && !String.IsNullOrEmpty(fi.TableName))
                {
                    string tableName = fi.TableName.ToUpper();
                    if (dicAllTable.ContainsKey(tableName))
                    {
                        dicAllTable[tableName].Add(columnName, Convert.ToString(dr[i]));
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(columnName, Convert.ToString(dr[i]));
                        dicAllTable.Add(tableName, dic);
                    }
                }
            }
            using (TScope ts = new TScope())
            {
                try
                {
                    if (CheckNameOnly(Convert.ToString(dr["Customer"])))
                    {
                        InsertOperation(dicAllTable);
                    }
                    else
                    {
                        UpdateOperation(Convert.ToString(dr["Customer"]), dicAllTable);
                    }
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    msg = string.Format("Error:" + ex.Message);
                    return false;
                }
            }
            msg = "Uploaded successfully";
            return true;
        } 

        /// <summary>
        /// Insert Sql语句 
        /// </summary>
        public static void InsertOperation(Dictionary<string, Dictionary<string, string>> dicAllTable)
        {
            string FileName = "";
            string FileValue = "";
            List<SqlParameter> paraList = new List<SqlParameter>();
            int i = 0;
            foreach (string tableName in dicAllTable.Keys)
            {
                foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
                {
                    if (FileName != string.Empty)
                    {
                        FileName += ", ";
                    }
                    if (FileValue != string.Empty)
                    {
                        FileValue += ", ";
                    }
                    FileName += "[" + kvField.Key + "] ";
                    FileValue += "@" + kvField.Key + " ";

                    paraList.Add(new SqlParameter("@" + kvField.Key + "", ConvertValue(kvField.Value)));
                    i++;
                }
                FileName += " ,[Creater] ";
                FileValue += " ,@Creater ";
                paraList.Add(new SqlParameter("@Creater", "" + AccessControl.CurrentLogonUser.Uid + ""));

                string sql = "INSERT INTO " + tableName + " (" + FileName + ") VALUES (" + FileValue + ");SELECT @@IDENTITY";
                int customerId = DbHelperSQL.GetSingle<int>(sql, paraList.ToArray());
                //日志记录
                Addlog(Convert.ToString(customerId));
            }

        }

        /// <summary>
        /// Update Sql语句 
        /// </summary>
        public static void UpdateOperation(string customer, Dictionary<string, Dictionary<string, string>> dicAllTable)
        {
            string FileValue = "";
            List<SqlParameter> paraList = new List<SqlParameter>();
            int i = 0;
            foreach (string tableName in dicAllTable.Keys)
            {
                foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
                {
                    if (FileValue != string.Empty)
                    {
                        FileValue += ", ";
                    }
                    FileValue += kvField.Key + "=@" + kvField.Key + " ";

                    paraList.Add(new SqlParameter("@" + kvField.Key + "", ConvertValue(kvField.Value)));
                    i++;
                }
                FileValue += " ,Modifier=@Modifier, ModificationDate=@ModificationDate ";
                paraList.Add(new SqlParameter("@Modifier", "" + AccessControl.CurrentLogonUser.Uid + ""));
                paraList.Add(new SqlParameter("@ModificationDate", "" + System.DateTime.Now.ToString() + ""));
                string sql = "UPDATE " + tableName + " SET " + FileValue + " WHERE customer=@customer ";
                DbHelperSQL.GetSingle<int>(sql, paraList.ToArray());
                //日志记录
                string strSql = "INSERT INTO SGP_CustomerProfile_Data_History SELECT * FROM SGP_CustomerProfile_Data WHERE DeleteState = 0 AND customer=@customer";
                DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@customer", customer));
            }
        }
    }
}
