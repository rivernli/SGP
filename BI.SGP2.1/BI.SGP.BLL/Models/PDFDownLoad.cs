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


namespace BI.SGP.BLL.Models
{
    public class PDFDownLoad
    {
        BaseFont bfTimes;
        Font terms;
        Font terms2;
        Font termsB;

        Font timesBlue;
        Document doc=new Document(PageSize.A4);
        DataTable dt;
        DataRow row;
        Color color = new Color(192,192,192);
        Color blue = new Color(0,0,255);
        Color black = new Color(0, 0, 0);
        
      
        int RLMargin = 25;
        float TBWidth = PageSize.A4.Width - 50;

        public string message = "";
        public void Dispose()
        {
            dt.Dispose();
        }
        public PDFDownLoad()
        {
            //get quoteData.
        }

        private Font pageFont()
        {
            
            Font f = FontFactory.getFont("Arial",9,Font.NORMAL);
            f.Size = 9f;
            return f;
        }
        private Font boldFont()
        {
            Font f = pageFont();
            f.setStyle(Font.BOLD.ToString());
            return f;
        }
        private Font bigBoldFont()
        {
            Font f = boldFont();
            f.Size = 10f;
            return f;
        }
        private Font smallBoldFont()
        {
            Font f = boldFont();
            f.Size = 8f;
            return f;
        }


        private void init()
        {

           // FontFactory.RegisterDirectories();

            timesBlue = FontFactory.getFont("Arial", 9, Font.NORMAL);
            timesBlue.Color = blue;

            terms = FontFactory.getFont("Arial", 9, Font.UNDERLINE);
            terms.Size = 8f;

            termsB = FontFactory.getFont("Arial", 9, Font.BOLD);
            termsB.Size = 8f;
            terms2 = FontFactory.getFont("Arial", 8, Font.NORMAL);
            terms2.Size = 8f;

            doc = new Document(PageSize.A4, RLMargin, RLMargin, 20, 20);
        }

        private void addHeader()
        {
            //string image_url = "http://mcnnt800.asia.ad.flextronics.com/Common/Content/Images/Multek_LOGO.PNG";
            string image_url = HttpContext.Current.Server.MapPath("~/tmp/logo.png");
            iTextSharp.text.Image png = iTextSharp.text.Image.getInstance(new Uri(image_url));
            png.scaleToFit(140, 140);
            png.Interpolation = true;
            //png.setAbsolutePosition(RLMargin, doc.PageSize.Height - 50);

          
           // doc.Add(png);

            PdfPTable table2 = new PdfPTable(2);
            table2.TotalWidth = TBWidth;
            //table2.LockedWidth = true;

            PdfPCell c1 = new PdfPCell(new Phrase(new Chunk(png,10,20)));
            c1.Border = 0;
           // c1.FixedHeight = 33f;
            table2.addCell(c1);

            PdfPCell ch = new PdfPCell(new Phrase(new Chunk("www.multek.com", bigBoldFont())));
            ch.HorizontalAlignment = Element.ALIGN_RIGHT;
            ch.VerticalAlignment = Element.ALIGN_MIDDLE;
            ch.Border = 0;
            ch.FixedHeight = 33f;
            table2.addCell(ch);
          //  table2.SpacingAfter = 4f;

            //ch.Rowspan
            Phrase p = new Phrase(new Chunk("Quotation", bigBoldFont()));
            //p.Font.Size = 14f;
            PdfPCell cell = new PdfPCell(p);

            cell.MinimumHeight = 18f;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.BackgroundColor = color;
            cell.Colspan = 2;
            table2.addCell(cell);
           // table2.SpacingAfter = 4f;
            doc.Add(table2);

        }
        private string showProgramme(string key)
        {
            string cur = "";

            if (key.Trim() != "" && key.Trim() != "0" && row["ProgramName"].ToString().Trim().Length > 0)
                cur = row["ProgramName"].ToString().Trim() + ";";
            return cur;
        }
        private string showCurrency(string key)
        {
            string cur = "";

            if (key.Trim() != ""&&key.Trim()!="0")
                cur = row["Currency"].ToString().Trim();
            return cur;
        }
        private string showCurrencyKey(string key)
        {
            string cur = "";
            if (key.Trim() != ""&&key.Trim()!="0")
                cur = row["Currency"].ToString().Trim() + " " + key.Trim();
            return cur;
        }
        private string Value(string key)
        {
            return row[key].ToString().Trim();
        }
        private PdfPCell getValue(string val)
        {
            if (val == "0")
            {
                val = " ";
            }
            if (val == "")
            {
                val = "  ";
            
            }
            Phrase p = new Phrase(new Chunk(val, timesBlue));
            PdfPCell c = new PdfPCell(p);
            c.PaddingLeft = 4f;
            c.FixedHeight = 15f;
            return c;
        }
        private PdfPCell addCell(string val, bool border, bool bold)
        {
            Phrase p = new Phrase(new Chunk(val, (bold) ? boldFont() : pageFont()));
            PdfPCell c = new PdfPCell(p);
            c.PaddingLeft = 4f;
            if (!border)
                c.Border = 0;
            c.FixedHeight = 15f;
            return c;
        }
        private PdfPCell addCell(string val, bool border, bool bold, iTextSharp.text.Color color)
        {
            PdfPCell c = addCell(val, border, bold);
            c.BackgroundColor = color;
            return c;
        }
        private PdfPCell titleCell(string s)
        {
            Font f = smallBoldFont();
            Phrase p = new Phrase(new Chunk(s, f));
            PdfPCell c = new PdfPCell(p);
            c.BackgroundColor = color;
            c.PaddingLeft = 4f;
            //c.VerticalAlignment = Element.ALIGN_TOP;
            c.FixedHeight = 15f;
            return c;
        }
        private PdfPCell titleCellC(string s)
        {
            PdfPCell c = titleCell(s);
            c.HorizontalAlignment = Element.ALIGN_CENTER;
            return c;
        }

        private void part1()
        {
            try
            {
                PdfPTable table = new PdfPTable(4);
                table.TotalWidth = TBWidth;
                //table.LockedWidth = true;
                float[] widths = new float[] { 15f, 45f, 15f, 25f };
                table.setWidths(widths);
                //table.SpacingAfter = 5f;
                table.addCell(addCell("Company :", false, true));
                string company = row["OEM"].ToString();
                company += (row["CEM"].ToString().Trim() != "") ? " - " + row["CEM"].ToString() : "";
                table.addCell(addCell(company, false, true));
                table.addCell(addCell("Date :", false, true));
                string  customerquotedate=row["CustomerQuoteDate"].ToString();
                if(string.IsNullOrEmpty(customerquotedate.Trim()))
                {
                    customerquotedate = DateTime.Now.ToString("dd-MMM-yyyy");
                }
                DateTime customerdate=new DateTime();
                DateTime.TryParse(customerquotedate,out customerdate);
                table.addCell(addCell(customerdate.ToString("dd-MMM-yyyy"), false, true));
                table.addCell(addCell("Attention :", false, true));
                table.addCell(addCell(row["CustomerContact"].ToString(), false, true));
                table.addCell(addCell("RFQ# :", false, true));


                string extnumber = row["ExtNumber"].ToString();

                if (row["ExtNumber"].ToString().IndexOf('-') > 0)
                { 
                  extnumber=row["ExtNumber"].ToString().Substring(0, row["ExtNumber"].ToString().IndexOf('-'));
                }

                table.addCell(addCell(extnumber, false, true));
                doc.Add(table);
                Paragraph p = new Paragraph(10f, ""+"\n", smallBoldFont());
                doc.Add(p);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                throw ex;
            }
        }

        private void part2()
        {
            Paragraph p = new Paragraph(10f, "Thank you for you inquiry, we are pleased to provide you quotation for our products as follow:", smallBoldFont());
            //p.SpacingAfter = 2f;
            //doc.Add(p);
            PdfPTable table = new PdfPTable(6);
            table.TotalWidth = TBWidth;
            //table.LockedWidth = true;
            float[] widths = new float[] { 28f, 15f, 11f, 9f, 10f, 27f };
            table.setWidths(widths);
            //table.SpacingAfter = 10f;
            PdfPCell c1 = new PdfPCell(p);
            //c1.FixedHeight = 33f;
            c1.Colspan = 6;
            c1.Border = 0;
            table.addCell(c1);

            table.addCell(titleCellC("Part Number"));
            table.addCell(titleCellC("Lead-Time"));
            table.addCell(titleCellC("Qty"));
            table.addCell(titleCellC("Currency"));
            table.addCell(titleCellC("Unit Price Each"));
            table.addCell(titleCellC("Remarks"));
            string programme = "";
            string PartInfor = "";
            if (row["ProgramName"].ToString().Trim() != "")
                programme = row["ProgramName"].ToString();
            if (row["CustomerPartNumber"].ToString().Trim() != "")
                PartInfor += ((PartInfor == "") ? "" : ", ") + row["CustomerPartNumber"].ToString().Trim();
            if (row["Revision"].ToString().Trim() != "")
                PartInfor += ((PartInfor == "") ? "" : ", ") + row["Revision"].ToString().Trim();

            table.addCell(getValue(PartInfor));
            table.addCell(getValue(row["LeadTime"].ToString()));
            table.addCell(getValue(row["Price1Qty"].ToString()));
            table.addCell(getValue(row["Currency"].ToString()));
            table.addCell(getValue(row["UnitPrice1"].ToString()));
            string rm = Value("Remark1");
            int len = rm.Length;
            table.addCell(getValue(rm));

            if (len > 100)
                table.addCell(addCell("  ", true, false));
            else
                table.addCell(addCell("  ", true, false));
            table.addCell(addCell("  ", true, false));
            table.addCell(getValue(row["Price2Qty"].ToString()));
            table.addCell(getValue(showCurrency(row["UnitPrice2"].ToString())));
            table.addCell(getValue(row["UnitPrice2"].ToString()));
            table.addCell(getValue(row["Remark2"].ToString()));

            if (len > 150)
                table.addCell(addCell("  ", true, false));
            else
                table.addCell(addCell("  ", true, false));
            table.addCell(addCell("  ", true, false));
            table.addCell(getValue(row["Price3Qty"].ToString()));
            table.addCell(getValue(showCurrency(row["UnitPrice3"].ToString())));
            table.addCell(getValue(row["UnitPrice3"].ToString()));
            table.addCell(getValue(row["Remark3"].ToString()));

            if (len > 200)
                table.addCell(addCell("  ", true, false));
            else
                table.addCell(addCell("   ", true, false));
            table.addCell(addCell("  ", true, false));
            table.addCell(getValue(row["Price4Qty"].ToString()));
            table.addCell(getValue(showCurrency(row["UnitPrice4"].ToString())));
            table.addCell(getValue(row["UnitPrice4"].ToString()));
            table.addCell(getValue(row["Remark4"].ToString()));
            if (len > 250)
                table.addCell(addCell("  ", true, false));
            else
                table.addCell(addCell("  ", true, false));
            table.addCell(addCell("  ", true, false));
            table.addCell(getValue(row["Price5Qty"].ToString()));
            table.addCell(getValue(showCurrency(row["UnitPrice5"].ToString())));
            table.addCell(getValue(row["UnitPrice5"].ToString()));
            table.addCell(getValue(row["Remark5"].ToString()));

            doc.Add(table);
        }
        private void part3()
        {
            Paragraph p = new Paragraph(10f, "" + "\n", smallBoldFont());
            doc.Add(p);
            PdfPTable table = new PdfPTable(4);
            table.TotalWidth = TBWidth;
            //table.LockedWidth = true;
            //table.SpacingAfter = 10f;
            float[] widths = new float[] { 25f, 25f, 25f, 25f };
            table.setWidths(widths);

            table.addCell(titleCellC("NRE Charge"));
            table.addCell(titleCellC("  "));
            table.addCell(titleCell("Min.Order Value"));
            table.addCell(getValue(showCurrencyKey(row["MOV"].ToString())));

            table.addCell(addCell("Set-up Charge", true, false));
            table.addCell(getValue(showCurrencyKey(row["SetupCharge"].ToString())));
            table.addCell(titleCell("Min.Order Qty"));
            table.addCell(getValue(row["MOQ"].ToString()));

            table.addCell(addCell("E-Testing Charge", true, false));
            table.addCell(getValue(showCurrencyKey(row["EtestCharge"].ToString())));
            table.addCell(titleCell("Shipment Term"));
            string shipterms=row["ShipmentTerms"].ToString();
            if(string.IsNullOrEmpty(shipterms.Trim()))
            {
                shipterms = "FCA HK - Free Carrier";
            }
            shipterms = shipterms+" "+row["Location"].ToString();
            table.addCell(getValue(shipterms));

            table.addCell(addCell("Tooling Charge", true, false));
            table.addCell(getValue(showCurrencyKey(row["ToolingCharge"].ToString())));
            table.addCell(titleCell("Payment Terms"));
            table.addCell(getValue(row["PayTerms"].ToString()));

            doc.Add(table);
        }
        private void part4()
        {
            Paragraph p = new Paragraph(10f, "" + "\n", smallBoldFont());
            doc.Add(p);
            PdfPTable table = new PdfPTable(4);
            table.TotalWidth = TBWidth;
            //table.LockedWidth = true;
            //table.SpacingAfter = 10f;
            float[] widths = new float[] { 20f, 30f, 20f, 30f };
            table.setWidths(widths);

            PdfPCell c = titleCell("Conditions");
            c.Colspan = 4;
            table.addCell(c);
            table.addCell(addCell("Card Size", true, false));
            string cs = "";
            if (row["UnitSizeLength"].ToString().Trim() + row["UnitSizeWidth"].ToString().Trim() != "")
                cs = row["UnitSizeLength"].ToString() + " x " + row["UnitSizeWidth"].ToString() + " " + row["UnitType"].ToString();
            table.addCell(getValue(cs));
            table.addCell(addCell("Material", true, false));
            //if (string.IsNullOrEmpty(row["LaminateType"].ToString()))
            //{
            //    table.addCell(getValue(row["MaterialCategory"].ToString() + "" + row["LaminateType"].ToString()));
            //}
            //else
            //{
            //    table.addCell(getValue(row["MaterialCategory"].ToString() + " (" + row["LaminateType"].ToString() + ")"));
            //}

            table.addCell(getValue(row["MaterialCategory"].ToString()));
            table.addCell(addCell("Array Size", true, false));
            string ars = " ";
            if (row["ArraySizeWidth"].ToString().Trim() + row["ArraySizeLength"].ToString().Trim() != "")
                ars = row["ArraySizeWidth"].ToString() + " x " + row["ArraySizeLength"].ToString() + " " + row["UnitType"].ToString() + " ";
            if (row["UnitPerArray"].ToString().Trim() != "")
                ars += "(" + row["UnitPerArray"].ToString() + " up)";
            table.addCell(getValue(ars));
            table.addCell(addCell("Thickness (mm/inch)", true, false));
            table.addCell(getValue(row["BoardThickness"].ToString() + " (" + row["UnitType"].ToString() + ")"));

            table.addCell(addCell("# of Layers", true, false));
            string viaStructure = row["ViaStructure"].ToString();
            if (!string.IsNullOrEmpty(viaStructure))
            {
                table.addCell(getValue(row["LayerCount"].ToString() + " [ " + viaStructure + " ] "));
            }
            else
            {
                table.addCell(getValue(row["LayerCount"].ToString()));
            }
            table.addCell(addCell("Copper (ext/int)", true, false));
            table.addCell(getValue(row["Copper"].ToString()));

            table.addCell(addCell("# of Holes", true, false));
            table.addCell(getValue(row["Holes"].ToString() + " " + row["UnitOrArray"].ToString()));
            table.addCell(addCell("Finish", true, false));
            table.addCell(getValue(row["Finishing"].ToString()));

            table.addCell(addCell("Smallest Hole", true, false));
            table.addCell(getValue(row["SmallestHole"].ToString() + " " + row["UnitType"].ToString()));
            table.addCell(addCell("Line & Space (ext)", true, false));
            string lno = row["LnO"].ToString();
            if (string.IsNullOrEmpty(lno.Trim()))
            {
                table.addCell(getValue(""));

            }
            else
            {
                table.addCell(getValue(row["LnO"].ToString() + " " + row["UnitType"].ToString()));
            }

            table.addCell(addCell("Impedance (Y/N)", true, false));
            table.addCell(getValue(row["Imped"].ToString()));
            table.addCell(addCell("Line & Space (int)", true, false));
            string lni = row["LnI"].ToString();
            if (string.IsNullOrEmpty(lni.Trim()))
            {
                table.addCell(getValue("  "));
            }
            else
            {
                table.addCell(getValue(row["LnI"].ToString() + " " + row["UnitType"].ToString()));
            
            }
            string s = "   ";
            if (Value("USize") != "")
                s = Value("USize") + " ";
            if (Value("UQty") != "")
                s += "(" + Value("UQty") + ")";
            table.addCell(addCell("Micro Via Size (Qty)", true, false));
            table.addCell(getValue(s));
            table.addCell(addCell("Outline Profiling", true, false));
            table.addCell(getValue(Value("Outline")));

            table.addCell(addCell("Blind/Buried Via", true, false));
            s = " ";
            if (Value("BlindSize") != "")
                s = Value("BlindSize") + " ";
            if (Value("BlindSize") != "")
                s += "(" + Value("BlindSize") + ")";
            s += "/";
            if (Value("BuriedSize") != "")
                s += Value("BuriedSize") + " ";
            if (Value("BuriedQty") != "")
                s += "(" + Value("BuriedQty") + ")";
            table.addCell(getValue(s));
            table.addCell(addCell("Internal Reference", true, false));
            string reference = row["PanelSizeWidth"].ToString().Trim() + row["PanelSizeLength"].ToString().Trim() + row["UnitPerWorkingPanel"].ToString().Trim();

            if (reference.Length > 2)
            {
                reference = reference.Substring(1, reference.Length - 1);
            }

            reference =  reference+ '(' + plant2Number(row["Building"].ToString().Trim().ToUpper()).ToString() + ')';

            table.addCell(getValue(reference + "  " + row["ProjectNumber"].ToString()));

            doc.Add(table);

            PdfPTable table2 = new PdfPTable(1);
            table2.TotalWidth = TBWidth;
            //table2.LockedWidth = true;
            //table2.SpacingAfter = 6f;
            table2.addCell(titleCell("Technical Remarks & Remarks"));
            PdfPCell cr = getValue(showProgramme(row["UnitPrice1"].ToString()) + Environment.NewLine + Value("TechnicalRemarks") + Environment.NewLine + Value("Remarks"));
            //cr.FixedHeight = 40f;
            cr.MinimumHeight = 40f;
            cr.VerticalAlignment = Element.ALIGN_MIDDLE;
            table2.addCell(cr);
            doc.Add(table2);

        }


        private int plant2Number(string building)
        {

            

            switch (building)
            {
                case "HK":
                case "VVI":
                    {
                        return 0;
                    }
                case "B1":
                    {
                        return 1; 
                    }
                case "B2":
                case "B2f":
                    {
                        return 2; 
                    }
                case "B3":
                    {
                        return 3; 
                    }
                case "B4":
                    {
                        return 4; 
                    }
                case "B5":
                    {
                        return 5; 
                    }
                default:
                    {
                        return -1;
                    }
               
            }
           
        
        
        }

        private void part5()
        {
            Paragraph t = new Paragraph(10f, "" + "\n", smallBoldFont());
            doc.Add(t);
            PdfPTable table = new PdfPTable(1);
            Paragraph p = new Paragraph(11f, "Term and Conditions", terms);

            PdfPCell c1 = new PdfPCell(p);
            c1.Border = 0;
            table.addCell(c1);
            Paragraph p2 = new Paragraph(10f, "Lead-time is quoted in business days (excludes weekends & holidays) and is subject to " +
                "the availability of material and may change within 24 hours notice." + Environment.NewLine +
                "Lead-time for non-standard materials, approximately three weeks and is subject to confirmation. Prices are valid for 30 days from date of quote." + Environment.NewLine +
                "Purchase order & data files, as stated below, must be received no later than 10:00AM for that day to be applied." + Environment.NewLine +
                "All quote information is subject to change after receipt of Gerber data files, fabrication drawings & specifications." +
                "Prices are only applicable for new purchase orders unless specified.  Acceptance of a purchase order is contingent upon " +
                "satisfactory credit approval.  Deliveries maybe subject to applicable local sales taxes, VAT.", terms2);
            //p2.SpacingAfter = 4f;
            PdfPCell c2 = new PdfPCell(p2);
            c2.Border = 0;
            table.addCell(c2);
            Paragraph p3 = new Paragraph(10f, "This quote is provided in accordance with the Multek Warranty Policy Terms and Conditions.", terms2);
            //p3.SpacingAfter = 6f;
            PdfPCell c3 = new PdfPCell(p3);
            c3.Border = 0;
            table.addCell(c3);

            Paragraph x1 = new Paragraph(11f, "Policy for Cancellations", terms);
            PdfPCell c4 = new PdfPCell(x1);
            c4.Border = 0;
            table.addCell(c4);
            Paragraph x2 = new Paragraph(11f, "- 45 calendar days for the order" + Environment.NewLine +
                "- 60 days for standard material" + Environment.NewLine +
                "- 90 days for \"special\" material ( i.e. 13 Teflon, Cyanate Ester, GTEK, etc..)", terms2);
            PdfPCell c5 = new PdfPCell(x2);
            c5.Border = 0;
            table.addCell(c5);
            //x2.SpacingAfter = 8f;

            //Paragraph ad = new Paragraph(11f, "ADDITIONAL NOTES:", termsB);
            //ad.SpacingBefore = 2f;
            //doc.Add(ad);


            //Paragraph ad2 = new Paragraph(11f);
            //ad2.Add(new Chunk("-Minimum Order per Delivery: ", termsB));
            //ad2.Add(new Chunk("= " + row["Currency"].ToString() + row["MOV"].ToString(), timesBlue));
            //ad2.Add(new Chunk(Environment.NewLine + "-" + row["Notes"].ToString() +
            //    Environment.NewLine + "-" + row["Notes2"].ToString(), termsB));
            /*
            Paragraph ad2 = new Paragraph(11f, "-Minimum Order per Delivery: = " + row["currency"].ToString()+ row["moq2"].ToString() +
                Environment.NewLine+"-"+row["note1"].ToString()+
                Environment.NewLine + "-" + row["note2"].ToString(), termsB);
            */
            //ad2.SpacingAfter = 5f;
            //doc.Add(ad2);

            Paragraph inf = new Paragraph(11f, "Thanks & Best regards,", termsB);
            //inf.SpacingAfter = 3f;
            PdfPCell c6 = new PdfPCell(inf);
            c6.Border = 0;
            table.addCell(c6);

            Paragraph inf0 = new Paragraph(12f, row["PrimaryContact"].ToString(), termsB);
            //inf0.SpacingAfter = 3f;
            PdfPCell c7 = new PdfPCell(inf0);
            c7.Border = 0;
            table.addCell(c7);

            Paragraph inf1 = new Paragraph(12f, "Direct Line : " + row["Phone"].ToString(), pageFont());
            PdfPCell c8 = new PdfPCell(inf1);
            c8.Border = 0;
            table.addCell(c8);
            Paragraph inf2 = new Paragraph(11f, "Direct Fax : " + row["Fax"].ToString(), pageFont());
            PdfPCell c9 = new PdfPCell(inf2);
            c9.Border = 0;
            table.addCell(c9);
            Paragraph inf3 = new Paragraph(11f, "Cell Phone : " + row["CellPhone"].ToString(), pageFont());
            PdfPCell c10 = new PdfPCell(inf3);
            c10.Border = 0;
            table.addCell(c10);
            Paragraph inf4 = new Paragraph(11f, "Email Address : " + row["Email"].ToString(), pageFont());
            PdfPCell c11 = new PdfPCell(inf4);
            c11.Border = 0;
            table.addCell(c11);

            doc.Add(table);
           
        
            //Paragraph inf6 = new Paragraph(11f, "1", pageFont());
            //doc.Add(inf6);

            //Paragraph inf7 = new Paragraph(11f, "Rev. B 11/10/2011", pageFont());
            //doc.Add(inf7);

            /*
    Policy for Cancellations

    */
        }
        private void setDT(int id)
        {
            string strSql = "SELECT a.*,b.* FROM V_SGP a left join Access_User b on a.PrimaryContact=b.Name where a.rfqid = @RFQID";
            dt = new DataTable();

            dt = DbHelperSQL.Query(strSql, new SqlParameter("@RFQID", id)).Tables[0];
           
        }


        public bool getPDF(ref MemoryStream mem, int quoteId, out string filename)
        {
            filename = "";
            bool pass = false;
            try
            {

                setDT(quoteId);
                row = dt.Rows[0];
                string extnumber = row["ExtNumber"].ToString();
                 if (row["ExtNumber"].ToString().IndexOf('-') > 0)
                { 
                  extnumber=row["ExtNumber"].ToString().Substring(0, row["ExtNumber"].ToString().IndexOf('-'));
                }


                filename = row["OEM"].ToString() + "_" +extnumber + "_" + row["CustomerPartNumber"].ToString().Trim();
                filename = filename.Replace("\"", "").Replace("'", "").Replace(";", "").Replace(":", "") + "_" + DateTime.Now.ToString("mmssffff");
                //filename = HttpUtility.HtmlEncode(filename) + "_"+DateTime.Now.ToString("mmss");///.Minute.ToString() + DateTime.Now.Second.ToString();
                if (dt.Rows.Count > 0)
                {
                    pass = true;

                    init();
                    PdfWriter writer = PdfWriter.getInstance(doc, mem);
                    doc.Open();
                    addHeader();
                    part1();
                    part2();
                    part3();
                    part4();
                    part5();
                    PdfContentByte cb = writer.DirectContent;

                    ColumnText ct1 = new ColumnText(cb);
                    ColumnText ct2 = new ColumnText(cb);
                    ColumnText ct3 = new ColumnText(cb);
                    Phrase p1 = new Phrase(10f, "Multek Warranty T&C ",pageFont());
                    Phrase p2 = new Phrase(10f, "1",pageFont());
                    Phrase p3 = new Phrase(10f, "Rev. B 11/10/2011",pageFont());
                    ct1.setSimpleColumn(p1, 65f, 35f, 595f, 0f,10f, Element.ALIGN_LEFT);
                    ct2.setSimpleColumn(p2, 380f, 35f, 240f, 0f, 10f, Element.ALIGN_CENTER);
                    ct3.setSimpleColumn(p3, 530f, 35f, 65f, 0f, 10f, Element.ALIGN_RIGHT);
                    //ct2.setSimpleColumn(p1, 300f, 36.23f, 363.22f, 36.23f, 10f, Element.ALIGN_CENTER);
                    //ct2.setSimpleColumn(p2, 350.22f, 36.23f, 239.22f, 36.23f, 10f, Element.ALIGN_CENTER);
                    //ct3.setSimpleColumn(p3, 550.02f, 36.23f, 510f, 36.23f,10f, Element.ALIGN_RIGHT);
                    //ct.SetSimpleColumn(dd,25,50,50,500,10,Element.ALIGN_CENTER);
                    ct1.go();
                    ct2.go();
                    ct3.go();

                    string file = HttpContext.Current.Server.MapPath("~/tmp/template_1.pdf");
                    PdfReader reader = new PdfReader(file);

                    for (int i = 2; i < reader.NumberOfPages + 1; i++)
                    {
                        doc.setPageSize(reader.getPageSizeWithRotation(1));
                        doc.newPage();
                        if (i == 1)
                        {
                            Chunk fileref = new Chunk(" ");
                            fileref.setLocalDestination(file);
                            doc.Add(fileref);
                        }
                        PdfImportedPage p = writer.getImportedPage(reader, i);
                        cb.addTemplate(p, 1f, 0, 0, 1f, 0, 0);
                    }
                    doc.Close();
                }
                dt.Dispose();
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                // throw ex;
            }
            finally
            {
                dt.Dispose();

            }
            return pass;
        }


    }


}
