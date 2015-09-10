using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BI.SGP.BLL.Models;

namespace BI.SGP.BLL.DataModels
{
    /// <summary>
    /// 字段信息
    /// </summary>
    public class FieldInfo
    {
        public const string DATATYPE_STRING = "string";
        public const string DATATYPE_INT = "int";
        public const string DATATYPE_DATE = "date";
        public const string DATATYPE_DATETIME = "datetime";
        public const string DATATYPE_TIME = "time";
        public const string DATATYPE_BOOLEAN = "bool";
        public const string DATATYPE_DOUBLE = "double";
        public const string DATATYPE_FLOAT = "float";
        public const string DATATYPE_LIST = "list";
        public const string DATATYPE_LIST2 = "list2";
        public const string DATATYPE_LIST_SQL = "listsql";
        public const string DATATYPE_LIST_SQL2 = "listsql2";
        public const string DATATYPE_ACTIVITY = "activity";
        public const string DATATYPE_SUMMARY = "summary";
        public const string DATATYPE_PERCENT = "percent";
        public const string DATATYPE_CHECKBOXLIST = "checkboxlist";
        public const string DATATYPE_QUERY = "query";

        public int ID { get; set; }

        /// <summary>
        /// ColumnCategory的ID
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 字段显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 字列表类型
        /// </summary>
        public string SubDataType { get; set; }
        /// <summary>
        /// 字段对应的数据库表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 字段属性选择
        /// </summary>
        public string DataOptions { get; set; }
        /// <summary>
        /// 对应KeyValue表的Key
        /// </summary>
        public string KeyValueSource { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 字段的宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 按指定格式显示
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int Visible { get; set; }
        /// <summary>
        /// 合并的列
        /// </summary>
        public int Enable { get; set; }
        /// <summary>
        /// 合并的列
        /// </summary>
        public int ColSpan { get; set; }
        /// <summary>
        /// 水平对齐
        /// </summary>
        public string Align { get; set; }
        /// <summary>
        /// 状态(0 InActive 1 Active)
        /// </summary>
        public int Status { get; set; }

        public int ExtEmptyCol { get; set; }

        public int ExtEmptyColSpan { get; set; }

        ///// <summary>
        ///// 当次查询的数据值
        ///// </summary>
        private object _DataValue;
        public object DataValue { get { return _DataValue; } set { _DataValue = value; CurrentlyInUse = true; } }

        public int DataIndex { get; set; }
        
        public RequiredOption WFRequiredOption { get; set; }

        public FieldCategory Category { get; set; }

        public DataOptions Options { get; set; }

        /// <summary>
        /// 在当前更新情况下是否在使用，比如是否包含在EXCEL上传中
        /// </summary>
        public bool CurrentlyInUse { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}", this.FieldName, this.DisplayName);
            //return base.ToString();
        }

        public object GetFieldValue()
        {
            if (this.DataValue is ArrayList)
            {
                if (this.DataIndex == -1)
                {
                    return "";
                }
                else
                {
                    return ((ArrayList)this.DataValue)[this.DataIndex];
                }
            }
            else
            {
                return this.DataValue;
            }
        }

        public void CheckRequired(SystemMessages sysMsg)
        {
            if (this.Options.Required)
            {
                if (String.IsNullOrEmpty(this.SubDataType))
                {
                    if (FieldIsEmpty(this, Convert.ToString(this.DataValue)))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(String.Format("\"{0}\" Field Check", this.Category.Description), String.Format("\"{0}\" is required.", this.DisplayName));
                    }
                }
                else
                {
                    ArrayList arrVal = this.DataValue as ArrayList;
                    if (arrVal != null)
                    {
                        for (int i = 0; i < arrVal.Count; i++)
                        {
                            if (FieldIsEmpty(this, Convert.ToString(arrVal[i])))
                            {
                                sysMsg.isPass = false;
                                sysMsg.Messages.Add(String.Format("\"{0}\" Field Check", this.Category.Description), String.Format("\"{0}(line:{1})\" is required.", this.DisplayName, i + 1));
                            }
                        }
                    }
                }
            }
        }

        public void CheckRequired(SystemMessages sysMsg, int index)
        {
            ArrayList arrVal = this.DataValue as ArrayList;
            if (arrVal != null && arrVal.Count > index)
            {
                if (FieldIsEmpty(this, Convert.ToString(arrVal[index])))
                {
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add(String.Format("\"{0}\" Field Check", this.Category.Description), String.Format("\"{0}(line:{1})\" is required.", this.DisplayName, index + 1));
                }
            }
        }

        private bool FieldIsEmpty(FieldInfo field, string fieldValue)
        {
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
                        double dValue = 0;
                        double.TryParse(fieldValue, out dValue);
                        if (dValue == 0 && !this.Options.AllowZero)
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
    }

    public enum RequiredOption { None = 0, Required = 1, Optional = 2 }
}
