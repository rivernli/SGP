using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.WF
{
    public enum WorkflowStatus
    {
        NotStart,
        InProcess,
        Finished
    }

    public class WFTemplate
    {
        public int ID { get; protected set; }
        public string Name { get; protected set; }
        public string ViewName { get; protected set; }
        public string ViewKey { get; protected set; }
        public string TableName { get; protected set; }
        public string TableKey { get; protected set; }
        public string MessageField { get; protected set; }
        public string rfqNumber { get; set; }
        public int EntityID { get; protected set; }
        public WFActivity FromActivity { get; set; }
        public WFActivity ToActivity { get; set; }
        private WFActivity _firstActivity;
        private WFActivity _lastActivity;
        private WFActivity _currentActivity;
        private WFActivity _prevActivity;
        private WFActivity _nextActivity;
        private WFActivityCollection _activities;
        private DataRow _masterData;
        private Dictionary<string, DataTable> _subData;
        private string p;
        private int? nullable;

        public WorkflowStatus Status
        {
            get
            {
                if (CurrentActivity == null)
                {
                    return WorkflowStatus.NotStart;
                }
                else if (CurrentActivity.ID == LastActivity.ID)
                {
                    return WorkflowStatus.Finished;
                }
                else
                {
                    return WorkflowStatus.InProcess;
                }
            }
        }
        
        public WFActivity FirstActivity
        {
            get
            {
                if (_firstActivity == null)
                {
                    if (Activities != null && Activities.Count > 0)
                    {
                        _firstActivity = Activities[0];
                    }
                }
                return _firstActivity;
            }
        }

        public WFActivity LastActivity
        {
            get
            {
                if (_lastActivity == null)
                {
                    if (Activities != null && Activities.Count > 0)
                    {
                        _lastActivity = Activities[Activities.Count - 1];
                    }
                }
                return _lastActivity;
            }
        }

        public WFActivity PrevActivity
        {
            get
            {
                if (Activities != null && CurrentActivity != null)
                {
                    int currIndex = Activities.IndexOf(CurrentActivity);
                    if (currIndex > 0)
                    {
                        _prevActivity = Activities[currIndex - 1];
                    }
                }
                return _prevActivity;
            }
        }

        public WFActivity CurrentActivity
        {
            get
            {
                if (Activities != null && this.MasterData != null)
                {
                    _currentActivity = Activities.Get(ParseHelper.Parse<int>(this.MasterData["ActivityID"]));
                }
                return _currentActivity;
            }
        }

        public WFActivity NextActivity
        {
            get
            {
                if (Activities != null && CurrentActivity != null)
                {
                    int nextIndex = Activities.IndexOf(CurrentActivity) + 1;
                    if (nextIndex < Activities.Count)
                    {
                        _nextActivity = Activities[nextIndex];
                    }
                }
                return _nextActivity;
            }
        }

        public WFActivityCollection Activities
        {
            get
            {
                if (_activities == null)
                {
                    _activities = new WFActivityCollection();
                    string strSql = "SELECT * FROM SYS_WFActivity WHERE TemplateID = @TemplateID AND ParentID = 0 AND Status <> 0 ORDER BY Sort ASC";
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@TemplateID", this.ID)).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        _activities.Add(WFActivity.FillActivity(dr, this));
                    }
                }
                return _activities;
            }
        }

        public DataRow MasterData
        {
            get
            {
                if (_masterData == null)
                {
                    string strSql = string.Empty;
                    if (this.Name == "SUPPLIERWF" && !string.IsNullOrWhiteSpace(this.rfqNumber))
                    {
                        strSql = String.Format("SELECT * FROM {0} WHERE {1} = @entityId AND NVARCHAR1 = @Number",
                                                this.ViewName, this.ViewKey);
                        DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@entityId", this.EntityID),
                                                                new SqlParameter("@Number", this.rfqNumber)).Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            _masterData = dt.Rows[0];
                        }
                    }
                    else
                    {
                        strSql = String.Format("SELECT * FROM {0} WHERE {1} = @entityId", this.ViewName, this.ViewKey);
                        DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@entityId", this.EntityID)).Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            _masterData = dt.Rows[0];
                        }
                    }
                }
                return _masterData;
            }
            set
            {
                _masterData = value;
            }
        }

        public Dictionary<string, DataTable> SubData
        {
            get
            {
                if (_subData == null)
                {
                    string strSql = "SELECT * FROM SYS_WFSubData WHERE TemplateID=@TemplateID";
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@TemplateID", this.ID)).Tables[0];
                    _subData = new Dictionary<string, DataTable>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        string tableName = Convert.ToString(dr["TableName"]).ToUpper();
                        strSql = "SELECT * FROM SGP_SubData WHERE EntityName = @EntityName AND EntityID=@EntityID ORDER BY DataIndex";
                        DataTable sdt = DbHelperSQL.Query(strSql, new SqlParameter("@EntityName", tableName), new SqlParameter("@EntityID", this.EntityID)).Tables[0];
                        SubData.Add(tableName, sdt);
                    }
                }
                return _subData;
             }
        }

        public WFTemplate(int templateId, int entityId)
        {
            this.EntityID = entityId;
            this.ID = templateId;
            string strSql = "SELECT * FROM SYS_WFTemplate WHERE ID=@ID AND Status <> 0";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", templateId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                InitBaseData(dt.Rows[0]);
            }
            else
            {
                throw new Exception(String.Format("can not find the template id [{0}]", templateId));
            }
        }

        public WFTemplate(int templateId, int entityId, string rfqNumber)
        {
            this.EntityID = entityId;
            this.ID = templateId;
            this.rfqNumber = rfqNumber;
            string strSql = "SELECT * FROM SYS_WFTemplate WHERE ID=@ID AND Status <> 0";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", templateId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                InitBaseData(dt.Rows[0]);
            }
            else
            {
                throw new Exception(String.Format("can not find the template id [{0}]", templateId));
            }
        }

        public WFTemplate(string templateName, int entityId)
        {
            this.EntityID = entityId;
            string strSql = "SELECT * FROM SYS_WFTemplate WHERE Name=@Name AND Status <> 0";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", templateName)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                InitBaseData(dt.Rows[0]);
            }
            else
            {
                throw new Exception(String.Format("can not find the template [{0}]", templateName));
            }
        }

        public WFTemplate(string templateName, int entityId, string rfqNumber)
        {
            this.EntityID = entityId;
            this.rfqNumber = rfqNumber;
            string strSql = "SELECT * FROM SYS_WFTemplate WHERE Name=@Name AND Status <> 0";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", templateName)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                InitBaseData(dt.Rows[0]);
            }
            else
            {
                throw new Exception(String.Format("can not find the template [{0}]", templateName));
            }
        }

        public WFTemplate(string p, int? nullable)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.nullable = nullable;
        }

        private void InitBaseData(DataRow dr)
        {
            this.ID = Convert.ToInt32(dr["ID"]);
            this.Name = Convert.ToString(dr["Name"]);
            this.ViewName = Convert.ToString(dr["ViewName"]);
            this.ViewKey = Convert.ToString(dr["ViewKey"]);
            this.TableName = Convert.ToString(dr["TableName"]);
            this.TableKey = Convert.ToString(dr["TableKey"]);
            this.MessageField = Convert.ToString(dr["MessageField"]);
        }

        public SystemMessages Run()
        {
            return Run(0);
        }

        public SystemMessages Run(int fromChildActivityId)
        {
            return Run(fromChildActivityId, true);
        }

        public SystemMessages Run(int fromChildActivityId, bool checkData)
        {
            int nextId = 0;
            if (this.Status == WorkflowStatus.NotStart)
            {
                nextId = FirstActivity.ID;
            }
            else if (this.Status == WorkflowStatus.InProcess)
            {
                nextId = NextActivity.ID;
            }
            return Skip(nextId, fromChildActivityId, checkData, true);
        }

        public SystemMessages Skip(int toActivityId)
        {
            return Skip(toActivityId, 0);
        }

        public SystemMessages Skip(int toActivityId, int fromChildActivityId, params int[] toChildActivityId)
        {
            return Skip(toActivityId, fromChildActivityId, true, true, toChildActivityId);
        }

        public SystemMessages Skip(int toActivityId, int fromChildActivityId, bool checkData, bool waitAllChildComplated, params int[] toChildActivityId) 
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();

            if (this.Status == WorkflowStatus.Finished)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add(String.Format("{0}", this.MasterData[this.MessageField]), String.Format("can not submit this record, because the workflow status is \"{0}\".", LastActivity.Name));
            }

            if (sysMsg.isPass && CurrentActivity != null && fromChildActivityId == 0 && CurrentActivity.CurrentChildActivities.Count > 0)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add(String.Format("{0}", this.MasterData[this.MessageField]), String.Format("unknow sub activityId [{0}].", fromChildActivityId));
            }

            if (sysMsg.isPass)
            {
                WFActivity nextActivity = Activities.Get(toActivityId);
                if (nextActivity != null)
                {
                    WFActivity fromActivity = null;
                    if (fromChildActivityId == 0)
                    {
                        fromActivity = this.CurrentActivity;
                        if (this.Name == "SUPPLIERWF")
                        {
                            fromActivity = this.FirstActivity;
                        }
                    }
                    else if (this.CurrentActivity != null)
                    {
                        fromActivity = this.CurrentActivity.CurrentChildActivities.Get(fromChildActivityId);
                        if (fromActivity == null)
                        {
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add(String.Format("{0}", this.MasterData[this.MessageField]), "can not submit this record, because this sub stage is complated.");
                        }
                    }

                    if (fromActivity != null && sysMsg.isPass)
                    {
                        fromActivity.CheckUserPermissions(sysMsg, AccessControl.CurrentLogonUser.Uid);
                        if (sysMsg.isPass)
                        {
                            //if (checkData || this.Name == "SUPPLIERWF")
                            //{
                            //    fromActivity.CheckData(sysMsg);
                            //}
                            //else 
                            if (checkData && ((fromActivity.CheckFieldType == CheckFieldType.Submit && nextActivity.Sort > CurrentActivity.Sort)
                                || (fromActivity.CheckFieldType == CheckFieldType.Reject && nextActivity.Sort < CurrentActivity.Sort)
                                || fromActivity.CheckFieldType == CheckFieldType.Always))
                            {
                                fromActivity.CheckData(sysMsg);
                            }

                            if (sysMsg.isPass && this.CurrentActivity.CurrentChildActivities.Count > 0)
                            {
                                SubmitChildActivity(sysMsg, fromActivity, nextActivity);
                            }
                        }
                    }

                    if (sysMsg.isPass && (CurrentActivity == null || nextActivity.Sort < CurrentActivity.Sort || !waitAllChildComplated || CurrentActivity.AllChildCompleted()))
                    {
                        SubmitToNextActivity(sysMsg, nextActivity, toChildActivityId);
                    }
                }
                else
                {
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Workflow Exception", "can not find any activities.");
                }
            }

            return sysMsg;
        }

        private void SubmitChildActivity(SystemMessages sysMsg, WFActivity fromActivity, WFActivity nextActivity)
        {
            string strUserSql = String.Format("DELETE FROM SGP_CurrentUserTask WHERE TemplateID={0} AND EntityId={1} AND ActivityID = {2}", this.ID, EntityID, fromActivity.ID);
            DbHelperSQL.ExecuteSql(strUserSql);

            AddProcessLog(fromActivity.ID, nextActivity.ID, 2);

            this.FromActivity = CurrentActivity;
            this.ToActivity = nextActivity;
            _masterData = null;

            fromActivity.DoAction();

            if (this.CurrentActivity.CurrentUserChildActivities.Contains(fromActivity))
            {
                this.CurrentActivity.CurrentUserChildActivities.Remove(fromActivity);
            }
        }

        private void SubmitToNextActivity(SystemMessages sysMsg, WFActivity nextActivity, params int[] toChildActivityId)
        {
            string strSql = String.Format("DELETE FROM SGP_CurrentUserTask WHERE TemplateID={0} AND EntityId={1}", this.ID, EntityID);
            DbHelperSQL.ExecuteSql(strSql);
            string extSql = "";

            if (nextActivity.ID == LastActivity.ID)
            {
                extSql = ",StatusID = 9";
            }
            else
            {
                if (nextActivity.ChildActivities.Count > 0)
                {
                    if (toChildActivityId != null && toChildActivityId.Length > 0)
                    {
                        for (int i = 0; i < toChildActivityId.Length; i++)
                        {
                            WFActivity ncta = nextActivity.MatchChildActivities.Get(toChildActivityId[i]);
                            if (ncta != null)
                            {
                                AddCurrentUserTask(ncta);
                            }
                        }
                    }
                    else
                    {
                        foreach (WFActivity nca in nextActivity.MatchChildActivities)
                        {
                            AddCurrentUserTask(nca);
                        }
                    }
                }
                else
                {
                    AddCurrentUserTask(nextActivity);
                }
            }

            if (nextActivity.ChildActivities.Count > 0)
            {
                if (toChildActivityId != null && toChildActivityId.Length > 0)
                {
                    for (int i = 0; i < toChildActivityId.Length; i++)
                    {
                        
                        
                        WFActivity ncta = nextActivity.MatchChildActivities.Get(toChildActivityId[i]);
                        if (ncta != null)
                        {
                            AddProcessLog(nextActivity.ID, ncta.ID, 4);
                        }
                    }
                }
                else
                {
                    foreach (WFActivity nca in nextActivity.MatchChildActivities)
                    {
                        AddProcessLog(nextActivity.ID, nca.ID, 4);
                    }
                }

                AddProcessLog(CurrentActivity == null ? 0 : CurrentActivity.ID, nextActivity.ID, 3);
            }
            else
            {
                AddProcessLog(CurrentActivity == null ? 0 : CurrentActivity.ID, nextActivity.ID, (CurrentActivity != null && CurrentActivity.ChildActivities.Count > 0) ? 1 : 0);
            }

            strSql = String.Format("UPDATE {0} SET ActivityID = @ActivityID, TemplateID = @TemplateID {2} WHERE {1} = @{1}", this.TableName, this.TableKey, extSql);
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[]{
                    new SqlParameter("@ActivityID", nextActivity.ID),
                    new SqlParameter("@TemplateID", this.ID),
                    new SqlParameter("@" + this.TableKey, this.EntityID)
                });

            this.FromActivity = CurrentActivity;
            this.ToActivity = nextActivity;
            _masterData = null;

            nextActivity.DoAction();
            foreach (WFActivity nca in nextActivity.MatchChildActivities)
            {
                nca.DoAction();
            }
        }

        private void AddCurrentUserTask(WFActivity activity)
        {
            string strSql = String.Format("INSERT INTO SGP_CurrentUserTask(TemplateID,ActivityID,EntityID,UID) VALUES({0},{1},{2},@UID)", this.ID, activity.ID, this.EntityID);
            foreach (KeyValuePair<string, WFUser> kvUser in activity.WFUsers)
            {
                if (kvUser.Value.IsKeyUser || kvUser.Value.IsApprover)
                {
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter[] { new SqlParameter("@UID", kvUser.Value.UserID) });
                }
            }
        }

        private void AddProcessLog(int fromId, int toId, int subType)
        {
            string strSql = "INSERT INTO SYS_WFProcessLog(EntityID,TemplateID,FromActivityID,ToActivityID,SubActivityType,ActionUser) VALUES(@EntityID,@TemplateID,@FromActivityID,@ToActivityID,@SubActivityType,@ActionUser)";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[]{
                new SqlParameter("@EntityID", this.EntityID),
                new SqlParameter("@TemplateID", this.ID),
                new SqlParameter("@FromActivityID", fromId),
                new SqlParameter("@ToActivityID", toId),
                new SqlParameter("@SubActivityType", subType),
                new SqlParameter("@ActionUser", AccessControl.CurrentLogonUser.Uid),
            });
        }
    }
}
