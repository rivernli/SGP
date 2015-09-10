using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using SGP.DBUtility;

namespace BI.SGP.BLL.WF
{
    public class WFHelper
    {
        /// <summary>
        /// 取得当前用户需要处理的TASK数量，还需要考虑代理
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetUserTaskCount(string userId)
        {
            string sql = string.Format("SELECT COUNT(1) FROM SGP_CurrentUserTask WHERE UID='{0}'", userId);
            int count = DbHelperSQL.GetSingle<int>(sql);
            return count;
        }

        public static SystemMessages CreateMessages()
        {
            return new SystemMessages { isPass = true, MessageType = SystemMessages.MESSAGE_TYPE_WORKFLOW };
        }

        public static void CheckField(WFTemplate template, List<WFActivityField> listFields, SystemMessages sysMsg, WFActivity activity)
        {
            if (sysMsg == null)
            {
                sysMsg = CreateMessages();
            }

            foreach (WFActivityField field in listFields)
            {
                if (field.IsRequired)
                {
                    if (String.IsNullOrEmpty(field.SubDataType))
                    {
                        bool isEmpty = true;
                        if (template.MasterData.Table.Columns.Contains(field.FieldName))
                        {
                            string fieldValue = Convert.ToString(template.MasterData[field.FieldName]).Trim();
                            isEmpty = FieldIsEmpty(field, fieldValue);
                        }
                        if (isEmpty)
                        {
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add(String.Format("{0} - Validation", template.MasterData[activity.Template.MessageField])  , String.Format("\"{0}\" is required.", field.DisplayName));
                        }
                    }
                    else
                    {
                        bool isEmpty = true;
                        string tableName = field.SubDataType.ToUpper();
                        if (template.SubData.ContainsKey(tableName))
                        {
                            DataTable subDt = template.SubData[tableName];
                            if (subDt.Rows.Count > 0)
                            {
                                isEmpty = false;
                                for (int i = 0; i < subDt.Rows.Count; i++)
                                {
                                    string fieldValue = Convert.ToString(subDt.Rows[i][field.FieldName]).Trim();
                                    if (FieldIsEmpty(field, fieldValue))
                                    {
                                        sysMsg.isPass = false;
                                        sysMsg.Messages.Add(String.Format("{0} - Validation", template.MasterData[activity.Template.MessageField]), String.Format("\"{0}(line:{1})\" is required.", field.DisplayName, i+1));
                                    }
                                }
                            }
                        }
                        if (isEmpty)
                        {
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add(String.Format("{0} - Validation", template.MasterData[activity.Template.MessageField]), String.Format("\"{0}\" is required.", field.DisplayName));
                        }
                    }
                }
            }
        }

        private static bool FieldIsEmpty(WFActivityField field, string fieldValue)
        {
            if (String.IsNullOrEmpty(fieldValue))
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
                        if (dValue == 0)
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        public static List<WFProcessLog> GetProcessLog(int templateId, int entityId)
        {
            string strSql = "SELECT t1.*, t2.Name AS ActionUserName FROM SYS_WFProcessLog t1 LEFT JOIN Access_User t2 ON t1.ActionUser = t2.UID WHERE EntityID = @EntityID AND TemplateID = @TemplateID ORDER BY t1.ID ASC";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@TemplateID", templateId),
                new SqlParameter("@EntityID", entityId)
            }).Tables[0];

            List<WFProcessLog> AllPrcLog = ModelHandler<WFProcessLog>.FillModel(dt);
            List<WFProcessLog> lstPrcLog = null;
            List<WFProcessLog> lstTemPrcLog = null;

            if (AllPrcLog != null)
            {
                lstPrcLog = new List<WFProcessLog>();
                foreach (WFProcessLog log in AllPrcLog)
                {
                    if (log.SubActivityType == 0)
                    {
                        lstPrcLog.Add(log);
                        lstTemPrcLog = null;
                    }
                    else if (log.SubActivityType == 1 || log.SubActivityType == 3)
                    {
                        foreach (WFProcessLog l in lstTemPrcLog)
                        {
                            l.ParentProcessLog = log;
                            log.SubProcessLog.Add(l);
                        }
                        lstTemPrcLog = null;
                        lstPrcLog.Add(log);
                    }
                    else if (log.SubActivityType == 2 || log.SubActivityType == 4)
                    {
                        if (lstTemPrcLog == null)
                        {
                            lstTemPrcLog = new List<WFProcessLog>();
                        }

                        if (log.SubActivityType == 2) log.SubActivityComplated = true;
                        lstTemPrcLog.Add(log);
                    }
                }

                if (lstTemPrcLog != null && lstPrcLog.Count > 0)
                {
                    WFProcessLog lastProcLog = lstPrcLog[lstPrcLog.Count - 1];
                    foreach (WFProcessLog l in lstTemPrcLog)
                    {
                        lastProcLog.CurrentComplatedProcessLog.Add(l);
                        foreach (WFProcessLog ll in lastProcLog.SubProcessLog)
                        {
                            if (ll.ToActivityID == l.FromActivityID)
                            {
                                ll.SubActivityComplated = true;
                            }
                        }
                    }
                }
            }
            return lstPrcLog;
        }

        public static string GetCurrentSubComplatedTimelineString(WFProcessLog complatedLog)
        {
            WFActivity activity = new WFActivity(complatedLog.FromActivityID);
            return "{\"activityName\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activity.Name) +
                                ",\"actionUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(complatedLog.ActionUserName) +
                                ", \"actionTime\":\"" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", complatedLog.ActionTime) + "\"" +
                                ", \"sort\":" + activity.Sort +
                                ",\"activityType\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activity.ActivityType) +
                                ",\"subActivityComplated\":" + (complatedLog.SubActivityComplated ? "1" : "0") + "}";
        }

        public static string GetSubProLogTimelineString(WFProcessLog processLog)
        {
            if (processLog.SubActivityComplated)
            {
                return processLog.FromActivity == null ? "" : "{\"activityName\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.FromActivity.Name) +
                                ",\"actionUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.ActionUserName) +
                                ", \"actionTime\":\"" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", processLog.ActionTime) + "\"" +
                                ", \"sort\":" + processLog.FromActivity.Sort +
                                ",\"activityType\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.ToActivity.ActivityType) +
                                ",\"subActivityComplated\":" + (processLog.SubActivityComplated ? "1" : "0") + "}";
            }
            else
            {
                return processLog.ToActivity == null ? "" : "{\"activityName\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.ToActivity.Name) +
                            ",\"ownerUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.ToActivity.OwnerUser) +
                            ",\"ccUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(processLog.ToActivity.CCUser) +
                            ",\"sort\":" + processLog.ToActivity.Sort +
                            ",\"subActivityComplated\":" + (processLog.SubActivityComplated ? "1" : "0") + "}";
            }
        }

        public static string GetTimelineJSON(List<WFProcessLog> prcLog)
        {
            string jsonData = "{";

            if (prcLog != null)
            {
                jsonData += "\"activites\":[";
                for (int i = 0; i < prcLog.Count; i++)
                {
                    WFProcessLog log = prcLog[i];
                    string activityName, activityType;
                    int activitySort;
                    if (log.FromActivity == null)
                    {
                        activityName = "Workflow Start";
                        activityType = "Normal"; ;
                        activitySort = 0;
                    }
                    else
                    {
                        activityName = log.FromActivity.Name;
                        activityType = log.ToActivity.ActivityType;
                        activitySort = log.FromActivity.Sort;
                    }
                    jsonData += "{\"activityName\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activityName) +
                    ",\"actionUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(log.ActionUserName) +
                    ", \"actionTime\":\"" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", log.ActionTime) + "\"" +
                    ", \"sort\":" + activitySort +
                    ",\"activityType\":" + Newtonsoft.Json.JsonConvert.SerializeObject(activityType);

                    if (log.SubActivityType == 1 && log.SubProcessLog.Count > 0)
                    {
                        jsonData += ",\"subActivities\":[";
                        foreach (WFProcessLog l in log.SubProcessLog)
                        {
                            string sptls = GetSubProLogTimelineString(l);
                            if (!String.IsNullOrWhiteSpace(sptls))
                            {
                                jsonData += (sptls + ",");
                            }
                        }
                        jsonData = jsonData.TrimEnd(',');
                        jsonData += "]";
                    }

                    jsonData += "},";
                }

                jsonData = jsonData.TrimEnd(',');

                jsonData += "]";

                WFProcessLog lastProLog = prcLog[prcLog.Count - 1];
                WFActivity lastActivity = lastProLog.ToActivity;
                jsonData += ",\"currentActivity\":{ \"activityName\":" + Newtonsoft.Json.JsonConvert.SerializeObject(lastActivity.Name) +
                            ",\"ownerUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(lastActivity.OwnerUser) +
                            ",\"ccUser\":" + Newtonsoft.Json.JsonConvert.SerializeObject(lastActivity.CCUser) +
                            ", \"sort\":" + lastActivity.Sort;
                if (lastProLog.SubActivityType == 3)
                {
                    jsonData += ",\"subActivities\":[";
                    foreach (WFProcessLog l in lastProLog.SubProcessLog)
                    {
                        if (!l.SubActivityComplated)
                        {
                            string sptls = GetSubProLogTimelineString(l);
                            if (!String.IsNullOrWhiteSpace(sptls))
                            {
                                jsonData += (sptls + ",");
                            }
                        }
                    }
                    foreach (WFProcessLog l in lastProLog.CurrentComplatedProcessLog)
                    {
                        jsonData += (GetCurrentSubComplatedTimelineString(l) + ",");
                    }
                    jsonData = jsonData.TrimEnd(',');
                    jsonData += "]";
                }
                jsonData += "}";
            }

            jsonData += "}";
            return jsonData;
        }

        public static List<WFActivity> GetActivities(int templateId)
        {
            string strSql = "SELECT * FROM SYS_WFActivity WHERE TemplateID = @TemplateID AND ParentID = 0 AND Status <> 0 ORDER BY Sort ASC";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@TemplateID", templateId)).Tables[0];
            return ModelHandler<WFActivity>.FillModel(dt);
        }

        public static List<WFUser> GetDelegateUsers(Dictionary<string, WFUser> wfUser)
        {
            List<WFUser> lstUser = new List<WFUser>();
            if (wfUser != null)
            {
                foreach (KeyValuePair<string, WFUser> kvUser in wfUser)
                {
                    string strSql = String.Format("SELECT t2.UID AS UserID,t2.Name AS DisplayName,t2.Email,CAST({0} AS BIT) AS IsKeyUser, CAST({1} AS BIT) AS IsApprover FROM SGP_Delegation t1, Access_User t2 WHERE t1.ToUser = t2.UID AND t1.FromUser = @UID", kvUser.Value.IsKeyUser == true ? "1" : "0", kvUser.Value.IsApprover == true ? "1" : "0");
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@UID", kvUser.Value.UserID)).Tables[0];
                    List<WFUser> deleUser = ModelHandler<WFUser>.FillModel(dt);
                    if (deleUser != null)
                    {
                        lstUser.AddRange(deleUser);
                    }
                }
            }
            return lstUser;
        }
    }
}
