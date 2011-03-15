using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	public class BaseProcessor : ProcessorService
	{
		#region Fields
		/*=========================*/

		protected Dictionary<string, string> _tableTypeMappings;
		protected ArrayList _dataIdentifiers = new ArrayList();

		/*=========================*/
		#endregion   

		#region Consts
		/*=========================*/

		protected const int DivideFieldsValue = 1000000;
		protected const int MaxInsertRetries = 3;
		protected string _xmlRowName = string.Empty;
		protected string _tableName = string.Empty;
		protected string _serviceType = string.Empty;
		protected string _defaultErrorSubDirPath = string.Empty;
		protected int _defaultMinusRequiredDate = 0;
		protected string _accountFieldName = "Account_Name";
		protected string _accountName = string.Empty;

		
		//private const string MonthAggregationType = "MONTH";

		/*=========================*/
		#endregion   

		#region Constructors
		/*=========================*/

		public BaseProcessor()
		{
			// Initalize Dictionary to map between sql data type to .net data type.
			_tableTypeMappings = new Dictionary<string, string>();
			_tableTypeMappings.Add("int", "System.Int32");
			_tableTypeMappings.Add("bigint", "System.Int64");
			_tableTypeMappings.Add("float", "System.Double");
			_tableTypeMappings.Add("nvarchar", "System.String");
			_tableTypeMappings.Add("datetime", "System.DateTime");
			_tableTypeMappings.Add("bool", "System.Bool");
			_tableTypeMappings.Add("decimal", "System.Double");
		}

		/*=========================*/
		#endregion   

		#region Empty Protected Virtual Methods
		/*=========================*/

		protected virtual void InitalizeGatewayGK(SqlCommand insertCommand, XmlTextReader xmlReader, FieldElement fe)
		{
		}

		protected virtual void InitalizeGatewayGK(SqlCommand insertCommand, SourceDataRowReader<RetrieverDataRow> reader, FieldElement fe)
		{
		}

		/// <summary>
		/// Used to Activate Content stored procedure for relevant services.
		/// </summary>
		protected virtual void HandledContentSP()
		{
		}

		//protected virtual void InitalizeNullValues(SqlCommand insertCommand, FieldElementSection rawDataFields, FieldElementSection metaDataFields)
		//{
		//}

		protected virtual void InitalizeNullValues(SqlCommand insertCommand, FieldElementSection rawDataFields, FieldElementSection metaDataFields)
		{
			// Initalize the insertCommand null values for the fields in rawDataFields.
			foreach (FieldElement fe in rawDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				// Initalize field with null value.
				if (fe.DBDefaultValue == null)
					insertCommand.Parameters["@" + fe.DBFieldName].Value = DBNull.Value;
				else
					insertCommand.Parameters["@" + fe.DBFieldName].Value =
						Convert.ChangeType(fe.DBDefaultValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));
			}

			// Initalize the insertCommand null values for the fields in metaDataFields.
			foreach (FieldElement fe in metaDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				// Initalize field with null value.
				if (fe.DBDefaultValue == null)
					insertCommand.Parameters["@" + fe.DBFieldName].Value = DBNull.Value;
				else
					insertCommand.Parameters["@" + fe.DBFieldName].Value =
						Convert.ChangeType(fe.DBDefaultValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));
			}
		}

		/*=========================*/
		#endregion   

		#region Protected Virtual Methods
		/*=========================*/

		protected virtual void ProccessData(string xmlPath, string defaultErrorSubDirPath)
		{
			bool hasBackOffice = HasBackOffice(_accountID);
			bool xmlFileEmpty = false;
			
			FieldElementSection rawDataFields;
			FieldElementSection metaDataFields;
			SqlCommand insertCommand;
			Dictionary<string, string> gatewayNameFields;
			InitalizeProcessData(out rawDataFields, out metaDataFields, out insertCommand, out gatewayNameFields);

			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.StartTransaction();
				DataManager.Current.AssociateCommands(insertCommand);

				try
				{
					// Load the reader with the data file and ignore all white space nodes.         
					ReadFile(xmlPath, defaultErrorSubDirPath, hasBackOffice, ref xmlFileEmpty, rawDataFields, metaDataFields, insertCommand, gatewayNameFields);
					DataManager.Current.CommitTransaction();
				}
				catch
				{
                    throw;
                    //try
                    //{
                    //    DataManager.Current.RollbackTransaction(); //fix by alon _transaction was null 25/1/2011
                    //}
                    //catch
                    //{              
                    //}
					
				}
			}

			// Throw Exception if the xml report file is empty.
			if (xmlFileEmpty)
				Log.Write(string.Format("Xml report file has no rows for date {0}.", _requiredDay.ToShortDateString()), LogMessageType.Warning);
		}

		// original ReadFile function		
		protected virtual void ReadFile(string xmlPath, string defaultErrorSubDirPath, bool hasBackOffice, ref bool xmlFileEmpty, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields)
		{
			bool firstRowInXml = true;
			using (XmlTextReader xmlReader = new XmlTextReader(xmlPath))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;

				// Read root node
				xmlReader.Read();
				xmlFileEmpty = true;

				// Loop on all the rows in the xml report file.
				while (xmlReader.Read())
				{
					if (!GetToRowsSection(xmlReader))
						continue;

					// Initalize dayCode.
					if (firstRowInXml)
					{
						xmlFileEmpty = false;
						firstRowInXml = false;
						// ASAF: raise log error if we have diffrent date in the file and in _requiredDay.
						_requiredDay = DayCode.GenerateDateTime(GetDate(xmlReader));

						if (_accountID == SystemAccountID)
						{
							GetAccountID(xmlReader.GetAttribute("acctname").ToString(), out _accountID, _accountFieldName);
							_accountName = xmlReader.GetAttribute("acctname").ToString();
						}

						// Delete old data in the DB for the same data we fetch the new data.			
						DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);
						_dataIdentifiers.Add(dataIdentifier);
						HandleDeleteDay();
						InitalizeNullValues(insertCommand, rawDataFields, metaDataFields);

					}

					CheckRowData(xmlReader);

					try
					{
						HandleRow(hasBackOffice, rawDataFields, metaDataFields, insertCommand, gatewayNameFields, xmlReader);
					}
					catch (Exception ex)
					{
						WriteErrorMesageToFile(xmlPath, xmlReader.ReadOuterXml(), ex, defaultErrorSubDirPath);
					}
				}
			}
		}
		
		protected virtual void CheckRowData(SourceDataRowReader<RetrieverDataRow> reader)
		{
			int accountID;
			// New date in the file.
			if (DayCode.GenerateDateTime(GetDate(reader)) != _requiredDay)
			{
				_requiredDay = DayCode.GenerateDateTime(GetDate(reader));

				DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);

				if (!_dataIdentifiers.Contains(dataIdentifier))
				{
					// Delete old data in the DB for the same data we fetch the new data.			
					HandleDeleteDay();
					_dataIdentifiers.Add(dataIdentifier);
				}
			}

			if (!string.IsNullOrEmpty(_accountName))
			{
				// New account in the file.
				if (reader.CurrentRow.Fields["acctname"] != _accountName)
				{
					GetAccountID(reader.CurrentRow.Fields["acctname"], out accountID, _accountFieldName);

					_accountID = accountID;
					_accountName = reader.CurrentRow.Fields["acctname"];

					DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);

					if (!_dataIdentifiers.Contains(dataIdentifier))
					{
						// Delete old data in the DB for the same data we fetch the new data.			
						HandleDeleteDay();
						_dataIdentifiers.Add(dataIdentifier);
					}
				}
			}
		}

		protected virtual void CheckRowData(XmlTextReader xmlReader)
		{
			int accountID;
			// New date in the file.
			if (DayCode.GenerateDateTime(GetDate(xmlReader)) != _requiredDay)
			{
				_requiredDay = DayCode.GenerateDateTime(GetDate(xmlReader));

				DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);


				if (!_dataIdentifiers.Contains(dataIdentifier))
				{
					// Delete old data in the DB for the same data we fetch the new data.			
					HandleDeleteDay();
					_dataIdentifiers.Add(dataIdentifier);
				}
			}

			if (!string.IsNullOrEmpty(_accountName))
			{
				// New account in the file.
				if (xmlReader.GetAttribute("acctname").ToString() != _accountName)
				{
					GetAccountID(xmlReader.GetAttribute("acctname").ToString(), out accountID, _accountFieldName);

					_accountID = accountID;
					_accountName = xmlReader.GetAttribute("acctname").ToString();

					DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);

					if (!_dataIdentifiers.Contains(dataIdentifier))
					{
						// Delete old data in the DB for the same data we fetch the new data.			
						HandleDeleteDay();
						_dataIdentifiers.Add(dataIdentifier);
					}
				}
			}
		}

		protected virtual void InitalizeProcessData(out FieldElementSection rawDataFields, out FieldElementSection metaDataFields, out SqlCommand insertCommand, out Dictionary<string, string> gatewayNameFields)
		{
			// Get the sections with the fields for the insert command.
			rawDataFields = (FieldElementSection)ConfigurationManager.GetSection
				(GetConfigurationOptionsField("FieldsMapping"));
			metaDataFields = (FieldElementSection)ConfigurationManager.GetSection
				(GetConfigurationOptionsField("TableMapping"));

			// Initalize Insert command
			string sqlCmd = CreateInsertCmdString(_tableName, rawDataFields, metaDataFields);
			insertCommand = DataManager.CreateCommand(sqlCmd, CommandType.Text);
			insertCommand.CommandTimeout = 120;

			// Initalize a Dictionary with report fields & gateway Name Fields.			
			gatewayNameFields = new Dictionary<string, string>();
			InitalizeGatewayNameMapping(gatewayNameFields, "GatewayName");
		}

		/// <summary>
		/// Initalize row with raw data from the xml and meta 
		/// data (like gk fields) and insert the row to the DB.
		/// </summary>
		/// <param name="hasBackOffice">Indicate if the account have backoffice.</param>
		/// <param name="rawDataFields">Dictionary that contain all the required raw data fields from the DB.</param>
		/// <param name="metaDataFields">Dictionary that contain all the required Meta data fields from the DB.</param>
		/// <param name="insertCommand"></param>
		/// <param name="gatewayNameFields"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param>
		protected virtual void HandleRow(bool hasBackOffice, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields, SourceDataRowReader<RetrieverDataRow> reader)
		{
			InitalizeRawDataFromXml(rawDataFields, metaDataFields, insertCommand, reader);
			InitalizeMetaDataParameters(insertCommand, reader, hasBackOffice, gatewayNameFields);
			InsertRowToDB(insertCommand);
		}

		protected virtual void HandleRow(bool hasBackOffice, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields, XmlTextReader xmlReader)
		{
			InitalizeNullValues(insertCommand, rawDataFields, metaDataFields);
			InitalizeRawDataFromXml(rawDataFields, metaDataFields, insertCommand, xmlReader);
			InitalizeMetaDataParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);
			InsertRowToDB(insertCommand);
		}

		protected void InsertRowToDB(SqlCommand insertCommand)
		{
			int numOfRetries = 0;
			bool reportSaved = false;

			while (!reportSaved)
			{
				try
				{
					insertCommand.ExecuteNonQuery();
					reportSaved = true;
				}
				catch (Exception ex)
				{
					if (numOfRetries >= MaxInsertRetries)	// too many retries, bail out					
						throw ex;
					++numOfRetries;
				}
			}
		}

		/// <summary>
		/// Check if we got to the rows section in the xml acroding to the xmlRowName.
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns>True - we got to the rows section. False - not there yet.</returns>
		protected virtual bool GetToRowsSection(XmlReader xmlReader)
		{
			if (xmlReader.Name.ToLower() != _xmlRowName || !xmlReader.HasAttributes)
				return false;
			else
				return true;
		}

		/// <summary>
		/// Activate stored procedure SP_Content_Add_Total_Imps_Measure.  
		/// This SP deletes the relevant "content total imps" records by the accout_id, Channel_id & day_code
		/// and recalculates the missing imps between creative (search) data and content data, 
		/// Then adds the diff imps as new record in content table
		/// </summary>
		public void ActivateContentSP()
		{
			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Initalize stored procedure Sp_Delete_Table_Days.
				SqlCommand contentSPCmd =
					DataManager.CreateCommand("SP_Content_Add_Total_Imps_Measure(@Account_ID:NVarChar,@Day_Code:NVarChar, @Channel_ID:NVarChar)", CommandType.StoredProcedure);
				contentSPCmd.CommandTimeout = 30000;

				// Init Paramters
				contentSPCmd.Parameters["@Account_ID"].Value = _accountID;
				contentSPCmd.Parameters["@Day_Code"].Value = GetDayCode(_requiredDay);
				contentSPCmd.Parameters["@Channel_ID"].Value = _channelID;

				try
				{
					// Activate stored procedure.
					contentSPCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw new Exception("Error activate content stored procedure.", ex);
				}
			}
		}

		/// <summary>
		/// Get the date of the row from the xml and return it in day code format.
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns>Date of the row day code format.</returns>
		protected virtual int GetDate(SourceDataRowReader<RetrieverDataRow> reader)
		{
			int retriveredDate = ResolveInt(reader.CurrentRow.Fields["date"].Replace("-", string.Empty));
			return retriveredDate;
		}

		protected virtual int GetDate(XmlTextReader xmlReader)
		{
			int retriveredDate = ResolveInt(xmlReader.GetAttribute("date").Replace("-", string.Empty));
			return retriveredDate;
		}

		protected void InsertNullValue(SqlCommand insertCommand, FieldElement fe)
		{
		
			// Initalize field with null value.
			if (insertCommand.Parameters["@" + fe.DBFieldName].Value != null && 
				insertCommand.Parameters["@" + fe.DBFieldName].Value.ToString() != string.Empty)
				return;
			
			if (fe.DBDefaultValue == null)
				insertCommand.Parameters["@" + fe.DBFieldName].Value = DBNull.Value;
			else
				insertCommand.Parameters["@" + fe.DBFieldName].Value =
					Convert.ChangeType(fe.DBDefaultValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));

		}

		/// <summary>
		/// Initalize the row raw data fields with values from the xml. 
		/// And initalize Meta Data fields with null values.
		/// </summary>
		/// <param name="rawDataFields">Dictionary that contain all the required raw data fields from the DB.</param>
		/// <param name="metaDataFields">Dictionary that contain all the required Meta data fields from the DB.</param>
		/// <param name="insertCommand"></param>
		/// <param name="xmlReader"></param>
		protected virtual void InitalizeRawDataFromXml(FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, SourceDataRowReader<RetrieverDataRow> reader)
		{
			// Initalize the insertCommand with the values from the xmlReader.
			foreach (FieldElement fe in rawDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				if (!String.IsNullOrEmpty(reader.CurrentRow.Fields[fe.Value]))
				{
					insertCommand.Parameters["@" + fe.DBFieldName].Value =
						Convert.ChangeType(reader.CurrentRow.Fields[fe.Value], Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));

					InitalizeGatewayGK(insertCommand, reader, fe);
				}
				else
					InsertNullValue(insertCommand, fe);
			}

			// Initalize Meta Data fields with null values.
			foreach (FieldElement fe in metaDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				InsertNullValue(insertCommand, fe);
			}
		}

		protected virtual void InitalizeRawDataFromXml(FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, XmlTextReader xmlReader)
		{
			// Initalize the insertCommand with the values from the xmlReader.
			foreach (FieldElement fe in rawDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				if (!String.IsNullOrEmpty(xmlReader.GetAttribute(fe.Value)))
				{
					insertCommand.Parameters["@" + fe.DBFieldName].Value =
						Convert.ChangeType(xmlReader.GetAttribute(fe.Value), Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));

					InitalizeGatewayGK(insertCommand, xmlReader, fe);
				}
				else
					InsertNullValue(insertCommand, fe);
			}

			// Initalize Meta Data fields with null values.
			foreach (FieldElement fe in metaDataFields.Fields)
			{
				if (!fe.InsertToDB || !fe.Enabled)
					continue;

				InsertNullValue(insertCommand, fe);
			}
		}


		/// <summary>
		/// Initazlie common meta data parameters.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param>
		/// <param name="hasBackOffice">Indicate if the account have backoffice.</param>
		/// <param name="gatewayNameFields"></param>
		protected virtual void InitalizeMetaDataParameters(SqlCommand insertCommand,
			XmlTextReader xmlReader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			insertCommand.Parameters["@Day_Code"].Value = Easynet.Edge.Core.Utilities.DayCode.ToDayCode(_requiredDay);
			insertCommand.Parameters["@Downloaded_Date"].Value = DateTime.Now;
			insertCommand.Parameters["@account_ID"].Value = _accountID;
		}

		protected virtual void InitalizeMetaDataParameters(SqlCommand insertCommand,
			SourceDataRowReader<RetrieverDataRow> reader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			insertCommand.Parameters["@Day_Code"].Value = DayCode.ToDayCode(_requiredDay);
			insertCommand.Parameters["@Downloaded_Date"].Value = DateTime.Now;
			insertCommand.Parameters["@account_ID"].Value = _accountID;
		}


		protected virtual void InitalizeAdwordsParameters(SqlCommand insertCommand,
			SourceDataRowReader<RetrieverDataRow> reader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			#region Obsolete - delete data by AggregationType.
			//if (_aggregationType == AggregationType.Month)			
			//    insertCommand.Parameters["@Day_Code"].Value = DayCodeSummary();
			//else
			//    insertCommand.Parameters["@Day_Code"].Value = retriveredDate;	
			#endregion

			insertCommand.Parameters["@Channel_ID"].Value = _channelID;

			// Divide by 1000000 relevent Parameters.
			insertCommand.Parameters["@ctr"].Value =
				ResolveDouble(reader.CurrentRow.Fields["ctr"]) / DivideFieldsValue;
			insertCommand.Parameters["@cpc"].Value =
				ResolveDouble(reader.CurrentRow.Fields["cpc"]) / DivideFieldsValue;
			insertCommand.Parameters["@cost"].Value =
				ResolveDouble(reader.CurrentRow.Fields["cost"]) / DivideFieldsValue;
			insertCommand.Parameters["@budget"].Value =
				ResolveDouble(reader.CurrentRow.Fields["budget"]) / DivideFieldsValue;

			//insertCommand.Parameters["@maxCpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxCpm")) / DivideFieldsValue;
			//insertCommand.Parameters["@maxContentCpc"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxContentCpc")); // Yaniv : maybe need to divide this as well in 1000000
			//insertCommand.Parameters["@cpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("cpm")) / DivideFieldsValue;

			// Fields to remove in the future.
			insertCommand.Parameters["@customerid"].Value =
				ResolveLong(reader.CurrentRow.Fields["campaignid"]);
			insertCommand.Parameters["@Account_ID_SRC"].Value = _accountID;
			insertCommand.Parameters["@date"].Value = DateTime.Now;

			HandleMatchType(insertCommand, reader);
		}


		protected virtual void InitalizeAdwordsParameters(SqlCommand insertCommand,
			XmlTextReader xmlReader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			#region Obsolete - delete data by AggregationType.
			//if (_aggregationType == AggregationType.Month)			
			//    insertCommand.Parameters["@Day_Code"].Value = DayCodeSummary();
			//else
			//    insertCommand.Parameters["@Day_Code"].Value = retriveredDate;	
			#endregion

			insertCommand.Parameters["@Channel_ID"].Value = _channelID;

			// Divide by 1000000 relevent Parameters.
			insertCommand.Parameters["@ctr"].Value =
				ResolveDouble(xmlReader.GetAttribute("ctr")) / DivideFieldsValue;
			insertCommand.Parameters["@cpc"].Value =
				ResolveDouble(xmlReader.GetAttribute("cpc")) / DivideFieldsValue;
			insertCommand.Parameters["@cost"].Value =
				ResolveDouble(xmlReader.GetAttribute("cost")) / DivideFieldsValue;
			insertCommand.Parameters["@budget"].Value =
				ResolveDouble(xmlReader.GetAttribute("budget")) / DivideFieldsValue;

			//insertCommand.Parameters["@maxCpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxCpm")) / DivideFieldsValue;
			//insertCommand.Parameters["@maxContentCpc"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("maxContentCpc")); // Yaniv : maybe need to divide this as well in 1000000
			//insertCommand.Parameters["@cpm"].Value =
			//    ResolveDouble(xmlReader.GetAttribute("cpm")) / DivideFieldsValue;

			// Fields to remove in the future.
			insertCommand.Parameters["@customerid"].Value =
				ResolveLong(xmlReader.GetAttribute("campaignid"));
			insertCommand.Parameters["@Account_ID_SRC"].Value = _accountID;
			insertCommand.Parameters["@date"].Value = DateTime.Now;

			HandleMatchType(insertCommand, xmlReader);
		}

	
		/// <summary>
		/// Hi M8, This function create a Dictionary with the fields from the required 
		/// section in the configuration.
		/// </summary>
		/// <param name="fields">A Dictionary that contain the fields from the required 
		/// section in the configuration.</param>
		/// <param name="sectionName">The section name from the configuration to load.</param>
		protected virtual void InitalizeGatewayNameMapping(Dictionary<string, string> fields, string sectionName)
		{
			//***************************
			// ANTI-YANIV HACK
            ServiceInstanceInfo targetInstance = Instance;
            string trackerParamsRaw = string.Empty;
			if (sectionName == "GatewayName")
			{
                while (targetInstance != null)
                {
                    trackerParamsRaw = targetInstance.Configuration.Options["TrackingParameters"];
                    if (string.IsNullOrEmpty(trackerParamsRaw))
                        targetInstance = targetInstance.ParentInstance;
                    else
                        break;                    
                }
				 
				if (trackerParamsRaw != null)
				{
					string[] trackerParamPairs = trackerParamsRaw.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string trackerParamPair in trackerParamPairs)
					{
						string[] pair = trackerParamPair.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						if (pair.Length < 2)
							continue;
						else
							fields.Add(pair[0], pair[1]);
					}

					// If we found anything ignore the rest of this horrible function
                    if (fields.Count >= 1) //fix by alon changed from >1 to >=1 we need at least 1 25/11
						return;
				}
			}
			// ANTI-YANIV HACK
			//***************************

			try
			{
				// Load the proper report paramters by the report name.
				FieldElementSection fes = (FieldElementSection)ConfigurationManager.GetSection(sectionName);

				// Initalize the hash table with the fields of the the report.
				foreach (FieldElement fe in fes.Fields)			
					fields.Add(fe.Key, fe.Value);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Error get configuration Data from {0} section.", sectionName), ex);
			}		
		}

		/// <summary>
		/// Create a string which represents insert command from the configuration.
		/// The command created from 2 sections: mappingSection & tableSection.
		/// First we add the fields names and then we add there parmeters with their field type.
		/// </summary>
		/// <param name="tableName">The table name for the insert command.</param>
		/// <param name="rawDataFields"> This section in the configuration contain all the
		/// fields we get from the xmland map each field in the xml to a field in the db.</param>
		/// <param name="metaDataFields"> This section in the configuration contain all the
		/// fields of meta data like account_id, date, gk fields and etc.</param>		
		/// <returns>insert command string.</returns>
		protected virtual string CreateInsertCmdString(string tableName, FieldElementSection rawDataFields, FieldElementSection metaDataFields)
		{
			try
			{
				char[] charsToTrim = { ',' };
				string sqlCmd = "INSERT INTO " + tableName + "(";

				// Add fields' names
				// =============

				// Add fields' names from mapping Section
				foreach (FieldElement fe in rawDataFields.Fields)
				{
					if (fe.Enabled && fe.InsertToDB)
						sqlCmd += " " + fe.DBFieldName + ",";
				}

				// Add the field names from table Section
				foreach (FieldElement fe in metaDataFields.Fields)
				{
					if (fe.Enabled && fe.InsertToDB)
						sqlCmd += " " + fe.DBFieldName + ",";
				}

				// Remove last ,
				sqlCmd = sqlCmd.TrimEnd(charsToTrim);
				sqlCmd += ") VALUES ( ";

				// Add fields' values and types
				// ==============================

				// Add fields' values and types from mapping Section
				foreach (FieldElement fe in rawDataFields.Fields)
				{
					if (fe.Enabled && fe.InsertToDB)
						sqlCmd += " @" + fe.DBFieldName + ":" + fe.DBFieldType + ",";
				}

				// Add the field values from table Section
				foreach (FieldElement fe in metaDataFields.Fields)
				{
					if (fe.Enabled && fe.InsertToDB)
						sqlCmd += " @" + fe.DBFieldName + ":" + fe.DBFieldType + ",";
				}

				sqlCmd = sqlCmd.TrimEnd(charsToTrim);
				sqlCmd += ")";
				return sqlCmd;
			}
			catch (Exception ex)
			{
				throw new Exception("Error fetch data from the configuration.", ex);
			}
		}

		protected virtual void InitalizeChannelID()
		{
			_channelID = Convert.ToInt32(GetConfigurationOptionsField("ChannelID"));
		}

		protected virtual void HandleDeleteDay()
		{
			DeleteDay(Core.Utilities.DayCode.ToDayCode(_requiredDay), _tableName);
		}

		/// <summary>
		/// Initalize the required day for processing the data.
		/// We check for manual date or lastday date.
		/// Every service has his own default required day.
		/// </summary>
		/// <param name="_requiredDay">return the required date to process the data.</param>
		protected virtual void InitalizeRequiredDate()
		{
			// Check if we have several days to run
			ArrayList dates = new ArrayList();
						
			// Check if we need to get manual date.
			_requiredDay = DateTime.Today.AddDays(_defaultMinusRequiredDate);
			CheckManualDate();
		}


		/// <summary>
		/// Initalize every need data before process the xml including:
		/// - Check if need to fetch data from file.
		/// - Check if xml report file exist.
		/// - Initalize Required Date
		/// - delete old data.
		/// </summary>
		/// <returns>The path of the xml report file.</returns>
		protected virtual bool InitalizeServiceData(ref bool rangeDate, ArrayList filesPaths)
		{
			if (!InitalizeAccountID())
				return false;

			rangeDate = false;
			InitalizeChannelID();

			string filePath = GetConfigurationOptionsField("FilePath");
			string fileDate = GetConfigurationOptionsField("FileDate");

			// Load a specific xml file from the configuration
			if ((!String.IsNullOrEmpty(filePath)) &&
				(!String.IsNullOrEmpty(fileDate)))
			{
				Log.Write(string.Format("Fetch the file {0}.", filePath), LogMessageType.Information);
				filesPaths.Add(filePath);

				// Check if the file exist.
				if (!File.Exists(filePath))
					throw new Exception(string.Format("file {0} doesn't exist.", filePath));

				StringToDate(fileDate, out _requiredDay);
				return true;
			}
			else
				GetReportPath(filesPaths, _serviceType);

			//if (!File.Exists(xmlPath))
			//    throw new Exception(string.Format("Xml file doesn't exist in the path {0}.", xmlPath));

			// _aggregationType = GetConfigurationOptionsField("AggregationType");

			if (filesPaths.Count > 1)
				rangeDate = true;
			else
				InitalizeRequiredDate();

			return true;
		}

		/*=========================*/
		#endregion

		#region Private Methods
		/*=========================*/

		private void RunProcess(string xmlPath, string defaultErrorSubDirPath)
		{
			Log.Write(string.Format("Run processor for service {2}, for account {0}, for date {1}.", _accountID.ToString(), _requiredDay.ToShortDateString(), Instance.ParentInstance.Configuration.Name), LogMessageType.Information);

			ProccessData(xmlPath, defaultErrorSubDirPath);

			foreach (DataIdentifier dataIdentifier in _dataIdentifiers)
			{
				_requiredDay = DayCode.GenerateDateTime(dataIdentifier.DayCode);
				_accountID = dataIdentifier.AccountID;
				HandledContentSP();
			}

			if (!String.IsNullOrEmpty(ErrorFilePath))
				Log.Write(string.Format("Error file {0} was created for date {1}.", ErrorFilePath, _requiredDay.ToShortDateString()), LogMessageType.Warning);
		}

		/*=========================*/
		#endregion

		#region Protected Methods
		/*=========================*/

		protected void InitalizeAnalyticsGKFields(SqlCommand cmd, int channelID, bool hasBackOffice, int accountID)
		{
			InitalizeKeywordGK(cmd, accountID);

			// Future use
			//InitalizeCampaignGK(insertCommand, channelID, accountID);
			//InitalizeAdGroupGK(insertCommand, channelID, accountID, (long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value));
			//InitalizeAdgroupKeywordGK(insertCommand, accountID, channelID, hasBackOffice);
		}

		protected void InitalizeContentGKFields(SqlCommand insertCommand, int channelID, bool hasBackOffice, int accountID)
		{
			InitalizeCampaignGK(insertCommand, channelID, accountID);
			InitalizeAdGroupGK(insertCommand, channelID, accountID, (long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value));
			InitalizeSiteGK(insertCommand, accountID);
			InitalizePPCSiteGK(insertCommand, channelID, accountID, false);
		}

		protected void InitalizeCreativeGKFields(SqlCommand insertCommand,
			int channelID, bool hasBackOffice, int accountID)
		{
			InitalizeCampaignGK(insertCommand, channelID, accountID);
			InitalizeAdGroupGK(insertCommand, channelID, accountID, (long)Convert.ToInt32(insertCommand.Parameters["@Campaign_GK"].Value));

			InitalizeKeywordGK(insertCommand, accountID);
			InitalizeCreativeGK(insertCommand, accountID);

			// Initalize backoffice fields.
			if (hasBackOffice)
			{
				if (insertCommand.Parameters["@kwDestUrl"].Value == DBNull.Value)
					insertCommand.Parameters["@kwDestUrl"].Value = string.Empty;

				// Initalzie Gateway Reference Type by the kwDestUrl parameter.
				GatewayReferenceType gatewayReferenceType =
					Uri.IsWellFormedUriString(insertCommand.Parameters["@kwDestUrl"].Value.ToString(), UriKind.Absolute) ?
					GatewayReferenceType.Keyword :
					GatewayReferenceType.Creative;

				// Gateway_gk
				if ((insertCommand.Parameters["@Keyword_GK"].Value == DBNull.Value) ||
					(insertCommand.Parameters["@Creative_gk"].Value == DBNull.Value))
					return;

				InitalizeGatewayGK(insertCommand, accountID, gatewayReferenceType, channelID);
			}

			InitalizeAdgroupCreativeGK(insertCommand, accountID, channelID, hasBackOffice);
			InitalizeAdgroupKeywordGK(insertCommand, accountID, channelID, hasBackOffice);
		}


		/// <summary>
		/// Check if there are rows in the content table 
		/// for the current day and account.
		/// </summary>
		/// <param name="dayCode"></param>
		/// <returns></returns>
		protected bool ContentDataExist(int dayCode)
		{
			// Initalize select command to get table fields.
			SqlCommand selectCommand = DataManager.CreateCommand(
				@"
				select top 1 (site_gk)  
					from Paid_API_Content
				where 
					account_id = @Account_id:Int and  
					channel_id = @channel_id:Int and  
					day_code = @Day_code:Int and
					site_gk is not null", CommandType.Text);

			selectCommand.Parameters["@Account_id"].Value = _accountID;
			selectCommand.Parameters["@Day_code"].Value = dayCode;
			selectCommand.Parameters["@channel_id"].Value = _channelID;

			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(selectCommand);

				try
				{
					object result = selectCommand.ExecuteScalar();
					if (result != null)
						return true;
					else
						return false;
				}
				catch (Exception ex)
				{
					return false;
					//throw new Exception("Can't check if there is content data in Paid_API_AllColumns.", ex);
				}
			}
		}

		/*=========================*/
		#endregion

		#region Override Methods
		/*=========================*/

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork()
		{
            if(_tableName.Equals(""))
                _tableName = GetConfigurationOptionsField("TableName");
			ArrayList filesPaths = new ArrayList();
			bool rangeDate = false;
			if (!InitalizeServiceData(ref rangeDate , filesPaths))
				return ServiceOutcome.Failure;
			
			if (rangeDate)
			{
				for (int i = 0; i < filesPaths.Count; ++i)
				{
					_requiredDay = GetDateFromFilePath(filesPaths[i].ToString());
					RunProcess(filesPaths[i].ToString(), _defaultErrorSubDirPath);
					ErrorFilePath = string.Empty;
				}
			}
			else
			{
				if (filesPaths.Count > 0)
				{
					_requiredDay = GetDateFromFilePath(filesPaths[0].ToString());
					RunProcess(filesPaths[0].ToString(), _defaultErrorSubDirPath);
				}
			}

			return ServiceOutcome.Success;
		}

		/// <summary>
		/// Handle abort event - delete all data we insert.
		/// </summary>
		protected override void OnEnded(ServiceOutcome serviceOutcome)
		{

			if ((serviceOutcome == ServiceOutcome.Aborted) ||
				(serviceOutcome == ServiceOutcome.Failure))
			{
				#region Obsolete - delete data by AggregationType.
				// Delete old data on the same day.
				//if (_aggregationType == AggregationType.Empty)
				//{
				//}
				//else if (_aggregationType == AggregationType.Month) // Monthly report
				//{
				//    DeleteDay(DayCodeSummary(), AdWordsCreativeTable);
				//}
				//else // Daily report
				//{
				//    DeleteDay(DayCode(_requiredDay), AdWordsCreativeTable);
				//}
				#endregion

				DeleteDay(GetDayCode(_requiredDay), _tableName);
			}
		}

		/*=========================*/
		#endregion   

	}
}
