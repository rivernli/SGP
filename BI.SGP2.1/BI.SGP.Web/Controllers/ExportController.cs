using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using BI.SGP.BLL.UIManager;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.Export;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Utils;

namespace BI.SGP.Web.Controllers
{
    public class ExportController : Controller
    {
        //
        // GET: /Export/
        public ActionResult ExportExcel()
        {
            return View();
        }

        public FileResult DownloadExcelByGroupName()
        {
            FieldGroup fieldGroup = new FieldGroup(Request.QueryString["excelList"]);
            return ExportExcel(fieldGroup, Request.QueryString["renderType"]);
        }

        public FileResult DownloadExcel()
        {
            string value = Request.QueryString["excelList"];
            int groupID;
            if (!int.TryParse(value, out groupID))
            {
                groupID = 4;
            }

            if (groupID == 10)
            {
                groupID = 4;
            }

            FieldGroup fieldGroup = new FieldGroup(groupID);

            return ExportExcel(fieldGroup, Request.QueryString["renderType"]);
        }

        private FileResult ExportExcel(FieldGroup fieldGroup, string renderType)
        {
            string tempFile = "";
            if (fieldGroup.GroupName == "VVIMasterReportGrid")
            {
                tempFile = GroupData.GetExportDataForVVI(Request, fieldGroup);
            }
            else
            {
                tempFile = GroupData.GetExportData(Request, fieldGroup);
            }
            return File(tempFile, "application/ms-excel", fieldGroup.GroupName.Replace(" ", "_") + ".xlsx");
        }

        public ActionResult DownloadWFTemplate(int ID)
        {
            if (ID > 0)
            {
                WFActivity activity = new WFActivity(ID);
                string tempFile = ExcelHelper.ExportWorkflowTemplate(activity);
                return File(tempFile, "application/ms-excel", activity.Name.Replace(" ", "_") + "_template.xlsx"); 
            }
            else
            {
                return null;
            }
        }

        public FileResult DownloadTemplate()
        {
            string filename=Request.QueryString["filename"];
            string path = Server.MapPath(@"~/tmp/"+filename);
            return File(path, "application/ms-excel", filename);
        }

        public ActionResult Files()
        {
            return View();
        }

        public ActionResult UploadFile()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            string categoryDesc = Request["categoryDesc"];
            HttpPostedFileBase file = Request.Files["Filedata"];

            try
            {
                int id = 0;
                if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id) && id > 0)
                {
                    FileHelper.SaveRFQFiels(id.ToString(), file, true, category, categoryDesc);
                }
                else
                {
                    FileHelper.SaveRFQFiels(TempId, file, false, category, categoryDesc);
                }
                
                return Content("");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult UpdateFileInfo()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            int id = 0;
            if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id))
            {
                if (id > 0)
                {
                    FileHelper.UpdateTempToNormal(TempId, id.ToString());
                }
            }

            return null;
        }

        public string GetFilesData()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            DataTable dt = null;
            int id = 0;
            if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id) && id > 0)
            {
                dt = FileHelper.GetFilesData(id.ToString(), category);
            }
            else
            {
                dt = FileHelper.GetFilesData(TempId, category);
            }

            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("CreateTime", "{0:yyyy-MM-dd hh:mm}"));
        }

        public JsonResult DelFile()
        {
            string fileId = Request["fileId"];
            string errMessage = "";
            int id = 0;
            if (!String.IsNullOrWhiteSpace(fileId) && int.TryParse(fileId, out id) && id > 0)
            {
                try
                {
                    FileHelper.DelFile(id);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }
            else
            {
                errMessage = "invaild id";
            }

            var jsonResult = new
            {
                success = errMessage == "",
                message  = errMessage
            };

            return Json(jsonResult);
        }

        public ActionResult UploadFileForVVI()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            string categoryDesc = Request["categoryDesc"];
            HttpPostedFileBase file = Request.Files["Filedata"];

            try
            {
                int id = 0;
                if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id) && id > 0)
                {
                    FileHelperForVVI.SaveRFQFiels(id.ToString(), file, true, category, categoryDesc);
                }
                else
                {
                    FileHelperForVVI.SaveRFQFiels(TempId, file, false, category, categoryDesc);
                }

                return Content("");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult UpdateFileInfoForVVI()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            int id = 0;
            if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id))
            {
                if (id > 0)
                {
                    FileHelperForVVI.UpdateTempToNormal(TempId, id.ToString());
                }
            }

            return null;
        }

        public string GetFilesDataForVVI()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            DataTable dt = null;
            int id = 0;
            if (!String.IsNullOrWhiteSpace(RFQID) && int.TryParse(RFQID, out id) && id > 0)
            {
                dt = FileHelperForVVI.GetFilesData(id.ToString(), category);
            }
            else
            {
                dt = FileHelperForVVI.GetFilesData(TempId, category);
            }

            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("CreateTime", "{0:yyyy-MM-dd hh:mm}"));
        }

        public JsonResult DelFileForVVI()
        {
            string fileId = Request["fileId"];
            string errMessage = "";
            int id = 0;
            if (!String.IsNullOrWhiteSpace(fileId) && int.TryParse(fileId, out id) && id > 0)
            {
                try
                {
                    FileHelperForVVI.DelFile(id);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }
            else
            {
                errMessage = "invaild id";
            }

            var jsonResult = new
            {
                success = errMessage == "",
                message = errMessage
            };

            return Json(jsonResult);
        }


        public FileResult DownFileForVVI(string ID)
        {
            int fileId = 0;
            if (int.TryParse(ID, out fileId) && fileId > 0)
            {
                DataTable dt = FileHelperForVVI.GetFileData(fileId);
                if (dt.Rows.Count > 0)
                {
                    string fileFullPath = FileHelperForVVI.FolderPath + dt.Rows[0]["Folder"] + "\\" + dt.Rows[0]["FileName"];
                    string fileName = Convert.ToString(dt.Rows[0]["SourceName"]);
                    if (fileFullPath != "")
                    {
                        return File(fileFullPath, "application/octet-stream", fileName);
                    }
                }
            }
            return null;
        }
        public FileResult DownFile(string ID) 
        {
            int fileId = 0;
            if (int.TryParse(ID, out fileId) && fileId > 0)
            {
                DataTable dt = FileHelper.GetFileData(fileId);
                if (dt.Rows.Count > 0)
                {
                    string fileFullPath = FileHelper.FolderPath + dt.Rows[0]["Folder"] + "\\" + dt.Rows[0]["FileName"];
                    string fileName = Convert.ToString(dt.Rows[0]["SourceName"]);
                    if (fileFullPath != "")
                    {
                        return File(fileFullPath, "application/octet-stream", fileName);
                    }
                }
            }
            return null;
        }



        public ActionResult FileUpload()
        {
            return View();
        }
        public ActionResult Upload()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            string categoryDesc = Request["categoryDesc"];
            HttpPostedFileBase file = Request.Files["Filedata"];

            try
            {
                FileHelper.SaveRFQFiels(RFQID, file, false, category, categoryDesc);
                return Content("");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public string GetFiles()
        {
            string RFQID = Request["RFQID"];
            string TempId = Request["TempId"];
            string category = Request["category"];
            DataTable dt = FileHelper.GetFilesData(RFQID, category);

            GridData gd = new GridData();
            gd.Page = "0";
            gd.DataTable = dt;

            return gd.ToJson(new TableFormatString("CreateTime", "{0:yyyy-MM-dd hh:mm}"));
        }
        public FileResult DownloadFile(string ID)
        {
            int fileId = 0;
            if (int.TryParse(ID, out fileId) && fileId > 0)
            {
                DataTable dt = FileHelper.GetFileData(fileId);
                if (dt.Rows.Count > 0)
                {
                    string fileFullPath = FileHelper.FolderPath + dt.Rows[0]["Folder"] + "\\" + dt.Rows[0]["FileName"];
                    string fileName = Convert.ToString(dt.Rows[0]["SourceName"]);
                    if (fileFullPath != "")
                    {
                        return File(fileFullPath, "application/octet-stream", fileName);
                    }
                }
            }
            return null;
        }
        public JsonResult DeleteFile()
        {
            string fileId = Request["fileId"];
            string errMessage = "";
            int id = 0;
            if (!String.IsNullOrWhiteSpace(fileId) && int.TryParse(fileId, out id) && id > 0)
            {
                try
                {
                    FileHelper.DelFile(id);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
            }
            else
            {
                errMessage = "invaild id";
            }

            var jsonResult = new
            {
                success = errMessage == "",
                message = errMessage
            };

            return Json(jsonResult);
        }
    }
}
