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
using BI.SGP.BLL.CostEngine;
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class CostIOController : Controller
    {
        public ActionResult Input(string ID)
        {
            int dataId = ParseHelper.Parse<int>(ID);
            int RFQId = ParseHelper.Parse<int>(Request["RFQID"]);
            string RFQNumber = "";
            if (RFQId > 0)
            {
                string strSql = "SELECT Number FROM SGP_RFQ WHERE ID = @RFQID";
                RFQNumber = Convert.ToString(DbHelperSQL.GetSingle(strSql, new SqlParameter("@RFQID", RFQId)));

                if (!String.IsNullOrEmpty(RFQNumber))
                {
                    strSql = "SELECT ID, Status FROM V_CostingView WHERE RFQNumber = @RFQNumber";
                    DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQNumber", RFQNumber)).Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        int status = ParseHelper.Parse<int>(dt.Rows[0]["Status"]);
                        dataId = ParseHelper.Parse<int>(dt.Rows[0]["ID"]);

                        if (status == 2)
                        {
                            return RedirectToAction("Output", new { ID = dataId });
                        }
                    }
                }
            }

            ViewBag.DataId = dataId;
            ViewBag.RFQID = RFQId;
            ViewBag.RFQNumber = RFQNumber;
            return View();
        }

        public ActionResult Output(string ID)
        {
            int dataId = ParseHelper.Parse<int>(ID);
            ViewBag.DataId = dataId;
            return View();
        }

        public ActionResult List()
        {
            return View();
        }

        private int Save(string postData, SystemMessages sysMsg, bool checkRequired)
        {
            int id = 0;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string commonCategoryName = Convert.ToString(jsonData["commonCategoryName"]);
                string basicInfoCategoryName = Convert.ToString(jsonData["basicInfoCategoryName"]);
                string processFlowCategoryName = Convert.ToString(jsonData["processFlowCategoryName"]);
                string bomCategoryName = Convert.ToString(jsonData["bomCategoryName"]);
                string edmCategoryName = Convert.ToString(jsonData["edmCategoryName"]);
                string specialCategoryName = Convert.ToString(jsonData["specialCategoryName"]);

                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
                Dictionary<string, object> yieldData = jsonData["yieldData"] as Dictionary<string, object>;

                CostingInputDetail ciDetail = new CostingInputDetail();

                ciDetail.CommonCategory = new FieldCategory(commonCategoryName);
                ciDetail.BasicInfoCategory = new FieldCategory(basicInfoCategoryName);
                ciDetail.ProcessFlowCategory = new FieldCategory(processFlowCategoryName);
                ciDetail.BOMCategory = new FieldCategory(bomCategoryName);
                ciDetail.EDMCategories = FieldCategory.GetCategoryByName(edmCategoryName);
                ciDetail.SpecialCategory = FieldCategory.GetCategoryByName(specialCategoryName);
                ciDetail.YieldData = yieldData;
                ciDetail.FillData(data);
                ciDetail.CheckDataType(sysMsg);

                if (checkRequired)
                {
                    ciDetail.CheckRequired(sysMsg);
                }

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            if (id > 0)
                            {
                                ciDetail.Update(id);
                            }
                            else
                            {
                                id = ciDetail.Add();
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

        private int Clone(string postData, SystemMessages sysMsg)
        {
            int id = 0;
            if (!String.IsNullOrWhiteSpace(postData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> jsonData = jss.Deserialize<Dictionary<string, object>>(postData) as Dictionary<string, object>;

                string dataId = Convert.ToString(jsonData["dataId"]);
                string commonCategoryName = Convert.ToString(jsonData["commonCategoryName"]);
                string basicInfoCategoryName = Convert.ToString(jsonData["basicInfoCategoryName"]);
                string processFlowCategoryName = Convert.ToString(jsonData["processFlowCategoryName"]);
                string bomCategoryName = Convert.ToString(jsonData["bomCategoryName"]);
                string edmCategoryName = Convert.ToString(jsonData["edmCategoryName"]);
                string specialCategoryName = Convert.ToString(jsonData["specialCategoryName"]);

                Int32.TryParse(dataId, out id);
                Dictionary<string, object> data = jsonData["data"] as Dictionary<string, object>;
                Dictionary<string, object> yieldData = jsonData["yieldData"] as Dictionary<string, object>;

                CostingInputDetail ciDetail = new CostingInputDetail();

                ciDetail.CommonCategory = new FieldCategory(commonCategoryName);
                ciDetail.BasicInfoCategory = new FieldCategory(basicInfoCategoryName);
                ciDetail.ProcessFlowCategory = new FieldCategory(processFlowCategoryName);
                ciDetail.BOMCategory = new FieldCategory(bomCategoryName);
                ciDetail.EDMCategories = FieldCategory.GetCategoryByName(edmCategoryName);
                ciDetail.SpecialCategory = FieldCategory.GetCategoryByName(specialCategoryName);
                ciDetail.YieldData = yieldData;
                ciDetail.FillData(data);
                ciDetail.CheckDataType(sysMsg);

                if (sysMsg.isPass)
                {
                    using (TScope ts = new TScope())
                    {
                        try
                        {
                            id = ciDetail.Add();
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

            int id = 0;
            string postData = Request["postData"];
            id = Save(postData, sysMsg, false);
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg
            };
            return Json(jsonResult);
        }

        public ActionResult Clone()
        {
            SystemMessages sysMsg = new SystemMessages();

            int id = 0;
            string postData = Request["postData"];
            id = Clone(postData, sysMsg);
            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg
            };
            return Json(jsonResult);
        }

        public ActionResult SubmitToRFQ()
        {
            SystemMessages sysMsg = new SystemMessages();

            int id = ParseHelper.Parse<int>(Request["dataId"]);
            id = CostingInputDetail.SubmitToRFQ(id, sysMsg);
            var jsonResult = new
            {
                RFQID = id,
                SysMsg = sysMsg
            };
            return Json(jsonResult);
        }

        public ActionResult DelData()
        {
            string errMessage = "";
            int id = ParseHelper.Parse<int>(Request["id"]);

            using (TScope ts = new TScope())
            {
                try
                {
                    CostingInputDetail.Delete(id);
                }
                catch (Exception ex)
                {
                    ts.Rollback();
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

        public ActionResult GenerateCosting()
        {
            SystemMessages sysMsg = new SystemMessages();

            int id = 0;
            string postData = Request["postData"];
            id = Save(postData, sysMsg, true);

            if (sysMsg.isPass && id > 0)
            {
                CostHelper ch = new CostHelper();
                using (TScope ts = new TScope())
                {
                    try
                    {
                        ch.GenerateCosting(id);
                    }
                    catch (Exception ex)
                    {
                        ts.Rollback();
                        sysMsg.isPass = false;
                        sysMsg.Messages.Add("Error", ex.Message);
                    }
                }
            }

            var jsonResult = new
            {
                DataId = id,
                SysMsg = sysMsg
            };
            return Json(jsonResult);
        }

        public JsonResult GenrateCategory()
        {
            string html = "";
            string errMessage = "";
            int id = 0;
            string dataId = Request["dataId"];
            string categoryName = Request["categoryName"];
            Int32.TryParse(dataId, out id);

            try
            {
                FieldCategory category = new FieldCategory(categoryName);

                if (id > 0)
                {
                    CostingInputDetail ciDetail = new CostingInputDetail();
                    ciDetail.FillMainCategoryData(category, id);
                }

                html = BI.SGP.BLL.UIManager.CostingMasterDataDetailHelper.GenrateCategory(category);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                html = html,
                errMessage = errMessage
            };
            return Json(result);
        }

        public JsonResult GenrateYieldCategory()
        {
            string html = "";
            string errMessage = "";
            int id = ParseHelper.Parse<int>(Request["dataId"]);
            string pcbType = Request["pcbType"];
            string version = Request["version"];

            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();

                if (id > 0)
                {
                    CostingInputDetail ciDetail = new CostingInputDetail();
                    ciDetail.FillYieldData(data, id);
                }
                html = BI.SGP.BLL.UIManager.CostingMasterDataDetailHelper.GenrateYieldCategory(pcbType, version, data);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                html = html,
                errMessage = errMessage
            };
            return Json(result);
        }

        public JsonResult GenrateSubCategories()
        {
            string html = "";
            string errMessage = "";
            string dataId = Request["dataId"];
            string categoriesName = Request["categoryName"];
            string canEditCategory = Request["canEditCategory"];
            string headEditCategory = Request["headEditCategory"];

            int id = ParseHelper.Parse<int>(dataId);

            try
            {
                string[] cns = categoriesName.Split(',');
                foreach (string cn in cns)
                {
                    FieldCategory category = new FieldCategory(cn.Trim());
                    if (id > 0)
                    {
                        CostingInputDetail ciDetail = new CostingInputDetail();
                        ciDetail.FillSubCategoryData(category, id);
                    }
                    html += BI.SGP.BLL.UIManager.CostingMasterDataDetailHelper.GenrateSubCategory(category, cns.Length > 1, canEditCategory.IndexOf(cn) != -1, headEditCategory.IndexOf(cn) != -1);
                }
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                html = html,
                errMessage = errMessage
            };
            return Json(result);
        }

        public string GetTableKeyByCostItem()
        {
            string costItem = Request["costItem"];
            Dictionary<string, CostItem> CostItemMap = CostItemFactory.CostItemMap;

            if(CostItemMap.ContainsKey(costItem)) {
                return CostItemMap[costItem].TableKey;
            }
            return "";
        }

        public string GetPanelSize()
        {
            string strSql = "SELECT PanelSize,PanelArea FROM SCM_PanelSize WHERE Plant=@Plant";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Plant", Request["plant"])).Tables[0];
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }

        public string GetPanelArea()
        {
            string strSql = "SELECT PanelArea FROM SCM_PanelSize WHERE Plant=@Plant AND PanelSize=@PanelSize";
            return Convert.ToString(DbHelperSQL.GetSingle(strSql, new SqlParameter("@Plant", Request["plant"]), new SqlParameter("@PanelSize", Request["panelSize"])));
        }

        public string GetProductType()
        {
            string strSql = "SELECT ProductType FROM SCM_ProductType WHERE Category=@Category AND LayerCount=@LayerCount AND Type=@Type";
            return Convert.ToString(DbHelperSQL.GetSingle(strSql, new SqlParameter("@Category", Request["category"]), new SqlParameter("@LayerCount", Request["layerCount"]), new SqlParameter("@Type", Request["pcbType"])));
        }

        public string GetProcFlowByProdType()
        {
            string strSql = "SELECT t1.WorkCenter,t1.Layer,t2.DescChinese,t2.DescEnglish FROM SCM_StandardProcess t1 LEFT JOIN SCM_SubWorkCenter t2 ON t1.Plant=t2.Plant AND t1.WorkCenter=t2.Name WHERE t1.Plant=@Plant AND t1.Technology=@Technology ORDER BY t1.Step,t1.ID";
            DataTable dtFlow = DbHelperSQL.Query(strSql, new SqlParameter("@Plant", Request["plant"]), new SqlParameter("@Technology", Request["prodType"])).Tables[0];
            if (dtFlow.Rows.Count == 0)
            {
                strSql = "SELECT t1.WorkCenter,t1.Layer,t2.DescChinese,t2.DescEnglish FROM SCM_StandardProcess t1 LEFT JOIN SCM_SubWorkCenter t2 ON t1.Plant=t2.Plant AND t1.WorkCenter=t2.Name WHERE t1.Plant=@Plant AND t1.Technology=@Technology ORDER BY t1.Step,t1.ID";
                dtFlow = DbHelperSQL.Query(strSql, new SqlParameter("@Plant", Request["plant"]), new SqlParameter("@Technology", Request["viaStructure"])).Tables[0];
            }

            strSql = "SELECT Layer,Times FROM SCD_ProductStdProc WHERE Version=@Version AND ProductType=@ProductType";
            DataTable dtTimes = DbHelperSQL.Query(strSql, new SqlParameter("@Version", Request["version"]), new SqlParameter("@ProductType", Request["prodType"])).Tables[0];

            var data = new
            {
                flow = dtFlow,
                times = dtTimes
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public string GetWCDesc()
        {
            string strSql = "SELECT DescEnglish, DescChinese FROM SCM_MainWorkCenter WHERE Name=@Name AND Plant=@Plant UNION SELECT DescEnglish, DescChinese FROM SCM_SubWorkCenter WHERE Name=@Name AND Plant=@Plant";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Name", Request["wc"]), new SqlParameter("@Plant", Request["plant"])).Tables[0];
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }

        public string GetMainWC()
        {
            string strSql = "SELECT MainWorkCenter FROM SCM_SubWorkCenter WHERE Plant=@Plant and Name=@Name";
            string mwc = Convert.ToString(DbHelperSQL.GetSingle(strSql, new SqlParameter("@Plant", Request["plant"]), new SqlParameter("@Name", Request["wc"])));

            return String.IsNullOrEmpty(mwc) ? Request["wc"]: mwc;
        }

        public string GetWCMapping()
        {
            string wcData = Request["wcData"];
            SystemMessages sysMsg = new SystemMessages();

            if (!String.IsNullOrEmpty(wcData))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, string>> jsonDataList = jss.Deserialize<List<Dictionary<string, string>>>(wcData);
                for (int i = 0; i < jsonDataList.Count; i++)
                {
                    foreach(KeyValuePair<string, string> kv in jsonDataList[i]) 
                    {
                        if(String.IsNullOrWhiteSpace(kv.Value)) 
                        {
                            sysMsg.isPass = false;
                            sysMsg.Messages.Add("Work Center Check", String.Format("\"(line:{0})Work Center\" is required.", i + 1));
                        }
                        else
                        {
                            if (!CostingInputDetail.WorkCenterExists(kv.Value, kv.Key))
                            {
                                sysMsg.isPass = false;
                                sysMsg.Messages.Add("Work Center Check", String.Format("\"(line:{0})Can not find [{1}] in [{2}].", i + 1, kv.Value, kv.Key));
                            }
                        }
                    }
                }
            }

            DataTable dt = null;
            if (sysMsg.isPass)
            {
                string strSql = "SELECT t1.MainWorkCenter, t2.Name AS SubWorkCenter, t1.Material, t1.Category, t1.ExceptWC, t2.Plant FROM SCM_MaterialMapping t1 LEFT JOIN SCM_SubWorkCenter t2 ON t1.MainWorkCenter = t2.MainWorkCenter WHERE t1.TYPE=@Type";
                dt = DbHelperSQL.Query(strSql, new SqlParameter("@Type", Request["pcbType"])).Tables[0];
            }

            var jsonResult = new
            {
                data = dt,
                sysMsg = sysMsg
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult);
        }

        public string GetWCBOMMapping()
        {
            string strSql = "SELECT Material FROM SCM_MaterialMapping WHERE TYPE=@Type AND MainWorkCenter=@WorkCenter";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Type", Request["pcbType"]), new SqlParameter("@WorkCenter", Request["workCenter"])).Tables[0];
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }

        public string GetRFQInfo()
        {
            string strSql = "SELECT OEM,CEM,CustomerPartNumber,Revision,Application,VolumePerMonth FROM V_SGP WHERE Number = @Number";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@Number", Request["rfqNumber"])).Tables[0];
            if (dt.Rows.Count == 0)
            {
                strSql = "SELECT OEM,CEM,CustomerPartNumber,Revision,Application,VolumePerMonth FROM V_SGPForFPC WHERE Number = @Number";
                dt = DbHelperSQL.Query(strSql, new SqlParameter("@Number", Request["rfqNumber"])).Tables[0];
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }

        public string GetProcGroup()
        {
            string strSql = "SELECT * FROM SCM_ProcFlowGroup WHERE ProcGroup=@ProcGroup ORDER BY Sort,ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@ProcGroup", Request["procGroup"])).Tables[0];
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }

        public ActionResult GetProcBDFields()
        {
            JsonResult json = new JsonResult();
            json.Data = CostingOutputDetail.GetProcBDFields();
            return json;
        }

        public string GetProcBDData()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(CostingOutputDetail.GetProcBDData(ParseHelper.Parse<int>(Request["dataId"])));
        }

        public string GetBOMRptData()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(CostingOutputDetail.GetBOMRptData(ParseHelper.Parse<int>(Request["dataId"])));
        }

        public JsonResult GenrateCostBreakdownCategory()
        {
            object html = "";
            string errMessage = "";

            try
            {
                html = CostingOutputDetail.GenrateCostSummaryAndBreakdown(ParseHelper.Parse<int>(Request["dataId"]));
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            var result = new
            {
                success = (errMessage == "" ? true : false),
                html = html,
                errMessage = errMessage
            };
            return Json(result);
        }

        public FileResult DownOutputExcel(string ID)
        {
            int id = ParseHelper.Parse<int>(ID);
            if (id > 0)
            {
                Dictionary<string, string> dicProcBDBOM = new Dictionary<string, string>();
                GridColumns gc = CostingOutputDetail.GetProcBDFields();

                for (int i = 0; i < gc.colNames.Count; i++)
                {
                    dicProcBDBOM.Add(gc.colModel[i].name, gc.colNames[i]);
                }

                Dictionary<string, string> dicBOM = new Dictionary<string, string>{
                    {"Layer","Layer"},
                    {"Step","Step"},
                    {"Material","Material"},
                    {"WorkCenter","Work Center"},
                    {"MainWorkCenter","Main Work Center"},
                    {"LayupQtyPanel","Consumption Qty Per PNL"},
                    {"Unit","Unit"},
                    {"ManualPrice","Price(USD)"},
                    {"CostValue","Price Cost"}
                };

                DataTable procBDData = CostingOutputDetail.GetProcBDData(id);
                procBDData.TableName = "Process Breakdown";

                for (int i = procBDData.Columns.Count - 1; i >= 0; i--)
                {
                    if (dicProcBDBOM.ContainsKey(procBDData.Columns[i].ColumnName))
                    {
                        string cname = dicProcBDBOM[procBDData.Columns[i].ColumnName];
                        if (procBDData.Columns[i].ColumnName != cname)
                        {
                            if (procBDData.Columns.Contains(cname))
                            {
                                cname = cname + "_";
                            }
                            procBDData.Columns[i].ColumnName = cname;
                        }
                    }
                    else
                    {
                        procBDData.Columns.RemoveAt(i);
                    }
                }

                DataTable bomRptData = CostingOutputDetail.GetBOMRptData(id);
                bomRptData.TableName = "BOM Report";

                for (int i = bomRptData.Columns.Count - 1; i >= 0; i--)
                {
                    if (dicBOM.ContainsKey(bomRptData.Columns[i].ColumnName))
                    {
                        bomRptData.Columns[i].ColumnName = dicBOM[bomRptData.Columns[i].ColumnName];
                    }
                    else
                    {
                        bomRptData.Columns.RemoveAt(i);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(procBDData.Copy());
                ds.Tables.Add(bomRptData.Copy());

                string tempFile = ExcelHelper.DataSetToExcel(ds);

                return File(tempFile, "application/ms-excel", Guid.NewGuid().ToString() + ".xlsx");
            }
            else
            {
                return null;
            }
        }
    }
}
