using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Event
{
    public class UserChangedEvent : IDataChangedEvent
    {
        private int _entityId;
        WFActivity _activity;
        List<WFUser> _previewUser;
        Dictionary<string, string> _oldUser = new Dictionary<string,string>();
        Dictionary<string, string> _newUser = new Dictionary<string, string>();

        public bool WasChanged
        {
            get
            {
                if (_oldUser != null && _newUser != null)
                {
                    foreach (KeyValuePair<string, string> kv in _newUser)
                    {
                        if (_oldUser.ContainsKey(kv.Key) && _oldUser[kv.Key] != _newUser[kv.Key])
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public UserChangedEvent(int RFQID)
        {
            return;
            _entityId = RFQID;
            if (_entityId > 0)
            {
                WFTemplate wf = new WFTemplate(1, _entityId);
                _activity = wf.CurrentActivity;
            }
        }

        public void DoBefore()
        {
            return;
            if (_activity != null)
            {
                _previewUser = _activity.GetEntityPreviewUser();
                for (int i = _previewUser.Count - 1; i >= 0; i--)
                {
                    if (!_previewUser[i].IsKeyUser && !_previewUser[i].IsApprover)
                    {
                        _previewUser.RemoveAt(i);
                    }
                }
                if (_previewUser != null && _previewUser.Count > 0)
                {
                    foreach (WFUser user in _previewUser)
                    {
                        if (_activity.Template.MasterData.Table.Columns.Contains(user.UserID))
                        {
                            string fieldValue = Convert.ToString(_activity.Template.MasterData[user.UserID]);
                            if (!String.IsNullOrWhiteSpace(fieldValue))
                            {
                                _oldUser.Add(user.UserID, fieldValue);
                            }
                        }
                    }
                }
                _activity.Template.MasterData = null;
            }
        }

        public void DoAfter()
        {
            return;
            if (_activity != null)
            {
                if (_previewUser != null && _previewUser.Count > 0)
                {
                    foreach (WFUser user in _previewUser)
                    {
                        if (_activity.Template.MasterData.Table.Columns.Contains(user.UserID))
                        {
                            string fieldValue = Convert.ToString(_activity.Template.MasterData[user.UserID]);
                            if (!String.IsNullOrWhiteSpace(fieldValue))
                            {
                                _newUser.Add(user.UserID, fieldValue);
                            }
                        }
                    }
                }
                _activity.Template.MasterData = null;
                DoAction();
            }
        }

        public void DoAction()
        {
            return;
            if (WasChanged)
            {
                BI.SGP.BLL.WF.Action.SendMailAction sma = new WF.Action.SendMailAction();
                sma.DoAction(_activity);
            }
        }
    }
}
