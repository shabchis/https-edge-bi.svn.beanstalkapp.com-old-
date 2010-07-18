using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval;

namespace Easynet.Edge.Services.BackOffice.EasyForex
{
	
	public class EasyForexBackOfficeProcessorService : BackOfficeProcessorService 
	{
		#region Consts
		/*=========================*/

		private const string RetrieverTable = "BackOffice_Retriever";
		private const string BackOfficeTable = "BackOffice_Client_Gateway";

		/*=========================*/
		#endregion

		#region Members
		/*=========================*/

		private DateTime _requiredDay;

		/*=========================*/
		#endregion

        #region Private Methods
        /*=========================*/
	
		/// <summary>
		/// Fills DB with data that retrieved by EasyForex BackOffice
		/// </summary>
		/// <param name="dataFromBO">Data from BackOffice to write to DB.</param>
		private void ProcessData(string xmlPath, DateTime _requiredDay)
		{
			SqlCommand insertCommand = InitalizeInsertCommand();

			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Init insertCommand with the data manger connection 
				DataManager.ApplyConnection(insertCommand);

				// Yaniv: add Exception
				/////////////////////
				Type t = Type.GetType(Instance.Configuration.Options["BackOfficeXmlReader"]);
				System.Reflection.ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(string) });
				//if (constructor == null)
				//	throw new blahl
				//BackOfficeXmlReader reader = (BackOfficeXmlReader) constructor.Invoke(new object[] { xmlPath });

				////////////////////

				//EasyForexReader reader = new EasyForexReader(xmlPath);

				// Initalize const parmaters.
				insertCommand.Parameters["@Downloaded_Date"].Value = DateTime.Now;
				insertCommand.Parameters["@day_Code"].Value = DayCode(_requiredDay);
				insertCommand.Parameters["@hour_Code"].Value = DayCode(_requiredDay) == DayCode(DateTime.Today) ? DateTime.Now.Hour : 0;
				insertCommand.Parameters["@account_ID"].Value = Instance.AccountID;
				//insertCommand.Parameters["@channel_ID"].Value = ChannelID;

				using (EasyForexReader reader = (EasyForexReader) constructor.Invoke(new object[] { xmlPath }))
				{
					// Read all rows in the BackOffice XML and insert them to the DB.
					while (reader.Read())
					{
						if (reader.CurrentRow.GatewayID == 0)
						{
							Log.Write("Error parsing BackOffice row, Can't insert row to DB.", LogMessageType.Error);
							continue;
						}

						InitalizeParametersWithNull(insertCommand);

						// Initalize command parmaters.
						insertCommand.Parameters["@gateway_id"].Value = reader.CurrentRow.GatewayID;
						insertCommand.Parameters["@total_Hits"].Value = reader.CurrentRow.TotalHits;
						insertCommand.Parameters["@new_Leads"].Value = reader.CurrentRow.NewLeads;
						insertCommand.Parameters["@new_Users"].Value = reader.CurrentRow.NewUsers;
						insertCommand.Parameters["@new_Active_Users"].Value = reader.CurrentRow.NewActiveUsers;
						insertCommand.Parameters["@new_Net_Deposits_in_dollars"].Value = reader.CurrentRow.NewNetDepostit;
						insertCommand.Parameters["@active_Users"].Value = reader.CurrentRow.ActiveUsers;
						insertCommand.Parameters["@total_Net_Deposits_in_dollars"].Value = reader.CurrentRow.TotalNetDeposit;
						insertCommand.Parameters["@Gateway_GK"].Value = GkManager.GetGatewayGK(Instance.AccountID, reader.CurrentRow.GatewayID);

						try
						{
							// Execute command.
							insertCommand.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							Log.Write(string.Format("Error in Inserting data to BackOffice_Client_Gateway table in easynet_Oltp DB."), ex);
						}
					}
				}
			}
		}

        /*=========================*/
        #endregion

        #region Service Override Methods
        /*=========================*/
        
        /// <summary>
        /// Handle abort event.
        /// </summary>
		protected override void OnEnded(ServiceOutcome serviceOutcome)
        {
			if ((serviceOutcome == ServiceOutcome.Aborted) ||
				(serviceOutcome == ServiceOutcome.Failure))
			{
				// Delete old data from Today.
				DeleteDay(DayCode(_requiredDay), BackOfficeTable);
			}
        } 
	
        /// <summary>
        /// Main entry point of the service.
        /// </summary>
		/// <returns>True for success. False for failure</returns>
        protected override ServiceOutcome DoWork()
        {	
			string xmlPath = string.Empty;

			// Load a specific xml file. 
			if ((Instance.Configuration.Options["FilePath"] != null) &&
				(Instance.Configuration.Options["FileDate"] != null))
			{
				Log.Write(string.Format("Fetch the file {0}.", Instance.Configuration.Options["FilePath"]), LogMessageType.Information);
				xmlPath = Instance.Configuration.Options["FilePath"];

				// Check if the file exist.
				if (!File.Exists(xmlPath))
				{
					throw new Exception(string.Format("file {0} doesn't exist.", Instance.Configuration.Options["FilePath"]));
				}

				// Initalize _requiredDay with file creation time.
				StringToDate(Instance.Configuration.Options["FileDate"].ToString(), out _requiredDay);
			}				
			else
			{
				_requiredDay = DateTime.Today;
				// Get the path of the xml file.
				GetReportPath(ref xmlPath, RetrieverTable);			

				// Return all data of last day.
				if ((Instance.Configuration.Options["LastDay"] != null) ||
					(Convert.ToBoolean(Instance.Configuration.Options["LastDay"])))
				{
					Log.Write("Fetch last day data.", LogMessageType.Information);
					_requiredDay = _requiredDay.AddDays(-1);
				}
				else if (Instance.ParentInstance.ParentInstance.Configuration.Options["Date"] != null)
				{
					Console.WriteLine("{0} EasyForex BO Processor service run for date {1}",
						ShortHeader(),
						Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString());

					Log.Write(string.Format("Fetch data for date {0}.", Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString()), LogMessageType.Information);
					StringToDate(Instance.ParentInstance.ParentInstance.Configuration.Options["Date"].ToString(), out _requiredDay);
				}
			}
		
			DataSet dataFromBO = new DataSet();
			dataFromBO.ReadXml(xmlPath);
			ProcessData(xmlPath, _requiredDay);

			return ServiceOutcome.Success;
		 }

		/*=========================*/
        #endregion   
	}
}
