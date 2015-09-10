using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SGP.DBUtility;
using BI.SGP.BLL.Models;
using System.Text;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.WF;
using System.Data;
using System.IO;
using BI.SGP.BLL.DataModels;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace BI.SGP.Web.Controllers
{
    public class PricingController : Controller
    {
        //
        // GET: /Pricing/
         [MyAuthorize]
        public ActionResult PricingView()
        {
            //BI.SGP.Web.Models.PagingGrid dd = new Models.PagingGrid();
            ViewBag.UserID = AccessControl.CurrentLogonUser.Uid;
            return View();
        }

         [MyAuthorize]
        public ActionResult Detail(SGP.BLL.Models.RFQDetail rf)
        {
            int RFQID = 0;
            if (Request.QueryString["RFQID"] != null)
            {
                RFQID = Int32.Parse(Request.QueryString["RFQID"]);
            }
            if (SGP.BLL.Models.RFQManager.GetDetail(RFQID).Count > 0)
            {

                rf = SGP.BLL.Models.RFQManager.GetDetail(RFQID)[0];
            }

            if (RFQID > 0)
            {
                SGP.BLL.WF.WFTemplate wf = new WFTemplate(1, RFQID);
                if (wf.CurrentActivity != null)
                {
                    ViewData["CurrWFID"] = wf.CurrentActivity.ID;
                }
                ViewData["LastWFID"] = wf.LastActivity.ID;

            }
            return View(rf);
        }



        /// <summary>
        /// 通过AJAX实时计算各个值
        /// </summary>
        /// <returns></returns>
        public ActionResult GetComputedValue()
        {
            string UnitPrice1 = Request.QueryString["UnitPrice1"];
            string UnitPrice2 = Request.QueryString["UnitPrice2"];
            string UnitPrice3 = Request.QueryString["UnitPrice3"];
            string UnitPrice4 = Request.QueryString["UnitPrice4"];
            string UnitPrice5 = Request.QueryString["UnitPrice5"];
            string UnitPrice6 = Request.QueryString["UnitPrice6"];
            string UnitPrice7 = Request.QueryString["UnitPrice7"];
            string UnitPrice8 = Request.QueryString["UnitPrice8"];
            string UnitPrice9 = Request.QueryString["UnitPrice9"];
            string UnitPrice10 = Request.QueryString["UnitPrice10"];

            string FixedCost = Request.QueryString["FixedCost"];
            string MaterialCost = Request.QueryString["MaterialCost"];
            string VariableCost = Request.QueryString["VariableCost"];
            string UnitPerWorkingPanel = Request.QueryString["UnitPerWorkingPanel"];
            string ArraySizeWidth = Request.QueryString["ArraySizeWidth"];
            string ArraySizeLength = Request.QueryString["ArraySizeLength"];
            string PanelSizeWidth = Request.QueryString["PanelSizeWidth"];
            string PanelSizeLength = Request.QueryString["PanelSizeLength"];
            string TargetPrice = Request.QueryString["TargetPrice"];

            string ExchangRatePerUSD = Request.QueryString["ExchangRatePerUSD"];
            string ShipTermsAdder = Request.QueryString["ShipTermsAdder"];
            string MinSqInch1 = Request.QueryString["MinSqInch"];
            string LayerCount = Request.QueryString["LayerCount"];
            string UnitPerArray = Request.QueryString["UnitPerArray"];
            string UnitType = Request.QueryString["UnitType"];
            string TotalCost1 = Request.QueryString["TotalCost"];


            RFQDetail detail = new RFQDetail();
            double UnitPrice1Int = 0; double.TryParse(UnitPrice1, out UnitPrice1Int);
            double UnitPrice2Int = 0; double.TryParse(UnitPrice2, out UnitPrice2Int);
            double UnitPrice3Int = 0; double.TryParse(UnitPrice3, out UnitPrice3Int);
            double UnitPrice4Int = 0; double.TryParse(UnitPrice4, out UnitPrice4Int);
            double UnitPrice5Int = 0; double.TryParse(UnitPrice5, out UnitPrice5Int);
            double UnitPrice6Int = 0; double.TryParse(UnitPrice6, out UnitPrice6Int);
            double UnitPrice7Int = 0; double.TryParse(UnitPrice7, out UnitPrice7Int);
            double UnitPrice8Int = 0; double.TryParse(UnitPrice8, out UnitPrice8Int);
            double UnitPrice9Int = 0; double.TryParse(UnitPrice9, out UnitPrice9Int);
            double UnitPrice10Int = 0; double.TryParse(UnitPrice10, out UnitPrice10Int);
            double fixedCostInt = 0; double.TryParse(FixedCost, out fixedCostInt);
            double MaterialCostInt = 0; double.TryParse(MaterialCost, out MaterialCostInt);
            double VariableCostInt = 0; double.TryParse(VariableCost, out VariableCostInt);
            double UnitPerWorkingPanelInt = 0; double.TryParse(UnitPerWorkingPanel, out UnitPerWorkingPanelInt);
            double ArraySizeWidthInt = 0; double.TryParse(ArraySizeWidth, out ArraySizeWidthInt);
            double ArraySizeLengthInt = 0; double.TryParse(ArraySizeLength, out ArraySizeLengthInt);
            double PanelSizeWidthInt = 0; double.TryParse(PanelSizeWidth, out PanelSizeWidthInt);
            double PanelSizeLengthInt = 0; double.TryParse(PanelSizeLength, out PanelSizeLengthInt);
            double TargetPriceInt = 0; double.TryParse(TargetPrice, out TargetPriceInt);
            double ExchangRatePerUSDInt = 0; double.TryParse(ExchangRatePerUSD, out ExchangRatePerUSDInt);
            double ShipTermsAdderInt = 0; double.TryParse(ShipTermsAdder, out ShipTermsAdderInt);
            double MinSqInchInt = 0; double.TryParse(MinSqInch1.Replace('%', '0'), out MinSqInchInt);
            double LayerCountInt = 0; double.TryParse(LayerCount, out LayerCountInt);
            double UnitPerArrayInt = 0; double.TryParse(UnitPerArray, out UnitPerArrayInt);
            double TotalCostInt = 0; double.TryParse(TotalCost1, out TotalCostInt);

            detail.UnitPrice1 = UnitPrice1Int;
            detail.UnitPrice2 = UnitPrice2Int;
            detail.UnitPrice3 = UnitPrice3Int;
            detail.UnitPrice4 = UnitPrice4Int;
            detail.UnitPrice5 = UnitPrice5Int;
            detail.UnitPrice6 = UnitPrice6Int;
            detail.UnitPrice7 = UnitPrice7Int;
            detail.UnitPrice8 = UnitPrice8Int;
            detail.UnitPrice9 = UnitPrice9Int;
            detail.UnitPrice10 = UnitPrice10Int;

            detail.FixedCost = fixedCostInt;
            detail.MaterialCost = MaterialCostInt;
            detail.VariableCost = VariableCostInt;
            detail.UnitPerWorkingPanel = UnitPerWorkingPanelInt;
            detail.ArraySizeWidth = ArraySizeWidthInt;
            detail.ArraySizeLength = ArraySizeLengthInt;
            detail.PanelSizeWidth = PanelSizeWidthInt;
            detail.PanelSizeLength = PanelSizeLengthInt;
            detail.TargetPrice = TargetPriceInt;
            detail.ExchangRatePerUSD = ExchangRatePerUSDInt;
            detail.ShipTermsAdder = ShipTermsAdderInt;
            detail.MinSqInch = MinSqInchInt;
            detail.LayerCount = LayerCountInt;
            detail.UnitPerArray = UnitPerArrayInt;
            detail.UnitType = UnitType;
            detail.TotalCost = TotalCostInt;

            double TargetPrice1 = Math.Round(detail.TargetPrice, 2);
            double TargetASP = Math.Round(detail.TargetASP, 2);
            double MinASP = Math.Round(detail.MinASP, 2);
            double TargetASPL = Math.Round(detail.TargetASPL, 2);
            double MinASPL = Math.Round(detail.MinASPL, 2);
            double TargetSqIn = Math.Round(detail.TargetSqIn, 2);
            double MinSqInch = Math.Round(detail.MinSqInch, 2);
            double TargetCLsqin = Math.Round(detail.TargetCLsqin, 2);
            double MinCLsqin = Math.Round(detail.MinCLsqin, 2);
            double TargetVSActucal = Math.Round(detail.TargetVSActucal, 2);
            double TotalCost = Math.Round(detail.TotalCost, 4);
            double OP = Math.Round(detail.OP, 2);
            double MP = Math.Round(detail.MP, 2);
            double OP1 = Math.Round(detail.OP1, 2);
            double MP1 = Math.Round(detail.MP1, 2);
            double OP2 = Math.Round(detail.OP2, 2);
            double MP2 = Math.Round(detail.MP2, 2);
            double OP3 = Math.Round(detail.OP3, 2);
            double MP3 = Math.Round(detail.MP3, 2);
            double OP4 = Math.Round(detail.OP4, 2);
            double MP4 = Math.Round(detail.MP4, 2);
            double OP5 = Math.Round(detail.OP5, 2);
            double MP5 = Math.Round(detail.MP5, 2);
            double OP6 = Math.Round(detail.OP6, 2);
            double MP6 = Math.Round(detail.MP6, 2);
            double OP7 = Math.Round(detail.OP7, 2);
            double MP7 = Math.Round(detail.MP7, 2);
            double OP8 = Math.Round(detail.OP8, 2);
            double MP8 = Math.Round(detail.MP8, 2);
            double OP9 = Math.Round(detail.OP9, 2);
            double MP9 = Math.Round(detail.MP9, 2);
            double OP10 = Math.Round(detail.OP10, 2);
            double MP10 = Math.Round(detail.MP10, 2);
            string PanelUtilization = detail.PanelUtilization;

            var jsonObject = new
            {

                TargetASP = TargetASP,
                MinASP = MinASP,
                TargetASPL = TargetASPL,
                MinASPL = MinASPL,
                TargetSqIn = TargetSqIn,
                MinSqInch = MinSqInch,
                TargetCLsqin = TargetCLsqin,
                MinCLsqin = MinCLsqin,
                TargetVSActucal = TargetVSActucal,
                TotalCost = TotalCost,
                OP = OP,
                MP = MP,
                OP1 = OP1,
                MP1 = MP1,
                OP2 = OP2,
                MP2 = MP2,
                OP3 = OP3,
                MP3 = MP3,
                OP4 = OP4,
                MP4 = MP4,
                OP5 = OP5,
                MP5 = MP5,
                OP6 = OP6,
                MP6 = MP6,
                OP7 = OP7,
                MP7 = MP7,
                OP8 = OP8,
                MP8 = MP8,
                OP9 = OP9,
                MP9 = MP9,
                OP10 = OP10,
                MP10 = MP10,
                TargetPrice1 = TargetPrice1,
                PanelUtilization = PanelUtilization
            };


            return Json(jsonObject, JsonRequestBehavior.AllowGet);
        }

  
        /// <summary>
        /// 检查以及保存数据，返回实体到RFQDetail，返回结果到SystemMessages
        /// </summary>
        /// <returns></returns>        
        private void Save(ref RFQDetail rfdetail, ref SystemMessages sysmgs)
        {
            //check data检查数据，返回状态到sysmgs
            RFQManager.CheckData(Request, ref sysmgs);

            //检查数据通过则执行保存到数据库
            if (sysmgs.isPass)
            {
                NameValueCollection Result = BI.SGP.BLL.Models.RFQManager.Save(Request);


                //if (Result["RFQID"] != null && Result["Mgs"] != null)
                //{

                //    sysmgs.isPass = false;
                //    sysmgs.MessageType = "Update Error";
                //    sysmgs.Messages.Add("Update Error", Result["Mgs"]);

                //}
                string ID = Result["RFQID"];
                int rfqId = 0; Int32.TryParse(ID, out rfqId);

                BI.SGP.BLL.Models.RFQManager.ForceCloseRFQ(rfqId);
                //有MSG返回，表示保存失败
                if (string.IsNullOrEmpty(Result["Mgs"]) == false)
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "Save Error";
                    sysmgs.Messages.Add("Save Error", Result["Mgs"]);

                }
                else
                {
                    sysmgs.isPass = true;
                    sysmgs.MessageType = "Save Success";
                    //sysmgs.Messages.Add("Save Success", Result["Mgs"]);
                }

                if (rfqId > 0)
                {
                    rfdetail = RFQManager.GetDetailById(rfqId);
                }

            }
            else // 检查数据不通过
            {
                sysmgs.MessageType = "Error";
            }

            //sysmgs.Messages.Add("RFQ:", "format error");
        }


        private void ReQuote(ref RFQDetail rfdetail, ref SystemMessages sysmgs)
        {
            //check data检查数据，返回状态到sysmgs
            RFQManager.CheckData(Request, ref sysmgs);

            //检查数据通过则执行保存到数据库
            if (sysmgs.isPass)
            {
                NameValueCollection Result = BI.SGP.BLL.Models.RFQManager.ReQuoteData(Request);

                //if (Result["RFQID"] != null && Result["Mgs"] != null)
                //{

                //    sysmgs.isPass = false;
                //    sysmgs.MessageType = "Update Error";
                //    sysmgs.Messages.Add("Update Error", Result["Mgs"]);

                //}
                string ID = Result["RFQID"];
                int rfqId = 0; Int32.TryParse(ID, out rfqId);

                //有MSG返回，表示保存失败
                if (string.IsNullOrEmpty(Result["Mgs"]) == false)
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "Save Error";
                    sysmgs.Messages.Add("Save Error", Result["Mgs"]);

                }
                else
                {
                    sysmgs.isPass = true;
                    sysmgs.MessageType = "Save Success";
                    //sysmgs.Messages.Add("Save Success", Result["Mgs"]);
                }

                if (rfqId > 0)
                {
                    if (BI.SGP.BLL.Models.RFQManager.GetDetail(rfqId) != null)
                    {
                        rfdetail = BI.SGP.BLL.Models.RFQManager.GetDetail(rfqId)[0];
                    }
                }

            }
            else // 检查数据不通过
            {
                sysmgs.MessageType = "Error";
            }

            //sysmgs.Messages.Add("RFQ:", "format error");
        }
        private void SaveAs(ref RFQDetail rfdetail, ref SystemMessages sysmgs)
        {
            //check data检查数据，返回状态到sysmgs
            RFQManager.CheckData(Request, ref sysmgs);

            //检查数据通过则执行保存到数据库
            if (sysmgs.isPass)
            {
                NameValueCollection Result = BI.SGP.BLL.Models.RFQManager.SaveAs(Request);

                //if (Result["RFQID"] != null && Result["Mgs"] != null)
                //{

                //    sysmgs.isPass = false;
                //    sysmgs.MessageType = "Update Error";
                //    sysmgs.Messages.Add("Update Error", Result["Mgs"]);

                //}
                string ID = Result["RFQID"];
                int rfqId = 0; Int32.TryParse(ID, out rfqId);

                //有MSG返回，表示保存失败
                if (string.IsNullOrEmpty(Result["Mgs"]) == false)
                {
                    sysmgs.isPass = false;
                    sysmgs.MessageType = "Save Error";
                    sysmgs.Messages.Add("Save Error", Result["Mgs"]);

                }
                else
                {
                    sysmgs.isPass = true;
                    sysmgs.MessageType = "Save Success";
                    //sysmgs.Messages.Add("Save Success", Result["Mgs"]);
                }

                if (rfqId > 0)
                {
                    if (BI.SGP.BLL.Models.RFQManager.GetDetail(rfqId) != null)
                    {
                        rfdetail = BI.SGP.BLL.Models.RFQManager.GetDetail(rfqId)[0];
                    }
                }

            }
            else // 检查数据不通过
            {
                sysmgs.MessageType = "Error";
            }

            //sysmgs.Messages.Add("RFQ:", "format error");
        }

        [HttpPost]
        public ActionResult SaveData()
        {

            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();

            Save(ref rfdetail, ref sysmgs);
            string html = "";
            if (sysmgs.isPass == true)
            {
                SGP.BLL.WF.WFTemplate wf = new WFTemplate(1, rfdetail.RFQID);
                html = SGP.BLL.UIManager.UIManager.GenrateModelforRFQDetail(rfdetail, wf.CurrentActivity.ID.ToString());
            }
            // string aa = Request.Form["Number"];
            var returnData = new
            {
                HTML = html,
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveAsData()
        {

            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();

            SaveAs(ref rfdetail, ref sysmgs);

            SGP.BLL.WF.WFTemplate wf = new WFTemplate(1, rfdetail.RFQID);
            string html = "";
            html = SGP.BLL.UIManager.UIManager.GenrateModelforRFQDetail(rfdetail, wf.CurrentActivity.ID.ToString());
            // string aa = Request.Form["Number"];
            var returnData = new
            {
                HTML = html,
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveSubmitData()
        {
            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();

            Save(ref rfdetail, ref sysmgs);
            WFTemplate wfTemplate = new WFTemplate("DefaultWF", rfdetail.RFQID);


            if (sysmgs.isPass)
            {
                try
                {

                    if (RFQManager.IsPendingStatus(rfdetail.RFQID, wfTemplate.NextActivity.ID) == false)
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add("System Exception", "the HitRate Status is Pending, do not allow go to the Closure Status");
                    }
                    else
                    {
                        sysmgs.Merge(wfTemplate.Run());
                    }
                    RFQManager.PostRFQToVVI(rfdetail.RFQID);
                }
                catch (Exception ex)
                {
                    sysmgs.isPass = false;
                    sysmgs.Messages.Add("System Exception", ex.Message);
                }
            }
            string[] WFIDS = { "5", "6", "7" };
            string PDFDIV = "";
            if (WFIDS.Contains(wfTemplate.CurrentActivity.ID.ToString()))
            {
                PDFDIV = @"<button id=""btnDownlPDF"" class=""btn btn-purple""   onclick=""return downloadpdf();"" >
                                                                 Download PDF
                                                                <i class=""icon-file small-30""></i>
                                                                 </button>";
            }

            var returnData = new
            {
                HTML = SGP.BLL.UIManager.UIManager.GenrateModelforRFQDetail(rfdetail, wfTemplate.CurrentActivity.ID.ToString()),
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs,
                PDF = PDFDIV
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAndSkip()
        {
            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();
            Save(ref rfdetail, ref sysmgs);
            WFTemplate wfTemplate = new WFTemplate("DefaultWF", rfdetail.RFQID);
            if (sysmgs.isPass)
            {
                string toActivityId = Request.Form["toActivityId"];
                int toActId = 0;
                int.TryParse(toActivityId, out toActId);

                if (toActId > 0)
                {
                    try
                    {
                        if (RFQManager.IsPendingStatus(rfdetail.RFQID, toActId) == false)
                        {
                            sysmgs.isPass = false;
                            sysmgs.Messages.Add("System Exception", "the HitRate Status is Pending, do not allow go to the Closure Status");
                        }
                        else
                        {
                            sysmgs.Merge(wfTemplate.Skip(toActId));
                        }
                        RFQManager.PostRFQToVVI(rfdetail.RFQID);
                    }
                    catch (Exception ex)
                    {
                        sysmgs.isPass = false;
                        sysmgs.Messages.Add("System Exception", ex.Message);
                    }
                }
            }
            string[] WFIDS = { "5", "6", "7" };
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
                HTML = SGP.BLL.UIManager.UIManager.GenrateModelforRFQDetail(rfdetail, wfTemplate.CurrentActivity.ID.ToString()),
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs,
                PDF = PDFDIV
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveRequoteData()
        {

            RFQDetail rfdetail = new RFQDetail();
            SystemMessages sysmgs = new SystemMessages();

            ReQuote(ref rfdetail, ref sysmgs);

            SGP.BLL.WF.WFTemplate wf = new WFTemplate(1, rfdetail.RFQID);
            // string aa = Request.Form["Number"];
            var returnData = new
            {
                HTML = SGP.BLL.UIManager.UIManager.GenrateModelforRFQDetail(rfdetail, wf.CurrentActivity.ID.ToString()),
                RFQNumber = rfdetail.Number,
                RFQID = rfdetail.RFQID,
                SysMsg = sysmgs
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Test(BI.SGP.Web.Models.test t)
        {

            if (ModelState.IsValid)
            {
                if (Request.Form["id"] != null)
                {

                    t.id = int.Parse(Request.Form["id"]);
                }
                RedirectToAction("Detail");
            }

            t.id = 12;
            ViewBag.test = t;
            return View();

        }

        public FileResult DownloadPDF()
        {
            string KeyValues = Request.QueryString["RFQID"];
            string fileName = "";
            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/" + Guid.NewGuid() + ".tmp");
            MemoryStream mem = new MemoryStream();
            if (!String.IsNullOrEmpty(KeyValues))
            {
                KeyValues = KeyValues.TrimEnd(',');
                string[] ids = KeyValues.Split(',');
                if (ids.Length == 1)
                {
                    int id = Int32.Parse(ids[0]);

                    BI.SGP.BLL.Models.PDFDownLoad pdf = new PDFDownLoad();
                    if (pdf.getPDF(ref mem, id, out fileName))
                    {

                        if (fileName == "")
                        {
                            fileName = "multek_" + DateTime.Now.ToString("mmssffff") + ".pdf";
                        }
                        else
                        {
                            fileName += ".pdf";
                        }
                        using (var fileStream = FileHelper.CreateFile(tempFile))
                        {
                            fileStream.Write(mem.GetBuffer(), 0, mem.GetBuffer().Length);
                        }
                    }
                }
                else
                {
                    ZipHelper.PDFToZip(ids, mem);
                    fileName = "PDF_Packages_" + DateTime.Now.ToString("mmss") + ".zip";
                    using (var fileStream = FileHelper.CreateFile(tempFile))
                    {
                        mem.WriteTo(fileStream);
                    }
                }
            }

            return File(tempFile, "application/octet-stream", fileName);
        }

        public ActionResult WFRun(string KeyValues)
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();

            if (!String.IsNullOrEmpty(KeyValues))
            {
                string[] ids = KeyValues.Split(',');
                if (ids != null && ids.Length > 0)
                {
                    using (TScope ts = new TScope())
                    {
                        foreach (string id in ids)
                        {
                            if (!String.IsNullOrEmpty(id))
                            {
                                int entityId = 0;
                                if (int.TryParse(id, out entityId))
                                {
                                    try
                                    {
                                        WFTemplate wfTemplate = new WFTemplate("DefaultWF", entityId);
                                        sysMsg.Merge(wfTemplate.Run());
                                    }
                                    catch (Exception ex)
                                    {
                                        sysMsg.isPass = false;
                                        sysMsg.Messages.Add("System Exception", ex.Message);
                                    }
                                }
                                else
                                {
                                    sysMsg.isPass = false;
                                    sysMsg.Messages.Add("ID Error", String.Format("[{0}] is not a valid ID", id));
                                }
                            }
                        }

                        if (!sysMsg.isPass)
                        {
                            ts.Rollback();
                        }
                    }
                }
            }

            return Json(sysMsg);
        }
        public ActionResult WFRunSkips(string KeyValues, string ToID)
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();


            string toActivityId = ToID;
            int toActId = 0;
            int.TryParse(toActivityId, out toActId);


            if (!String.IsNullOrEmpty(KeyValues))
            {
                string[] ids = KeyValues.Split(',');
                if (ids != null && ids.Length > 0)
                {
                    using (TScope ts = new TScope())
                    {
                        foreach (string id in ids)
                        {
                            if (!String.IsNullOrEmpty(id))
                            {
                                int entityId = 0;
                                if (int.TryParse(id, out entityId))
                                {
                                    try
                                    {
                                        WFTemplate wfTemplate = new WFTemplate("DefaultWF", entityId);
                                        if (RFQManager.IsPendingStatus(entityId, toActId) == false)
                                        {
                                            sysMsg.isPass = false;
                                            sysMsg.Messages.Add("System Exception", "the HitRate Status is Pending, do not allow go to the Closure Status");
                                        }
                                        else
                                        {
                                            sysMsg.Merge(wfTemplate.Skip(toActId));
                                        }
                                        RFQManager.PostRFQToVVI(entityId);
                                    }
                                    catch (Exception ex)
                                    {
                                        sysMsg.isPass = false;
                                        sysMsg.Messages.Add("System Exception", ex.Message);
                                    }
                                }
                                else
                                {
                                    sysMsg.isPass = false;
                                    sysMsg.Messages.Add("ID Error", String.Format("[{0}] is not a valid ID", id));
                                }
                            }
                        }

                        if (!sysMsg.isPass)
                        {
                            ts.Rollback();
                        }
                    }
                }
            }

            return Json(sysMsg);
        }
    }
}
