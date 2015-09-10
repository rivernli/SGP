using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using System.Web;
using System.Reflection;
using BI.SGP.BLL.DataModels;
using System.Transactions;
using System.Collections;
using System.Globalization;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.UIManager;

namespace BI.SGP.BLL.Models
{
    public class CustomerVisitActionsTrackerManager
    {
        /// <summary>
        /// 字段的特性
        /// </summary>
        public int? ID { get; set; }
        /// <summary>
        /// Visit ID
        /// </summary>
        public int VisitId { get; set; }
        /// <summary>
        ///主题
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 跟进人
        /// </summary>
        public string FollowUpOwner { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 附件ID
        /// </summary>
        public string AttachmentId { get; set; }



        /// <summary>
        /// Customer Visit Actions Tracker 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string GetCustomerVisitActionsTrackerData(HttpRequestBase Request, string VisitId)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string strSql = @"SELECT *,(SELECT Customer FROM SGP_CustomerVisit WHERE SGP_CustomerVisit.ID=SGP_CustomerVisit_ActionsTracker.VisitId) AS Customer FROM SGP_CustomerVisit_ActionsTracker WHERE 1=1 ";
            if (!String.IsNullOrEmpty(VisitId))
            {
                strSql += " AND VisitId = @VisitId";
                ps.Add(new SqlParameter("@VisitId", "" + VisitId + ""));
            }
            if (!String.IsNullOrEmpty(Request["txCustomer"]))
            {
                strSql += " AND VisitId IN (SELECT ID FROM SGP_CustomerVisit WHERE Customer Like @Customer) ";
                ps.Add(new SqlParameter("@Customer", "%" + Request["txCustomer"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txTopic"]))
            {
                strSql += " AND Topic LIKE @Topic";
                ps.Add(new SqlParameter("@Topic", "%" + Request["txTopic"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txFollowUpOwner"]))
            {
                strSql += " AND FollowUpOwner LIKE @FollowUpOwner";
                ps.Add(new SqlParameter("@FollowUpOwner", "%" + Request["txFollowUpOwner"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txStatus"]))
            {
                strSql += " AND Status LIKE @Status";
                ps.Add(new SqlParameter("@Status", "%" + Request["txStatus"] + "%"));
            }
            GridData gridData = GridManager.GetGridData(Request, strSql, ps.ToArray());
            return gridData.ToJson();
        }


        /// <summary>
        /// Delete Visit Actions Tracker Data
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteVisitActionsTrackerData(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerVisit_ActionsTracker WHERE ID = @ID; ";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        //保存
        public void Save()
        {
            List<SqlParameter> ps = new List<SqlParameter>()
            {
                new SqlParameter("@VisitId", this.VisitId),
                new SqlParameter("@Topic", this.Topic),
                new SqlParameter("@Comments", this.Comments),
                new SqlParameter("@FollowUpOwner", this.FollowUpOwner),
                new SqlParameter("@Status", this.Status),
                new SqlParameter("@AttachmentId", this.AttachmentId),
                new SqlParameter("@Initiator", AccessControl.CurrentLogonUser.Uid),
                new SqlParameter("@InitiatorDate", System.DateTime.Now.ToString()) 
            };

            string strSql = string.Empty;
            if (ID > 0)
            {
                strSql = @"UPDATE SGP_CustomerVisit_ActionsTracker SET [VisitId]=@VisitId,[Topic]=@Topic,[Comments]=@Comments,[FollowUpOwner]=@FollowUpOwner,[Status]=@Status,[AttachmentId]=@AttachmentId,[Initiator]=@Initiator,[InitiatorDate]=@InitiatorDate WHERE ID = '" + ID + "' ";
                DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());

            }
            else
            {
                strSql = @"INSERT INTO SGP_CustomerVisit_ActionsTracker ([VisitId],[Topic],[Comments],[FollowUpOwner],[Status],[AttachmentId],[Initiator],[InitiatorDate]) 
                        VALUES(@VisitId, @Topic, @Comments, @FollowUpOwner, @Status, @AttachmentId, @Initiator, @InitiatorDate);SELECT @@IDENTITY";
                this.ID = DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
            }
        }
    }
}
