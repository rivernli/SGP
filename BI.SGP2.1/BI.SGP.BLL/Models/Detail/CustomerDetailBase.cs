using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;
using System.Collections.Specialized;

namespace BI.SGP.BLL.Models.Detail
{
    public abstract class CustomerDetailBase
    {
        public int ID { get; set; }
        protected Dictionary<string, string> _tableKey;
        public List<FieldCategory> Categories { get; set; }
        //public Dictionary<string, object> RawData { get; set; }
        protected FieldInfoCollecton _allMasterFields;
        protected FieldInfoCollecton _allSubFields;
        protected Dictionary<string, Dictionary<string, string>> _masterTableData;
        protected Dictionary<string, Dictionary<string, object>> _subTableData;
        public abstract Dictionary<string, string> TableKey { get; }
        public abstract string MainTable { get; }

        public FieldInfoCollecton AllMasterFields
        {
            get
            {
                if (_allMasterFields == null)
                {
                    _allMasterFields = new FieldInfoCollecton();
                    foreach (FieldCategory fc in Categories)
                    {
                        foreach (FieldInfo fi in fc.MasterFields)
                        {
                            _allMasterFields.Add(fi);
                        }
                    }
                }
                return _allMasterFields;
            }
        }

        public FieldInfoCollecton AllSubFields
        {
            get
            {
                if (_allSubFields == null)
                {
                    _allSubFields = new FieldInfoCollecton();
                    foreach (FieldCategory fc in Categories)
                    {
                        foreach (FieldInfo fi in fc.SubFields)
                        {
                            _allSubFields.Add(fi);
                        }
                    }
                }
                return _allSubFields;
            }
        }

        public Dictionary<string, Dictionary<string, string>> MasterTableData
        {
            get
            {
                if (_masterTableData == null)
                {
                    _masterTableData = new Dictionary<string, Dictionary<string, string>>();
                    foreach (FieldCategory fc in Categories)
                    {
                        foreach (FieldInfo fi in fc.MasterFields)
                        {
                            if (fi.CurrentlyInUse == false || fi.Visible == 0) continue;
                            string tableName = fi.TableName.ToUpper();
                            if (!_masterTableData.ContainsKey(tableName))
                            {
                                _masterTableData.Add(tableName, new Dictionary<string, string>());
                            }

                            _masterTableData[tableName].Add(fi.FieldName, string.Format("{0}", fi.DataValue));
                        }
                    }
                }
                return _masterTableData;
            }
        }

        public Dictionary<string, Dictionary<string, object>> SubTableData
        {
            get
            {
                if (_subTableData == null)
                {
                    _subTableData = new Dictionary<string, Dictionary<string, object>>();
                    foreach (FieldCategory fc in Categories)
                    {
                        foreach (FieldInfo fi in fc.SubFields)
                        {
                            if (fi.CurrentlyInUse == false || fi.Visible == 0) continue;
                            string entityName = fi.SubDataType.ToUpper();
                            if (!_subTableData.ContainsKey(entityName))
                            {
                                _subTableData.Add(entityName, new Dictionary<string, object>());
                            }

                            _subTableData[entityName].Add(fi.FieldName, fi.DataValue);
                        }
                    }
                }
                return _subTableData;
            }
        }

        public Dictionary<string, string> MainTableData
        {
            get
            {
                if (MasterTableData.ContainsKey(MainTable))
                {
                    return MasterTableData[MainTable];
                }
                else
                {
                    Dictionary<string, string> mt = new Dictionary<string, string>();
                    MasterTableData.Add(MainTable, mt);
                    return mt;
                }
            }
        }

        public CustomerDetailBase() { }

        private void FillCategoryData(FieldCategory category, int dataId, Dictionary<string, DataTable> dicMasterData, Dictionary<string, DataTable> dicSubData)
        {
            DataTable dtData;
            foreach (FieldInfo fi in category.MasterFields)
            {
                string tableName = fi.TableName.ToUpper();
                if (dicMasterData.ContainsKey(tableName))
                {
                    dtData = dicMasterData[tableName];
                }
                else
                {
                    string tk = TableKey[tableName];
                    string strSql = "SELECT * FROM " + tableName + " WHERE " + tk + "=" + dataId;
                    dtData = DbHelperSQL.Query(strSql).Tables[0];
                    dicMasterData.Add(tableName, dtData);
                }

                if (dtData.Rows.Count > 0 && dtData.Columns.Contains(fi.FieldName))
                {
                    fi.DataValue = dtData.Rows[0][fi.FieldName];
                }
            }

            foreach (FieldInfo fi in category.SubFields)
            {
                string entityName = fi.SubDataType.ToUpper();
                if (dicSubData.ContainsKey(entityName))
                {
                    dtData = dicSubData[entityName];
                }
                else
                {
                    if (entityName == "VVIPRODUCTINFORMATION")
                    {
                        string strSql = @"SELECT * FROM SGP_SubData WHERE EntityID in(select id from SGP_RFQForVVI where rfqid in(
                                            select RFQID from SGP_RFQForVVI where ID=" + dataId + @") ) ORDER BY DataIndex";
                        dtData = DbHelperSQL.Query(strSql).Tables[0];
                        dicSubData.Add(entityName, dtData);

                    }
                    else if (entityName == "VVIDETAIL")
                    {

                        string strsqlforupdate = "update SGP_SubData set FLOAT16=b.UnitPrice1,FLOAT17=(a.FLOAT12-b.UnitPrice1)/a.FLOAT12  from  SGP_SubData a,SGP_RFQPricing b where a.EntityID=b.RFQID and b.UnitPrice1>0 and a.EntityID = " + dataId + " AND a.EntityName = '" + entityName + "'";
                        DbHelperSQL.ExecuteSql(strsqlforupdate);
                        string strSql = "SELECT * FROM SGP_SubData WHERE EntityID = " + dataId + " AND EntityName = '" + entityName + "' ORDER BY DataIndex";
                        dtData = DbHelperSQL.Query(strSql).Tables[0];
                        dicSubData.Add(entityName, dtData);
                    }
                    else
                    {
                        string strSql = "SELECT * FROM SGP_SubData WHERE EntityID = " + dataId + " AND EntityName = '" + entityName + "' ORDER BY DataIndex";
                        dtData = DbHelperSQL.Query(strSql).Tables[0];
                        dicSubData.Add(entityName, dtData);
                    }


                }

                if (dtData.Rows.Count > 0 && dtData.Columns.Contains(fi.FieldName))
                {
                    ArrayList arr = new ArrayList();
                    foreach (DataRow dr in dtData.Rows)
                    {
                        arr.Add(dr[fi.FieldName]);
                    }
                    fi.DataValue = arr;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="dataId"></param>
        /// <param name="number"></param>
        /// <param name="dicMasterData"></param>
        /// <param name="dicSubData"></param>
        private void FillCategoryData(FieldCategory category, int dataId, string number,
            Dictionary<string, DataTable> dicMasterData, Dictionary<string, DataTable> dicSubData)
        {
            DataTable dtData;
            foreach (FieldInfo fi in category.MasterFields)
            {
                string tableName = fi.TableName.ToUpper();
                if (dicMasterData.ContainsKey(tableName))
                {
                    dtData = dicMasterData[tableName];
                }
                else
                {
                    string tk = TableKey[tableName];
                    string strSql = "SELECT * FROM " + tableName + " WHERE " + tk + "=@DataId AND NVARCHAR1=@Number";

                    dtData = DbHelperSQL.Query(strSql,
                        new SqlParameter("@DataId", dataId),
                        new SqlParameter("@Number", number)).Tables[0];
                    dicMasterData.Add(tableName, dtData);
                }

                if (dtData.Rows.Count > 0 && dtData.Columns.Contains(fi.FieldName))
                {
                    fi.DataValue = dtData.Rows[0][fi.FieldName];
                }
            }

            foreach (FieldInfo fi in category.SubFields)
            {
                string entityName = fi.SubDataType.ToUpper();
                if (dicSubData.ContainsKey(entityName))
                {
                    dtData = dicSubData[entityName];
                }
                else
                {
                    if (entityName == "VVIPRODUCTINFORMATION")
                    {
                        string strSql = @"SELECT * FROM SGP_SubData WHERE EntityID in(select id from SGP_RFQForVVI where rfqid in(
                                            select RFQID from SGP_RFQForVVI where ID=" + dataId + @") ) ORDER BY DataIndex";
                        dtData = DbHelperSQL.Query(strSql).Tables[0];
                        dicSubData.Add(entityName, dtData);

                    }
                    else
                    {
                        string strSql = "SELECT * FROM SGP_SubData WHERE EntityID = " + dataId + " AND EntityName = '" + entityName + "' ORDER BY DataIndex";
                        dtData = DbHelperSQL.Query(strSql).Tables[0];
                        dicSubData.Add(entityName, dtData);
                    }


                }

                if (dtData.Rows.Count > 0 && dtData.Columns.Contains(fi.FieldName))
                {
                    ArrayList arr = new ArrayList();
                    foreach (DataRow dr in dtData.Rows)
                    {
                        arr.Add(dr[fi.FieldName]);
                    }
                    fi.DataValue = arr;
                }
            }
        }

        public void FillCategoryData(List<FieldCategory> categories, int dataId)
        {
            if (dataId > 0)
            {
                Dictionary<string, DataTable> dicMasterData = new Dictionary<string, DataTable>();
                Dictionary<string, DataTable> dicSubData = new Dictionary<string, DataTable>();
                foreach (FieldCategory fc in categories)
                {
                    FillCategoryData(fc, dataId, dicMasterData, dicSubData);
                    foreach (FieldCategory sfc in fc.SubCategory)
                    {
                        FillCategoryData(sfc, dataId, dicMasterData, dicSubData);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="id">RFQ ID</param>
        /// <param name="number">RFQ Number</param>
        public void FillCategoryData(List<FieldCategory> categories, int id, string number)
        {
            if (id > 0)
            {
                Dictionary<string, DataTable> dicMasterData = new Dictionary<string, DataTable>();
                Dictionary<string, DataTable> dicSubData = new Dictionary<string, DataTable>();
                foreach (FieldCategory fc in categories)
                {
                    FillCategoryData(fc, id, number, dicMasterData, dicSubData);
                    foreach (FieldCategory sfc in fc.SubCategory)
                    {
                        FillCategoryData(sfc, id, dicMasterData, dicSubData);
                    }
                }
            }
        }

        public CustomerDetailBase(List<FieldCategory> categories, Dictionary<string, object> data)
        {
            this.Categories = categories;
            foreach (FieldCategory fc in Categories)
            {
                if (data.ContainsKey(fc.ID))
                {
                    fc.FillFieldsData(data[fc.ID] as Dictionary<string, object>);
                }
            }
        }

        public CustomerDetailBase(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
        {
            this.Categories = categories;
            foreach (FieldCategory fc in categories)
            {
                foreach (FieldInfo field in fc.MasterFields)
                {
                    foreach (DataColumn dc in mainData.Table.Columns)
                    {
                        if (String.IsNullOrEmpty(field.SubDataType) && (String.Compare(dc.ColumnName, field.FieldName, true) == 0 || String.Compare(dc.ColumnName, field.DisplayName, true) == 0))
                        {
                            field.DataValue = mainData[dc.ColumnName];
                            field.CurrentlyInUse = true;
                            break;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, DataRow[]> kv in subData)
            {
                string tableName = kv.Key;
                DataRow[] subDrs = kv.Value;
                DataColumnCollection dcs = subDrs[0].Table.Columns;
                foreach (FieldCategory fc in categories)
                {
                    foreach (FieldInfo field in fc.SubFields)
                    {

                        foreach (DataColumn dc in dcs)
                        {
                            if (String.Compare(tableName, field.SubDataType, true) == 0 && (String.Compare(dc.ColumnName, field.FieldName, true) == 0 || String.Compare(dc.ColumnName, field.DisplayName, true) == 0))
                            {
                                ArrayList arrList = new ArrayList();
                                foreach (DataRow subDr in subDrs)
                                {
                                    arrList.Add(subDr[dc.ColumnName]);
                                }
                                field.DataValue = arrList;
                                field.CurrentlyInUse = true;
                                break;
                            }
                        }
                    }
                }
            }
        }




        public void Update(int ID)
        {
            this.ID = ID;
            foreach (string tableName in MasterTableData.Keys)
            {
                string strField = "";
                List<SqlParameter> listParames = new List<SqlParameter>();
                foreach (KeyValuePair<string, string> tableFields in MasterTableData[tableName])
                {
                    strField += tableFields.Key + "=@" + tableFields.Key + ",";
                    listParames.Add(String.IsNullOrEmpty(tableFields.Value.Trim()) ? new SqlParameter("@" + tableFields.Key, DBNull.Value) : new SqlParameter("@" + tableFields.Key, tableFields.Value));
                }

                strField += " Modifier=@Modifier, ModificationDate=@ModificationDate ";
                listParames.Add(new SqlParameter("@Modifier", "" + AccessControl.CurrentLogonUser.Uid + ""));
                listParames.Add(new SqlParameter("@ModificationDate", "" + System.DateTime.Now.ToString() + ""));

                strField = strField.TrimEnd(',');
                string tk = TableKey[tableName.ToUpper()];
                listParames.Add(new SqlParameter("@" + tk, this.ID));
                string strSql = "UPDATE " + tableName + " SET " + strField + " WHERE " + tk + " = @" + tk + ";";
                DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
            }
            AddHistory();
        }


        public virtual int Add()
        {
            string strSql = "";
            string tableName = MainTable.ToUpper();
            string strField = "";
            string strValue = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            foreach (KeyValuePair<string, string> tableFields in MasterTableData[tableName])
            {
                strField += tableFields.Key + ",";
                strValue += "@" + tableFields.Key + ",";
                listParames.Add(String.IsNullOrEmpty(tableFields.Value.Trim()) ? new SqlParameter("@" + tableFields.Key, DBNull.Value) : new SqlParameter("@" + tableFields.Key, tableFields.Value));
            }

            strField += " Creater";
            strValue += " @Creater ";
            listParames.Add(new SqlParameter("@Creater", "" + AccessControl.CurrentLogonUser.Uid + ""));

            strField = strField.TrimEnd(',');
            strValue = strValue.TrimEnd(',');
             

            strSql = String.Format("INSERT INTO " + tableName + "(" + strField + ") VALUES(" + strValue + ");SELECT @@IDENTITY");
            this.ID = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
            AddHistory();
            return this.ID;
        }

        public virtual void UpdateOther()
        {

        }

        //记录每次保存的数据
        public virtual void AddHistory()
        {
            string strSql = "INSERT INTO SGP_CustomerProfile_Data_History SELECT * FROM SGP_CustomerProfile_Data WHERE ID=@ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", this.ID));
        }

    }
}
