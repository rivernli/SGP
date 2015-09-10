using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.WF
{
    public class WFUser
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsKeyUser { get; set; }
        public bool IsApprover { get; set; }

        public WFUser() { }
        public WFUser(string uid)
        {
            string strSql = "SELECT UID,NAME,Email FROM Access_User WHERE UID = @UID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@UID", uid)
            }).Tables[0];

            if (dt.Rows.Count > 0)
            {
                UserID = Convert.ToString(dt.Rows[0]["UID"]);
                DisplayName = Convert.ToString(dt.Rows[0]["NAME"]);
                Email = Convert.ToString(dt.Rows[0]["Email"]);
            }
            else
            {
                throw new Exception(String.Format("can not find uid \"{0}\"", uid));
            }
        }
    }
}
