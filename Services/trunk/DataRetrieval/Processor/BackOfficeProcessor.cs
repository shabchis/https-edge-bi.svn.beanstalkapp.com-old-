using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval.DataReader;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	public class BackOfficeProcessor : BaseProcessor
	{
		#region Consts
		/*=========================*/

		private const string XmlRowName = "tracker";
		private const string TableName = "BackOffice_Client_Gateway";
		private const string ServiceType = "BackOffice";
		private const string DefaultErrorSubDirPath = @"BackOffice\Errors\";

		/*=========================*/
		#endregion   

		#region constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public BackOfficeProcessor()
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
			DeleteDayBO(GetDayCode(_requiredDay), _tableName);
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
					GkManager.GetGatewayGK(_accountID, Convert.ToInt64(xmlReader.GetAttribute(fe.Value)));
		}

		protected override void InitalizeGatewayGK(SqlCommand insertCommand, SourceDataRowReader<RetrieverDataRow> reader, FieldElement fe)
		{
			if (fe.DBFieldName.ToLower() == "gateway_id")
				insertCommand.Parameters["@Gateway_GK"].Value =
					GkManager.GetGatewayGK(_accountID, Convert.ToInt64(reader.CurrentRow.Fields[fe.Value]));
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
