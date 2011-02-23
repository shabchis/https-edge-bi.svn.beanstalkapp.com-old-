using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using Microsoft.Http;
using Newtonsoft;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using EdgeBI.RestTester;


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
			List<string> ComboValues = GetListOfValuesBySetting(EdgeBI.RestTester.Properties.Settings.Default.Services);
			ServiceAddressComboBox.DataSource = ComboValues;
			ServiceAddressComboBox.Refresh();
		}

		private List<string> GetListOfValuesBySetting(string settings)
		{
			List<string> values = settings.Split(',').ToList<string>();
			return values;
		}

		private void LoadHeaders()
		{
			List<string> KeyValues = GetListOfValuesBySetting(EdgeBI.RestTester.Properties.Settings.Default.DefaultHeaders);
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



		private void GetButton_Click(object sender, EventArgs e)
		{
			CreateRequest("GET");
		}

		private void CreateRequest(string requestType)
		{
			string server;
			string service;
			if (localCheckBox.Checked)
				server = EdgeBI.RestTester.Properties.Settings.Default.LocalAdress;
			else
				server = EdgeBI.RestTester.Properties.Settings.Default.RemoteAdress;
			if (!string.IsNullOrEmpty(QueryStringstextBox.Text.Trim()) && Regex.Matches(ServiceAddressComboBox.Text, "{\\d}").Count > 0)
			{
				string[] queryStringsArray = QueryStringstextBox.Text.Split(',');
				service = string.Format(ServiceAddressComboBox.Text, queryStringsArray);
			}
			else
				service = ServiceAddressComboBox.Text.Trim();


			string fullAddress = string.Format("http://{0}/{1}", server, service);

			HttpClient client = new HttpClient();
			HttpRequestMessage request = new HttpRequestMessage(requestType, fullAddress);
			
			//Microsoft.Http.Headers.Connection conn = new Microsoft.Http.Headers.Connection() { Close = false };
			
			if (requestType=="GET")
			request.Headers.Accept.Add(new Microsoft.Http.Headers.StringWithOptionalQuality("application/json"));
			else
				request.Headers.Accept.Add(new Microsoft.Http.Headers.StringWithOptionalQuality("text/tab-separated-values"));







			foreach (ListViewItem item in HeaderslistView.Items)
			{
				request.Headers.Add(item.Text, item.SubItems[1].Text);

			}
			if (requestType == "POST" || requestType=="DELETE")
			{
				request.Headers.ContentType = "application/json";
				if (!string.IsNullOrEmpty(BodyTextBox.Text))
					request.Content = HttpContent.Create(BodyTextBox.Text);
			}
			HttpResponseMessage response = new HttpResponseMessage();

			if (requestType == "GET")
				response.Headers.ContentType = "application/json";
			else
				response.Headers.ContentType = "text/tab-separated-values";
			
			response = client.Send(request);
			ResponseHeaderTextBox.Text = string.Empty;
			ResponseHeaderTextBox.Text += response.StatusCode + "\n";
			foreach (KeyValuePair<string, string[]> header in response.Headers)
			{
				ResponseHeaderTextBox.Text += string.Format(header.Key + ": {0}\n", header.Value[0]);
			}
			ResponseBodyRichTextBox.Text = response.Content.ReadAsString();
		

		}

		


		private void SetButton_Click(object sender, EventArgs e)
		{
			CreateRequest("POST");
		}

		private void ServiceAddressComboBox_Validated(object sender, EventArgs e)
		{
			if (ServiceAddressComboBox.Text.ToLower().Contains("sessions") && string.IsNullOrEmpty(BodyTextBox.Text.Trim()))
				BodyTextBox.Text = EdgeBI.RestTester.Properties.Settings.Default.BodyLogInJson;
			else if (ServiceAddressComboBox.Text.ToLower().Contains("permissions") && string.IsNullOrEmpty(BodyTextBox.Text.Trim()))
				BodyTextBox.Text = EdgeBI.RestTester.Properties.Settings.Default.BodyGetSpecificPermission;
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

						XmlDocument odoc=new XmlDocument();
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
	}
}
