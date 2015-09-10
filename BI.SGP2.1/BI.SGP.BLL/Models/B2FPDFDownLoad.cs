using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SGP.DBUtility;
using System.Web;
using System.Drawing;
using BI.SGP.BLL.DataModels;


namespace BI.SGP.BLL.Models
{
    public class B2FPDFDownLoad
    {
        public Font defaultfont = FontFactory.getFont("Arial", 9, Font.NORMAL, new Color(0, 0, 0));
        public Font font8 = FontFactory.getFont("Arial", 8, Font.NORMAL, new Color(0, 0, 0));
        public Font underlinefont8 = FontFactory.getFont("Arial", 8, Font.UNDERLINE, new Color(0, 0, 0));
        public Font boldfont8 = FontFactory.getFont("Arial", 8, Font.BOLD, new Color(0, 0, 0));
        public Font boldfont = FontFactory.getFont("Arial", 9, Font.BOLD, new Color(0, 0, 0));
        public Font boldbulefont = FontFactory.getFont("Arial", 9, Font.BOLD, new Color(0, 0, 255));
        public Font bluefont = FontFactory.getFont("Arial", 9, Font.NORMAL, new Color(0, 0, 255));
        public Font redfont = FontFactory.getFont("Arial", 9, Font.NORMAL, new Color(255, 0, 0));
        public Document doc;
        public PdfWriter writer;
        public DataTable maindt;
        public int rowid = 0;
        Dictionary<string, DataTable> tabls = new Dictionary<string, DataTable>();
        List<string> priceCurr = new List<string>();
        List<string> shipmentTermsList = new List<string>();
        List<string> payTermsList = new List<string>();

        public B2FPDFDownLoad()
        {


        }
        public void SetDT(int id)
        {

            Dictionary<string, Dictionary<string, string>> subvalue = new Dictionary<string, Dictionary<string, string>>();
            List<FieldCategory> fcs = FieldCategory.GetCategorys(FieldCategory.Category_TYPE_B2F);
            foreach (FieldCategory fc in fcs)
            {
                FieldInfoCollecton subfis = fc.SubFields;
                foreach (FieldInfo fi in subfis)
                {
                    if (!subvalue.ContainsKey(fi.SubDataType))
                    {
                        subvalue.Add(fi.SubDataType, new Dictionary<string, string>());
                    }
                    if (!subvalue[fi.SubDataType].ContainsKey(fi.FieldName))
                    {
                        subvalue[fi.SubDataType].Add(fi.FieldName, fi.DisplayName);
                    }

                }
            }
            Dictionary<string, Dictionary<string, string>> dd = subvalue;
            foreach (string field in subvalue.Keys)
            {
                string strfield = "";

                foreach (string fi in subvalue[field].Keys)
                {
                    strfield += fi + ",";
                }
                strfield = strfield.TrimEnd(',');
                string strSql = "select " + strfield + " from SGP_SubData where Entityid=@RFQID and EntityName=@EntityName";
                DataTable dt = new DataTable();
                dt = DbHelperSQL.Query(strSql, new SqlParameter[] { new SqlParameter("@RFQID", id), new SqlParameter("@EntityName", field) }).Tables[0];

                tabls.Add(field, dt);
            }
            string strmainsql = "SELECT a.*,b.* FROM V_SGPForFPC a left join Access_User b on a.PrimaryContact=b.Name where a.rfqid = @RFQID";
            maindt = new DataTable();

            maindt = DbHelperSQL.Query(strmainsql, new SqlParameter("@RFQID", id)).Tables[0];

        }

        /// <summary>
        /// Get Product Information Data
        /// </summary>
        /// <param name="rfqId"></param>
        /// <returns></returns>
        /// Lance Chen 20150102
        private DataTable GetProductInformation(int rfqId)
        {
            string strSql = @"
select f.UnitSizeWidth, f.UnitSizeLength, f.MaterialCategory, f.ArraySizeWidth, f.ArraySizeLength,
		f.BoardThickness, f.UnitType, f.LayerCount, f.Copper, f.Finishing, f.Holes, f.LnO, f.LnI,
		f.SmallestHole, f.Imped, f.Outline, f.XOutAllowance, f.BlindQty, f.BlindSize, f.BuriedQty,
		f.BuriedSize, f.BoardConstruction, f.LPISolderMask, f.MicroViaSize, f.PreBending, f.ViaStructure
from V_SGPForFPC f
where f.RFQID = @RFQID";
            DataSet ds = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", rfqId));
            DataTable dt = ds.Tables[0];
            return dt;
        }

        /// <summary>
        /// Get Tooling Summary Data
        /// </summary>
        /// <param name="rfqId"></param>
        /// <returns></returns>
        /// Lance Chen 20141231
        private DataTable GetToolingSummary(int rfqId)
        {
            string strSql = @"
with a
as (select * from V_SGPForFPC
	where RFQID = @RFQID)
select '01' Seq, 'Prototype Tooling Price' ToolingType, OutlineTool1_1 OutlineTool1, OutlineTool2_1 OutlineTool2,
	   TopCoverlayTool_1 TopCoverlayTool, BottomCoverlayTool_1 BottomCoverlayTool,
	   ETestCADArtworkNRE_1 ETestCADArtworkNRE, AssemblySampleTooling SMTToolingFixture,
	   Total_1 Total
from a
union
select '02' Seq, 'MP Tooling Price' ToolingType, OutlineTool1_2 OutlineTool1, OutlineTool2_2 OutlineTool2,
	   TopCoverlayTool_2 TopCoverlayTool, BottomCoverlayTool_2 BottomCoverlayTool,
	   ETestCADArtworkNRE_2 ETestCADArtworkNRE, AssemblyMpTooling SMTToolingFixture,
	   Total_2 Total
from a
union
select '03' Seq, ToolingType_3 ToolingType, OutlineTool1_3 OutlineTool1, OutlineTool2_3 OutlineTool2,
	   TopCoverlayTool_3 TopCoverlayTool, BottomCoverlayTool_3 BottomCoverlayTool,
	   ETestCADArtworkNRE_3 ETestCADArtworkNRE, SMTToolingFixture_3 SMTToolingFixture,
	   Total_3 Total
from a
where ToolingType_3 is not null
and not (OutlineTool1_3 is null and OutlineTool2_3 is null and TopCoverlayTool_3 is null
		and BottomCoverlayTool_3 is null and ETestCADArtworkNRE_3 is null
		and SMTToolingFixture_3 is null and Total_3 is null)
union
select '04' Seq, ToolingType_4 ToolingType, OutlineTool1_4 OutlineTool1, OutlineTool2_4 OutlineTool2,
	   TopCoverlayTool_4 TopCoverlayTool, BottomCoverlayTool_4 BottomCoverlayTool,
	   ETestCADArtworkNRE_4 ETestCADArtworkNRE, SMTToolingFixture_4 SMTToolingFixture,
	   Total_4 Total
from a
where ToolingType_4 is not null
and not (OutlineTool1_4 is null and OutlineTool2_4 is null and TopCoverlayTool_4 is null
		and BottomCoverlayTool_4 is null and ETestCADArtworkNRE_4 is null
		and SMTToolingFixture_4 is null and Total_4 is null)";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", rfqId)).Tables[0];
            return dt;
        }

        /// <summary>
        /// Get Quotation Summary Data
        /// </summary>
        /// <param name="rfqId"></param>
        /// <returns></returns>
        /// Lance Chen 20141231
        private DataTable GetPriceDetail(int rfqId)
        {
            string strSql = @"
with a
as (select * from V_SGPForFPC
	where RFQID = @RFQID)
select '1' Item, a.PCBFPCPrice1 PCBFPCPrice, a.BOMPrice1 BOMPrice, a.AssemblyPrice1 AssemblyPrice,
	   a.TotalPrice1 TotalPrice, a.MOQ1 MOQ, a.Remark1 Remark, a.Currency1 Currency,
	   a.ShipmentTerms1 ShipmentTerms, a.PayTerms1 PayTerms
from a
where not (a.PCBFPCPrice1 is null and a.BOMPrice1 is null and a.AssemblyPrice1 is null
		  and a.TotalPrice1 is null and a.MOQ1 is null)
and not (a.PCBFPCPrice1 = 0 and a.BOMPrice1 = 0 and a.AssemblyPrice1 = 0 and
		  a.TotalPrice1 = 0)
union
select '2' Item, a.PCBFPCPrice2 PCBFPCPrice, a.BOMPrice2 BOMPrice, a.AssemblyPrice2 AssemblyPrice,
	   a.TotalPrice2 TotalPrice, a.MOQ2 MOQ, a.Remark2 Remark, a.Currency2 Currency,
	   a.ShipmentTerms2 ShipmentTerms, a.PayTerms2 PayTerms
from a
where not (a.PCBFPCPrice2 is null and a.BOMPrice2 is null and a.AssemblyPrice2 is null
		  and a.TotalPrice2 is null and a.MOQ2 is null)
and not (a.PCBFPCPrice2 = 0 and a.BOMPrice2 = 0 and a.AssemblyPrice2 = 0 and
		  a.TotalPrice2 = 0)
union
select '3' Item, a.PCBFPCPrice3 PCBFPCPrice, a.BOMPrice3 BOMPrice, a.AssemblyPrice3 AssemblyPrice,
	   a.TotalPrice3 TotalPrice, a.MOQ3 MOQ, a.Remark3 Remark, a.Currency3 Currency,
	   a.ShipmentTerms3 ShipmentTerms, a.PayTerms3 PayTerms
from a
where not (a.PCBFPCPrice3 is null and a.BOMPrice3 is null and a.AssemblyPrice3 is null
		  and a.TotalPrice3 is null and a.MOQ3 is null)
and not (a.PCBFPCPrice3 = 0 and a.BOMPrice3 = 0 and a.AssemblyPrice3 = 0 and
		  a.TotalPrice3 = 0)
union
select '4' Item, a.PCBFPCPrice4 PCBFPCPrice, a.BOMPrice4 BOMPrice, a.AssemblyPrice4 AssemblyPrice,
	   a.TotalPrice4 TotalPrice, a.MOQ4 MOQ, a.Remark4 Remark, a.Currency4 Currency,
	   a.ShipmentTerms4 ShipmentTerms, a.PayTerms4 PayTerms
from a
where not (a.PCBFPCPrice4 is null and a.BOMPrice4 is null and a.AssemblyPrice4 is null
		  and a.TotalPrice4 is null and a.MOQ4 is null)
and not (a.PCBFPCPrice4 = 0 and a.BOMPrice4 = 0 and a.AssemblyPrice4 = 0 and
		  a.TotalPrice4 = 0)
union
select '5' Item, a.PCBFPCPrice5 PCBFPCPrice, a.BOMPrice5 BOMPrice, a.AssemblyPrice5 AssemblyPrice,
	   a.TotalPrice5 TotalPrice, a.MOQ5 MOQ, a.Remark5 Remark, a.Currency5 Currency,
	   a.ShipmentTerms5 ShipmentTerms, a.PayTerms5 PayTerms
from a
where not (a.PCBFPCPrice5 is null and a.BOMPrice5 is null and a.AssemblyPrice5 is null and
		  a.TotalPrice5 is null and a.MOQ5 is null)
and not (a.PCBFPCPrice5 = 0 and a.BOMPrice5 = 0 and a.AssemblyPrice5 = 0 and
		  a.TotalPrice5 = 0)";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", rfqId)).Tables[0];
            return dt;
        }

        /// <summary>
        /// Read the graph's path of remarks & pricing assumption
        /// </summary>
        /// <param name="rfqId"></param>
        /// <returns></returns>
        /// Lance Chen 20141230
        public string GetGraphPath(int rfqId)
        {
            string strSql = @"select top 1 FileName, SourceName
                            from SGP_Files
                            where RelationKey = @RFQID
                            and CategoryDesc = 'Product Information'
                            and RIGHT(FileName, CHARINDEX('.', REVERSE(FileName))-1) in ('png', 'jpg', 'jpeg', 'gif', 'bmp', 'DXF', 'TIFF')
                            order by CreateTime";
            DataTable dtFileInfo = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", rfqId)).Tables[0];
            if (dtFileInfo != null && dtFileInfo.Rows.Count > 0)
            {
                string folderPath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"].TrimEnd('\\') + "\\";
                folderPath += rfqId.ToString() + "\\";
                if (Directory.Exists(folderPath))
                {
                    DataRow drFileInfo = dtFileInfo.Rows[0];
                    return folderPath + drFileInfo["FileName"].ToString();
                }
            }
            return string.Empty;
        }

        public bool WriterPDF(ref MemoryStream mem, int quoteId, out string filename)
        {
            filename = "";
            bool pass = false;
            try
            {

                SetDT(quoteId);
                DataRow row = maindt.Rows[0];
                string extnumber = row["ExtNumber"].ToString();
                if (row["ExtNumber"].ToString().IndexOf('-') > 0)
                {
                    extnumber = row["ExtNumber"].ToString().Substring(0, row["ExtNumber"].ToString().IndexOf('-'));
                }


                if (maindt.Rows.Count > 0)
                {

                    filename = row["OEM"].ToString() + "_" + extnumber + "_" + row["CustomerPartNumber"].ToString().Trim();
                    filename = filename.Replace("\"", "").Replace("'", "").Replace(";", "").Replace(":", "") + "_" + DateTime.Now.ToString("mmssffff");
                    doc = new Document(PageSize.A4);
                    // doc.setPageSize(PageSize.A4);
                    writer = PdfWriter.getInstance(doc, mem);
                    AddHeaderFooter();
                    doc.Open();
                    AddAddress();
                    AddQuotationTitle();
                    AddQuotationInfo();
                    AddQuotationSummary(quoteId);
                    AddToolingSummary(quoteId);
                    AddLeadTimePayShipTerms();
                    AddTechniaclSummary(quoteId);
                    AddRemarkPricingAssumption(quoteId);
                    AddTermConditions();
                    AddReaderContent();
                    doc.Close();
                    pass = true;
                }
            }
            catch (Exception ex)
            {
                string g = ex.ToString();
                pass = false;

            }
            return pass;
        }
        public void AddHeaderFooter()
        {
            string image_url = HttpContext.Current.Server.MapPath("~/tmp/logo.png");
            iTextSharp.text.Image png = iTextSharp.text.Image.getInstance(new Uri(image_url));
            png.scaleToFit(100, 100);
            png.Interpolation = true;
            HeaderFooter header = new HeaderFooter(new Phrase(new Chunk(png, 10, 0)), false);
            header.Border = iTextSharp.text.Rectangle.NO_BORDER;
            doc.Header = header;
        }
        public void AddAddress()
        {
            PdfPTable pt = new PdfPTable(2);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue(@"Purchase Order Address:", false, boldfont, 0));
            pt.addCell(CellValue(@"  ", false, boldfont, 0));
            pt.addCell(CellValue(@"MULTEK TECHNOLOGIES LIMITED" + System.Environment.NewLine +
                                 @"1st Floor, The Exchange," + System.Environment.NewLine +
                                 @"18 Cybercity, " + System.Environment.NewLine +
                                 @"Ebene, REPUBLIC OF MAURITIUS" + System.Environment.NewLine +
                                 @"Fax the PO copy to Multek Hong Kong", false, defaultfont, 0));
            pt.addCell(CellValue(@" " + System.Environment.NewLine +
                                 @"Telephone # (230) 454 3200" + System.Environment.NewLine +
                                 @"Fax # (230) 454 3202" + System.Environment.NewLine +
                                 @"Fax # (852) 2276 1122" + System.Environment.NewLine, false, defaultfont, 0));
            doc.Add(pt);
        }
        public void AddQuotationTitle()
        {
            PdfPTable pt = new PdfPTable(1);
            pt.WidthPercentage = 100;
            PdfPCell cell = CellValue(@"Quotation", true, boldfont, 1);
            cell.BackgroundColor = new Color(192, 192, 192);
            pt.addCell(cell);
            doc.Add(pt);
        }
        public void AddQuotationInfo()
        {
            PdfPTable pt = new PdfPTable(4);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue(@"Company:", false, boldfont, 0));
            pt.addCell(CellValue(maindt.Rows[0]["OEM"].ToString(), false, bluefont, 0));
            pt.addCell(CellValue(@"Date:", false, boldfont, 0));
            pt.addCell(CellValue(Convert.ToDateTime(maindt.Rows[0]["CustomerQuoteDate"]).ToString("MM/dd/yyyy"), false, bluefont, 0));

            pt.addCell(CellValue(@"Attention	:", false, boldfont, 0));
            pt.addCell(CellValue(maindt.Rows[0]["CustomerContact"].ToString(), false, bluefont, 0));
            pt.addCell(CellValue(@"RFQ#	:", false, boldfont, 0));
            pt.addCell(CellValue(maindt.Rows[0]["ExtNumber"].ToString(), false, bluefont, 0));

            pt.addCell(CellValue(@"Customer PN :", false, boldfont, 0));
            pt.addCell(CellValue(maindt.Rows[0]["CustomerPartNumber"].ToString(), false, bluefont, 0));
            pt.addCell(CellValue(@"", false, boldfont, 0));
            pt.addCell(CellValue("", false, bluefont, 0));
            doc.Add(pt);
            doc.Add(new Phrase("" + System.Environment.NewLine));
            PdfPTable pt1 = new PdfPTable(1);
            pt1.WidthPercentage = 100;
            pt1.addCell(CellValue(@"Thank you for your inquiry, we are pleased to provide quotation for the requested flexible circuits as follow:", false, defaultfont, 0));
            doc.Add(pt1);
        }

        public void AddQuotationSummary(int rfqId)
        {
            PdfPTable pt = new PdfPTable(1);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue(@"1.	Quotation Summary", false, boldfont, 0));
            doc.Add(pt);

            PdfPTable pt1 = new PdfPTable(7);
            pt1.WidthPercentage = 100;
            pt1.addCell(CellValue(@"Item", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"Bare FPC/RFPC Price(USD)", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"BOM Price(USD)", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"Assembly Price(USD)", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"Total Price(USD)", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"MOQ(pcs)", true, boldfont, 1, true));
            pt1.addCell(CellValue(@"Remark", true, boldfont, 1, true));
            float[] widths = new float[] { 40f, 30f, 30f, 30f, 30f, 30f, 60f };
            pt1.setWidths(widths);

            DataTable dt = GetPriceDetail(rfqId);
            foreach (DataRow dr in dt.Rows)
            {
                priceCurr.Add(dr[7].ToString());
                if (!(dr[8] is DBNull) && !shipmentTermsList.Contains(dr[8].ToString()))
                {
                    shipmentTermsList.Add(dr[8].ToString());
                }
                if (!(dr[9] is DBNull) && !payTermsList.Contains(dr[9].ToString()))
                {
                    payTermsList.Add(dr[9].ToString());
                }
                for (int i = 0; i < 7; i++)
                {
                    if (i == 1 || i == 2 || i == 3 || i == 4)
                    {
                        if (dr[i] is DBNull || dr[i].ToString() == "0")
                        {
                            pt1.addCell(CellValue("N/A", true, bluefont, 1, false));
                        }
                        else
                        {
                            pt1.addCell(CellValue("$" + dr[i].ToString(), true, bluefont, 1, false));
                        }
                    }
                    else
                    {
                        pt1.addCell(CellValue(dr[i] is DBNull ? "" : dr[i].ToString(), true, bluefont, 1, false));
                    }
                }
            }

            //pt1.addCell(CellValue(@"Remark:" + System.Environment.NewLine +
            //                       @"  1. The tooling cost base on 10k/year and the panel is 48up/set." + "\n" +
            //                       @"  2. Assume the FPCA assembly standard follows IPC-A-610 class2." + "\n" +
            //                       @"  3. Assume DI water cleaning process with water solution solder paste." + "\n" +
            //                       @"  4. Assume ICT test time is 24s per unit and the tester cost is around $1400. Any change the cost must be update.", false, bluefont, 0, 7));
            string remarks = "Remark:\n" + maindt.Rows[0]["Remarks"].ToString();
            pt1.addCell(CellValue(remarks, false, bluefont, 0, 7));

            doc.Add(pt1);
        }
        public void AddToolingSummary(int rfqId)
        {
            DataTable dt = GetToolingSummary(rfqId);
            int rcount = dt.Rows.Count;

            PdfPTable pt = new PdfPTable(1);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue(@"2.	Tooling Summary", false, boldfont, 0));
            doc.Add(pt);

            PdfPTable pt1 = new PdfPTable(rcount + 1);
            pt1.WidthPercentage = 70;
            pt1.HorizontalAlignment = Element.ALIGN_LEFT;
            pt1.addCell(CellValue(@"Item", true, boldfont, 1, true));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][1].ToString(), true, boldfont, 1, true));
            }

            List<float> widths = new List<float>();
            for (int i = 0; i < rcount + 1; i++)
            {
                widths.Add(40f);
            }
            pt1.setWidths(widths.ToArray());

            pt1.addCell(CellValue(@"Outline Tool-1", true, bluefont, 1, false));

            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][2] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][2]).ToString("#,###"), true, bluefont, 1, false));
            }
            
            pt1.addCell(CellValue(@"Outline Tool-2", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][3] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][3]).ToString("#,###"), true, bluefont, 1, false));
            }

            pt1.addCell(CellValue(@"Top Coverlay Tool", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][4] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][4]).ToString("#,###"), true, bluefont, 1, false));
            }

            pt1.addCell(CellValue(@"Bottom Coverlay Tool", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][5] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][5]).ToString("#,###"), true, bluefont, 1, false));
            }

            pt1.addCell(CellValue(@"E-test/CAD/Artwork/NRE", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][6] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][6]).ToString("#,###"), true, bluefont, 1, false));
            }

            pt1.addCell(CellValue(@"SMT Tooling/Fixture", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][7] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][7]).ToString("#,###"), true, bluefont, 1, false));
            }

            pt1.addCell(CellValue(@"Total", true, bluefont, 1, false));
            for (int i = 0; i < rcount; i++)
            {
                pt1.addCell(CellValue(dt.Rows[i][8] is DBNull ? "N/A" : "$" + Convert.ToInt32(dt.Rows[i][8]).ToString("#,###"), true, bluefont, 1, false));
            }

            int eau = Convert.ToInt32(maindt.Rows[rowid]["VolumePerMonth"] is DBNull ? "0" : maindt.Rows[rowid]["VolumePerMonth"]);
            pt1.addCell(CellValue(@"Remark:" + System.Environment.NewLine +
                                   string.Format(@"  MP Tooling charge based on run-rate of {0}K/Year.", eau / 1000), false, bluefont, 0, 7));
            doc.Add(pt1);
        }

        public void AddLeadTimePayShipTerms()
        {
            PdfPTable pt = new PdfPTable(2);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue("3.	Production Lead-time, Payment & Shipment Terms", false, boldfont, 0, 2));
            pt.addCell(CellValue("Lead Time of FPC/RFPC", true, defaultfont, 0, true));
            string leadTimeText = string.Format("Production first article parts will ship {0} after receipt of order.\r\n", maindt.Rows[rowid]["LeadTime"].ToString());
            leadTimeText += "Lead time for follow on production orders is 4 weeks with forecast, or 6 weeks without forecast.\r\n";
            leadTimeText += "Actual lead time depends on material and factory loading.";
            pt.addCell(CellValue(leadTimeText, true, bluefont, 0, false));

            pt.addCell(CellValue("Lead Time of BOM", true, defaultfont, 0, true));
            pt.addCell(CellValue(maindt.Rows[rowid]["SMTBOMLeadtime"].ToString(), true, bluefont, 0, false));
            
            pt.addCell(CellValue("Shipment Term", true, defaultfont, 0, true));
            string strShipmentTerms = string.Empty;
            foreach (string shipmentTerms in shipmentTermsList)
            {
                strShipmentTerms += shipmentTerms + "\n";
            }
            pt.addCell(CellValue(strShipmentTerms, true, bluefont, 0, false));

            pt.addCell(CellValue("Payment Term", true, defaultfont, 0, true));
            //pt.addCell(CellValue(maindt.Rows[rowid]["PayTerms"].ToString(), true, bluefont, 0, false));
            string strPayTerms = string.Empty;
            foreach (string payTerms in payTermsList)
            {
                strPayTerms += "Payment terms are " + payTerms + "\n";
            }
            strPayTerms += "Production tooling will be invoiced 100% upon order placement and 100% upon shipment of first article circuits.";
            pt.addCell(CellValue(strPayTerms, true, bluefont, 0, false));
            doc.Add(pt);
        }

        public void AddTechniaclSummary(int rfqId)
        {
            DataTable dt = GetProductInformation(rfqId);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                PdfPTable pt = new PdfPTable(4);
                pt.WidthPercentage = 100;
                pt.addCell(CellValue("4.	Technical Summary", false, boldfont, 0, 4));
                pt.addCell(CellValue("Conditions", true, boldfont, 0, true, 4));

                pt.addCell(CellValue("Card Size", true, defaultfont, 0, false));
                string unitSizeW = row["UnitSizeWidth"] is DBNull ? "" : row["UnitSizeWidth"].ToString();
                string unitSizeL = row["UnitSizeLength"] is DBNull ? "" : row["UnitSizeLength"].ToString();
                if (!string.IsNullOrEmpty(unitSizeW) && !string.IsNullOrEmpty(unitSizeL))
                {
                    pt.addCell(CellValue(unitSizeW + "*" + unitSizeL + "mm", true, bluefont, 0, false));
                }
                else
                {
                    pt.addCell(CellValue("N/A", true, bluefont, 0, false));
                }
                pt.addCell(CellValue("Material", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["MaterialCategory"] is DBNull ? "N/A" : row["MaterialCategory"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("Array Size", true, defaultfont, 0, false));
                string arraySizeW = row["ArraySizeWidth"] is DBNull ? "" : row["ArraySizeWidth"].ToString();
                string arraySizeL = row["ArraySizeLength"] is DBNull ? "" : row["ArraySizeLength"].ToString();
                if (!string.IsNullOrEmpty(arraySizeW) && !string.IsNullOrEmpty(arraySizeL))
                {
                    pt.addCell(CellValue(arraySizeW + "*" + arraySizeL + "mm", true, bluefont, 0, false));
                }
                else
                {
                    pt.addCell(CellValue("N/A", true, bluefont, 0, false));
                }
                pt.addCell(CellValue("Thickness (mm/inch)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["BoardThickness"] is DBNull ? "N/A" : row["BoardThickness"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("# of Layers", true, defaultfont, 0, false));
                string viaStructure = row["ViaStructure"].ToString();
                if (!string.IsNullOrEmpty(viaStructure))
                {
                    pt.addCell(CellValue(row["LayerCount"] is DBNull ?
                        "N/A" : row["LayerCount"].ToString() + " [ " + viaStructure + " ] ", true, bluefont, 0, false));
                }
                else
                {
                    pt.addCell(CellValue(row["LayerCount"] is DBNull ? "N/A" : row["LayerCount"].ToString(), true, bluefont, 0, false));
                }
                pt.addCell(CellValue("Copper (ext/int)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["Copper"] is DBNull ? "N/A" : row["Copper"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("Construction", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["BoardConstruction"] is DBNull ? "N/A" : row["BoardConstruction"].ToString(), true, bluefont, 0, false));
                pt.addCell(CellValue("Surface Finishing", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["Finishing"] is DBNull ? "N/A" : row["Finishing"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("# of Holes", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["Holes"] is DBNull ? "N/A" : row["Holes"].ToString(), true, bluefont, 0, false));
                pt.addCell(CellValue("Line & Space (ext)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["LnO"] is DBNull ? "N/A" : row["LnO"].ToString(), true, bluefont, 0, false));


                pt.addCell(CellValue("Smallest Hole", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["SmallestHole"] is DBNull ? "N/A" : row["SmallestHole"].ToString(), true, bluefont, 0, false));
                pt.addCell(CellValue("Line & Space (int)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["LnI"] is DBNull ? "N/A" : row["LnI"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("Impedance (Y/N)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["Imped"] is DBNull ? "N/A" : row["Imped"].ToString(), true, bluefont, 0, false));
                pt.addCell(CellValue("Outline Profiling", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["Outline"] is DBNull ? "N/A" : row["Outline"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("Micro Via Size (Qty)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["MicroViaSize"] is DBNull ? "N/A" : row["MicroViaSize"].ToString(), true, bluefont, 0, false));
                pt.addCell(CellValue("LPI Solder Mask(Y/N)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["LPISolderMask"] is DBNull ? "N/A" : row["LPISolderMask"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("Blind / Buried Via", true, defaultfont, 0, false));
                string blindQty = row["BlindQty"].ToString();
                string blindSize = row["BlindSize"].ToString();
                string buriedQty = row["BuriedQty"].ToString();
                string buriedSize = row["BuriedSize"].ToString();
                if ((!string.IsNullOrEmpty(blindQty) && blindQty != "0") ||
                    (!string.IsNullOrEmpty(blindSize) && blindQty != "0") ||
                    (!string.IsNullOrEmpty(buriedQty) && blindQty != "0") ||
                    (!string.IsNullOrEmpty(buriedSize) && blindQty != "0"))
                {
                    pt.addCell(CellValue("Y", true, bluefont, 0, false));
                }
                else
                {
                    pt.addCell(CellValue("N", true, bluefont, 0, false));
                }
                pt.addCell(CellValue("Pre-bending (Y/N)", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["PreBending"] is DBNull ? "N/A" : row["PreBending"].ToString(), true, bluefont, 0, false));

                pt.addCell(CellValue("X out criteria", true, defaultfont, 0, false));
                pt.addCell(CellValue(row["XOutAllowance"] is DBNull ? "N/A" : row["XOutAllowance"].ToString(), true, bluefont, 0, 4));

                doc.Add(pt);
            }
        }
        public void AddRemarkPricingAssumption(int rfqId)
        {

            PdfPTable pt = new PdfPTable(1);
            pt.WidthPercentage = 100;
            pt.addCell(CellValue("   ", false, boldfont, 0, 4));
            pt.addCell(CellValue("Remarks & Pricing Assumption", true, boldfont, 0, true));

            //string remarks = maindt.Rows[rowid]["Remarks"].ToString();
            //if (!string.IsNullOrEmpty(remarks))
            //{
            //    PdfPCell c1 = new PdfPCell(CellValue(remarks, true, bluefont, 0, false));
            //    c1.Border = Rectangle.LEFT | Rectangle.RIGHT | Rectangle.TOP;
            //    pt.addCell(c1);
            //}
            //else
            //{
            //    pt.addCell(CellValue("  ", true, bluefont, 0, 4));
            //}

            bool isAllCurrSame = true;
            if (priceCurr.Count > 0)
            {
                foreach (string curr1 in priceCurr)
                {
                    foreach (string curr2 in priceCurr)
                    {
                        if (curr1 != curr2)
                        {
                            isAllCurrSame = false;
                            break;
                        }
                    }
                    if (!isAllCurrSame)
                    {
                        break;
                    }
                }
            }
            else
            {
                isAllCurrSame = false;
            }
            string remarks = "This quote is budgetary pending review of the finalized design and specifications.\n";
            if (isAllCurrSame)
            {
                remarks += string.Format("All prices shown are in {0}.\n", priceCurr[0]);
            }
            remarks += "Actual delivery schedules must be negotiated with and confirmed by Multek Flexible Circuits upon receipt of order.\n";
            remarks += "Assumed stackup as below;\n";
            PdfPCell c1 = new PdfPCell(CellValue(remarks, true, bluefont, 0, 4));
            //c1.Border = Rectangle.LEFT | Rectangle.RIGHT | Rectangle.TOP;
            pt.addCell(c1);

            string filePath = GetGraphPath(rfqId);
            if (!string.IsNullOrEmpty(filePath))
            {
                iTextSharp.text.Image png = iTextSharp.text.Image.getInstance(new Uri(filePath));
                png.scaleToFit(400, 1200);
                png.Interpolation = true;

                PdfPCell c2 = new PdfPCell(new Phrase(new Chunk(png, 10, 20)));

                c2.Border = Rectangle.LEFT | Rectangle.RIGHT | Rectangle.BOTTOM;
                pt.addCell(c2);
            }
            doc.Add(pt);
            //string image_url = HttpContext.Current.Server.MapPath("~/tmp/pic1.png");
            //iTextSharp.text.Image png = iTextSharp.text.Image.getInstance(new Uri(image_url));
            //png.scaleToFit(400, 1200);
            //png.Interpolation = true;

            //PdfPCell c2 = new PdfPCell(new Phrase(new Chunk(png, 10, 20)));

            //c2.Border = Rectangle.LEFT | Rectangle.RIGHT | Rectangle.BOTTOM;
            //pt.addCell(c2);
            //doc.Add(pt);
        }
        public void AddTermConditions()
        {

            doc.Add(new Phrase(new Chunk("Term and Conditions" + "\n", underlinefont8)));

            doc.Add(new Phrase(new Chunk(@"Lead-time is quoted in business days (excludes weekends & holidays) and is subject to the availability of material and may change within 24 hours " + "\n" +
                                         @"notice.  Lead-time for non-standard materials, approximately three weeks and is subject to confirmation.  Prices are valid for 30 days from date of " + "\n" +
                                         @"quote.  Purchase order & data files, as stated below, must be received no later than 10:00AM for that day to be applied." + "\n" +
                                         @"All quote information is subject to change after receipt of Gerber data files, fabrication drawings & specifications.  Prices are only applicable for new " + "\n" +
                                         @"purchase orders unless specified.  Acceptance of a purchase order is contingent upon satisfactory credit approval.  Deliveries maybe subject to " + "\n" +
                                         @"applicable local sales taxes, VAT." + "\n" +
                                         @"" + "\n" +
                                         @"Multek standard Terms and Conditions are applied.  Unless otherwise negotiated or modified herein, these terms and conditions apply to this " + "\n" +
                                         @"quotation and to all purchase orders placed against this quotation. This quote is provided in accordance with the Multek Warranty Policy Terms and " + "\n" +
                                         @"Conditions." + "\n" +
                                         @"" + "\n"
                                         , font8)));
            doc.Add(new Phrase(new Chunk(@"Customer is responsible for any excess and obsolete material/component resulted from ECO including MOQ." + "\n" +
                                         @"Excess and Obsolete will be reviewed and settled on quarterly/monthly basis or on EOL stage." + "\n" +
                                         @"Excess material resulted from MOQ will be reviewed in quarterly basis to settle E&O liability." + "\n" +
                                         @"Excess and obsolete material  due to pull out delivery schedule for three months need to be settled from customer immediately." + "\n" +
                                         @"" + "\n" + "", boldfont8)));
            doc.Add(new Phrase(new Chunk("Policy for Cancellations" + "\n", underlinefont8)));
            doc.Add(new Phrase(new Chunk(@"- 45 calendar days for the order " + "\n" +
                                         @"- 60 days for standard material " + "\n" +
                                         @"- 90 days for ""special"" material" + "\n" +
                                         @" " + "\n"
                                        , font8)));
            doc.Add(new Phrase(new Chunk("ADDITIONAL NOTES :" + "\n", boldfont)));
            doc.Add(new Phrase(new Chunk(@"- Minimum Order per Delivery : = ", boldfont)));
            doc.Add(new Phrase(new Chunk(@"$" + "\n", boldbulefont)));
            doc.Add(new Phrase(new Chunk(@"-" + "\n", boldfont)));
            doc.Add(new Phrase(new Chunk(@"We thank you for the opportunity and we are looking forward to receive your order!" + "\n", boldfont)));

        }
        public void AddReaderContent()
        {
            // string file = System.Environment.CurrentDirectory + @"\template.pdf";

            string file = HttpContext.Current.Server.MapPath("~/tmp/b2ftemplate.pdf");
            PdfContentByte cb = writer.DirectContent;
            PdfReader reader = new PdfReader(file);

            for (int i = 3; i < reader.NumberOfPages + 1; i++)
            {
                doc.setPageSize(reader.getPageSizeWithRotation(1));
                doc.newPage();
                PdfImportedPage p = writer.getImportedPage(reader, i);
                cb.addTemplate(p, 1f, 0, 0, 1f, 0, 0);
            }
        }
        private PdfPCell CellValue(string val, bool border, Font f, int Align)
        {
            if (val == "0")
            {
                val = "  ";
            }
            if (val == "")
            {
                val = "  ";

            }
            Phrase p = new Phrase(new Chunk(val, f));
            PdfPCell c = new PdfPCell(p);
            if (!border)
                c.Border = 0;
            c.HorizontalAlignment = Align;
            c.VerticalAlignment = 5;
            return c;
        }
        private PdfPCell CellValue(string val, bool border, Font f, int Align, int colspan)
        {
            if (val == "0")
            {
                val = "  ";
            }
            if (val == "")
            {
                val = "  ";

            }
            Phrase p = new Phrase(new Chunk(val, f));
            PdfPCell c = new PdfPCell(p);
            if (!border)
                c.Border = 0;
            c.HorizontalAlignment = Align;
            c.VerticalAlignment = 5;
            c.Colspan = colspan;
            return c;
        }
        private PdfPCell CellValue(string val, bool border, Font f, int Align, bool isbackground)
        {

            if (val == "0")
            {
                val = "  ";
            }
            if (val == "")
            {
                val = "  ";

            }
            Phrase p = new Phrase(new Chunk(val, f));
            PdfPCell c = new PdfPCell(p);
            if (!border)
                c.Border = 0;
            if (isbackground)
                c.BackgroundColor = new Color(192, 192, 192);
            c.HorizontalAlignment = Align;
            c.VerticalAlignment = 5;
            return c;
        }
        private PdfPCell CellValue(string val, bool border, Font f, int Align, bool isbackground, int colspan)
        {
            if (val == "0")
            {
                val = "  ";
            }
            if (val == "")
            {
                val = "  ";

            }
            Phrase p = new Phrase(new Chunk(val, f));
            PdfPCell c = new PdfPCell(p);
            if (!border)
                c.Border = 0;
            if (isbackground)
                c.BackgroundColor = new Color(192, 192, 192);
            c.HorizontalAlignment = Align;
            c.VerticalAlignment = 5;
            c.Colspan = colspan;
            return c;
        }

    }
}
