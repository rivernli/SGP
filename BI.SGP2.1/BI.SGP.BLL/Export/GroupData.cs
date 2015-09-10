using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Export
{
    public class GroupData
    {
        public static string GetExportData(HttpRequestBase request, FieldGroup fieldGroup)
        {
            DataSet ds = new DataSet();
            FieldGroupDetailCollection fields = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
            FieldGroupDetailCollection mainFields = new FieldGroupDetailCollection();
            Dictionary<string, FieldGroupDetailCollection> subFields = new Dictionary<string, FieldGroupDetailCollection>();

            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid));

            FieldGroupDetailCollection relationFields = new FieldGroupDetailCollection();
            if (!String.IsNullOrEmpty(fieldGroup.SubRelationFields))
            {
                string[] rfa = fieldGroup.SubRelationFields.Split(',');
                if (rfa != null && rfa.Length > 0)
                {
                    foreach (string rf in rfa)
                    {
                        FieldGroupDetail rfd = fields.Find(t => String.Compare(t.FieldName, rf.Trim(), true) == 0);
                        if (rfd != null)
                        {
                            relationFields.Add(rfd);
                        }
                    }
                }
            }

            foreach (FieldGroupDetail f in fields)
            {
                if (String.IsNullOrEmpty(f.SubDataType))
                {
                    mainFields.Add(f);
                }
                else
                {
                    if (!subFields.ContainsKey(f.SubDataType))
                    {
                        subFields.Add(f.SubDataType, new FieldGroupDetailCollection());
                    }
                    subFields[f.SubDataType].Add(f);
                }
            }

            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strWhere = GridManager.GetWhereSql(request, fieldGroup.GroupName, mainFields, lstParams);

            if (fieldGroup.Authority)
            {
                strWhere += GridManager.GetWhereSqlPermission(lstParams);
            }
            
            StringBuilder mainSql = new StringBuilder("SELECT ");
            foreach (FieldGroupDetail field in mainFields)
            {
                mainSql.AppendFormat("[{0}] AS [{1}],", field.FieldName, field.DisplayName);
            }
            mainSql.Remove(mainSql.Length - 1, 1);
            mainSql.AppendFormat(" FROM {0} AS t", fieldGroup.SourceName);
            mainSql.Append(" WHERE 1=1 ").Append(strWhere);
            if (AccessControl.IsVendor() &&
                (fieldGroup.GroupName.ToUpper() == "VENDORRFQREPORTGRID" ||
                fieldGroup.GroupName.ToUpper() == "SUPPLIERRFQGRID"))
            {
                string uId = AccessControl.CurrentLogonUser.Uid;
                mainSql.Append(" AND RIGHT(NVARCHAR1, CHARINDEX('-', REVERSE(NVARCHAR1)) - 1) = @UserId");
                lstParams.Add(new SqlParameter("@UserId", uId));
            }

            DataTable mainTable = DbHelperSQL.Query(mainSql.ToString(), lstParams.ToArray()).Tables[0].Copy();
            mainTable.TableName = "Primary";
            ds.Tables.Add(mainTable);

            StringBuilder subSql = new StringBuilder(); 
            foreach (KeyValuePair<string, FieldGroupDetailCollection> kv in subFields)
            {
                subSql.Clear();
                subSql.Append("SELECT ");
                foreach (FieldGroupDetail field in relationFields)
                {
                    subSql.AppendFormat("t1.[{0}] AS [{1}],", field.FieldName, field.DisplayName);
                }
                foreach (FieldGroupDetail field in kv.Value)
                {
                    subSql.AppendFormat("t2.[{0}] AS [{1}],", field.FieldName, field.DisplayName);
                }
                subSql.Remove(subSql.Length - 1, 1);
                subSql.AppendFormat(" FROM {0} t1, SGP_SubData t2 WHERE t1.{1}=t2.EntityID AND EntityName = '{2}' {3}", fieldGroup.SourceName, fieldGroup.SourceKey, kv.Key, strWhere);
                DataTable subTable = DbHelperSQL.Query(subSql.ToString(), lstParams.ToArray()).Tables[0].Copy();
                subTable.TableName = kv.Key;
                ds.Tables.Add(subTable);
            }

            RenderType rt = request.QueryString["renderType"] == "2" ? RenderType.Vertical : RenderType.Horizontal;

            return ExcelHelper.DataSetToExcel(ds, rt);
        }
        public static string GetExportDataForVVI(HttpRequestBase request, FieldGroup fieldGroup)
        {
            DataSet ds = new DataSet();
            FieldGroupDetailCollection fields = fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid);
          //  FieldGroupDetailCollection mainFields = new FieldGroupDetailCollection();
            FieldGroupDetailCollection allFields = new FieldGroupDetailCollection();
           // Dictionary<string, FieldGroupDetailCollection> subFields = new Dictionary<string, FieldGroupDetailCollection>();

            string strSql = GridManager.GetQuerySql(fieldGroup.SourceName, fieldGroup.GetFieldsByUser(AccessControl.CurrentLogonUser.Uid));

            FieldGroupDetailCollection relationFields = new FieldGroupDetailCollection();
            if (!String.IsNullOrEmpty(fieldGroup.SubRelationFields))
            {
                string[] rfa = fieldGroup.SubRelationFields.Split(',');
                if (rfa != null && rfa.Length > 0)
                {
                    foreach (string rf in rfa)
                    {
                        FieldGroupDetail rfd = fields.Find(t => String.Compare(t.FieldName, rf.Trim(), true) == 0);
                        if (rfd != null)
                        {
                            relationFields.Add(rfd);
                        }
                    }
                }
            }

            foreach (FieldGroupDetail f in fields)
            {
                allFields.Add(f);
            }

            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strWhere = GridManager.GetWhereSql(request, fieldGroup.GroupName, allFields, lstParams);

            if (fieldGroup.Authority)
            {
                strWhere += GridManager.GetWhereSqlPermission(lstParams);
            }

            StringBuilder mainSql = new StringBuilder("SELECT ");
            foreach (FieldGroupDetail field in allFields)
            {
                mainSql.AppendFormat("[{0}] AS [{1}],", field.FieldName, field.DisplayName);
            }
            mainSql.Remove(mainSql.Length - 1, 1);
            mainSql.AppendFormat(" FROM {0} AS t", fieldGroup.SourceName);
            mainSql.Append(" WHERE 1=1 ").Append(strWhere);
            DataTable mainTable = DbHelperSQL.Query(mainSql.ToString(), lstParams.ToArray()).Tables[0].Copy();
            mainTable.TableName = "Primary";
            ds.Tables.Add(mainTable);

            //StringBuilder subSql = new StringBuilder();
            //foreach (KeyValuePair<string, FieldGroupDetailCollection> kv in subFields)
            //{
            //    subSql.Clear();
            //    subSql.Append("SELECT ");
            //    foreach (FieldGroupDetail field in relationFields)
            //    {
            //        subSql.AppendFormat("t1.[{0}] AS [{1}],", field.FieldName, field.DisplayName);
            //    }
            //    foreach (FieldGroupDetail field in kv.Value)
            //    {
            //        subSql.AppendFormat("t2.[{0}] AS [{1}],", field.FieldName, field.DisplayName);
            //    }
            //    subSql.Remove(subSql.Length - 1, 1);
            //    subSql.AppendFormat(" FROM {0} t1, SGP_SubData t2 WHERE t1.{1}=t2.EntityID AND EntityName = '{2}' {3}", fieldGroup.SourceName, fieldGroup.SourceKey, kv.Key, strWhere);
            //    DataTable subTable = DbHelperSQL.Query(subSql.ToString(), lstParams.ToArray()).Tables[0].Copy();
            //    subTable.TableName = kv.Key;
            //    ds.Tables.Add(subTable);
            //}

            RenderType rt = request.QueryString["renderType"] == "2" ? RenderType.Vertical : RenderType.Horizontal;

            return ExcelHelper.DataSetToExcel(ds, rt);
        }
    }
}
