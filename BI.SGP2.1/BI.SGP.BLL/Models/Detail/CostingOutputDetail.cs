using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Models.Detail
{
    public class CostingOutputDetail
    {
        public static GridColumns GetProcBDFields()
        {
            List<string> colNames = new List<string>();
            List<GridColumnModel> colModels = new List<GridColumnModel>();

            colNames.Add("Layer");
            colNames.Add("WorkCenter");
            colNames.Add("Step");
            colNames.Add("Main WC");

            GridColumnModel model = new GridColumnModel();
            model.name = "Layer";
            model.index = "Layer";
            model.width = 80;
            model.align = "center";
            colModels.Add(model);

            model = new GridColumnModel();
            model.name = "WorkCenter";
            model.index = "WorkCenter";
            model.width = 100;
            model.align = "center";
            colModels.Add(model);

            model = new GridColumnModel();
            model.name = "Step";
            model.index = "Step";
            model.width = 80;
            model.align = "center";
            colModels.Add(model);

            model = new GridColumnModel();
            model.name = "MainWorkCenter";
            model.index = "MainWorkCenter";
            model.width = 80;
            model.align = "center";
            colModels.Add(model);

            string strSql = "SELECT CostItem,ISNULL(CostSubItem,DisplayName) DisplayName FROM SCS_CostItem t1 LEFT JOIN SCS_CostRateItem t2 ON t1.CostItem=t2.CostKey ORDER BY t1.Sort";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                string displayName = Convert.ToString(dr["DisplayName"]);
                string costItem = Convert.ToString(dr["CostItem"]);
                colNames.Add(displayName);
                model = new GridColumnModel();
                model.name = costItem;
                model.index = costItem;
                model.width = 100;
                model.align = "center";
                colModels.Add(model);
            }

            colNames.Add("Cycle Time");
            model = new GridColumnModel();
            model.name = "TotalCycleTime";
            model.index = "TotalCycleTime";
            model.width = 80;
            model.align = "center";
            colModels.Add(model);

            return new GridColumns
            {
                colNames = colNames,
                colModel = colModels
            };
        }

        public static DataTable GetProcBDData(int dataId)
        {
            string strSql = "SELECT CostItem FROM SCS_CostItem ORDER BY Sort";
            string strFields = "";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                strFields += dr["CostItem"] + ",";
            }

            strFields = strFields.TrimEnd(',');

            strSql = String.Format(@"SELECT t1.Layer,t1.MainWorkCenter,t1.WorkCenter,t1.Step,t2.*,(SELECT SUM(TotalCycleTime) FROM SCI_EDM WHERE PFID=t1.ID AND SCIID=t1.SCIID) AS TotalCycleTime FROM (
                            SELECT SCIID,ID,MainWorkCenter,WorkCenter,Layer,Step FROM SCI_ProcessFlow WHERE SCIID = @SCIID UNION ALL SELECT SCIID,99999,'NPW','NPW',Layer,Step+10 FROM SCI_ProcessFlow WHERE ID=(SELECT MAX(ID) FROM SCI_ProcessFlow WHERE SCIID=@SCIID)
                            ) t1 LEFT JOIN(
                            SELECT PFID,{0} FROM (SELECT PFID,CostItem,CostValue FROM SCO_ProcBreakdown WHERE SCIID=@SCIID) AS [SourceTable] 
                            PIVOT(SUM(CostValue) FOR CostItem IN ({0})) AS [PivotTable]) t2 
                            ON t1.ID = t2.PFID
                            WHERE t1.SCIID = @SCIID ORDER BY t1.ID", strFields);

            return DbHelperSQL.Query(strSql, new SqlParameter("@SCIID", dataId)).Tables[0];
        }

        public static DataTable GetBOMRptData(int dataId)
        {
            string strSql = @"SELECT t2.Layer,t2.Step,t1.MainWorkCenter,t1.WorkCenter,t1.LayupQtyPanel,t1.ManualPrice,t1.Unit,t1.Amount AS CostValue,
                            (SELECT DisplayName FROM SCS_CostItem WHERE CostItem=t1.MaterialType) Material 
                            FROM SCI_BOM t1 LEFT JOIN SCI_ProcessFlow t2 ON t1.PFID = t2.ID AND t1.SCIID = t2.SCIID WHERE t1.SCIID=@SCIID";
            return DbHelperSQL.Query(strSql, new SqlParameter("@SCIID", dataId)).Tables[0];
        }

        public static object GenrateCostSummaryAndBreakdown(int dataId)
        {
            FieldInfoCollecton fields = new FieldInfoCollecton();
            string strSql = "SELECT CostItem, DisplayName, DisplayMainGroup, DisplaySecondGroup FROM SCS_CostItem ORDER BY Sort";
            DataTable dtItem = DbHelperSQL.Query(strSql).Tables[0];

            strSql = "SELECT CostItem,SUM(CostValue) CostValue FROM SCO_ProcBreakdown WHERE SCIID = @SCIID GROUP BY CostItem";
            DataTable dtData = DbHelperSQL.Query(strSql, new SqlParameter("@SCIID", dataId)).Tables[0];

            Dictionary<string, Dictionary<string, FieldInfoCollecton>> dicMainGroup = new Dictionary<string, Dictionary<string, FieldInfoCollecton>>();

            foreach (DataRow drItem in dtItem.Rows)
            {
                string item = Convert.ToString(drItem["CostItem"]);

                FieldInfo fi = new FieldInfo();
                fi.FieldName = item;
                fi.DisplayName = Convert.ToString(drItem["DisplayName"]);
                fi.Enable = 1;
                fi.Visible = 0;
                fi.DataType = FieldInfo.DATATYPE_STRING;

                DataRow[] drs = dtData.Select(String.Format("CostItem='{0}'", item));
                if (drs != null && drs.Length > 0)
                {
                    fi.DataValue = drs[0]["CostValue"];
                }

                fields.Add(fi);

                string displayMainGroup = Convert.ToString(drItem["DisplayMainGroup"]);
                string displaySecondGroup = Convert.ToString(drItem["DisplaySecondGroup"]);
                if (!dicMainGroup.ContainsKey(displayMainGroup))
                {
                    Dictionary<string, FieldInfoCollecton> disSecondGroup = new Dictionary<string, FieldInfoCollecton>();
                    dicMainGroup.Add(displayMainGroup, disSecondGroup);
                }

                if (!dicMainGroup[displayMainGroup].ContainsKey(displaySecondGroup))
                {
                    FieldInfoCollecton fieldsSecond = new FieldInfoCollecton();
                    dicMainGroup[displayMainGroup].Add(displaySecondGroup, fieldsSecond);
                }

                dicMainGroup[displayMainGroup][displaySecondGroup].Add(fi);
            }

            return new
            {
                costBreakdown = GenrateCostBreakdownCategory(dicMainGroup),
                costSummary = GenrateSummaryCategory(dicMainGroup)
            };
        }

        public static string GenrateCostBreakdownCategory(Dictionary<string, Dictionary<string, FieldInfoCollecton>> dicMainGroup)
        {
            
            StringBuilder html = new StringBuilder();
            StringBuilder htmlMain = new StringBuilder();

            double totalValue = 0;
            foreach (KeyValuePair<string, Dictionary<string, FieldInfoCollecton>> kv in dicMainGroup)
            {
                double mainValue = 0;
                foreach (KeyValuePair<string, FieldInfoCollecton> kvSecode in kv.Value)
                {
                    foreach (FieldInfo f in kvSecode.Value)
                    {
                        mainValue += ParseHelper.Parse<double>(f.DataValue);
                    }
                }
                
                totalValue += mainValue;
                htmlMain.AppendFormat(@"<div class='widget-box'>
                                            <div class='widget-header header-color-blue'>
                                                <h5>{0}: $ {1}</h5>
                                                <div class='widget-toolbar'>
                                                    <a href='#' data-action='collapse'>
                                                        <i class='1 bigger-125 icon-chevron-up'></i>
                                                    </a>
                                                </div>
                                            </div>
                                            <div class='widget-body'>
                                                <div class='widget-body-inner' style='display: block;'>
                                                {2}
                                                </div>
                                            </div>
                                        </div>", kv.Key, mainValue, GenrateCostBreakdownCategory(kv.Value));
            }

            html.AppendFormat(@"<table width='100%'><tr><td width='50%'><h4 class='blue'>
                                &nbsp;<i class='orange icon-credit-card bigger-110'></i>
                                Total Cost: {0}
                            </h4></td><td width='50%' align='right'>
                            <button id='btnExpand' class='btn btn-minier btn-success' onclick='return expandCostBreakdown();'>
                                Expand
                                <i class='icon-plus icon-on-right bigger-110'></i>
                            </button>
                            <button id='btnCollapse' class='btn btn-minier btn-info' onclick='return collapseCostBreakdown();'>
                                Collapse
                                <i class='icon-minus icon-on-right bigger-110'></i>
                            </button></td></tr></table>", totalValue).Append(htmlMain);


            return html.ToString();
        }

        public static string GenrateCostBreakdownCategory(Dictionary<string, FieldInfoCollecton> dic)
        {
            Dictionary<string, string> dcwi = new Dictionary<string, string>();
            dcwi.Add("lableWidth", "16%");
            dcwi.Add("comWidth", "17%");
            StringBuilder html = new StringBuilder();
            foreach (KeyValuePair<string, FieldInfoCollecton> kv in dic)
            {
                double secondValue = 0;
                foreach (FieldInfo f in kv.Value)
                {
                    secondValue += ParseHelper.Parse<double>(f.DataValue);
                }

                html.AppendFormat(@"<div class='panel panel-default' style='margin-bottom: 0px'>
                                        <div class='panel-heading'>
                                            <a href='#faq-1-{0}' data-parent='#faq-list-{0}' data-toggle='collapse' class='accordion-toggle collapsed'>
                                                <i class='pull-right icon-chevron-left' data-icon-hide='icon-chevron-down' data-icon-show='icon-chevron-left'></i>
                                                <i class='icon-user bigger-130'></i>&nbsp; {1} : $ {2}
                                            </a>
                                        </div>
                                        <div class='panel-collapse collapse' id='faq-1-{0}' style='height: auto;'>
                                            <div class='panel-body'>
                                                    <table style='width:100%'>
                                                        {3}
                                                    </table>
                                    </div></div></div>", kv.Key.Replace(" ", "_"), kv.Key, secondValue, CostingMasterDataDetailHelper.GenrateCategoryFields(kv.Value, 6, dcwi));
            }
            return html.ToString();
        }

        public static string GenrateSummaryCategory(Dictionary<string, Dictionary<string, FieldInfoCollecton>> dicMainGroup)
        {
            StringBuilder html = new StringBuilder();
            html.AppendFormat("<table style='width:100%'><tr>");

            double variableCost = 0;
            double fixedCost = 0;

            foreach (KeyValuePair<string, FieldInfoCollecton> kvSecode in dicMainGroup["Variable Cost"])
            {
                foreach (FieldInfo f in kvSecode.Value)
                {
                    variableCost += ParseHelper.Parse<double>(f.DataValue);
                }
            }

            foreach (KeyValuePair<string, FieldInfoCollecton> kvSecode in dicMainGroup["Fixed Cost"])
            {
                foreach (FieldInfo f in kvSecode.Value)
                {
                    fixedCost += ParseHelper.Parse<double>(f.DataValue);
                }
            }

            html.AppendFormat(@"<td>
                                    <div class='pricing-box'>
                                        <div class='widget-box'>
                                            <div class='widget-header header-color-orange'>
                                                <h5 class='bigger lighter'>Variable Cost</h5>
                                            </div>
                                            <div class='widget-body'>
                                                <div class='widget-main'>
                                                    <ul class='list-unstyled spaced2' style='height:200px'>
                                                        {0}
                                                    </ul>
                                                    <hr>
                                                    <div class='price'>
                                                        ${1}
                                                        <small>/panel</small>
                                                    </div>
                                                </div>  
                                            </div>
                                        </div>
                                    </div>
                                </td>", GenrateSummaryCategory(dicMainGroup["Variable Cost"]), variableCost);

            html.Append("<td style='align-content:center;text-align:center'><span class='btn spinner-up btn-xs btn-primary'>+</span></td>");

            html.AppendFormat(@"<td>
                                    <div class='pricing-box'>
                                        <div class='widget-box'>
                                            <div class='widget-header header-color-blue'>
                                                <h5 class='bigger lighter'>Fixed Cost</h5>
                                            </div>
                                            <div class='widget-body'>
                                                <div class='widget-main'>
                                                    <ul class='list-unstyled spaced2' style='height:200px'>
                                                        {0}
                                                    </ul>
                                                    <hr>
                                                    <div class='price'>
                                                        ${1}
                                                        <small>/panel</small>
                                                    </div>
                                                </div>  
                                            </div>
                                        </div>
                                    </div>
                                </td>", GenrateSummaryCategory(dicMainGroup["Fixed Cost"]), fixedCost);

            html.Append("<td style='align-content:center;text-align:center'><span class='btn spinner-up btn-xs btn-primary'>=</span></td>");

            html.AppendFormat(@"<td>
                                    <div class='pricing-box'>
                                        <div class='widget-box'>
                                            <div class='widget-header header-color-green'>
                                                <h5 class='bigger lighter'>Total Cost</h5>
                                            </div>
                                            <div class='widget-body'>
                                                <div class='widget-main'>
                                                    <ul class='list-unstyled spaced2' style='height:200px'>
                                                        <li>
                                                            Variable Cost:
                                                            <b class='red'>${0}</b>
                                                        </li>
                                                        <li>
                                                            Fixed Cost:
                                                            <b class='red'>${1}</b>
                                                        </li>
                                                    </ul>
                                                    <hr>
                                                    <div class='price'>
                                                        ${2}
                                                        <small>/panel</small>
                                                    </div>
                                                </div>  
                                            </div>
                                        </div>
                                    </div>
                                </td>", variableCost, fixedCost, variableCost + fixedCost);

            html.AppendFormat("</tr></table>");

            return html.ToString();
        }

        public static string GenrateSummaryCategory(Dictionary<string, FieldInfoCollecton> dic)
        {
            StringBuilder html = new StringBuilder();

            foreach (KeyValuePair<string, FieldInfoCollecton> kv in dic)
            {
                double secondValue = 0;
                foreach (FieldInfo f in kv.Value)
                {
                    secondValue += ParseHelper.Parse<double>(f.DataValue);
                }

                html.AppendFormat(@"<li>
                                        {0}:
                                        <b class='red'>${1}</b>
                                    </li>", kv.Key, secondValue);
            }
            return html.ToString();
        }
    }
}
