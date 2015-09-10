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
    public class CustomerOrderInformationManager
    {
        public static string GetQuery(string Category, string site, string PartNo, string Item)
        {
            StringBuilder where = new StringBuilder();
            string tdata = string.Empty;
            bool b = false;

            tdata = Category;
            if (string.IsNullOrEmpty(tdata))
            {
                tdata = "Quantity";
            }
            if (b == true) where.Append("AND ");
            where.Append(" [Category] = '" + tdata + "'");
            b = true;

            tdata = site;
            if (!String.IsNullOrEmpty(tdata))
            {
                if (b == true) where.Append("AND ");
                where.Append(" [site] like '%" + tdata + "%'");
                b = true;
            }
            tdata = PartNo;
            if (!String.IsNullOrEmpty(tdata))
            {
                if (b == true) where.Append("AND ");
                where.Append(" [cPartNo] like '%" + tdata + "%'");
                b = true;
            }
            tdata = Item;
            if (!String.IsNullOrEmpty(tdata))
            {
                if (b == true) where.Append("AND ");
                where.Append(" [mitem] like '%" + tdata + "%'");
                b = true;
            }

            return where.ToString();
        }

        public static string GetCustomerOrder(HttpRequestBase Request)
        {
            string strSql = "SELECT * FROM V_SGP_Cusomer_Order_Information_Result ";
            String where = GetQuery(Request["myrad"], Request["site"], Request["PartNo"], Request["Item"]);
            if (!String.IsNullOrEmpty(where))
            {
                strSql += " WHERE 1=1 AND " + where;
            }
            GridData gridData = GridManager.GetGridData(Request, strSql);

            //Total
            strSql = @"SELECT 'Total:' as mitem,ISNULL(SUM(Hub90days),0) as Hub90days,ISNULL(SUM(Hub60days),0) as Hub60days,ISNULL(SUM(Hub30days),0) as Hub30days,ISNULL(SUM(Hub0days),0) as Hub0days,ISNULL(SUM(TotalAgent),0) as TotalAgent, 
                      ISNULL(SUM(BacklogCurrentP),0) as BacklogCurrentP,ISNULL(SUM(BacklogNextP),0) as BacklogNextP,ISNULL(SUM(BacklogThirdP),0) as BacklogThirdP,ISNULL(SUM(GHub),0) as GHub, 
                      ISNULL(SUM(SiteInventory),0) as SiteInventory, ISNULL(SUM(DemandCurrentP),0) as DemandCurrentP, ISNULL(SUM(DemandNextP),0) as DemandNextP, ISNULL(SUM(DemandThirthP),0) as DemandThirthP,
                      ISNULL(SUM(DemandFourthP),0) as DemandFourthP, ISNULL(SUM(DemandFifthP),0) as DemandFifthP, ISNULL(SUM(DemandSixthP),0) as DemandSixthP  FROM V_SGP_Cusomer_Order_Information_Result ";
            if (!String.IsNullOrEmpty(where))
            {
                strSql += " WHERE 1=1 AND " + where;
            }
            DataTable dt = SqlText.ExecuteDataset(strSql).Tables[0];

            List<TableFormatString> formatString = new List<TableFormatString>();
            if (Request["myrad"] == "Amount")
            {
                formatString.Add(new TableFormatString("Hub90days", "{0:C0}"));
                formatString.Add(new TableFormatString("Hub60days", "{0:C0}"));
                formatString.Add(new TableFormatString("Hub30days", "{0:C0}"));
                formatString.Add(new TableFormatString("Hub0days", "{0:C0}"));
                formatString.Add(new TableFormatString("TotalAgent", "{0:C0}"));
                formatString.Add(new TableFormatString("BacklogCurrentP", "{0:C0}"));
                formatString.Add(new TableFormatString("BacklogNextP", "{0:C0}"));
                formatString.Add(new TableFormatString("BacklogThirdP", "{0:C0}"));
                formatString.Add(new TableFormatString("GHub", "{0:C0}"));
                formatString.Add(new TableFormatString("SiteInventory", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandCurrentP", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandNextP", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandThirthP", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandFourthP", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandFifthP", "{0:C0}"));
                formatString.Add(new TableFormatString("DemandSixthP", "{0:C0}"));
            }

            return gridData.ToJsonTotal(dt,formatString.ToArray());
        }
 
        public static DataTable GetCustomerOrderData(string myrad, string site, string PartNo, string Item)
        {
            string strSql = "SELECT * FROM V_SGP_Cusomer_Order_Information_Result ";

            String where = GetQuery(myrad, site, PartNo, Item);
            if (!String.IsNullOrEmpty(where))
            {
                strSql += " WHERE 1=1 AND " + where;
            }
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];

            return dt;
        }
    }
}
