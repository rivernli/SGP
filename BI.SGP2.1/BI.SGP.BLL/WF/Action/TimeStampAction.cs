using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.WF.Action
{
    public class TimeStampAction : IAction
    {
        public void DoAction(WFActivity activity)
        {
            WFActivity fromActivity = activity.Template.FromActivity;
            WFActivity toActivity = activity.Template.ToActivity;

            if (fromActivity.ID == 1 || fromActivity.ID == 201)
            {
                string strSql = String.Format("UPDATE SGP_RFQGeneral SET RFQDateIn = GETDATE(), RFQDateOut=NULL, QuoteDateIn=NULL,QuoteDateOut=NULL,PriceDateOut=NULL WHERE RFQID = {0}", activity.Template.EntityID);
                DbHelperSQL.ExecuteSql(strSql);
            }

            //RFQ Date Out	Date of Stage 6 Latest Start Time
            if (toActivity != null && (toActivity.ID == 6 || toActivity.ID == 206))
            {
                string strSql = String.Format("UPDATE SGP_RFQGeneral SET RFQDateOut = GETDATE() WHERE RFQID = {0}", activity.Template.EntityID);
                DbHelperSQL.ExecuteSql(strSql);
            }


            //Price Date Out Date of Stage 5 Lastest Start Time
            if (toActivity != null && (toActivity.ID == 5 || toActivity.ID == 205))
            {
                string strSql = String.Format("UPDATE SGP_RFQGeneral SET PriceDateOut = GETDATE() WHERE RFQID = {0}", activity.Template.EntityID);
                DbHelperSQL.ExecuteSql(strSql);
            }

            //Cost Date In 	Date of Stage 2 Start Time
            if (toActivity != null && (toActivity.ID == 2 || toActivity.ID == 202))
            {
                string strSql = String.Format("UPDATE SGP_RFQGeneral SET QuoteDateIn = CASE WHEN QuoteDateIn IS NULL THEN GETDATE() ELSE QuoteDateIn END WHERE RFQID = {0}", activity.Template.EntityID);
                DbHelperSQL.ExecuteSql(strSql);
            }

            //Cost Date Out	Date of Stage 2 Latest End Time
            if (fromActivity != null && (fromActivity.ID == 2 || fromActivity.ID == 202))
            {
                string strSql = String.Format("UPDATE SGP_RFQGeneral SET QuoteDateOut = GETDATE() WHERE RFQID = {0}", activity.Template.EntityID);
                DbHelperSQL.ExecuteSql(strSql);
            }
        }
    }
}
