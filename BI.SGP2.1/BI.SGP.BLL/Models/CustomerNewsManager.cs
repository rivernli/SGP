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
    public class CustomerNewsManager
    {
        /// <summary>
        /// 字段的特性
        /// </summary>
        public int? ID { get; set; }
        /// <summary>
        /// 客户主表ID
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 新闻主题
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 新闻内容
        /// </summary>
        public string NewsContent { get; set; }
        /// <summary>
        /// 发布人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime PostedDate { get; set; }
        /// <summary>
        /// 附件ID
        /// </summary>
        public string AttachmentId { get; set; }




        //grid
        public static string GetCustomerNews(HttpRequestBase Request, string CustomerId)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string strSql = @"SELECT *,(select Name from Access_User where uid=PostedBy) as UserName, 
                                (SELECT COUNT(ID) FROM SGP_CustomerNews_Vews WHERE NewsId=SGP_CustomerNews.ID) AS SumViews, 
                                (SELECT COUNT(ID) FROM SGP_CustomerNews_Comments WHERE NewsId=SGP_CustomerNews.ID) AS Replies, 
                                (SELECT COUNT(ID) FROM SGP_CustomerNews_Vews WHERE NewsId=SGP_CustomerNews.ID AND SGP_CustomerNews_Vews.Uid='" + AccessControl.CurrentLogonUser.Uid + "') AS isRead FROM SGP_CustomerNews WHERE CustomerId='" + CustomerId + "'";
            if (!String.IsNullOrEmpty(Request["txTopic"]))
            {
                strSql += " AND Topic LIKE @Topic";
                ps.Add(new SqlParameter("@Topic", "%" + Request["txTopic"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txContent"]))
            {
                strSql += " AND NewsContent LIKE @NewsContent";
                ps.Add(new SqlParameter("@NewsContent", "%" + Request["txContent"] + "%"));
            } 

            GridData gridData = GridManager.GetGridData(Request, strSql, ps.ToArray());
            return gridData.ToJson();
        }

        //删除
        public static void DeleteCustomerNews(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerNews WHERE ID = @ID;DELETE FROM SGP_CustomerNews_Comments WHERE NewsId= @ID ";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }


        //保存
        public void Save()
        {
            List<SqlParameter> ps = new List<SqlParameter>()
            {
                new SqlParameter("@CustomerId", this.CustomerId),
                new SqlParameter("@Topic", this.Topic),
                new SqlParameter("@NewsContent", this.NewsContent),
                new SqlParameter("@AttachmentId", this.AttachmentId),
                new SqlParameter("@PostedBy", AccessControl.CurrentLogonUser.Uid),
                new SqlParameter("@PostedDate", System.DateTime.Now.ToString()) 
            };

            string strSql = string.Empty;
            if (ID > 0)
            {
                strSql = @"UPDATE SGP_CustomerNews SET [CustomerId]=@CustomerId,[Topic]=@Topic,[NewsContent]=@NewsContent,[AttachmentId]=@AttachmentId,[PostedBy]=@PostedBy,[PostedDate]=@PostedDate WHERE ID = '" + ID + "' ";
                DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());

            }
            else
            {
                strSql = @"INSERT INTO SGP_CustomerNews ([CustomerId],[Topic],[NewsContent],[AttachmentId],[PostedBy],[PostedDate]) 
                        VALUES(@CustomerId,@Topic,@NewsContent,@AttachmentId,@PostedBy,@PostedDate);SELECT @@IDENTITY";
                this.ID = DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
            }
        }

        //新闻点击数
        public static void SaveViews(string Id)
        {
            List<SqlParameter> ps = new List<SqlParameter>()
                {
                    new SqlParameter("@NewsId", Id),
                    new SqlParameter("@Uid", AccessControl.CurrentLogonUser.Uid)
                };

            string strSql = @"INSERT INTO SGP_CustomerNews_Vews ([NewsId],[Uid]) VALUES (@NewsId,@Uid)";
            DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
        }

        //保存新闻评论
        public void SaveReply(string NewsId,string Comment)
        {
            List<SqlParameter> ps = new List<SqlParameter>()
                {
                    new SqlParameter("@NewsId", NewsId),
                    new SqlParameter("@Comment", Comment),
                    new SqlParameter("@PostedBy", AccessControl.CurrentLogonUser.Uid),
                    new SqlParameter("@PostedDate", System.DateTime.Now.ToString()) 
                };

            string strSql = @"INSERT INTO SGP_CustomerNews_Comments ([NewsId],[Comment],[PostedBy],[PostedDate]) 
                            VALUES(@NewsId,@Comment,@PostedBy,@PostedDate)";
            DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
        }

        //新闻评论
        public static DataTable GetReplyData(string NewsId)
        {
            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strSql = "SELECT (select Name from Access_User where uid=PostedBy) as UserName,* FROM SGP_CustomerNews_Comments WHERE NewsId = @NewsId";
            lstParams.Add(new SqlParameter("@NewsId", NewsId));

            return DbHelperSQL.Query(strSql, lstParams.ToArray()).Tables[0];
        }
    }
}
