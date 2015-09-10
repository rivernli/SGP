using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.WF.Action
{
    public class SubTimeStampAction : IAction
    {
        public void DoAction(WFActivity activity)
        {
            if (activity.ParentID == activity.Template.ToActivity.ID)
            {
                if (activity.ID == 20202)
                {
                    string strSql = String.Format("UPDATE SGP_RFQGeneral SET BOMDateIn = ISNULL(BOMDateIn,GETDATE()) WHERE RFQID = {0}", activity.Template.EntityID);
                    DbHelperSQL.ExecuteSql(strSql);
                }
                else if (activity.ID == 20203)
                {
                    string strSql = String.Format("UPDATE SGP_RFQGeneral SET AssemblyDateIn = ISNULL(AssemblyDateIn,GETDATE()) WHERE RFQID = {0}", activity.Template.EntityID);
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
            else if (activity.ParentID == activity.Template.FromActivity.ID)
            {
                if (activity.ID == 20202)
                {
                    string strSql = String.Format("UPDATE SGP_RFQGeneral SET BOMDateOut = GETDATE() WHERE RFQID = {0}", activity.Template.EntityID);
                    DbHelperSQL.ExecuteSql(strSql);
                }
                else if (activity.ID == 20203)
                {
                    string strSql = String.Format("UPDATE SGP_RFQGeneral SET AssemblyDateOut = GETDATE() WHERE RFQID = {0}", activity.Template.EntityID);
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
        }
    }
}
