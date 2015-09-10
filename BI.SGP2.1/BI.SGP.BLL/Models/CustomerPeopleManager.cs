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
    public class CustomerPeopleManager
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
        /// 客户人名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int? Age { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 爱好
        /// </summary>
        public string Hobbies { get; set; }
        /// <summary>
        /// 其它
        /// </summary>
        public string Other { get; set; }


        //
        public static string GetCustomerPeople(HttpRequestBase Request, string CustomerId)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string strSql = "SELECT * FROM SGP_CustomerPeople WHERE CustomerId='" + CustomerId + "' ";
            if (!String.IsNullOrEmpty(Request["txName"]))
            {
                strSql += " AND Name LIKE @Name";
                ps.Add(new SqlParameter("@Name", "%" + Request["txName"] + "%"));
            }
            if (!String.IsNullOrEmpty(Request["txTitle"]))
            {
                strSql += " AND Title LIKE @Title";
                ps.Add(new SqlParameter("@Title", "%" + Request["txTitle"] + "%"));
            }
            GridData gridData = GridManager.GetGridData(Request, strSql, ps.ToArray());
            return gridData.ToJson();
        }

        //删除
        public static void DeleteCustomerPeopleData(string id)
        {
            string strSql = "DELETE FROM SGP_CustomerPeople WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", id));
        }

        //保存
        public void Save()
        {
            List<SqlParameter> ps = new List<SqlParameter>()
            {
                new SqlParameter("@CustomerId", this.CustomerId),
                new SqlParameter("@Name", ConvertValue(this.Name)),
                new SqlParameter("@Gender", ConvertValue(this.Gender)),
                new SqlParameter("@Age", ConvertValue(this.Age)),
                new SqlParameter("@Title", ConvertValue(this.Title)),
                new SqlParameter("@Country", ConvertValue(this.Country)),
                new SqlParameter("@City", ConvertValue(this.City)),
                new SqlParameter("@Language", ConvertValue(this.Language)),
                new SqlParameter("@Phone", ConvertValue(this.Phone)),
                new SqlParameter("@Mobile", ConvertValue(this.Mobile)),
                new SqlParameter("@Hobbies", ConvertValue(this.Hobbies)),
                new SqlParameter("@Other", ConvertValue(this.Other))
            };

            string strSql = string.Empty;
            if (ID > 0)
            {
                strSql = @"UPDATE SGP_CustomerPeople SET [CustomerId]=@CustomerId,[Name]=@Name,[Gender]=@Gender,[Age]=@Age,[Title]=@Title,
                            [Country]=@Country,[City]=@City,[Language]=@Language,[Phone]=@Phone,[Mobile]=@Mobile,
                            [Hobbies]=@Hobbies,[Other]=@Other  WHERE ID = '" + ID + "'";
                DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());

            }
            else
            {
                strSql = @"INSERT INTO SGP_CustomerPeople([CustomerId],[Name],[Gender],[Age],[Title],[Country],[City],[Language]
           ,[Phone],[Mobile],[Hobbies],[Other]) 
            VALUES(@CustomerId,@Name,@Gender,@Age,@Title,@Country,@City,@Language,
                   @Phone,@Mobile,@Hobbies,@Other);SELECT @@IDENTITY";
                this.ID = DbHelperSQL.GetSingle<int>(strSql, ps.ToArray());
            }
        }


        private string NullToEmpty(string s)
        {
            return s == null ? "" : s.Trim();
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
