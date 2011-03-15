using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval.DataReader;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	/// <summary>
	/// This service get data from Google AdWords in source DB 
	/// procees and insert it to edge_oltp.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>29/06/2009</creation_date>
	public class AdwordsCreativeProcessor :BaseProcessor
	{
		#region Consts
		/*=========================*/

		private const string XmlRowName = "row";
		private const string TableName = "Paid_API_AllColumns";
		private const string ServiceType = "Google.Adwords.Creative";
		private const string DefaultErrorSubDirPath = @"Adwords\Google\Errors\";
		private const int DefaultMinusRequiredDate = 1;
		private const string AccountFieldName = "GoogleAccountName";

		/*=========================*/
		#endregion   

		#region constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public AdwordsCreativeProcessor()
		{
			_xmlRowName = XmlRowName;
			_tableName = TableName;
			_serviceType = ServiceType;
			_defaultErrorSubDirPath = DefaultErrorSubDirPath;
			_defaultMinusRequiredDate = DefaultMinusRequiredDate;
			_accountFieldName = AccountFieldName;
		}

		/*=========================*/
		#endregion   

		#region Override Methods
		/*=========================*/

		/// <summary>
		/// Activate Content stored procedure only id data exist in 
		/// the content table for the same account and date.
		/// </summary>
		protected override void HandledContentSP()
		{
			if (ContentDataExist(GetDayCode(_requiredDay)))
				ActivateContentSP();
		}

		/// <summary>
		/// Initalize and calculate all relevent parmaeters for adwords creative including gk fields.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param>
		/// <param name="hasBackOffice">Indicate if the account have backoffice.</param>
		/// <param name="gatewayNameFields"></param>
		protected override void InitalizeMetaDataParameters(SqlCommand insertCommand,
			XmlTextReader xmlReader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			base.InitalizeMetaDataParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);
			InitalizeAdwordsParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);

			// If headline value is empty we initalize it with imgCreativeName value.
			if (ResolveString(xmlReader.GetAttribute("headline")) == string.Empty)
				insertCommand.Parameters["@headline"].Value = ResolveString(xmlReader.GetAttribute("imgCreativeName"));

			insertCommand.Parameters["@maxCpc"].Value =
				ResolveDouble(xmlReader.GetAttribute("maxCpc")) / DivideFieldsValue;

			InitalizeAdVariation(insertCommand, ResolveString(xmlReader.GetAttribute("creativeType").ToString()));

			// Initalize Gateway ID
			if (hasBackOffice)
				InitalizeGatewayID(insertCommand, _channelID, gatewayNameFields);

			InitalizeCreativeGKFields(insertCommand, _channelID, hasBackOffice, _accountID);
		}


		/// <summary>
		/// Initalize and calculate all relevent parmaeters for adwords creative including gk fields.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <param name="xmlReader"></param>
		/// <param name="retriveredDate">Required date.</param>
		/// <param name="hasBackOffice">Indicate if the account have backoffice.</param>
		/// <param name="gatewayNameFields"></param>
		protected override void InitalizeMetaDataParameters(SqlCommand insertCommand,
			SourceDataRowReader<RetrieverDataRow> reader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			base.InitalizeMetaDataParameters(insertCommand, reader, hasBackOffice, gatewayNameFields);
			InitalizeAdwordsParameters(insertCommand, reader, hasBackOffice, gatewayNameFields);

			// If headline value is empty we initalize it with imgCreativeName value.
			if (reader.CurrentRow.Fields.ContainsKey("headline") && string.IsNullOrEmpty(reader.CurrentRow.Fields["headline"]))
				insertCommand.Parameters["@headline"].Value = ResolveString(reader.CurrentRow.Fields["imgCreativeName"]);

			//insertCommand.Parameters["@maxCpc"].Value =
			//    ResolveDouble(reader.CurrentRow.Fields["maxCpc"]) / DivideFieldsValue;

			if (reader.CurrentRow.Fields.ContainsKey("creativeType"))
				InitalizeAdVariation(insertCommand, reader.CurrentRow.Fields["creativeType"]);

			// Initalize Gateway ID
			if (hasBackOffice)
				InitalizeGatewayID(insertCommand, _channelID, gatewayNameFields);

			InitalizeCreativeGKFields(insertCommand, _channelID, hasBackOffice, _accountID);
		}
		
		protected override void HandleDeleteDay()
		{
			string shouldDelete = Instance.ParentInstance.Configuration.Options["ShouldDeleteDay"];
			bool deleteDay = String.IsNullOrEmpty(shouldDelete) || shouldDelete.ToLower() != "false";
			if (deleteDay)
			{
				//Log.Write(String.Format("Deleting day {0} for table {1}", Core.Utilities.DayCode.ToDayCode(_requiredDay), _tableName), LogMessageType.Information);
				base.HandleDeleteDay();
			}
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
