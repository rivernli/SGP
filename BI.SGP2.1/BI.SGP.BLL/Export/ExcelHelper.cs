using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.BLL.Export
{
    public enum RenderType
    {
        Horizontal,
        Vertical
    }

    public class ExcelHelper
    {
        public static string ExportWorkflowTemplate(WFActivity activity)
        {
            List<WFActivityField> fields = activity.GetCheckFields();
            List<WFActivityField> mainFields = new List<WFActivityField>();
            Dictionary<string, List<WFActivityField>> subFields = new Dictionary<string, List<WFActivityField>>();
            Dictionary<string, ISheet> subSheets = new Dictionary<string, ISheet>();

            foreach (WFActivityField f in fields)
            {
                if (String.IsNullOrEmpty(f.SubDataType))
                {
                    mainFields.Add(f);
                }
                else
                {
                    if (!subFields.ContainsKey(f.SubDataType))
                    {
                        subFields.Add(f.SubDataType, new List<WFActivityField>());
                    }
                    subFields[f.SubDataType].Add(f);
                }
            }

            IWorkbook workbook = new XSSFWorkbook();

            ISheet sheet = workbook.CreateSheet("Primary");
            foreach (string k in subFields.Keys)
            {
                string strSql = "SELECT RelationFields FROM SYS_WFSubData WHERE TemplateID=@TemplateID AND TableName=@TableName";
                DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[] { new SqlParameter("@TemplateID", activity.TemplateID), new SqlParameter("@TableName", k) }).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string[] rfs = Convert.ToString(dt.Rows[0]["RelationFields"]).Split(',');
                    if (rfs != null && rfs.Length > 0)
                    {
                        List<WFActivityField> relationFields = new List<WFActivityField>();
                        foreach (string rf in rfs)
                        {
                            if (!String.IsNullOrEmpty(rf))
                            {
                                WFActivityField f = mainFields.Find(t => String.Compare(t.FieldName, rf, true) == 0);
                                if (f != null)
                                {
                                    relationFields.Add(f);
                                }
                            }
                        }
                        if (relationFields.Count > 0)
                        {
                            subFields[k].InsertRange(0, relationFields);
                        }
                    }
                }

                subSheets.Add(k, workbook.CreateSheet(k.Replace(" ", "_")));
            }

            int dsStartIndex = 0;
            ISheet sheetDS = workbook.CreateSheet("DataSource");

            int sheetIndex = 0;
            buildTemplateSheet(workbook, sheet, sheetDS, mainFields, ref dsStartIndex, sheetIndex);
            foreach (string k in subFields.Keys)
            {
                sheetIndex++;
                buildTemplateSheet(workbook, subSheets[k], sheetDS, subFields[k], ref dsStartIndex, sheetIndex);
            }

            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                workbook.Write(fileStream);
            }
            return tempFile;
        }

        private static void buildTemplateSheet(IWorkbook workbook, ISheet sheet, ISheet sheetDS, List<WFActivityField> fields, ref int dsStartIndex, int sheetIndex)
        {
            if (fields != null)
            {
                IRow rowColumn = sheet.CreateRow(0);
                for (int i = 0; i < fields.Count; i++)
                {
                    ICell cell = rowColumn.CreateCell(i);
                    string colName = String.IsNullOrWhiteSpace(fields[i].DisplayName) ? fields[i].FieldName : fields[i].DisplayName;
                    cell.SetCellValue(colName);
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    if (fields[i].IsRequired)
                    {
                        cellStyle.FillForegroundColor = IndexedColors.Yellow.Index;
                    }
                    else
                    {
                        cellStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                    }
                    cellStyle.FillPattern = FillPattern.SolidForeground;
                    cell.CellStyle = cellStyle;
                    sheet.AutoSizeColumn(i);

                    switch (fields[i].DataType)
                    {
                        case FieldInfo.DATATYPE_LIST:
                        case FieldInfo.DATATYPE_LIST_SQL:
                            string lstFormulaName = fields[i].FieldName + "fn";
                            int dsEndIndex = BuildDataSource(fields[i], sheetDS, dsStartIndex);
                            if (dsEndIndex > dsStartIndex)
                            {
                                IName name = sheet.Workbook.CreateName();
                                name.RefersToFormula = String.Format("'DataSource'!$A${0}:$A${1}", dsStartIndex + 1, dsEndIndex);
                                name.NameName = lstFormulaName;
                                name.SheetIndex = sheetIndex;
                                CellRangeAddressList addressList = new CellRangeAddressList(1, 1, i, i);
                                IDataValidationHelper dvHelper = sheet.GetDataValidationHelper();
                                IDataValidationConstraint dvConstraint = dvHelper.CreateFormulaListConstraint(lstFormulaName);
                                IDataValidation validation = dvHelper.CreateValidation(dvConstraint, addressList);
                                sheet.AddValidationData(validation);
                                dsStartIndex = dsEndIndex;
                            }
                            break;
                    }
                }
            }
        }

        public static string ExportSCMasterTemplate(string tableKey)
        {
            FieldCategory fc = new FieldCategory(tableKey);
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheetDS = workbook.CreateSheet(tableKey);
            IRow rowColumn = sheetDS.CreateRow(0);
            //int index = 0;
            FieldInfoCollecton fields = fc.EnableFields;
            for (int i = 0; i < fields.Count; i++)
            {
                ICell cell = rowColumn.CreateCell(i);
                string colName = String.IsNullOrWhiteSpace(fields[i].DisplayName) ? fields[i].FieldName : fields[i].DisplayName;
                cell.SetCellValue(colName);
                ICellStyle cellStyle = workbook.CreateCellStyle();

                if (fields[i].Options.Required)
                {
                    cellStyle.FillForegroundColor = IndexedColors.Yellow.Index;
                }
                else
                {
                    cellStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                }
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cell.CellStyle = cellStyle;
                sheetDS.AutoSizeColumn(i);

            }
            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                workbook.Write(fileStream);
            }
            return tempFile;
        }

        public static int BuildDataSource(WFActivityField field, ISheet sheetDS, int startIndex)
        {
            string strSql = "";
            switch (field.DataType)
            {
                case FieldInfo.DATATYPE_LIST:
                    strSql = String.Format("SELECT [Value] FROM [dbo].[SGP_KeyValue] WHERE [Key] = '{0}' AND ISNULL([Value],'')<>'' AND Status = 1 ORDER BY Sort", field.KeyValueSource);
                    break;
                case FieldInfo.DATATYPE_LIST_SQL:
                    strSql = field.KeyValueSource;
                    break;
            }
            if (strSql != "")
            {
                DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    sheetDS.CreateRow(startIndex).CreateCell(0).SetCellValue(Convert.ToString(dr["Value"]));
                    startIndex++;
                }
            }
            return startIndex;
        }

        /// <summary>
        /// 将DataSet转换成Excel，并写到数据流
        /// </summary>
        /// <param name="ds">DataSet数据源</param>
        public static string DataSetToExcel(DataSet ds)
        {
            IWorkbook workbook = new XSSFWorkbook();
            foreach (DataTable dt in ds.Tables)
            {
                CreateSheet(workbook, dt, dt.TableName);
            }

            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                workbook.Write(fileStream);
            }

            return tempFile;
        }

        /// <summary>
        /// 将DataTable转换成Excel
        /// </summary>
        /// <param name="dt">DataTable 数据源</param>
        public static string DataTableToExcel(DataTable dt)
        {
            return DataTableToExcel(dt, RenderType.Horizontal);
        }

        /// <summary>
        /// 将DataTable转换成Excel
        /// </summary>
        /// <param name="dt">DataTable 数据源</param>
        public static string DataTableToExcel(DataTable dt, RenderType renderType)
        {
            IWorkbook workbook = new XSSFWorkbook();

            CreateSheet(workbook, dt, dt.TableName, renderType);

            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                workbook.Write(fileStream);
            }

            return tempFile;
        }

        public static string DataSetToExcel(DataSet ds, RenderType renderType)
        {
            IWorkbook workbook = new XSSFWorkbook();
            CreateSheet(workbook, ds.Tables[0], ds.Tables[0].TableName, renderType);

            for (int i = 1; i < ds.Tables.Count; i++)
            {
                CreateSheet(workbook, ds.Tables[i], ds.Tables[i].TableName, renderType);
            }
            
            string tempFile = System.Web.HttpContext.Current.Server.MapPath("~/temp/") + Guid.NewGuid() + ".xlsx";
            using (var fileStream = FileHelper.CreateFile(tempFile))
            {
                workbook.Write(fileStream);
            }

            return tempFile;
        }

        /// <summary>
        /// 读取Excel到DataSet
        /// </summary>
        /// <param name="filePath">Excel文件的物理路径</param>
        /// <returns></returns>
        public static DataSet ReadExcel(string filePath)
        {
            return ReadExcel(File.Open(filePath, FileMode.Open, FileAccess.Read), true);
        }

        /// <summary>
        /// 读取Excel到DataSet
        /// </summary>
        /// <param name="filePath">Excel文件的物理路径</param>
        /// <param name="header">Excel的第一行是否作为表头</param>
        /// <returns></returns>
        public static DataSet ReadExcel(string filePath, bool header)
        {
            return ReadExcel(File.Open(filePath, FileMode.Open, FileAccess.Read), header);
        }

        /// <summary>
        /// 读取Excel到DataSet
        /// </summary>
        /// <param name="stream">Excel的文件流</param>
        /// <param name="header">Excel的第一行是否作为表头</param>
        /// <returns></returns>
        public static DataSet ReadExcel(Stream stream, bool header)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook = WorkbookFactory.Create(stream);
            int sheetsCount = workbook.NumberOfSheets;

            for (int sc = 0; sc < sheetsCount; sc++)
            {
                ISheet sheet = workbook.GetSheetAt(sc);
                DataTable dt = new DataTable();
                dt.TableName = sheet.SheetName;

                //获取Excel的行数
                int rowsCount = sheet.PhysicalNumberOfRows;
                if (rowsCount > 0)
                {
                    var rows = sheet.GetRowEnumerator();
                    rows.MoveNext();

                    IRow row = (IRow)rows.Current;

                    int colsnum = row.LastCellNum;

                    for (int i = 0; i < row.LastCellNum; i++)
                    {
                        ICell cell = row.GetCell(i);
                        string columnName;
                        if (!header || cell == null || string.IsNullOrWhiteSpace(cell.StringCellValue))
                        {
                            columnName = "C" + i.ToString();
                        }
                        else
                        {
                            columnName = cell.StringCellValue;
                        }
                        //string columnName = header ? cell.StringCellValue : "C" + i.ToString();
                        dt.Columns.Add(columnName.Trim(), typeof(string));
                    }

                    if (!header)
                    {
                        DataRow first = dt.NewRow();
                        for (int i = 0; i < row.LastCellNum; i++)
                        {
                            ICell cell = row.GetCell(i);
                            if (cell != null)
                            {
                                string val = cell.StringCellValue;
                                if (val != null) val = val.Trim(); // 去掉空格
                                first[i] = val;
                            }
                        }
                        dt.Rows.Add(first);
                    }

                    while (rows.MoveNext())
                    {
                        row = (IRow)rows.Current;
                        DataRow dataRow = dt.NewRow();

                        for (int i = 0; i < row.LastCellNum; i++)
                        {
                            if (i == colsnum)
                            {
                                dt.Columns.Add("C" + i);
                                colsnum++;
                            }

                            ICell cell = row.GetCell(i);
                            if (cell != null)
                            {
                                switch (cell.CellType)
                                {
                                    case CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(cell))
                                        {
                                            dataRow[i] = cell.DateCellValue;
                                        }
                                        else
                                        {
                                            dataRow[i] = cell.NumericCellValue;
                                        }
                                        break;
                                    case CellType.Formula:
                                        try
                                        {
                                            IFormulaEvaluator e = WorkbookFactory.CreateFormulaEvaluator(workbook);
                                            dataRow[i] = e.Evaluate(cell).StringValue;
                                        }
                                        catch (Exception)
                                        {
                                            cell.SetCellType(CellType.String);
                                            dataRow[i] = cell.StringCellValue;
                                        }
                                        break;
                                    default:
                                        {
                                            cell.SetCellType(CellType.String);
                                            string val = cell.StringCellValue;
                                            if (val != null) val = val.Trim(); // 去掉空格
                                            dataRow[i] = val;
                                            break;
                                        }
                                }
                            }

                        }
                        dt.Rows.Add(dataRow);
                    }

                }

                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    bool hasData = false; ;
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (Convert.ToString(dt.Rows[i][j]).Trim() != "")
                        {
                            hasData = true;
                            break;
                        }
                    }
                    if (!hasData)
                    {
                        dt.Rows.RemoveAt(i);
                    }
                }

                ds.Tables.Add(dt);
            }

            return ds;
        }

        private static void CreateSheet(IWorkbook workbook, DataTable dt, string sheetName)
        {
            CreateSheet(workbook, dt, sheetName, RenderType.Horizontal);
        }


        private static void CreateSheet(IWorkbook workbook, DataTable dt, string sheetName, RenderType renderType)
        {
            ISheet sheet = workbook.CreateSheet(sheetName);

            if (dt.Rows.Count > 10000)
            {
                renderType = RenderType.Horizontal;
            }

            if (renderType == RenderType.Vertical)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    IRow rowColumn = sheet.CreateRow(i);
                    ICell cell = rowColumn.CreateCell(0);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                }
                sheet.AutoSizeColumn(0);

                for (int rownum = 0; rownum < dt.Columns.Count; rownum++)
                {
                    for (int cellnum = 0; cellnum < dt.Rows.Count; cellnum++)
                    {
                        IRow row = sheet.GetRow(rownum);
                        ICell cell = row.CreateCell(cellnum + 1);

                        SetCellValue(cell, dt.Rows[cellnum][rownum], cellnum, rownum, null);
                        if (rownum == 0)
                        {
                            sheet.AutoSizeColumn(cellnum + 1);
                        }
                    }
                }
            }
            else
            {
                IRow rowColumn = sheet.CreateRow(0);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = rowColumn.CreateCell(i);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                }

                for (int rownum = 0; rownum < dt.Rows.Count; rownum++)
                {
                    IRow row = sheet.CreateRow(rownum + 1);
                    for (int cellnum = 0; cellnum < dt.Columns.Count; cellnum++)
                    {
                        ICell cell = row.CreateCell(cellnum);
                        SetCellValue(cell, dt.Rows[rownum][cellnum], rownum, cellnum, null);
                    }
                }
            }

        }

        private static void SetCellValue(ICell cell, object cellSource, int rownum, int cellnum, ICellStyle cellStyle)
        {
            string cellValue = cellSource.ToString();
            switch (cellSource.GetType().ToString())
            {
                case "System.DateTime":
                    DateTime dateV;
                    if (DateTime.TryParse(cellValue, out dateV))
                    {
                        cell.SetCellValue(dateV);
                        if (cellStyle == null)
                        {
                            IDataFormat format = cell.Sheet.Workbook.CreateDataFormat();
                            cellStyle = cell.Sheet.Workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("m/d/yyyy hh:mm:ss");
                        }
                    }
                    else
                    {
                        cell.SetCellValue(cellValue);
                    }
                    break;
                case "System.Boolean":
                    bool boolV = false;
                    if (bool.TryParse(cellValue, out boolV))
                    {
                        cell.SetCellValue(boolV);
                    }
                    else
                    {
                        cell.SetCellValue(cellValue);
                    }
                    break;
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                case "System.Decimal":
                case "System.Double":
                    double doubV = 0;
                    if (double.TryParse(cellValue, out doubV))
                    {
                        cell.SetCellValue(doubV);
                    }
                    else
                    {
                        cell.SetCellValue(cellValue);
                    }
                    break;
                default:
                    cell.SetCellValue(cellValue);
                    break;
            }

            if (cellStyle != null)
            {
                cell.CellStyle = cellStyle;
            }
        }



        /// <summary>
        /// 读取Customer固定Excel表格到DataSet
        /// </summary>
        /// <param name="filePath">Excel文件的物理路径</param>
        /// <returns></returns>
        public static DataSet ReadCustomerExcel(string filePath)
        {
            return ReadCustomerExcel(File.Open(filePath, FileMode.Open, FileAccess.Read), true);
        }

        /// <summary>
        /// 读取Customer固定Excel表格到DataSet
        /// </summary>
        /// <param name="stream">Excel的文件流</param>
        /// <param name="header">Excel的第一行是否作为表头</param>
        /// <returns></returns>
        public static DataSet ReadCustomerExcel(Stream stream, bool header)
        {
            FieldInfoCollecton fields = FieldCategory.GetAllFields(FieldCategory.Category_TYPE_CUSTOMERPROFILE);

            DataSet ds = new DataSet();
            IWorkbook workbook = WorkbookFactory.Create(stream);
            int sheetsCount = workbook.NumberOfSheets;

            DataTable dt = new DataTable();
            dt.TableName = "CustomerProfile";

            foreach (FieldInfo Customerfiled in fields)
            {
                dt.Columns.Add(Customerfiled.FieldName, typeof(string));
            }
            ds.Tables.Add(dt);

            for (int sc = 0; sc < sheetsCount; sc++)
            {
                ISheet sheet = workbook.GetSheetAt(sc);

                //获取Excel的行数
                int rowsCount = sheet.PhysicalNumberOfRows;
                if (rowsCount > 0)
                {                   

                    DataRow dataRow = dt.NewRow();

                    IRow row = sheet.GetRow(2);
                    ICell cell = row.GetCell(2);
                    dataRow[0] = cell.StringCellValue.Trim();//Customer
                    cell = row.GetCell(4);
                    dataRow[1] = cell.StringCellValue.Trim();//Market

                    row = sheet.GetRow(3);
                    cell = row.GetCell(2);
                    dataRow[2] = cell.StringCellValue.Trim();//Location
                    cell = row.GetCell(4);
                    dataRow[3] = cell.StringCellValue.Trim();//Flextronics Business Segment
                    cell = row.GetCell(6);
                    dataRow[4] = cell.StringCellValue.Trim();//Sub-Segment

                    row = sheet.GetRow(4);
                    cell = row.GetCell(2);
                    dataRow[5] = cell.StringCellValue.Trim();//Ticker Symbol
                    cell = row.GetCell(4);
                    dataRow[6] = cell.NumericCellValue;//Customer's Annual Revenue
                    cell = row.GetCell(6);
                    dataRow[7] = cell.StringCellValue.Trim();//Multek GAM/BDM

                    row = sheet.GetRow(5);
                    cell = row.GetCell(2);
                    dataRow[8] = cell.StringCellValue.Trim();//Customer's Web Site
                    cell = row.GetCell(4);
                    dataRow[9] = cell.StringCellValue.Trim();//Revenue Currency 
                    cell = row.GetCell(6);
                    dataRow[10] = cell.StringCellValue.Trim();//Inside Sales Lead

                    row = sheet.GetRow(6);
                    cell = row.GetCell(2);
                    dataRow[11] = cell.NumericCellValue;//TAM
                    cell = row.GetCell(4);
                    dataRow[12] = cell.StringCellValue.Trim();//Revenue Year
                    cell = row.GetCell(6);
                    dataRow[13] = cell.StringCellValue.Trim();//FAE

                    row = sheet.GetRow(7);
                    cell = row.GetCell(2);
                    dataRow[14] = cell.NumericCellValue;//Multek SAM
                    cell = row.GetCell(4);
                    dataRow[15] = cell.StringCellValue.Trim();//Customer's Profitability
                    cell = row.GetCell(6);
                    dataRow[16] = cell.StringCellValue.Trim();//Customer Service

                    row = sheet.GetRow(8);
                    cell = row.GetCell(2);
                    dataRow[17] = cell.StringCellValue.Trim();//Multek Market Share of SAM
                    cell = row.GetCell(4);
                    dataRow[18] = cell.StringCellValue.Trim();//Key Purchasing Contact         
                    row = sheet.GetRow(10);
                    cell = row.GetCell(6);
                    dataRow[19] = cell.StringCellValue.Trim();//Flex GAM 
                    
                    row = sheet.GetRow(9);
                    cell = row.GetCell(2);
                    dataRow[20] = cell.NumericCellValue;//FY'14 Revenue
                    cell = row.GetCell(4);
                    dataRow[21] = cell.StringCellValue.Trim();//Purchasing Manager        

                    row = sheet.GetRow(10);
                    cell = row.GetCell(2);
                    dataRow[22] = cell.NumericCellValue;//FY'15 Forecast
                    cell = row.GetCell(4);
                    dataRow[23] = cell.StringCellValue.Trim();//Supplier Quality Engineer

                    row = sheet.GetRow(11);
                    cell = row.GetCell(2);
                    dataRow[24] = cell.NumericCellValue;//FY'16 Forecast
                    cell = row.GetCell(4);
                    dataRow[25] = cell.StringCellValue.Trim();//VP of Supply Chain / Purchasing

                    row = sheet.GetRow(12);
                    cell = row.GetCell(2);
                    dataRow[26] = cell.NumericCellValue;//FY'17 Forecast
                    cell = row.GetCell(4);
                    dataRow[27] = cell.StringCellValue.Trim();//President

                    row = sheet.GetRow(13);
                    cell = row.GetCell(2);
                    dataRow[28] = cell.NumericCellValue;//FY'18 Forecast
                    
                    row = sheet.GetRow(15);
                    cell = row.GetCell(2);
                    dataRow[29] = cell.StringCellValue.Trim();//Programs Supported 

                    row = sheet.GetRow(16);
                    cell = row.GetCell(4);
                    dataRow[30] = cell.StringCellValue.Trim();//Multek Approved Facilities

                    row = sheet.GetRow(17);
                    cell = row.GetCell(2);
                    dataRow[31] = cell.StringCellValue.Trim();//Multek Competitors

                    row = sheet.GetRow(19);
                    cell = row.GetCell(2);
                    dataRow[32] = cell.StringCellValue.Trim();//Strategy for Growth

                    dt.Rows.Add(dataRow);

                }
            }

            return ds;
        }
        /// <summary>
        /// Customer
        /// </summary>
        /// <returns></returns>
        public static void CustomerToExcel(DataTable dt, Stream stream)
        {

            //记录条数
            double doubV = 0;
            XSSFWorkbook workbook = new XSSFWorkbook();
            XSSFSheet sheet1 = (XSSFSheet)workbook.CreateSheet("Sheet1");
            sheet1.SetColumnWidth(0, 8000);
            sheet1.SetColumnWidth(1, 8000);
            sheet1.SetColumnWidth(2, 8000);
            sheet1.SetColumnWidth(3, 8000);
            sheet1.SetColumnWidth(4, 8000);
            sheet1.SetColumnWidth(5, 8000);

            //----------样式-----------
            //Titel
            XSSFFont fontTitle = (XSSFFont)workbook.CreateFont();
            fontTitle.Boldweight = (short)FontBoldWeight.Bold;
            XSSFCellStyle headStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            headStyle.Alignment = HorizontalAlignment.Center;
            headStyle.SetFont(fontTitle);
            headStyle.VerticalAlignment = VerticalAlignment.Center;

            //Lable
            XSSFFont fontLable = (XSSFFont)workbook.CreateFont();
            XSSFCellStyle styleLable = (XSSFCellStyle)workbook.CreateCellStyle();
            styleLable.Alignment = HorizontalAlignment.Right;
            styleLable.SetFont(fontLable);
            styleLable.VerticalAlignment = VerticalAlignment.Center;

            //Content
            XSSFFont fontContent = (XSSFFont)workbook.CreateFont();
            fontContent.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            XSSFCellStyle styleContent = (XSSFCellStyle)workbook.CreateCellStyle();
            styleContent.Alignment = HorizontalAlignment.Center;
            styleContent.SetFont(fontContent);
            styleContent.VerticalAlignment = VerticalAlignment.Center;

            //Content2
            XSSFFont fontContent2 = (XSSFFont)workbook.CreateFont();
            fontContent2.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            XSSFCellStyle styleContent2 = (XSSFCellStyle)workbook.CreateCellStyle();
            styleContent2.Alignment = HorizontalAlignment.Left;
            styleContent2.SetFont(fontContent);
            styleContent2.VerticalAlignment = VerticalAlignment.Center;

            //Content3 美元
            XSSFFont fontContentUSD = (XSSFFont)workbook.CreateFont();
            fontContentUSD.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            XSSFCellStyle styleContentUSD = (XSSFCellStyle)workbook.CreateCellStyle();
            styleContentUSD.Alignment = HorizontalAlignment.Right;
            styleContentUSD.SetFont(fontContentUSD);
            styleContentUSD.VerticalAlignment = VerticalAlignment.Center;
            XSSFDataFormat format1 = (XSSFDataFormat)workbook.CreateDataFormat();
            styleContentUSD.DataFormat = format1.GetFormat("$#,##0");
            //--------------------------

            foreach (DataRow dr in dt.Rows)
            {
                XSSFRow row0 = (XSSFRow)sheet1.CreateRow(0);
                row0.HeightInPoints = 30;
                row0.CreateCell(0).SetCellValue("Customer Profile");
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));
                row0.GetCell(0).CellStyle = headStyle;

                XSSFRow row1 = (XSSFRow)sheet1.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Customer:");
                row1.CreateCell(1).SetCellValue(Convert.ToString(dr["Customer"]));
                row1.CreateCell(2).SetCellValue("Market:");
                row1.CreateCell(3).SetCellValue(Convert.ToString(dr["Market"]));
                row1.CreateCell(4).SetCellValue("MULTEK Team");
                row1.CreateCell(5).SetCellValue("");
                row1.GetCell(0).CellStyle = styleLable;
                row1.GetCell(2).CellStyle = styleLable;
                row1.GetCell(4).CellStyle = styleLable;
                row1.GetCell(1).CellStyle = styleContent;
                row1.GetCell(3).CellStyle = styleContent;
                row1.GetCell(5).CellStyle = styleContent;

                XSSFRow row2 = (XSSFRow)sheet1.CreateRow(2);
                row2.CreateCell(0).SetCellValue("Location:");
                row2.CreateCell(1).SetCellValue(Convert.ToString(dr["Location"]));
                row2.CreateCell(2).SetCellValue("Flextronics Business Segment:");
                row2.CreateCell(3).SetCellValue(Convert.ToString(dr["FlextronicsBusinessSegment"]));                
                row2.CreateCell(4).SetCellValue("Sub-Segment:");
                row2.CreateCell(5).SetCellValue(Convert.ToString(dr["SubSegment"]));
                row2.GetCell(0).CellStyle = styleLable;
                row2.GetCell(2).CellStyle = styleLable;
                row2.GetCell(4).CellStyle = styleLable;
                row2.GetCell(1).CellStyle = styleContent;
                row2.GetCell(3).CellStyle = styleContent;
                row2.GetCell(5).CellStyle = styleContent;

                XSSFRow row3 = (XSSFRow)sheet1.CreateRow(3);
                row3.CreateCell(0).SetCellValue("Ticker Symbol:");
                row3.CreateCell(1).SetCellValue(Convert.ToString(dr["TickerSymbol"]));
                row3.CreateCell(2).SetCellValue("Customer's Annual Revenue:");
                if (double.TryParse(Convert.ToString(dr["CustomersAnnualRevenue"]), out doubV))
                {
                    row3.CreateCell(3).SetCellValue(doubV);
                    row3.GetCell(3).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row3.CreateCell(3).SetCellValue("");
                }
                row3.CreateCell(4).SetCellValue("Multek GAM/BDM:");
                row3.CreateCell(5).SetCellValue(Convert.ToString(dr["MultekGAM"]));
                row3.GetCell(0).CellStyle = styleLable;
                row3.GetCell(2).CellStyle = styleLable;
                row3.GetCell(4).CellStyle = styleLable;
                row3.GetCell(1).CellStyle = styleContent;
                row3.GetCell(5).CellStyle = styleContent;

                XSSFRow row4 = (XSSFRow)sheet1.CreateRow(4);
                row4.CreateCell(0).SetCellValue("Customer's Web Site:");
                row4.CreateCell(1).SetCellValue(Convert.ToString(dr["CustomersWebSite"]));
                row4.CreateCell(2).SetCellValue("Revenue Currency:");
                row4.CreateCell(3).SetCellValue(Convert.ToString(dr["RevenueCurrency"]));
                row4.CreateCell(4).SetCellValue("Inside Sales Lead:");
                row4.CreateCell(5).SetCellValue(Convert.ToString(dr["MultekSalesPerson"]));
                row4.GetCell(0).CellStyle = styleLable;
                row4.GetCell(2).CellStyle = styleLable;
                row4.GetCell(4).CellStyle = styleLable;
                row4.GetCell(1).CellStyle = styleContent;
                row4.GetCell(3).CellStyle = styleContent;
                row4.GetCell(5).CellStyle = styleContent;

                XSSFRow row5 = (XSSFRow)sheet1.CreateRow(5);
                row5.CreateCell(0).SetCellValue("TAM:");
                if (double.TryParse(Convert.ToString(dr["TAM"]), out doubV))
                {
                    row5.CreateCell(1).SetCellValue(doubV);
                    row5.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row5.CreateCell(1).SetCellValue("");
                }
                row5.CreateCell(2).SetCellValue("Revenue Year:");
                row5.CreateCell(3).SetCellValue(Convert.ToString(dr["RevenueYear"]));
                row5.CreateCell(4).SetCellValue("FAE:");
                row5.CreateCell(5).SetCellValue(Convert.ToString(dr["FAE"]));
                row5.GetCell(0).CellStyle = styleLable;
                row5.GetCell(2).CellStyle = styleLable;
                row5.GetCell(4).CellStyle = styleLable;
                row5.GetCell(3).CellStyle = styleContent;
                row5.GetCell(5).CellStyle = styleContent;

                XSSFRow row6 = (XSSFRow)sheet1.CreateRow(6);
                row6.CreateCell(0).SetCellValue("Multek SAM:");
                if (double.TryParse(Convert.ToString(dr["MultekSAM"]), out doubV))
                {
                    row6.CreateCell(1).SetCellValue(doubV);
                    row6.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row6.CreateCell(1).SetCellValue("");
                }
                row6.CreateCell(2).SetCellValue("Customer's Profitability:");
                row6.CreateCell(3).SetCellValue(Convert.ToString(dr["CustomersProfitability"]));
                row6.CreateCell(4).SetCellValue("Customer Service:");
                row6.CreateCell(5).SetCellValue(Convert.ToString(dr["CustomerService"]));
                row6.GetCell(0).CellStyle = styleLable;
                row6.GetCell(2).CellStyle = styleLable;
                row6.GetCell(4).CellStyle = styleLable;
                row6.GetCell(3).CellStyle = styleContent;
                row6.GetCell(5).CellStyle = styleContent;


                XSSFRow row7 = (XSSFRow)sheet1.CreateRow(7);
                row7.CreateCell(0).SetCellValue("Multek Market Share of SAM:");
                row7.CreateCell(1).SetCellValue(Convert.ToString(dr["MultekMarketShareOfSAM"]));
                row7.CreateCell(2).SetCellValue("Key Purchasing Contact:");
                row7.CreateCell(3).SetCellValue(Convert.ToString(dr["KeyPurchasingContact"]));
                row7.CreateCell(4).SetCellValue("Flex GAM:");
                row7.CreateCell(5).SetCellValue(Convert.ToString(dr["FlextronicsBDMGAM"]));
                row7.GetCell(0).CellStyle = styleLable;
                row7.GetCell(2).CellStyle = styleLable;
                row7.GetCell(4).CellStyle = styleLable;
                row7.GetCell(1).CellStyle = styleContent;
                row7.GetCell(3).CellStyle = styleContent;
                row7.GetCell(5).CellStyle = styleContent;

                XSSFRow row8 = (XSSFRow)sheet1.CreateRow(8);
                row8.CreateCell(0).SetCellValue("FY'14 Revenue:");
                if (double.TryParse(Convert.ToString(dr["FY14Revenue"]), out doubV))
                {
                    row8.CreateCell(1).SetCellValue(doubV);
                    row8.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row8.CreateCell(1).SetCellValue("");
                }
                row8.CreateCell(2).SetCellValue("Purchasing Manager:");
                row8.CreateCell(3).SetCellValue(Convert.ToString(dr["PurchasingManager"]));
                row8.CreateCell(4).SetCellValue("");
                row8.CreateCell(5).SetCellValue("");
                row8.GetCell(0).CellStyle = styleLable;
                row8.GetCell(2).CellStyle = styleLable;
                row8.GetCell(3).CellStyle = styleContent;

                XSSFRow row9 = (XSSFRow)sheet1.CreateRow(9);
                row9.CreateCell(0).SetCellValue("FY'15 Forecast:");
                if (double.TryParse(Convert.ToString(dr["FY15Forecast"]), out doubV))
                {
                    row9.CreateCell(1).SetCellValue(doubV);
                    row9.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row9.CreateCell(1).SetCellValue("");
                }
                row9.CreateCell(2).SetCellValue("Supplier Quality Engineer:");
                row9.CreateCell(3).SetCellValue(Convert.ToString(dr["SupplierQualityEngineer"]));
                row9.CreateCell(4).SetCellValue("");
                row9.CreateCell(5).SetCellValue("");
                row9.GetCell(0).CellStyle = styleLable;
                row9.GetCell(2).CellStyle = styleLable;
                row9.GetCell(3).CellStyle = styleContent;


                XSSFRow row10 = (XSSFRow)sheet1.CreateRow(10);
                row10.CreateCell(0).SetCellValue("FY'16 Forecast:");
                if (double.TryParse(Convert.ToString(dr["FY16Forecast"]), out doubV))
                {
                    row10.CreateCell(1).SetCellValue(doubV);
                    row10.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row10.CreateCell(1).SetCellValue("");
                }
                row10.CreateCell(2).SetCellValue("VP of Supply Chain / Purchasing:");
                row10.CreateCell(3).SetCellValue(Convert.ToString(dr["VPofSupplyChainPurchasing"]));
                row10.CreateCell(4).SetCellValue("");
                row10.CreateCell(5).SetCellValue("");
                row10.GetCell(0).CellStyle = styleLable;
                row10.GetCell(2).CellStyle = styleLable;
                row10.GetCell(3).CellStyle = styleContent;


                XSSFRow row11 = (XSSFRow)sheet1.CreateRow(11);
                row11.CreateCell(0).SetCellValue("FY'17 Forecast:");
                if (double.TryParse(Convert.ToString(dr["FY17Forecast"]), out doubV))
                {
                    row11.CreateCell(1).SetCellValue(doubV);
                    row11.GetCell(1).CellStyle = styleContentUSD;//格式化显示
                }
                else
                {
                    row11.CreateCell(1).SetCellValue("");
                }
                row11.CreateCell(2).SetCellValue("President:");
                row11.CreateCell(3).SetCellValue(Convert.ToString(dr["President"]));
                row11.CreateCell(4).SetCellValue("");
                row11.CreateCell(5).SetCellValue("");
                row11.GetCell(0).CellStyle = styleLable;
                row11.GetCell(2).CellStyle = styleLable;
                row11.GetCell(3).CellStyle = styleContent;


                XSSFRow row13 = (XSSFRow)sheet1.CreateRow(12);
                row13.CreateCell(0).SetCellValue("Programs Supported:");
                row13.CreateCell(1).SetCellValue(Convert.ToString(dr["ProgramsSupported"]));
                row13.CreateCell(2).SetCellValue("");
                row13.CreateCell(3).SetCellValue("");
                row13.CreateCell(4).SetCellValue("");
                row13.CreateCell(5).SetCellValue("");
                sheet1.AddMergedRegion(new CellRangeAddress(12, 12, 1, 5));
                row13.GetCell(0).CellStyle = styleLable;
                row13.GetCell(2).CellStyle = styleLable;
                row13.GetCell(1).CellStyle = styleContent2;
                row13.GetCell(3).CellStyle = styleContent;

                XSSFRow row14 = (XSSFRow)sheet1.CreateRow(13);
                row14.CreateCell(0).SetCellValue("Multek Approved Facilities:");
                row14.CreateCell(1).SetCellValue(Convert.ToString(dr["MultekApprovedFacilities"]));
                row14.CreateCell(2).SetCellValue("");
                row14.CreateCell(3).SetCellValue("");
                row14.CreateCell(4).SetCellValue("");
                row14.CreateCell(5).SetCellValue("");
                row14.GetCell(0).CellStyle = styleLable;
                row14.GetCell(1).CellStyle = styleContent;

                XSSFRow row15 = (XSSFRow)sheet1.CreateRow(14);
                row15.CreateCell(0).SetCellValue("Multek Competitors:");
                row15.CreateCell(1).SetCellValue(Convert.ToString(dr["MultekCompetitors"]));
                row15.CreateCell(2).SetCellValue("");
                row15.CreateCell(3).SetCellValue("");
                row15.CreateCell(4).SetCellValue("");
                row15.CreateCell(5).SetCellValue("");
                sheet1.AddMergedRegion(new CellRangeAddress(14, 14, 1, 5));
                row15.GetCell(0).CellStyle = styleLable;
                row15.GetCell(1).CellStyle = styleContent2;

                XSSFRow row17 = (XSSFRow)sheet1.CreateRow(15);
                row17.CreateCell(0).SetCellValue("Strategy for Growth:");
                row17.CreateCell(1).SetCellValue(Convert.ToString(dr["StrategyforGrowth"]));
                row17.CreateCell(2).SetCellValue("");
                row17.CreateCell(3).SetCellValue("");
                row17.CreateCell(4).SetCellValue("");
                row17.CreateCell(5).SetCellValue("");
                row17.HeightInPoints = 50;
                sheet1.AddMergedRegion(new CellRangeAddress(15, 15, 1, 5));
                row17.GetCell(0).CellStyle = styleLable;
                row17.GetCell(1).CellStyle = styleContent2;

            }
            //MemoryStream ms = new MemoryStream();
            workbook.Write(stream);
            //ms.Seek(0, SeekOrigin.Begin);
            //return ms;
        }

        /// <summary>
        /// Customer State Info
        /// </summary>
        /// <returns></returns>
        public static void CustomerStateInfoToExcel(DataTable dt, Stream stream, string Category)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Cisco Hub Management");

            ICellStyle style = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.Boldweight = short.MaxValue;
            style.SetFont(font);
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;

            ICellStyle dateStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            if (Category == "Amount")
            {
                dateStyle.DataFormat = format.GetFormat("$#,##0");
            }
            else
            {
                dateStyle.DataFormat = format.GetFormat("0");
            }
            
            IRow dataRow0 = sheet.CreateRow(0);
            IRow dataRow1 = sheet.CreateRow(1);

            ICell c = dataRow1.CreateCell(0);
            c.SetCellValue("Customer");
            c.CellStyle = style;

            c = dataRow1.CreateCell(1);
            c.SetCellValue("P/N");
            c.CellStyle = style;

            c = dataRow1.CreateCell(2);
            c.SetCellValue("Project");
            c.CellStyle = style;

            c = dataRow1.CreateCell(3);
            c.SetCellValue("Split");
            c.CellStyle = style;

            c = dataRow1.CreateCell(4);
            c.SetCellValue("Over 90 days");
            c.CellStyle = style;

            c = dataRow0.CreateCell(4);
            c.SetCellValue("Hub Inventory Aging(A)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(5);
            c.SetCellValue("Over 60 days");
            c.CellStyle = style;

            c = dataRow1.CreateCell(6);
            c.SetCellValue("Over 30 days");
            c.CellStyle = style;

            c = dataRow1.CreateCell(7);
            c.SetCellValue("30 days or less");
            c.CellStyle = style;

            c = dataRow1.CreateCell(8);
            c.SetCellValue("Total Aging");
            c.CellStyle = style;

            c = dataRow0.CreateCell(9);
            c.SetCellValue("Backlog(B)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(9);
            c.SetCellValue("Current Period");
            c.CellStyle = style;

            c = dataRow1.CreateCell(10);
            c.SetCellValue("Next Period(+1)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(11);
            c.SetCellValue("Next Next Period(+2)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(12);
            c.SetCellValue("GHub");
            c.CellStyle = style;

            c = dataRow1.CreateCell(13);
            c.SetCellValue("Site Inventory");
            c.CellStyle = style;

            c = dataRow0.CreateCell(14);
            c.SetCellValue("Demand(C)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(14);
            c.SetCellValue("Current Period");
            c.CellStyle = style;

            c = dataRow1.CreateCell(15);
            c.SetCellValue("Next Period(+1)");
            c.CellStyle = style;

            c = dataRow1.CreateCell(16);
            c.SetCellValue("Thirth Period");
            c.CellStyle = style;

            c = dataRow1.CreateCell(17);
            c.SetCellValue("Fourth Period");
            c.CellStyle = style;

            c = dataRow1.CreateCell(18);
            c.SetCellValue("Fifth Period");
            c.CellStyle = style;

            c = dataRow1.CreateCell(19);
            c.SetCellValue("Sixth Period");
            c.CellStyle = style;
            

            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 4, 8));
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 9, 11));
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 14, 19));


            for (int i = 0; i < 11; i++)
            {
                sheet.SetColumnWidth(i, 3840);
            }

            int dcIndex = 2;

            foreach (DataRow dr in dt.Rows)
            {
                IRow dataRow = sheet.CreateRow(dcIndex);

                c = dataRow.CreateCell(0);
                c.SetCellValue(Convert.ToString(dr["site"]));

                c = dataRow.CreateCell(1);
                c.SetCellValue(Convert.ToString(dr["cPartNo"]));

                c = dataRow.CreateCell(2);
                c.SetCellValue(Convert.ToString(dr["mitem"]));

                c = dataRow.CreateCell(3);
                c.SetCellValue(Convert.ToString(dr["Split"]));

                c = dataRow.CreateCell(4);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["Hub90days"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["Hub90days"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(5);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["Hub60days"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["Hub60days"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(6);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["hub30days"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["hub30days"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(7);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["Hub0days"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["Hub0days"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(8);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["TotalAgent"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["TotalAgent"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(9);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["BacklogCurrentP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["BacklogCurrentP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(10);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["BacklogNextP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["BacklogNextP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(11);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["BacklogThirdP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["BacklogThirdP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(12);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["GHub"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["GHub"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(13);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["SiteInventory"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["SiteInventory"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(14);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandCurrentP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandCurrentP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(15);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandNextP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandNextP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(16);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandThirthP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandThirthP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(17);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandFourthP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandFourthP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(18);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandFifthP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandFifthP"]));
                    c.CellStyle = dateStyle;
                }

                c = dataRow.CreateCell(19);
                if (!string.IsNullOrEmpty(Convert.ToString(dr["DemandSixthP"])))
                {
                    c.SetCellValue(ParseHelper.Parse<int>(dr["DemandSixthP"]));
                    c.CellStyle = dateStyle;
                }
                 
                dcIndex++;
            }

            workbook.Write(stream);            
        }

    }
}
