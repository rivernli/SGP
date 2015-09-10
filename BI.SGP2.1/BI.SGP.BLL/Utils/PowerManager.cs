using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.Utils
{
    public static class PowerManager
    {
        private static IDictionary _powers;

        /// <summary>
        /// 查看权限
        /// </summary>
        public static bool HasView
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(_powers["View"]);
                }
                catch
                { }
                return false;
            }
        }

        /// <summary>
        /// 编辑编辑
        /// </summary>
        public static bool HasEdit
        {
            get
            {               
                try
                {
                    return Convert.ToBoolean(_powers["Edit"]);
                }
                catch
                { }
                return false;
            }
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        public static bool HasDel
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(_powers["Del"]);
                }
                catch
                { }
                return false;
            }
        }


        public static void LoadPower(string PageID)
        {
            _powers = new HybridDictionary();

            bool view, edit, del;
            string temp = String.Empty;
            view = edit = del = false;

            string AppID = System.Configuration.ConfigurationManager.AppSettings["AppID"];
            string sqlstr = "select CanSee,CanAdd,CanModify,CanDelete from Access_UserPermission_Page where AppID='" + AppID + "' AND Uid='" + BI.SGP.BLL.Utils.AccessControl.CurrentLogonUser.Uid + "' AND PageID='" + PageID + "' ";

            DataSet ds = DbHelperSQL.Query(sqlstr);
            DataTable dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToBoolean(dr["CanAdd"]) || Convert.ToBoolean(dr["CanModify"]))
                {
                    edit = true;
                }
                if (Convert.ToBoolean(dr["CanSee"]))
                {
                    view = true;
                }
                if (Convert.ToBoolean(dr["CanDelete"]))
                {
                    del = true;
                }
            }

            _powers.Add("View", view);
            _powers.Add("Edit", edit);
            _powers.Add("Del", del);
        }
    }
}
