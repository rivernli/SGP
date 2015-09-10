using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using SGP.DBUtility;

namespace BI.SGP.BLL.WF
{
    public class WFCondition
    {
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string Operator { get; set; }
        public DataRow CompareData { get; set; }
        private List<WFCondition> _childCondition;
        public List<WFCondition> ChildCondition
        {
            get
            {
                if (_childCondition == null)
                {
                    _childCondition = new List<WFCondition>();
                    string strSql = "SELECT * FROM SYS_WFConditionGroup WHERE ParentID = @ParentID";
                    DataTable dt = SqlText.ExecuteDataset(strSql, "@ParentID", this.ID).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        _childCondition.Add(FillCondition(dr, this.CompareData));
                    }
                }

                return _childCondition;
            }
        }

        public WFCondition() { }

        public WFCondition(int groupId, DataRow compareData)
        {
            string strSql = "SELECT * FROM SYS_WFConditionGroup WHERE ID = @ID";
            DataTable dt = SqlText.ExecuteDataset(strSql, "@ID", groupId).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.ID = groupId;
                this.ParentID = Convert.ToInt32(dt.Rows[0]["ParentID"]);
                this.Operator = Convert.ToString(dt.Rows[0]["Operator"]);
                this.CompareData = compareData;
            }
        }

        public bool Compare()
        {
            if (CompareData == null)
            {
                return false;
            }
            else
            {
                bool result = true;
                string strSql = "SELECT * FROM SYS_WFConditionDetail WHERE GroupID = @GroupID";
                DataTable dt = SqlText.ExecuteDataset(strSql, "@GroupID", this.ID).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strFieldName = Convert.ToString(dr["FieldName"]);
                        string strDataType = Convert.ToString(dr["DataType"]);
                        string strOperator = Convert.ToString(dr["Operator"]);
                        string strValue = Convert.ToString(dr["Value"]);
                        int iResult = CompareValue(dr);

                        switch (strOperator)
                        {
                            case "=":
                                result = iResult == 0;
                                break;
                            case "<>":
                                result = iResult != 0;
                                break;
                            case ">":
                                result = iResult == 1;
                                break;
                            case "<":
                                result = iResult == -1;
                                break;
                            case ">=":
                                result = iResult >= 0;
                                break;
                            case "<=":
                                result = iResult <= 0;
                                break;
                            default:
                                throw new Exception("unknow compare operator");
                        }

                        if (this.Operator == "AND" && !result)
                        {
                            return false;
                        }
                        else if (this.Operator == "OR" && result)
                        {
                            return true;
                        }
                    }
                }

                foreach (WFCondition wfc in ChildCondition)
                {
                    result = wfc.Compare();
                    if (this.Operator == "AND" && !result)
                    {
                        return false;
                    }
                    else if (this.Operator == "OR" && result)
                    {
                        return true;
                    }
                }

                return result;
            }
        }

        private int CompareValue(DataRow dr)
        {
            string strFieldName = Convert.ToString(dr["FieldName"]);
            string strDataType = Convert.ToString(dr["DataType"]);
            string strOperator = Convert.ToString(dr["Operator"]);
            string strValue = Convert.ToString(dr["Value"]);

            switch (strDataType)
            {
                case "string":
                    return String.Compare(Convert.ToString(this.CompareData[strFieldName]), strValue, true);
                case "number":
                    return ParseHelper.Parse<double>(this.CompareData[strFieldName]).CompareTo(ParseHelper.Parse<double>(strValue));
            }
            throw new Exception("unknow compare type");
        } 

        public static WFCondition FillCondition(DataRow dr, DataRow compareData)
        {
            WFCondition wfCondition = ModelHandler<WFCondition>.FillModel(dr);
            wfCondition.CompareData = compareData;
            return wfCondition;
        }
    }
}
