using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using SGP.DBUtility;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.Models.Detail
{
    public class CostingMasterDetailData
    {
        public int ID { get; set; }
        protected string _tableName;
        protected string ChangedContent { get; set; }
        public FieldCategory Category { get; set; }
        public TableParams Params { get; set; }
        public DataRow BeforeData { get; set; }

        public CostingMasterDetailData(FieldCategory category)
        {
            this.Category = category;
            this.Params = new TableParams(category.CategoryName);
        }

        public CostingMasterDetailData(FieldCategory category, Dictionary<string, object> data)
        {
            this.Category = category;
            this.Params = new TableParams(category.CategoryName);
            foreach (KeyValuePair<string, object> kv in data)
            {
                FieldInfo f = category.Fields[kv.Key];
                if (f != null)
                {
                    f.DataValue = kv.Value;
                }
            }
        }

        public String TableName
        {
            get
            {
                if (_tableName == null)
                {
                    if (Params != null && !String.IsNullOrEmpty(Params.TableName))
                    {
                        _tableName = Params.TableName;
                    }
                    else
                    {
                        foreach (FieldInfo fi in Category.Fields)
                        {
                            if (!String.IsNullOrEmpty(fi.TableName))
                            {
                                _tableName = fi.TableName.ToUpper();
                                break;
                            }
                        }
                    }
                    
                }
                return _tableName;
            }
        }

        public virtual int Update(int ID)
        {
            return Update(ID, false);
        }

        public virtual int Update(int ID, bool checkExistsData)
        {
            if (checkExistsData && CheckExistsData(ID) > 0)
            {
                string errMessage = "Unique Exception, ";
                foreach (string uk in this.Params.UniqueKey)
                {
                    FieldInfo f = Category.Fields[uk];
                    errMessage += String.Format("{0}:\"{1}\"", f.DisplayName, f.DataValue);
                }
                throw new Exception(errMessage);
            }
            string strSql = "SELECT * FROM " + TableName + " WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.BeforeData = dt.Rows[0];
            }

            if (HasChanged())
            {
                if (Params.TableType == TableParams.TableType_PriceMaster)
                {
                    strSql = "UPDATE " + TableName + " SET ExpiryDate = GETDATE() WHERE ID = @ID";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
                    this.ID = Add();
                }
                else
                {
                    string strField = "";
                    List<SqlParameter> listParames = new List<SqlParameter>();
                    foreach (FieldInfo tableFields in Category.Fields)
                    {
                        if (tableFields.CurrentlyInUse)
                        {
                            strField += tableFields.FieldName + "=@" + tableFields.FieldName + ",";
                            string fieldValue = String.Format("{0}", tableFields.DataValue);
                            listParames.Add(new SqlParameter("@" + tableFields.FieldName, GetFieldValue(tableFields)));
                        }
                    }
                    strField = strField.TrimEnd(',');
                    listParames.Add(new SqlParameter("@ID", ID));
                    strSql = "UPDATE " + TableName + " SET " + strField + " WHERE ID=@ID;";
                    DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                    this.ID = ID;
                    AddChangedLog();
                }
            }

            return this.ID;
        }

        public virtual void UpdateEffectiveDateAndExpiryDate(int ID)
        {
            DateTime expiryDate = DateTime.Now;
            string strSql = "UPDATE " + TableName + " SET EffectiveDate = GETDATE(),ExpiryDate = '9999-12-31' WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
        }

        public int CheckExistsData(int ID) 
        {
            if (Params != null && Params.UniqueKey != null && Params.UniqueKey.Count > 0)
            {
                string strSql = String.Format("SELECT ID FROM {0} WHERE 1=1", TableName);
                if (Params.TableType == TableParams.TableType_PriceMaster)
                {
                    strSql += " AND ExpiryDate>GETDATE()";
                }
                if (ID > 0)
                {
                    strSql += String.Format(" AND ID <> {0}", ID);
                }
                List<SqlParameter> listParames = new List<SqlParameter>();
                foreach (string uk in Params.UniqueKey)
                {
                    strSql += String.Format(" AND {0}=@{0}", uk);
                    listParames.Add(new SqlParameter("@" + uk, Category.Fields[uk].DataValue));
                }

                return DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
            }
            return 0;
        }

        public virtual int Add()
        {
            int existsID = CheckExistsData(0);
            if (existsID > 0)
            {
                return Update(existsID);
            }
            else
            {
                string strSql = "";
                string strField = "CreatorName,";
                string strValue = "@CreatorName,";
                List<SqlParameter> listParames = new List<SqlParameter>();
                listParames.Add(new SqlParameter("@CreatorName", AccessControl.CurrentLogonUser.Name));

                if (Params.TableType == TableParams.TableType_PriceMaster && Params.TableKey != "SCExchangeRate")
                {
                    double exchangeRate = GetExchangeRate();
                    if (exchangeRate == 0)
                    {
                        throw new Exception("can not find exchange rate.");
                    }
                    else
                    {
                        strField += "USDPrice,";
                        strValue += "@USDPrice,";
                        listParames.Add(new SqlParameter("@USDPrice", Math.Round(ParseHelper.Parse<double>(Category.Fields["Price"].DataValue) / exchangeRate, 10)));
                    }
                }

                foreach (FieldInfo tableFields in Category.Fields)
                {
                    if (tableFields.CurrentlyInUse)
                    {
                        strField += tableFields.FieldName + ",";
                        strValue += "@" + tableFields.FieldName + ",";
                        listParames.Add(new SqlParameter("@" + tableFields.FieldName, GetFieldValue(tableFields)));
                    }
                }

                strField = strField.TrimEnd(',');
                strValue = strValue.TrimEnd(',');
                strSql = String.Format("INSERT INTO " + TableName + "(" + strField + ") VALUES(" + strValue + ");SELECT @@IDENTITY");
                this.ID = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());

                if (Params.TableType == TableParams.TableType_PriceMaster)
                {
                    UpdateEffectiveDateAndExpiryDate(this.ID);
                }

                return this.ID;
            }
        }

        private object GetFieldValue(FieldInfo field)
        {
            string fieldValue = String.Format("{0}", field.DataValue);
            object pv = null;
            if (String.IsNullOrEmpty(fieldValue.Trim()))
            {
                if (IsNumber(field))
                {
                    pv = ParseHelper.Parse<double>(fieldValue);
                }
                else
                {
                    pv = DBNull.Value;
                }
            }
            else
            {
                if (field.DataType == FieldInfo.DATATYPE_PERCENT)
                {
                    pv = ParseHelper.Parse<double>(fieldValue.Replace("%", "")) / 100;
                }
                else
                {
                    pv = fieldValue;
                }
            }
            return pv;
        }

        public virtual void Delete(int ID)
        {
            if (Params.TableType == TableParams.TableType_PriceMaster)
            {
                string strSql = "UPDATE " + TableName + " SET ExpiryDate = GETDATE() WHERE ID = @ID";
                DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
            }
            else
            {
                AddDeletedLog(ID);
                string strSql = "DELETE FROM " + TableName + " WHERE ID = @ID";
                DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", ID));
            }
        }

        public virtual void BatchDelete()
        {
            string strSql = "DELETE FROM " + TableName;
            DbHelperSQL.ExecuteSql(strSql);
        }

        public void FillCategoryData(int dataId)
        {
            string strSql = "SELECT * FROM " + TableName + " WHERE ID = @ID";
            DataTable dtData = DbHelperSQL.Query(strSql, new SqlParameter("@ID", dataId)).Tables[0];
            foreach (FieldInfo fi in Category.Fields)
            {
                if (dtData.Rows.Count > 0 && dtData.Columns.Contains(fi.FieldName))
                {
                    if (fi.DataType == FieldInfo.DATATYPE_PERCENT)
                    {
                        string val = Convert.ToString(dtData.Rows[0][fi.FieldName]);
                        fi.DataValue = val == "" ? "" : Convert.ToString(ParseHelper.Parse<double>(val) * 100);
                    }
                    else
                    {
                        fi.DataValue = dtData.Rows[0][fi.FieldName];
                    }
                }
            }
        }

        public void CheckData(SystemMessages sysMsg)
        {
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.Required)
                {
                    if (FieldIsEmpty(field))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName, String.Format("\"{0}\" is required.", field.DisplayName));
                    }
                }
            }
        }

        protected bool HasChanged()
        {
            ChangedContent = "";
            if (BeforeData != null)
            {
                foreach (FieldInfo tableFields in Category.Fields)
                {
                    if (tableFields.CurrentlyInUse && BeforeData.Table.Columns.Contains(tableFields.FieldName) && !FieldDataQquals(tableFields, BeforeData[tableFields.FieldName]))
                    {
                        ChangedContent += String.Format("{0}:{1}>>{2};", tableFields.DisplayName, BeforeData[tableFields.FieldName], tableFields.DataValue);
                    }
                }
            }

            return ChangedContent != "";
        }

        protected void AddChangedLog()
        {
            string tableKey = String.IsNullOrEmpty(Params.TableKey) ? Category.CategoryName : Params.TableKey;
            string strSql = "INSERT INTO SCS_DataLog(Action,DataID,TableKey,Description,UpdateBy) VALUES('Update',@DataID,@TableKey,@Description,@UpdateBy)";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@DataID", this.ID), new SqlParameter("@TableKey", tableKey), new SqlParameter("@Description", ChangedContent), new SqlParameter("@UpdateBy", AccessControl.CurrentLogonUser.Name));
        }

        protected void AddDeletedLog(int ID, string tableKey)
        {
            string deletedContent = "";
            string strSql = "SELECT * FROM " + TableName + " WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (FieldInfo field in Category.Fields)
                {
                    if (dt.Columns.Contains(field.FieldName))
                    {
                        deletedContent += String.Format("{0}:{1};", field.DisplayName, dt.Rows[0][field.FieldName]);
                    }
                }
            }

            strSql = "INSERT INTO SCS_DataLog(Action,DataID,TableKey,Description,UpdateBy) VALUES('Delete',@DataID,@TableKey,@Description,@UpdateBy)";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@DataID", ID), new SqlParameter("@TableKey", tableKey), new SqlParameter("@Description", deletedContent), new SqlParameter("@UpdateBy", AccessControl.CurrentLogonUser.Name));
        }

        protected void AddDeletedLog(int ID)
        {
            AddDeletedLog(ID, Params.TableKey);
        }

        public bool FieldDataQquals(FieldInfo field, object data)
        {
            switch (field.DataType)
            {
                case FieldInfo.DATATYPE_INT:
                case FieldInfo.DATATYPE_DOUBLE:
                case FieldInfo.DATATYPE_FLOAT:
                    return ParseHelper.Parse<double>(field.DataValue) == ParseHelper.Parse<double>(data);
                default:
                    return Convert.ToString(field.DataValue).Trim() == Convert.ToString(data).Trim();
            }
        }

        public bool IsNumber(FieldInfo field)
        {
            switch (field.DataType)
            {
                case FieldInfo.DATATYPE_INT:
                case FieldInfo.DATATYPE_DOUBLE:
                case FieldInfo.DATATYPE_FLOAT:
                case FieldInfo.DATATYPE_PERCENT:
                    return true;
            }
            return false;
        }

        public bool FieldIsEmpty(FieldInfo field)
        {
            string fieldValue = String.Format("{0}", field.DataValue);
            if (String.IsNullOrWhiteSpace(fieldValue))
            {
                return true;
            }
            else
            {
                switch ((field.DataType))
                {
                    case FieldInfo.DATATYPE_DOUBLE:
                    case FieldInfo.DATATYPE_FLOAT:
                    case FieldInfo.DATATYPE_INT:
                    case FieldInfo.DATATYPE_PERCENT:
                        double dValue = 0;
                        double.TryParse(fieldValue, out dValue);
                        if (dValue == 0)
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        protected double GetExchangeRate()
        {
            FieldInfo cf = Category.Fields["Currency"];
            if (Convert.ToString(cf.DataValue).ToUpper() == "USD")
            {
                return 1;
            }
            else
            {
                FieldInfo pf = Category.Fields["Period"];
                DataTable dt = (DataTable)HttpRuntime.Cache.Get("ExchangeRate_" + pf.DataValue);
                if (dt == null)
                {
                    dt = DbHelperSQL.Query("SELECT Currency,Rate FROM SCP_ExchangeRate WHERE Period = @Period AND ExpiryDate > GETDATE()", new SqlParameter("@Period", pf.DataValue)).Tables[0];
                    HttpRuntime.Cache.Insert("ExchangeRate_" + pf.DataValue, dt, null, DateTime.Now.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                DataRow[] drs = dt.Select("Currency = '" + cf.DataValue + "'");
                if (drs.Length > 0)
                {
                    return Convert.ToDouble(drs[0]["Rate"]);
                }

                return 0;
            }
        }
    }
}