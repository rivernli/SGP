using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using SGP.DBUtility;
using System.IO;
using System.Collections.Specialized;
using BI.SGP.BLL.UIManager;

namespace BI.SGP.Web.Controllers
{
    public class CustomerPeopleController : Controller
    {
        //
        // GET: /CustomerPeople/
        public ActionResult CustomerPeopleList()
        {            
            return View();
        }

        public ActionResult CustomerPeopleEditor()
        {
            return View();
        }


        //Customer People Grid Data
        public string GetCusomerPeopleData()
        {
            string CustomerId=Request.QueryString["CustomerId"];
            string jsonData = CustomerPeopleManager.GetCustomerPeople(Request, CustomerId);
            return jsonData;
        }

        ///Get Customer People Detail
        public ActionResult GetCustomerPeopleDetail(string id)
        {
            CustomerPeopleManager CustomerEntity = null;
            string sSql = "SELECT * FROM SGP_CustomerPeople WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(sSql, new SqlParameter("@ID", id)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                CustomerEntity = ModelHandler<CustomerPeopleManager>.FillModel(dt.Rows[0]);
            }
            return Json(CustomerEntity);
        }
        

        //Save Customer People Data
        public ActionResult SaveData(CustomerPeopleManager curd )
        {
            //CustomerProfileManager curd
            string message = "";
            bool success = true;
            try
            {
                curd.Save();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            var jsonData = new
            {
                success = success,
                message = message,
                data = curd
            };

            return Json(jsonData);
        }

        //Delete Data
        public JsonResult DelData(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    CustomerPeopleManager.DeleteCustomerPeopleData(ID);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                errMessage = errMessage
            };
            return Json(result);
        }
    }
}
