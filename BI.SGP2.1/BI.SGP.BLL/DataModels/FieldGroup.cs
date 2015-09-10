using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.Models;

namespace BI.SGP.BLL.DataModels
{
    public class FieldGroup
    {
        const string GROUP_TYPE_EXCEL = "Excel";
        const string GROUP_TYPE_GRID = "Grid";
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
        public string SourceName { get; set; }
        public string SourceKey { get; set; }
        public string SubRelationFields { get; set; }
        public bool Authority { get; set; }

        public FieldGroup(int groupID)
        {
            string strSql = "SELECT * FROM SYS_FieldGroup WHERE ID = @GroupID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@GroupID", groupID)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                FillObject(dt.Rows[0]);
            }
        }

        public FieldGroup(string groupName)
        {
            string strSql = "SELECT * FROM SYS_FieldGroup WHERE GroupName = @GroupName";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@GroupName", groupName)).Tables[0];
            if(dt.Rows.Count > 0)
            {
                FillObject(dt.Rows[0]);
            }
        }

        public FieldGroup(string groupName, string groupType)
        {
            string strSql = "SELECT * FROM SYS_FieldGroup WHERE GroupName = @GroupName AND GroupType = @GroupType";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@GroupName", groupName), new SqlParameter("@GroupType", groupType)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                FillObject(dt.Rows[0]);
            }
        }

        private void FillObject(DataRow dr)
        {
            this.GroupID = Convert.ToInt32(dr["ID"]);
            this.GroupName = Convert.ToString(dr["GroupName"]);
            this.GroupType = Convert.ToString(dr["GroupType"]);
            this.SourceName = Convert.ToString(dr["SourceName"]);
            this.SourceKey = Convert.ToString(dr["SourceKey"]);
            this.SubRelationFields = Convert.ToString(dr["SubRelationFields"]);
            this.Authority = Convert.ToBoolean(dr["Authority"]);
        }

        public static List<FieldGroup> GetExcelGroups()
        {
            string strSql = @"SELECT GroupName FROM SYS_FieldGroup WHERE GroupType = '" + GROUP_TYPE_EXCEL + "' ORDER BY Sort";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];

            List<FieldGroup> list = new List<FieldGroup>();

            foreach (DataRow dr in dt.Rows)
            {
                FieldGroup fg = new FieldGroup(dr["GroupName"].ToString());
                list.Add(fg);
            }

            return list;
        }

        public FieldGroupDetailCollection GetFieldsByUser(string uid)
        {
            FieldGroupDetailCollection fgd = GetFields(uid);
            if (fgd.Count == 0)
            {
                return GetDefaultFields();
            }

            return fgd;
        }

        public FieldGroupDetailCollection GetDefaultFields()
        {
            return GetFields("default");
        }

        public FieldGroupDetailCollection GetFields() 
        {
            string strSql = @"SELECT t1.UID, t1.Width AS Width_Detail, t1.Format AS Format_Detail, t1.Sort AS Sort_Detail, t2.* 
                            FROM SYS_FieldGroupDetail t1, SYS_FieldInfo t2
                            WHERE t2.Status = 1 AND t1.FieldID = t2.ID AND t1.GroupID = @GroupID
                            ORDER BY t1.Sort";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[] { new SqlParameter("@GroupID", this.GroupID) }).Tables[0];

            return new FieldGroupDetailCollection(ModelHandler<FieldGroupDetail>.FillModel(dt));
        }

        private FieldGroupDetailCollection GetFields(string uid)
        {
            string strSql = @"SELECT t1.UID, t1.Width AS Width_Detail, t1.Format AS Format_Detail, t1.Sort AS Sort_Detail, t2.* 
                            FROM SYS_FieldGroupDetail t1, SYS_FieldInfo t2
                            WHERE t2.Status = 1 AND t1.FieldID = t2.ID AND t1.GroupID = @GroupID AND t1.UID = @UID
                            ORDER BY t1.Sort";

            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[] { new SqlParameter("@GroupID", this.GroupID), new SqlParameter("@UID", uid) }).Tables[0];

            return new FieldGroupDetailCollection(ModelHandler<FieldGroupDetail>.FillModel(dt));
        }

        public void SaveUserFieldGroup(string uid, FieldsSettingModel fieldsSettingModel)
        {
            string strSql = "DELETE FROM SYS_FieldGroupDetail WHERE UID = @UID AND GroupID = @GroupID";
            DbHelperSQL.Query(strSql, new SqlParameter[] { new SqlParameter("@UID", uid), new SqlParameter("@GroupID", GroupID) });

            strSql = "INSERT INTO SYS_FieldGroupDetail(GroupID, FieldID, UID, Sort) VALUES(@GroupID, @FieldID, @UID, @Sort)";

            if (fieldsSettingModel.FieldIds != null)
            {
                for (int i = 0; i < fieldsSettingModel.FieldIds.Length; i++)
                {
                    string fieldId = fieldsSettingModel.FieldIds[i];
                    if (!String.IsNullOrEmpty(fieldId))
                    {
                        SqlParameter[] ps = 
                        {
                            new SqlParameter("@GroupID", this.GroupID),
                            new SqlParameter("@FieldID", fieldsSettingModel.FieldIds[i]),
                            new SqlParameter("@UID", uid),
                            new SqlParameter("@Sort", i)    
                        };

                        DbHelperSQL.ExecuteSql(strSql, ps);
                    }
                }
            }
        }
    }
}
