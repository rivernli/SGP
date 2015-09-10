using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BI.SGP.BLL.AccessServiceReference;
using System.Web.Mvc;

namespace BI.SGP.BLL.Utils
{
    /// <summary>
    /// 访问控制类
    /// </summary>
    public static class AccessControl
    {
        public static bool IsITAdmin(this User user)
        {
            if (user != null)
            {
                Role[] roles = user.Roles;
                if (roles != null)
                {
                    return (roles.Contains("ITAdmin"));
                }
            }
            return false;
        }

        public static bool IsAdmin(this User user)
        {
            if (user != null)
            {
                Role[] roles = user.Roles;
                if (roles != null)
                {
                    return (roles.Contains("ITAdmin") || roles.Contains("SGP_Management"));
                }
            }
            return false;
        }

        /// <summary>
        /// Check the login user is vendor or not
        /// </summary>
        /// <returns></returns>
        /// Lance Chen 20150210
        public static bool IsVendor()
        {
            foreach (BI.SGP.BLL.AccessServiceReference.Role role in CurrentLogonUser.Roles)
            {
                if (!string.IsNullOrWhiteSpace(role.Name) && role.Name.ToUpper() == "VVI_VENDOR")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(this BI.SGP.BLL.AccessServiceReference.Role[] roles, string roleName)
        {
            if (roles != null)
            {
                foreach (Role r in roles)
                {
                    if (r.Name.Trim() == roleName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 当前登录用户
        /// </summary>
        public static User CurrentLogonUser
        {
            get
            {
                User u = null;
                object userObj = HttpContext.Current.Session["_CurrentUser"];
                if (userObj != null && userObj is User)
                {
                    u = userObj as User;
                    return u;
                }
                string userId = HttpContext.Current.User.Identity.Name;
                if (string.IsNullOrEmpty(userId) == false && userId.IndexOf('\\') > 0) userId = userId.Split('\\')[1];

                if (String.IsNullOrEmpty(userId))
                {
                    userId = "mcnafeng";
                }
                u = GetUser(userId);
                HttpContext.Current.Session["_CurrentUser"] = u;
                return u;

            }
        }
        /// <summary>
        /// 获取用户
        /// </summary>
        public static User GetUser(string Uid)
        {
            User u = null;
            string AppID = System.Configuration.ConfigurationManager.AppSettings["AppID"];
            AccessClient client = new AccessClient();
            try
            {
                client.Open();
                u = client.GetCurrentUser(Convert.ToInt32(AppID), Uid);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Redirect("~/Home/Msg/AccessIssue");
            }
            finally
            {
                client.Close();
            }
            return u;
        }

        /// <summary>
        /// 日志
        /// </summary>
        public static bool LogAdd(string Action, string Description)
        {

            string AppID = System.Configuration.ConfigurationManager.AppSettings["AppID"];
            AccessClient client = new AccessClient();
            try
            {
                client.Open();
                return client.LogAdd(Convert.ToInt32(AppID), CurrentLogonUser.Uid, System.Web.HttpContext.Current.Request.Url.ToString(), Action, Description);
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        public static bool IsCustomerAdmin(this User user)
        {
            if (user != null)
            {
                Role[] roles = user.Roles;
                if (roles != null)
                {
                    return (roles.Contains("CustomerAdmin"));
                }
            }
            return false;
        }

    }

        /// <summary>
        /// 操作权限 
        /// 编辑，删除,
        /// </summary>
        //public static string AuthorityFilter()
        //{
        //    foreach (BI.SGP.BLL.AccessServiceReference.Role role in CurrentLogonUser.Roles)
        //    {
        //        if (!string.IsNullOrWhiteSpace(role.Name) && role.Name.ToUpper() == "VVI_VENDOR")
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}


    /// <summary>
    /// 用户基础类
    /// </summary>
    public class UserInfo
    {
        public string UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get { return _roles; } }

        private List<string> _roles = new List<string>();
    }
    public class MyAuthorize:AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (AccessControl.CurrentLogonUser.Roles.Contains("VVI_VENDOR"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }


    public class AuthorityFilter : ActionFilterAttribute
    {
        public string RolesCode { set; get; }//要验证的权限的代码 

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpResponseBase response = filterContext.HttpContext.Response;
            if (AuthorityCheck(RolesCode))
            {
                //没权限访问
                filterContext.Result = new ContentResult { Content = @"You don't have permission to access." };
            }
            base.OnActionExecuting(filterContext);
        }

        public bool AuthorityCheck(string RolesCode)
        {
            if (AccessControl.CurrentLogonUser.Roles.Contains(RolesCode))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    } 
}
