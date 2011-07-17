using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using Easynet.Edge.Core.Configuration;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	class BabylonBackOfficeRetreiver : BackOfficeGenericRetriever
	{
		protected override string WriteDataSetToFile(string url, DateTime startDate, DateTime endDate)
		{
			if (url == String.Empty ||
				url == null)
				throw new ArgumentException("Invalid URL. Cannot be empty or null.");

			DataSet ds = new DataSet();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = AppSettings.Get(this, "UserAgent");
			request.Timeout = 10 * 60 * 1000;
			request.ContentType = "application/x-www-form-urlencoded ";
			request.Method = "POST";

			//UrlParameters ="&amp;UserName=easynet&amp;Password=apieasynet&amp;DateFrom=_DATEFROM_&amp;DateTo=_DATETO_&amp;BrandID=1&amp;ShowTotal=true"
			ASCIIEncoding encoding = new ASCIIEncoding();

			string urlParameters = String.Empty;
			if (Instance.ParentInstance.Configuration.Options["UrlParameters"] != null)
			{
				urlParameters = Instance.ParentInstance.Configuration.Options["UrlParameters"].ToString();
			}
			else
			{
				throw new Exception("Could not find Url Parameters for Babylon BO service. Cannot continue.");
			}

			//urlParameters = urlParameters.Replace("_DATEFROM_", startDate.ToUniversalTime().ToString("MM/dd/yyyy"));
			//urlParameters = urlParameters.Replace("_DATETO_", endDate.ToUniversalTime().ToString("MM/dd/yyyy"));

			urlParameters = urlParameters.Replace("_DATEFROM_", startDate.ToString("MM/dd/yyyy"));
			urlParameters = urlParameters.Replace("_DATETO_", endDate.ToString("MM/dd/yyyy"));

			byte[] post = encoding.GetBytes(urlParameters);
			request.ContentLength = urlParameters.Length;
			Stream newStream = request.GetRequestStream();
			// Send the data.
			newStream.Write(post, 0, post.Length);
			newStream.Close();

			string fileName = String.Empty;
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				Stream urlStream = response.GetResponseStream();
				urlStream.ReadTimeout = 10 * 60 * 1000;

				fileName = InitalizeFileName(startDate, string.Empty);
				ds.ReadXml(urlStream);
				ds.WriteXml(fileName);
				ds.Dispose();
			}

			return fileName;
		}
	}
}
