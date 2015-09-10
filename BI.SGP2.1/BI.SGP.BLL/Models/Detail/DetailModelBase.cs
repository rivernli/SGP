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

namespace BI.SGP.BLL.Models
{
    public abstract class DetailModelBase
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

        public Dictionary<string, Dictionary<string, object>> SubTableDataForVVI
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
                            if ((fi.CurrentlyInUse == false || fi.Visible == 0) && (fi.FieldName!="NVARCHAR1" && fi.FieldName != "FLOAT19")) continue;
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

        public DetailModelBase() { }

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
                    else if(entityName=="VVIDETAIL")
                    {

                        string strsqlforupdate = "update SGP_SubData set FLOAT16=b.UnitPrice1,FLOAT17=round((b.UnitPrice1-a.FLOAT12)/b.UnitPrice1*100,2)  from  SGP_SubData a,SGP_RFQPricing b where a.EntityID=b.RFQID and b.UnitPrice1>0 and a.EntityID = " + dataId + " AND a.EntityName = '" + entityName + "'";
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

        public DetailModelBase(List<FieldCategory> categories, Dictionary<string, object> data)
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

        public DetailModelBase(List<FieldCategory> categories, DataRow mainData, Dictionary<string, DataRow[]> subData)
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
                strField = strField.TrimEnd(',');
                string tk = TableKey[tableName.ToUpper()];
                listParames.Add(new SqlParameter("@" + tk, this.ID));
                string strSql = "UPDATE " + tableName + " SET " + strField + " WHERE " + tk + " = @" + tk + ";";
                DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
            }

            UpdateSubData();
            UpdateOther();
            AddHistory();
        }

        public string REVFormat(string rev)
        {
            int i = 0;
            Int32.TryParse(rev, out i);
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
        }
        public void UpdateSubData()
        {
            foreach (string tableName in SubTableData.Keys)
            {
                string strSql = "", strField = "DataIndex,EntityID,EntityName,", strValue = "@DataIndex,@EntityID,@EntityName,", strFieldUpdate = "";
                List<SqlParameter> listEntPara = new List<SqlParameter>();
                listEntPara.Add(new SqlParameter("@EntityID", this.ID));
                listEntPara.Add(new SqlParameter("@EntityName", tableName));
                //strSql = "DELETE FROM SGP_SubData WHERE EntityID=@EntityID AND EntityName=@EntityName";
                //DbHelperSQL.ExecuteSql(strSql, new SqlParameter[] { new SqlParameter("@EntityID", this.ID), new SqlParameter("@EntityName", tableName) });

                int subCount = 0;
                foreach (KeyValuePair<string, object> tableFields in SubTableData[tableName])
                {
                    strField += tableFields.Key + ",";
                    strValue += "@" + tableFields.Key + ",";
                    strFieldUpdate += tableFields.Key + "=@" + tableFields.Key + ",";
                    if (subCount == 0)
                    {
                        ArrayList arrVal = tableFields.Value as ArrayList;
                        if (arrVal != null)
                        {
                            subCount = arrVal.Count;
                        }
                    }
                }
                strField = strField.TrimEnd(',');
                strValue = strValue.TrimEnd(',');
                strFieldUpdate = strFieldUpdate.TrimEnd(',');
                string strupdateSql = "";
                if (tableName == "PRODUCTINFORMATION")
                {
                    strupdateSql = "Update SGP_SubData set " + strFieldUpdate + " where  EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR1=@NVARCHAR1";
                }
                else if (tableName == "PRICEDETAIL")
                {
                    strupdateSql = "Update SGP_SubData set " + strFieldUpdate + " where  EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR4=@NVARCHAR4";

                }
                else if (tableName == "TOOLINGSUMMARY")
                {
                    strupdateSql = "Update SGP_SubData set " + strFieldUpdate + " where  EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR1=@NVARCHAR1";
                }


                strSql = "SELECT MAX(DataIndex) FROM SGP_SubData WHERE EntityID=@EntityID AND EntityName=@EntityName";
                int MaxDataIndex = DbHelperSQL.GetSingle<int>(strSql, new SqlParameter[] { new SqlParameter("@EntityID", this.ID), new SqlParameter("@EntityName", tableName) });
                string strInsertSql = "INSERT INTO SGP_SubData(" + strField + ") VALUES(" + strValue + ")";

                for (int i = 0; i < subCount; i++)
                {
                    List<SqlParameter> listParames = new List<SqlParameter>();
                    listParames.AddRange(listEntPara);

                    foreach (KeyValuePair<string, object> tableFields in SubTableData[tableName])
                    {

                        ArrayList arrVal = tableFields.Value as ArrayList;
                        if (tableName == "PRODUCTINFORMATION" && tableFields.Key == "NVARCHAR1")
                        {
                            listParames.Add(new SqlParameter("@" + tableFields.Key, REVFormat(arrVal[i].ToString())));
                        }
                        else if (tableName == "PRICEDETAIL" && tableFields.Key == "NVARCHAR4")
                        {
                            listParames.Add(new SqlParameter("@" + tableFields.Key, REVFormat(arrVal[i].ToString())));
                        }
                        else
                        {
                            listParames.Add(new SqlParameter("@" + tableFields.Key, String.IsNullOrEmpty(arrVal[i].ToString()) ? DBNull.Value : arrVal[i]));
                        }
                    }

                    if (tableName == "PRODUCTINFORMATION")
                    {
                        strSql = "SELECT COUNT(*) FROM SGP_SubData WHERE EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR1=@NVARCHAR1";
                        int exists = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
                        if (exists > 0)
                        {

                            DbHelperSQL.ExecuteSql(strupdateSql, listParames.ToArray());
                        }
                        else
                        {
                            listParames.Add(new SqlParameter("@DataIndex", ++MaxDataIndex));
                            DbHelperSQL.ExecuteSql(strInsertSql, listParames.ToArray());
                        }
                    }
                    if (tableName == "PRICEDETAIL")
                    {
                        strSql = "SELECT COUNT(*) FROM SGP_SubData WHERE EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR4=@NVARCHAR4";
                        int exists = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
                        if (exists > 0)
                        {

                            DbHelperSQL.ExecuteSql(strupdateSql, listParames.ToArray());
                        }
                        else
                        {
                            listParames.Add(new SqlParameter("@DataIndex", ++MaxDataIndex));
                            DbHelperSQL.ExecuteSql(strInsertSql, listParames.ToArray());
                        }
                    }
                    if (tableName == "TOOLINGSUMMARY")
                    {
                        strSql = "SELECT COUNT(*) FROM SGP_SubData WHERE EntityID=@EntityID AND EntityName=@EntityName AND NVARCHAR1=@NVARCHAR1";
                        int exists = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
                        if (exists > 0)
                        {

                            DbHelperSQL.ExecuteSql(strupdateSql, listParames.ToArray());
                        }
                        else
                        {
                            listParames.Add(new SqlParameter("@DataIndex", ++MaxDataIndex));
                            DbHelperSQL.ExecuteSql(strInsertSql, listParames.ToArray());
                        }
                    }

                }
            }
        }

        public void UpdateForVVI(int ID)
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
                if (!string.IsNullOrEmpty(strField))
                {
                    strField = strField.TrimEnd(',');
                    string tk = TableKey[tableName.ToUpper()];
                    listParames.Add(new SqlParameter("@" + tk, this.ID));
                    string strSql = "UPDATE " + tableName + " SET " + strField + " WHERE " + tk + " = @" + tk + ";";
                    DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                }
            }

            UpdateSubDataForVVI();
            UpdateOther();
            AddHistory();
        }

        public void UpdateSubDataForVVI()
        {
            foreach (string tableName in SubTableDataForVVI.Keys)
            {
                string strFieldUpdate = "";
                List<SqlParameter> listEntPara = new List<SqlParameter>();
                listEntPara.Add(new SqlParameter("@EntityID", this.ID));
                listEntPara.Add(new SqlParameter("@EntityName", tableName));
                int subCount = 0;
                foreach (KeyValuePair<string, object> tableFields in SubTableDataForVVI[tableName])
                {
                    if (tableFields.Key == "NVARCHAR1")
                    {
                        continue;
                    }
                    strFieldUpdate += tableFields.Key + "=@" + tableFields.Key + ",";
                    if (subCount == 0)
                    {
                        ArrayList arrVal = tableFields.Value as ArrayList;
                        if (arrVal != null)
                        {
                            subCount = arrVal.Count;
                        }
                    }
                }
                strFieldUpdate = strFieldUpdate.TrimEnd(',');

                string strupdateSql = "Update SGP_SubData set " + strFieldUpdate + " where  NVARCHAR1=@NVARCHAR1 AND EntityID=@EntityID AND EntityName=@EntityName";


                for (int i = 0; i < subCount; i++)
                {
                    List<SqlParameter> listParames = new List<SqlParameter>();
                    listParames.AddRange(listEntPara);
                    foreach (KeyValuePair<string, object> tableFields in SubTableDataForVVI[tableName])
                    {
                        
                        ArrayList arrVal = tableFields.Value as ArrayList;
                        listParames.Add(new SqlParameter("@" + tableFields.Key, String.IsNullOrEmpty(arrVal[i].ToString()) ? DBNull.Value : arrVal[i]));
                        
                    }
                   
                    DbHelperSQL.ExecuteSql(strupdateSql, listParames.ToArray());
                }


            }

        }

        public SystemMessages AssignVVIData(int ID, string suppliercode, SystemMessages sysmgs)
        {
            this.ID = ID;
            //SystemMessages sysmgs = new SystemMessages();

            try
            {
                string strField = "", strValue = "";
                FieldInfoCollecton supplierfields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGPFORSUPPLIER);
                string number = "";
                foreach (FieldInfo fi in AllMasterFields)
                {
                    number += fi.FieldName == "Number" ? fi.DataValue.ToString() + "-" + suppliercode : "";

                    foreach (FieldInfo supplierfi in supplierfields)
                    {
                        if (fi.DisplayName == supplierfi.DisplayName)
                        {
                            if (supplierfi.FieldName == "NVARCHAR1")
                            {
                                fi.DataValue = number;
                                strField += supplierfi.FieldName + ",";
                                strValue += "'" + number + "',";
                            }
                         
                            else
                            {
                                strField += supplierfi.FieldName + ",";
                                strValue += "'" + fi.DataValue.ToString() + "',";
                            }
                        }
                    }
                }

                strField += "DATETIME2,";
                strValue += "GetDate(),";

                string vendorname = DbHelperSQL.GetSingle<string>("select SupplierName from SYS_Supplier where SupplyCode=@SupplyCode", new SqlParameter("@SupplyCode",suppliercode));
                vendorname = suppliercode + "[" + vendorname + "]";

                strField += "NVARCHAR7,";
                strValue += "'" + vendorname + "',";
                
                strField = strField.TrimEnd(',');
                strValue = strValue.TrimEnd(',');
                bool existsRFQNumber = DbHelperSQL.Exists("select count(*) from SGP_SubData where NVARCHAR1=@Number and EntityName='VVIDETAIL' ", new SqlParameter("@Number", number));
                if (existsRFQNumber == false)
                {
                    try
                    {
                        int maxdataindex = -1;
                        maxdataindex = DbHelperSQL.GetSingle<int>("select Max(DataIndex) from SGP_SubData where EntityID=" + ID.ToString() + " and EntityName='VVIDETAIL'");
                        maxdataindex++;
                        string strSql = "insert into SGP_SubData(DataIndex,EntityID,EntityName," + strField + ") values(" + maxdataindex.ToString() + "," + ID.ToString() + ",'VVIDETAIL'," + strValue + ")";
                        DbHelperSQL.ExecuteSql(strSql);
                        string vendormail = "";
                        try
                        {
                            WF.WFUser user = new WF.WFUser(suppliercode);
                            if (user != null)
                            {
                                vendormail = user.Email;
                            }
                            else
                            {
                                vendormail = "cfszy@163.com";
                            }
                        }
                        catch
                        {
                            vendormail = "cfszy@163.com"; 
                        }
                       
                        
                      
                       // wf.Run();
                        sysmgs.isPass = true;
                        sysmgs.MessageType = "Success";
                        sysmgs.Messages.Add("OK", number);
                        BI.SGP.BLL.WF.WFTemplate wftemplate = new WF.WFTemplate("SUPPLIERWF", ID, number);
                        BI.SGP.BLL.WF.WFActivity wfactivity = wftemplate.FirstActivity;
                        BI.SGP.BLL.WF.Action.SendMailAction sendmail = new WF.Action.SendMailAction();
                        sendmail.DoActionForVVI(wfactivity, vendorname, vendormail);


                        string strsql1 = "update SGP_RFQForVVI set ActivityID=102 where (ActivityID in(101) or ActivityID is null) and RFQID=@RFQID";

                        string strsql2 = "insert into SYS_WFProcessLog select @RFQID,3,@FromActivityID,@ToActivityID,0,GetDate(),@UserID,@Comment";

                        SqlParameter[] sp1 = new SqlParameter[] { new SqlParameter("@RFQID", ID) };

                        SqlParameter[] sp2 = new SqlParameter[] { new SqlParameter("@RFQID",ID),
                                                                  new SqlParameter("@FromActivityID",101),
                                                                  new SqlParameter("@ToActivityID",102),
                                                                  new SqlParameter("@UserID", BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid),
                                                                  new SqlParameter("@Comment", "Assign RFQ")};


                        DbHelperSQL.ExecuteSql(strsql1, sp1.ToArray());
                        DbHelperSQL.ExecuteSql(strsql2, sp2.ToArray());


                        AddHistory();
                    }
                    catch (Exception ex)
                    {
                        sysmgs.isPass = false;
                        sysmgs.MessageType = "Error";
                        sysmgs.Messages.Add("Error",ex.Message);
                    }
                }
                else
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "already exists";
                    sysmgs.Messages.Add("already exists", number);
                }
            }
            catch (Exception ex)
            {
                sysmgs.isPass = false;
                sysmgs.MessageType = "Error";
                sysmgs.Messages.Add("Error", ex.Message);
            }
            return sysmgs;
        }

        public virtual int Add()
        {
            CreateMainRecord();
            string strSql = "";
            foreach (KeyValuePair<string, string> kv in TableKey)
            {
                if (kv.Key != MainTable)
                {
                    string tableName = kv.Key.ToUpper();
                    string strField = kv.Value + ",";
                    string strValue = "@" + kv.Value + ",";
                    List<SqlParameter> listParames = new List<SqlParameter>();
                    listParames.Add(new SqlParameter("@" + kv.Value, this.ID));

                    if (MasterTableData.ContainsKey(tableName))
                    {
                        foreach (KeyValuePair<string, string> tableFields in MasterTableData[tableName])
                        {
                            strField += tableFields.Key + ",";
                            strValue += "@" + tableFields.Key + ",";
                            listParames.Add(String.IsNullOrEmpty(tableFields.Value.Trim()) ? new SqlParameter("@" + tableFields.Key, DBNull.Value) : new SqlParameter("@" + tableFields.Key, tableFields.Value));
                        }
                    }
                    strField = strField.TrimEnd(',');
                    strValue = strValue.TrimEnd(',');
                    strSql = String.Format("DELETE FROM " + kv.Key + " WHERE " + kv.Value + "=@" + kv.Value + ";INSERT INTO " + kv.Key + "(" + strField + ") VALUES(" + strValue + ")");
                    DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                }
            }
            UpdateSubData();
            UpdateOther();
            AddHistory();
            return this.ID;
        }

        public virtual void CreateMainRecord()
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

            strField = strField.TrimEnd(',');
            strValue = strValue.TrimEnd(',');
            strSql = String.Format("INSERT INTO " + tableName + "(" + strField + ") VALUES(" + strValue + ");SELECT @@IDENTITY");
            int newID = DbHelperSQL.GetSingle<int>(strSql, listParames.ToArray());
            this.ID = newID;


        }

        public virtual void UpdateOther()
        {



        }


        public void CloneAttachments(int rfqIdFrom, int rfqIdTo)
        {
            string sql = string.Format(@" INSERT INTO SGP_Files([RelationKey],[FileName],[SourceName],[Folder],[FileSize],[Category],[CategoryDesc],[CreateTime],[Creator],[Status])
                                          SELECT {1},[FileName],[SourceName],[Folder],[FileSize],[Category],[CategoryDesc],[CreateTime],[Creator],[Status] FROM [SGP_Files] WHERE RelationKey='{0}'", rfqIdFrom, rfqIdTo);
            DbHelperSQL.ExecuteSql(sql);
        }
        public virtual void AddHistory()
        {

            //string strSql = "INSERT INTO SGP_RFQHISTORYFORFPC SELECT *,GETDATE() FROM V_SGPForFPC WHERE RFQID=@RFQID";
            //DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", this.ID));
        }

    }
}
