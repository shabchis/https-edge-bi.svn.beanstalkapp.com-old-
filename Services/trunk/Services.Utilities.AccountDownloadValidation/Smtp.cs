using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net.Mail;
using System.Net;

namespace Easynet.Edge.Services.Utilities
{
	public class Smtp
	{
		internal static void Send(string subject, bool highPriority,
										string body,bool IsBodyHtml,string attachment)
		{
			string _toAddress, _fromAddress;
			System.Net.Mail.MailMessage msg = new MailMessage();
			msg.Subject = subject;
			if (highPriority)
				msg.Priority = MailPriority.High;
			try
			{
				if (!String.IsNullOrEmpty(body)) msg.Body = body;
				if (IsBodyHtml) msg.IsBodyHtml = true;
					else msg.IsBodyHtml = false;
				SmtpClient smtp = Smtp.GetSmtpConnection(out _toAddress, out _fromAddress);
				msg.To.Add(_toAddress);
				msg.From = new MailAddress(_fromAddress);
				if (!String.IsNullOrEmpty(attachment))
				{ 
					msg.Attachments.Add(new Attachment(attachment));
				}
				smtp.Send(msg);
			}
			catch (Exception e)
			{
				throw new Exception("Cannot send Email" + e.Message);
			}
		}
		internal static SmtpClient GetSmtpConnection(out string to, out string from)
		{
			try
			{
				IDictionary smtpCon = Config.GetSection("SmtpConnection");
				SmtpClient smtp = new SmtpClient(smtpCon["server"].ToString(), Int32.Parse((smtpCon["port"].ToString())));
				smtp.Credentials = new NetworkCredential(smtpCon["user"].ToString(), smtpCon["pass"].ToString());
				smtp.UseDefaultCredentials = Boolean.Parse(smtpCon["UseDefaultCredentials"].ToString());
				smtp.EnableSsl = Boolean.Parse(smtpCon["EnableSsl"].ToString());

				to = smtpCon["to"].ToString();
				from = smtpCon["from"].ToString();
				return smtp;
			}
			catch (Exception ex)
			{
				throw new Exception("SMTP Configuration Error" + ex.Message);
			}
		}
	}
}
