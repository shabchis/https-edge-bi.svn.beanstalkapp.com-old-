using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.BusinessObjects;
using System.Xml;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	class BackOfficeProcessorNodes : BaseProcessor
	{
		#region Consts
		/*=========================*/

		private const string XmlRowName = "tracker";
		private const string TableName = "BackOffice_Client_Gateway";
		private const string ServiceType = "BackOffice";
		private const string DefaultErrorSubDirPath = @"BackOffice\Errors\";
		private const int EasyForexAccountID = 7;

		/*=========================*/
		#endregion   

		#region Fields
		/*=========================*/

		private string _rowName = string.Empty;

		/*=========================*/
		#endregion   

		#region constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public BackOfficeProcessorNodes()
		{
			_xmlRowName = XmlRowName;
			_tableName = TableName;
			_serviceType = ServiceType;
			_defaultErrorSubDirPath = DefaultErrorSubDirPath;
		}

		/*=========================*/
		#endregion   

		#region Empty Override Methods
		/*=========================*/

		protected override void CheckRowData(XmlTextReader xmlReader)
		{
		}

		protected override void CheckRowData(SourceDataRowReader<RetrieverDataRow> reader)
		{
		}

		/// <summary>
		/// BackOffice delete date using backoffice cleaner. 
		/// </summary>
		protected override void HandleDeleteDay()
		{
			// Dont' delete data for east forex (account 7) because it has 2 retrievers.
            if (_accountID != EasyForexAccountID)
            {
                //_tableName = TableName;
                DeleteDayBO(GetDayCode(_requiredDay), _tableName);
            }
		}

		/// <summary>
		/// BackOffice don't get channel ID from the configuration.
		/// </summary>
		protected override void InitalizeChannelID()
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

		/*=========================*/
		#endregion   

		#region Override Methods
		/*=========================*/

		// TODO: fetch the date from the backoffice xml
		protected override int GetDate(XmlTextReader xmlReader)
		{
			return GetDayCode(_requiredDay);
		}

		protected override int GetDate(SourceDataRowReader<RetrieverDataRow> reader)
		{
			return GetDayCode(_requiredDay);
		}

		protected virtual void HandleBackOfficeNode(string nodeName, string nodeValue, FieldElementSection rawDataFields, SqlCommand insertCommand)
		{
			try
			{
				if (nodeName == String.Empty ||
					nodeName == null)
					throw new ArgumentException("Invalid node name. Cannot be null or empty.");

				FieldElement fe = rawDataFields.Fields[nodeName];
				//if (fe != null)
				//    insertCommand.Parameters["@" + fe.DBFieldName].Value =
				//        Convert.ChangeType(nodeValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));


				if (fe == null || !fe.InsertToDB || !fe.Enabled)
					return;

				if (!String.IsNullOrEmpty(nodeValue))
				{
					insertCommand.Parameters["@" + fe.DBFieldName].Value =
						Convert.ChangeType(nodeValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));

					// Initalize Gateway_GK
					if (rawDataFields.Fields[nodeName].DBFieldName.ToLower() == "gateway_id")
						insertCommand.Parameters["@Gateway_GK"].Value =
							GkManager.GetGatewayGK(_accountID, Convert.ToInt64(nodeValue));
				}
				else
				{
					// Initalize field with null value.
					if (fe.DBDefaultValue == null)
						insertCommand.Parameters["@" + fe.DBFieldName].Value = DBNull.Value;
					else
						insertCommand.Parameters["@" + fe.DBFieldName].Value =
							Convert.ChangeType(fe.DBDefaultValue, Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));
				}
			}
			catch (Exception ex)
			{
				string str = ex.Message;
			}
		}

		/// <summary>
		/// Get all the fields from the xml row. when we get to the end of the row
		/// we initalzie null values and all the required meta data fields.
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
			string nodeName = string.Empty;
			// Loop on all the rows in the xml report file.		
			switch (xmlReader.NodeType)
			{
				case XmlNodeType.Element:
					nodeName = xmlReader.Name;
					//Virtual function to handle the actual node value. If some processing is needed on this
					//value, inherit from this class, and override the HandleBackOfficeNode function.
					if (!xmlReader.Name.ToLower().Contains(_rowName))
					{
						xmlReader.Read();
						HandleBackOfficeNode(nodeName, xmlReader.Value, rawDataFields, insertCommand);
					}
					break;
				//case XmlNodeType.Text:
				//	break;
				case XmlNodeType.EndElement:
					// Arrived to end of row.
					if (xmlReader.Name.ToLower().Contains(_rowName.ToLower()))
					{
						InitalizeMetaDataParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);

						// Execute command.
						insertCommand.ExecuteNonQuery();

						InitalizeNullValues(insertCommand, rawDataFields, metaDataFields);
					}

					break;
				default:
					break;
			}
		}
	
		/// <summary>
		/// Check if we got to the rows section in the xml acroding to the xmlRowName.
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns>True - we got to the rows section. False - not there yet.</returns>
		protected override bool GetToRowsSection(XmlReader xmlReader)
		{
			if (_rowName == string.Empty)
			{
				_rowName = GetConfigurationOptionsField("RowName").ToLower();
				return false;
			}
		
			return true;
		}

		/// <summary>
		/// Initalize GatewayGK by the gateway_id fields for data from backoffice.
		/// </summary>
		/// <param name="insertCommand">The field GatewayGK in the insert command will be initalized.</param>
		/// <param name="xmlReader">The xml reader that contain the gateway_id value.</param>
		/// <param name="fe">The current field we fetach from xml reader.</param>
		protected override void InitalizeGatewayGK(SqlCommand insertCommand, XmlTextReader xmlReader, FieldElement fe)
		{
			if (fe.DBFieldName.ToLower() == "gateway_id") 
				insertCommand.Parameters["@Gateway_GK"].Value =
					GkManager.GetGatewayGK(_accountID, Convert.ToInt64(xmlReader.GetAttribute(fe.DBFieldName)));
		}

		protected override void InitalizeGatewayGK(SqlCommand insertCommand, SourceDataRowReader<RetrieverDataRow> reader, FieldElement fe)
		{
			if (fe.DBFieldName.ToLower() == "gateway_id")
				insertCommand.Parameters["@Gateway_GK"].Value =
					GkManager.GetGatewayGK(_accountID, Convert.ToInt64(reader.CurrentRow.Fields[fe.Key]));
		}

		//protected override void ReadFile(string xmlPath, string defaultErrorSubDirPath, bool hasBackOffice, ref bool xmlFileEmpty, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields)
		//{
		//    bool firstRowInXml = true;
		//    if (string.IsNullOrEmpty(GetConfigurationOptionsField("ReaderType")))
		//        throw new Exception("The service doesn't have attribue 'ReaderType'.");

		//    Type t = Type.GetType(GetConfigurationOptionsField("ReaderType"));
		//    if (t == null)
		//        throw new Exception("The service doesn't have valid attribue 'ReaderType'.");

		//    // Initalize arguments to the constructor.
		//    string rowName = GetConfigurationOptionsField("RowName").ToLower();
		//    Object[] args = new Object[String.IsNullOrEmpty(rowName) ? 1 : 2];
		//    args[0] = xmlPath;

		//    if (!String.IsNullOrEmpty(rowName))
		//        args[1] = rowName;

		//    using (SourceDataRowReader<RetrieverDataRow> reader = (SourceDataRowReader<RetrieverDataRow>)Activator.CreateInstance(t, args))
		//    {
		//        xmlFileEmpty = true;

		//        // Loop on all the rows in the xml report file.
		//        while (reader.Read())
		//        {
		//            // Initalize dayCode.
		//            if (firstRowInXml)
		//            {
		//                xmlFileEmpty = false;
		//                firstRowInXml = false;
		//                // ASAF: raise log error if we have diffrent date in the file and in _requiredDay.
		//                _requiredDay = DayCode.GenerateDateTime(GetDate(reader));

		//                if (_accountID == SystemAccountID)
		//                {
		//                    GetAccountID(reader.CurrentRow.Fields["acctname"], out _accountID, _accountFieldName);
		//                    _accountName = reader.CurrentRow.Fields["acctname"];
		//                }

		//                // Delete old data in the DB for the same data we fetch the new data.			
		//                DataIdentifier dataIdentifier = new DataIdentifier(_accountID, GetDayCode(_requiredDay), _channelID);
		//                _dataIdentifiers.Add(dataIdentifier);
		//                HandleDeleteDay();
		//            }

		//            CheckRowData(reader);

		//            try
		//            {
		//                InitalizeNullValues(insertCommand, rawDataFields, metaDataFields);
		//                HandleRow(hasBackOffice, rawDataFields, metaDataFields, insertCommand, gatewayNameFields, reader);
		//            }
		//            catch (Exception ex)
		//            {
		//                WriteErrorMesageToFile(xmlPath, reader.CurrentRow.ToString(), ex, defaultErrorSubDirPath);
		//            }
		//        }
		//    }
		//}


		/*=========================*/
		#endregion   
	}
}
