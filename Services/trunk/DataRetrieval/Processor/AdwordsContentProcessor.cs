using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using Easynet.Edge.Services.DataRetrieval.DataReader;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	public class AdwordsContentProcessor : BaseProcessor
	{
		#region Consts
		/*=========================*/ 

		// ASAF: move this data to the config.
		private const string XmlRowName = "row";
		private const string TableName = "Paid_API_Content";
		private const string ServiceType = "Google.Adwords.Content";
		private const string DefaultErrorSubDirPath = @"Adwords\Google\Errors\";
		private const int DefaultMinusRequiredDate = 3;
		private const string AccountFieldName = "GoogleAccountName";


		/*=========================*/
		#endregion   

		#region constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public AdwordsContentProcessor()
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

		#region Empty Override Methods
		/*=========================*/

		/// <summary>
		/// Adwords content Doesn't use Gateway Name Mapping.
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

		protected override void OnInit()
		{
			string offsetValue = GetConfigurationOptionsField("OffsetDays");
			if (!String.IsNullOrEmpty(offsetValue))
			{
				int offset;
				if (Int32.TryParse(offsetValue, out offset) && offset < 0)
					_defaultMinusRequiredDate = -1 * offset;
			}

			base.OnInit();
		}

		/// <summary>
		/// Activate Content stored procedure after each run of adwords content.
		/// </summary>
		protected override void HandledContentSP()
		{			
			ActivateContentSP();
		}


		protected override void HandleDeleteDay()
		{
			string shouldDelete = Instance.ParentInstance.Configuration.Options["ShouldDeleteDay"];
			bool deleteDay = String.IsNullOrEmpty(shouldDelete) || shouldDelete.ToLower() != "false";
			if (deleteDay)
			{
				Log.Write(String.Format("Deleting day {0} for table {1}", Core.Utilities.DayCode.ToDayCode(_requiredDay), _tableName), LogMessageType.Information);
				base.HandleDeleteDay();
			}
		}

		/// <summary>
		/// Initalize and calculate all relevent parmaeters for adwords content including gk fields.
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <param name="xmlReader"></param>
		/// <param name="hasBackOffice">Indicate if the account have backoffice.</param>
		/// <param name="gatewayNameFields"></param>
		protected override void InitalizeMetaDataParameters(SqlCommand insertCommand,
			SourceDataRowReader<RetrieverDataRow> reader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			base.InitalizeMetaDataParameters(insertCommand, reader, hasBackOffice, gatewayNameFields);
			InitalizeAdwordsParameters(insertCommand, reader, hasBackOffice, gatewayNameFields);
			InitalizeContentGKFields(insertCommand, _channelID, hasBackOffice, _accountID);
		}

		protected override void InitalizeMetaDataParameters(SqlCommand insertCommand,
			XmlTextReader xmlReader, bool hasBackOffice, Dictionary<string, string> gatewayNameFields)
		{
			base.InitalizeMetaDataParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);
			InitalizeAdwordsParameters(insertCommand, xmlReader, hasBackOffice, gatewayNameFields);
			InitalizeContentGKFields(insertCommand, _channelID, hasBackOffice, _accountID);
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
