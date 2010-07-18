using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	class EasyForexBackOfficeRetriever:BaseRetriever
	{
		#region Consts
		/*=========================*/

		private const string BackOfficeServiceType = "BackOffice";
		private const string BackOfficeTable = "BackOffice_Client_Gateway";

		/*=========================*/
		#endregion   

		#region Fields
		/*=========================*/

		//DataSet _dataFromBO;
		DataTable _dataFromBO;
		private EasyForexBackOfficeAPI.Marketing _easyForexBackOffice = null;

		/*=========================*/
		#endregion  
 
		#region Constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public EasyForexBackOfficeRetriever()
		{
			serviceType = BackOfficeServiceType;
		}

		/*=========================*/
		#endregion   

		#region Override Protected Methods
		/*=========================*/

		protected override string WriteResultToFile()
		{
			return WriteResultToFile(_dataFromBO, _requiredDay);
		}

		/// <summary>
		/// Handle OnEnded event.
		/// </summary>
		protected override void OnEnded(ServiceOutcome serviceOutcome)
		{
			if ((serviceOutcome == ServiceOutcome.Aborted) ||
				(serviceOutcome == ServiceOutcome.Failure))
			{
				if (_easyForexBackOffice != null)
					_easyForexBackOffice.Abort();
			}
		}

		protected override bool InitalizeServiceData()
		{
			//_dataFromBO = new DataSet();
			_dataFromBO = new DataTable();
			return base.InitalizeServiceData();
		}

		/*=========================*/
		#endregion

		#region Private Methods
		/*=========================*/

		/// <summary>
		/// Validate the data we got from EasyForex BackOffice by checking
		/// the total hits of Major gateway ID that always suppose to
		/// be above 0.
		/// </summary>
		/// <remarks>Insert to app.config the attribure "ValidateString"
		/// with the value of a major gateway ID (see example) in 
		/// each BackOffice Retriever Service.</remarks>
		/// <example>Example for attribute line to insert to app.config:
		/// ValidateString="GID = 23045"</example>
		/// <param name="dataFromBO">The result data we got from EasyForex
		/// BackOffice web service.</param>
		/// <returns>True for valid BO result, false for invalid BO result.</returns>
		private void ValidateReport(DataTable dataFromBO)
		{
			// Check if the attribure "ValidateString" found in the 
			// configuration, if not we write warning and return true.
			if (Instance.Configuration.Options["ValidateString"] == null && ((Instance.ParentInstance != null) &&
				Instance.ParentInstance.Configuration.Options["ValidateString"] == null))
			{
				Log.Write("There isn't a validate string for EasyForex BO Service.", LogMessageType.Warning);
				return;
			}

			DataRow[] rows = dataFromBO.Select(GetConfigurationOptionsField("ValidateString"));

			//DataRow[] rows = dataFromBO.Tables[0].Select(GetConfigurationOptionsField("ValidateString"));

			if (Convert.ToInt32(rows[0]["TotalHits"]) > 0)
			{
				return;
			}

			throw new Exception("The data that Retrievered from EasyForex BackOffice is incorrect.");
		}

		/// <summary>
		/// Creates access property to Easyforex API object.
		/// </summary>
		/// <param name="user">Account name (for example "Amiry")</param>
		/// <param name="password">Password (for example "wretg2gad")</param>
		/// <returns>Easyforex API access class</returns>
		private EasyForexBackOfficeAPI.AuthHeader InitBOAccess(string user, string password)
		{
			EasyForexBackOfficeAPI.AuthHeader accessAccount = new EasyForexBackOfficeAPI.AuthHeader();
			accessAccount.Password = password;
			accessAccount.Username = user;
			return accessAccount;
		}
		
		/// <summary>
		/// Get the data from EasyForex BackOffice using account info and 
		/// return the data in dataFromBO data set.
		/// </summary>
		protected override void GetReportData()
		{
			string userName = GetConfigurationOptionsField("User");
			// Init BackOffice object		
			_easyForexBackOffice = new EasyForexBackOfficeAPI.Marketing();
			_easyForexBackOffice.Timeout = 10 * 60 * 1000; // 10 minutes.

			// Init access data to easy forex.
            _easyForexBackOffice.AuthHeaderValue = InitBOAccess(userName, Encryptor.Decrypt(GetConfigurationOptionsField("Password"), GetConfigurationOptionsField("Password")));
			//DataTable tempDataTable;
			// Initalize _requiredDay with format that valid for EasyForex BackOffice.
			_requiredDay = new DateTime(_requiredDay.Year, _requiredDay.Month, _requiredDay.Day);

			// Get the data from EasyForex BO. 
			lock (_easyForexBackOffice)
			{
				try
				{
					//tempDataTable = _easyForexBackOffice.GetCampaignStatistics(1, 1000000, _requiredDay, _requiredDay.AddDays(1).AddTicks(-1)).Tables[0];
					//tempDataTable = _easyForexBackOffice.GetCampaignStatisticsNEW(1, 1000000, _requiredDay, _requiredDay.AddDays(1).AddTicks(-1)).Tables[0];
					_dataFromBO = _easyForexBackOffice.GetCampaignStatisticsNEW(1, 1000000, _requiredDay, _requiredDay.AddDays(1).AddTicks(-1)).Tables[0];
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("EASY FOREX BUG - Error get data from EasyForex BackOffice for user {0}.", userName), ex);
				}
			}

			// Update DataTable name and insert it to dataFromBO.

			_dataFromBO.DataSet.Tables.Remove(_dataFromBO);
			_dataFromBO.TableName = _dataFromBO.TableName + userName;

			//tempDataTable.DataSet.Tables.Remove(tempDataTable);
			//tempDataTable.TableName = tempDataTable.TableName + userName;
			//_dataFromBO.Tables.Add(tempDataTable);

			// Yaniv: remove the remark
			ValidateReport(_dataFromBO);
		}



		/*=========================*/
		#endregion
	}
}
