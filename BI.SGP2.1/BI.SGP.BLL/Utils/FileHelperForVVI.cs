using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using SGP.DBUtility;

namespace BI.SGP.BLL.Utils
{
    public class FileHelperForVVI
    {
        public static string FolderPath = System.Configuration.ConfigurationManager.AppSettings["FilesFolderForVVI"].TrimEnd('\\') + "\\";

        public static FileStream CreateFile(string tempFile)
        {
            string tempPath = System.Web.HttpContext.Current.Server.MapPath("~/temp/");

            DirectoryInfo mydir = new DirectoryInfo(tempPath);
            if (mydir.Exists)
            {
                foreach (FileSystemInfo fsi in mydir.GetFileSystemInfos())
                {
                    if (fsi is FileInfo)
                    {
                        try
                        {
                            FileInfo fi = (FileInfo)fsi;
                            if (fi.CreationTime < DateTime.Now.AddHours(-2))
                            {
                                fi.Delete();
                            }
                        }
                        catch { }
                    }
                }
            }
            else
            {
                mydir.Create();
            }

            return System.IO.File.Create(tempFile);
        }

        public static void SaveRFQFiels(string id, HttpPostedFileBase file, bool entityExists, string category, string categoryDesc)
        {
            DelTempFiles();

            string folderFullPath = FolderPath + id + "\\";
            if (!Directory.Exists(folderFullPath))
            {
                Directory.CreateDirectory(folderFullPath);
            }

            string sourceName = file.FileName;
            string fileType = System.IO.Path.GetExtension(sourceName).ToLower();
            string fileName = Guid.NewGuid().ToString() + fileType;
            int fileSize = file.ContentLength;
            string creator = AccessControl.CurrentLogonUser.Name;
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            file.SaveAs(folderFullPath + fileName);

            string strSql = "INSERT INTO SGP_FilesForVVI(RelationKey,FileName,SourceName,Folder,FileSize,Creator,Status,Category,CategoryDesc) VALUES(@RelationKey,@FileName,@SourceName,@Folder,@FileSize,@Creator,@Status,@Category,@CategoryDesc)";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[] { 
                new SqlParameter("@RelationKey", id),
                new SqlParameter("@FileName", fileName),
                new SqlParameter("@SourceName", sourceName),
                new SqlParameter("@Folder", id),
                new SqlParameter("@FileSize", fileSize > 1048575 ? String.Format("{0:F2}", fileSize/1048576.0) +  "MB" : String.Format("{0:F2}", fileSize/1024.0) + "KB"),
                new SqlParameter("@Creator", creator),
                new SqlParameter("@Status", entityExists == true ? 1 : 0 ),
                new SqlParameter("@Category", category == null? "":category),
                new SqlParameter("@CategoryDesc", categoryDesc == null? "":categoryDesc),
            });
        }

        public static void UpdateTempToNormal(string tempId, string id)
        {
            string strSql = "SELECT COUNT(*) FROM SGP_FilesForVVI WHERE RelationKey = @TempId";
            if (DbHelperSQL.GetSingle<int>(strSql, new SqlParameter[] { new SqlParameter("@TempId", tempId) }) > 0)
            {
                using (TScope ts = new TScope())
                {
                    strSql = "UPDATE SGP_FilesForVVI SET RelationKey = @RelationKey, Status = 1, Folder = @RelationKey FROM SGP_Files WHERE RelationKey = @TempId";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter[]{
                        new SqlParameter("@RelationKey", id),
                        new SqlParameter("@TempId", tempId),
                    });

                    try
                    {
                        FolderRename(tempId, id);
                    }
                    catch
                    {
                        ts.Rollback();
                    }
                }

            }
        }

        public static DataTable GetFilesData(string relationKey, string category)
        {
            List<SqlParameter> lstParams = new List<SqlParameter>();
            string strSql = "SELECT ID, SourceName, FileSize, Creator, CreateTime, CategoryDesc FROM SGP_FilesForVVI WHERE RelationKey = @RelationKey AND [Status] <> 9";
            lstParams.Add(new SqlParameter("@RelationKey", relationKey));

            if (!String.IsNullOrEmpty(category))
            {
                strSql += " AND Category = @Category";
                lstParams.Add(new SqlParameter("@Category", category));
            }

            return DbHelperSQL.Query(strSql, lstParams.ToArray()).Tables[0];
        }

        public static void DelFile(int id)
        {
            string fileFullName = GetFilePath(id);
            if (fileFullName != "" && File.Exists(fileFullName))
            {
                File.Delete(fileFullName);

            }
            string strSql = "DELETE FROM SGP_FilesForVVI WHERE ID = @ID";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[]{
                new SqlParameter("@ID", id)
            });
        }

        public static string GetFilePath(int id)
        {
            string fileFullName = "";
            string strSql = "SELECT * FROM SGP_FilesForVVI WHERE ID = @ID";
            DataTable dt = DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@ID", id)
            }).Tables[0];
            if (dt.Rows.Count > 0)
            {
                fileFullName = FolderPath + dt.Rows[0]["Folder"] + "\\" + dt.Rows[0]["FileName"];
            }
            return fileFullName;
        }

        public static DataTable GetFileData(int id)
        {
            string strSql = "SELECT * FROM SGP_FilesForVVI WHERE ID = @ID";
            return DbHelperSQL.Query(strSql, new SqlParameter[]{
                new SqlParameter("@ID", id)
            }).Tables[0];
        }

        private static void DelTempFiles()
        {
            string strSql = "SELECT ID, Folder FROM SGP_FilesForVVI WHERE Status = 0 AND DATEDIFF(HOUR,CreateTime,getdate()) > 2";
            DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                string folderFullPath = FolderPath + dr["Folder"];
                DirectoryInfo di = new DirectoryInfo(folderFullPath);
                try
                {
                    if (di.Exists)
                    {
                        di.Delete(true);
                    }
                    strSql = "DELETE FROM SGP_FilesForVVI WHERE ID = @ID";
                    DbHelperSQL.ExecuteSql(strSql, new SqlParameter("@ID", dr["ID"]));
                }
                catch { }
            }
        }

        private static void FolderRename(string orgName, string newName)
        {
            string folder = FolderPath + "\\";
            string fullOrgName = folder + orgName;
            string fullNewName = folder + newName;
            DirectoryInfo di = new DirectoryInfo(fullOrgName);
            if (di.Exists)
            {
                di.MoveTo(fullNewName);
            }
        }
    }
}
