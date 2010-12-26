using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;
using System.IO;
using Easynet.Edge.Core.Utilities;
using System.Web;

namespace Easynet.Edge.Messaging
{
	public class SmsMessage
	{
		public static string EventLogSource = "SmsMessage.exe";
		public static readonly string AccountToken1 = AppSettings.GetAbsolute("SMS.AccountToken1");
		public static readonly string AccountToken2 = AppSettings.GetAbsolute("SMS.AccountToken2");
		public static readonly string Sender = AppSettings.GetAbsolute("SMS.Sender");
		public static readonly string Url = AppSettings.GetAbsolute("SMS.Url");
		public static readonly string Body = AppSettings.GetAbsolute("SMS.Body")
			.Replace("{AccountToken1}", AccountToken1)
			.Replace("{AccountToken2}", AccountToken2)
			.Replace("{Sender}", Sender);

		public static void Send(string message, string phoneNumber)
		{
			message = String.Format("{0} ({1}, {2:d/M/yyyy@HH:mm:ss})", message, Environment.MachineName.ToLower(), DateTime.Now);

			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(Url);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded"; // Not needed
			string content = Body
				.Replace("{TargetNumber}", phoneNumber)
				.Replace("{Message}", Uri.EscapeUriString(message));
			using (Stream stream = request.GetRequestStream())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(content);
				stream.Write(bytes, 0, bytes.Length);
			}
			request.BeginGetResponse(delegate(IAsyncResult result)
			{
				WebResponse response = request.EndGetResponse(result);
				string responseText = HttpUtility.HtmlDecode(new StreamReader(response.GetResponseStream()).ReadToEnd());

				// These are SmartSMS.co.il specific error codes, log them just in case.
				if (responseText.Contains("<result><success>"))
				{
					Log.Write(EventLogSource,
						String.Format("SMS sent. Server response: {0}", responseText),
						LogMessageType.Information);
				}
				else
				{
					Log.Write(EventLogSource,
						String.Format("Error sending SMS. Server response: {0}", responseText),
						LogMessageType.Warning);
				}
			},
			null);
			
		}

		public static void Send(string message, string[] phoneNumbers)
		{
			foreach (string phoneNumber in phoneNumbers)
				Send(message, phoneNumber);
		}
	}
}
