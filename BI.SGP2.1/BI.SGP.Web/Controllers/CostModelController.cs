using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.WF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BI.SGP.Web.Controllers
{
    public class CostModelController : Controller
    {
        //
        // GET: /CostModel/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SimpleMI(string SCIID)
        {
            SCIID = "1";
            int dataId = ParseHelper.Parse<int>(SCIID);


            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_SCSI);
       //     List<FieldCategory> allfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SCSI);
            //string[] categorytypes = { "SCSI","RSCSI"};

            //List<FieldCategory> rigidtype = FieldCategory.GetCategorys(categorytypes);
            

            if (dataId > 0)
            {
                CostModellingDetail costmodelling = new CostModellingDetail();
                costmodelling.FillCategoryData(lfc, dataId);
            }

            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            return View();
        }
        public ActionResult CostModelOutPut()
        {
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_SCO);
            ViewBag.Categories = lfc;
            return View();
        }

        public ActionResult Save()
        {
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_SCO);
            ViewBag.Categories = lfc;
            return View("CostModelOutPut");
        }
        public ActionResult BackToChange(string SCIID)
        {
            SCIID = "1";
            int dataId = ParseHelper.Parse<int>(SCIID);


            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_SCSI);
            if (dataId > 0)
            {
                CostModellingDetail costmodelling = new CostModellingDetail();
                costmodelling.FillCategoryData(lfc, dataId);
            }

            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;

            return View("SimpleMI");
        }

        public ActionResult CostModelInput(string postData)
        {

            postData = Request["postData"];
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

             
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            }
            var jsonObject = new
            {
                Keys= Request.Form.AllKeys,
           
            };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }
       
    }
}
