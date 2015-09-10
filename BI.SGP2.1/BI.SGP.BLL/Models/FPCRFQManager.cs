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


namespace BI.SGP.BLL.Models
{
   public class FPCRFQManager
    {
         /// Check RFQDetail Data for Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CheckData(HttpRequestBase Request, ref SystemMessages message)
        {
            message = new SystemMessages();
            FieldInfoCollecton allFields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_FPC);

            foreach (string key in Request.Form.Keys)
            {
                CheckData(key, Request.Form[key], allFields, ref message);
            }
            return true;
        }
        public static bool CheckData(DataRow dr, ref SystemMessages message)
        {
            message = new SystemMessages();
            FieldInfoCollecton allFields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_FPC);

            foreach (DataColumn dc in dr.Table.Columns)
            {
                CheckData(dc.ColumnName, dr[dc].ToString(), allFields, ref message);
            }
            return true;
        }


        /// <summary>
        /// Check RFQDetail Data, and bind to systemmessages
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="allFields"></param>
        /// <param name="message"></param>
        public static void CheckData(string key, string value, FieldInfoCollecton allFields, ref SystemMessages message)
        {

            if (allFields[key] != null)
            {
                SGP.BLL.DataModels.FieldInfo fi = allFields[key];

                switch (allFields[key].DataType)
                {
                    case "int":
                        if (!CheckVBalueIfInt(value))
                        {
                            message.isPass = false;
                            message.Messages.Add(key, value + " is not numeric type");
                        }
                        break;
                    case "date":
                        if (!CheckVBalueIfDate(value))
                        {
                            message.isPass = false;
                            message.Messages.Add(key, value + " is not date time type");
                        }
                        break;
                    case "float":
                    case "double":
                        if (!CheckVBalueIffloat(value))
                        {
                            message.isPass = false;
                            message.Messages.Add(key, value + " is not float Type");
                        }
                        break;
                    case "list":
                        if (!CheckVBalueIfList(key, value))
                        {
                            message.isPass = false;
                            message.Messages.Add(key, value + " not exist in data list");
                        }
                        break;
                    case "listsql":
                        if (!CheckVBalueIfListSQL(key, value, fi.KeyValueSource))
                        {
                            message.isPass = false;
                            message.Messages.Add(key, value + " not exist in data list");
                        }
                        break;
                }

            }


        }

        /// <summary>
        /// if the data is int? 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static bool CheckVBalueIfInt(string val)
        {
            int v;
            if (string.IsNullOrEmpty(val)) return true;
            if (Int32.TryParse(val, out v)) return true;

            return false;
        }
        /// <summary>
        /// if data is datetime?
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static bool CheckVBalueIfDate(string val)
        {
            if (string.IsNullOrEmpty(val)) return true;

            //if(string.IsNullOrEmpty(val.Trim())==true)
            //{
            //    val = DateTime.Now.ToString("mm/dd/yyyy");
            //}
            DateTime v;
            if (DateTime.TryParseExact(val, "mm/dd/yyyy", null, DateTimeStyles.None, out v)) return true;

            return true;
        }
        /// <summary>
        /// if data is float?
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static bool CheckVBalueIffloat(string val)
        {
            float v;
            if (string.IsNullOrEmpty(val)) return true;
            if (float.TryParse(val, out v)) return true;

            return false;
        }
        /// <summary>
        /// if data in list?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private static bool CheckVBalueIfList(string key, string val)
        {
            if (val.Trim() == "" && val.Trim() == string.Empty) return true;

            bool islist = DbHelperSQL.Exists(@"select count(*) from SGP_KeyValue where [Key]=@Key and [Value]=@Value and status=1",
                                            new SqlParameter("@Key", key), new SqlParameter("@Value", val));
            return islist;

        }
        private static bool CheckVBalueIfListSQL(string key, string val, string listsql)
        {
            if (val.Trim() == "" && val.Trim() == string.Empty) return true;
            int place = listsql.LastIndexOf("order by");
            if (place > 0)
            {
                listsql = listsql.Substring(0, place);
            }
            bool islist = DbHelperSQL.Exists(@"select count(*) from (" + listsql + ") as a where [Key]=@Key and [Value]=@Value",
                                            new SqlParameter("@Key", key), new SqlParameter("@Value", val));
            return islist;

        }
        /// <summary>
        /// CheckPrimaryKey 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CheckPrimaryKey(string tablename, string key, string value)
        {

            string strSql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}={2}", tablename, key, value);

            return DbHelperSQL.Exists(strSql);
        }

        public static RFQDetail GetLastDetailByExtNumber(string extNumber)
        {
            string strSql = "SELECT TOP 1 * FROM V_SGP where ExtNumber = @EXTnumber ORDER BY Number DESC";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@EXTnumber", extNumber)).Tables[0];

            if (dt != null && dt.Rows.Count > 0)
            {
                RFQDetail detail = new RFQDetail();
                FillRFQData(detail, dt.Rows[0]);
                return detail;
            }
            return null;
        }
        public static RFQDetail GetDetailById(int rfqid)
        {
            List<RFQDetail> rfqs = GetDetail(rfqid);
            if (rfqs != null && rfqs.Count > 0) return rfqs[0];
            return null;
        }
        /// <summary>
        /// 根据Number查找RFQ
        /// </summary>
        /// <param name="intNumber"></param>
        /// <returns></returns>
        public static RFQDetail GetDetailByInternalNumber(string intNumber)
        {
            string strSql = "SELECT * FROM V_SGP where Number = @RFQnumber";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQnumber", intNumber)).Tables[0];

            if (dt != null && dt.Rows.Count > 0)
            {
                RFQDetail detail = new RFQDetail();
                FillRFQData(detail, dt.Rows[0]);
                return detail;
            }
            return null;
        }
        /// <summary>
        /// Get RFQDetail modal class 
        /// </summary>
        /// <param name="RFQID"></param>
        /// <returns></returns>
        public static List<RFQDetail> GetDetail(int RFQID)
        {
            string strSql = "SELECT * FROM V_SGP where rfqid = @RFQID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", RFQID)).Tables[0];

            List<RFQDetail> DetailData = new List<RFQDetail>();

            if (dt.Rows.Count == 0)
            {
                return DetailData;
            }


            foreach (DataRow dr in dt.Rows)
            {
                RFQDetail Detail = new RFQDetail();

                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    FillRFQData(Detail, dr);
                }
                DetailData.Add(Detail);

            }

            return DetailData;
        }

        private static void FillRFQData(RFQDetail Detail, DataRow dr)
        {
            Type tp = typeof(RFQDetail);
            //FieldInfoCollecton fields = FieldCategory.GetAllFields();
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string propertyName = dr.Table.Columns[i].ColumnName;
                PropertyInfo propertyinfo = tp.GetProperty(propertyName);
                //if (propertyinfo == null) { 
                //    BI.SGP.BLL.DataModels.FieldInfo fi=fields.Find(t=>t.FieldName)
                //}
                if (propertyinfo != null && dr[i] != DBNull.Value)
                {
                    propertyinfo.SetValue(Detail, dr[i], null);
                }
            }
            Detail.ID = Detail.RFQID;
        }

        public static bool RecordIsExists(string number)
        {
            string strSql = "SELECT COUNT(*) FROM SGP_RFQ WHERE Number = @Number";
            return DbHelperSQL.Exists(strSql, new SqlParameter("@Number", number));
        }

        /// <summary>
        /// 根据Internal NUMBER 查找RFQID
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int GetRFQID(string number)
        {
            int Id = 0;
            if (number != null && number.Trim() != "")
            {
                string strSql = "SELECT ID FROM SGP_RFQ WHERE Number = @Number";
                string strId = Convert.ToString(DbHelperSQL.GetSingle(strSql, new SqlParameter("@Number", number)));
                int.TryParse(strId, out Id);
            }
            return Id;
        }
        public static void UpdateSpecialFields(int rfqid)
        {
            List<RFQDetail> rfqdetails = GetDetail(rfqid);
            if (rfqdetails != null)
            {
                RFQDetail rfqdetail = rfqdetails[0];

                string strsqlforPricing = string.Format(@"update SGP_RFQPricing set 
                                                TargetASP={1},
                                                MinASP={2},
                                                TargetSqIn={3},
                                                TargetCLsqin={4},
                                                MinCLsqin={5},
                                                TargetVSActucal={6},
                                                MOQ={7},
                                                OP1={8},OP2={9},OP3={10},OP4={11},OP5={12},OP6={13},OP7={14},OP8={15},OP9={16},OP10={17},
                                                MP1={18},MP2={19},MP3={20},MP4={21},MP5={22},MP6={23},MP7={24},MP8={25},MP9={26},MP10={27}                                              
                                                where rfqid={0}"
                                               , rfqid, rfqdetail.TargetASP
                                               , rfqdetail.MinASP, rfqdetail.TargetSqIn, rfqdetail.TargetCLsqin, rfqdetail.MinCLsqin
                                               , rfqdetail.TargetVSActucal, rfqdetail.MOQ, rfqdetail.OP1, rfqdetail.OP2, rfqdetail.OP3
                                               , rfqdetail.OP4, rfqdetail.OP5, rfqdetail.OP6, rfqdetail.OP7, rfqdetail.OP8, rfqdetail.OP9, rfqdetail.OP10
                                               , rfqdetail.MP1, rfqdetail.MP2, rfqdetail.MP3, rfqdetail.MP4, rfqdetail.MP5, rfqdetail.MP6
                                               , rfqdetail.MP7, rfqdetail.MP8, rfqdetail.MP9, rfqdetail.MP10);
                string strsqlforProduction = string.Format(@"update SGP_RFQProduction set
                                                PanelUtilization='{1}'
                                                where rfqid={0}"
                                               , rfqid, rfqdetail.PanelUtilization);
                string strsqlforCosting = string.Format(@"update SGP_RFQCostingProfitability set
                                                TotalCost={1}, 
                                                OP={2},
                                                MP={3}
                                                where rfqid={0}"
                                               , rfqid, rfqdetail.TotalCost, rfqdetail.OP, rfqdetail.MP);
                DbHelperSQL.ExecuteSql(strsqlforPricing);
                DbHelperSQL.ExecuteSql(strsqlforProduction);
                DbHelperSQL.ExecuteSql(strsqlforCosting);
            }


        }

        private static object _lockObjForRfqId = new object();

        public static int CreateNewRFQID(string uid)
        {
            string number = string.Empty;
            return CreateNewRFQID(uid, out number);
        }
        /// <summary>
        /// 创建新的RFQ ID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int CreateNewRFQID(string uid, out string internalNumber)
        {
            string oldBuilding = string.Empty;
            string oldInternalRev = string.Empty;
            //锁定，确保单个创建
            lock (_lockObjForRfqId)
            {
                string newNumber = string.Empty;
                int lastNumberInt = 0;
                int numberLen = 7;
                string prefix = string.Format("GP{0}", uid.Substring(0, 3));
                prefix = prefix.ToUpper();
                string sql = string.Format("SELECT TOP 1 [ExtNumber] from SGP_RFQ WHERE [ExtNumber] LIKE '{0}%' order by [ExtNumber] DESC", prefix);
                string lastNumber = DbHelperSQL.GetSingle<string>(sql);


                if (string.IsNullOrEmpty(lastNumber))
                {
                    lastNumberInt = 0;
                }
                else
                {
                    //判断号码当中是否有横杠，如有，去掉横杠后面的部分
                    int p = lastNumber.IndexOf('-');
                    if (p > 0)
                    {
                        lastNumber = lastNumber.Substring(0, p);
                    }
                    string s = lastNumber.Substring(prefix.Length);
                    Int32.TryParse(s, out lastNumberInt);
                    lastNumberInt++;
                }

                string newExtNumber = string.Format("{0}{1}-000", prefix, lastNumberInt.ToString().PadLeft(numberLen, '0'));
                newNumber = newExtNumber;//= string.Format("{0}-000", newExtNumber);

                internalNumber = newNumber;
                int rfqId = CreateNewRFQ4AllTables(newNumber, newExtNumber, string.Empty, "01", uid);

                return rfqId;
            }
        }

        /// <summary>
        /// 初始化数据，插入所有表
        /// </summary>
        /// <param name="newNumber"></param>
        /// <param name="newExtNumber"></param>
        /// <param name="rev"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private static int CreateNewRFQ4AllTables(
            string newNumber, string newExtNumber, string building, string rev, string uid)
        {
            string strSql = string.Format("INSERT INTO SGP_RFQ(Number,ExtNumber,Building,InternalRevisionNumber, EmployeeID) VALUES('{0}','{1}','{2}','{3}','{4}');SELECT @@IDENTITY"
                , newNumber, newExtNumber, building, rev, uid);
            int rfqId = DbHelperSQL.GetSingle<int>(strSql);

            strSql = string.Format("INSERT INTO SGP_RFQProduction (RFQID)VALUES({0})", rfqId); DbHelperSQL.ExecuteSql(strSql);
            strSql = string.Format("INSERT INTO SGP_RFQGeneral (RFQID)VALUES({0})", rfqId); DbHelperSQL.ExecuteSql(strSql);
            strSql = string.Format("INSERT INTO SGP_RFQCostingProfitability (RFQID)VALUES({0})", rfqId); DbHelperSQL.ExecuteSql(strSql);
            strSql = string.Format("INSERT INTO SGP_RFQPricing (RFQID)VALUES({0})", rfqId); DbHelperSQL.ExecuteSql(strSql);
            strSql = string.Format("INSERT INTO SGP_RFQClosure (RFQID)VALUES({0})", rfqId); DbHelperSQL.ExecuteSql(strSql);

            //将旧的一条数据的状态改为删除，同一个EXTNUMBER的数据的条数大于1，则将EXTNUMBER=NUMBER的一条删除
            strSql = string.Format(@"
UPDATE SGP_RFQ SET STATUSID=0
WHERE ExtNumber='{0}' AND Number='{0}' 
AND EXISTS(SELECT 1 FROM(SELECT COUNT(1) C FROM SGP_RFQ WHERE ExtNumber='{0}') T WHERE C>1)
", newExtNumber);
            DbHelperSQL.ExecuteSql(strSql);

            return rfqId;
        }

        /// <summary>
        /// 在BUILDING改变的时候使用这个方法，创建新数据，或者根据情况返回已存在的数据的ID
        /// </summary>
        /// <param name="oldRFQId"></param>
        /// <param name="rfqDetail"></param>
        /// <param name="newBuilding"></param>
        /// <param name="newRev"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private static int CreateNewRFQID4BuildingChange(int oldRFQId, RFQDetail rfqDetail, string newBuilding, string newRev, string uid, out string newInternalNumber)
        {
            newInternalNumber = rfqDetail.Number;
            newRev = newRev.PadLeft(2, '0');
            string oldExtNumber = rfqDetail.ExtNumber;

            //如果BUILDING为空
            if (string.IsNullOrEmpty(newBuilding) && string.IsNullOrEmpty(rfqDetail.Building)) return oldRFQId;

            //如果新BUILDING为空，旧BUILDING不为空
            if (string.IsNullOrEmpty(newBuilding))
            {
                //throw new Exception
            }

            //如果BUILDING没有改变
            if (newBuilding == rfqDetail.Building)
            {
                //如果版本为空或者版本没有改变，则返回
                if (string.IsNullOrEmpty(newRev) || newRev == rfqDetail.InternalRevisionNumber)
                {
                    return oldRFQId;
                }
            }

            //如果没有提供REV则默认为01
            if (string.IsNullOrEmpty(newRev)) newRev = "01";

            newRev = newRev.PadLeft(2, '0');


            //合成新的内部号码，格式：GPMCN0023066-000-05B4
            newInternalNumber = string.Format("{0}-{1}{2}", oldExtNumber, newRev, newBuilding);

            //检查该内部号码是否已经存在
            string sql = string.Format("SELECT ID FROM SGP_RFQ WHERE Number='{0}'", newInternalNumber);
            int existsId = DbHelperSQL.GetSingle<int>(sql);

            if (existsId == oldRFQId) return oldRFQId;

            //如果已经存在记录，并且不是当前记录的话
            if (existsId != 0 && existsId != oldRFQId)
            {
                return existsId;
                //throw new Exception(string.Format("The RFQ[{0}] for Building[{1}], rev[{2}] alrady exists in system."
                //    , newInternalNumber, newBuilding, newRev));
            }

            //创建新的RFQ数据
            int rfqId = CreateNewRFQ4AllTables(newInternalNumber, oldExtNumber, newBuilding, newRev, uid);

            //克隆数据从旧的RFQ
            CloneData(oldRFQId, rfqId);
            //将新的RFQ状态同步过去
            CloneWorkflowStatus(oldRFQId, rfqId);
            //克隆附件
            CloneAttachments(oldRFQId, rfqId);

            return rfqId;
        }

        /// <summary>
        /// 克隆附件
        /// </summary>
        /// <param name="rfqIdFrom"></param>
        /// <param name="rfqIdTo"></param>
        private static void CloneAttachments(int rfqIdFrom, int rfqIdTo)
        {
            string sql = string.Format(@"
INSERT INTO SGP_Files(
[RelationKey],[FileName],[SourceName],[Folder],[FileSize],[CreateTime],[Creator],[Status])
SELECT 
{1},[FileName],[SourceName],[Folder],[FileSize],[CreateTime],[Creator],[Status]
  FROM [SGP_Files] WHERE RelationKey='{0}'
", rfqIdFrom, rfqIdTo);
            DbHelperSQL.ExecuteSql(sql);
        }

        /// <summary>
        /// 克隆WF状态和历史记录
        /// </summary>
        /// <param name="rfqIdFrom"></param>
        /// <param name="rfqIdTo"></param>
        private static void CloneWorkflowStatus(int rfqIdFrom, int rfqIdTo)
        {
            string sql = string.Format(@"
INSERT INTO SGP_CurrentUserTask(TemplateID,ActivityId,EntityId,UID)
SELECT TemplateID,ActivityId,{0},UID from SGP_CurrentUserTask WHERE EntityId={1}"
                , rfqIdTo, rfqIdFrom);
            DbHelperSQL.ExecuteSql(sql);

            sql = string.Format("UPDATE SGP_RFQ SET ActivityId=(SELECT ActivityId FROM SGP_RFQ WHERE ID={0}) WHERE ID={1}"
                , rfqIdFrom, rfqIdTo);
            DbHelperSQL.ExecuteSql(sql);

            sql = string.Format(@"
INSERT INTO SYS_WFProcessLog (EntityId,TemplateID,FromActivityId,ToActivityId,ActionUser,ActionTime,Comment) 
SELECT {0},TemplateID,FromActivityId,ToActivityId,ActionUser,ActionTime,'CLONE' FROM SYS_WFProcessLog 
WHERE EntityId={1} ORDER BY ID ASC", rfqIdTo, rfqIdFrom);
            DbHelperSQL.ExecuteSql(sql);

        }

        private static int CreateNewRFQID4ReQuote(int oldRFQId, RFQDetail rfqDetail, string uid)
        {
            string internalNumber = rfqDetail.Number;
            string extNumber = rfqDetail.ExtNumber;
            string newSeq = "";
            int p = 0;

            string prefix = extNumber;
            p = extNumber.IndexOf('-');
            if (p > 0)
            {
                string[] ary = extNumber.Split('-');
                prefix = ary[0];
            }

            string sql = string.Format("SELECT ExtNumber from SGP_RFQ WHERE ExtNumber LIKE '{0}%' ORDER BY ExtNumber DESC", prefix);
            string lastExtNumber = DbHelperSQL.GetSingle<string>(sql);
            p = lastExtNumber.IndexOf('-');
            if (p > 0)
            {
                string[] ary = lastExtNumber.Split('-');
                string lastSeq = ary[1];
                int intLastSeq = 0;
                Int32.TryParse(lastSeq, out intLastSeq);
                intLastSeq++;
                newSeq = intLastSeq.ToString().PadLeft(3, '0');
            }
            if (string.IsNullOrEmpty(newSeq)) newSeq = "001";

            string newExtNumber = string.Format("{0}-{1}", prefix, newSeq);
            string newIntNumber = string.Format("{0}", newExtNumber);
            //创建新的RFQ数据
            int rfqId = CreateNewRFQ4AllTables(newIntNumber, newExtNumber, string.Empty, "01", uid);

            //将新的数据复制过去
            CloneData(oldRFQId, rfqId);

            return rfqId;
        }

        /// <summary>
        /// 点击REQUOTE时执行
        /// </summary>
        /// <param name="rfqId"></param>
        /// <param name="dicAllTable"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool ReQuote(ref int rfqId, Dictionary<string, Dictionary<string, string>> dicAllTable, FieldInfoCollecton fields)
        {
            string uid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;

            string internalNumber = string.Empty;
            if (dicAllTable["SGP_RFQ"] != null && dicAllTable["SGP_RFQ"]["Number"] != null) internalNumber = string.Format("{0}", dicAllTable["SGP_RFQ"]["Number"]);

            RFQDetail rfqOld = GetDetailByInternalNumber(internalNumber);

            //Insert(ref rfqId, dicAllTable, fields);

            //string sql = string.Format("UPDATE SGP_RFQ SET ");

            rfqId = rfqOld.ID;

            rfqId = CreateNewRFQID4ReQuote(rfqId, rfqOld, uid);
            string sql = string.Format("update SGP_RFQGeneral set RFQDateIn=GETDATE(),RFQDateOut=null,QuoteDateIn=null,QuoteDateOut=null,PriceDateOut=null where RFQID={0}",rfqId);

            DbHelperSQL.ExecuteSql(sql);

            return UpdateData(rfqId, dicAllTable, fields);

            //return true;
        }

        public static bool __Insert(ref int rfqId, Dictionary<string, Dictionary<string, string>> dicAllTable, FieldInfoCollecton fields)
        {
            string uid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;

            string strSql = string.Empty;
            //string strSql = "INSERT INTO SGP_RFQ(Number, EmployeeID) VALUES(dbo.fx_newGPRFQ('" + uid + "'), '" + uid + "');SELECT @@IDENTITY";

            using (TScope ts = new TScope())
            {
                try
                {
                    rfqId = CreateNewRFQID(uid);
                    //string tempId = Convert.ToString(DbHelperSQL.GetSingle(strSql));
                    string tempId = rfqId.ToString();


                    //int.TryParse(tempId, out rfqId);
                    if (rfqId > 0)
                    {
                        if (dicAllTable.Keys.Contains("SGP_RFQ"))
                        {
                            dicAllTable.Remove("SGP_RFQ");
                        }

                        foreach (string tableName in dicAllTable.Keys)
                        {
                            List<SqlParameter> listParames = new List<SqlParameter>();
                            string strField = "RFQID,";
                            string strValue = "@RFQID,";
                            listParames.Add(new SqlParameter("@RFQID", rfqId));
                            foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
                            {
                                strField += kvField.Key + ",";
                                strValue += "@" + kvField.Key + ",";
                                if (fields[kvField.Key].DataType == BLL.DataModels.FieldInfo.DATATYPE_DATETIME && String.IsNullOrEmpty(kvField.Value))
                                {
                                    listParames.Add(new SqlParameter("@" + kvField.Key, DBNull.Value));
                                }
                                else
                                {
                                    if (String.IsNullOrEmpty(kvField.Value.Trim()))
                                    {

                                        listParames.Add(new SqlParameter("@" + kvField.Key, DBNull.Value));
                                    }
                                    else
                                    {
                                        listParames.Add(new SqlParameter("@" + kvField.Key, kvField.Value));
                                    }
                                }

                            }
                            strField = strField.TrimEnd(',');
                            strValue = strValue.TrimEnd(',');

                            strSql = "INSERT INTO " + tableName + "(" + strField + ") VALUES(" + strValue + ")";

                            DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());
                        }

                        UpdateSpecialFields(rfqId);

                        strSql = "Insert Into SGP_RFQHistory select *,getdate() from v_sgp where RFQID=@RFQID";
                        DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", tempId));


                        List<SqlParameter> Params = new List<SqlParameter>() { new SqlParameter("@RFQID", tempId), new SqlParameter("@UID", uid) };
                        strSql = "Insert into [SYS_WFProcessLog] select @RFQID,1,0,1,getdate(),@UID,NULL";
                        DbHelperSQL.ExecuteSql(strSql, Params.ToArray());

                        return true;
                    }
                    else
                    {
                        ts.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    throw ex;
                }
            }

            return false;
        }

        private static string PrepareInternalNumber(FieldInfoCollecton fields, out string extNumber)
        {
            string internalNumber = string.Empty;
            extNumber = string.Empty;
            SGP.BLL.DataModels.FieldInfo fiNumber = fields.Find(t => string.Compare(t.FieldName, "Number", true) == 0);
            if (fiNumber != null) internalNumber = string.Format("{0}", fiNumber.DataValue);

            SGP.BLL.DataModels.FieldInfo fiExtNumber = fields.Find(t => string.Compare(t.FieldName, "ExtNumber", true) == 0);
            if (fiExtNumber != null) extNumber = string.Format("{0}", fiExtNumber.DataValue);

            //如果内部编号为空，但是外部编号不为空，则合成外部编号
            if (string.IsNullOrEmpty(internalNumber))
            {
                if (string.IsNullOrEmpty(extNumber) == false)
                {
                    string rev = string.Empty;
                    string building = string.Empty;
                    SGP.BLL.DataModels.FieldInfo fiRev = fields.Find(t => string.Compare(t.FieldName, "InternalRevisionNumber", true) == 0);
                    SGP.BLL.DataModels.FieldInfo fiBuilding = fields.Find(t => string.Compare(t.FieldName, "Building", true) == 0);
                    if (fiRev != null) rev = string.Format("{0}", fiRev.DataValue);
                    if (fiBuilding != null) building = string.Format("{0}", fiBuilding.DataValue);

                    if (string.IsNullOrEmpty(rev)) rev = "01";
                    rev = rev.PadLeft(2, '0');

                    if (string.IsNullOrEmpty(building))
                    {
                        internalNumber = extNumber;
                    }
                    else
                    {
                        internalNumber = string.Format("{0}-{1}{2}", extNumber, rev, building);
                    }
                }
            }

            return internalNumber;
        }
        /// <summary>
        /// 更新数据，数据必须先初始化进入所有表
        /// </summary>
        /// <param name="rfqId"></param>
        /// <param name="dicAllTable"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool Update(ref int rfqId, FieldInfoCollecton fields)
        {
            string uid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;

            string internalNumber = string.Empty;
            string extNumber = string.Empty;

            SGP.BLL.DataModels.FieldInfo fiNumber = fields.Find(t => string.Compare(t.FieldName, "Number", true) == 0);
            if (fiNumber != null) internalNumber = string.Format("{0}", fiNumber.DataValue);

            SGP.BLL.DataModels.FieldInfo fiExtNumber = fields.Find(t => string.Compare(t.FieldName, "ExtNumber", true) == 0);
            if (fiExtNumber != null) extNumber = string.Format("{0}", fiExtNumber.DataValue);

            if (string.IsNullOrEmpty(internalNumber)) //如果没有内部号码，则根据外部号码，BUILDING等合成内部号码
            {
                internalNumber = PrepareInternalNumber(fields, out extNumber);
            }

            if (fiNumber != null) fiNumber.DataValue = internalNumber;

            RFQDetail detail = null;
            if (string.IsNullOrEmpty(internalNumber) == false)
            {
                //查找当前记录
                detail = GetDetailByInternalNumber(internalNumber);
                if (detail != null)
                {
                    rfqId = detail.RFQID;
                    extNumber = detail.ExtNumber;
                    if (fiExtNumber != null) fiExtNumber.DataValue = extNumber;
                }

                //在有外部号码的情况下，查找数据不存在，则查找最大的一条外部号码对应的数据
                if (detail == null)
                {
                    detail = GetLastDetailByExtNumber(extNumber);
                    if (detail != null)
                    {
                        rfqId = detail.RFQID;
                    }
                }
            }


            //当前记录不存在，但是内部号码存在的情况，先找到外部号码，然后尝试获取该外部号码对应的一条数据
            if (rfqId <= 0 && string.IsNullOrEmpty(internalNumber) == false && detail == null)
            {
                if (internalNumber.IndexOf('-') > 0)
                {
                    string[] ary = internalNumber.Split('-');
                    if (ary.Length >= 2)
                    {
                        extNumber = string.Format("{0}-{1}", ary[0], ary[1]);
                        detail = GetLastDetailByExtNumber(extNumber);
                        if (detail != null)
                        {
                            rfqId = detail.RFQID;
                            if (fiExtNumber != null) fiExtNumber.DataValue = extNumber;
                        }
                    }
                }
            }


            if (rfqId <= 0)//新数据，则创建数据
            {
                rfqId = CreateNewRFQID(uid, out internalNumber);
                extNumber = internalNumber;
                if (fiNumber != null) fiNumber.DataValue = internalNumber;
                if (fiExtNumber != null) fiExtNumber.DataValue = internalNumber;
            }

            if (detail != null)// 如果在有老数据存在的情况下，判断Building是否有修改，如果有需要一条复制数据
            {
                extNumber = detail.ExtNumber;
                string revOld = detail.InternalRevisionNumber;
                string buildingOld = detail.Building;
                string buildingNew = string.Empty;
                string revNew = string.Empty;
                if (fields["Building"] != null) buildingNew = string.Format("{0}", fields["Building"].DataValue);
                if (fields["InternalRevisionNumber"] != null) revNew = string.Format("{0}", fields["InternalRevisionNumber"].DataValue);

                //创建新数据
                string newNumber = string.Empty;
                rfqId = CreateNewRFQID4BuildingChange(rfqId, detail, buildingNew, revNew, uid, out newNumber);
                if (fiNumber != null) fiNumber.DataValue = newNumber;
            }

            if (rfqId > 0)
            {
                return UpdateData(rfqId, fields);
            }

            return false;
        }



        /// <summary>
        /// 从一个ID向另一个ID复制数据，不包括RFQNUMBER表
        /// </summary>
        /// <param name="rfqIdFrom"></param>
        /// <param name="rfqIdTo"></param>
        /// <returns></returns>
        private static bool CloneData(int rfqIdFrom, int rfqIdTo)
        {
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();

            foreach (BLL.DataModels.FieldInfo field in fields)
            {

                if (field != null && !String.IsNullOrEmpty(field.TableName))
                {
                    string tableName = field.TableName.ToUpper();
                    if (dicAllTable.ContainsKey(tableName))
                    {
                        dicAllTable[tableName].Add(field.FieldName, field.FieldName);
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(field.FieldName, field.FieldName);
                        dicAllTable.Add(tableName, dic);
                    }
                }
            }

            string sql = string.Empty;

            foreach (string tableName in dicAllTable.Keys)
            {
                if (tableName == "SGP_RFQ") continue; //不处理SGPRFQ表

                StringBuilder sqlFrom = new StringBuilder();
                StringBuilder sqlTo = new StringBuilder();
                sqlFrom.AppendFormat("{0}", rfqIdTo);
                sqlTo.Append("RFQID");
                foreach (string fieldName in dicAllTable[tableName].Keys)
                {
                    sqlFrom.AppendFormat(",[{0}]", fieldName);
                    sqlTo.AppendFormat(",[{0}]", fieldName);
                }

                sql = string.Format("DELETE FROM {0} WHERE RFQID={1}", tableName, rfqIdTo);
                DbHelperSQL.ExecuteSql(sql);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO {0} ({1}) SELECT {2} FROM {0} WHERE RFQID={3}"
                    , tableName, sqlTo, sqlFrom, rfqIdFrom);
                DbHelperSQL.ExecuteSql(sb.ToString());

            }



            return true;
        }

        /// <summary>
        /// 将字段放入到字典中准备生成SQL语句
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, string>> PrepareData(FieldInfoCollecton fields)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();

            foreach (BI.SGP.BLL.DataModels.FieldInfo fc in fields)
            {
                if (fc.CurrentlyInUse == false) continue;
                string tableName = fc.TableName.ToUpper();
                if (!dicAllTable.ContainsKey(tableName)) dicAllTable.Add(tableName, new Dictionary<string, string>());

                dicAllTable[tableName].Add(fc.FieldName, string.Format("{0}", fc.DataValue));
            }
            return dicAllTable;
        }

        /// <summary>
        /// 真正写入数据，不做任何判断
        /// </summary>
        /// <param name="rfqId"></param>
        /// <param name="dicAllTable"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool UpdateData(int rfqId, FieldInfoCollecton fields)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = PrepareData(fields);
            return UpdateData(rfqId, dicAllTable, fields);
        }


        /// <summary>
        /// 真正写入数据，不做任何判断
        /// </summary>
        /// <param name="rfqId"></param>
        /// <param name="dicAllTable"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool UpdateData(int rfqId, Dictionary<string, Dictionary<string, string>> dicAllTable, FieldInfoCollecton fields)
        {
            string uid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;

            if (dicAllTable.Keys.Contains("SGP_RFQ"))
            {
                dicAllTable.Remove("SGP_RFQ");
            }

            using (TScope ts = new TScope())
            {
                try
                {
                    BI.SGP.BLL.Event.UserChangedEvent uce = new Event.UserChangedEvent(rfqId);
                    uce.DoBefore();

                    string strSql = "";
                    foreach (string tableName in dicAllTable.Keys)
                    {
                        List<SqlParameter> listParames = new List<SqlParameter>();
                        string strField = "";

                        foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
                        {
                            if (kvField.Key == "RFQDateIn") continue;
                            if (kvField.Key == "RFQDateOut") continue;
                            if (kvField.Key == "QuoteDateIn") continue;
                            if (kvField.Key == "QuoteDateOut") continue;
                            if (kvField.Key == "PriceDateOut") continue;

                            strField += kvField.Key + "=@" + kvField.Key + ",";
                            if (fields[kvField.Key].DataType == BLL.DataModels.FieldInfo.DATATYPE_DATETIME && String.IsNullOrEmpty(kvField.Value))
                            {
                                listParames.Add(new SqlParameter("@" + kvField.Key, DBNull.Value));
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(kvField.Value.Trim()))
                                {

                                    listParames.Add(new SqlParameter("@" + kvField.Key, DBNull.Value));
                                }
                                else
                                {
                                    listParames.Add(new SqlParameter("@" + kvField.Key, kvField.Value));
                                }
                            }

                        }
                        strField = strField.TrimEnd(',');
                        listParames.Add(new SqlParameter("@RFQID", rfqId));
                        strSql = String.Format("UPDATE SGP_RFQ SET EmployeeID = '{0}', LastUpdate=getdate() WHERE ID = {1};", uid, rfqId);
                        strSql += "UPDATE " + tableName + " SET " + strField + " WHERE RFQID = @RFQID";

                        DbHelperSQL.ExecuteSql(strSql, listParames.ToArray());

                    }

                    UpdateSpecialFields(rfqId);

                    strSql = "Insert Into SGP_RFQHistory select *,getdate() from v_sgp where RFQID=@RFQID";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@RFQID", rfqId));

                    uce.DoAfter();
                    return true;
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    throw ex;
                }
            }
            return true;
        }

        /// <summary>
        /// 保存数据，返回结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueCollection Save(HttpRequestBase request)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            NameValueCollection ExecResult = new NameValueCollection();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);

            //将数据从FORM中读取到FIELDS
            foreach (string key in request.Form.Keys)
            {
                BLL.DataModels.FieldInfo field = fields[key];
                if (field != null)
                {
                    string v = request.Form[key];
                    if (string.IsNullOrWhiteSpace(v) == false) v = HttpUtility.UrlDecode(v);
                    field.DataValue = v;
                }
                if (field != null && !String.IsNullOrEmpty(field.TableName))
                {
                    string tableName = field.TableName.ToUpper();
                    if (dicAllTable.ContainsKey(tableName))
                    {
                        dicAllTable[tableName].Add(key, request.Form[key]);
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(key, request.Form[key]);
                        dicAllTable.Add(tableName, dic);
                    }
                }
            }

            //判断是否是已有的数据
            int rfqId = 0;//GetRFQID(request["Number"]);

            try
            {
                Update(ref rfqId, fields);
                ExecResult.Add("RFQID", rfqId.ToString());
            }
            catch (Exception ex)
            {
                ExecResult.Add("RFQID", rfqId.ToString());
                ExecResult.Add("Mgs", ex.Message);

            }


            return ExecResult;
        }
        public static NameValueCollection SaveAs(HttpRequestBase request)
        {
            //Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            NameValueCollection ExecResult = new NameValueCollection();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);

            foreach (string key in request.Form.Keys)
            {
                BLL.DataModels.FieldInfo field = fields[key];
                if (field != null && !String.IsNullOrEmpty(field.TableName))
                {
                    field.DataValue = request.Form[key];
                    field.CurrentlyInUse = true;
                }
            }

            int rfqId = 0;

            try
            {
                try
                {
                    // using (TransactionScope tscope = new TransactionScope())
                    {
                        //Insert(ref rfqId, dicAllTable, fields);
                        string uid = BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid;
                        string newIntNumber = string.Empty;
                        rfqId = CreateNewRFQID(uid, out newIntNumber);
                        BLL.DataModels.FieldInfo fldNumber = fields.Find(t => string.Compare(t.FieldName, "Number") == 0);
                        if (fldNumber != null) fldNumber.DataValue = newIntNumber;
                        Update(ref rfqId, fields);
                        ExecResult.Add("RFQID", rfqId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ExecResult.Add("Mgs", ex.ToString());
                }


            }
            catch (Exception ex)
            {
                ExecResult.Add("RFQID", rfqId.ToString());
                ExecResult.Add("Mgs", ex.ToString());
            }




            return ExecResult;
        }

        public static NameValueCollection ReQuoteData(HttpRequestBase request)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            NameValueCollection ExecResult = new NameValueCollection();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);

            //将数据从FORM中读取到FIELDS
            foreach (string key in request.Form.Keys)
            {
                BLL.DataModels.FieldInfo field = fields[key];
                if (field != null)
                {
                    field.DataValue = request.Form[key];
                }
                if (field != null && !String.IsNullOrEmpty(field.TableName))
                {
                    string tableName = field.TableName.ToUpper();
                    if (dicAllTable.ContainsKey(tableName))
                    {
                        dicAllTable[tableName].Add(key, request.Form[key]);
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add(key, request.Form[key]);
                        dicAllTable.Add(tableName, dic);
                    }
                }
            }

            //判断是否是已有的数据
            int rfqId = 0;//GetRFQID(request["Number"]);

            try
            {
                ReQuote(ref rfqId, dicAllTable, fields);
                ExecResult.Add("RFQID", rfqId.ToString());
            }
            catch (Exception ex)
            {
                ExecResult.Add("RFQID", rfqId.ToString());
                ExecResult.Add("Mgs", ex.Message);

            }


            return ExecResult;
        }
        public static bool SaveforExcel(DataRow dr)
        {
            SystemMessages sysmgs = new SystemMessages();

            //补齐内部版本号为两位
            if (dr.Table.Columns.Contains("InternalRevisionNumber"))
            {
                string s = string.Format("{0}", dr["InternalRevisionNumber"]);
                s = s.PadLeft(2, '0');
                if (s == "00") s = "01";
                dr["InternalRevisionNumber"] = s;
            }

            //check data检查数据，返回状态到sysmgs
            RFQManager.CheckData(dr, ref sysmgs);
            if (sysmgs.isPass == false)
            {
                if (dr["Message"] != null)
                {
                    dr["Message"] = sysmgs.MessageString;
                    return false;
                }
            }

            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGP);
            //NameValueCollection ExecResult = new NameValueCollection();
            //List<string> strcol = new List<string>();
            //List<string> strcoldispaly = new List<string>();

            //不存在的数据
            StringBuilder theNotExistsColumns = new StringBuilder();

            //载入值到fields中
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string columnName = dr.Table.Columns[i].ColumnName;
                if (string.Compare(columnName, "Message") == 0) continue;
                if (string.Compare(columnName, "RFQID") == 0) continue;
                if (string.Compare(columnName, "RFQDateIn") == 0) continue;
                if (string.Compare(columnName, "RFQDateOut") == 0) continue;
                if (string.Compare(columnName, "QuoteDateIn") == 0) continue;
                if (string.Compare(columnName, "QuoteDateOut") == 0) continue;
                if (string.Compare(columnName, "PriceDateOut") == 0) continue;
                if (string.Compare(columnName, "WFStatus") == 0) continue;
                if (string.Compare(columnName, "Workflow Status") == 0) continue;
                	
                BI.SGP.BLL.DataModels.FieldInfo fi = fields.Find(t => string.Compare(t.FieldName, columnName, true) == 0);
                if (fi == null)
                {
                    theNotExistsColumns.AppendFormat("{0},", columnName); continue;
                }

                fi.DataValue = dr[i];
            }


            if (theNotExistsColumns.Length > 0)
            {
                dr["Message"] += theNotExistsColumns.ToString();
                dr["Message"] += " do not exist.";
                return false;
            }


            try
            {
                int rfqId = 0;
                bool UpdatePass = Update(ref rfqId, fields);
                if (UpdatePass == true)
                {
                    BLL.DataModels.FieldInfo fiNumber = fields.Find(t => string.Compare(t.FieldName, "Number") == 0);
                    if (fiNumber != null) dr["Number"] = string.Format("{0}", fiNumber.DataValue);
                    BLL.DataModels.FieldInfo fiExtNumber = fields.Find(t => string.Compare(t.FieldName, "ExtNumber") == 0);
                    if (fiExtNumber != null) dr["ExtNumber"] = string.Format("{0}", fiExtNumber.DataValue);
                    dr["RFQID"] = rfqId;

                }
            }
            catch (Exception ex)
            {
                dr["Message"] = ex.Message;
                return false;
            }


            return true;

        }

        public static int RigidPCBORFPC(DataRow dr)
        {
            ///0 Fail,1 PCB, 2 FPC
            ///
            SystemMessages sysmgs = new SystemMessages();
            int result=0;
            DataTable dt = dr.Table;
            DataColumnCollection cols = dt.Columns;
            string[] keycols = { "Number", "Internal RFQ Number", "External RFQ Number", "ExtNumber" };
            string[] typecols = { "TypeOfQuote", "Type Of Product" };
            string[] defineFPC={"FPC","FPCA"};
            ArrayList keylist = new ArrayList();
            ArrayList typelist = new ArrayList();

            foreach (DataColumn col in cols)
            {
                if (keycols.Contains(col.ColumnName))
                {
                    if (string.IsNullOrEmpty(dr[col.ColumnName].ToString()) == false)
                    {
                        keylist.Add(dr[col.ColumnName].ToString());
                    }
                }
                if (typecols.Contains(col.ColumnName))
                {
                    if (string.IsNullOrEmpty(dr[col.ColumnName].ToString()) == false)
                    {
                        typelist.Add(dr[col.ColumnName].ToString());
                    }
                }
            }
            RFQDetail rfqdeatil = new RFQDetail();

           
            if (typelist.Count > 0)
            {
                foreach (string t in typelist)
                {
                    if (defineFPC.Contains(t))
                    {
                        result = 2;
                    }
                    else
                    {
                        result = 1;
                    }
                
                }

            }
            else if (keylist.Count > 0)
            {
                foreach (string k in keylist)
                {
                    rfqdeatil = RFQManager.GetLastDetailByExtNumber(k.Substring(0, 16));
                    if (rfqdeatil != null)
                    {
                        if (defineFPC.Contains(rfqdeatil.TypeOfQuote))
                        {
                            result = 2;
                        }
                        else
                        {
                            result = 1;
                        }
                    }
                    else
                    {
                        result = 0;
                    }
                }

            }
            else
            {
                result=0;
            }

           

            return result;
        }


    }
    
}
