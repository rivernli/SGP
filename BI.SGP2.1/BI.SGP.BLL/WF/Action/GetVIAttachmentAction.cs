using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SGP.DBUtility;
using System.IO;


namespace BI.SGP.BLL.WF.Action
{
    public class GetVIAttachmentAction : IAction
    {
        public void DoAction(WFActivity activity)
        {
            string viFromNumber = string.Format("{0}", activity.Template.MasterData["VIForm"]);
            
            if (string.IsNullOrEmpty(viFromNumber)) return;

            int RFQID=activity.Template.EntityID;

            CopyFile(viFromNumber,RFQID); 
        }

        public void CopyFile(string VIFormNumber,int RFQID)
        {
            string strconn = System.Configuration.ConfigurationManager.AppSettings["ConnVIDB"];
            string VIHost = System.Configuration.ConfigurationManager.AppSettings["VIHost"];
            string ServerPath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"]+"\\"+RFQID.ToString();
            DirectoryInfo di = new DirectoryInfo(ServerPath);

            if (!di.Exists)
            {

                di.Create();
            }
          

            System.Net.WebClient webclient = new System.Net.WebClient();
            SqlConnection conn=new SqlConnection(strconn);
            string strsql = "select * from [tVI_attachment] where rfq=@VIFormNumber";
             DataTable dt = SqlText.ExecuteDataset(conn, null, strsql, null, new SqlParameter("@VIFormNumber", VIFormNumber)).Tables[0];
             string strSgpFile = "select * from SGP_Files where RelationKey=@RelationKey and [status]=1";
            DataTable targetdt = DbHelperSQL.Query(strSgpFile, new SqlParameter("@RelationKey", RFQID.ToString())).Tables[0];
              
             if (dt.Rows.Count > 0)
             {

                 foreach (DataRow dr in dt.Rows)
                 {
                     string sourcefile = VIHost + dr["folder"].ToString() +"\\"+dr["filename"].ToString();
                     string stuffix = dr["filename"].ToString().Substring(dr["filename"].ToString().LastIndexOf('.'), dr["filename"].ToString().Length - dr["filename"].ToString().LastIndexOf('.'));                 
                     string filename = System.Guid.NewGuid().ToString();
                     string sourcename = dr["filename"].ToString();
                     string targetfile = ServerPath + "\\" +filename + stuffix;
                     webclient.DownloadFile(sourcefile,targetfile);
                     string filesize = GetAutoSizeString(new FileInfo(targetfile).Length, 2);
                     DataRow[] drs = targetdt.Select("SourceName='" + sourcename + "' and RelationKey='"+RFQID.ToString()+"'");
                     if (drs.Length>0)
                     {
                         strsql = "update SGP_Files set [status]=9 where SourceName=@SourceName and RelationKey=@RelationKey";
                         List<SqlParameter> spa = new List<SqlParameter>() { new SqlParameter("@SourceName", sourcename), new SqlParameter("@RelationKey", RFQID.ToString()) };
                         DbHelperSQL.ExecuteSql(strsql,spa.ToArray());
                     }
                     strsql = string.Format("insert into SGP_Files select '{0}','{1}','{2}','{3}','{4}',Null,Null,getdate(),'{5}',1",RFQID,filename+stuffix,dr["filename"].ToString(),RFQID,filesize
                         ,"VI System");
                     DbHelperSQL.ExecuteSql(strsql);
                     
                     webclient.Dispose();
                 }
             
             }
        
        }

        public static string GetAutoSizeString(double size, int roundCount)
        {

            double b = 1024;
            double kb = b * 1024;
            double mb = kb * 1024;
            
            double gb = mb * 1024;

            if (b >size)
            {
                return Math.Round(size, roundCount) + "B";
            }
            else if (kb > size)
            {
                return Math.Round(size / b, roundCount) + "KB";
            }
            else if (mb > size)
            {
                return Math.Round(size / kb, roundCount) + "MB";
            }
            else
            {

                return Math.Round(size / mb, roundCount) + "GB";
            }
        }


    }
}
