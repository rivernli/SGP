using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Data.SqlClient;
using SGP.DBUtility;

namespace BI.SGP.BLL.Utils
{
    public class Email
    {
        private MailMessage _mailMessage = null;

        public string Subject { get; set; }
        public string Body { get; set; }
        public string MailType { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string OuterErrMessage { get; set; }

        private List<string> _attachments;
        public List<string> Attachments
        {
            get 
            {
                if(_attachments == null) {
                    _attachments = new List<string>(); 
                }
                return _attachments;
            }
        }

        public Email()
        {
            _mailMessage = new MailMessage();
            _mailMessage.IsBodyHtml = true;
            string mailSender = "SGP Reminder<MultekBISupport@flextronics.com>";
            MailAddress from = new MailAddress(mailSender);
            _mailMessage.From = from;
        }

        public bool Send()
        {
            bool success;
            string Message = "";
            SmtpClient SMTPServer = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SMTPServer"]);
            try
            {
                _mailMessage.Subject = this.Subject;
                _mailMessage.Body = this.Body;
                PushAddress(_mailMessage.To, this.To);
                PushAddress(_mailMessage.CC, this.CC);
                PushAddress(_mailMessage.Bcc, this.BCC);

                foreach (string att in this.Attachments)
                {
                    Attachment attItem = new Attachment(att);
                    _mailMessage.Attachments.Add(attItem);
                }

                throw new Exception("test system can not send email");

                SMTPServer.Send(_mailMessage);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                Message = ex.Message;
            }

            if (OuterErrMessage != "")
            {
                if(Message != "") {
                    Message += ";" + OuterErrMessage;
                }
                else
                {
                    Message = OuterErrMessage;
                }
            }

            string strSql = "INSERT INTO SYS_MailLog(ToUser,CCUser,MailSubject,MailBody,MailType,Success,Message) VALUES(@ToUser,@CCUser,@MailSubject,@MailBody,@MailType,@Success,@Message)";
            DbHelperSQL.ExecuteSql(strSql, new SqlParameter[]{
                    new SqlParameter("@ToUser", this.To),
                    new SqlParameter("@CCUser", this.CC),
                    new SqlParameter("@MailSubject", this.Subject),
                    new SqlParameter("@MailBody", this.Body),
                    new SqlParameter("@MailType", MailType),
                    new SqlParameter("@Success", success == true ? 1 : 0),
                    new SqlParameter("@Message", Message),
                });

            return success;
        }

        private void PushAddress(MailAddressCollection mailAddress, string address)
        {
            if (!String.IsNullOrWhiteSpace(address))
            {
                string[] adds = address.Split(';');
                if (adds != null && adds.Length > 0)
                {
                    foreach (string add in adds)
                    {
                        if (!String.IsNullOrWhiteSpace(add))
                        {
                            mailAddress.Add(add.Trim());
                        }
                    }
                }
            }
        }
    }

}
