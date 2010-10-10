using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
namespace WFTestWizard
{
	public partial class frmTestWizard : Form
	{
		const string baseXml = "BaseRequestXml.xml";
		int? _sessionID = null;

		Uri _baseUri = new Uri("http://localhost:8080/wizard");

		public frmTestWizard()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{


		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(txtWizardNum.Text))
				{
					MessageBox.Show("Please fill wizard number");

				}
				else
				{
					string sessionID = StartWizard(int.Parse(txtWizardNum.Text));
					txtResult.Text = string.Format("Account Wizard Service Started:\nSessionID IS:{0}\nReady for First Step\n----------------------", sessionID);
					gvKeyValue.Enabled = true;
					btnNextStep.Enabled = true;
					btnExecute.Enabled = false;
					btnGetSummary.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private string StartWizard(int? _wizardID)
		{
			try
			{

				WebRequest request = HttpWebRequest.Create(_baseUri + "/start?wizardID=1");
				WebResponse response = request.GetResponse();
				XmlDocument doc = GetXmlResponse(response);
				string value = GetXmlValue("SessionID", doc);
				_sessionID = int.Parse(value);
				return value;


			}
			catch (Exception)
			{
				throw;

			}

		}

		private string GetXmlValue(string xmlNodeName, XmlDocument doc)
		{
			try
			{
				return doc.DocumentElement[xmlNodeName].InnerText;

			}
			catch (Exception)
			{

				throw;
			}
		}

		private XmlDocument GetXmlResponse(WebResponse response)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					doc.LoadXml(reader.ReadToEnd());
				}
			}
			catch (Exception)
			{

				throw;
			}
			return doc;
		}

		private void btnNextStep_Click(object sender, EventArgs e)
		{


			DialogResult result = System.Windows.Forms.DialogResult.Yes;
			WebRequest request = HttpWebRequest.Create(string.Format("{0}/collect?sessionID={1}", _baseUri, _sessionID));
			request.ContentType = "application/xml";
			request.Method = "POST";
			request.Timeout = 130000;
			try
			{
				SetBody(ref request);
			}
			catch (Exception ex)
			{

				result = MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.YesNo);
			}
			if (result == System.Windows.Forms.DialogResult.Yes)
			{
				try
				{
					try
					{
						WebResponse response = request.GetResponse();
						XmlDocument doc = GetXmlResponse(response);
						string value = GetXmlValue("Result", doc);

						switch (value)
						{
							case "Next":
								{

									txtResult.Text = "\n--------------------\nStep Collect Finish\nReady for next step:";
									break;
								}
							case "HasErrors":
								{
									txtResult.Text = "Error:\n" + doc.OuterXml;

									break;
								}
							case "Done":
								{
									txtResult.Text = "\nFinsish collecting from all steps\nready to get summary or execute:";
									btnNextStep.Enabled = false;
									btnGetSummary.Enabled = true;
									btnExecute.Enabled = true;
									break;
								}
						}



					}
					catch (WebException ex)
					{

						using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
						{

							txtResult.Text=string.Format("ErrorCode:{0}\n ErrorDescription: {1} \nInnerException:{2}", ex.Status.ToString(), reader.ReadToEnd(), ex.InnerException);
						}

					}
				}
				catch (Exception ex)
				{

					txtResult.Text = ex.Message;
				}

			}
			
		}

		private void SetBody(ref WebRequest request)
		{
			bool empty = true;
			XmlDocument doc = new XmlDocument();
			doc.Load(string.Format(Path.Combine(Application.StartupPath, baseXml)));
			XmlNode parentNode = doc.DocumentElement["CollectedValues"];			
			foreach (DataGridViewRow row in gvKeyValue.Rows)
			{
				if (row.Cells[0].Value != null && row.Cells[1].Value != null)
				{
					empty = false;
					XmlElement elem = doc.CreateElement("KeyValueOfstringanyType");
					elem.LocalName = "a";
					
					elem.InnerText = string.Format("<a:Key>{0}</a:Key><a:Value i:type=\"b:string\" xmlns:b=\"http://www.w3.org/2001/XMLSchema\">{1}</a:Value>\"</a:KeyValueOfstringanyType>", row.Cells[0].Value.ToString(), row.Cells[0].Value.ToString());
					parentNode.AppendChild(elem);
				}


			}
			if (empty)
			{
				throw new Exception("No data on the grid, do you want to procceed?");

			}
			else
			{
				using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
				{
					writer.Write(doc.OuterXml);
				}
			}

		}



		private void txtWizardNum_Validating(object sender, CancelEventArgs e)
		{
			Regex patern = new Regex(@"\d");
			TextBox t = (TextBox)sender;
			for (int i = 0; i < t.Text.Length; i++)
			{
				if (!patern.IsMatch(t.Text.Substring(i)))
				{
					t.Text = string.Empty;
					e.Cancel = true;


				}

			}

		}

		private void btnGetSummary_Click(object sender, EventArgs e)
		{
			try
			{
				WebRequest request = HttpWebRequest.Create(string.Format("{0}/summary?sessionID={1}", _baseUri, _sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				WebResponse response = request.GetResponse();
				XmlDocument SummaryXml = new XmlDocument();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{

					SummaryXml.LoadXml(reader.ReadToEnd());
					txtResult.Text = SummaryXml.DocumentElement.InnerText;

				}
			}
			catch (Exception ex)
			{

				txtResult.Text = ex.Message;
			}

		}

		private void btnExecute_Click(object sender, EventArgs e)
		{
			try
			{
				WebRequest request = HttpWebRequest.Create(string.Format("{0}/execute?sessionID={1}", _baseUri, _sessionID));
				request.Method = "POST";
				request.ContentType = "application/xml";
				request.ContentLength = 0;
				WebResponse response = request.GetResponse();
				XmlDocument strXml = new XmlDocument();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{

					strXml.LoadXml(reader.ReadToEnd());
					txtResult.Text = strXml.DocumentElement.InnerText;

				}
			}
			catch (Exception ex)
			{

				txtResult.Text = ex.Message;
			}
			finally
			{
				btnExecute.Enabled = false;
				btnNextStep.Enabled = false;
				btnGetSummary.Enabled = false;
				btnStart.Enabled = true;
			}
		}


	}


}
