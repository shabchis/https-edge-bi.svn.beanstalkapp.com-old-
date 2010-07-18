using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Messaging;

namespace Easynet.Edge.Services.Messaging.Providers
{
    public class EmailProvider : Provider
    {
        public override void Initialize()
        {
            _type = "email";

            //Read configuration.
            AddProviderConfiguration("Easynet.Edge.Messaging.Host",AppSettings.GetAbsolute("Easynet.Edge.Messaging.Host"));
            AddProviderConfiguration("Easynet.Edge.Messaging.Port", AppSettings.GetAbsolute("Easynet.Edge.Messaging.Port"));
        }

        public override void Send(string recipient, Message msg)
        {
            if (recipient == String.Empty ||
                recipient == null)
                throw new Exception("Invalid recipient. Cannot be empty/null.");

            int port = -1;
            if (!Int32.TryParse(GetProviderConfiguration("Easynet.Edge.Messaging.Port").ToString(), out port))
                throw new Exception("Invalid port.");

            string host = GetProviderConfiguration("Easynet.Edge.Messaging.Host").ToString();
            if (host == String.Empty || host == null)
                throw new Exception("Invalid host.");

            SmtpClient smtp = new SmtpClient(host, port);
            smtp.UseDefaultCredentials = true;

            MailMessage mm = new MailMessage("admin@easynet.co.il", recipient);
            mm.Subject = msg.Title;
            mm.Body = msg.MessageText;

            if (msg.Attachments != null)
            {
                foreach (string s in msg.Attachments)
                {
                    mm.Attachments.Add(new Attachment(s));
                }
            }

            smtp.Send(mm);
        }
    }
}
