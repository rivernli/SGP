using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using SGP.DBUtility;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.WF
{
    public enum CheckFieldType
    {
        Submit = 1,
        Reject = 2,
        Always = 3
    }

    public class WFActivity
    {
        public const string WFACTIVITY_TYPE_NORMAL = "Normal";
        public const string WFACTIVITY_TYPE_FINISH = "Finish";

        public WFActivity ParentActivity { get; set; }
        public WFTemplate Template { get; set; }
        public int ID { get; set; }
        public int ParentID { get; set; }
        public int TemplateID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ActivityType { get; set; }
        public CheckFieldType CheckFieldType { get; set; }
        public int ConditionGroupID { get; set; }
        public int Sort { get; set; }
        private string _ownerUser = null;
        private string _ccUser = null;
        private WFActivityCollection _childActivities;
        private WFActivityCollection _matchChildActivities;
        private WFActivityCollection _currentChildActivities;
        private WFActivityCollection _currentUserChildActivities;
        private WFCondition _condition;

        public WFActivityCollection ChildActivities
        {
            get
            {
                if (_childActivities == null)
                {
                    _childActivities = new WFActivityCollection();
                    string strSql = "SELECT * FROM SYS_WFActivity WHERE ParentID = @ParentID AND Status <> 0 ORDER BY Sort ASC";
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ParentID", this.ID)).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        WFActivity act = FillActivity(dr, this.Template);
                        act.ParentActivity = this;
                        _childActivities.Add(act);
                    }
                }
                return _childActivities;
            }
        }

        public WFActivityCollection MatchChildActivities
        {
            get
            {
                if (_matchChildActivities == null)
                {
                    _matchChildActivities = new WFActivityCollection();
                    foreach (WFActivity ca in ChildActivities)
                    {
                        if (ca.Condition != null)
                        {
                            if (ca.Condition.Compare())
                            {
                                _matchChildActivities.Add(ca);
                            }
                        }
                        else
                        {
                            _matchChildActivities.Add(ca);
                        }
                    }
                    
                }
                return _matchChildActivities;
            }
        }

        public WFActivityCollection CurrentChildActivities
        {
            get
            {
                if (_currentChildActivities == null)
                {
                    _currentChildActivities = new WFActivityCollection();
                    string strSql = "SELECT * FROM SYS_WFActivity t1 WHERE TemplateID = @TemplateID AND ParentID = @ParentID AND Status <> 0 AND EXISTS(SELECT * FROM SGP_CurrentUserTask t2 WHERE t2.ActivityID = t1.ID AND t2.EntityID = @EntityID) ORDER BY Sort ASC";
                    DataTable dt = DbHelperSQL.Query(strSql,
                            new SqlParameter[]{
                            new SqlParameter("@TemplateID", this.Template.ID),
                            new SqlParameter("@EntityID", this.Template.EntityID),
                            new SqlParameter("@ParentID", this.ID),
                        }
                    ).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        WFActivity act = FillActivity(dr, this.Template);
                        act.ParentActivity = this;
                        _currentChildActivities.Add(act);
                    }
                }
                return _currentChildActivities;
            }
        }

        public WFActivityCollection CurrentUserChildActivities
        {
            get
            {
                if (_currentUserChildActivities == null)
                {
                    _currentUserChildActivities = new WFActivityCollection();
                    string uid = AccessControl.CurrentLogonUser.Uid;
                    foreach (WFActivity ca in CurrentChildActivities)
                    {
                        if (ca.CheckUserPermissions(uid))
                        {
                            _currentUserChildActivities.Add(ca);
                        }
                    }
                }

                return _currentUserChildActivities;
            }
        }

        public WFCondition Condition
        {
            get
            {
                if (_condition == null)
                {
                    if (ConditionGroupID > 0)
                    {
                        _condition = new WFCondition(ConditionGroupID, this.Template.MasterData);
                    }
                }

                return _condition;
            }
        }

        public string OwnerUser
        {
            get
            {
                if (_ownerUser == null)
                {
                    SetUserList();
                }

                return _ownerUser;
            }
        }

        public string CCUser
        {
            get
            {
                if (_ccUser == null)
                {
                    SetUserList();
                }

                return _ccUser;
            }
        }

        private Dictionary<string, WFUser> _wfusers;
        public Dictionary<string, WFUser> WFUsers
        {
            get
            {
                if (_wfusers == null || _wfusers.Count <= 0)
                {
                    _wfusers = new Dictionary<string, WFUser>(StringComparer.OrdinalIgnoreCase);
                    MergeUser(_wfusers, GetStaticUsers());
                    MergeUser(_wfusers, GetRoleUsers());
                    if (Template != null && Template.EntityID > 0)
                    {
                        MergeUser(_wfusers, GetEntityUsers());
                    }
                    MergeUser(_wfusers, GetExtUsers());
                    MergeUser(_wfusers, WFHelper.GetDelegateUsers(_wfusers));
                }
                return _wfusers;
            }
        }

        public bool IsOwner(string uid)
        {
            foreach (KeyValuePair<string, WFUser> kvUser in WFUsers)
            {
                if (kvUser.Key == uid && (kvUser.Value.IsApprover || kvUser.Value.IsKeyUser))
                {
                    return true;
                }
            }
            return false;
        }

        public WFActivity() { }

        public WFActivity(int activityId)
        {
            string strSql = "SELECT * FROM SYS_WFActivity WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", activityId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                this.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                this.Name = Convert.ToString(dt.Rows[0]["Name"]);
                this.ParentID = Convert.ToInt32(dt.Rows[0]["ParentID"]);
                this.TemplateID = Convert.ToInt32(dt.Rows[0]["TemplateID"]);
                this.Description = Convert.ToString(dt.Rows[0]["Description"]);
                this.ActivityType = Convert.ToString(dt.Rows[0]["ActivityType"]);
                this.CheckFieldType = (CheckFieldType)dt.Rows[0]["CheckFieldType"];
                this.ConditionGroupID = Convert.ToInt32(dt.Rows[0]["ConditionGroupID"]);
                this.Sort = Convert.ToInt32(dt.Rows[0]["Sort"]);
            }
            else
            {
                throw new Exception(String.Format("can not find activityId [{0}] from SYS_WFActivity.", activityId));
            }
        }

        private void SetUserList()
        {
            _ownerUser = "";
            _ccUser = "";
            foreach (KeyValuePair<string, WFUser> kvUser in WFUsers)
            {
                if (kvUser.Value.IsApprover || kvUser.Value.IsKeyUser)
                {
                    _ownerUser += kvUser.Value.DisplayName + ";";
                }
                else
                {
                    _ccUser += kvUser.Value.DisplayName + ";";
                }
            }

            _ownerUser = _ownerUser.TrimEnd(';');
            _ccUser = _ccUser.TrimEnd(';');
        }

        public bool CheckUserPermissions(string uid)
        {
            return AccessControl.CurrentLogonUser.IsAdmin() || (WFUsers != null && IsOwner(uid));
            //return (WFUsers != null && IsOwner(uid));
        }

        public void AddNoPermissionMessage(SystemMessages sysMsg)
        {
            string errMessage = String.Format("You have no permission to submit this, the current user for [{0}] are:<br>Owner:{1}", this.Name, OwnerUser);
            if (CCUser != "")
            {
                errMessage += "<br>CC:" + CCUser;
            }
            sysMsg.isPass = false;
            sysMsg.Messages.Add(String.Format("{0}", this.Template.MasterData[this.Template.MessageField]), errMessage);
        }

        public void CheckUserPermissions(SystemMessages sysMsg, string uid)
        {
            if (!CheckUserPermissions(uid))
            {
                AddNoPermissionMessage(sysMsg);
            }
        }

        public void CheckData(SystemMessages sysMsg)
        {
            List<WFActivityField> listFields = GetCheckFields();
            if (listFields != null && listFields.Count > 0)
            {
                if (this.Template.MasterData != null)
                {
                    WFHelper.CheckField(this.Template, listFields, sysMsg, this);
                }
                else
                {
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Workflow Exception", String.Format("can not find record[{0}] from [{1}]", this.Template.ViewName));
                }
            }
            else
            {
                sysMsg.isPass = true;
            }
        }

        public bool AllChildCompleted()
        {
            if (ChildActivities.Count > 0)
            {
                string strSql = "SELECT COUNT(*) FROM SGP_CurrentUserTask t1 INNER JOIN SYS_WFActivity t2 ON t1.ActivityID = t2.ID WHERE t1.TemplateID = @TemplateID AND t1.EntityID = @EntityID AND t2.ParentID = @ParentID AND t2.Status <> 0";
                SqlParameter[] ps =
                {
                    new SqlParameter("@TemplateID", this.Template.ID),
                    new SqlParameter("@EntityID", this.Template.EntityID),
                    new SqlParameter("@UID", AccessControl.CurrentLogonUser.Uid),
                    new SqlParameter("@ParentID", this.ID),
                };
                return DbHelperSQL.GetSingle<int>(strSql, ps) == 0;
            }
            else
            {
                return true;
            }
        }

        public void DoAction()
        {
            string strSql = "SELECT ActionPath FROM SYS_WFAction WHERE ActivityID = @ActivityID ORDER BY Sort ASC";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ActivityID", this.ID)).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                string actionPath = Convert.ToString(dr["ActionPath"]).Trim();
                Type type = Type.GetType(actionPath);
                if (type != null)
                {
                    Action.IAction action = (Action.IAction)Activator.CreateInstance(type, null);
                    action.DoAction(this);
                }
            }
        }

        public void MergeUser(Dictionary<string, WFUser> dicUsers, List<WFUser> listUsers)
        {
            if (listUsers != null)
            {
                foreach (WFUser wfUser in listUsers)
                {
                    MergeUser(dicUsers, wfUser);
                }
            }
        } 

        private void MergeUser(Dictionary<string, WFUser> dicUsers, WFUser wfUser) 
        {
            if (dicUsers.Keys.Contains(wfUser.UserID))
            {
                if (wfUser.IsApprover)
                {
                    dicUsers[wfUser.UserID].IsApprover = true;
                }
                if (wfUser.IsKeyUser)
                {
                    dicUsers[wfUser.UserID].IsKeyUser = true;
                }
            }
            else
            {
                dicUsers.Add(wfUser.UserID, wfUser);
            }
        }

        public List<WFActivityField> GetCheckFields()
        {
            string strSql = "SELECT t1.ID, t1.IsRequired, t2.ID AS FieldID, t2.FieldName, t2.DisplayName, t2.DataType, t2.SubDataType, t2.KeyValueSource, t1.Sort FROM SYS_WFActivityField t1, SYS_FieldInfo t2 WHERE ActivityID = @ActivityID AND t1.FieldID = t2.ID AND t2.Status <> 0 ORDER BY t1.Sort ASC";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ActivityID", this.ID)).Tables[0];
            return ModelHandler<WFActivityField>.FillModel(dt);
        }

        public List<WFActivityField> GetCheckFields(bool Sorted)
        {
            List<WFActivityField> lstField = GetCheckFields();
            if (Sorted && lstField != null && lstField.Count > 1)
            {
                Comparison<WFActivityField> com = new Comparison<WFActivityField>(Compare);
                lstField.Sort(com);
            }

            return lstField;
        }

        private int Compare(WFActivityField field1, WFActivityField field2)
        {
            return String.Compare(field1.DisplayName, field2.DisplayName);
        }

        public List<WFUser> GetStaticUsers()
        {
            string strSql = "SELECT t1.ID,t2.UID AS UserID,t2.Name AS DisplayName,t2.Email,t1.IsKeyUser,t1.IsApprover FROM SYS_WFActivityUser t1,Access_User t2 WHERE t1.ActivityID = @ID AND t1.UID =  t2.UID AND UserType = 'Static'";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
            return ModelHandler<WFUser>.FillModel(dt);
        }

        public List<WFUser> GetEntityUsers()
        {
            List<WFUser> listDynamicUser = new List<WFUser>();
            string strSql = "SELECT ID,IsKeyUser,IsApprover,(SELECT FieldName FROM SYS_FieldInfo WHERE ID = SYS_WFActivityUser.UID) UID FROM SYS_WFActivityUser WHERE ActivityID = @ID AND UserType = 'Entity'";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                String displayName = Convert.ToString(this.Template.MasterData[Convert.ToString(dr["UID"])]);
                strSql = String.Format("SELECT {0} AS ID,UID AS UserID,Name AS DisplayName,Email,CAST({1} AS BIT) AS IsKeyUser, CAST({2} AS BIT) AS IsApprover FROM Access_User WHERE Name = @Name", Convert.ToInt32(dr["ID"]), Convert.ToInt32(dr["IsKeyUser"]), Convert.ToInt32(dr["IsApprover"]));
                DataTable dtUser = DbHelperSQL.Query(strSql, new SqlParameter("@Name", displayName)).Tables[0];

                foreach (DataRow drUser in dtUser.Rows) 
                {
                    WFUser wfUser = ModelHandler<WFUser>.FillModel(drUser);
                    if (wfUser != null)
                    {
                        listDynamicUser.Add(wfUser);
                    }
                }
            }
            return listDynamicUser;
        }

        public List<WFUser> GetRoleUsers()
        {
            string strSql = @"SELECT t4.ID,t1.UID AS UserID,t1.Name AS DisplayName,t1.Email,t4.IsKeyUser,t4.IsApprover FROM Access_User t1, Access_Role_MappingUser t2, Access_Role t3, SYS_WFActivityUser t4
                              WHERE t1.UID = t2.UID AND t2.RoleID = t3.RoleID AND t3.Name = t4.UID AND t2.IsMapping = 1 AND ActivityID = @ID AND UserType = 'Role'";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
            return ModelHandler<WFUser>.FillModel(dt);
        }

        public List<WFUser> GetExtUsers()
        {
            List<WFUser> listUsers = new List<WFUser>();

            if (this.Template.MasterData != null)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(System.Web.HttpContext.Current.Server.MapPath("~/Content/Template/User/ExtUser.xml"));
                XmlNode root = dom.SelectSingleNode("Root");
                XmlNodeList nodes = root.ChildNodes;
                foreach (XmlNode node in nodes)
                {
                    bool isMatchAct = false;
                    XmlNodeList nodeActivities = node.SelectSingleNode("Activities").ChildNodes;
                    foreach (XmlNode nodeAct in nodeActivities)
                    {
                        string strActId = nodeAct.InnerText.Trim();
                        if (!string.IsNullOrWhiteSpace(strActId))
                        {
                            int actId = 0;
                            if (int.TryParse(strActId, out actId))
                            {

                                if (actId == this.ID)
                                {
                                    isMatchAct = true;
                                    break;
                                }
                            }
                        }
                    }

                    bool isMatchField = false;
                    if (isMatchAct)
                    {
                        string field = node.SelectSingleNode("Field").InnerText;
                        string entityValue = Convert.ToString(this.Template.MasterData[field]);
                        XmlNodeList nodeValues = node.SelectSingleNode("Values").ChildNodes;
                        foreach (XmlNode nodeValue in nodeValues)
                        {
                            if (entityValue.Equals(nodeValue.InnerText, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatchField = true;
                                break;
                            }
                        }
                    }

                    if (isMatchAct && isMatchField)
                    {
                        XmlNodeList nodeUsers = node.SelectSingleNode("Users").ChildNodes;
                        foreach (XmlNode nodeUser in nodeUsers)
                        {
                            string uid = nodeUser.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(uid))
                            {
                                WFUser wfUser = new WFUser(uid);
                                wfUser.IsKeyUser = true;
                                listUsers.Add(wfUser);
                            }
                        }
                    }
                }
            }

            return listUsers;
        }

        public List<WFUser> GetEntityPreviewUser()
        {
            List<WFUser> listPreviewUser = new List<WFUser>();
            string strSql = "SELECT ID,IsKeyUser,IsApprover,UID FROM SYS_WFActivityUser WHERE ActivityID = @ID AND UserType = 'Entity'";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                string fieldId = Convert.ToString(dr["UID"]);
                strSql = String.Format("SELECT {0} AS ID, FieldName AS UserID,DisplayName AS DisplayName,CAST({1} AS BIT) AS IsKeyUser, CAST({2} AS BIT) AS IsApprover FROM SYS_FieldInfo WHERE ID = @ID", Convert.ToInt32(dr["ID"]), Convert.ToInt32(dr["IsKeyUser"]), Convert.ToInt32(dr["IsApprover"]));
                DataTable dtUser = DbHelperSQL.Query(strSql, new SqlParameter("@ID", fieldId)).Tables[0];

                foreach (DataRow drUser in dtUser.Rows)
                {
                    WFUser wfUser = ModelHandler<WFUser>.FillModel(drUser);
                    if (wfUser != null)
                    {
                        listPreviewUser.Add(wfUser);
                    }
                }
            }
            return listPreviewUser;
        }

        public List<WFUser> GetRolePreviewUser()
        {
            string strSql = "SELECT ID,IsKeyUser,IsApprover,UID AS UserID,UID AS DisplayName FROM SYS_WFActivityUser WHERE ActivityID = @ID AND UserType = 'Role'";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", this.ID)).Tables[0];
            return  ModelHandler<WFUser>.FillModel(dt);
        }

        public static WFActivity FillActivity(DataRow dr, WFTemplate template)
        {
            WFActivity wfActivity = ModelHandler<WFActivity>.FillModel(dr);
            wfActivity.Template = template;
            if (template != null)
            {
                wfActivity.TemplateID = template.ID;
            }
            return wfActivity;
        }
    }
}
