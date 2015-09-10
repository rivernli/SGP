using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.UIManager
{
    public class GridManager
    {
        public static GridColumns GetGridFields(string listName, string uid)
        {
            FieldGroup fieldGroup = new FieldGroup(listName);
            FieldGroupDetailCollection fieldDetails = fieldGroup.GetFieldsByUser(uid);

            GridColumns gridColumns = new GridColumns();
            List<string> colNames = new List<string>();
            List<GridColumnModel> colModels = new List<GridColumnModel>();

            foreach (FieldGroupDetail field in fieldDetails)
            {
                colNames.Add(field.DisplayName);
                GridColumnModel model = new GridColumnModel();
                model.name = field.FieldName;
                model.index = field.FieldName;
                model.width = field.Width_Detail > 0 ? field.Width_Detail : field.Width;
                model.align = field.Align;

                colModels.Add(model);
            }

            return new GridColumns
            {
                colNames = colNames,
                colModel = colModels
            };
        }

        public static GridData GetGridData(HttpRequestBase request, string sql, params SqlParameter[] cmdParms)
        {
            return GetGridData(request["sidx"], request["sord"], request["page"], request["rows"], sql, cmdParms);
        }
        public static GridData GetGridData(string sort, string direction, string page, string rows, string sql, params SqlParameter[] cmdParms)
        {
            int currentPage = String.IsNullOrEmpty(page) ? 1 : Convert.ToInt32(page);
            int rowCount = String.IsNullOrEmpty(rows) ? 10 : Convert.ToInt32(rows);
            int start = ((currentPage - 1) * rowCount) + 1;
            int end = start + rowCount - 1;

            if (String.IsNullOrEmpty(sort))
            {
                sort = "RFQID";
                direction = "DESC";
            }

            string sSql = String.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {1} {2}) AS ROWNUMBER, * FROM ({0}) AS T) AS hty WHERE ROWNUMBER BETWEEN {3} AND {4}", sql, sort, direction, start, end);

            DataTable dt = DbHelperSQL.Query(sSql, cmdParms).Tables[0];
            sSql = String.Format("SELECT COUNT(*) FROM ({0}) AS T", sql);
            object value = DbHelperSQL.GetSingle(sSql, cmdParms);
            int total = value == null ? 0 : Convert.ToInt32(value);

            GridData gridData = new GridData();
            gridData.Total = Convert.ToInt32(Math.Ceiling(total / Convert.ToDouble(rowCount)));
            gridData.Records = total;
            gridData.DataTable = dt;
            gridData.Page = page;

            return gridData;
        }

        public static GridData GetGridDataforSetting(HttpRequestBase request, string sql, params SqlParameter[] cmdParms)
        {
            return GetGridDataforSetting(request["sidx"], request["sord"], request["page"], request["rows"], sql, cmdParms);
        }

        public static GridData GetGridDataforSetting(string sort, string direction, string page, string rows, string sql, params SqlParameter[] cmdParms)
        {
            int currentPage = String.IsNullOrEmpty(page) ? 1 : Convert.ToInt32(page);
            int rowCount = String.IsNullOrEmpty(rows) ? 10 : Convert.ToInt32(rows);
            int start = ((currentPage - 1) * rowCount) + 1;
            int end = start + rowCount - 1;
            if (String.IsNullOrEmpty(sort))
            {
                sort = "FieldName";
                direction = "ASC";
            }

            string sSql = String.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY [{1}] {2}) AS ROWNUMBER, * FROM ({0}) AS T) AS hty WHERE ROWNUMBER BETWEEN {3} AND {4}", sql, sort, direction, start, end);

            DataTable dt = DbHelperSQL.Query(sSql, cmdParms).Tables[0];
            sSql = String.Format("SELECT COUNT(*) FROM ({0}) AS T", sql);
            object value = DbHelperSQL.GetSingle(sSql, cmdParms);
            int total = value == null ? 0 : Convert.ToInt32(value);

            GridData gridData = new GridData();
            gridData.Total = Convert.ToInt32(Math.Ceiling(total / Convert.ToDouble(rowCount)));
            gridData.Records = total;
            gridData.DataTable = dt;
            gridData.Page = page;

            return gridData;
        }

        public static GridData GetGridData(HttpRequestBase request, string strSql)
        {
            return GetGridData(request, strSql, true);
        }

        public static GridData GetGridData(HttpRequestBase request, string strSql, bool auth)
        {
            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GetWhereSql(request, searchGroupName, listParames);
            }

            if (auth)
            {
                strWhere += GetWhereSqlPermission(listParames);
            }

            if (strWhere != "")
            {
                strSql += " WHERE 1= 1" + strWhere;
            }
            return GetGridData(request, strSql, listParames.ToArray());
        }

        /// <summary>
        /// Get filtered RFQ data for supplier 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="strSql"></param>
        /// <param name="auth"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static GridData GetGridData(HttpRequestBase request, string strSql, bool auth, FieldGroupDetailCollection fields)
        {
            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GetWhereSql(request, searchGroupName, listParames);
            }

            if (auth)
            {
                strWhere += GetWhereSqlPermission(listParames);
            }

            ///
            foreach (FieldGroupDetail field in fields)
            {
                if (field.DisplayName.ToUpper() == "RFQ NUMBER")
                {
                    string fieldName = field.FieldName;
                    string uId = AccessControl.CurrentLogonUser.Uid;
                    strWhere += string.Format(" AND RIGHT({0}, CHARINDEX('-', REVERSE({0})) - 1) = @UserId",
                                                fieldName);
                    listParames.Add(new SqlParameter("@UserId", uId));
                    break;
                }
            }

            if (strWhere != "")
            {
                strSql += " WHERE 1= 1" + strWhere;
            }
            return GetGridData(request, strSql, listParames.ToArray());
        }

        public static GridData GetGridData(HttpRequestBase request, string strSql, FieldGroup fieldGroup)
        {
            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GetWhereSql(request, searchGroupName, listParames);
            }

            if (fieldGroup.Authority)
            {
                strWhere += GetWhereSqlList(fieldGroup.GroupName, listParames);
            }

            if (strWhere != "")
            {
                strSql += " WHERE 1= 1" + strWhere;
            }
            return GetGridData(request, strSql, listParames.ToArray());
        }

        public static DataTable GetAllQueryData(string sourceName, FieldGroupDetailCollection fieldGroupDetails)
        {
            string strSql = GridManager.GetQuerySql(sourceName, fieldGroupDetails);
            List<SqlParameter> listParames = new List<SqlParameter>();
            string strWhere = GetWhereSqlPermission(listParames);

            if (strWhere != "")
            {
                strSql += " WHERE 1= 1" + strWhere;
            }
            return DbHelperSQL.Query(strSql, listParames.ToArray()).Tables[0];
        }

        public static DataTable GetAllQueryData(HttpRequestBase request, string strSql)
        {
            return GetAllQueryData(request, strSql, true);
        }

        public static DataTable GetAllQueryData(HttpRequestBase request, string strSql, bool auth)
        {
            string strWhere = "";
            List<SqlParameter> listParames = new List<SqlParameter>();
            string searchGroupName = request.QueryString["searchGroup"];
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GetWhereSql(request, searchGroupName, listParames);
            }

            if (auth)
            {
                strWhere += GetWhereSqlPermission(listParames);
            }

            if (strWhere != "")
            {
                strSql += " WHERE 1= 1" + strWhere;
            }
            //strSql += " ORDER BY RFQID DESC";
            return DbHelperSQL.Query(strSql, listParames.ToArray()).Tables[0];
        }

        public static string GetQuerySql(string sourceName, FieldGroupDetailCollection fieldGroupDetails, params string[] extFields)
        {
            StringBuilder sqlBuilder = new StringBuilder("SELECT ");

            if (extFields != null)
            {
                foreach (string extField in extFields)
                {
                    if (!String.IsNullOrEmpty(extField) && fieldGroupDetails[extField] == null)
                    {
                        sqlBuilder.AppendFormat("{0},", extField);
                }
            }
            }

            foreach (FieldGroupDetail field in fieldGroupDetails)
            {
                if (String.IsNullOrEmpty(field.SubDataType))
                {
                    if (field.DataType == FieldInfo.DATATYPE_SUMMARY)
                    {
                        sqlBuilder.AppendFormat("({0}) AS {1},", field.KeyValueSource, field.FieldName);
                    }
                    else
                    {
                        sqlBuilder.AppendFormat("{0},", field.FieldName);
                    }
                    
                }
            }
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            sqlBuilder.AppendFormat(" FROM {0} AS t", sourceName);
            return sqlBuilder.ToString();
        }

        public static string GetWhereSql(HttpRequestBase request, string searchGroupName, List<SqlParameter> listParames)
        {
            FieldGroup searchGroup = new FieldGroup(searchGroupName);
            FieldGroupDetailCollection fields = searchGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
            return GetWhereSql(request, searchGroupName, fields, listParames);
        }

        public static string GetWhereSql(HttpRequestBase request, string groupName, FieldGroupDetailCollection fields, List<SqlParameter> listParames)
        {
            string strWhere = "";
            foreach (FieldGroupDetail field in fields)
            {
                strWhere += GetWhereOperational(request.QueryString, field, listParames, groupName);
            }

            strWhere += GetWhereSqlExt(request, groupName, listParames);

            return strWhere;
        }

        public static string GetWhereSqlExt(HttpRequestBase request, string searchGroupName, List<SqlParameter> listParames)
        {
            string strWhere = "";
            if (searchGroupName == "DefaultSearch")
            {
                string statusID = request.QueryString["StatusID"];
                if (!String.IsNullOrEmpty(statusID))
                {
                    if (statusID == "1")
                    {
                        strWhere += " AND StatusID <> 9";
                    }
                    else if (statusID == "9")
                    {
                        strWhere += " AND StatusID = 9";
                    }
                }
            }
            return strWhere;
        }

        public static string GetWhereSqlPermission(List<SqlParameter> listParames)
        {
            BI.SGP.BLL.AccessServiceReference.Role[] roles = AccessControl.CurrentLogonUser.Roles;
            string strWhere = "";
            if (!AccessControl.CurrentLogonUser.IsAdmin() && !roles.Contains("SGP_TechnicalCosting"))
            {
                string subWhere = "(SELECT @UserName UNION SELECT Name FROM Access_User WHERE ManagerName = @UserName UNION SELECT FromUserName FROM SGP_Delegation WHERE ToUser = @uid)";
                strWhere += " AND (Initiator IN " + subWhere;
                strWhere += " OR PricingAnalyst IN " + subWhere;
                strWhere += " OR TechnicalQuoting IN " + subWhere;
                strWhere += " OR PrimaryContact IN " + subWhere;
                strWhere += " OR GAMBDM IN " + subWhere + ")";
                listParames.Add(new SqlParameter("@uid", AccessControl.CurrentLogonUser.Uid));
                listParames.Add(new SqlParameter("@UserName", AccessControl.CurrentLogonUser.Name));
            }

            //Customer Profile
            //if (!AccessControl.CurrentLogonUser.IsCustomerAdmin() && roles.Contains("Customer_Profile"))
            //{
            //    if (roles.Contains("Customer_Profile"))
            //    {
            //        strWhere = " AND (Creater = @uid ";
            //        strWhere += " OR Creater IN (SELECT Uid from Access_User WHERE BusinessManagement=@uid))";
            //        listParames.Add(new SqlParameter("@uid", AccessControl.CurrentLogonUser.Uid));
            //    }
            //}

            return strWhere;
        }

        private static string GetWhereOperational(NameValueCollection requestData, FieldGroupDetail field, List<SqlParameter> listParames, string groupName)
        {
            string strWhere = "";
            string fieldValue = requestData[field.FieldName];

            //Change FieldGroup's value for supplier search sql
            if (new FieldGroup(groupName).SourceName.ToUpper() == "V_SGPFORSUPPLIER" &&
                field.DisplayName.ToUpper() == "SUBMITSTATUS")
            { 
                switch (fieldValue)
                {
                    case "Launch":
                        fieldValue = "0";break;
                    case "Closed":
                        fieldValue = "9";break;
                }
            }

            if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Trim() != "")
            {
                switch (field.DataType)
                {
                    case FieldInfo.DATATYPE_DATETIME:
                        string[] values = fieldValue.Split('-');
                        if (values.Length == 2)
                        {
                            DateTime dt;
                            string startDate = values[0].Trim();
                            string endDate = values[1].Trim();
                            if (DateTime.TryParse(startDate, out dt) && DateTime.TryParse(endDate, out dt))
                            {
                                strWhere = String.Format(" AND ({0} >= @Start_{0} AND {0} <= @End_{0})", field.FieldName);
                                listParames.Add(new SqlParameter("@Start_" + field.FieldName, startDate + " 00:00:00"));
                                listParames.Add(new SqlParameter("@End_" + field.FieldName, endDate + " 23:59:59"));
                            }
                        }
                        break;
                    default:
                        strWhere = GetSplitWhere(fieldValue, field, listParames);
                        break;
                }
            }

            return strWhere;
        }

        private static string GetSplitWhere(string fieldValue, FieldGroupDetail field, List<SqlParameter> listParames)
        {
            string strWhere = "";
            string[] moreValues = fieldValue.Trim().Split(';', ',');
            if (moreValues != null && moreValues.Length > 0)
            {
                strWhere += " AND (";
                for (int i = 0; i < moreValues.Length; i++)
                {
                    if (i > 0)
                    {
                        strWhere += " OR ";
                    }
                    switch (field.DataType)
                    {
                        case FieldInfo.DATATYPE_INT:
                        case FieldInfo.DATATYPE_FLOAT:
                        case FieldInfo.DATATYPE_DOUBLE:
                        case FieldInfo.DATATYPE_LIST:
                            strWhere += String.Format("{0} = @{0}{1}", field.FieldName, i);
                            listParames.Add(new SqlParameter("@" + field.FieldName + i, moreValues[i].Trim()));
                            break;
                        case FieldInfo.DATATYPE_SUMMARY:
                            strWhere += String.Format("({0}) LIKE @{1}{2}", field.KeyValueSource, field.FieldName, i);
                            listParames.Add(new SqlParameter("@" + field.FieldName + i, "%" + moreValues[i].Trim() + "%"));
                            break;
                        default:
                            strWhere += String.Format("{0} LIKE @{0}{1}", field.FieldName, i);
                            listParames.Add(new SqlParameter("@" + field.FieldName + i, "%" + moreValues[i].Trim() + "%"));
                            break;
                    }

                }
                strWhere += ")";

            }

            return strWhere;
        }

        public static string GetWhereSqlList(string groupName, List<SqlParameter> listParames)
        {
            BI.SGP.BLL.AccessServiceReference.Role[] roles = AccessControl.CurrentLogonUser.Roles;
            string strWhere = "";

            //Customer Profile
            if (groupName.ToUpper() == "CUSTOMERPROFILEGRID")
            {
                if (!AccessControl.CurrentLogonUser.IsCustomerAdmin())
                {
                    if (roles.Contains("Customer_Profile"))
                    {
                        strWhere = " AND (Creater = @uid ";
                        strWhere += " OR Creater IN (SELECT Uid FROM Access_User WHERE BusinessManagement=@uid) ";
                        strWhere += " OR ID IN (SELECT CustomerId FROM SGP_CustomerPermissionUser WHERE Uid=@uid)) ";
                        listParames.Add(new SqlParameter("@uid", AccessControl.CurrentLogonUser.Uid));
                    }
                }
            }

            return strWhere;
        }
    }
}
