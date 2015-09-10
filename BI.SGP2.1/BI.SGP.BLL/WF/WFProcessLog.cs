using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.DataModels;

namespace BI.SGP.BLL.WF
{
    
    public class WFProcessLog
    {
        public int ID { get; set; }
        public int EntityID { get; set; }
        public int TemplateID { get; set; }
        public int FromActivityID { get; set; }
        public int ToActivityID { get; set; }
        public int SubActivityType { get; set; }
        public bool SubActivityComplated { get; set; }
        public DateTime ActionTime { get; set; }
        public string ActionUser { get; set; }
        public string ActionUserName { get; set; }
        public string Comment { get; set; }
        public List<WFProcessLog> SubProcessLog = new List<WFProcessLog>();
        public List<WFProcessLog> CurrentComplatedProcessLog = new List<WFProcessLog>();
        public WFProcessLog ParentProcessLog { get; set; }

        private WFActivity _fromActivity;
        public WFActivity FromActivity
        {
            get
            {
                if (_fromActivity == null)
                {
                    if (SubActivityType == 2)
                    {
                        _fromActivity = ParentProcessLog.FromActivity.ChildActivities.Get(FromActivityID);
                    }
                    else
                    {
                        _fromActivity = Template.Activities.Get(FromActivityID);
                    }
                    
                }
                return _fromActivity;
            }
        }

        private WFActivity _toActivity;
        public WFActivity ToActivity
        {
            get
            {
                if (_toActivity == null)
                {
                    if (SubActivityType == 4)
                    {
                        _toActivity = ParentProcessLog.ToActivity.ChildActivities.Get(ToActivityID);
                    }
                    else
                    {
                        _toActivity = Template.Activities.Get(ToActivityID);
                    }
                    
                }
                return _toActivity;
            }
        }

        private WFTemplate _template;
        public WFTemplate Template
        {
            get
            {
                if (_template == null)
                {
                    _template = new WFTemplate(TemplateID, EntityID);
                }
                return _template;
            }
        }
    }
}
