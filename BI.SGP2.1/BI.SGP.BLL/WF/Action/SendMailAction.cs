using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web;
using System.Data;
using SGP.DBUtility;
using System.Net.Mail;
using BI.SGP.BLL.Utils;

namespace BI.SGP.BLL.WF.Action
{
    public class SendMailAction : IAction
    {
        private Email _mail = null;
        private WFActivity _activity;
        private string toUser = "";
        private string toMail = "";
        private string ccMail = "";
        private string _title;
        private string _body;
        private string _errMessage = "";
        private string Title
        {
            get
            {
                if (String.IsNullOrEmpty(_title))
                {
                    _title = GetMailTitle();
                }
                return _title;
            }
        }

        private string Body
        {
            get
            {
                if (String.IsNullOrEmpty(_body))
                {
                    if (this._activity.Template.Name == "SUPPLIERWF")
                    {
                        _body = GetMailBodyForSupplierRfq();
                    }
                    else
                    {
                        _body = GetMailBody();
                    }
                }
                return _body;
            }
        }

        private string ErrMessage
        {
            get { return _errMessage; }
            set { _errMessage += value + ";"; }
        }

        public void DoAction(WFActivity activity)
        {
            this._activity = activity;
            _mail = new Email();
            foreach (KeyValuePair<string, WFUser> kvUser in activity.WFUsers)
            {
                string mailAddress = String.IsNullOrWhiteSpace(kvUser.Value.Email) ? "" : kvUser.Value.Email.Trim() + ";";
                if (kvUser.Value.IsKeyUser)
                {
                    toUser += kvUser.Value.DisplayName + ",";
                    toMail += mailAddress;
                }
                else if (kvUser.Value.IsApprover && !kvUser.Value.IsKeyUser)
                {
                    continue;
                }
                else
                {
                    ccMail += mailAddress;
                }
            }

            if (AccessControl.IsVendor())
            {
                toUser += AccessControl.CurrentLogonUser.Name + ",";
                toMail += string.IsNullOrEmpty(AccessControl.CurrentLogonUser.Email) ?
                    "" : AccessControl.CurrentLogonUser.Email.Trim() + ";";
            }

            SendMail();
        }
        public void DoActionForVVI(WFActivity activity, string Vendor, string vendormail)
        {
            this._activity = activity;
            _mail = new Email();
            foreach (KeyValuePair<string, WFUser> kvUser in activity.WFUsers)
            {
                string mailAddress = String.IsNullOrWhiteSpace(kvUser.Value.Email) ? "" : kvUser.Value.Email.Trim() + ";";
                if (kvUser.Value.IsKeyUser)
                {
                    toUser += kvUser.Value.DisplayName + ",";
                    toMail += mailAddress;
                }
                else
                {
                    ccMail += mailAddress;
                }
            }


            toUser = Vendor;
            toMail += string.IsNullOrEmpty(vendormail) ?
                "" : vendormail + ";";
            SendMail();
        }

        private string GetMailTitle()
        {
            DataRow data = _activity.Template.MasterData;
            if (_activity.Template.Name == "SUPPLIERWF")
            {
                if (_activity.Name == "Closed")
                {
                    string number = data["NVARCHAR1"].ToString();
                    return String.Format("{0}, {1},{2}",
                        number.Substring(0, number.LastIndexOf("-")),
                        data["NVARCHAR2"],
                        number.Substring(number.LastIndexOf("-") + 1) + "(Reply)");
                }
                else
                {
                    string number = data["NVARCHAR1"].ToString();
                    return String.Format("{0}, {1}, {2}",
                        number.Substring(0, number.LastIndexOf("-")),
                        data["NVARCHAR2"],
                        number.Substring(number.LastIndexOf("-") + 1) + "(Inquiry)");
                }
            }
            if (_activity.Template.Name == "VVIWF")
            {
                return String.Format("{0}, {1}",
              data["Number"], data["CustomerPartNumber"]);
            }
            return String.Format("{0}, {1}, {2}, {3}",
                data["Number"], data["CustomerPartNumber"], data["ProgramName"], data["OEM"]);
        }

        private string GetMailBody()
        {
            string strconn = System.Configuration.ConfigurationManager.AppSettings["ConnVIDB"];
            string systemURL = System.Configuration.ConfigurationManager.AppSettings["SystemURL"];
            string path = System.Web.HttpContext.Current.Server.MapPath(String.Format("~/Content/Template/Email/ActionEmail_{0}_{1}.html", _activity.Template.ID, _activity.ID));
            string strContent = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path, System.Text.Encoding.Default))
            {
                strContent = sr.ReadToEnd();
            }

            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(strContent, @"\@\{([^\}]*)\}");

            DataRow data = this._activity.Template.MasterData;

            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                string placeholder = m.Groups[1].Value;

                if (placeholder == "ToUser")
                {
                    strContent = strContent.Replace(m.Value, toUser);
                }
                else if (placeholder == "SystemURL")
                {
                    strContent = strContent.Replace(m.Value, systemURL);
                }
                else if (placeholder == "AttachPDF")
                {
                    strContent = strContent.Replace(m.Value, "");
                    string pdfFile = CreatePDF();
                    if (!String.IsNullOrWhiteSpace(pdfFile))
                    {
                        _mail.Attachments.Add(pdfFile);
                    }
                }
                else if (placeholder == "VIFormRemark")
                {

                    StringBuilder sb = new StringBuilder();
                    if (data.Table.Columns.Contains("VIForm"))
                    {
                        if (string.IsNullOrEmpty(data["VIForm"].ToString().Trim()) == false)
                        {
                            SqlConnection conn = new SqlConnection(strconn);
                            string strsql = "select * from [tVI4] where rfq=@VIFormNumber";
                            DataTable dt = SqlText.ExecuteDataset(conn, null, strsql, null, new SqlParameter("@VIFormNumber", data["VIForm"].ToString())).Tables[0];
                            if (dt.Rows.Count > 0)
                            {


                                sb.Append(@"<table style=""border-collapse:collapse;border-spacing:1;"">");
                                sb.Append(@"<tr>
                                            <td style=""border:inset #CCCCCC 1.0pt;background:#CCCCCC;padding:1.5pt 1.5pt 1.5pt 1.5pt;"">VI System Remark</td>
                                            </tr>");
                                sb.Append(@"<tr>
                                             <td style=""border:inset #CCCCCC 1.0pt;border-top:none;padding:1.5pt 1.5pt 1.5pt 1.5pt;"">" + dt.Rows[0]["remark"] + @"</td>       
                                            </tr>
                                          </table>");

                            }
                        }
                    }
                    strContent = strContent.Replace(m.Value, sb.ToString());

                }
                else
                {
                    strContent = strContent.Replace(m.Value, Convert.ToString(data[placeholder]));
                }
            }

            return strContent;
        }

        private string GetMailBodyForSupplierRfq()
        {
            string strconn = System.Configuration.ConfigurationManager.AppSettings["ConnVIDB"];
            string systemURL = System.Configuration.ConfigurationManager.AppSettings["SystemURL"];
            string path = System.Web.HttpContext.Current.Server.MapPath(String.Format("~/Content/Template/Email/ActionEmail_{0}_{1}.html", _activity.Template.ID, _activity.ID));
            string strContent = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path, System.Text.Encoding.Default))
            {
                strContent = sr.ReadToEnd();
            }

            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(strContent, @"\@\{([^\}]*)\}");

            DataRow data = this._activity.Template.MasterData;

            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                string placeholder = m.Groups[1].Value;

                if (placeholder == "SystemURL")
                {
                    strContent = strContent.Replace(m.Value, systemURL);
                }
                else if (placeholder == "ToUser")
                {
                    strContent = strContent.Replace(m.Value, toUser);
                }
                else if (placeholder == "DATETIME1")
                {
                    DateTime costDateIn = Convert.ToDateTime(data[placeholder]);
                    if (costDateIn == null || costDateIn.ToString("yyyyMMddhhmmss") == "19000101120000")
                    {
                        strContent = strContent.Replace(m.Value, "");
                    }
                    else
                    {
                        strContent = strContent.Replace(m.Value, Convert.ToString(costDateIn.AddDays(1)));
                    }
                }
                else
                {
                    strContent = strContent.Replace(m.Value, Convert.ToString(data[placeholder]));
                }
            }

            return strContent;
        }

        private string GetMailBodyForNewsletter()
        {
            string strconn = System.Configuration.ConfigurationManager.AppSettings["ConnVIDB"];
            string systemURL = System.Configuration.ConfigurationManager.AppSettings["SystemURL"];
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Content/Template/Email/NewsletterNotification.html");
            string strContent = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path, System.Text.Encoding.Default))
            {
                strContent = sr.ReadToEnd();
            }

            strContent = strContent.Replace("Creator", AccessControl.CurrentLogonUser.Name);
            strContent = strContent.Replace("CreateDate", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

            return strContent;
        }

        public void SendMail()
        {
            try
            {
                _mail.Subject = this.Title;
                _mail.Body = this.Body;
                _mail.To = toMail;
                _mail.CC = ccMail;
                _mail.BCC = "Jason.Yan@multek.com;Arno.Feng@multek.com;Lance.Chen@multek.com;";
                _mail.MailType = "Workflow";
                _mail.OuterErrMessage = this.ErrMessage;
                _mail.Send();
            }
            catch { }
        }

        public void SendNewsletterMail(string mailAddress, int newsId)
        {
            try
            {
                _mail = new Email();
                _mail.Subject = string.Format("PCB FORTNIGHTLY NEWSLETTER - {0}", DateTime.Now.ToString("MM/dd/yyyy"));
                _mail.Body = GetMailBodyForNewsletter();
                _mail.To = mailAddress;
                _mail.CC = "";
                _mail.BCC = "Lance.Chen@multek.com;";
                _mail.MailType = "Newsletter";
                DataSet ds = GetAttachmentData(newsId);
                List<string> attachments = GetNewsletterAttachments(ds);
                foreach (string attachment in attachments)
                {
                    _mail.Attachments.Add(attachment);
                }
                _mail.OuterErrMessage = this.ErrMessage;
                _mail.Send();
                //RestoreFiles(ds);
            }
            catch(Exception ex)
            {
                string err = ex.Message;
            }
        }

        private DataSet GetAttachmentData(int newsId)
        {
            string sqlStr = "select * from SGP_CustomerNews a join SGP_Files b on b.RelationKey = a.AttachmentId where a.ID = @ID";
            DataSet ds = DbHelperSQL.Query(sqlStr, new SqlParameter("@ID", newsId));
            return ds;
        }

        private List<string> GetNewsletterAttachments(DataSet ds)
        {
            List<string> attachments = new List<string>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string folderPath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"].TrimEnd('\\') + "\\";
                string fullFolderPath = folderPath + dt.Rows[0]["Folder"].ToString() + "\\";
                string tempFolderPath = fullFolderPath + "TEMP\\";
                if (System.IO.Directory.Exists(fullFolderPath))
                {
                    if(!System.IO.Directory.Exists(tempFolderPath))
                    {
                        System.IO.Directory.CreateDirectory(tempFolderPath);
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        string fileName = fullFolderPath + dr["FileName"].ToString();
                        string sourceName = tempFolderPath + dr["SourceName"].ToString();
                        System.IO.File.Copy(fileName, sourceName, true);
                        attachments.Add(sourceName);
                    }
                }
            }
            return attachments;
        }

        private void RestoreFiles(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string folderPath = System.Configuration.ConfigurationManager.AppSettings["FilesFolder"].TrimEnd('\\') + "\\";
                string fullFolderPath = folderPath + dt.Rows[0]["Folder"].ToString() + "\\";
                string tempFolderPath = fullFolderPath + "TEMP\\";
                if (System.IO.Directory.Exists(fullFolderPath) && System.IO.Directory.Exists(tempFolderPath))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string sourceName = tempFolderPath + dr["SourceName"].ToString();
                        System.IO.File.Delete(sourceName);
                    }
                }
            }
        }

        private string CreatePDF()
        {
            string fileName = "";
            try
            {
                System.IO.MemoryStream mem = new System.IO.MemoryStream();
                BI.SGP.BLL.Models.PDFDownLoad pdf = new BI.SGP.BLL.Models.PDFDownLoad();
                if (pdf.getPDF(ref mem, this._activity.Template.EntityID, out fileName))
                {
                    if (fileName == "")
                    {
                        fileName = "multek_" + DateTime.Now.ToString("mmss") + ".pdf";
                    }
                    else
                    {
                        fileName += ".pdf";
                    }

                    fileName = System.Web.HttpContext.Current.Server.MapPath("~/temp/" + fileName);
                    using (var fileStream = FileHelper.CreateFile(fileName))
                    {
                        fileStream.Write(mem.GetBuffer(), 0, mem.GetBuffer().Length);
                    }
                }
            }
            catch { }

            return fileName;
        }
    }
}
