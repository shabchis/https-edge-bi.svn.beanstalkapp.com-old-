using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.DataRetrieval.DataReader;
using Easynet.Edge.Core.Data;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	public class AnalyticsProcessor : BaseProcessor
	{
		#region Consts
		/*=========================*/

		private const string XmlRowName = "entry";
		private const string TableName = "GAnalytics";
		private const string ServiceType = "Analytics";
		private const string DefaultErrorSubDirPath = @"Analytics\Errors\";
		private const int DefaultMinusRequiredDate = 1;
		private const string AnanlyticsNullValue = "(not set)";

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		private long	_profileID;
		private string	_profileName = string.Empty;
		private bool	_getToRowsSection = false;

		/*=========================*/
		#endregion

		#region constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public AnalyticsProcessor()
		{
			_xmlRowName = XmlRowName;
			_tableName = TableName;
			_serviceType = ServiceType;
			_defaultErrorSubDirPath = DefaultErrorSubDirPath;
			_defaultMinusRequiredDate = DefaultMinusRequiredDate;
		}

		/*=========================*/
		#endregion

		#region Private Methods
		/*=========================*/

		/// <summary>
		/// Get the profile name from the node "dxp:tableName".
		/// </summary>
		/// <param name="xmlReader"></param>
		private void GetProfileName(XmlReader xmlReader) //(XmlTextReader xmlReader)
		{
			if (xmlReader.Name == "dxp:tableName")
			{
				xmlReader.Read();
				_profileName = xmlReader.Value;
			}
		}

		/// <summary>
		/// if the denominator != 0 then return numerator/denominator
		/// </summary>
		/// <param name="numerator"></param>
		/// <param name="denominator"></param>
		/// <returns>numerator/denominator</returns>
		private double DivResult(object numerator, object denominator)
		{
			return Convert.ToInt32(denominator) == 0 ? 0 : Convert.ToDouble(numerator) / Convert.ToDouble(denominator);
		}

		/// <summary>
		/// Initalize the relevent field (fieldName) with fieldValue.
		/// </summary>
		/// <param name="fieldName">The name of the field to initalize.</param>
		/// <param name="fieldValue">The value of to set in the required field.</param>
		/// <param name="rawDataFields">FieldElementSection that contain all the required raw data fields from the DB.</param>
		/// <param name="insertCommand"></param>
		private void HandleNode(string fieldName, string fieldValue, FieldElementSection rawDataFields, SqlCommand insertCommand)
		{
			if (rawDataFields.Fields[fieldName] != null && 
				rawDataFields.Fields[fieldName].InsertToDB && 
				rawDataFields.Fields[fieldName].Enabled)
			{
				if (fieldValue != AnanlyticsNullValue)
				{
					insertCommand.Parameters["@" + rawDataFields.Fields[fieldName].DBFieldName].Value =
						Convert.ChangeType(fieldValue, Type.GetType(_tableTypeMappings[rawDataFields.Fields[fieldName].DBFieldType.ToLower()]));
				}
				else
				{
					// Initalize null value.
					if (rawDataFields.Fields[fieldName].DBDefaultValue == null)
						insertCommand.Parameters["@" + rawDataFields.Fields[fieldName].DBFieldName].Value = DBNull.Value;
					else
						insertCommand.Parameters["@" + rawDataFields.Fields[fieldName].DBFieldName].Value =
							Convert.ChangeType(rawDataFields.Fields[fieldName].DBDefaultValue, Type.GetType(_tableTypeMappings[rawDataFields.Fields[fieldName].DBFieldType.ToLower()]));
				}
			}
		}

		/*=========================*/
		#endregion
		
		#region Empty Override Methods
		/*=========================*/


		/// <summary>
		/// Analytics don't get channel ID from the configuration, it take it from source field 
		/// from the xml for each row.
		/// </summary>
		protected override void InitalizeChannelID()
		{
		}

		protected override void CheckRowData(XmlTextReader xmlReader)
		{
		}

		protected override void CheckRowData(SourceDataRowReader<RetrieverDataRow> reader)
		{
		}


		/// <summary>
		/// Analytics Doesn't use Gateway Name Mapping.
		/// </summary>
		/// <param name="fields"></param>
		/// <param name="sectionName"></param>
		protected override void InitalizeGatewayNameMapping(Dictionary<string, string> fields, string sectionName)
		{
		}

		/// <summary>
		/// We don't get CampaignID from Analytics.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <returns></returns>
		protected override long FetchCampaignID(SqlCommand insertCommand)
		{
			return 0;
		}

		/// <summary>
		/// We don't get AdGroupID from Analytics.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <returns></returns>
		protected override long? GetAdGroupID(SqlCommand insertCommand)
		{
			return null;
		}

		/*=========================*/
		#endregion

		#region Override Methods
		/*=========================*/

		/// <summary>
		/// Get profile ID from the configuration.
		/// </summary>
		/// <returns></returns>
		protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
		{
			// Get profile ID from configuration and convert it to long.
			if (!Int64.TryParse(GetConfigurationOptionsField("profileID"), out _profileID))
				throw new Exception(string.Format("Failed to convert profile ID {1} to long account {0}", _accountID, GetConfigurationOptionsField("profileID")));

			return base.DoWork();
		}
        protected void DeleteDayAnalytics(int dayCode, string tableName, int accountID, long profileID)
		{
			using (ConnectionKey key = DataManager.Current.OpenConnection())
			{
				// Initalize stored procedure Sp_Delete_Table_Days.
                SqlCommand deleteDayStoredProcedureCmd = DataManager.CreateCommand("Sp_Delete_Table_Days_GAnalytics_no_channel(@Table_Name:NVarChar, @Account_ID:NVarChar,@Day:NVarChar, @Profile_ID:NVarChar)", CommandType.StoredProcedure);
				deleteDayStoredProcedureCmd.CommandTimeout = 30000;

				// Init Parameters
				deleteDayStoredProcedureCmd.Parameters["@Table_Name"].Value = tableName;
				deleteDayStoredProcedureCmd.Parameters["@Account_ID"].Value = accountID;
				deleteDayStoredProcedureCmd.Parameters["@Day"].Value = dayCode.ToString();
                deleteDayStoredProcedureCmd.Parameters["@Profile_ID"].Value = profileID.ToString();

				try
				{
					deleteDayStoredProcedureCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					throw new Exception("Error activate stored procedure Delete Table Days.", ex);
				}
			}
		}
		/// <summary>
		/// Delete table without channel ID.
		/// </summary>
		protected override void HandleDeleteDay()
		{
            if (!Int64.TryParse(GetConfigurationOptionsField("profileID"), out _profileID))
                throw new Exception(string.Format("Failed to convert profile ID {1} to long account {0}", _accountID, GetConfigurationOptionsField("profileID")));
            DeleteDayAnalytics(GetDayCode(_requiredDay), _tableName, _accountID, _profileID);
		}

		/// <summary>
		/// Used to mark that we finish to process the xml. if it's re run and we 
		/// process another xml we serach again for the rows section.
		/// </summary>
		protected override void HandledContentSP()
		{
			_getToRowsSection = false;
		}

		/// <summary>
		/// Transform _requiredDay to day_code format instead od getting it from the xml.
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns>required Day in DayCode format.</returns>
		protected override int GetDate(XmlTextReader xmlReader)
		{
			return GetDayCode(_requiredDay);
		}

		/// <summary>
		/// Initalize and calculate all relevent parmaeters for analytics including gk fields.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param>
		/// <param name="hasBackOffice">Indicate if the account have backoffice. - for future use.</param>
		/// <param name="gatewayNameFields"></param>
		protected override void InitalizeMetaDataParameters(SqlCommand cmd,
			XmlTextReader xmlReader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			base.InitalizeMetaDataParameters(cmd, xmlReader, hasBackOffice, gatewayNameFields);

			int channelID = GetChannelID(cmd.Parameters["@source"].Value.ToString());
			cmd.Parameters["@channel_ID"].Value = channelID;

			// Initalize calculated parmeters.
			cmd.Parameters["@avgPageViewPerVisit"].Value = DivResult(cmd.Parameters["@pageviews"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@avgNewVisits"].Value = DivResult(cmd.Parameters["@newVisits"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@bounceRate"].Value = DivResult(cmd.Parameters["@bounces"].Value, cmd.Parameters["@entrances"].Value);
			cmd.Parameters["@goal1ConversionRate"].Value = DivResult(cmd.Parameters["@goal1Completions"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@goal2ConversionRate"].Value = DivResult(cmd.Parameters["@goal2Completions"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@goal3ConversionRate"].Value = DivResult(cmd.Parameters["@goal3Completions"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@goal4ConversionRate"].Value = DivResult(cmd.Parameters["@goal4Completions"].Value, cmd.Parameters["@visits"].Value);
			cmd.Parameters["@avgTimeOnSiteSeconds"].Value = Math.Round(DivResult(cmd.Parameters["@timeOnSite"].Value, cmd.Parameters["@visits"].Value));
			cmd.Parameters["@avgTimeOnSiteMinutes"].Value = Math.Round(DivResult(cmd.Parameters["@timeOnSite"].Value, cmd.Parameters["@visits"].Value)) / 60;
			cmd.Parameters["@profileID"].Value = _profileID;
			cmd.Parameters["@profileName"].Value = _profileName;

			InitalizeAnalyticsGKFields(cmd, channelID, hasBackOffice, _accountID);
		}
	
		/// <summary>
		/// Check if we got to the rows section in the xml acroding to the xmlRowName.
		/// Also initalize the profile name and required date from the xml (before we get to the rows section).
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns>True - we gor to the rows section. False - not there yet.</returns>
		protected override bool GetToRowsSection(XmlReader xmlReader)
		{
			// Get profile name from xml.
			if (_profileName == string.Empty)
				GetProfileName(xmlReader);

			// Get required date from xml.
			if (xmlReader.Name == "dxp:startDate" && xmlReader.NodeType != XmlNodeType.EndElement)
			{
				xmlReader.Read();
				string tempDate = xmlReader.Value.Replace("-", string.Empty);
				StringToDate(tempDate, out _requiredDay);
			}

			// Check if we reach to rows section.
			if (xmlReader.Name.ToLower() != _xmlRowName && !_getToRowsSection)
				return false;
			else
			{
				_getToRowsSection = true;
				return true;
			}
		}

		/// <summary>
		/// Get all the fields from the xml row. when we get to the end of the row
		/// we initalzie null values and all the required meta data fields.s
		/// </summary>
		/// <remarks>
		/// Analytics works in nodes format and not attribute format like adwords.
		/// That mean that each row contain nodes which are the fields of the row.
		/// Each node that is dimension or metric is being handled.
		/// </remarks>
		/// <param name="hasBackOffice"></param>
		/// <param name="rawDataFields"></param>
		/// <param name="metaDataFields"></param>
		/// <param name="insertCommand"></param>
		/// <param name="gatewayNameFields"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param> 
		protected override void HandleRow(bool hasBackOffice, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields, XmlTextReader xmlReader)
		{
			switch (xmlReader.NodeType)
			{
				case XmlNodeType.Element:
					if (xmlReader.Name == "dxp:dimension" || xmlReader.Name == "dxp:metric")
						HandleNode(xmlReader.GetAttribute("name"), xmlReader.GetAttribute("value"), rawDataFields, insertCommand);
					break;

				case XmlNodeType.EndElement:
					// Arrived to end of row.
					if (xmlReader.Name.ToLower().Contains(XmlRowName.ToLower()))
					{						
						//InitalizeNullValues(metaDataFields, insertCommand, xmlReader);
						InitalizeMetaDataParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);

						// Execute command.
						InsertRowToDB(insertCommand);
					}
					break;
				default:
					break;
			}
		}

		/*=========================*/
		#endregion
	}
}
