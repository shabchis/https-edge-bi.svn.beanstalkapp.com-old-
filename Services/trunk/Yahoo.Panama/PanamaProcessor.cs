using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using System.Data.SqlClient;
using System.Collections;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.Configuration;
using Easynet.Edge.Core.Data;
using System.IO;


namespace Easynet.Edge.Services.Yahoo.Panama
{
	
	class PanamaProcessorService : PpcProcessorService
	{

		#region Consts
		/*=========================*/
		private const string AdWordsTable = "Paid_API_AllColumns";
		//private const string RetrieverTable = "Adwords_API_Retriever";
		private const string PanamaServiceType = "Yahoo.Panama";
		private const int GatewayIdNullValue = -99;
		/*=========================*/
		#endregion     

		#region Members
		/*=========================*/
		private int _channelID = 0;
		private DateTime _requiredDay;
		/*=========================*/
		#endregion   
 
		#region Private Members
		/*=========================*/

		/// <summary>
		/// Initalize insert command parameters from xml reader for current row.
		/// </summary>
		/// <param name="insertCommand">The command to initalize his parameters.</param>
		/// <param name="reader">Xml reader object.</param>
		private void InitalizeCommandParameters(SqlCommand insertCommand,
			XmlTextReader xmlReader, long retriveredDate, bool hasBackOffice)
		{
			
			insertCommand.Parameters["@Day_Code"].Value = retriveredDate;
			//.......................................................................
			// TODO : use configuration extension for mapping fields to parameters!!!
			//.......................................................................



			insertCommand.Parameters["@date"].Value = DateTime.Now;
			insertCommand.Parameters["@account_ID"].Value = Instance.AccountID;
			insertCommand.Parameters["@Account_ID_SRC"].Value = Instance.AccountID;
			insertCommand.Parameters["@Channel_ID"].Value = _channelID;



			// Initalize parameter to divide by 1000000.
			insertCommand.Parameters["@ctr"].Value =
				ResolveDouble(xmlReader.GetAttribute("ctr")) / 1000000;
			insertCommand.Parameters["@cpc"].Value =
				ResolveDouble(xmlReader.GetAttribute("cpc")) / 1000000;
			insertCommand.Parameters["@cost"].Value =
				ResolveDouble(xmlReader.GetAttribute("cost")) / 1000000;
			//insertCommand.Parameters["@cpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("cpm")) / 1000000;
			//insertCommand.Parameters["@maxCpc"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxCpc")) / 1000000;
			//insertCommand.Parameters["@maxCpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxCpm")) / 1000000;

			//insertCommand.Parameters["@maxContentCpc"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxContentCpc")); // Yaniv : maybe need to divide this as well in 1000000




			// We don't fetch this data from google (mayve remove
			// it from insert command as well.
			//insertCommand.Parameters["@kwSite"].Value =
			//    ResolveString(xmlReader.GetAttribute("kwSite"));
			//insertCommand.Parameters["@customerid"].Value =
			//    ResolveLong(xmlReader.GetAttribute("campaignid"));
			//// MCC field - can't get if we don't request MCC report		

			//insertCommand.Parameters["@pos"].Value =
			//            ResolveDouble(xmlReader.GetAttribute("pos"));


			string url = xmlReader.GetAttribute("url");

			//InitGatewayID(insertCommand, url);


			//InitalizeParametersGK(insertCommand, _channelID, hasBackOffice, Instance.AccountID);

			// TODO : to delete after DB clean up.
			// Fields that were fetch in the past and will not be fetch in this version.
			/*
			downloaded date
			kwSite
			site,
			kwSiteType,
			siteKwStatus,
			keywordMinCpc,
			hostingKey,
			imgCreativeName,	
			campaignenddate,		
			currCode
			
			*/
		}

		private void InitGatewayID(SqlCommand insertCommand, string uri)
		{
			// Page
			try
			{
				string page = uri.Substring(uri.IndexOf("pid=") + ("pid=").Length);
				insertCommand.Parameters["@Page"].Value = page;
				insertCommand.Parameters["@Page_ID"].Value =
					ResolveInt(page.Contains("gid=") ? page.Substring(0, page.IndexOf("gid=")) : page);
			}
			catch
			{
				insertCommand.Parameters["@Page_ID"].Value = GatewayIdNullValue;
				insertCommand.Parameters["@Page"].Value = string.Empty;
			}

			// Initalize Gateway_id by the value in the destUrl.
			// Gateway
			try
			{
				string gateway;

				// Uri not from Kenshoo, fetch gatway ID from string "gid=".
				if (uri.Contains("gid="))
				{
					gateway = uri.Substring(uri.IndexOf("gid=") + ("gid=").Length);
				}
				// Uri not from Kenshoo, fetch gatway ID from string "gid%3".			
				else if (uri.Contains("adnum="))
				{
					gateway = uri.Substring(uri.IndexOf("adnum=") + ("adnum=").Length);
				}
				else
				{
					gateway = uri.Substring(uri.IndexOf("gid%3") + ("gid%3").Length);
				}
				insertCommand.Parameters["@Gateway"].Value = gateway;
				int gid = ResolveInt(gateway.Contains("pid=") ? gateway.Substring(0, gateway.IndexOf("pid=")) : gateway);

				//WID
				string widStr = uri.Substring(uri.IndexOf("wid=") + ("wid=").Length);
				int wid = ResolveInt(widStr.Contains("wid=") ? widStr.Substring(0, widStr.IndexOf("wid=")) : widStr);

				// Initalize Gateway_id
				insertCommand.Parameters["@Gateway_id"].Value = gid > 0 ? gid : wid;
			}
			catch
			{
				insertCommand.Parameters["@Gateway_id"].Value = GatewayIdNullValue;
				insertCommand.Parameters["@Gateway"].Value = string.Empty;
			}
		}

		private bool InitalizeReportMapping(ArrayList fields)
		{
			try
			{
				// Load the proper report paramters by the report name.
				FieldElementSection fes = (FieldElementSection)ConfigurationManager.GetSection("PanamaDailyReport");

				// Initalize the hash table with the fields of the the report.
				foreach (FieldElement fe in fes.Fields)
				{
					if (fe.Enabled && fe.InsertToDB)
					{
						//fields.Add(fe.Key, fe.DBFieldName);
						fields.Add(fe.DBFieldName.ToString().ToLower());
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write("Error get configuration Data for the report.", ex, Instance.AccountID);
				return false;
			}
			return true;
		}


		/// <summary>
		/// Insert all the rows in the report xml to the table - 
		/// Paid_API_AllColumns Table.
		/// </summary>
		/// <param name="xmlPath">Path of the xml report file.</param>
		/// <param name="retriverDayCode">The date when the data retriver in DayCode format.</param>
		private bool ProccessData(string xmlPath)
		{
			SqlCommand insertCommand = InitalizeInsertCommand();
			long retriveredDate = 0;
			bool hasBackOffice = HasBackOffice(Instance.AccountID);
			Dictionary<string, Type> tableFields = new Dictionary<string, Type>();

			// Initalize a Dictionary with table Fields and there types.
			if (!InitalizeTableFieldsTypes(tableFields))
			{
				return false;
			}

			ArrayList reportFields = new ArrayList();
			// Initalize a Dictionary with report fields.
			if (!InitalizeReportMapping(reportFields))
			{
				return false;
			}

			using (DataManager.Current.OpenConnection())
			{
				DataManager.ApplyConnection(insertCommand);
				// Load the reader with the data file and ignore all white space nodes.         

				using (XmlTextReader xmlReader = new XmlTextReader(xmlPath))
				{
					xmlReader.WhitespaceHandling = WhitespaceHandling.None;

					// Read root node
					xmlReader.Read();
					// Loop on all the rows in the xml report file.
					while (xmlReader.Read()) 
					{
						if (xmlReader.Name != "row")
						{
							if (xmlReader.Name == "report")
							{
								retriveredDate = ResolveLong(xmlReader.GetAttribute("datestart").Replace("-", string.Empty));
								retriveredDate = retriveredDate / 10000;

							}
							else
							{
								continue;
							}
						}

						if (!xmlReader.HasAttributes)
						{
							continue;
						}
	

						
						//dayCode = date; //_aggregationType.Equals("Summary") ? DayCode(date) - 1 : date;

						// Initalize insert command parameters.


						//IDictionaryEnumerator dic = reportMappingFields.GetEnumerator();
						//while (dic.MoveNext())
						//{
						//    insertCommand.Parameters["@" + dic.Value.ToString()].Value =
						//        xmlReader.GetAttribute(dic.Value.ToString().ToLower()) != null ?
						//        Convert.ChangeType(xmlReader.GetAttribute(dic.Value.ToString().ToLower()), (Type)tableFields[dic.Value.ToString().ToLower()]) : 0;
						//}

						// Initalize all parameters with null.
						InitalizeCommandParametersWithNulls(insertCommand);

						for (int i = 0; i < reportFields.Count; i++)
						{
							insertCommand.Parameters["@" + reportFields[i]].Value =
								xmlReader.GetAttribute(reportFields[i].ToString()) != null ?
								Convert.ChangeType(xmlReader.GetAttribute(reportFields[i].ToString()), (Type)tableFields[reportFields[i].ToString()]) : 0;
						}

						InitalizeCommandParameters(insertCommand, xmlReader, retriveredDate, hasBackOffice);


						// Execute command.
						insertCommand.ExecuteNonQuery();

					}
				}
				return true;
			}
		}


		/*=========================*/
		#endregion   

		#region Service Override Methods
		/*=========================*/

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork() 	
		{
			string xmlPath;

			if (!GetReportData(out xmlPath, out _requiredDay, out _channelID, PanamaServiceType))
			{
				return ServiceFailed("Panama Processor");
			}
			

			if (xmlPath != string.Empty)
			{
				//if (!GetAggregationType(this, out _aggregationType))
				//{
				//    return ServiceFailed("Panama Processor");
				//}
								
				if (Instance.ParentInstance.Configuration.Options["Date"] != null)
				{
					Log.Write(string.Format("Fetch data for date {0}.", Instance.ParentInstance.Configuration.Options["Date"].ToString()), LogMessageType.Information, Instance.AccountID);
					if (!StringToDate(Instance.ParentInstance.Configuration.Options["Date"].ToString(), out _requiredDay))
					{
						return ServiceFailed("Panama Processor");
					}
				}

				if (!DeleteDay(DayCode(_requiredDay), AdWordsTable))
				{
					return ServiceFailed("Panama Processor");
				}
				
				ProccessData(xmlPath);
			}
			else
			{
				Log.Write("Report path is empty.", LogMessageType.Error, Instance.AccountID);
				return ServiceFailed("Panama Processor");
			}

			return ServiceOutcome.Success;
		}

	


	}

	/*=========================*/
		#endregion    
}
