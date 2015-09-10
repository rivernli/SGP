using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BI.SGP.BLL.Utils;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Models.Detail;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using SGP.DBUtility;
using System.IO;

namespace BI.SGP.Web.Controllers
{
    public class B2FController : Controller
    {
         [MyAuthorize]
        public ActionResult PricingView()
        {
            ViewBag.ExcelView = "~/Views/Shared/_ExportExcel.cshtml";
            ViewBag.GridGroup = "PricingGridForFPC";
            ViewBag.SearchGroup = "SearchForFPC";
            ViewBag.UserID = AccessControl.CurrentLogonUser.Uid;
            return View();
        }
         [MyAuthorize]
        public ActionResult Detail(string RFQID)
        {
            int dataId = ParseHelper.Parse<int>(RFQID);
            FPCRFQDetail rfqdetail = new FPCRFQDetail();
            WFTemplate template = new WFTemplate(2, dataId);
            List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_FPC);

            if (dataId > 0)
            {
                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                b2Detail.FillCategoryData(lfc, dataId);
            }

            ViewBag.WFTemplate = template;

            if (template.CurrentActivity != null)
            {
                ViewData["CurrWFID"] = template.CurrentActivity.ID;
            }
            ViewData["LastWFID"] = template.LastActivity.ID;
            ViewBag.Categories = lfc;
            ViewBag.DataId = dataId;
            return View(rfqdetail);
        }
         [MyAuthorize]
        public ActionResult ImportExcel()
        {

            return View();
        }
        public string REVFormat(string rev)
        {
            int i = 0;
            Int32.TryParse(rev, out i);
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
        }
        public string defaultbuilding(string build)
        { 
            if(string.IsNullOrEmpty(build))
            {
                return "B2F";
            }
            else
            {
                return build;
            }
                
        }
        private int Save(string postData, SystemMessages sysMsg)
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

                List<FieldCategory> lfc = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_FPC);

                foreach (FieldCategory fc in lfc)
                {
                    if (data.ContainsKey(fc.ID))
                    {
                        fc.CheckDataType(data[fc.ID] as Dictionary<string, object>, sysMsg);
                    }
                }
                int newid = 0;
                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            
                            B2FQuotationDetail dm = new B2FQuotationDetail(lfc, data);
                            if (operation == B2FQuotationDetail.OPERATION_REQUOTE)
                            {
                                newid=dm.ReQuote();
                                if (id != newid&&newid>0)
                                {
                                    dm.CloneAttachments(id, newid);

                                }
                                id = newid;
                            }
                            else if (operation == B2FQuotationDetail.OPERATION_CLONE)
                            {
                                newid = dm.Clone();
                                if (id != newid && newid > 0)
                                {
                                    dm.CloneAttachments(id, newid);

                                }
                                id = newid;
                            }
                            else
                            {
                                if (id > 0)
                                {
                                    BI.SGP.BLL.DataModels.FieldInfo Numberinfor = dm.AllMasterFields.Find(t => string.Compare(t.FieldName, "ExtNumber", true) == 0);
                                    BI.SGP.BLL.DataModels.FieldInfo Buildinginfor = dm.AllMasterFields.Find(t => string.Compare(t.FieldName, "Building", true) == 0);
                                    BI.SGP.BLL.DataModels.FieldInfo Revinfo = dm.AllMasterFields.Find(t => string.Compare(t.FieldName, "InternalRevisionNumber", true) == 0);
                                    List<SqlParameter> conparam = new List<SqlParameter>();
                                    conparam.Add(new SqlParameter("@Building", defaultbuilding(Buildinginfor.DataValue.ToString())));
                                    conparam.Add(new SqlParameter("@InternalRevisionNumber", REVFormat(Revinfo.DataValue.ToString())));
                                    conparam.Add(new SqlParameter("@ID", id));
                                    //int ExistsNumber = DbHelperSQL.GetSingle<int>("select Top 1 ID from SGP_RFQ where ID=@ID and Building=@Building and InternalRevisionNumber=@InternalRevisionNumber", conparam.ToArray());
                                    int ExistsNumber = DbHelperSQL.GetSingle<int>("select Top 1 ID from SGP_RFQ where ID=@ID and InternalRevisionNumber=@InternalRevisionNumber", conparam.ToArray());
                                    if (ExistsNumber > 0)
                                    {
                                        id = ExistsNumber;
                                        dm.Update(id);
                                    }
                                    else
                                    {
                                        int Original = DbHelperSQL.GetSingle<int>("select Top 1 ID from SGP_RFQ where ID=@ID and Number=ExtNumber", new SqlParameter("@ID",id));
                                        if (Original > 0)
                                        {
                                            DbHelperSQL.ExecuteSql("Update SGP_RFQ Set StatusID=0 where ID=@ID ", new SqlParameter("@ID", id));
 
                                        }
                                        newid = dm.ChangeBuildREV();
                                        if (id != newid && newid > 0)
                                        {
                                            dm.CloneAttachments(id, newid);

                                        }
                                        id = newid;
                                    }
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
        public ActionResult SaveData()
        {
            SystemMessages sysMsg = new SystemMessages();
            
            string html = "";
            string wfStatus = "";
            int id = 0;
            string postData = Request["postData"];
            id = Save(postData, sysMsg);

            
            
            if (id > 0 && sysMsg.isPass)
            {
                WFTemplate wfTemplate = new WFTemplate(2, id);
                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_FPC);
                b2Detail.FillCategoryData(lfc, id);
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
        public ActionResult WFSkip()
        {
            SystemMessages sysMsg = WFHelper.CreateMessages();
            string html = "";
            string wfStatus = "";
            string PDFDIV = "";
            string postData = Request["postData"];
            int id = Save(postData, sysMsg);

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

                    if (!sysMsg.isPass)
                    {
                        ts.Rollback();
                    }
                }

                B2FQuotationDetail b2Detail = new B2FQuotationDetail();
                List<FieldCategory> lfc = FieldCategory.GetMasterCategorys(FieldCategory.Category_TYPE_FPC);
                b2Detail.FillCategoryData(lfc, id);
                html = DetailUIHelper.GenrateCategories(lfc, wfTemplate);
                wfStatus = wfTemplate.Status == WorkflowStatus.Finished ? "Finished" : "";
                string[] WFIDS = { "203","204","205", "206", "207" };
               
                if (WFIDS.Contains(wfTemplate.CurrentActivity.ID.ToString()))
                {
                    PDFDIV = @"<button id=""btnDownlPDF"" class=""btn btn-purple""   onclick=""return downloadpdf();"" >
                                                                 Download PDF
                                                                <i class=""icon-file small-30""></i>
                                                                 </button>";
                }
            }
           
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg,
                Html = html,
                PDF=PDFDIV,
                wfStatus = wfStatus
            };
            return Json(jsonResult);
        }
        public ActionResult GetComputedValue(string postData)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;
            int  VolumePerMonth=ParseHelper.Parse<int>(jsonData["VolumePerMonth"]);
            double  DueDate=ParseHelper.Parse<double>(jsonData["DueDate"]);
            double  ExpectedLifeTime=ParseHelper.Parse<double>(jsonData["ExpectedLifeTime"]);
            double  EstQuoteSize=ParseHelper.Parse<double>(jsonData["EstQuoteSize"]);
            //double  MinComputedSize=ParseHelper.Parse<double>(jsonData["MinComputedSize"]);
            //string  Currency=jsonData["Currency"].ToString();
          //  double  ExchangRatePerUSD=ParseHelper.Parse<double>(jsonData["ExchangRatePerUSD"]);
            double  SetupCharge=ParseHelper.Parse<double>(jsonData["SetupCharge"]);
            double  EtestCharge=ParseHelper.Parse<double>(jsonData["EtestCharge"]);
            double  ToolingCharge=ParseHelper.Parse<double>(jsonData["ToolingCharge"]);
         //   double  ShipTermsAdder=ParseHelper.Parse<double>(jsonData["ShipTermsAdder"]);
            double  MOV=ParseHelper.Parse<double>(jsonData["MOV"]);
            double  TargetPrice=ParseHelper.Parse<double>(jsonData["TargetPrice"]);
            double  AssemblyCost=ParseHelper.Parse<double>(jsonData["AssemblyCost"]);
            double  PCBFPCPrice1=ParseHelper.Parse<double>(jsonData["PCBFPCPrice1"]);
            double  PCBFPCPrice2=ParseHelper.Parse<double>(jsonData["PCBFPCPrice2"]);
            double  PCBFPCPrice3=ParseHelper.Parse<double>(jsonData["PCBFPCPrice3"]);
            double  PCBFPCPrice4=ParseHelper.Parse<double>(jsonData["PCBFPCPrice4"]);
            double  PCBFPCPrice5=ParseHelper.Parse<double>(jsonData["PCBFPCPrice5"]);
            double  SMTBOMCost=ParseHelper.Parse<double>(jsonData["SMTBOMCost"]);
            double  BOMMarkup1=ParseHelper.Parse<double>(jsonData["BOMMarkup1"]);
            double  BOMMarkup2=ParseHelper.Parse<double>(jsonData["BOMMarkup2"]);
            double  BOMMarkup3=ParseHelper.Parse<double>(jsonData["BOMMarkup3"]);
            double  BOMMarkup4=ParseHelper.Parse<double>(jsonData["BOMMarkup4"]);
            double  BOMMarkup5=ParseHelper.Parse<double>(jsonData["BOMMarkup5"]);
            double  AssemblyMarkup1=ParseHelper.Parse<double>(jsonData["AssemblyMarkup1"]);
            double  AssemblyMarkup2=ParseHelper.Parse<double>(jsonData["AssemblyMarkup2"]);
            double  AssemblyMarkup3=ParseHelper.Parse<double>(jsonData["AssemblyMarkup3"]);
            double  AssemblyMarkup4=ParseHelper.Parse<double>(jsonData["AssemblyMarkup4"]);
            double  AssemblyMarkup5=ParseHelper.Parse<double>(jsonData["AssemblyMarkup5"]);
            string  UnitType=jsonData["UnitType"].ToString();
            double  UnitSizeWidth=ParseHelper.Parse<double>(jsonData["UnitSizeWidth"]);
            double  UnitSizeLength=ParseHelper.Parse<double>(jsonData["UnitSizeLength"]);
            double UnitArea = ParseHelper.Parse<double>(jsonData["UnitArea"]);
            double ArrayPerWorkingPanel = ParseHelper.Parse<double>(jsonData["ArrayPerWorkingPanel"]);
            double  UnitPerArray=ParseHelper.Parse<double>(jsonData["UnitPerArray"]);
            double  ArraySizeWidth=ParseHelper.Parse<double>(jsonData["ArraySizeWidth"]);
            double  ArraySizeLength=ParseHelper.Parse<double>(jsonData["ArraySizeLength"]);
          
            double  PanelSizeWidth=ParseHelper.Parse<double>(jsonData["PanelSizeWidth"]);
            double  PanelSizeLength=ParseHelper.Parse<double>(jsonData["PanelSizeLength"]);
            double  BoardThickness=ParseHelper.Parse<double>(jsonData["BoardThickness"]);
            double  CompetitiveWinPrice1=ParseHelper.Parse<double>(jsonData["CompetitiveWinPrice1"]);
            double  CompetitiveWinPrice2=ParseHelper.Parse<double>(jsonData["CompetitiveWinPrice2"]);
            double  Yield=ParseHelper.Parse<double>(jsonData["Yield"]);
            double  MaterialCost=ParseHelper.Parse<double>(jsonData["MaterialCost"]);
            double  VariableCost=ParseHelper.Parse<double>(jsonData["VariableCost"]);
            double  FixedCost=ParseHelper.Parse<double>(jsonData["FixedCost"]);
            string PriceType1 = jsonData["PriceType1"].ToString();
            string PriceType2 = jsonData["PriceType2"].ToString();
            string PriceType3 = jsonData["PriceType3"].ToString();
            string PriceType4 = jsonData["PriceType4"].ToString();
            string PriceType5 = jsonData["PriceType5"].ToString();
            double  LayerCount=ParseHelper.Parse<double>(jsonData["LayerCount"]);
            string  UnitOrArray=jsonData["UnitOrArray"].ToString();
            double  Holes=ParseHelper.Parse<double>(jsonData["Holes"]);
            double SmallestHole = ParseHelper.Parse<double>(jsonData["SmallestHole"]);

            double OutlineTool1_1 = ParseHelper.Parse<double>(jsonData["OutlineTool1_1"]);
            double OutlineTool2_1 = ParseHelper.Parse<double>(jsonData["OutlineTool2_1"]);
            double TopCoverlayTool_1 = ParseHelper.Parse<double>(jsonData["TopCoverlayTool_1"]);
            double BottomCoverlayTool_1 = ParseHelper.Parse<double>(jsonData["BottomCoverlayTool_1"]);
            double ETestCADArtworkNRE_1 = ParseHelper.Parse<double>(jsonData["ETestCADArtworkNRE_1"]);
            double OutlineTool1_2 = ParseHelper.Parse<double>(jsonData["OutlineTool1_2"]);
            double OutlineTool2_2 = ParseHelper.Parse<double>(jsonData["OutlineTool2_2"]);
            double TopCoverlayTool_2 = ParseHelper.Parse<double>(jsonData["TopCoverlayTool_2"]);
            double BottomCoverlayTool_2 = ParseHelper.Parse<double>(jsonData["BottomCoverlayTool_2"]);
            double ETestCADArtworkNRE_2 = ParseHelper.Parse<double>(jsonData["ETestCADArtworkNRE_2"]);
            double OutlineTool1_3 = ParseHelper.Parse<double>(jsonData["OutlineTool1_3"]);
            double OutlineTool2_3 = ParseHelper.Parse<double>(jsonData["OutlineTool2_3"]);
            double TopCoverlayTool_3 = ParseHelper.Parse<double>(jsonData["TopCoverlayTool_3"]);
            double BottomCoverlayTool_3 = ParseHelper.Parse<double>(jsonData["BottomCoverlayTool_3"]);
            double ETestCADArtworkNRE_3 = ParseHelper.Parse<double>(jsonData["ETestCADArtworkNRE_3"]);
            double OutlineTool1_4 = ParseHelper.Parse<double>(jsonData["OutlineTool1_4"]);
            double OutlineTool2_4 = ParseHelper.Parse<double>(jsonData["OutlineTool2_4"]);
            double TopCoverlayTool_4 = ParseHelper.Parse<double>(jsonData["TopCoverlayTool_4"]);
            double BottomCoverlayTool_4 = ParseHelper.Parse<double>(jsonData["BottomCoverlayTool_4"]);
            double ETestCADArtworkNRE_4 = ParseHelper.Parse<double>(jsonData["ETestCADArtworkNRE_4"]);


            FPCRFQDetail detail = new FPCRFQDetail();
         
            detail.VolumePerMonth = VolumePerMonth;
            detail.DueDate = DueDate;
            detail.ExpectedLifeTime = ExpectedLifeTime;
            detail.EstQuoteSize = EstQuoteSize;
           // detail.MinComputedSize = MinComputedSize;
           // detail.Currency = Currency;
          //  detail.ExchangRatePerUSD = ExchangRatePerUSD;
            detail.SetupCharge = SetupCharge;
            detail.EtestCharge = EtestCharge;
            detail.ToolingCharge = ToolingCharge;
           // detail.ShipTermsAdder = ShipTermsAdder;
            detail.MOV = MOV;
            detail.TargetPrice = TargetPrice;
            detail.AssemblyCost = AssemblyCost;
            detail.PCBFPCPrice1 = PCBFPCPrice1;
            detail.PCBFPCPrice2 = PCBFPCPrice2;
            detail.PCBFPCPrice3 = PCBFPCPrice3;
            detail.PCBFPCPrice4 = PCBFPCPrice4;
            detail.PCBFPCPrice5 = PCBFPCPrice5;
            detail.SMTBOMCost = SMTBOMCost;
            detail.BOMMarkup1 = BOMMarkup1;
            detail.BOMMarkup2 = BOMMarkup2;
            detail.BOMMarkup3 = BOMMarkup3;
            detail.BOMMarkup4 = BOMMarkup4;
            detail.BOMMarkup5 = BOMMarkup5;
            detail.AssemblyMarkup1 = AssemblyMarkup1;
            detail.AssemblyMarkup2 = AssemblyMarkup2;
            detail.AssemblyMarkup3 = AssemblyMarkup3;
            detail.AssemblyMarkup4 = AssemblyMarkup4;
            detail.AssemblyMarkup5 = AssemblyMarkup5;
            detail.UnitType = UnitType;
            detail.UnitSizeWidth = UnitSizeWidth;
            detail.UnitSizeLength = UnitSizeLength;
            detail.UnitPerArray = UnitPerArray;
            detail.UnitArea = UnitArea;
            detail.ArrayPerWorkingPanel = ArrayPerWorkingPanel;
            detail.ArraySizeWidth = ArraySizeWidth;
            detail.ArraySizeLength = ArraySizeLength;
        
            detail.PanelSizeWidth = PanelSizeWidth;
            detail.PanelSizeLength = PanelSizeLength;
            detail.BoardThickness = BoardThickness;
            detail.CompetitiveWinPrice1 = CompetitiveWinPrice1;
            detail.CompetitiveWinPrice2 = CompetitiveWinPrice2;
            detail.Yield = Yield;
            detail.MaterialCost = MaterialCost;
            detail.VariableCost = VariableCost;
            detail.FixedCost = FixedCost;
            detail.PriceType1 = PriceType1;
            detail.PriceType2 = PriceType2;
            detail.PriceType3 = PriceType3;
            detail.PriceType4 = PriceType4;
            detail.PriceType5 = PriceType5;
            detail.LayerCount = LayerCount;
            detail.UnitOrArray = UnitOrArray;
            detail.Holes = Holes;
            detail.SmallestHole = SmallestHole;
            detail.OutlineTool1_1 = OutlineTool1_1;
            detail.OutlineTool2_1 = OutlineTool2_1;
            detail.TopCoverlayTool_1 = TopCoverlayTool_1;
            detail.BottomCoverlayTool_1 = BottomCoverlayTool_1;
            detail.ETestCADArtworkNRE_1 = ETestCADArtworkNRE_1;
            detail.OutlineTool1_2 = OutlineTool1_2;
            detail.OutlineTool2_2 = OutlineTool2_2;
            detail.TopCoverlayTool_2 = TopCoverlayTool_2;
            detail.BottomCoverlayTool_2 = BottomCoverlayTool_2;
            detail.ETestCADArtworkNRE_2 = ETestCADArtworkNRE_2;
            detail.OutlineTool1_3 = OutlineTool1_3;
            detail.OutlineTool2_3 = OutlineTool2_3;
            detail.TopCoverlayTool_3 = TopCoverlayTool_3;
            detail.BottomCoverlayTool_3 = BottomCoverlayTool_3;
            detail.ETestCADArtworkNRE_3 = ETestCADArtworkNRE_3;
            detail.OutlineTool1_4 = OutlineTool1_4;
            detail.OutlineTool2_4 = OutlineTool2_4;
            detail.TopCoverlayTool_4 = TopCoverlayTool_4;
            detail.BottomCoverlayTool_4 = BottomCoverlayTool_4;
            detail.ETestCADArtworkNRE_4 = ETestCADArtworkNRE_4;

            double MOQ = Math.Round(detail.MOQ,4);
            double TargetPrice1 = Math.Round(detail.TargetPrice1,4);
            double TargetASP = Math.Round(detail.TargetASP,4);
            double MinASP = Math.Round(detail.MinASP,4);
            double TargetSqIn = Math.Round(detail.TargetSqIn,4);
            double MinSqInch = Math.Round(detail.MinSqInch,4);
            double TargetCLsqin = Math.Round(detail.TargetCLsqin,4);
            double MinCLsqin = Math.Round(detail.MinCLsqin,4);
            double TargetVSActucal = Math.Round(detail.TargetVSActucal,4);
            double BOMCost1 = Math.Round(detail.BOMCost1,4);
            double BOMCost2 = Math.Round(detail.BOMCost2,4);
            double BOMCost3 = Math.Round(detail.BOMCost3,4);
            double BOMCost4 = Math.Round(detail.BOMCost4,4);
            double BOMCost5 = Math.Round(detail.BOMCost5,4);
            double BOMPrice1 = Math.Round(detail.BOMPrice1,4);
            double BOMPrice2 = Math.Round(detail.BOMPrice2,4);
            double BOMPrice3 = Math.Round(detail.BOMPrice3,4);
            double BOMPrice4 = Math.Round(detail.BOMPrice4,4);
            double BOMPrice5 = Math.Round(detail.BOMPrice5,4);
            double AssemblyCost1 = Math.Round(detail.AssemblyCost1,4);
            double AssemblyCost2 = Math.Round(detail.AssemblyCost2,4);
            double AssemblyCost3 = Math.Round(detail.AssemblyCost3,4);
            double AssemblyCost4 = Math.Round(detail.AssemblyCost4,4);
            double AssemblyCost5 = Math.Round(detail.AssemblyCost5,4);
            double AssemblyPrice1 = Math.Round(detail.AssemblyPrice1,4);
            double AssemblyPrice2 = Math.Round(detail.AssemblyPrice2,4);
            double AssemblyPrice3 = Math.Round(detail.AssemblyPrice3,4);
            double AssemblyPrice4 = Math.Round(detail.AssemblyPrice4,4);
            double AssemblyPrice5 = Math.Round(detail.AssemblyPrice5,4);
            double TotalPrice1 = Math.Round(detail.TotalPrice1,4);
            double TotalPrice2 = Math.Round(detail.TotalPrice2,4);
            double TotalPrice3 = Math.Round(detail.TotalPrice3,4);
            double TotalPrice4 = Math.Round(detail.TotalPrice4,4);
            double TotalPrice5 = Math.Round(detail.TotalPrice5,4);
            string PanelUtilization =detail.PanelUtilization;
            double TotalCost = Math.Round(detail.TotalCost,4);
            double OP = Math.Round(detail.OP,4);
            double OP1 = Math.Round(detail.OP1,4);
            double OP2 = Math.Round(detail.OP2,4);
            double OP3 = Math.Round(detail.OP3,4);
            double OP4 = Math.Round(detail.OP4,4);
            double OP5 = Math.Round(detail.OP5,4);
            double MP = Math.Round(detail.MP,4);
            double MP1 = Math.Round(detail.MP1,4);
            double MP2 = Math.Round(detail.MP2,4);
            double MP3 = Math.Round(detail.MP3,4);
            double MP4 = Math.Round(detail.MP4,4);
            double MP5 = Math.Round(detail.MP5, 4);
            double Total_1 = Math.Round(detail.Total_1, 4);
            double Total_2 = Math.Round(detail.Total_2, 4);
            double Total_3 = Math.Round(detail.Total_3, 4);
            double Total_4 = Math.Round(detail.Total_4, 4);
            double UnitPerWorkingPanel = Math.Round(detail.UnitPerWorkingPanel,4);

            var jsonObject = new
            {

                MOQ = MOQ,
                TargetPrice1 = TargetPrice1,
                TargetASP = TargetASP,
                MinASP = MinASP,
                TargetSqIn = TargetSqIn,
                MinSqInch = MinSqInch,
                TargetCLsqin = TargetCLsqin,
                MinCLsqin = MinCLsqin,
                TargetVSActucal = TargetVSActucal,
                BOMCost1 = BOMCost1,
                BOMCost2 = BOMCost2,
                BOMCost3 = BOMCost3,
                BOMCost4 = BOMCost4,
                BOMCost5 = BOMCost5,
                BOMPrice1 = BOMPrice1,
                BOMPrice2 = BOMPrice2,
                BOMPrice3 = BOMPrice3,
                BOMPrice4 = BOMPrice4,
                BOMPrice5 = BOMPrice5,
                AssemblyCost1 = AssemblyCost1,
                AssemblyCost2 = AssemblyCost2,
                AssemblyCost3 = AssemblyCost3,
                AssemblyCost4 = AssemblyCost4,
                AssemblyCost5 = AssemblyCost5,
                AssemblyPrice1 = AssemblyPrice1,
                AssemblyPrice2 = AssemblyPrice2,
                AssemblyPrice3 = AssemblyPrice3,
                AssemblyPrice4 = AssemblyPrice4,
                AssemblyPrice5 = AssemblyPrice5,
                TotalPrice1 = TotalPrice1,
                TotalPrice2 = TotalPrice2,
                TotalPrice3 = TotalPrice3,
                TotalPrice4 = TotalPrice4,
                TotalPrice5 = TotalPrice5,
                PanelUtilization = PanelUtilization,
                TotalCost = TotalCost,
                OP = OP,
                OP1 = OP1,
                OP2 = OP2,
                OP3 = OP3,
                OP4 = OP4,
                OP5 = OP5,
                MP = MP,
                MP1 = MP1,
                MP2 = MP2,
                MP3 = MP3,
                MP4 = MP4,
                MP5 = MP5,
                Total_1=Total_1,
                Total_2=Total_2,
                Total_3=Total_3,
                Total_4=Total_4,
                UnitPerWorkingPanel = UnitPerWorkingPanel
            };
            return Json(jsonObject, JsonRequestBehavior.AllowGet);
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
                    B2FPDFDownLoad pdf = new B2FPDFDownLoad();

                    if (pdf.WriterPDF(ref mem, id, out fileName))
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
                    ZipHelper.B2FPDFToZip(ids, mem);
                    fileName = "PDF_Packages_" + DateTime.Now.ToString("mmss") + ".zip";
                    using (var fileStream = FileHelper.CreateFile(tempFile))
                    {
                        mem.WriteTo(fileStream);
                    }
                }
            }

            return File(tempFile, "application/octet-stream", fileName);
        }
    }
}
