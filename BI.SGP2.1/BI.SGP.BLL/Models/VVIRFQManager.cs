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

namespace BI.SGP.BLL.Models
{
    public class VVIRFQManager
    {
        public int? ID
        {
            get;
            set;
        }
        public int? RFQID
        {
            get;
            set;
        }
        public string Number
        {
            get;
            set;
        }
        public string OEM
        {
            get;
            set;
        }
        public string GAMBDM
        {
            get;
            set;
        }
        public string CustomerPartNumber
        {
            get;
            set;
        }
        public string Revision
        {
            get;
            set;
        }
        public string MarketSegment
        {
            get;
            set;
        }
        public string Application
        {
            get;
            set;
        }
        public double MassProductionEAU
        {
            get;
            set;
        }
        public DateTime MassProductionDate
        {
            get;
            set;
        }
        public string Vendor
        {
            get;
            set;
        }
        public string CapabilityCheck
        {
            get;
            set;
        }

        public double LayerCount
        {
            get;
            set;
        }
        public string MaterialCategory
        {
            get;
            set;
        }
        public string LaminateType
        {
            get;
            set;
        }
        public double Holes
        {
            get;
            set;
        }
        public double SmallestHole
        {
            get;
            set;
        }
        public string UnitType
        {
            get;
            set;
        }
        public double BoardThickness
        {
            get;
            set;
        }
        public string Finishing
        {
            get;
            set;
        }
        public string Copper
        {
            get;
            set;
        }
        public double UnitSizeWidth
        {
            get;
            set;
        }
        public double UnitSizeLength
        {
            get;
            set;
        }
        public string UnitOrArray
        {
            get;
            set;
        }

        public double ArraySizeWidth
        {
            get;
            set;
        }
        public double ArraySizeLength
        {
            get;
            set;
        }
        public double UnitPerArray
        {
            get;
            set;
        }
        public double PanelUtilization
        {
            get;
            set;
        }
        public string LnO
        {
            get;
            set;
        }
        public string LnI
        {
            get;
            set;
        }
        public string Outline
        {
            get;
            set;
        }
        public string Imped
        {
            get;
            set;
        }
        public string Finger
        {
            get;
            set;
        }
        public string Coverlay
        {
            get;
            set;
        }
        public string DepthControlDrill
        {
            get;
            set;
        }
        public string EdgePlating
        {
            get;
            set;
        }
        public string PeelableMask
        {
            get;
            set;
        }
        public string CarbonInk
        {
            get;
            set;
        }
        public string Class3
        {
            get;
            set;
        }
        public string USize
        {
            get;
            set;
        }
        public string UQty
        {
            get;
            set;
        }
        public string BlindSize
        {
            get;
            set;
        }
        public string BlindQty
        {
            get;
            set;
        }
        public string ShipmentTerms
        {
            get;
            set;
        }
        public string LeadTime
        {
            get;
            set;
        }
        public string CancellationWindows
        {
            get;
            set;
        }
        public double MOV
        {
            get;
            set;
        }
        public string Curreny
        {
            get;
            set;
        }
        public double NRECharge
        {
            get;
            set;
        }
        public double VariableCost
        {
            get;
            set;
        }
        public double SqInchPrice
        {
            get;
            set;
        }
        public string TechnicalRemarks
        {
            get;
            set;
        }
        public string VVIQuotationRemarks
        {
            get;
            set;
        }
        public double TargetPrice
        {
            get;
            set;
        }
        public string MultekQuoteOutPricing
        {
            get;
            set;
        }
        public double OP
        {
            get;
            set;
        }
        public string VendorXPlan
        {
            get;
            set;
        }
        public string OPWithXPlan
        {
            get;
            set;
        }
        public string ManagementApproved
        {
            get;
            set;
        }
        public string HitRateStatus
        {
            get;
            set;
        }
        public string ProjectNumber
        {
            get;
            set;
        }
        public string CustomerPNDescription
        {
            get;
            set;
        }
        public string SampleQTY
        {
            get;
            set;
        }
        
        public static bool CheckData(HttpRequestBase Request, ref SystemMessages message)
        {
            message = new SystemMessages();
            FieldInfoCollecton allFields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_VVI);

            foreach (string key in Request.Form.Keys)
            {
                CheckData(key, Request.Form[key], allFields, ref message);
            }
            return true;
        }
        public static bool CheckData(DataRow dr, ref SystemMessages message)
        {
            message = new SystemMessages();
            FieldInfoCollecton allFields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_VVI);

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

        public static List<VVIRFQManager> GetDetail(int ID)
        {
         
            string strSql = "SELECT * FROM V_SGPForVVI WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", ID)).Tables[0];


            List<VVIRFQManager> DetailData = new List<VVIRFQManager>();

            if (dt.Rows.Count == 0)
            {
                return DetailData;
            }


            foreach (DataRow dr in dt.Rows)
            {
                VVIRFQManager Detail = new VVIRFQManager();

                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    FillData(Detail, dr);
                }
                DetailData.Add(Detail);

            }

            return DetailData;
        }
        public static VVIRFQManager GetDetailById(int ID)
        {
            List<VVIRFQManager> rfqs = VVIRFQManager.GetDetail(ID);
            if (rfqs != null && rfqs.Count > 0) return rfqs[0];
            return null;
        }
        public static int IsPost(string RFQID)
        {
            string strSql = "SELECT count(*) FROM V_SGPForVVI WHERE RFQID = @RFQID";
            int ID = 0;
            ID = (int)DbHelperSQL.GetSingle(strSql, new SqlParameter("@RFQID", RFQID));
            return ID;
        }
        public static int GetRFQID(string ID)
        {
            string strSql = "SELECT RFQID FROM V_SGPForVVI WHERE ID = @ID";
            int vvirfqid = 0;
            vvirfqid = (int)DbHelperSQL.GetSingle(strSql, new SqlParameter("@ID", ID));
            return vvirfqid;
        }
        public static bool CheckVVINumberExists(string vvinumber)
        {
            string strSql = "SELECT top 1 EntityName FROM SGP_SubData WHERE EntityName = @vvinumber";   
            bool isexistnumber = DbHelperSQL.Exists(strSql, new SqlParameter("@vvinumber", vvinumber));
            return isexistnumber;
        }

        //public static int GetRFQID(string vvinumber)
        //{
        //    string strSql = "SELECT RFQID FROM V_SGPForVVI WHERE RFQID = @VVINumber";
        //    int RFQID = 0;
        //    RFQID = (int)DbHelperSQL.GetSingle(strSql, new SqlParameter("@VVINumber", vvinumber));
        //    return RFQID;
        //}


        private static void FillData(VVIRFQManager VVIRFQData, DataRow dr)
        {
            Type tp = typeof(VVIRFQManager);
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string propertyName = dr.Table.Columns[i].ColumnName;
                PropertyInfo propertyinfo = tp.GetProperty(propertyName);
                if (propertyinfo != null && dr[i] != DBNull.Value)
                {
                    propertyinfo.SetValue(VVIRFQData, dr[i], null);
                }
            }
        }

        public static DataTable GetVVIRFQData(string id)
        {
            string strSql = "SELECT * FROM V_SGPForVVI WHERE StatusID = 1 AND ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ID", id)).Tables[0];
            return dt;
        }

        //删除 把字段DeleteState标为1
        public static void DeleteVVIRFQData(string id)
        {
            string strSql = "UPDATE SGP_RFQForVVI SET StatusID = 0 WHERE RFQID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        /// <summary>
        /// Does a RFQ exist in VVI
        /// </summary>
        /// <param name="rfqId"></param>
        /// <returns></returns>
        /// Lance Chen 20150423
        public static bool IsExistInVVI(string rfqId)
        {
            string strSql = @"select *
from SGP_RFQ r
join SGP_RFQForVVI v on v.Number = r.Number
where r.ID = @ID";
            DataSet ds = DbHelperSQL.Query(strSql, new SqlParameter("@ID", rfqId));
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string NullToEmpty(string s)
        {
            return s == null ? "" : s.Trim();
        }

        public object ConvertValue(object strValue)
        {
            if (strValue != null || Convert.ToString(strValue).Trim() != String.Empty)
            {
                return strValue;
            }
            return DBNull.Value;
        }

        public static int ConvertToInt32(object Value)
        {
            if (Value == null || Convert.ToString(Value).Trim() == String.Empty || Value == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(Value);
            }
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
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_VVI);

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

            int vvirfqId = ConvertToInt32(request.Form["ID"]);
            try
            {
                if (vvirfqId > 0)
                {
                    //Update
                    UpdateOperation(dicAllTable, vvirfqId);
                }
                else
                {
                    //Insert
                    InsertOperation(ref vvirfqId, dicAllTable);
                }

                ExecResult.Add("ID", vvirfqId.ToString());
            }
            catch (Exception ex)
            {
                ExecResult.Add("ID", vvirfqId.ToString());
                ExecResult.Add("Mgs", ex.Message);
            }

            return ExecResult;
        }
        public static NameValueCollection AssignSuppliers(HttpRequestBase request)
        {
            Dictionary<string, Dictionary<string, string>> dicAllTable = new Dictionary<string, Dictionary<string, string>>();
            NameValueCollection ExecResult = new NameValueCollection();
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_VVI);
            FieldInfoCollecton supplierfields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_SGPFORSUPPLIER);



            //将数据从FORM中读取到FIELDS
            foreach (string key in request.Form.Keys)
            {
                BLL.DataModels.FieldInfo field = fields[key];
                foreach(BLL.DataModels.FieldInfo fi in supplierfields)
                {
                    if (field.DisplayName == fi.DisplayName && string.IsNullOrEmpty(field.SubDataType))
                    {
                        if (field != null)
                        {
                            string v = request.Form[key];
                            if (string.IsNullOrWhiteSpace(v) == false) v = HttpUtility.UrlDecode(v);
                            fi.DataValue = v;
                        }
                        if (field != null && !String.IsNullOrEmpty(field.TableName))
                        {
                            string tableName = fi.TableName.ToUpper();
                            if (dicAllTable.ContainsKey(tableName))
                            {
                                dicAllTable[tableName].Add(fi.FieldName, request.Form[key]);
                            }
                            else
                            {
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                dic.Add(fi.FieldName, request.Form[key]);
                                dicAllTable.Add(tableName, dic);
                            }
                        }
 
                    }
                }
                 
                
            }


            int rfqid = ConvertToInt32(RFQManager.GetRFQID(request.Form["Number"].ToString()));
            string suppliercode = request.Form["SupplierCode"].ToString();
          
            try
            {


                InsertRFQVVI(rfqid,suppliercode,dicAllTable);
            

              
            }
            catch (Exception ex)
            {
               
                ExecResult.Add("Mgs", ex.Message);
            }

            return ExecResult;
        }


        /// <summary>
        /// 动态构造Insert Sql语句 
        /// </summary>
        public static void InsertOperation(ref int rfqid, Dictionary<string, Dictionary<string, string>> dicAllTable)
        {
            string FileName = "";
            string FileValue = "";
            SqlParameter[] paras = null;
            List<SqlParameter> paraList = new List<SqlParameter>();
            int i = 0;
            foreach (string tableName in dicAllTable.Keys)
            {
                foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
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

                    paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                    i++;
                }
                //FileName += " ,[Creater],[CreationDate] ";
                //FileValue += " ,@Creater,@CreationDate ";
                //paraList.Add(new SqlParameter("@Creater", "" + AccessControl.CurrentLogonUser.Uid + ""));
                //paraList.Add(new SqlParameter("@CreationDate", "" + System.DateTime.Now.ToString() + ""));
                paras = paraList.ToArray();//将List对象转换为数组
                string sql = "INSERT INTO " + tableName + " (" + FileName + ") VALUES (" + FileValue + ");SELECT @@IDENTITY";
                rfqid = DbHelperSQL.GetSingle<int>(sql, paras.ToArray());
            }

        }
        public static void InsertRFQVVI(int rfqid,string suppliercode, Dictionary<string, Dictionary<string, string>> dicAllTable)
        {
            string FileName = "";
            string FileValue = "";
            SqlParameter[] paras = null;
            List<SqlParameter> paraList = new List<SqlParameter>();
            int i = 0;



            foreach (KeyValuePair<string, string> kvField in dicAllTable["SGP_RFQForVVI"])
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
                if (kvField.Key == "EntityName")
                {
                    paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                    paraList.Add(new SqlParameter("@EntityName", "" + kvField.Value + "-" + suppliercode + ""));
                }
                else
                {
                    if (kvField.Key == "EntityName") continue;
                    paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                }
                i++;
            }
            paras = paraList.ToArray();
            string sql = "INSERT INTO VVIDETAIL (" + FileName + ") VALUES (" + FileValue + ");";
            DbHelperSQL.ExecuteSql(sql, paras.ToArray());
            

        }

        public static void UpdateOperationForPostBack(string id)
        {
            string sql = "exec SP_VVIPostBackSGP "+int.Parse(id)+"";
            DbHelperSQL.GetSingle<int>(sql);

        }
        /// <summary>
        /// 动态构造Update Sql语句 
        /// </summary>
        public static void UpdateOperation(Dictionary<string, Dictionary<string, string>> dicAllTable, int vvirfqId)
        {
            string FileValue = "";
            SqlParameter[] paras = null;
            List<SqlParameter> paraList = new List<SqlParameter>();
            int i = 0;
            foreach (string tableName in dicAllTable.Keys)
            {
                
                    foreach (KeyValuePair<string, string> kvField in dicAllTable[tableName])
                    {
                        if (FileValue != string.Empty)
                        {
                            FileValue += ", ";
                        }
                        FileValue += kvField.Key + "=@" + kvField.Key + " ";

                        paraList.Add(new SqlParameter("@" + kvField.Key + "", "" + kvField.Value + ""));
                        i++;
                    }
                    //FileValue += " ,Modifier=@Modifier, ModificationDate=@ModificationDate ";
                    //paraList.Add(new SqlParameter("@Modifier", "" + AccessControl.CurrentLogonUser.Uid + ""));
                    //paraList.Add(new SqlParameter("@ModificationDate", "" + System.DateTime.Now.ToString() + ""));
                    paraList.Add(new SqlParameter("@ID", vvirfqId));
                    paras = paraList.ToArray();//将List对象转换为数组
                    string sql = "UPDATE " + tableName + " SET " + FileValue + " WHERE ID=@ID ";
                    DbHelperSQL.GetSingle<int>(sql, paras.ToArray());
                
            }
        }




        public static void SavaAndCheckData(ref VVIRFQManager vvirfdetail, ref SystemMessages sysmgs,HttpRequestBase request)
        {


            Dictionary<string, object> data = new Dictionary<string, object>();


            foreach(string key in request.Form.Keys)
            {
                if (!data.ContainsKey(key))
                {

                    data.Add(key,request.Form[key]);
                
                }
            }
            VVIRFQManager checkdata = new VVIRFQManager();
            VVIRFQManager.CheckData(request, ref sysmgs);
         
            //检查数据通过则执行保存到数据库
            if (sysmgs.isPass)
            {
                NameValueCollection Result = BI.SGP.BLL.Models.VVIRFQManager.Save(request);
                string ID = Result["ID"];
                int vvirfqid = 0; Int32.TryParse(ID, out vvirfqid);

                //有MSG返回，表示保存失败
                if (string.IsNullOrEmpty(Result["Mgs"]) == false)
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "Save Error";
                    sysmgs.Messages.Add("Save Error", Result["Mgs"]);

                }
                else
                {
                    sysmgs.isPass = true;
                    sysmgs.MessageType = "Save Success";
                    //sysmgs.Messages.Add("Save Success", Result["Mgs"]);
                }

                if (vvirfqid > 0)
                {

                    vvirfdetail = VVIRFQManager.GetDetailById(vvirfqid);
                }

            }
            else // 检查数据不通过
            {
                sysmgs.MessageType = "Error";
            }

        }

    }
}
