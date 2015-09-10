using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BI.SGP.BLL.Models.Detail
{
    public class SCMPriceMasterDetail
    {
        /// <summary>
        /// 新Quote Request
        /// </summary>
        public const string QuoteStatus_Launch = "Launch";
        /// <summary>
        /// 已报价
        /// </summary>
        public const string QuoteStatus_Quoted = "Quoted";
        /// <summary>
        /// 价格上浮
        /// </summary>
        public const string QuoteStatus_Rise = "Rise";
        /// <summary>
        /// 回退报价
        /// </summary>
        public const string QuoteStatus_Reject = "Reject";
        /// <summary>
        /// 审批通过
        /// </summary>
        public const string QuoteStatus_Approved = "Approved";

        /// <summary>
        /// 空闲
        /// </summary>
        public const string RequestStatus_Free = "Free";
        /// <summary>
        /// 报价流程中
        /// </summary>
        public const string RequestStatus_InProcess = "InProcess";

        protected FieldCategory Category { get; set; }
        protected string pageType = string.Empty;

        public SCMPriceMasterDetail(FieldCategory category, Dictionary<string, object> data, string pageType)
        {
            this.Category = category;
            this.pageType = pageType;
            foreach (KeyValuePair<string, object> kv in data)
            {
                FieldInfo f = category.Fields[kv.Key];
                if (f != null)
                {
                    f.DataValue = kv.Value;
                }
            }
        }

        public SCMPriceMasterDetail()
        {

        }

        public void CheckData(SystemMessages sysMsg)
        {
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.UpdateKey && field.Options.Required)
                {
                    if (FieldIsEmpty(field))
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add(field.DisplayName, String.Format("\"{0}\" is required.", field.DisplayName));
                    }
                }

            }
        }

        public bool FieldIsEmpty(FieldInfo field)
        {
            string fieldValue = String.Format("{0}", field.DataValue);
            if (String.IsNullOrWhiteSpace(fieldValue))
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
                    case FieldInfo.DATATYPE_PERCENT:
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

        public void InsertUploadFile(DateTime date)
        {
            if (this.pageType == "Laminate" || this.pageType == "Prepreg")
            {
                if (HasExisted())
                {
                    UpdateMainData(date);
                }
                else
                {
                    AddMainData(date);
                }
            }
            else if (this.pageType == "Vendor")
            {
                InsertVendorQuote(date);
            }
        }

        /// <summary>
        /// 主数据更新
        /// </summary>
        public void UpdateMainData(DateTime date)
        {
            string strUpdateField = string.Empty;
            string strWhere = string.Empty;
            List<SqlParameter> parameters = new List<SqlParameter>();
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.UpdateKey)
                {
                    strUpdateField += field.FieldName + "=@" + field.FieldName + ",";
                    parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                }

                if (field.Options.ConditionKey)
                {
                    strWhere += " and " + field.FieldName + "=@" + field.FieldName;
                    parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                }
            }
            strUpdateField += "UpdateUser=@UpdateUser,UpdateDate=@UpdateDate";
            string strSql = "update " + Category.Fields[0].TableName + " set " + strUpdateField + " where PageType=@PageType" + strWhere;
            parameters.Add(new SqlParameter("@UpdateUser", AccessControl.CurrentLogonUser.Uid));
            parameters.Add(new SqlParameter("@UpdateDate", date));
            parameters.Add(new SqlParameter("@PageType", this.pageType));
            DbHelperSQL.ExecuteSql(strSql, parameters.ToArray());
        }

        /// <summary>
        /// 插入新主数据
        /// </summary>
        public void AddMainData(DateTime date)
        {
            string strField = string.Empty;
            string strValue = string.Empty;
            List<SqlParameter> parameters = new List<SqlParameter>();
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.UpdateKey)
                {
                    strField += field.FieldName + ",";
                    strValue += "@" + field.FieldName + ",";
                    parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                }
            }
            strField += "UpdateUser,UpdateDate,PageType,RequestStatus";
            strValue += "@UpdateUser,@UpdateDate,@PageType,'Free'";
            parameters.Add(new SqlParameter("@UpdateUser", AccessControl.CurrentLogonUser.Uid));
            parameters.Add(new SqlParameter("@UpdateDate", date));
            parameters.Add(new SqlParameter("@PageType", this.pageType));
            string strSql = "insert into " + Category.Fields[0].TableName + "(" + strField + ") values(" + strValue + ")";
            DbHelperSQL.ExecuteSql(strSql, parameters.ToArray());
        }

        /// <summary>
        /// 供应商更新报价
        /// </summary>
        public void InsertVendorQuote(DateTime date)
        {
            int mainId = GetMainId();
            if (mainId > 0)
            {
                string quoteStatus = QuoteStatus_Quoted;
                decimal sqft = 0;
                decimal pic = 0;
                string strUpdateField = string.Empty;
                List<SqlParameter> parameters = new List<SqlParameter>();
                foreach (FieldInfo field in Category.Fields)
                {
                    if (field.Options.UpdateKey)
                    {
                        strUpdateField += field.FieldName + "=@" + field.FieldName + ",";
                        parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                    }

                    if (field.FieldName.ToUpper() == "SQFT")
                    {
                        decimal.TryParse(field.DataValue.ToString(), out sqft);
                    }

                    if (field.FieldName.ToUpper() == "PIC")
                    {
                        decimal.TryParse(field.DataValue.ToString(), out pic);
                    }
                }

                if (HasRise(mainId, sqft, pic))
                {
                    quoteStatus = QuoteStatus_Rise;
                }

                //更新状态Status=1(Quoted)
                strUpdateField += "QuoteStatus=@QuoteStatus,QuoteDate=@QuoteDate";
                string strSql = "update SGP_SCMQuote set " + strUpdateField;
                strSql += " where MainID=@MainID and (QuoteStatus<>@StatusQuoted)";
                parameters.Add(new SqlParameter("@QuoteStatus", quoteStatus));
                parameters.Add(new SqlParameter("@QuoteDate", date));
                parameters.Add(new SqlParameter("@MainID", mainId));
                parameters.Add(new SqlParameter("@StatusQuoted", QuoteStatus_Quoted));
                DbHelperSQL.ExecuteSql(strSql, parameters.ToArray());
            }
        }

        /// <summary>
        /// 检查本次报价与当前生效最新报价是否上浮
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sqft"></param>
        /// <param name="pic"></param>
        /// <returns>true:上浮; false:未上浮</returns>
        private bool HasRise(int id, decimal sqft, decimal pic)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("select count(1) from SGP_SCM ");
            sbSql.Append("where NewestSqft>=@Sqft and NewestPic>=@Pic and ID=@ID");
            SqlParameter[] paras = { 
                                   new SqlParameter("@Sqft", sqft),
                                   new SqlParameter("@Pic", pic),
                                   new SqlParameter("@ID", id)
                                   };
            int i = Convert.ToInt32(DbHelperSQL.GetSingle(sbSql.ToString(), paras));
            if (i > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 取当前更新数据对应主数据ID
        /// </summary>
        /// <returns></returns>
        private int GetMainId()
        {
            string strWhere = string.Empty;
            List<SqlParameter> parameters = new List<SqlParameter>();
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.ConditionKey)
                {
                    strWhere += " and " + field.FieldName + "=@" + field.FieldName;
                    parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                }
            }
            //所更新数据需对应当前操作供应商
            strWhere += " and VendorCode=@VendorCode";
            ///TODO: RZ0220 => AccessControl.CurrentLogonUser.Uid
            parameters.Add(new SqlParameter("@VendorCode", "RZ0220"));
            string strSql = "select top 1 ID from SGP_SCM where 1=1" + strWhere;
            int id = Convert.ToInt32(DbHelperSQL.GetSingle(strSql, parameters.ToArray()));
            return id;
        }

        public int GetMainId(string quoteId)
        {
            string strSql = "select top 1 MainID from SGP_SCMQuote where ID=@ID";
            int mainId = Convert.ToInt32(DbHelperSQL.GetSingle(strSql, new SqlParameter("@ID", quoteId)));
            return mainId;
        }

        /// <summary>
        /// 当前更新主数据是否已存在
        /// </summary>
        /// <returns>true:已存在; false:不存在</returns>
        public bool HasExisted()
        {
            StringBuilder strSql = new StringBuilder();
            List<SqlParameter> parameters = new List<SqlParameter>();
            strSql.Append("select count(1) from SGP_SCM where 1=1");
            foreach (FieldInfo field in Category.Fields)
            {
                if (field.Options.ConditionKey)
                {
                    strSql.Append(" and " + field.FieldName + "=@" + field.FieldName);
                    parameters.Add(new SqlParameter("@" + field.FieldName, field.DataValue));
                }
            }
            int i = Convert.ToInt32(DbHelperSQL.GetSingle(strSql.ToString(), parameters.ToArray()));
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新主数据最新与上次报价信息
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="quoteId"></param>
        public void UpdateMainQuoteRecord(int mainId, string quoteId)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("update SGP_SCM set LastPriceDate=NewestPriceDate,LastSqft=NewestSqft,LastPic=NewestPic where ID=@MainID");
            DbHelperSQL.ExecuteSql(sbSql.ToString(), new SqlParameter("@MainID", mainId));

            sbSql.Clear();
            sbSql.Append("update SGP_SCM set NewestPriceDate=b.QuoteDate,NewestSqft=b.Sqft,NewestPic=b.Pic ");
            sbSql.Append("from SGP_SCM a join SGP_SCMQuote b on b.MainID=a.ID and b.ID=@QuoteID where a.ID=@MainID");
            SqlParameter[] paras = { 
                                   new SqlParameter("@QuoteID", quoteId),
                                   new SqlParameter("@MainID", mainId)
                                   };
            DbHelperSQL.ExecuteSql(sbSql.ToString(), paras);
        }

        /// <summary>
        /// 更新报价记录状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quoteStatus"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool ChangeQuoteStatus(string id, string quoteStatus, DateTime date)
        {
            string strSql = string.Empty;
            string strUpdate = string.Empty;
            string strCondition = string.Empty;
            List<SqlParameter> paraList = new List<SqlParameter>();
            strSql = "update SGP_SCMQuote set QuoteStatus=@QuoteStatus,UpdateUser=@UpdateUser,UpdateDate=@UpdateDate";
            paraList.Add(new SqlParameter("@QuoteStatus", quoteStatus));
            paraList.Add(new SqlParameter("@UpdateUser", AccessControl.CurrentLogonUser.Uid));
            paraList.Add(new SqlParameter("@UpdateDate", date));
            if (quoteStatus == QuoteStatus_Approved)
            {
                strUpdate += ",ApproveUser=@ApproveUser,ApproveDate=@ApproveDate";
                paraList.Add(new SqlParameter("ApproveUser", AccessControl.CurrentLogonUser.Uid));
                paraList.Add(new SqlParameter("@ApproveDate", date));

                strCondition += " and (QuoteStatus=@StatusQuoted or QuoteStatus=@StatusRise or QuoteStatus=@StatusReject)";
                paraList.Add(new SqlParameter("@StatusQuoted", QuoteStatus_Quoted));
                paraList.Add(new SqlParameter("@StatusRise", QuoteStatus_Rise));
                paraList.Add(new SqlParameter("@StatusReject", QuoteStatus_Reject));
            }
            else if (quoteStatus == QuoteStatus_Reject)
            {
                strCondition += " and (QuoteStatus=@StatusQuoted or QuoteStatus=@StatusRise)";
                paraList.Add(new SqlParameter("@StatusQuoted", QuoteStatus_Quoted));
                paraList.Add(new SqlParameter("@StatusRise", QuoteStatus_Rise));
            }

            strSql += strUpdate + " where ID=@ID" + strCondition;
            paraList.Add(new SqlParameter("@ID", id));
            int i = DbHelperSQL.ExecuteSql(strSql, paraList.ToArray());
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改SGP_SCM Request状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestStatus"></param>
        public void ChangeRequestStatus(string id, string requestStatus)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("update SGP_SCM set RequestStatus=@RequestStatus where ID=@ID");
            SqlParameter[] parameters = {
                                        new SqlParameter("@RequestStatus", requestStatus),
                                        new SqlParameter("@ID", id)
                                        };
            DbHelperSQL.ExecuteSql(sbSql.ToString(), parameters);
        }

        /// <summary>
        /// 创建新询价
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool CreateNewRequestToVendor(string id, DateTime date)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("insert into SGP_SCMQuote(MainID,CreateUser,CreateDate,QuoteStatus) ");
            sbSql.Append("values(@MainID,@CreateUser,@CreateDate,@StatusLaunch)");
            SqlParameter[] paras = {
                                   new SqlParameter("@MainID", id),
                                   new SqlParameter("@CreateUser", AccessControl.CurrentLogonUser.Uid),
                                   new SqlParameter("@CreateDate", date),
                                   new SqlParameter("@StatusLaunch", QuoteStatus_Launch)
                                   };
            int i = DbHelperSQL.ExecuteSql(sbSql.ToString(), paras);
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 未审批报价修改为失效
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool DisableQuote(string id, DateTime date)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("update SGP_SCMQuote set Available=0,UpdateUser=@UpdateUser,UpdateDate=@UpdateDate ");
            sbSql.Append("where ID=@ID and QuoteStatus<>@StatusApproved");
            SqlParameter[] paras = { 
                                   new SqlParameter("@UpdateUser", AccessControl.CurrentLogonUser.Uid),
                                   new SqlParameter("@UpdateDate", date),
                                   new SqlParameter("@ID", id),
                                   new SqlParameter("@StatusApproved", QuoteStatus_Approved)
                                   };
            int i = DbHelperSQL.ExecuteSql(sbSql.ToString(), paras);
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查主数据是否正在报价流程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckIsFree(string id)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("select count(1) from SGP_SCM where RequestStatus=@StatusFree and ID=@ID");
            SqlParameter[] paras = { 
                                   new SqlParameter("@StatusFree", RequestStatus_Free),
                                   new SqlParameter("@ID", id)
                                   };
            int i = Convert.ToInt32(DbHelperSQL.GetSingle(sbSql.ToString(), paras));
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DataSet GetReportData(HttpRequestBase request, string categoryName)
        {
            DataSet dsOri = GetOriginalData(request);
            DataSet dsReport = new DataSet();
            if (dsOri != null)
            {
                foreach (DataTable dtOri in dsOri.Tables)
                {
                    List<DateTime> quoteDateList = GetAllQuoteDate(dtOri);
                    DataTable dtReport = InitReportTable(quoteDateList, dtOri.TableName);
                    InsertDataToReportTable(dtOri, ref dtReport);
                    TransColumnName(categoryName, ref dtReport);
                    dsReport.Tables.Add(dtReport);
                }
            }

            if (dsReport == null || dsReport.Tables.Count <= 0)
            {
                dsReport.Tables.Add(InitReportTable(new List<DateTime>(), "Laminate"));
            }
            return dsReport;
        }

        private void TransColumnName(string categoryName, ref DataTable dtReport)
        {
            FieldCategory fc = new FieldCategory(categoryName);
            FieldInfoCollecton fields = fc.VisibleFields;
            foreach (FieldInfo field in fields)
            {
                if (dtReport.Columns.Contains(field.FieldName))
                {
                    dtReport.Columns[field.FieldName].ColumnName = field.DisplayName;
                }
            }
        }

        private void InsertDataToReportTable(DataTable dtOri, ref DataTable dtReport)
        {
            List<string> idList = new List<string>();
            foreach (DataRow drOri in dtOri.Rows)
            {
                string id = drOri["ID"].ToString();
                if (!idList.Contains(id))
                {
                    idList.Add(id);
                    DataRow drReport = dtReport.NewRow();
                    foreach (DataColumn dcOri in dtOri.Columns)
                    {
                        if (dtReport.Columns.Contains(dcOri.ColumnName))
                        {
                            drReport[dcOri.ColumnName] = drOri[dcOri.ColumnName];
                        }
                    }

                    DataRow[] drs = dtOri.Select(string.Format("ID={0}", id));
                    foreach (DataRow dr in drs)
                    {
                        DateTime quoteDate = DateTime.MinValue;
                        if (IsValidQuoteDate(dr["QuoteDate"].ToString(), out quoteDate))
                        {
                            string columnNameSqft = quoteDate.ToString("yyyy-MM-dd") + " $/Sqft";
                            string columnNamePic = quoteDate.ToString("yyyy-MM-dd") + " $/Pic";

                            drReport[columnNameSqft] = dr["Sqft"];
                            drReport[columnNamePic] = dr["Pic"];
                        }
                    }
                    dtReport.Rows.Add(drReport);
                }
                else
                {
                    continue;
                }
            }
            dtReport.AcceptChanges();
        }

        private DataTable InitReportTable(List<DateTime> quoteDateList, string priceMasterType)
        {
            DataTable dt = new DataTable("Laminate");
            dt.Columns.Add("ItemGroup");
            dt.Columns.Add("Category");
            dt.Columns.Add("VendorCode");
            dt.Columns.Add("VendorName");
            dt.Columns.Add("TgType");
            dt.Columns.Add("Location");
            dt.Columns.Add("Tg");
            dt.Columns.Add("PartNo");
            dt.Columns.Add("Description");
            dt.Columns.Add("VendorPN");
            if (priceMasterType == "Laminate")
            {
                dt.Columns.Add("CoreThickness");
                dt.Columns.Add("CopperThickness");
                dt.Columns.Add("CopperTreatment");
                dt.Columns.Add("DPlus9");
                dt.Columns.Add("ZBC");
                dt.Columns.Add("HF");
                dt.Columns.Add("Construction");
            }
            else if (priceMasterType == "Prepreg")
            {
                dt.Columns.Add("Type");
                dt.Columns.Add("RC");
                dt.Columns.Add("DPlus9");
                dt.Columns.Add("HF");
            }
            dt.Columns.Add("DimFill");
            dt.Columns.Add("DimWrap");
            dt.Columns.Add("Unit");
            dt.Columns.Add("Currency");
            dt.Columns.Add("Volume");
            dt.Columns.Add("PriceType");
            dt.Columns.Add("PurchaseSqft");

            foreach (DateTime quoteDate in quoteDateList)
            {
                dt.Columns.Add(quoteDate.ToString("yyyy-MM-dd") + " $/Sqft");
                dt.Columns.Add(quoteDate.ToString("yyyy-MM-dd") + " $/Pic");
            }

            dt.Columns.Add("Rebate");
            dt.Columns.Add("Remark");
            dt.Columns.Add("LeadTime");
            dt.Columns.Add("SPQ");
            dt.Columns.Add("MOQ");
            return dt;
        }

        private List<DateTime> GetAllQuoteDate(DataTable dt)
        {
            List<DateTime> quoteDateList = new List<DateTime>();
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    DateTime quoteDate = DateTime.MinValue;
                    if (IsValidQuoteDate(dr["QuoteDate"].ToString(), out quoteDate))
                    {
                        quoteDateList.Add(quoteDate);
                    }
                }
            }
            return quoteDateList;
        }

        private DataSet GetOriginalData(HttpRequestBase request)
        {
            string strSql = string.Empty;
            string strWhere = string.Empty;
            string searchGroupName = request.QueryString["searchGroup"];
            DataSet dsOri = new DataSet();
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (!String.IsNullOrEmpty(searchGroupName))
            {
                strWhere += GridManager.GetWhereSql(request, searchGroupName, parameters);
            }

            List<string> typeList = GetSCMPriceMasterType();
            foreach (string priceMasterType in typeList)
            {
                strSql = string.Empty;
                strSql += "select * from V_SGP_SCM_REPORT";
                strSql += " where Category=@PriceMasterType" + priceMasterType;
                parameters.Add(new SqlParameter("@PriceMasterType" + priceMasterType, priceMasterType));
                if (!string.IsNullOrEmpty(strWhere))
                {
                    strSql += strWhere;
                }
                strSql += " order by QuoteDate DESC, CreateDate DESC, UpdateDate DESC, ID DESC";
                DataSet ds = DbHelperSQL.Query(strSql, parameters.ToArray());
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dtOri = ds.Tables[0].Copy();
                    dtOri.TableName = priceMasterType;
                    dsOri.Tables.Add(dtOri);
                }
            }
            return dsOri;
        }

        private List<string> GetSCMPriceMasterType()
        {
            List<string> typeList = new List<string>();
            string strSql = "select [key],[value] from SGP_KeyValue where [key]='SCMPriceMasterType' order by Sort";
            DataSet ds = DbHelperSQL.Query(strSql);
            if (ds != null)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    typeList.Add(dr["value"].ToString());
                }
            }
            return typeList;
        }

        private bool IsValidQuoteDate(string strQuoteDate, out DateTime quoteDate)
        {
            quoteDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(strQuoteDate))
            {
                return DateTime.TryParse(strQuoteDate, out quoteDate);
            }
            return false;
        }
    }
}
