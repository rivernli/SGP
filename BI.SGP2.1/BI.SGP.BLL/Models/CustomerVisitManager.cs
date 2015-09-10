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
    public class CustomerVisitManager
    {
        /// <summary>
        /// 字段的特性
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 访问目的
        /// </summary>
        public string VisitPurpose { get; set; }
        /// <summary>
        /// Site
        /// </summary>
        public string Site { get; set; }
        /// <summary>
        /// 会议开始日期
        /// </summary>
        public DateTime ConferenceStartDate { get; set; }
        /// <summary>
        /// 会议开始时间
        /// </summary>
        public string ConferenceStartTime { get; set; }
        /// <summary>
        /// 会议结束日期
        /// </summary>
        public DateTime ConferenceEndDate { get; set; }
        /// <summary>
        /// 会议结束时间
        /// </summary>
        public string ConferenceEndTime { get; set; }
        /// <summary>
        /// 会议室
        /// </summary>
        public string ConferenceRoom { get; set; }
        /// <summary>
        /// 工作餐
        /// </summary>
        public string WorkingLunch { get; set; }
        /// <summary>
        /// 会议跟进人
        /// </summary>
        public string ResponsiblePerson { get; set; }
        /// <summary>
        /// 会议备注
        /// </summary>
        public string ConferenceRemark { get; set; }

        /// <summary>
        /// Date This Form Completed
        /// </summary>
        public DateTime DateThisFormCompleted { get; set; }
        /// <summary>
        /// Sales Organization
        /// </summary>
        public string SalesOrganization { get; set; }
        /// <summary>
        /// Sales Representatives
        /// </summary>
        public string SalesRepresentatives { get; set; }
        /// <summary>
        /// Tour Date
        /// </summary>
        public DateTime TourDate { get; set; }
        /// <summary>
        /// Arrival Time
        /// </summary>
        public DateTime ArrivalTime { get; set; }
        /// <summary>
        /// Departure Time
        /// </summary>
        public DateTime DepartureTime { get; set; }
        /// <summary>
        /// Customer
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// Location
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// ContactName
        /// </summary>
        public string ContactName { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Others TOAttend Tour
        /// </summary>
        public string OthersTOAttendTour { get; set; }
        /// <summary>
        /// Others TOAttend Tour Title
        /// </summary>
        public string OthersTOAttendTourTitle { get; set; }
        /// <summary>
        /// Is New Customer
        /// </summary>
        public string IsNewCustomer { get; set; }
        /// <summary>
        /// Is New Customer Remarks
        /// </summary>
        public string IsNewCustomerRemarks { get; set; }
        /// <summary>
        /// Customer Background
        /// </summary>
        public string CustomerBackground { get; set; }
        /// <summary>
        /// Customer Background Remarks
        /// </summary>
        public string CustomerBackgroundRemarks { get; set; }
        /// <summary>
        /// Any Issues
        /// </summary>
        public string AnyIssues { get; set; }
        /// <summary>
        /// Any Special Interests
        /// </summary>
        public string AnySpecialInterests { get; set; }
        /// <summary>
        /// Written Survey Complete
        /// </summary>
        public string WrittenSurveyComplete { get; set; }
        /// <summary>
        /// Written Survey Complete DateDue
        /// </summary>
        public DateTime WrittenSurveyCompleteDateDue { get; set; }
        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Initiator
        /// </summary>
        public string Initiator { get; set; }
        /// <summary>
        /// Initiator Date
        /// </summary>
        public DateTime InitiatorDate { get; set; }






        public static DataSet GetVisit()
        {
            //string strSql = "SELECT * FROM SGP_CustomerVisit";
            string strSql = @"SELECT ID,Customer,VisitPurpose, 
                            CONVERT(varchar(100), ConferenceStartDate, 23)+' '+ConferenceStartTime AS StartDate,
                            CONVERT(varchar(100), ConferenceEndDate, 23)+' '+ConferenceEndTime AS EndDate FROM SGP_CustomerVisit";
            DataSet ds = DbHelperSQL.Query(strSql);
            return ds;
        }

        /// <summary>
        /// Customer Visit
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string GetCustomerVisitData(HttpRequestBase Request)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string strSql = @"SELECT *,(CASE WHEN ConferenceStartDate=ConferenceEndDate THEN CONVERT(varchar(100), ConferenceStartDate, 23)+' '+ConferenceStartTime +'-'+ConferenceEndTime 
                            ELSE CONVERT(varchar(100), ConferenceStartDate, 23)+' '+ConferenceStartTime +'-'+
                            CONVERT(varchar(100), ConferenceEndDate, 23)+' '+ConferenceEndTime END) AS ConferenceTime FROM SGP_CustomerVisit WHERE 1=1 ";
            if (!String.IsNullOrEmpty(Request["txCustomer"]))
            {
                strSql += " AND Customer LIKE @Customer";
                ps.Add(new SqlParameter("@Customer", "%" + Request["txCustomer"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txVisitPurpose"]))
            {
                strSql += " AND VisitPurpose LIKE @VisitPurpose";
                ps.Add(new SqlParameter("@VisitPurpose", "%" + Request["txVisitPurpose"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txConferenceTime"]))
            {
                string[] sArray = Request["txConferenceTime"].Split('-');
                strSql += " AND ConferenceStartDate >= @ConferenceStartDate";
                ps.Add(new SqlParameter("@ConferenceStartDate", "" + sArray[0] + ""));
                strSql += " AND ConferenceEndDate >= @ConferenceEndDate";
                ps.Add(new SqlParameter("@ConferenceEndDate", "" + sArray[1] + ""));
            }
            if (!String.IsNullOrEmpty(Request["txFollowUpOwner"]))
            {
                strSql += " AND ResponsiblePerson LIKE @ResponsiblePerson";
                ps.Add(new SqlParameter("@ResponsiblePerson", "%" + Request["txResponsiblePerson"] + "%"));
            }
            GridData gridData = GridManager.GetGridData(Request, strSql, ps.ToArray());
            return gridData.ToJson();
        }
         

        /// <summary>
        /// Insert Sql语句 
        /// </summary>
        public int Add(Dictionary<string, object> dicAllTable)
        {
            string FileName = "";
            string FileValue = "";

            List<SqlParameter> paraList = new List<SqlParameter>(); 
            foreach (KeyValuePair<string, object> item in dicAllTable)
            {
                var MasterItem = (Dictionary<string, object>)item.Value;
                foreach (var kvField in MasterItem)
                {
                    if (item.Key.ToString() != "fmMultekUser")
                    {
                        if (FileName != string.Empty)
                        {
                            FileName += ", ";
                        }
                        if (FileValue != string.Empty)
                        {
                            FileValue += ", ";
                        }
                        FileName += "[" + kvField.Key + "] ";
                        FileValue += "@" + kvField.Key + " ";

                        paraList.Add(new SqlParameter("@" + kvField.Key + "", ConvertValue(kvField.Value)));
                    }
                }
            }
            string sqlstr = "INSERT INTO SGP_CustomerVisit (" + FileName + ") VALUES (" + FileValue + ");SELECT @@IDENTITY";
            this.ID = DbHelperSQL.GetSingle<int>(sqlstr, paraList.ToArray());

            //子表
            SubInsertData(dicAllTable);

            return this.ID;
        }

        /// <summary>
        /// Update Sql语句 
        /// </summary>
        public void Update(int ID, Dictionary<string, object> dicAllTable)
        {
            this.ID = ID;
            string FileValue = "";
            List<SqlParameter> paraList = new List<SqlParameter>();
            foreach (KeyValuePair<string, object> item in dicAllTable)
            {
                var MasterItem = (Dictionary<string, object>)item.Value;
                foreach (var kvField in MasterItem)
                {
                    if (item.Key.ToString() != "fmMultekUser")
                    {
                        if (FileValue != string.Empty)
                        {
                            FileValue += ", ";
                        }
                        FileValue += kvField.Key + "=@" + kvField.Key + " ";

                        paraList.Add(new SqlParameter("@" + kvField.Key + "", ConvertValue(kvField.Value)));
                    }
                } 
            }
            paraList.Add(new SqlParameter("@ID", this.ID));
            string sql = "UPDATE SGP_CustomerVisit SET " + FileValue + " WHERE ID=@ID ";
            DbHelperSQL.GetSingle<int>(sql, paraList.ToArray());

            //子表
            SubInsertData(dicAllTable);
        }

        private void SubInsertData(Dictionary<string, object> dicAllTable)
        {
            string sqlstr = string.Empty; 
            int subIndex = 0;

            //先删除后增加
            List<SqlParameter> paraDelList = new List<SqlParameter>();
            paraDelList.Add(new SqlParameter("@VisitId", ID));
            sqlstr = "DELETE FROM SGP_CustomerVisit_InvolvedPeople WHERE VisitId=@VisitId ";
            DbHelperSQL.GetSingle<int>(sqlstr, paraDelList.ToArray());

            foreach (KeyValuePair<string, object> item in dicAllTable)
            {
                var MasterItem = (Dictionary<string, object>)item.Value;
                foreach (var kvField in MasterItem)
                {
                    if (item.Key.ToString() == "fmMultekUser")
                    {
                        if (subIndex == 0)
                        {
                            if (kvField.Key == "ID") continue;
                            if (kvField.Value is ArrayList)
                            {
                                ArrayList userNames = (ArrayList)MasterItem["MultekUserName"];
                                ArrayList userTitles = (ArrayList)MasterItem["MultekUserTitle"];

                                for (int i = 0; i < userNames.Count; i++)
                                {
                                    List<SqlParameter> paraList = new List<SqlParameter>();
                                    paraList.Add(new SqlParameter("@Name", ConvertValue(userNames[i])));
                                    paraList.Add(new SqlParameter("@Title", ConvertValue(userTitles[i])));
                                    paraList.Add(new SqlParameter("@VisitId", ID));
                                    sqlstr = "INSERT INTO SGP_CustomerVisit_InvolvedPeople(VisitId, Name, Title) VALUES(@VisitId, @Name, @Title) ";
                                    DbHelperSQL.GetSingle<int>(sqlstr, paraList.ToArray());
                                }
                            }
                            else
                            {
                                string struserNames = (string)MasterItem["MultekUserName"];
                                string struserTitles = (string)MasterItem["MultekUserTitle"];
                                List<SqlParameter> paraList = new List<SqlParameter>();
                                paraList.Add(new SqlParameter("@Name", ConvertValue(struserNames)));
                                paraList.Add(new SqlParameter("@Title", ConvertValue(struserTitles)));
                                paraList.Add(new SqlParameter("@VisitId", ID));
                                sqlstr = "INSERT INTO SGP_CustomerVisit_InvolvedPeople(VisitId, Name, Title) VALUES(@VisitId, @Name, @Title) ";
                                DbHelperSQL.GetSingle<int>(sqlstr, paraList.ToArray());
                            }
                            subIndex++;
                        }
                    }
                }
            }
        } 

        public static object ConvertValue(object strValue)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(strValue)))
            {
                return strValue;
            }
            return DBNull.Value;
        }

        /// <summary>
        /// Delete Visit Data
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteVisitListData(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerVisit WHERE ID = @ID; DELETE FROM SGP_CustomerVisit_InvolvedPeople WHERE VisitId = @ID; DELETE FROM SGP_CustomerVisit_ActionsTracker WHERE VisitId = @ID; ";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        } 

        /// <summary>
        /// Delete Visit Multek People
        /// </summary>
        /// <param name="id"></param>
        public static void DeletePeople(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerVisit_InvolvedPeople WHERE ID = @ID ";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        public static DataTable GetVisitInvolvedPeopleData(string VisitID)
        {
            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strSql = "SELECT * FROM SGP_CustomerVisit_InvolvedPeople WHERE VisitID = @VisitID ";
            lstParams.Add(new SqlParameter("@VisitID", VisitID));             
            return DbHelperSQL.Query(strSql, lstParams.ToArray()).Tables[0];
        }

        public static string GenrateVisitList()
        {
            StringBuilder html = new StringBuilder();
            string strSql = @"SELECT TOP 10 ID,Customer, 
                            (CASE WHEN ConferenceStartDate=ConferenceEndDate THEN CONVERT(varchar(100), ConferenceStartDate, 23)+' '+ConferenceStartTime +'-'+ConferenceEndTime 
                            ELSE CONVERT(varchar(100), ConferenceStartDate, 23)+' '+ConferenceStartTime +'-'+
                            CONVERT(varchar(100), ConferenceEndDate, 23)+' '+ConferenceEndTime END) AS ConferenceTime FROM SGP_CustomerVisit ORDER BY ConferenceStartDate ASC ";
            DataSet ds = DbHelperSQL.Query(strSql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                html.AppendFormat(@"<div class='external-event label-grey ui-draggable' data-class='label-grey' style='position: relative;'>
                                        <i class='icon-move'></i>
                                        <a href='javascript:void(0)' onclick='CustomerVisitDetailed({0})' style='color:#FFFFFF'>{1}</a>
                                  </div>", Convert.ToString(dr["ID"]), Convert.ToString(dr["Customer"])); 
            } 
            return html.ToString();
        }
    }
}
