using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;


namespace BI.SGP.BLL.Models
{

    public class SystemMessages
    {
        public const string MESSAGE_TYPE_WORKFLOW = "WorkFlow";
        public string RFQNumber { get; set; }

        public bool isPass { get; set; }
        public string MessageType { get; set; }
        public MessageCollection Messages = new MessageCollection();

        public SystemMessages()
        {
            this.isPass = true;
        }

        public void Merge(SystemMessages sysMsg)
        {
            if (sysMsg != null)
            {
                if (!sysMsg.isPass)
                {
                    this.isPass = false;
                }
                this.Messages.Add(sysMsg.Messages);
            }
        }

        public string MessageString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Messages.Count; i++)
                {
                    sb.AppendFormat("{0}:  {1} <br/>", Messages[i].Key, Messages[i].Value);
                }
                //foreach (string k in Messages.AllKeys)
                //{
                //    string val = Messages.Get(k);
                //    sb.AppendFormat("{0}:  {1} <br/>", k, val);
                //}
                return sb.ToString();

            }
        }
    }
}
