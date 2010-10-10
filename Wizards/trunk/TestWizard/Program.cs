using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace TestWizard
{
	class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("Starting Account Wizard");

			//Call the start method (Rest)
			Uri baseUri = new Uri("http://localhost:8080/wizard");
			WebRequest request = HttpWebRequest.Create(baseUri + "/start?wizardID=1");
			WebResponse response = request.GetResponse();



			//Get the new sessionID from xml (wizardSession)
			XmlDocument wizardSession = new XmlDocument();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				wizardSession.LoadXml(reader.ReadToEnd());
			}

			int sessionID = int.Parse(wizardSession.DocumentElement["SessionID"].InnerText);

			Console.WriteLine("Session ID is {0}\n", sessionID);
			bool passed = false;
			////Call the First step
			while (!passed)
			{
				Console.WriteLine("Press enter to continue...\nReady for 1st step");
				Console.ReadLine();


				
				request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", baseUri, sessionID));
				request.ContentType = "application/xml";
				request.Method = "POST";


				SetBody(ref request);
				request.Timeout = 130000;

				try
				{
					response = request.GetResponse();
					XmlDocument stepcollectresponse = new XmlDocument();
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						stepcollectresponse.LoadXml(reader.ReadToEnd());
						if (stepcollectresponse.DocumentElement["Result"].InnerText != "HasErrors")
						{
							Console.WriteLine("Step 1 Finished collecting\n");
							passed = true;
						}
						else
						{
							Console.WriteLine("Service did not started yet!!!\n");
							passed = false;
						}


					}
				}
				catch (WebException ex)
				{
					using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
					{

						Console.WriteLine("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(),ex.InnerException);
					}

					
				}
			
				
			}

			response.Close();
			Console.WriteLine("Ready for the 2nd and last step!\nPress any key to continue\n");
			Console.ReadLine();
			 passed = false;
			while (!passed)
			{
				request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", baseUri, sessionID));
				request.ContentType = "application/xml";
				request.Method = "POST";


				SetBody(ref request);
				request.Timeout = 130000;
				XmlDocument stepcollectresponse = new XmlDocument();
				response = request.GetResponse();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					stepcollectresponse.LoadXml(reader.ReadToEnd());
					if (stepcollectresponse.DocumentElement["Result"].InnerText != "HasErrors")
					{
						Console.WriteLine("Step 2 Finished collecting\n");
						passed = true;
					}
					else
					{
						Console.WriteLine("Service did not started yet!!!\n");
					}


				}
			}
			Console.WriteLine("Finished Collecting from all steps\nPress any key to get the Summary");
			Console.ReadLine();
			
			
				request = HttpWebRequest.Create(string.Format("{0}/summary?sessionID={1}", baseUri, sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				response = request.GetResponse();
				XmlDocument strXml = new XmlDocument();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					Console.WriteLine("Summary:\n");
					strXml.LoadXml(reader.ReadToEnd());
					Console.WriteLine(strXml.DocumentElement.InnerText);
					
				}


				Console.WriteLine("ReadyToExecute\nPress any key to continue!");


				Console.ReadLine();
				request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", baseUri, sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				response = request.GetResponse();
				strXml = new XmlDocument();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					Console.WriteLine("ExecutionResult: \n");
					strXml.LoadXml(reader.ReadToEnd());
					Console.WriteLine(strXml.DocumentElement.InnerText);

				}



				Console.WriteLine("Press any key to exit\n");


				Console.ReadLine();





		}
		public static void SetBody(ref WebRequest request)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(@"C:\Users\alonya\Desktop\Tools\Stepresponse.xml");
			using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(doc.OuterXml);
			}

		}
		

	}
}
