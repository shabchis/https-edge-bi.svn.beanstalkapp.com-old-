using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using EdgeBI.Objects;
using System.Reflection;
using System.IO;

namespace EdgeBI.Tester
{
	public class Test
	{
		
		public static void Main()
		{
			//Get User


			//WebRequest request = HttpWebRequest.Create(string.Format(@"http://localhost:54796/EdgeBIAPIService.svc/users/{0}", 8));
			//request.Timeout = 300000;

			//WebResponse response = request.GetResponse();

			//ShowResponse(response, "Get User");



			// Get Menu
			//WebRequest request = HttpWebRequest.Create("http://localhost:54796/EdgeBIAPIService.svc/menu");
			//request.Timeout = 3000000;

			//WebResponse response = request.GetResponse();

			//ShowResponse(response, "Get Menu");


			WebRequest request = HttpWebRequest.Create(string.Format(@"http://localhost:54796/EdgeBIAPIService.svc/Accounts/{0}",1));
			request.Timeout = 300000;

			WebResponse response = request.GetResponse();

			ShowResponse(response, "Get Account");



		}

		private static void ShowResponse(WebResponse response, string p)
		{
			Console.WriteLine(p);
			using (StreamReader reader=new StreamReader(response.GetResponseStream()) )
			{
				
				
				Console.WriteLine(reader.ReadToEnd());
				
			}
			Console.ReadLine();
		}
		
		
		
	}
}
