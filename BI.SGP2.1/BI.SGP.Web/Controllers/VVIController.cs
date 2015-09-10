using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;
using System.Collections.Specialized;
using System.Text;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;

namespace BI.SGP.Web.Controllers
{
    public class VVIController : Controller
    {
        //
        // GET: /VVI/
        /// <summary>
        /// Supplier DataGrid
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult SupplierView()
        {
            ViewBag.Title = "Supplier";
            ViewBag.GridGroup = "SupplierGrid";
            ViewBag.SearchGroup = "SupplierSearch";
            return View();

        }
        /// <summary>
        /// Supplier Edit Page
        /// </summary>
        /// <param name="rf"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult SupplierEdit(SGP.BLL.Models.SupplierManager rf)
        {
            if (Request.QueryString["ID"] != null)
            {
                rf = SupplierManager.GetDetail(Request.QueryString["ID"])[0];
            }
            return View(rf);
        }
        /// <summary>
        /// Delete Supplier
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public JsonResult SupplierDel(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    SupplierManager.DeleteSupplierData(ID);
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
        /// <summary>
        /// Update Supplier 
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveSupplierData()
        {
            //SupplierManager curd
            string supplierId = string.Empty;
            string message = "";
            bool success = true;
            try
            {
                //curd.Save();
                NameValueCollection Result = SupplierManager.Save(Request);
                supplierId = Result["ID"];
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
                DataId = supplierId
            };

            return Json(jsonData);
        }
        /// <summary>
        /// Get Supplier name list
        /// </summary>
        /// <returns></returns>
        public ActionResult SupplierName()
        {
            DataTable dt = DbHelperSQL.Query("select * from sys_supplier").Tables[0];
            string jsonData = "{\"Suppliers\":[";

            foreach (DataRow dr in dt.Rows)
            {
                jsonData += "{\"id\":" + dr[0] + ",\"SupplyCode\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr[3]) + ",\"name\":" + Newtonsoft.Json.JsonConvert.SerializeObject(dr[5]) + "},";
            }
            jsonData = jsonData.TrimEnd(',');

            jsonData += "]";
            jsonData += "}";

            return Content(jsonData);
        }
        /// <summary>
        /// RFQ VVI DataGrid
        /// </summary>
        /// <returns></returns>

        public ActionResult SupplierPricingView()
        {
            ViewBag.Title = "Supplier Pricing View";
            ViewBag.GridGroup = "SupplierRFQGrid";
            ViewBag.SearchGroup = "SupplierRFQSearch";
            return View();
        }
        //public ActionResult SupplierPricingDetail(SGP.BLL.Models.VVIRFQManager vvrfq)
        //{
        //    int id = 0;
        //    if (Request.QueryString["ID"] != null)
        //    {
        //        int.TryParse(Request.QueryString["ID"], out id);
        //        vvrfq = VVIRFQManager.GetDetail(id)[0];
        //    }
        //    return View(vvrfq);
        //}
        public ActionResult SupplierPricingDetail(string ID, string number)
        {
            int dataId = ParseHelper.Parse<int>(ID);

            WFTemplate template = new WFTemplate(4, dataId, number);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_SGPFORSUPPLIER);

            if (dataId > 0)
            {
                SupplierRFQDetail vd = new SupplierRFQDetail(lfc, dataId, number);
                //vd.FillCategoryData(lfc, dataId, number);
                //vd.InitAutoCalculateColumns(lfc);
            }

            ViewBag.WFTemplate = template;
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            ViewBag.Number = number;
            ViewBag.wfStatus = template.Status == WorkflowStatus.Finished ? "Finished" : "";
            return View("SupplierPricingDetail");
        }

        /// <summary>
        /// Supplier RFQ workflow
        /// </summary>
        /// <returns></returns>
        public ActionResult SUPPLIERRFQWFSkip()
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();

            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            string number = Convert.ToString(jsonData["number"]);
            int id = 0;
            WFTemplate wfTemplate;
            string wfStatus = string.Empty;

            using (TScope ts = new TScope())
            {
                bool isOutOfCapability;
                id = SupplierRfqSave(postData, sysMsg, out isOutOfCapability);
                wfTemplate = new WFTemplate("SUPPLIERWF", id, number);
                if (!isOutOfCapability)
                {
                    wfTemplate.FirstActivity.CheckData(sysMsg);
                }

                SkipRFQWFToNextForVVI(id, sysMsg);

                if (!sysMsg.isPass)
                {
                    ts.Rollback();
                }
                else
                {
                    //Send Email To VVI Team
                    wfTemplate.LastActivity.DoAction();
                }
            }

            wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }

        /// <summary>
        /// Skip the workflow of RFQ for VVI to next activity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sysMsg"></param>
        /// Lance Chen 20150128
        private void SkipRFQWFToNextForVVI(int id, SystemMessages sysMsg)
        {
            WFTemplate vviTemplate = new WFTemplate(3, id);
            if (vviTemplate.CurrentActivity.ID == 102)
            {
                sysMsg.Merge(vviTemplate.Skip(vviTemplate.NextActivity.ID, 0, false, false));
            }
        }

        private int SUPPLIERRFQSave(string postData, SystemMessages sysMsg)
        {
            int id = 0;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string operation = Convert.ToString(jsonData["operation"]);
                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_VVI);

                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                    }
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            SupplierRFQDetail dm = new SupplierRFQDetail(lfc, data);
                            if (operation == SupplierRFQDetail.OPERATION_REQUOTE)
                            {
                                id = dm.ReQuote();
                            }
                            else if (operation == SupplierRFQDetail.OPERATION_CLONE)
                            {
                                id = dm.Clone();
                            }
                            else
                            {
                                if (id > 0)
                                {
                                    dm.Update(id);
                                }
                                else
                                {
                                    id = dm.Add();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ts.Rollback();
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add("Error", ex.Message);
                        }
                    }
                }
            }

            return id;
        }

        public ActionResult SUPPLIERRFQSaveData()
        {
            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            string number = Convert.ToString(jsonData["number"]);
            bool isOutOfCapability;
            id = SupplierRfqSave(postData, sysMsg, out isOutOfCapability);
            WFTemplate wfTemplate = new WFTemplate("SUPPLIERWF", id, number);

            string wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }

        /// <summary>
        /// Save data for supplier RFQ
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="sysMsg"></param>
        /// <returns></returns>
        /// Lance Chen 20150126
        private int SupplierRfqSave(string postData, SystemMessages sysMsg, out bool isOutOfCapability)
        {
            isOutOfCapability = false;
            int id = 0;
            if (!string.IsNullOrEmpty(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string number = Convert.ToString(jsonData["number"]);
                string operation = Convert.ToString(jsonData["operation"]);
                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_SGPFORSUPPLIER);

                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                    }
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            SupplierRFQDetail dm = new SupplierRFQDetail(lfc, data);

                            if (id > 0)
                            {
                                dm.UpdateSubDataForSupplierRfq(id, number, out isOutOfCapability);
                                if (operation == "Submit")
                                {
                                    dm.UpdateSupplierRfqStatus(id, number);
                                    dm.AddSupplierRFQHistory(id, number, "Submit", sysMsg);
                                }
                                else
                                {
                                    dm.AddSupplierRFQHistory(id, number, "Save", sysMsg);
                                }
                            }
                            else
                            {
                                sysMsg.isPass = false;
                                sysMsg.Messages.Add("Error", "Please select a RFQ ");
                            }
                        }
                        catch (Exception ex)
                        {
                            ts.Rollback();
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add("Error", ex.Message);
                        }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// RFQ VVI Detail Page
        /// </summary>
        /// <param name="vvrfq"></param>
        /// <returns></returns>
        ///
        [MyAuthorize]
        public ActionResult VVIPricingView()
        {
            ViewBag.ExcelView = "~/Views/Shared/_ExportExcel.cshtml";
            ViewBag.GridGroup = "SGPForVVIGrid";
            ViewBag.SearchGroup = "SGPForVVISearch";
            ViewBag.UserID = AccessControl.CurrentLogonUser.Uid;
            return View();


        }



        [MyAuthorize]
        public ActionResult VVIPricingDetail(string RFQID)
        {
            int dataId = ParseHelper.Parse<int>(RFQID);

            WFTemplate template = new WFTemplate(3, dataId);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_VVI);

            if (dataId > 0)
            {
                VVIQuotationDetail vviDetail = new VVIQuotationDetail();
                vviDetail.FillCategoryData(lfc, dataId);
            }

            ViewBag.WFTemplate = template;
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            return View();
        }






        private int VVISave(string postData, SystemMessages sysMsg)
        {
            int id = 0;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string operation = Convert.ToString(jsonData["operation"]);
                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_VVI);

                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                    }
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {

                            VVIQuotationDetail dm = new VVIQuotationDetail(lfc, data);


                            if (id > 0)
                            {

                                dm.UpdateForVVI(id);
                            }
                            else
                            {
                                sysMsg.isPass = false;
                                sysMsg.Messages.Add("Error", "Please select a RFQ ");
                            }

                        }
                        catch (Exception ex)
                        {
                            ts.Rollback();
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add("Error", ex.Message);
                        }
                    }
                }
            }

            return id;
        }
        public ActionResult VVISaveData()
        {
            SystemMessages sysMsg = new SystemMessages();

            string html = "";
            string wfStatus = "";
            int id = 0;
            string postData = Request["postData"];
            id = VVISave(postData, sysMsg);



            if (id > 0 && sysMsg.isPass)
            {
                WFTemplate wfTemplate = new WFTemplate(3, id);
                VVIQuotationDetail vvidetail = new VVIQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_VVI);
                vvidetail.FillCategoryData(lfc, id);
                html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
                wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            }
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }

        public ActionResult VVIRedo()
        {
            SystemMessages sysMsg = new SystemMessages();
            string postData = Request["postData"];

            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
                string dataId = Convert.ToString(jsonData["dataId"]);
                string VendorRFQNumber = Convert.ToString(jsonData["redoid"]);
                try
                {
                    VVIQuotationDetail dm = new VVIQuotationDetail();
                    if (dm.CheckVendorRFQStatus(VendorRFQNumber, Int32.Parse(dataId)) == true)
                    {
                        dm.RedoRFQ(VendorRFQNumber, Int32.Parse(dataId));
                       // WFTemplate wf = new WFTemplate(4, Int32.Parse(dataId), VendorRFQNumber);
                       // wf.Run();
                    }
                    else
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add("Error", "don't need change the status.");
                    }
                }
                catch (Exception ex)
                {
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }

            }
            var jsonResult = new
            {
                SysMsg = sysMsg,
            };
            return Json(jsonResult);
        }
        public ActionResult VVIReturn()
        {

            string postData = Request["postData"];
            SystemMessages sysMsg = new SystemMessages();
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string VendorRFQNumber = Convert.ToString(jsonData["returnid"]);

                try
                {
                    VVIQuotationDetail dm = new VVIQuotationDetail();
                    if (dm.CheckVendorRFQStatus(VendorRFQNumber, Int32.Parse(dataId)) == true)
                    {
                        dm.ReturnRFQ(VendorRFQNumber, Int32.Parse(dataId));
                       // WFTemplate wf = new WFTemplate(4, Int32.Parse(dataId), VendorRFQNumber);
                       // wf.Run();
                    }
                    else
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add("Error", "don't need change the status.");
                    }
                }
                catch (Exception ex)
                {
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }

            }

            var jsonResult = new
            {
                SysMsg = sysMsg,
            };
            return Json(jsonResult);

        }


        public ActionResult VVIWFSkip()
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();
            string html = "";
            string wfStatus = "";
            string PDFDIV = "";
            string postData = Request["postData"];
            int id = VVISave(postData, sysMsg);

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            Dictionary<string, object> productiondata = data["24"] as Dictionary<string, object>;


            if (sysMsg.isPass && id > 0)
            {
                List<int> lstToChildIds = new List<int>();
                string templateName = Request["templateName"];
                string toChildIds = Request["toChildIds"];
                int entityId = id;
                int fromId = ParseHelper.Parse<int>(Request["fromId"]);
                int toId = ParseHelper.Parse<int>(Request["toId"]);
                bool checkData = !(Request["checkData"] == "false");
                bool waitAllChildComplated = !(Request["waitAllChildComplated"] == "false");

                if (!String.IsNullOrWhiteSpace(toChildIds))
                {
                    foreach (string stcid in toChildIds.Split(','))
                    {
                        int tcid = ParseHelper.Parse<int>(stcid);
                        if (tcid > 0)
                        {
                            lstToChildIds.Add(Convert.ToInt32(tcid));
                        }
                    }
                }

                WFTemplate wfTemplate = new WFTemplate(templateName, entityId);
                using (TScope ts = new TScope())
                {
                    if (toId == 0)
                    {
                        sysMsg.Merge(wfTemplate.Run(fromId, checkData));
                    }
                    else
                    {
                        sysMsg.Merge(wfTemplate.Skip(toId, fromId, checkData, waitAllChildComplated, lstToChildIds.ToArray()));
                    }

                    if (productiondata.ContainsKey("vendorrfqid") && (toId == 104 || wfTemplate.NextActivity.ID == 104))
                    {

                        if (string.IsNullOrEmpty(productiondata["vendorrfqid"].ToString()) == false)
                        {

                            try
                            {

                                VVIQuotationDetail dm = new VVIQuotationDetail();
                                if (dm.CheckMainRFQStatusByID(id))
                                {
                                    DbHelperSQL.ExecuteSql("exec SP_VVIPostBackSGP @EntityID,@VendorRFQNumber ", new SqlParameter("@EntityID", entityId), new SqlParameter("@VendorRFQNumber", productiondata["vendorrfqid"].ToString()));
                                    sysMsg.isPass = true;
                                }
                                else
                                {
                                    sysMsg.isPass = false;
                                    sysMsg.Messages.Add("Error", "Main RFQ status is not TechnicalCosting stage.");
                                }
                            }
                            catch (Exception ex)
                            {
                                sysMsg.isPass = false;
                                sysMsg.Messages.Add("error", ex.Message);
                            }

                        }
                    }


                    if (!sysMsg.isPass)
                    {
                        ts.Rollback();
                    }
                }

                VVIQuotationDetail b2Detail = new VVIQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_VVI);
                b2Detail.FillCategoryData(lfc, id);
                html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
                wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
            }

            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                PDF = PDFDIV,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }

        /// <summary>
        /// Delete RFQ VVI
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public JsonResult VVIPricingDel(string ID)
        {
            string errMessage = "";
            int dataId = 0;
            if (int.TryParse(ID, out dataId))
            {
                try
                {
                    VVIRFQManager.DeleteVVIRFQData(ID);
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


        /// <summary>
        /// Update RFQ VVI  Value
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveVVIDetailData()
        {

            VVIRFQManager vvirfqdetail = new VVIRFQManager();
            SystemMessages sysmgs = new SystemMessages();

            VVIRFQManager.SavaAndCheckData(ref vvirfqdetail, ref sysmgs, Request);
            //WFTemplate wf = new WFTemplate("VVIWF",vvirfqdetail.ID.Value);
            //if(wf.CurrentActivity.ID==104)
            //{
            //    VVIRFQManager.UpdateOperationForPostBack(vvirfqdetail.ID.Value.ToString());

            //}

            string html = "";
            if (sysmgs.isPass == true)
            {

                html = SGP.BLL.UIManager.VVIUIManager.GenrateVVIRFQDetail(vvirfqdetail, vvirfqdetail.ID.ToString(), "Edit");
            }
            // string aa = Request.Form["Number"];
            var returnData = new
            {
                HTML = html,
                ID = vvirfqdetail.ID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// RFQ VVI skip stage
        /// </summary>
        /// <returns></returns>
        public ActionResult RFQVVISaveAndSkip()
        {

            VVIRFQManager vvirfdetail = new VVIRFQManager();
            SystemMessages sysmgs = new SystemMessages();
            VVIRFQManager.SavaAndCheckData(ref vvirfdetail, ref sysmgs, Request);
            WFTemplate wfTemplate = new WFTemplate("VVIWF", vvirfdetail.ID);
            if (sysmgs.isPass)
            {
                string toActivityId = Request.Form["toActivityId"];
                int toActId = 0;
                int.TryParse(toActivityId, out toActId);

                if (toActId > 0)
                {
                    try
                    {

                        sysmgs.Merge(wfTemplate.Skip(toActId));
                    }
                    catch (Exception ex)
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add("System Exception", ex.Message);
                    }
                }
            }
            string[] WFIDS = { "3", "4", "5", "6", "7" };
            string PDFDIV = "";
            if (WFIDS.Contains(wfTemplate.CurrentActivity.ID.ToString()))
            {
                PDFDIV = @"<button id=""btnDownlPDF"" class=""btn btn-purple"" onclick=""return downloadpdf();"" >
                                                                 Download PDF
                                                                <i class=""icon-file small-30""></i>
                                                                 </button>";
            }

            var returnData = new
            {
                HTML = SGP.BLL.UIManager.UIManager.GenrateModelforRFQVVIDetail(vvirfdetail, wfTemplate.CurrentActivity.ID.ToString()),
                RFQNumber = vvirfdetail.Number,
                RFQID = vvirfdetail.RFQID,
                ID = vvirfdetail.ID,
                SysMsg = sysmgs,
                PDF = PDFDIV
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);


        }
        /// <summary>
        /// Update  RFQ VVI Value  & Submit RFQ VVI to next stage
        /// </summary>
        /// <returns></returns>

        public ActionResult RFQVVISaveAndSubmit()
        {
            VVIRFQManager vvirfdetail = new VVIRFQManager();
            SystemMessages sysmgs = new SystemMessages();

            VVIRFQManager.SavaAndCheckData(ref vvirfdetail, ref sysmgs, Request);

            WFTemplate wfTemplate = new WFTemplate("VVIWF", vvirfdetail.ID.Value);


            if (sysmgs.isPass)
            {
                try
                {
                    sysmgs.Merge(wfTemplate.Run());
                }
                catch (Exception ex)
                {
                    sysmgs.isPass = false;
                    sysmgs.Messages.Add("System Exception", ex.Message);
                }
            }
            string[] WFIDS = { "104" };
            string PDFDIV = "";
            if (WFIDS.Contains(wfTemplate.CurrentActivity.ID.ToString()))
            {
                if (wfTemplate.CurrentActivity.ID == 104)
                {
                    VVIRFQManager.UpdateOperationForPostBack(vvirfdetail.ID.Value.ToString());

                }
                PDFDIV = @"<button id=""btnDownlPDF"" class=""btn btn-purple""   onclick=""return downloadpdf();"" >
                                                                 Download PDF
                                                                <i class=""icon-file small-30""></i>
                                                                 </button>";
            }

            var returnData = new
            {
                HTML = SGP.BLL.UIManager.UIManager.GenrateModelforRFQVVIDetail(vvirfdetail, wfTemplate.CurrentActivity.ID.ToString()),
                RFQNumber = vvirfdetail.Number,
                RFQID = vvirfdetail.RFQID,
                ID = vvirfdetail.ID,
                SysMsg = sysmgs,
                PDF = PDFDIV
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// From SGP RFQ Post to RFQ VVI
        /// </summary>
        /// <returns></returns>
        public ActionResult PostRFQToVVI()
        {
            string RFQID = "0";
            if (Request.Form["RFQID"] != null)
            {
                RFQID = Request.Form["RFQID"];
            }
            SystemMessages sysmgs = new SystemMessages();
            if (RFQID == "0")
            {
                sysmgs.isPass = false;
                sysmgs.MessageType = "Post Error";
                sysmgs.Messages.Add("Post Error", "RFQ don't allow null ");
            }
            else
            {
                if (VVIRFQManager.IsPost(RFQID) <= 0)
                {

                    StringBuilder sb = new StringBuilder();
                    string sql = @"Insert into [SGP_RFQForVVI](RFQID,Number,VVINumber,[OEM],GAMBDM,CustomerPartNumber,Revision,MarketSegment,[Application],MassProductionEAU,MassProductionDate,StatusID,ActivityID,TemplateID)
                           Select RFQID,Number,Number,[OEM],GAMBDM,CustomerPartNumber,Revision,MarketSegment,[Application],VolumePerMonth,MassProductionDate,1,101,1 from V_SGP where RFQID=" + RFQID + "";
                    DbHelperSQL.ExecuteSql(sql);

                }
                else
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "Post Error";
                    sysmgs.Messages.Add("Post Error", "already exists ");

                }
            }
            var returnData = new
            {
                SysMsg = sysmgs
            };
            return Json(returnData, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// Assign RFQ VVI to Supplier
        /// </summary>
        /// <returns></returns>
        /// 
        public ActionResult AssignSupplier()
        {
            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            string dataId = Convert.ToString(jsonData["dataId"]);
            string operation = Convert.ToString(jsonData["operation"]);
            string suppliercode = Convert.ToString(jsonData["SupplierCode"]);
            List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_VVI);
            Int32.TryParse(dataId, out id);

            using (TScope ts = new TScope())
            {
                try
                {

                    VVIQuotationDetail dm = new VVIQuotationDetail(lfc, data);


                    if (id > 0)
                    {

                        dm.AssignVVIData(id, suppliercode, sysMsg);
                    }
                    else
                    {
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add("Error", "Please select a RFQ ");
                    }

                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    sysMsg.isPass = false;
                    sysMsg.Messages.Add("Error", ex.Message);
                }
            }
            string html = "";
            string wfStatus = "";

            VVIQuotationDetail b2Detail = new VVIQuotationDetail();
            // List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_VVI);
            b2Detail.FillCategoryData(lfc, id);
            WFTemplate wfTemplate = new WFTemplate("VVIWF", id);
            html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
            wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";

            var returnData = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                wfStatus = wfStatus
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult VVIMainRFQStatus()
        {
            SystemMessages sysMsg = new SystemMessages();

            int id = 0;
            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            string dataId = Convert.ToString(jsonData["dataId"]);
            string operation = Convert.ToString(jsonData["operation"]);
            Int32.TryParse(dataId, out id);

            try
            {
                VVIQuotationDetail dm = new VVIQuotationDetail();

                sysMsg.isPass = dm.CheckMainRFQStatusByID(id);

            }
            catch (Exception ex)
            {
                sysMsg.isPass = false;
                sysMsg.Messages.Add("Error", ex.Message);
            }

            var returnData = new
            {
                SysMsg = sysMsg
            };
            return Json(returnData, JsonRequestBehavior.AllowGet);

        }
        public ActionResult VendorRFQStatus()
        {

            SystemMessages sysMsg = new SystemMessages();
            int id = 0;
            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            string dataId = Convert.ToString(jsonData["dataId"]);
            string operation = Convert.ToString(jsonData["operation"]);
            Int32.TryParse(dataId, out id);
            string VendorRFQNumber = "";
            VVIQuotationDetail dm = new VVIQuotationDetail();
            try
            {
                if (jsonData.ContainsKey("returnid"))
                {
                    VendorRFQNumber = Convert.ToString(jsonData["returnid"]);
                    sysMsg.isPass = dm.CheckVendorRFQStatus(VendorRFQNumber, id);
                }
                else if (jsonData.ContainsKey("redoid"))
                {
                    VendorRFQNumber = Convert.ToString(jsonData["redoid"]);
                    sysMsg.isPass = !dm.CheckVendorRFQStatus(VendorRFQNumber, id);
                }
            }
            catch (Exception ex)
            {
                sysMsg.isPass = false;
            }

            var returnData = new
            {
                SysMsg = sysMsg
            };
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult AssignSupplier()
        //{
        //    SystemMessages sysmgs = new SystemMessages();
        //    string vvirfqid = string.Empty;
        //    sysmgs.isPass = true;
        //    try
        //    {

        //        //curd.Save();
        //        string number = Request.Form["Number"];
        //        string suppiler = Request.Form["SupplierCode"];
        //        string VVINumber = number + "-" + suppiler;
        //        if (VVIRFQManager.CheckVVINumberExists(VVINumber))
        //        {
        //            sysmgs.isPass = false;
        //            sysmgs.MessageType = "Aleady Assgined";
        //            sysmgs.Messages.Add("Aleady Assgined", "Aleady Assgined");
        //        }
        //        else
        //        {

        //            NameValueCollection Result = VVIRFQManager.AssignSuppliers(Request);

        //            if (string.IsNullOrEmpty(Result["Mgs"]) == false)
        //            {
        //                sysmgs.isPass = false;
        //                sysmgs.MessageType = "Assgin Error";
        //                sysmgs.Messages.Add("Assgin Error", Result["Mgs"]);

        //            }
        //            else
        //            {
        //                sysmgs.isPass = true;
        //                sysmgs.MessageType = "Assgin Success";
        //                //sysmgs.Messages.Add("Save Success", Result["Mgs"]);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        sysmgs.isPass = false;
        //        sysmgs.MessageType = "Assgin Error";
        //        sysmgs.Messages.Add("Assgin Error", ex.ToString());
        //    }

        //    var returnData = new
        //    {
        //        SysMsg = sysmgs
        //    };

        //    return Json(returnData, JsonRequestBehavior.AllowGet);

        //}
        public ActionResult PostBacKToGP()
        {

            VVIRFQManager vvirfqdetail = new VVIRFQManager();
            SystemMessages sysmgs = new SystemMessages();

            VVIRFQManager.SavaAndCheckData(ref vvirfqdetail, ref sysmgs, Request);
            string html = "";
            if (sysmgs.isPass == true)
            {

                html = SGP.BLL.UIManager.VVIUIManager.GenrateVVIRFQDetail(vvirfqdetail, vvirfqdetail.ID.ToString(), "Edit");
            }
            // string aa = Request.Form["Number"];
            var returnData = new
            {
                HTML = html,
                ID = vvirfqdetail.ID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);

        }
        public ActionResult RefreshVVIProductInformation()
        {
            int id = 0;
            string postData = Request["postData"];
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
            string dataId = Convert.ToString(jsonData["dataId"]);
            List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_VVI);
            Int32.TryParse(dataId, out id);
            string content = "";
            WFTemplate template = new WFTemplate(3, id);
            if (id > 0)
            {
                VVIQuotationDetail vviDetail = new VVIQuotationDetail();
                vviDetail.FillCategoryData(lfc, id);
            }
            foreach (FieldCategory fc in lfc)
            {
                if (fc.CategoryName == "VVI-Product Information")
                {
                    content = BI.SGP.BLL.UIManager.DetailUIHelper.GenrateCategorySubFieldsForVVI(fc);
                }
            }
            var jsonObject = new { content = content };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);

        }

        public ActionResult VendorRfqReport()
        {
            ViewBag.Title = "Vendor RFQ Report";
            ViewBag.GridGroup = "VendorRFQReportGrid";
            ViewBag.SearchGroup = "VendorRFQReportSearch";
            return View();
        }

        [MyAuthorize]
        public ActionResult VVIMasterReport()
        {
            ViewBag.Title = "VVI Master Report";
            ViewBag.GridGroup = "VVIMasterReportGrid";
            ViewBag.SearchGroup = "VVIMasterReportSearch";
            return View();
        }

        public ActionResult GetComputedValueForVVI(string postData)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;



            List<double> OP = DetailUIHelper.SubDataToList<double>(jsonData["OP"]);
            List<double> VendorXPlan = DetailUIHelper.SubDataToList<double>(jsonData["VendorXPlan"]);
            List<double> OPXPlan = DetailUIHelper.SubDataToList<double>(jsonData["OPXPlan"]);

            for (int i = 0; i < OPXPlan.Count; i++)
            {

                OPXPlan[i] = OP[i] + VendorXPlan[i];

            }
            var jsonObject = new
            {
                OP = OP,
                VendorXPlan = VendorXPlan,
                OPXPlan = OPXPlan
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetComputedValue(string postData)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

            double UnitSizeWidth = ParseHelper.Parse<double>(jsonData["UnitSizeWidth"]);
            double UnitSizeLength = ParseHelper.Parse<double>(jsonData["UnitSizeLength"]);
            double UnitArea = ParseHelper.Parse<double>(jsonData["UnitArea"]);
            double ArrayPerWorkingPanel = ParseHelper.Parse<double>(jsonData["ArrayPerWorkingPanel"]);
            double UnitPerArray = ParseHelper.Parse<double>(jsonData["UnitPerArray"]);
            double ArraySizeWidth = ParseHelper.Parse<double>(jsonData["ArraySizeWidth"]);
            double ArraySizeLength = ParseHelper.Parse<double>(jsonData["ArraySizeLength"]);
            double PanelSizeWidth = ParseHelper.Parse<double>(jsonData["PanelSizeWidth"]);
            double PanelSizeLength = ParseHelper.Parse<double>(jsonData["PanelSizeLength"]);
            double VariableCost = ParseHelper.Parse<double>(jsonData["VariableCost"]);

            SupplierRFQDetail detail = new SupplierRFQDetail();
            detail.UnitSizeWidth = UnitSizeWidth;
            detail.UnitSizeLength = UnitSizeLength;
            detail.UnitArea = UnitArea;
            detail.ArrayPerWorkingPanel = ArrayPerWorkingPanel;
            detail.UnitPerArray = UnitPerArray;
            detail.ArraySizeWidth = ArraySizeWidth;
            detail.ArraySizeLength = ArraySizeLength;
            detail.PanelSizeWidth = PanelSizeWidth;
            detail.PanelSizeLength = PanelSizeLength;
            detail.VariableCost = VariableCost;

            string panelUtilization = detail.PanelUtilization;
            string sqInchPriceUSD = detail.SqInchPriceUSD;

            var jsonObject = new
            {
                PanelUtilization = panelUtilization,
                SqInchPriceUSD = sqInchPriceUSD
            };

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierRFQImportExcel()
        {
            return View();
        }

        public ActionResult BatchSubmitVendorRFQ()
        {
            string keyValues = Request.QueryString["NUMBER"];
            SystemMessages sysMsg = WFHelper.CreateMessages();
            string errorMsg = string.Empty;
            if (!string.IsNullOrEmpty(keyValues))
            {
                string[] numbers = keyValues.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                SupplierRFQDetail sr = new SupplierRFQDetail();
                foreach (string number in numbers)
                {
                    using (TScope ts = new TScope())
                    {
                        SystemMessages msg = WFHelper.CreateMessages();
                        int id = sr.GetSupplierRFQId(number);
                        WFTemplate wfTemplate = new WFTemplate("SUPPLIERWF", id, number);
                        wfTemplate.FirstActivity.CheckData(msg);
                        if (!msg.isPass)
                        {
                            ts.Rollback();
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add("Submit Fail", msg.MessageString);
                        }
                        else
                        {
                            sr.UpdateSupplierRfqStatus(id, number);
                            SkipRFQWFToNextForVVI(id, msg);
                            wfTemplate.LastActivity.DoAction();
                            sysMsg.Messages.Add("Submit Success", string.Format("{0} Submit Success", number));
                        }
                    }
                }
            }
            return Json(sysMsg);
        }
    }
}
