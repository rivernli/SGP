using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.IO;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.CostEngine.DataConverter;
using SGP.DBUtility;

namespace BI.SGP.BLL.CostEngine
{
    public abstract class CostBase
    {
        public const string PCBTYPE_RIGID = "Rigid";
        public const string PCBTYPE_FPC = "FPC";
        public const string CYCLETIME_WC = "DRL,ROU,LAS,PUN,ATT";
        public const string CYCLETIME_WC_FPC = "QC1,FQC,FQA,FIN,RUW";
        private string _pcbType;
        public string PCBType 
        {
            get
            {
                if (_pcbType == null)
                {
                    _pcbType = GetPCBType(Convert.ToString(Data["Plant"]));
                }
                return _pcbType;
            }
        }

        public bool ComputeByCycleTime
        {
            get
            {
                string plant = Convert.ToString(Data["Plant"]);
                string workCenter = Convert.ToString(Data["MainWorkCenter"]);
                return IsComputeByCycleTime(plant, workCenter);
            }
        }

        public static string GetPCBType(string plant)
        {
            if (plant == "B2FPC")
            {
                return PCBTYPE_FPC;
            }
            else
            {
                return PCBTYPE_RIGID;
            }
        }

        public static bool IsComputeByCycleTime(string plant, string workCenter)
        {
            return (CYCLETIME_WC.IndexOf(workCenter) > 0 || (GetPCBType(plant) == PCBTYPE_FPC && CYCLETIME_WC_FPC.IndexOf(workCenter) > 0));
        }

        public string CostItem { get; set; }
        protected abstract string Formula { get; }
        protected object SourceData { get; set; }
        protected InputDataType InputDataType { get; set; }
        public Dictionary<string, object> Data { get; set; }
        protected Expression Exp { get; set; }
        protected virtual string[] RequiredInputKey { get { return null; } } 
        public bool DataReady = false;

        public CostBase(object data, InputDataType dataType)
        {
            this.SourceData = data;
            this.InputDataType = dataType;
            Initialize();
        }

        protected void Initialize()
        {
            Data = DataConverter.ConverterFactory.GetInstance(this.InputDataType).ReadData(this.SourceData);
            if (CheckInputData())
            {
                FillAdditionalData();
                this.Exp = Expression.GetExpression(Formula);
                //this.Exp = new Expression(Formula);
                CheckComputeData();
            }
        }

        protected virtual bool CheckInputData()
        {
            if (RequiredInputKey != null)
            {
                foreach (string key in RequiredInputKey)
                {
                    if (!Data.ContainsKey(key))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected virtual void CheckComputeData()
        {
            if (Data == null)
            {
                DataReady = false;
            }
            else
            {
                foreach (string key in Exp.ExpressionKey)
                {
                    if (!ExistsAndIsNumber(key))
                    {
                        DataReady = false;
                        return;
                    }
                }
                DataReady = true;
            }
        }

        protected virtual void FillAdditionalData()
        {

        }

        public virtual double Compute()
        {
            if (DataReady)
            {
                double val = Exp.Compute(Data);
                //Log(val); 
                return val;
            }
            return -1;
        }

        protected bool ExistsAndIsNumber(string key)
        {
            double rv = 0;
            return Data.ContainsKey(key) && double.TryParse(Convert.ToString(Data[key]), out rv);
        }

        protected void Log(double val)
        {
            string tempPath = System.Web.HttpContext.Current.Server.MapPath("~/temp/");
            using (StreamWriter sw = new StreamWriter(tempPath + DateTime.Now.ToString("yyyy_MM_dd") + "_Log.txt", true))
            {
                sw.WriteLine("-------------------------------------------------------");
                sw.WriteLine("{0} - {1}", DateTime.Now.ToLongDateString(), this.CostItem);
                sw.WriteLine("Formula:{0}", Formula);
                sw.WriteLine("Value:{0}", val);
                //sw.WriteLine("Expression:{0}", this.Exp.ExpressionBody);
                sw.Write("Input:", this.Exp.ExpressionBody);
                foreach (string key in this.Exp.ExpressionKey)
                {
                    if (this.Data.ContainsKey(key))
                    {
                        sw.Write("{0}:{1};", key, Data[key]);
                    }
                    else
                    {
                        sw.Write("{0}:null;", key);
                    }
                }
                sw.WriteLine("");
                sw.Close();
            }
            
        }
    }

    public class CostItemFactory
    {
        public static Dictionary<string, CostItem> CostItemMap
        {
            get
            {
                Dictionary<string, CostItem> dic = (Dictionary<string, CostItem>)HttpRuntime.Cache.Get("_CostItemMap");
                if (dic == null)
                {
                    dic = new Dictionary<string, CostItem>();
                    string strSql = "SELECT CostItem, DisplayName, TableKey, CostClass FROM SCS_CostItem WHERE Status = 1";
                    DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        dic.Add(Convert.ToString(dr["CostItem"]), new CostItem(dr));
                    }
                    HttpRuntime.Cache.Insert("_CostItemMap", dic, null, DateTime.Now.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                return dic;
            }
        }

        public static CostBase GetInstance(string costItem, object data, InputDataType dataType)
        {
            if (CostItemMap.ContainsKey(costItem))
            {
                Type type = Type.GetType(CostItemMap[costItem].CostClass);
                CostBase cb = (CostBase)Activator.CreateInstance(type, new object[] { data, dataType });
                if (cb != null)
                {
                    cb.CostItem = costItem;
                }
                return cb;
            }
            return null;
            //else
            //{
            //    throw new Exception(String.Format("Unknow Cost Item \"{0}\".", costItem));
            //}
        }
    }

}
