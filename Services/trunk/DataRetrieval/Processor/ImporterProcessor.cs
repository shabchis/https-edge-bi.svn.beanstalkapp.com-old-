using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
    public class ImporterProcessor : BaseProcessor
    {
        #region Consts
        /*=========================*/

        private const string ServiceType = "PpcImport";
        private const string DefaultErrorSubDirPath = @"Import\Errors\";

        // List that contain all the account we ecnounter in the CSV file.
        private Dictionary<int, List<int>> accountList = null;

        /*=========================*/
        #endregion

        #region constructor
        /*=========================*/

        /// <summary>
        /// Initalize the parent fields with const values of this class.
        /// </summary>
        public ImporterProcessor()
        {
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
        /// Analytics don't get channel ID from the configuration, it take it from source field 
        /// from the xml for each row.
        /// </summary>
        protected override void InitalizeChannelID()
        {
        }

        /*=========================*/
        #endregion

        #region Protected Override Methods
        /*=========================*/

        protected override Easynet.Edge.Core.Services.ServiceOutcome DoWork()
        {
            _tableName = GetConfigurationOptionsField("TableName");
            return base.DoWork();
        }


        protected override void ReadFile(string filePath, string defaultErrorSubDirPath, bool hasBackOffice, ref bool xmlFileEmpty, FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, string> gatewayNameFields)
        {
            // Initalzie headers Dictionaries.
            Dictionary<string, int> headersCSV = new Dictionary<string, int>();
            accountList = new Dictionary<int, List<int>>();

            using (StreamReader textReader = new StreamReader(filePath, Encoding.Unicode))
            {
                // Read the headers of the CSV file and DB fields and return 
                // a dictionary with the common headers.
                ReadHeaders(headersCSV, textReader);

                // Read first line in the CSV file.
                string csvTextLine = textReader.ReadLine();

                if (csvTextLine == null)
                {
                    Log.Write(string.Format("The file {0} is empty or not format in unicode txt.", filePath), LogMessageType.Error);
                    Exception ex = new Exception(string.Format("The file {0} is empty or not format in unicode txt.", filePath));
                    WriteErrorMesageToFile(filePath, string.Empty, ex, DefaultErrorSubDirPath);
                    throw ex;
                }

                // Read the CSV lines and insert them to the DB.
                while (!textReader.EndOfStream || !string.IsNullOrEmpty(csvTextLine))
                {
                    // Contiune if we have empty line in the csv file.
                    if (csvTextLine == null || csvTextLine == string.Empty)
                        continue;

                    try
                    {
                        // Initalize the insert command with values from Csv text line.
                        // InitalizeInsertCommandFields(filePath, headersDB, commonHeaders, headersCSV, insertCommand, csvTextLine, reportType);
                        InitalizeInsertCommandFields(rawDataFields, metaDataFields, filePath, headersCSV, insertCommand, csvTextLine);

                        insertCommand.ExecuteNonQuery();
                        InitalizeNullValues(insertCommand, rawDataFields, metaDataFields);
                    }
                    catch (Exception ex)
                    {
                        WriteErrorMesageToFile(filePath, csvTextLine, ex, DefaultErrorSubDirPath);
                    }
                    // Read the next line in the CSV file.
                    csvTextLine = textReader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Initalize the row raw data fields with values from the xml. 
        /// And initalize Meta Data fields with null values.
        /// </summary>
        /// <param name="rawDataFields">Dictionary that contain all the required raw data fields from the DB.</param>
        /// <param name="metaDataFields">Dictionary that contain all the required Meta data fields from the DB.</param>
        /// <param name="insertCommand"></param>
        /// <param name="xmlReader"></param>
        protected void InitalizeRawDataFromXml(FieldElementSection rawDataFields, FieldElementSection metaDataFields, SqlCommand insertCommand, Dictionary<string, int> headersCSV, string[] FieldsValues)
        {
            try
            {
                // Initalize the insertCommand with the values from the xmlReader.
                foreach (FieldElement fe in rawDataFields.Fields)
                {
                    if (!fe.InsertToDB || !fe.Enabled)
                        continue;

                    string dbFieldName = fe.DBFieldName.ToLower();

                    if ((headersCSV.ContainsKey(dbFieldName) &&
                        !String.IsNullOrEmpty(FieldsValues[headersCSV[dbFieldName]])) &&
                        dbFieldName != "matchtype")
                    {
                        insertCommand.Parameters["@" + fe.DBFieldName].Value =
                            Convert.ChangeType(FieldsValues[headersCSV[dbFieldName]], Type.GetType(_tableTypeMappings[fe.DBFieldType.ToLower()]));
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
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        /*=========================*/
        #endregion

        #region Private Override Methods
        /*=========================*/

        private static void ReadHeaders(Dictionary<string, int> headersCSV, StreamReader textReader)
        {
            // Read first line from the CSV file which contain the headers.
            string csvTextLine = textReader.ReadLine();

            if (csvTextLine == null)
                throw new Exception("Importer file don't have headers row.");

            // The headers split by tabs.
            string[] headersNames = csvTextLine.Split('\t');

            int i = 0;
            // Initalize headers with all the column's names that 
            // exist in the DB and in the CSV file.
            foreach (string header in headersNames)
            {
                if (header == string.Empty)
                    continue;
                headersCSV.Add(header.ToLower().Trim(), i);
                ++i;
            }
        }

        private void DeleteOldData(int accountID, int channelID, int dayCode)
        {
            // TODO: it don't look like it delete by channel,
            // if we have same day with diffrent channel it might won't delete the second channel.

            // If this is the first time we encounter this account, we add it
            // to the accountList and delete the data of this user for the 
            // same day and channel.
            if (!accountList.ContainsKey(accountID))
            {
                List<int> newList = new List<int>();
                newList.Add(dayCode);

                accountList.Add(accountID, newList);

                if (channelID != ChannelIDNull)
                    DeleteDayChannel(dayCode, _tableName, channelID, accountID);
                else
                    DeleteDay(dayCode, _tableName, accountID);
            }
            else if (accountList.ContainsKey(accountID) && !accountList[accountID].Contains(dayCode))
            {
                accountList[accountID].Add(dayCode);

                if (channelID != ChannelIDNull)
                    DeleteDayChannel(dayCode, _tableName, channelID, accountID);
                else
                    DeleteDay(dayCode, _tableName, accountID);
            }
        }

        /// <summary>
        /// Initalize the Insert command with null values and than Initalize
        /// all his fields in the CSV file.
        /// </summary>
        /// <param name="filePath">CSV file path (used for errors logging).</param>
        /// <param name="headersDB"> A dictionary with table Fields and there 
        /// types of AdWords table.</param>
        /// <param name="commonHeaders">A dictionary of the headers that exist 
        /// in the DB and in CSV file with header name as key and numbering as value.</param>
        /// <param name="headersCSV">A dictionary of the headers in the CSV file
        /// with header name as key and numbering as value.</param>
        /// <param name="insertCommand">Insert command to initalize his field.</param>
        /// <param name="csvTextLine"> The current text line from the 
        /// CSV file to insert to the DB.</param>
        /// <returns> True for success, False for failure.</returns>
        private void InitalizeInsertCommandFields(FieldElementSection rawDataFields, FieldElementSection metaDataFields, string filePath,
            Dictionary<string, int> headersCSV, SqlCommand insertCommand, string csvTextLine)
        {
            string[] FieldsValues = csvTextLine.Split('\t');
            int accountID;
            int dayCode;

            int channelID = InitalizeMandatoryFields(filePath, headersCSV, insertCommand, FieldsValues, out accountID, out dayCode);

            // Delete old data for the DB for the same accountID, dayCode and channelID.
            DeleteOldData(accountID, channelID, dayCode);

            InitalizeRawDataFromXml(rawDataFields, metaDataFields, insertCommand, headersCSV, FieldsValues);
            InitalizeMandatoryFields(filePath, headersCSV, insertCommand, FieldsValues, out accountID, out dayCode);

            // Initalize GK fields
            InitalizeParametersGK(headersCSV, insertCommand, accountID, channelID, FieldsValues);
        }

        private int InitalizeMandatoryFields(string filePath, Dictionary<string, int> headersCSV, SqlCommand insertCommand, string[] FieldsValues, out int accountID, out int dayCode)
        {
            int channelID = ChannelIDNull;
           
                insertCommand.Parameters["@Downloaded_Date"].Value = DateTime.Now;

            if (_tableName != "BackOffice_Client_Gateway")
            {
                // Initalize Channel ID
                if (headersCSV.ContainsKey("channel") && !String.IsNullOrEmpty(FieldsValues[headersCSV["channel"]]))
                    channelID = GetChannelID(FieldsValues[headersCSV["channel"]]);
                else
                    throw new Exception(string.Format("CSV file {0} don't contain channel_id field.", filePath));
                if (insertCommand.Parameters.Contains("@Channel_ID"))
                    insertCommand.Parameters["@Channel_ID"].Value = channelID;
            }

            // Initalize Account ID

            if (headersCSV.ContainsKey("accountname")
                && !String.IsNullOrEmpty(FieldsValues[headersCSV["accountname"]]) &&
                GetAccountID(FieldsValues[headersCSV["accountname"]], out accountID, _accountFieldName))
            {
               
                    insertCommand.Parameters["@Account_ID"].Value = accountID;
            }
            else
                throw new Exception(string.Format("CSV file {0} don't contain accountname field or account name doesn't exist in the DB.", filePath));

            dayCode = -1;

            // Validate day_code.
            if (!headersCSV.ContainsKey("day_code") || FieldsValues[headersCSV["day_code"]].ToString() == string.Empty)
                throw new Exception(string.Format("CSV file {0} don't contain day_code field.", filePath));
            else
            {
                dayCode = Convert.ToInt32(FieldsValues[headersCSV["day_code"]]);
                insertCommand.Parameters["@Day_Code"].Value = dayCode;

                if (dayCode > GetDayCode(DateTime.Now))
                    throw new Exception(string.Format("CSV file {0} don't contain valid day_code {1}.", filePath, dayCode.ToString()));
            }

            return channelID;
        }

        private void InitalizeCreativeFields(Dictionary<string, int> headersCSV, SqlCommand insertCommand, string[] Fields)
        {
            // Initalize creativeVisUrl
            if (insertCommand.Parameters["@creativeVisUrl"].Value.ToString() == string.Empty)
            {
                string creativeVisUrl = insertCommand.Parameters["@destUrl"].Value.ToString();

                // Remove http:// if exist
                if (!String.IsNullOrEmpty(creativeVisUrl))
                {
                    creativeVisUrl = GetCleanDomainName(creativeVisUrl);
                }
                insertCommand.Parameters["@creativeVisUrl"].Value = creativeVisUrl;
            }

            if (headersCSV.ContainsKey("matchtype")
                && !String.IsNullOrEmpty(Fields[headersCSV["matchtype"]]))
            {
                InitalizeMatchType(insertCommand, Fields[headersCSV["matchtype"]].ToString());
            }
            else
                insertCommand.Parameters["@MatchType"].Value = MatchType.Broad;

            if (headersCSV.ContainsKey("advariation")
                && !String.IsNullOrEmpty(Fields[headersCSV["advariation"]]))
            {
                InitalizeMatchType(insertCommand, Fields[headersCSV["advariation"]].ToString());
            }
        }

        private void InitalizeParametersGK(Dictionary<string, int> headersCSV, SqlCommand insertCommand, int accountID, int channelID, string[] Fields)
        {
            //bool hasBackOffice = false;
            //int accountID; 

            //if (Int32.TryParse( insertCommand.Parameters["@Account_ID"].ToString(), out accountID))
            //    hasBackOffice = HasBackOffice(accountID);
            bool hasBackOffice = HasBackOffice(accountID);

            if (_tableName == "Paid_API_AllColumns")
            {
                Dictionary<string, string> gatewayNameFields = new Dictionary<string, string>();
                // Initalize a Dictionary with report fields & gateway Name Fields.
                InitalizeGatewayNameMapping(gatewayNameFields, "GatewayName");

                InitalizeCreativeFields(headersCSV, insertCommand, Fields);
                // Initalize Gateway ID
                if (hasBackOffice)
                    InitalizeGatewayID(insertCommand, channelID, gatewayNameFields);

                InitalizeCreativeGKFields(insertCommand, channelID, hasBackOffice, accountID);
            }
            else if (_tableName == "Paid_API_Content")
                InitalizeContentGKFields(insertCommand, channelID, hasBackOffice, accountID);
            else if (_tableName == "BackOffice_Client_Gateway")
            {
                //Dictionary<string, string> babylonGatewayNameFields = new Dictionary<string, string>();
                //// Initalize a Dictionary with report fields & gateway Name Fields.
                //InitalizeGatewayNameMapping(babylonGatewayNameFields, "BabylonGatewayName");

                //string gatewayID = Fields[headersCSV["gateway_id"]].ToString();

                //foreach (string gatewayName in babylonGatewayNameFields.Keys)
                //{
                //    if (gatewayID.Contains(gatewayName))
                //    {
                //        insertCommand.Parameters["@gateway_ID"].Value = Convert.ToInt32(babylonGatewayNameFields[gatewayName]) + gatewayID.Substring(gatewayID.IndexOf(gatewayName) + gatewayName.Length);
                //        break;
                //    }
                //}

                // Initalize gateway_GK
                insertCommand.Parameters["@gateway_GK"].Value = GkManager.GetGatewayGK(
                    Convert.ToInt32(insertCommand.Parameters["@Account_ID"].Value),
                    Convert.ToInt64(insertCommand.Parameters["@gateway_ID"].Value));
            }
        }

        /// <summary>
        /// Get from the table Constant_Channel in easynet_OLTP DB
        /// the channel ID by the channel Name.
        /// </summary>
        /// <param name="channelName">Channel name to find his ID.</param>
        /// <param name="channelID">Channel ID of the channel name.</param>
        /// <returns> True for success, False for failure.</returns>
        //        private void GetChannelID(string channelName, out int channelID)
        //        {
        //            channelID = ChannelIDNull;
        //            // Initalize select command to channel ID by channel name.
        //            SqlCommand selectCommand2 = DataManager.CreateCommand(@"
        //				select Channel_ID
        //					from Constant_Channel
        //					where Channel_Name = @channelName:varchar",
        //                CommandType.Text);

        //            using (DataManager.Current.OpenConnection())
        //            {
        //                // Initalize connetion.
        //                DataManager.ApplyConnection(selectCommand2);

        //                // Initalize select command parameters.
        //                selectCommand2.Parameters["@channelName"].Value = channelName;

        //                try
        //                {
        //                    // Execute select command.
        //                    channelID = (int)selectCommand2.ExecuteScalar();
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(string.Format("Error get channel data from table Constant_Channel for channel {0}.", channelName), ex);
        //                }
        //            }
        //        }

        /*=========================*/
        #endregion
    }
}
