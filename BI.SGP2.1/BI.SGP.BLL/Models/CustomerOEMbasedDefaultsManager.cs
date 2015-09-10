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
    public class CustomerOEMbasedDefaultsManager
    {
        /// <summary>
        /// 字段的特性
        /// </summary>
        public int? ID { get; set; }
        /// <summary>
        /// OEM Name
        /// </summary>
        public string Customer_OEM { get; set; }
        /// <summary>
        /// ProgramName
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// ProductApplication
        /// </summary>
        public string ProductApplication { get; set; }
        /// <summary>
        /// RFQInitiator
        /// </summary>
        public string RFQInitiator { get; set; }
        /// <summary>
        /// TechCosting
        /// </summary>
        public string TechCosting { get; set; }
        /// <summary>
        /// PricingAnalyst
        /// </summary>
        public string PricingAnalyst { get; set; }
        /// <summary>
        /// RFQPrimaryContact
        /// </summary>
        public string RFQPrimaryContact { get; set; }
        /// <summary>
        /// GAMBDM
        /// </summary>
        public string GAMBDM { get; set; }
        /// <summary>
        /// CustomerContact
        /// </summary>
        public string CustomerContact { get; set; }
        /// <summary>
        /// TermsConditions
        /// </summary>
        public string TermsConditions { get; set; }
        /// <summary>
        /// MarketSegment
        /// </summary>
        public string MarketSegment { get; set; }


        
        public static string GetCustomerBasedDefaults(HttpRequestBase Request)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string strSql = "SELECT * FROM SGP_CustomerOEMBasedDefaults WHERE 1=1 ";
            if (!String.IsNullOrEmpty(Request["Customer_OEM"]))
            {
                strSql += " AND Customer_OEM LIKE @Customer_OEM";
                ps.Add(new SqlParameter("@Customer_OEM", "%" + Request["Customer_OEM"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["ProgramName"]))
            {
                strSql += " AND ProgramName LIKE @ProgramName";
                ps.Add(new SqlParameter("@ProgramName", "%" + Request["ProgramName"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["ProductApplication"]))
            {
                strSql += " AND ProductApplication LIKE @ProductApplication";
                ps.Add(new SqlParameter("@ProductApplication", "%" + Request["ProductApplication"] + "%"));
            }
            GridData gridData = GridManager.GetGridData(Request, strSql, ps.ToArray());
            return gridData.ToJson();
        }


        //删除
        public static void DeleteCustomerBasedDefaults(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerOEMBasedDefaults WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        //保存
        public void Save()
        {
            List<SqlParameter> ps = new List<SqlParameter>()
            {
                new SqlParameter("@Customer_OEM", ConvertValue(this.Customer_OEM)),
                new SqlParameter("@ProgramName", ConvertValue(this.ProgramName)),
                new SqlParameter("@ProductApplication", ConvertValue(this.ProductApplication)),
                new SqlParameter("@RFQInitiator", ConvertValue(this.RFQInitiator)),
                new SqlParameter("@TechCosting", ConvertValue(this.TechCosting)),
                new SqlParameter("@PricingAnalyst", ConvertValue(this.PricingAnalyst)),
                new SqlParameter("@RFQPrimaryContact", ConvertValue(this.RFQPrimaryContact)),
                new SqlParameter("@GAMBDM", ConvertValue(this.GAMBDM)),
                new SqlParameter("@CustomerContact", ConvertValue(this.CustomerContact)),
                new SqlParameter("@TermsConditions", ConvertValue(this.TermsConditions)),
                new SqlParameter("@MarketSegment", ConvertValue(this.MarketSegment)) 
            };

            string strSql = string.Empty;
            if (ID > 0)
            {
                strSql = @"UPDATE SGP_CustomerOEMBasedDefaults SET [Customer_OEM]=@Customer_OEM,[ProgramName]=@ProgramName,[ProductApplication]=@ProductApplication,[RFQInitiator]=@RFQInitiator,
                            [TechCosting]=@TechCosting,[PricingAnalyst]=@PricingAnalyst,[RFQPrimaryContact]=@RFQPrimaryContact,[GAMBDM]=@GAMBDM,[CustomerContact]=@CustomerContact,
                            [TermsConditions]=@TermsConditions,[MarketSegment]=@MarketSegment  WHERE ID = '" + ID + "'";
                DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());

            }
            else
            {
                strSql = @"INSERT INTO SGP_CustomerOEMBasedDefaults([Customer_OEM], [ProgramName], [ProductApplication], [RFQInitiator], [TechCosting], [PricingAnalyst], [RFQPrimaryContact], [GAMBDM]
                          , [CustomerContact], [TermsConditions], [MarketSegment]) 
                        VALUES(@Customer_OEM, @ProgramName, @ProductApplication, @RFQInitiator, @TechCosting, @PricingAnalyst, @RFQPrimaryContact, @GAMBDM
                          , @CustomerContact, @TermsConditions, @MarketSegment);SELECT @@IDENTITY";
                this.ID = DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
            }
        }

        private object ConvertValue(object strValue)
        {
            if (strValue != null || Convert.ToString(strValue).Trim() != String.Empty)
            {
                return strValue;
            }
            return DBNull.Value;
        }
    }
}
