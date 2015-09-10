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

namespace BI.SGP.BLL.Models
{
   public class SupplierManager
    {
        public int? ID
        {
           get;
           set;
        }
        public string Commodity
        {
           get;
           set;
        }
                public string Category
        {
           get;
           set;
        }
                public string SupplyCode
        {
           get;
           set;
        }
                public string City
        {
           get;
           set;
        }
                public string SupplierName
        {
           get;
           set;
        }
        public string SupplierAddress
        {
            get;
            set;
        }
        public string SupplierCity
        {
            get;
            set;
        }
        public string SalesManager
        {
            get;
            set;
        }
        public string SalesManagerPhone
        {
            get;
            set;
        }
        public string SalesManagerMail
        {
            get;
            set;
        }
        public string VPKeyContactPerson
        {
            get;
            set;
        }

        public string VPKeyContactPersonPhone
        {
            get;
            set;
        }
        public string VPKeyContactPersonMail
        {
            get;
            set;
        }
        public string ManufactoryName
        {
            get;
            set;
        }
        public string KeyContactPerson
        {
            get;
            set;
        }
        public string KeyContactPersonPhone
        {
            get;
            set;
        }
        public string KeyContactPersonMail
        {
            get;
            set;
        }
        public string OneTime
        {
            get;
            set;
        }
        public string PayMethod
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public string Currency
        {
            get;
            set;
        }
        public string PaymentTerm
        {
            get;
            set;
        }
        public string DeliveryTerm
        {
            get;
            set;
        }

        public string Spending
        {
            get;
            set;
        }
        public double XPlan
        {
            get;
            set;
        }

        public static List<SupplierManager> GetDetail(string ID)
        {
            string strSql = "SELECT * FROM V_Supplier WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];

            List<SupplierManager> SupplierData = new List<SupplierManager>();

            foreach (DataRow dr in dt.Rows)
            {
                SupplierManager Detail = new SupplierManager();
                
                //for (int i = 0; i < dr.Table.Columns.Count; i++)
                // {
                FillData(Detail, dr);
                //}
                SupplierData.Add(Detail);

            }

            return SupplierData;
        }

        private static void FillData(SupplierManager SupplierData, DataRow dr)
        {
            Type tp = typeof(SupplierManager);
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string propertyName = dr.Table.Columns[i].ColumnName;
                PropertyInfo propertyinfo = tp.GetProperty(propertyName);
                if (propertyinfo != null && dr[i] != DBNull.Value)
                {
                    propertyinfo.SetValue(SupplierData, dr[i], null);
                }
            }
        }

        public static DataTable GetSupplierData(string id)
        {
            string strSql = "SELECT * FROM V_Supplier WHERE StatusID = 1 AND ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", id)).Tables[0];
            return dt;
        }

        //删除 把字段DeleteState标为1
        public static void DeleteSupplierData(string id)
        {
            string strSql = "UPDATE SYS_Supplier SET StatusID = 0 WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }


        public string NullToEmpty(string s)
        {
            return s == null ? "" : s.Trim();
        }

        public object ConvertValue(object strValue)
        {
            if (strValue != null || Convert.ToString(strValue).Trim() != String.Empty)
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
        /// 保存数据，返回结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueCollection Save(HttpRequestBase request)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            NameValueCollection ExecResult = new NameValueCollection();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_Supplier);

            //将数据从FORM中读取到FIELDS
            foreach (string key in request.Form.Keys)
            {
                BLL.DataModels.FieldInfo field = fields[key];
                if (field != null)
                {
                    string v = request.Form[key];
                    if (string.IsNullOrWhiteSpace(v) == false) v = HttpUtility.UrlDecode(v);
                    field.DataValue = v;
                }
                if (field != null && !String.IsNullOrEmpty(field.TableName))
                {
                    string tableName = field.TableName.ToUpper();
                    if (dicAllTable.ContainsKey(tableName))
                    {
                        dicAllTable[tableName].Add(key, request.Form[key]);
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(key, request.Form[key]);
                        dicAllTable.Add(tableName, dic);
                    }
                }
            }

            int supplierId = ConvertToInt32(request.Form["ID"]);
            try
            {
                if (supplierId > 0)
                {
                    //Update
                    UpdateOperation(dicAllTable, supplierId);
                }
                else
                {
                    //Insert
                    InsertOperation(ref supplierId, dicAllTable);
                }

                ExecResult.Add("ID", supplierId.ToString());
            }
            catch (Exception ex)
            {
                ExecResult.Add("ID", supplierId.ToString());
                ExecResult.Add("Mgs", ex.Message);
            }

            return ExecResult;
        }


        /// <summary>
        /// 动态构造Insert Sql语句 
        /// </summary>
        public static void InsertOperation(ref int supplierId, Dictionary<string, Dictionary<string, string>> dicAllTable)
        {
            string FileName = "";
            string FileValue = "";
            SqlParameter[] paras = null;
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

                    paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                    i++;
                }
                //FileName += " ,[Creater],[CreationDate] ";
                //FileValue += " ,@Creater,@CreationDate ";
                //paraList.Add(new SqlParameter("@Creater", "" + AccessControl.CurrentLogonUser.Uid + ""));
                //paraList.Add(new SqlParameter("@CreationDate", "" + System.DateTime.Now.ToString() + ""));
                paras = paraList.ToArray();//将List对象转换为数组
                string sql = "INSERT INTO " + tableName + " (" + FileName + ") VALUES (" + FileValue + ");SELECT @@IDENTITY";
                supplierId = DbHelperSQL.GetSingle<int>(sql, paras.ToArray());
            }

        }

        /// <summary>
        /// 动态构造Update Sql语句 
        /// </summary>
        public static void UpdateOperation(Dictionary<string, Dictionary<string, string>> dicAllTable, int supplierId)
        {
            string FileValue = "";
            SqlParameter[] paras = null;
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

                    paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                    i++;
                }
                //FileValue += " ,Modifier=@Modifier, ModificationDate=@ModificationDate ";
                //paraList.Add(new SqlParameter("@Modifier", "" + AccessControl.CurrentLogonUser.Uid + ""));
                //paraList.Add(new SqlParameter("@ModificationDate", "" + System.DateTime.Now.ToString() + ""));
                paraList.Add(new SqlParameter("@ID", supplierId));
                paras = paraList.ToArray();//将List对象转换为数组
                string sql = "UPDATE " + tableName + " SET " + FileValue + " WHERE ID=@ID ";
                DbHelperSQL.GetSingle<int>(sql, paras.ToArray());
            }
        }



    }
}
