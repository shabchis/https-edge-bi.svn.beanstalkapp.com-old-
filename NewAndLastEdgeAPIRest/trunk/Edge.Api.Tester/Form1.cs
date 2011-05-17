using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NewRestApiTester;
using System.Net;
using Microsoft.Http;
using Newtonsoft;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using Edge.Objects;
using Newtonsoft.Json.Converters;



namespace NewRestApiTester__
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			LoadHeaders();
			LoadServicesComboxBox();
			FormatComboBox.SelectedIndex = 1;


			
		}

		private void LoadServicesComboxBox()
		{
			List<string> ComboValues = GetListOfValuesBySetting(NewRestApiTester.Properties.Settings.Default.Services);
			ServiceAddressComboBox.DataSource = ComboValues;
			ServiceAddressComboBox.Refresh();
		}

		private List<string> GetListOfValuesBySetting(string settings)
		{
			List<string> values = settings.Split(',').ToList<string>();
			values.Sort();
			return values;
		}

		private void LoadHeaders()
		{
			List<string> KeyValues = GetListOfValuesBySetting(NewRestApiTester.Properties.Settings.Default.DefaultHeaders);
			foreach (string str in KeyValues)
			{
				string[] keyvalArray = new string[2];
				keyvalArray = str.Split(':');
				HeaderslistView.Items.Add(new ListViewItem(keyvalArray) { Name = keyvalArray[0] });
			}

			RegistryKey temp = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("WcfRestTester");
			if (temp == null)
			{
				temp = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("WcfRestTester").CreateSubKey("LastSession");
				temp.SetValue("LastSession", "{0}");
			}
			else
			{
				string session = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("WcfRestTester").OpenSubKey("LastSession").GetValue("LastSession", "{0}").ToString();
				if (!string.IsNullOrEmpty(session))
					HeaderslistView.Items["x-edgebi-session"].SubItems[1].Text = session;
			}







		}
		public static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			try
			{
				Exception e = (Exception)args.ExceptionObject;
				MessageBox.Show(string.Format("Un Handeled exception occured:\n{0} istermainating={1}", e.Message, args.IsTerminating));
			}
			catch (Exception)
			{
				
				
			}
			
			
		}


		private void GetButton_Click(object sender, EventArgs e)
		{
			CreateRequest("GET");
		}

		private void CreateRequest(string requestType)
		{
			string server;
			string service;
			if (localCheckBox.Checked)
				server = NewRestApiTester.Properties.Settings.Default.LocalAdress;
			else
				server = NewRestApiTester.Properties.Settings.Default.RemoteAdress;
			if (!string.IsNullOrEmpty(QueryStringstextBox.Text.Trim()) && Regex.Matches(ServiceAddressComboBox.Text, "{\\d}").Count > 0)
			{
				string[] queryStringsArray = QueryStringstextBox.Text.Split(',');
				service = string.Format(ServiceAddressComboBox.Text, queryStringsArray);
			}
			else
				service = ServiceAddressComboBox.Text.Trim();


			string fullAddress = string.Format("{0}/{1}", server, service);


			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(fullAddress);
			request.Timeout = 9999999;
			request.Accept = "application/json";
			request.ContentType = "application/json";

			request.Method = requestType;


			foreach (ListViewItem item in HeaderslistView.Items)
			{
				request.Headers.Add(item.Text, item.SubItems[1].Text);

			}

			if (requestType == "POST" || requestType == "PUT")
			{
				request.ContentLength = BodyTextBox.Text.Length;
				request.Headers["ContentType"] = "application/json";
				if (!string.IsNullOrEmpty(BodyTextBox.Text))
					SetBodyForCollectRequest(ref request);

			}
			HttpWebResponse response = null;
			
			bool error = false;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException ex)
			{
				error = true;

				using (WebResponse res = ex.Response)
				{
					using (StreamReader reader = new StreamReader(res.GetResponseStream()))
					{
						
						ResponseBodyRichTextBox.Text = reader.ReadToEnd();
					}
					ResponseHeaderTextBox.Text = string.Empty;
					ResponseHeaderTextBox.Text=ex.Status.ToString() + " " + ex.Message;

					//ResponseHeaderTextBox.Text += response.StatusCode + "\n";
					//ResponseHeaderTextBox.Text += response.StatusDescription + "\n";
					//ResponseHeaderTextBox.Text += response.LastModified + "\n";
				}
				


			}



			if (error == false)
			{
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{

					ResponseBodyRichTextBox.Text = reader.ReadToEnd();
				}

				ResponseHeaderTextBox.Text = string.Empty;
				ResponseHeaderTextBox.Text += response.StatusCode + "\n";
				ResponseHeaderTextBox.Text += response.StatusDescription + "\n";
				ResponseHeaderTextBox.Text += response.LastModified + "\n";
				foreach (var header in response.Headers)
				{

					ResponseHeaderTextBox.Text += string.Format(header + "\n");
				}
				AfterRespondEvents(service);
				response.Close();
			}
			/*
			HttpClient client = new HttpClient();
			HttpRequestMessage request = new HttpRequestMessage(requestType, fullAddress);
			
			//Microsoft.Http.Headers.Connection conn = new Microsoft.Http.Headers.Connection() { Close = false };
			request.Headers.Accept.Add(new Microsoft.Http.Headers.StringWithOptionalQuality("application/json"));






			foreach (ListViewItem item in HeaderslistView.Items)
			{
				request.Headers.Add(item.Text, item.SubItems[1].Text);

			}
			if (requestType == "POST" || requestType=="PUT")
			{
				request.Headers.ContentType = "application/json";
				if (!string.IsNullOrEmpty(BodyTextBox.Text))
					request.Content = HttpContent.Create(BodyTextBox.Text);
			}
			HttpResponseMessage response = new HttpResponseMessage();
			response.Headers.ContentType = "application/json";
			lock (client)
			{
				response = client.Send(request);

			}
			ResponseHeaderTextBox.Text = string.Empty;
			ResponseHeaderTextBox.Text += response.StatusCode + "\n";
			foreach (KeyValuePair<string, string[]> header in response.Headers)
			{
				ResponseHeaderTextBox.Text += string.Format(header.Key + ": {0}\n", header.Value[0]);
			}
			ResponseBodyRichTextBox.Text = response.Content.ReadAsString();
			AfterRespondEvents(service);*/

		}

		private void SetBodyForCollectRequest(ref HttpWebRequest request)
		{
			using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(BodyTextBox.Text);
				//writer.Flush();

			}

		}

		private void AfterRespondEvents(string service)
		{
			if (service.ToLower().Trim().Contains("sessions"))
			{
				SessionResponseData sessionResponseData = null;
				try
				{
					sessionResponseData = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionResponseData>(ResponseBodyRichTextBox.Text);
				}
				catch (Exception)
				{


				}
				if (sessionResponseData != null && !string.IsNullOrEmpty(sessionResponseData.Session))
				{
					HeaderslistView.Items["x-edgebi-session"].SubItems[1].Text = sessionResponseData.Session;
					Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("WcfRestTester").CreateSubKey("LastSession").SetValue("LastSession", sessionResponseData.Session);
				}
			}

		}


		private void SetButton_Click(object sender, EventArgs e)
		{
			CreateRequest("POST");
		}

		private void ServiceAddressComboBox_Validated(object sender, EventArgs e)
		{
			if (ServiceAddressComboBox.Text.ToLower().Contains("sessions") && string.IsNullOrEmpty(BodyTextBox.Text.Trim()))
				BodyTextBox.Text = NewRestApiTester.Properties.Settings.Default.BodyLogInJson;
			else if (ServiceAddressComboBox.Text.ToLower().Contains("permissions") && string.IsNullOrEmpty(BodyTextBox.Text.Trim()))
				BodyTextBox.Text = NewRestApiTester.Properties.Settings.Default.BodyGetSpecificPermission;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(ResponseBodyRichTextBox.Text))
			{

				ResponseBodyRichTextBox.Text = ConvertToXMLORJSON(FormatComboBox.Text.Trim().ToUpper(), ResponseBodyRichTextBox.Text);
			}
		}
		private string ConvertToXMLORJSON(string convertType, string convertText)
		{
			string returnValue = convertText;
			if (!string.IsNullOrEmpty(convertText))
			{
				if (convertType.Trim().ToUpper() == "XML")
				{

					try
					{
						XmlDocument odoc;
						if (!convertText.Contains("root"))
						{
							try
							{
								odoc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(convertText, "root");
							}
							catch (Exception)
							{
								odoc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode("{\"root\":" + convertText + "}", "root");

							}
						}
						else
							odoc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(convertText);
						returnValue = FormatXml(odoc);
					}
					catch (Exception)
					{



					}

				}
				else
				{
					try
					{
						byte[] encodedString = Encoding.UTF8.GetBytes(convertText);
						MemoryStream ms = new MemoryStream(encodedString);
						ms.Flush();
						ms.Position = 0;

						XmlDocument odoc = new XmlDocument();
						odoc.Load(ms);
						returnValue = Newtonsoft.Json.JsonConvert.SerializeXmlNode(odoc.SelectSingleNode("root")).ToString();
					}
					catch (Exception)
					{


					}

				}

			}
			return returnValue;

		}

		private string FormatXml(XmlDocument odoc)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(memoryStream, Encoding.Unicode);
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 1;
			writer.IndentChar = '\t';
			odoc.WriteTo(writer);
			writer.Flush();
			writer.Close();
			return Encoding.Unicode.GetString(memoryStream.GetBuffer());
		}

		private void AddUpdateButton_Click(object sender, EventArgs e)
		{
			if (HeaderslistView.SelectedItems.Count == 0)
			{
				string[] keyval = new string[2];
				keyval[0] = HeaderKeytextBox.Text;
				keyval[1] = HeaderValuetextBox.Text;
				HeaderslistView.Items.Add(new ListViewItem(keyval) { Name = keyval[0] });
			}
			else
			{
				HeaderslistView.SelectedItems[0].Text = HeaderKeytextBox.Text;
				HeaderslistView.SelectedItems[0].SubItems[1].Text = HeaderValuetextBox.Text;
				HeaderslistView.SelectedItems[0].Name = HeaderKeytextBox.Text;


			}
		}

		private void removeButton_Click(object sender, EventArgs e)
		{
			HeaderslistView.Items.Remove(HeaderslistView.SelectedItems[0]);
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			HeaderslistView.Clear();
		}

		private void ServiceAddressComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void DeleteButton_Click(object sender, EventArgs e)
		{
			CreateRequest("DELETE");

		}

		private void btnPut_Click(object sender, EventArgs e)
		{
			CreateRequest("PUT");
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			
		}
		private List<WordIndexs> FindText(RichTextBox targetRiceTextBox,string wordToFind)
		{
			int indexToStart=0;
			List<WordIndexs> words = new List<WordIndexs>();
			while (indexToStart<targetRiceTextBox.Text.Length)
			{

				indexToStart = targetRiceTextBox.Find(wordToFind, indexToStart, targetRiceTextBox.Text.Length , RichTextBoxFinds.None);
				if (indexToStart == -1)
					break;
				words.Add(new WordIndexs() { StartIndex=indexToStart,Length=wordToFind.Length,EndIndex=indexToStart+wordToFind.Length-1});
				indexToStart += wordToFind.Length;
				
			}

			return words;
		}

		private void findButton_Click(object sender, EventArgs e)
		{
			List<WordIndexs> words = FindText(BodyTextBox, textBox1.Text);			
			
			BodyTextBox.BackColor = Color.White;
		
		
			if (words != null)
			{
				foreach (WordIndexs word in words)
				{
					BodyTextBox.SelectionStart = word.StartIndex;

					BodyTextBox.SelectionLength = word.Length;

					

					BodyTextBox.SelectionBackColor = Color.Yellow;
					
					

				}
			}
			words = null;
			words = FindText(ResponseBodyRichTextBox, textBox1.Text);
			ResponseBodyRichTextBox.BackColor = Color.White;
			if (words != null)
			{
				foreach (WordIndexs word in words)
				{
					ResponseBodyRichTextBox.SelectionStart = word.StartIndex;

					ResponseBodyRichTextBox.SelectionLength = word.Length;

					

					ResponseBodyRichTextBox.SelectionBackColor = Color.Yellow;



				}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Refund r = (Refund)Newtonsoft.Json.JsonConvert.DeserializeObject(BodyTextBox.Text, typeof(Refund));
			BodyTextBox.Text = Newtonsoft.Json.JsonConvert.SerializeObject(r, Newtonsoft.Json.Formatting.Indented, new IsoDateTimeConverter());
		}

		

	}
	public struct WordIndexs
	{
		public int StartIndex;
		public int Length;
		public int EndIndex;
	}
		
}
