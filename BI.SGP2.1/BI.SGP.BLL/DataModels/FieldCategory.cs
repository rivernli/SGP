using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.UIManager;
using SGP.DBUtility;

namespace BI.SGP.BLL.DataModels
{
    public enum FieldType
    {
        MasterField,
        SubField,
        All
    }

    public class FieldCategory
    {
        public const string Category_TYPE_SGP = "SGP";
        public const string Category_TYPE_WORKFLOW = "WorkFlow";
        public const string Category_TYPE_CUSTOMERPROFILE = "CustomerProfile";
        public const string Category_TYPE_VVI = "SGPForVVI";
        public const string Category_TYPE_Supplier = "Supplier";
        public const string Category_TYPE_B2F = "SGPForB2F";
        public const string Category_TYPE_FPC = "FPC";
        public const string Category_TYPE_SGPFORSUPPLIER = "SGPForSupplier";
        public const string Category_TYPE_SCSI = "SCSI";
        public const string Category_TYPE_SCO = "SCO";

        private FieldInfoCollecton _fields;
        private FieldInfoCollecton _enableFields;
        private FieldInfoCollecton _visibleFields;
        private FieldInfoCollecton _invalidFields;
        private FieldInfoCollecton _hiddenFields;
        private FieldInfoCollecton _masterFields;
        private FieldInfoCollecton _subFields;
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string AllowedRoles { get; set; }
        public string ActivityID { get; set; }
        public string CategoryType { get; set; }
        public string ID { get; set; }
        public int ColSpan { get; set; }
        public int Upload { get; set; }
        /// <summary>
        /// SCM Price Master 上传页面类型
        /// </summary>
        public string PageType { get; set; }

        private List<FieldCategory> _subCategory;
        public List<FieldCategory> SubCategory
        {
            get
            {
                if (_subCategory == null)
                {
                    _subCategory = new List<FieldCategory>();
                    string strSql = "SELECT * FROM SYS_FieldCategory WHERE ParentID = @ParentID ORDER BY Sort";
                    DataTable dt = SqlText.ExecuteDataset(strSql, "@ParentID", this.ID).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        FieldCategory fc = new FieldCategory();
                        fc.FillCategory(dr);
                        _subCategory.Add(fc);
                    }
                }

                return _subCategory;
            }
        }

        /// <summary>
        /// Catagory下所有的FieldInfo
        /// </summary>
        public FieldInfoCollecton Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = GetFields();
                }

                return _fields;
            }
        }

        public FieldInfoCollecton EnableFields
        {
            get
            {
                if (_enableFields == null)
                {
                    _enableFields = GetEnableFields();
                }

                return _enableFields;
            }
        }

        public FieldInfoCollecton VisibleFields
        {
            get
            {
                if (_visibleFields == null)
                {
                    _visibleFields = new FieldInfoCollecton();
                    foreach (FieldInfo f in Fields)
                    {
                        if (f.Enable == 1)
                        {
                            _visibleFields.Add(f);
                        }
                    }
                }

                return _visibleFields;
            }
        }

        public FieldInfoCollecton InvalidFields
        {
            get
            {
                if (_invalidFields == null)
                {
                    _invalidFields = new FieldInfoCollecton();
                    foreach (FieldInfo f in Fields)
                    {
                        if (f.Enable == 0 && f.Visible == 0)
                        {
                            _invalidFields.Add(f);
                        }
                    }
                }
                return _invalidFields;
            }
        }

        public FieldInfoCollecton HiddenFields
        {
            get
            {
                if (_hiddenFields == null)
                {
                    _hiddenFields = new FieldInfoCollecton();
                    foreach (FieldInfo f in Fields)
                    {
                        if (f.Enable == 0 && f.Visible == 2)
                        {
                            _hiddenFields.Add(f);
                        }
                    }
                }
                return _hiddenFields;
            }
        }

        public FieldInfoCollecton MasterFields
        {
            get
            {
                if (_masterFields == null)
                {
                    _masterFields = GetMasterFields();
                }

                return _masterFields;
            }
        }

        public FieldInfoCollecton SubFields
        {
            get
            {
                if (_subFields == null)
                {
                    _subFields = GetSubFields();
                }

                return _subFields;
            }
        }

        public FieldCategory() { }

        public FieldCategory(int categoryId)
        {
            string strSql = "SELECT * FROM SYS_FieldCategory WHERE ID = @ID";
            DataTable dt = SqlText.ExecuteDataset(strSql, "@ID", categoryId).Tables[0];
            if (dt.Rows.Count > 0)
            {
                FillCategory(dt.Rows[0]);
            }
        }

        public FieldCategory(string categoryName)
        {
            string strSql = "SELECT * FROM SYS_FieldCategory WHERE CategoryName = @CategoryName";
            DataTable dt = SqlText.ExecuteDataset(strSql, "@CategoryName", categoryName).Tables[0];
            if (dt.Rows.Count > 0)
            {
                FillCategory(dt.Rows[0]);
            }
        }

        public FieldCategory(string categoryName, string typeOfSCMPriceMaster)
        {
            string strSql = "SELECT * FROM SYS_FieldCategory WHERE CategoryName = @CategoryName";
            DataTable dt = SqlText.ExecuteDataset(strSql, "@CategoryName", categoryName).Tables[0];
            if (dt.Rows.Count > 0)
            {
                FillCategory(dt.Rows[0]);
                this.PageType = typeOfSCMPriceMaster;
            }
        }

        private void FillCategory(DataRow dr)
        {
            this.CategoryName = Convert.ToString(dr["CategoryName"]);
            this.Description = Convert.ToString(dr["Description"]);
            this.AllowedRoles = Convert.ToString(dr["AllowedRoles"]);
            this.ActivityID = Convert.ToString(dr["ActivityID"]);
            this.ID = Convert.ToString(dr["ID"]);
            this.ColSpan = ParseHelper.Parse<int>(dr["ColSpan"]);
            this.Upload = ParseHelper.Parse<int>(dr["Upload"]);
            this.CategoryType = Convert.ToString(dr["CategoryType"]);
            if (this.ColSpan == 0) this.ColSpan = 8;
        }

        public static List<FieldCategory> GetCategorys(params string[] categoryType)
        {
            string strWhere = "";
            foreach (string ct in categoryType)
            {
                if (strWhere != "")
                {
                    strWhere += " OR ";
                }

                strWhere += String.Format("CategoryType='{0}'", ct);
            }

            string strSql = string.Format("SELECT * FROM SYS_FieldCategory WHERE {0} ORDER BY Sort", strWhere);
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            List<FieldCategory> list = new List<FieldCategory>();

            foreach (DataRow dr in dt.Rows)
            {
                FieldCategory fc = new FieldCategory();
                fc.FillCategory(dr);
                list.Add(fc);
            }

            return list;
        }

        public static List<FieldCategory> GetMasterCategorys(params string[] categoryType)
        {
            string strWhere = "";
            foreach (string ct in categoryType)
            {
                if (strWhere != "")
                {
                    strWhere += " OR ";
                }

                strWhere += String.Format("CategoryType='{0}'", ct);
            }

            string strSql = string.Format("SELECT * FROM SYS_FieldCategory WHERE ParentID = 0 AND ({0}) ORDER BY Sort", strWhere);
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            List<FieldCategory> list = new List<FieldCategory>();

            foreach (DataRow dr in dt.Rows)
            {
                FieldCategory fc = new FieldCategory();
                fc.FillCategory(dr);
                list.Add(fc);
            }

            return list;
        }

        public static List<FieldCategory> GetCategoryByName(string categoryNames)
        {
            string strSql = string.Format("SELECT * FROM SYS_FieldCategory WHERE CategoryName IN({0}) ORDER BY Sort", "'" + categoryNames.Replace(",", "','") + "'");
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            List<FieldCategory> list = new List<FieldCategory>();

            foreach (DataRow dr in dt.Rows)
            {
                FieldCategory fc = new FieldCategory();
                fc.FillCategory(dr);
                list.Add(fc);
            }

            return list;
        }

        public static List<FieldCategory> GetAllCategorys()
        {
            string strSql = "SELECT * FROM SYS_FieldCategory ORDER BY Sort";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            List<FieldCategory> list = new List<FieldCategory>();

            foreach (DataRow dr in dt.Rows)
            {
                FieldCategory fc = new FieldCategory();
                fc.FillCategory(dr);
                list.Add(fc);
            }

            return list;
        }

        public static FieldInfoCollecton GetSubFields(string subFieldType, FieldInfoCollecton AllFields)
        {
            FieldInfoCollecton fields = new FieldInfoCollecton();
            foreach (FieldInfo f in fields)
            {
                if (String.Compare(f.SubDataType, subFieldType, true) == 0)
                {
                    fields.Add(f);
                }
            }
            return fields;
        }

        public static List<FieldCategory> GetChildCategories(FieldCategory category)
        {
            List<FieldCategory> childCategories = new List<FieldCategory>();
            string strSql = "SELECT * FROM SYS_FieldCategory WHERE ParentID = @ParentID ORDER BY Sort";
            DataTable dt = SqlText.ExecuteDataset(strSql, "@ParentID", category.ID).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                FieldCategory fc = new FieldCategory();
                fc.FillCategory(dr);
                childCategories.Add(fc);
            }
            return childCategories;
        }

        public static FieldInfoCollecton GetAllFields(params string[] categoryType)
        {
            string strWhere = "";
            foreach (string ct in categoryType)
            {
                if (strWhere != "")
                {
                    strWhere += " OR ";
                }

                strWhere += String.Format("CategoryType='{0}'", ct);
            }
            if (strWhere != "") strWhere = " AND (" + strWhere + ")";
            
            string strSql = String.Format("SELECT t1.* FROM SYS_FieldInfo t1, SYS_FieldCategory t2 WHERE t1.CategoryID = t2.ID AND t1.Status = 1 {0} ORDER BY t1.Sort", strWhere);
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
            return fields;
        }

        public static FieldInfoCollecton GetAllFieldsOrderbyCategoryID()
        {
            string strSql = "SELECT * FROM SYS_FieldInfo WHERE Status = 1 order by CategoryID,sort";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
            return fields;
        }

        private FieldInfoCollecton GetFields()
        {
            if (!String.IsNullOrWhiteSpace(ID))
            {
                string strSql = "SELECT * FROM SYS_FieldInfo t1, SYS_FieldCategory t2 WHERE t1.CategoryID = t2.ID AND t2.ID = @ID AND t1.Status = 1 order by t1.sort";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
                FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
                RelatedCategory(fields);
                return fields;
            }
            return null;
        }

        private FieldInfoCollecton GetEnableFields()
        {
            if (!String.IsNullOrWhiteSpace(ID))
            {
                string strSql = "SELECT * FROM SYS_FieldInfo t1, SYS_FieldCategory t2 WHERE t1.CategoryID = t2.ID AND t2.ID = @ID AND t1.Status = 1 AND t1.Enable = 1 order by t1.sort";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
                FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
                RelatedCategory(fields);
                return fields;
            }
            return null;
        }

        private FieldInfoCollecton GetMasterFields()
        {
            if (!String.IsNullOrWhiteSpace(ID))
            {
                string strSql = "SELECT * FROM SYS_FieldInfo t1, SYS_FieldCategory t2 WHERE t1.CategoryID = t2.ID AND t2.ID = @ID AND t1.Status = 1 AND t1.Enable = 1 AND ISNULL(t1.SubDataType,'') = '' ORDER BY t1.Sort";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
                FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
                RelatedCategory(fields);
                return fields;
            }
            return null;
        }

        private FieldInfoCollecton GetSubFields()
        {
            if (!String.IsNullOrWhiteSpace(ID))
            {
                string strSql = "SELECT * FROM SYS_FieldInfo t1, SYS_FieldCategory t2 WHERE t1.CategoryID = t2.ID AND t2.ID = @ID AND t1.Status = 1 AND t1.Enable = 1 AND t1.SubDataType <> '' ORDER BY t1.Sort";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];
                FieldInfoCollecton fields = new FieldInfoCollecton(ModelHandler<FieldInfo>.FillModel(dt));
                RelatedCategory(fields);
                return fields;
            }
            return null;
        }
        private void RelatedCategory(FieldInfoCollecton fields)
        {
            if (fields != null)
            {
                foreach (FieldInfo f in fields)
                {
                    f.Category = this;
                    f.Options = new DataOptions(f.DataOptions, this.PageType);
                }
            }
        }

        private void InitSCMPriceMasterType(FieldInfo field)
        {
            if (!string.IsNullOrEmpty(this.PageType))
            {
                
            }
        }

        public static void CheckDataType(FieldInfo field, object value, SystemMessages sysMsg, int line)
        {
            string strLine = line == 0 ? "" : "(line:" + line + ")";
            switch (field.DataType)
            {
                case FieldInfo.DATATYPE_INT:
                    if (!UIComponent.IsInt(value))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] is not numeric type", value));
                    }
                    break;
                case FieldInfo.DATATYPE_DATE:
                case FieldInfo.DATATYPE_DATETIME:
                    if (!UIComponent.IsDate(value))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] is not datetime type", value));
                    }
                    break;
                case FieldInfo.DATATYPE_DOUBLE:
                case FieldInfo.DATATYPE_FLOAT:
                    if (!UIComponent.IsFloat(value))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] is not float type", value));
                    }
                    break;
                case FieldInfo.DATATYPE_PERCENT:
                    if (!UIComponent.IsPercent(value))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] is not numeric type", value));
                    }
                    break;
                case FieldInfo.DATATYPE_LIST:
                    if (!UIComponent.IsList(field.KeyValueSource, value))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] not exist in data list", value));
                    }
                    break;
                case FieldInfo.DATATYPE_LIST_SQL:
                    if (!UIComponent.IsListSql(value, field.KeyValueSource))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName + strLine, String.Format("[{0}] not exist in data list", value));
                    }
                    break;
            }
        }

        public void CheckDataType(Dictionary<string, object> data, SystemMessages sysMsg)
        {
            foreach (FieldInfo field in MasterFields)
            {
                if (data.ContainsKey(field.FieldName))
                {
                    object value = data[field.FieldName];
                    if (!String.IsNullOrEmpty(field.SubDataType) && value is ArrayList)
                    {
                        ArrayList arrList = (ArrayList)value;
                        for (int i = 0; i < arrList.Count; i++)
                        {
                            var val = arrList[i];
                            CheckDataType(field, val, sysMsg, i+1);
                        }
                    }
                    else
                    {
                        CheckDataType(field, value, sysMsg, 0);
                    }
                }
            }

            foreach (FieldInfo field in SubFields)
            {
                if (data.ContainsKey(field.FieldName))
                {
                    object value = data[field.FieldName];
                    if (!String.IsNullOrEmpty(field.SubDataType) && value is ArrayList)
                    {
                        ArrayList arrList = (ArrayList)value;
                        for (int i = 0; i < arrList.Count; i++)
                        {
                            var val = arrList[i];
                            CheckDataType(field, val, sysMsg, i + 1);
                        }
                    }
                    else
                    {
                        CheckDataType(field, value, sysMsg, 0);
                    }
                }
            }
        }

        public void FillFieldsData(Dictionary<string, object> data)
        {
            foreach (KeyValuePair<string, object> kv in data)
            {
                SetFieldValue(kv, MasterFields, false);
                SetFieldValue(kv, SubFields, true);
            }
        }

        public void SetFieldValue(KeyValuePair<string, object> kv, FieldInfoCollecton fields, bool subDataToArray)
        {
            FieldInfo f = fields[kv.Key];
            if (f != null)
            {
                if (subDataToArray && !(kv.Value is ArrayList))
                {
                    f.DataValue = new ArrayList() { kv.Value };
                }
                else
                {
                    f.DataValue = kv.Value;
                }
                
                if (!String.IsNullOrEmpty(f.TableName))
                {
                    f.CurrentlyInUse = true;
                }
            }
        }

        public void ClearFieldsData()
        {
            foreach (FieldInfo f in MasterFields)
            {
                f.DataValue = null;
                f.CurrentlyInUse = false;
            }
            foreach (FieldInfo f in SubFields)
            {
                f.DataValue = null;
                f.CurrentlyInUse = false;
            }
        }

        public void ClearAllFieldsData()
        {
            foreach (FieldInfo f in Fields)
            {
                f.DataValue = null;
                f.CurrentlyInUse = false;
            }
        }
    }
}
