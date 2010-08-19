using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 
using System.ComponentModel;
using System.Drawing; 
using System.IO;
using System.Data;
using System.Data.OleDb; 
using System.Text;
using System.Diagnostics;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.UI.WebPages.Classes.Convertors;
using System.Data.SqlClient;


namespace Easynet.Edge.UI.WebPages.Converters
{
    public partial class YahooConvertor : BaseConvertor
    {
        string _errorMessage = string.Empty;
        int _errorsCounter = 0;
        string _destURLInHeader = string.Empty;
        StreamWriter _wrtTxtFile = null;
        private int firstRow = 0;
        // private static bool sourceChosen = false;
        //  private static bool destinationChosen = false;
        //  private static bool isValidDate = true;
        private System.Collections.Hashtable currencyRatioCountries = new System.Collections.Hashtable();

        public YahooConvertor(string account) :base(account)
        {
            BuildCurrencyNames();
        }

        public YahooConvertor(string CurrencyCode, string dateformat)
            : base(CurrencyCode, dateformat)
        {
            BuildCurrencyNames();
        }





        public void BuildCurrencyNames()
        {
		#region list
            /*
             http://www.webservicex.net/WS/WSDetails.aspx?CATID=2&WSID=10
             AFA-Afghanistan Afghani
ALL-Albanian Lek
DZD-Algerian Dinar
ARS-Argentine Peso
AWG-Aruba Florin
AUD-Australian Dollar
BSD-Bahamian Dollar
BHD-Bahraini Dinar
BDT-Bangladesh Taka
BBD-Barbados Dollar
BZD-Belize Dollar
BMD-Bermuda Dollar
BTN-Bhutan Ngultrum
BOB-Bolivian Boliviano
BWP-Botswana Pula
BRL-Brazilian Real
GBP-British Pound
BND-Brunei Dollar
BIF-Burundi Franc
XOF-CFA Franc (BCEAO)
XAF-CFA Franc (BEAC)
KHR-Cambodia Riel
CAD-Canadian Dollar
CVE-Cape Verde Escudo
KYD-Cayman Islands Dollar
CLP-Chilean Peso
CNY-Chinese Yuan
COP-Colombian Peso
KMF-Comoros Franc
CRC-Costa Rica Colon
HRK-Croatian Kuna
CUP-Cuban Peso
CYP-Cyprus Pound
CZK-Czech Koruna
DKK-Danish Krone
DJF-Dijibouti Franc
DOP-Dominican Peso
XCD-East Caribbean Dollar
EGP-Egyptian Pound
SVC-El Salvador Colon
EEK-Estonian Kroon
ETB-Ethiopian Birr
EUR-Euro
FKP-Falkland Islands Pound
GMD-Gambian Dalasi
GHC-Ghanian Cedi
GIP-Gibraltar Pound
XAU-Gold Ounces
GTQ-Guatemala Quetzal
GNF-Guinea Franc
GYD-Guyana Dollar
HTG-Haiti Gourde
HNL-Honduras Lempira
HKD-Hong Kong Dollar
HUF-Hungarian Forint
ISK-Iceland Krona
INR-Indian Rupee
IDR-Indonesian Rupiah
IQD-Iraqi Dinar
ILS-Israeli Shekel
JMD-Jamaican Dollar
JPY-Japanese Yen
JOD-Jordanian Dinar
KZT-Kazakhstan Tenge
KES-Kenyan Shilling
KRW-Korean Won
KWD-Kuwaiti Dinar
LAK-Lao Kip
LVL-Latvian Lat
LBP-Lebanese Pound
LSL-Lesotho Loti
LRD-Liberian Dollar
LYD-Libyan Dinar
LTL-Lithuanian Lita
MOP-Macau Pataca
MKD-Macedonian Denar
MGF-Malagasy Franc
MWK-Malawi Kwacha
MYR-Malaysian Ringgit
MVR-Maldives Rufiyaa
MTL-Maltese Lira
MRO-Mauritania Ougulya
MUR-Mauritius Rupee
MXN-Mexican Peso
MDL-Moldovan Leu
MNT-Mongolian Tugrik
MAD-Moroccan Dirham
MZM-Mozambique Metical
MMK-Myanmar Kyat
NAD-Namibian Dollar
NPR-Nepalese Rupee
ANG-Neth Antilles Guilder
NZD-New Zealand Dollar
NIO-Nicaragua Cordoba
NGN-Nigerian Naira
KPW-North Korean Won
NOK-Norwegian Krone
OMR-Omani Rial
XPF-Pacific Franc
PKR-Pakistani Rupee
XPD-Palladium Ounces
PAB-Panama Balboa
PGK-Papua New Guinea Kina
PYG-Paraguayan Guarani
PEN-Peruvian Nuevo Sol
PHP-Philippine Peso
XPT-Platinum Ounces
PLN-Polish Zloty
QAR-Qatar Rial
ROL-Romanian Leu
RUB-Russian Rouble
WST-Samoa Tala
STD-Sao Tome Dobra
SAR-Saudi Arabian Riyal
SCR-Seychelles Rupee
SLL-Sierra Leone Leone
XAG-Silver Ounces
SGD-Singapore Dollar
SKK-Slovak Koruna
SIT-Slovenian Tolar
SBD-Solomon Islands Dollar
SOS-Somali Shilling
ZAR-South African Rand
LKR-Sri Lanka Rupee
SHP-St Helena Pound
SDD-Sudanese Dinar
SRG-Surinam Guilder
SZL-Swaziland Lilageni
SEK-Swedish Krona
TRY-Turkey Lira
CHF-Swiss Franc
SYP-Syrian Pound
TWD-Taiwan Dollar
TZS-Tanzanian Shilling
THB-Thai Baht
TOP-Tonga Pa'anga
TTD-Trinidad&amp;Tobago Dollar
TND-Tunisian Dinar
TRL-Turkish Lira
USD-U.S. Dollar
AED-UAE Dirham
UGX-Ugandan Shilling
UAH-Ukraine Hryvnia
UYU-Uruguayan New Peso
VUV-Vanuatu Vatu
VEB-Venezuelan Bolivar
VND-Vietnam Dong
YER-Yemen Riyal
YUM-Yugoslav Dinar
ZMK-Zambian Kwacha
ZWD-Zimbabwe Dollar 
             *
              */
        #endregion
             
            currencyRatioCountries.Add("au", Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.AUD);
            currencyRatioCountries.Add("ch", Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.CHF);
            currencyRatioCountries.Add("no", Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.NOK);
            currencyRatioCountries.Add("fr", Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.EUR);
            currencyRatioCountries.Add("uk", Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.GBP);
        }


        public int GetFirstRowIndex(string firstRowsStr, DataTable table)
        {
            try
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {

                    if (table.Rows[i][0].ToString().Contains(firstRowsStr))
                        return i;

                }
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }


        


        private string GetDatetimeFromFile(string fileName)
        {
            //e-4-3-09 6
            //-4-12-09 7
            //12-12-09 8
            DateTime date;
            fileName = fileName.Substring(fileName.Length - 12, 8);
            if (fileName[0].Equals('-'))
            {
                fileName = fileName.Remove(0,1);
                string[] str = fileName.Split("-".ToCharArray());
                if(str[0].Length == 1)
                     date = DateTime.ParseExact(fileName, "M-dd-yy", null);
                else
                    date = DateTime.ParseExact(fileName, "MM-d-yy", null);
                return DayCode.ToDayCode(date).ToString();
            }
            else
                if (fileName[1].Equals('-'))
                {
                    fileName = fileName.Remove(0, 1);
                    fileName = fileName.Remove(0, 1);

                    date = DateTime.ParseExact(fileName, "M-d-yy", null);
                    return DayCode.ToDayCode(date).ToString();
                }

            date = DateTime.ParseExact(fileName, "MM-dd-yy", null);
           return DayCode.ToDayCode(date).ToString();
        }

        public new string ReadHeaders(System.Xml.XmlAttributeCollection coll)
        {
            System.Xml.XmlNodeList list = GetConvertionTypeNode();
            System.Xml.XmlAttributeCollection destURL = list[0].SelectNodes("DestinationURL")[0].Attributes;
            string ret = "";
            for (int i = 0; i < coll.Count; i++)
            {
                ret += coll[i].Name + @" "; //list[0].SelectNodes("tmpHeader")[0].Attributes[0].Name
                headersStrings.Add(coll[i].Value);
            }
            //adding dest url details
            ret += destURL[0].Name + @" ";
            headersStrings.Add(destURL[0].Value);
            _destURLInHeader = destURL[0].Value;


            return ret;
        }
        private System.Collections.Hashtable BuildOrderOfHeaders(List<string> headersStrings, string[] headersValues, string CostHeaderName)
        {
            int hashTableKey = 0;
            System.Collections.Hashtable ret = new System.Collections.Hashtable();
            for (int i = 0; i < headersStrings.Count; i++)
            {
                for (int j = 0; j < headersValues.Count<string>(); j++)
                {
                    if (headersValues[j].ToLower().Equals(headersStrings[i].ToLower()))
                    {

                        ret.Add(hashTableKey, j);
                        hashTableKey++;

                        if (headersValues[j].Equals(_destURLInHeader))
                            destURLColumn = j;
                        
                        if (headersValues[j].Equals(CostHeaderName))
                            columnToConvertToUSD = j;
                    }
                }
            }
            return ret;

        }
        private void writeHeadersInTXT(List<string> sourceFiles, string saveFilePath)
        {
            string header = null;
            try
            {

                System.Xml.XmlNodeList list = GetConvertionTypeNode();

                header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);
                header = "Channel AccountName day_code " + header + "CostConversionRate CostBeforeConversion";

                header = header.Replace(@" ", "\t");

                if (saveFilePath != "")
                    _wrtTxtFile = new StreamWriter(saveFilePath, false, Encoding.Unicode);
                else
                {
                    header = "Channel AccountName day_code " + header;
                    _wrtTxtFile = new StreamWriter(saveFilePath,//saveFilePath + "\\" + "CSV_EF_Yahoo_" + date.Day.ToString() + "." + date.Month.ToString() + "." + date.Year.ToString() + ".xls",//+DateTime.Now.Month.ToString().Substring(0,3)+"_"+  "Dec_22.12.09.xls",
                    false, Encoding.Unicode);
                }
                _wrtTxtFile.WriteLine(header);
            }
            catch (Exception ex)
            {
                // WriteToEventLog("Converter: \n" + ex.ToString());
                if (_wrtTxtFile != null)
                {
                    _wrtTxtFile.Close();
                    _wrtTxtFile.Dispose();
                }
            }
        }
        private int findGatewayInDest(string[] rowValues)
        {
            System.Xml.XmlNodeList list = GetConvertionTypeNode();
            string preGatewaySign = list[0].SelectNodes("PreGatewaySign")[0].ChildNodes[0].Value;
            string destURl = string.Empty;
            string gateway = string.Empty;

            destURl = rowValues[destURLColumn];
            int index = destURl.IndexOf(preGatewaySign);
            gateway = destURl.Substring(index + preGatewaySign.Length, destURl.Length - index - preGatewaySign.Length);
            int result = 0;

            if (Int32.TryParse(gateway, out result))
                return Int32.Parse(gateway);
            else
                return -1;
        }
        private string FindAccountName(string gateway, Int32 scopeID)
        {
            DataTable accountByGateway = null;
            lock (Easynet.Edge.Core.Data.DataManager.ConnectionString)
            {
                using (Easynet.Edge.Core.Data.DataManager.Current.OpenConnection())
                {
                    try
                    {
                        string comnd = @"select Account_Name, gt.Account_ID,Gateway_GK 
                                from dbo.UserProcess_GUI_Gateway gt inner join  dbo.User_GUI_Account ac on ac.Account_ID = gt.Account_ID 
                                inner join dbo.Constant_Channel ch on ch.Channel_ID = gt.Channel_ID 
                                where gateway_id = " + gateway + " and gt.account_id > 0 and ac.Scope_ID = " + scopeID + " and ch.Channel_ID = " + 2;
                        //"select * from dbo.UserProcess_GUI_Gateway where gateway_id = " + gateway + " and account_id > 0";
                        SqlCommand cmd = DataManager.CreateCommand(comnd);
                        System.Data.SqlClient.SqlDataAdapter adapter1 = new System.Data.SqlClient.SqlDataAdapter(cmd);
                        adapter1.SelectCommand.Connection = new System.Data.SqlClient.SqlConnection();
                        adapter1.SelectCommand.Connection.ConnectionString = Easynet.Edge.Core.Data.DataManager.ConnectionString;
                        adapter1 = new System.Data.SqlClient.SqlDataAdapter(cmd);
                        


                        try
                        {
                            accountByGateway = new DataTable();

                            adapter1.Fill(accountByGateway);
                            //if two rows retrieved for the same gateway we'll export all retrieved data to an error log file
                            if (accountByGateway.Rows.Count > 1)
                                createErrorLog(accountByGateway);

                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            //   Easynet.Edge.Core.Data.DataManager.ConnectionString = prevConnString;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            if(accountByGateway.Rows.Count == 0)
                return base.errorAccountString;
            else
                return accountByGateway.Rows[0]["Account_Name"].ToString();

        }
        private void createErrorLog(DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    _errorMessage += dt.Rows[i][j].ToString();
                    _errorMessage += "/n";
                }
            }
            _errorsCounter++;
        }
        private void ReadTSVFile(List<string> sourceFiles)
        {
            //string firstRowsStr = string.Empty;
            string country = string.Empty;
            string date = GetDatetimeFromFile(sourceFiles[0]);
            System.Collections.Hashtable headersHash = null;
            StringBuilder sBuilder = new StringBuilder();
            string cost = string.Empty;
            double ConvertionRate = 1.0;
            bool isNewRow = true;
            System.Xml.XmlNodeList list;
            int firstHeaderRowNum;
            int firstDataRowNum;
            int countryRowNum;
            bool accountIdFound = false;
            string accountIdInFile = string.Empty;
            string accountNameInAccountSettings = string.Empty;
            string beforeCountrySignInTSV;
            DataTable scopeByAccountID;

            try
            {
                list = GetConvertionTypeNode();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            //try
            //{
            //    firstRowsStr = list[0].SelectNodes("FirstRowName")[0].ChildNodes[0].Value;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("FirstRowName Node wasn't found in configuration. " + ex.Message.ToString());
            //}
            try
            {
                firstHeaderRowNum = Int32.Parse(list[0].SelectNodes("FirstHeaderRowNum")[0].ChildNodes[0].Value);
            }
            catch (Exception ex)
            {
                throw new Exception("FirstHeaderRowNum Node wasn't found in configuration. " + ex.Message.ToString());
            }
            try
            {
                firstDataRowNum = Int32.Parse(list[0].SelectNodes("FirstDataRowNum")[0].ChildNodes[0].Value);
            }
            catch (Exception ex)
            {
                throw new Exception("FirstDataRowNum Node wasn't found in configuration. " + ex.Message.ToString());
            }
            try
            {
                countryRowNum = Int32.Parse(list[0].SelectNodes("CountryInRowNum")[0].ChildNodes[0].Value);
            }
            catch (Exception ex)
            {
                throw new Exception("CountryInRowNum Node wasn't found in configuration. " + ex.Message.ToString());
            }
            try
            {
                beforeCountrySignInTSV = list[0].SelectNodes("SignForCountryInTSV")[0].ChildNodes[0].Value;
            }
            catch (Exception ex)
            {
                throw new Exception("SignForCountryInTSV Node wasn't found in configuration. " + ex.Message.ToString());
            }
            int scopeID;
            try
            {
                string comnd = "select Scope_ID from dbo.User_GUI_Account where Account_ID = " + accountID;
                SqlCommand cmd = DataManager.CreateCommand(comnd);
                System.Data.SqlClient.SqlDataAdapter adapter1 = new System.Data.SqlClient.SqlDataAdapter(cmd);
                adapter1.SelectCommand.Connection = new System.Data.SqlClient.SqlConnection();
                adapter1.SelectCommand.Connection.ConnectionString = Easynet.Edge.Core.Data.DataManager.ConnectionString;
                scopeByAccountID = new DataTable();

                adapter1.Fill(scopeByAccountID);
                scopeID = (Int32)scopeByAccountID.Rows[0]["Scope_ID"];
            }
            catch(Exception ex)
            {
                throw new Exception();
            }
            try
            {
                foreach (var item in sourceFiles)
                {
                    using (StreamReader textReader = new StreamReader(item))
                    {
                        string[] tsvValues;
                        // Read first line in the CSV file.
                        string tsvTextLine = textReader.ReadLine();
                        int iterCounter = 1;
                        //read account table in db - account_name and account_settings
                        FillAccountNames2DataSet();
                        // Read the CSV lines and insert them to the DB.
                        while (!textReader.EndOfStream || !string.IsNullOrEmpty(tsvTextLine))
                        {
                            // Contiune if we have empty line in the csv file.
                            if (tsvTextLine == null || tsvTextLine == string.Empty)
                            {
                                tsvTextLine = textReader.ReadLine();
                                iterCounter++;
                                continue;
                            }
                            try
                            {
                                tsvValues = readLine(tsvTextLine);
                                if (!accountIdFound && tsvTextLine.Contains("Account Id"))
                                {
                                    //in file the format is: account id, equelas, number..
                                    accountIdInFile = tsvValues[2];
                                    accountNameInAccountSettings = findAccountNameInAccountTable(accountIdInFile);
                                    accountIdFound = true;
                                }
                                
                                foreach (string value in tsvValues)
                                {
                                    //find country name in the row it exists
                                    if (iterCounter == countryRowNum && value.Contains(beforeCountrySignInTSV))
                                    {
                                        country = value;
                                        int lenght = country.Length;
                                        int index = country.IndexOf(beforeCountrySignInTSV);
                                        country = country.Substring(index + 1, lenght - index - 1).ToLower();
                                        try
                                        {
                                            if (currencyRatioCountries[country] != null)
                                            {
                                                ConvertionRate = Easynet.Edge.UI.WebPages.Classes.Convertors.CurrencyManager.Convert(base.CurrecnyCode, "USD", date);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Write("Cannot convert currency: " + e.Message, Easynet.Edge.Core.Utilities.LogMessageType.Error);
                                        }
                                    }

                                    else if (iterCounter == firstHeaderRowNum)
                                    {
                                        headersHash = BuildOrderOfHeaders(headersStrings, tsvValues, "Cost");
                                        //read next row in tsv file
                                        tsvTextLine = textReader.ReadLine();
                                        tsvValues = readLine(tsvTextLine);
                                        iterCounter++;
                                        continue;
                                    }
                                    //we got to the main part of the file
                                    else if (iterCounter >= firstDataRowNum)
                                    {
                                        
                                        if (isNewRow)
                                        {
                                            int gatewayNum = findGatewayInDest(tsvValues);
                                            if (!accountIdFound)
                                            {
                                                base._account = FindAccountName(gatewayNum.ToString(),scopeID);
                                                if (_errorsCounter == 50)
                                                {
                                                    _errorsCounter = 0;
                                                    Log.Write("Yahoo Convertor",_errorMessage,LogMessageType.Error);
                                                }
                                            }
                                            else if (accountNameInAccountSettings.Equals(""))
                                                base._account = base.errorAccountString;
                                            else
                                                base._account = accountNameInAccountSettings;
                                            
                                            isNewRow = false;

                                            sBuilder.Append("Yahoo\t" + base._account);
                                            sBuilder.Append("\t" + date);
                                            for (int k = 0; k < headersHash.Count; k++)
                                            {
                                                int columNumber = Convert.ToInt32(headersHash[k]);
                                                if (columNumber != -1)
                                                {
                                                    if (columnToConvertToUSD == columNumber)//CONVERT TO USD!
                                                    {
                                                        cost = tsvValues[columNumber].Replace(",", ".");
                                                        //cost = (Convert.ToDouble(cost) * ConvertionRate).ToString();
                                                        sBuilder.Append("\t" + (Convert.ToDouble(cost) * ConvertionRate).ToString());
                                                    }
                                                    else
                                                        sBuilder.Append("\t" + tsvValues[columNumber]);
                                                }
                                            }
                                            sBuilder.Append("\t" + ConvertionRate);
                                            sBuilder.Append("\t" + cost);
                                        }
                                    }
                                }
                                //read next row in tsv file
                                tsvTextLine = textReader.ReadLine();
                                tsvValues = readLine(tsvTextLine);
                            }
                            catch (Exception ex)
                            {
                            }
                            iterCounter++;
                            isNewRow = true;
                            if(sBuilder.Length > 0)
                                _wrtTxtFile.WriteLine(sBuilder);
                            sBuilder = new StringBuilder();
                        }
                    }
                }
                _wrtTxtFile.Close();
                _wrtTxtFile.Dispose();
                CopyFile();
            }
            catch (Exception ex)
            {
            }
        }
        private string findAccountNameInAccountTable(string accountIdInFile)
        {
            string accountSettings = string.Empty;
            string accountID = string.Empty;
            string accountName = string.Empty;
            string[] FieldsValues;

            for(int i = 0; i < accountNamesDS.Tables[0].Rows.Count; i++)
            {
                accountSettings = accountNamesDS.Tables[0].Rows[i]["accountSettings"].ToString();
                FieldsValues = accountSettings.Split(';');
                foreach (string item in FieldsValues)
                {
                    if (item.ToLower().Contains("yahoo_account_id"))
                    {
                        accountID = item.Substring(item.IndexOf(':') + 1);
                        if (accountID == accountIdInFile)
                        {
                            accountName = accountNamesDS.Tables[0].Rows[i]["account_name"].ToString();
                            break;
                        }
                    }
                }
                if (!accountName.Equals(""))
                    break;
            }
            return accountName;
        }

        private string[] readLine(string csvTextLine)
        {
            string[] FieldsValues = csvTextLine.Split('\t');
            return FieldsValues;
        }
        //private void createTXTfromTSV(List<string> soureFilePath)
        //{
        //    DataSet dataSet1 = new DataSet();

        //    DataTable dt = new DataTable();

        //    DateTime date = new DateTime();
        //    StringBuilder sBuilder = new StringBuilder();
        //    string rowString = "";
        //    string firstRowsStr = null, firstHeaderRowsStr = null;
        //    string header = null;
        //    try
        //    {

        //        System.Xml.XmlNodeList list = GetConvertionTypeNode();

        //        firstRowsStr = list[0].SelectNodes("FirstRowName")[0].ChildNodes[0].Value;
        //        firstHeaderRowsStr = list[0].SelectNodes("FirstHeaderRowName")[0].ChildNodes[0].Value;


        //        header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);
        //        header = "Channel AccountName day_code " + header + "CostConversionRate CostBeforeConversion";

        //        header = header.Replace(@" ", "\t");
        //        date = DateTime.Now;

        //        if (saveFilePath != "")
        //            _wrtTxtFile = new StreamWriter(saveFilePath, false, Encoding.Unicode);
        //        else
        //        {//default header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);
        //            header = "Channel AccountName day_code " + header;
        //            _wrtTxtFile = new StreamWriter(saveFilePath,//saveFilePath + "\\" + "CSV_EF_Yahoo_" + date.Day.ToString() + "." + date.Month.ToString() + "." + date.Year.ToString() + ".xls",//+DateTime.Now.Month.ToString().Substring(0,3)+"_"+  "Dec_22.12.09.xls",
        //          false, Encoding.Unicode);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        // WriteToEventLog("Converter: \n" + ex.ToString());
        //        if (_wrtTxtFile != null)
        //        {
        //            _wrtTxtFile.Close();
        //            _wrtTxtFile.Dispose();
        //        }
        //    }
        //    try
        //    {
        //        _wrtTxtFile.WriteLine(header);

        //        int accountIDRowIndex;
        //        for (int i = 0; i < listTables.Count; i++)
        //        {
        //            int FirstRowHeaders = 0;
        //            FillAccountNames2DataSet();
     
        //            FirstRowHeaders = GetFirstRowIndex(firstHeaderRowsStr, dt);   
        //            System.Collections.Hashtable headersHash = BuildHeadersOrder(headersStrings, dt, FirstRowHeaders,"Cost");



        //            double ConvertionRate = 1.0;
        //            string cost = string.Empty;
        //            // double ConvertionRate = Convert.ToDouble( ConvertionRates[country]);
        //            for (int rowsCounter = firstRow; rowsCounter < dt.Rows.Count; rowsCounter++)
        //            {
        //                if (dt.Rows[rowsCounter][0].ToString().Equals(""))
        //                {
        //                }
        //                else
        //                {
        //                    sBuilder.Append("Yahoo\t" + base._account);
        //                    sBuilder.Append("\t" + rowString);



        //                    for (int k = 0; k < headersHash.Count; k++)
        //                    {
        //                        int columNumber = Convert.ToInt32(headersHash[k]);
        //                        if (columNumber != -1)
        //                        {


        //                            //if (accountNamesDS.Tables.Count == 0)
        //                            //{
        //                            //    base._account = "-1";
        //                            //}
        //                            //else
        //                            //    base._account = FindAccountName(accountIDStr, "Yahoo_Account_Name", dt.Rows[rowsCounter][7].ToString());



        //                            if (columnToConvertToUSD == columNumber)//CONVERT TO USD!
        //                            {
        //                                cost = dt.Rows[rowsCounter][columNumber].ToString().Replace(",", ".");
        //                                //    cost = (Convert.ToDouble(cost) * ConvertionRate).ToString();
        //                                sBuilder.Append("\t" + (Convert.ToDouble(cost) * ConvertionRate).ToString());
        //                            }
        //                            else
        //                                sBuilder.Append("\t" + dt.Rows[rowsCounter][columNumber].ToString());
        //                        }
        //                    }

        //                    sBuilder.Append("\t" + ConvertionRate);
        //                    sBuilder.Append("\t" + cost);

        //                    _wrtTxtFile.WriteLine(sBuilder);
        //                    sBuilder = new StringBuilder();
        //                }
        //            }
        //        }
        //        _wrtTxtFile.Close();

        //        CopyFile();
        //        _wrtTxtFile.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        _wrtTxtFile.Close();
        //        _wrtTxtFile.Dispose();
        //    }
        //}
        public override bool DoWork(List<string> soureFilePath, string saveFilePath)
        {

            CurrencyConvertor.CurrencyConvertor currencyConvertor = new Easynet.Edge.UI.WebPages.CurrencyConvertor.CurrencyConvertor();

            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            DataSet dataSet1 = new DataSet();


            DataTable dt = new DataTable();

            if (System.IO.Path.GetExtension(soureFilePath[0]).ToLower().Equals(".tsv"))
            {
                try
                {
                    writeHeadersInTXT(soureFilePath, saveFilePath);
                    ReadTSVFile(soureFilePath);
                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }

                //createTXTfromTSV(soureFilePath);
            }
            else
            {
                ReadXLSFile(soureFilePath);


                DateTime date = new DateTime();
                StringBuilder sBuilder = new StringBuilder();
                string rowString = "";
                string firstRowsStr, firstHeaderRowsStr;
                string header;
                try
                {

                    System.Xml.XmlNodeList list = GetConvertionTypeNode();

                    firstRowsStr = list[0].SelectNodes("FirstRowName")[0].ChildNodes[0].Value;
                    firstHeaderRowsStr = list[0].SelectNodes("FirstRowName")[0].ChildNodes[0].Value;


                    header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);
                    header = "Channel AccountName day_code " + header + "CostConversionRate CostBeforeConversion";

                    header = header.Replace(@" ", "\t");
                    date = DateTime.Now;

                    if (saveFilePath != "")
                        _wrtTxtFile = new StreamWriter(saveFilePath, false, Encoding.Unicode);
                    else
                    {//default header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);
                        header = "Channel AccountName day_code " + header;
                        _wrtTxtFile = new StreamWriter(saveFilePath,//saveFilePath + "\\" + "CSV_EF_Yahoo_" + date.Day.ToString() + "." + date.Month.ToString() + "." + date.Year.ToString() + ".xls",//+DateTime.Now.Month.ToString().Substring(0,3)+"_"+  "Dec_22.12.09.xls",
                      false, Encoding.Unicode);
                    }

                }
                catch (Exception ex)
                {
                    // WriteToEventLog("Converter: \n" + ex.ToString());
                    if (_wrtTxtFile != null)
                    {

                        _wrtTxtFile.Close();
                        _wrtTxtFile.Dispose();

                    }
                    return false;
                }

                try
                {
                    _wrtTxtFile.WriteLine(header);
                    //  _wrtTxtFile.WriteLine("Channel\tAccountName\tDay_Code\tHeadline\tDesc1\tDestURL\tAdgroup\tCampaign\tImps\tClicks\tCost\tPos");

                    int accountIDRowIndex;
                    for (int i = 0; i < listTables.Count; i++)
                    {
                        dt = listTables[i];
                        firstRow = GetFirstRowIndex(firstRowsStr, dt);
                        firstRow++;

                        accountIDRowIndex = GetFirstRowIndex("Account Id", dt);
                        if (accountIDRowIndex == -1) //no account found in excel table
                        {

                        }

                        int FirstRowHeaders = 0;

                        string accountIDStr = dt.Rows[accountIDRowIndex][0].ToString();

                        //_sqlCommand = "Select Account_Name FROM [easynet_OLTP].[dbo].[User_GUI_Account] where AccountSettings is like %" + accountIDStr+"%";
                        FillAccountNames2DataSet();




                        //  base._account = accountNamesDS.Tables[0].Rows[0][0].ToString();

                        FirstRowHeaders = GetFirstRowIndex(firstHeaderRowsStr, dt);
                        System.Collections.Hashtable headersHash = BuildHeadersOrder(headersStrings, dt, FirstRowHeaders, "Cost");

                        //rowString = GetDatetimeFromFile(soureFilePath[i]);


                        string country = dt.Columns[1].ColumnName;
                        int lenght = country.Length;
                        int index = country.IndexOf("-");
                        country = country.Substring(index + 1, lenght - index - 1);

                        double ConvertionRate = 1.0;
                        try
                        {
                            if (currencyRatioCountries[country] != null)
                            {


                                ConvertionRate = Easynet.Edge.UI.WebPages.Classes.Convertors.CurrencyManager.Convert(base.CurrecnyCode, "USD", rowString);



                                // ConvertionRate =  CurrencyManager.Convert(currencyRatioCountries[country].ToString(), "USD", rowString);
                                //     Easynet.Edge.UI.WebPages.Converters.
                                //  string gg = rowString;//20091222  91/00/22
                                //    string url = @"http://www.oanda.com/currency/historical-rates?date_fmt=us&date="+rowString.Substring(4,2)+@"/"+rowString.Substring(6,2) +@"/"+rowString.Substring(2,2)  +@"&date1="+ rowString.Substring(4,2)+@"/"+rowString.Substring(6,2) +@"/"+rowString.Substring(2,2)  +@"&exch=" + (Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency)currencyRatioCountries[country] + @"&expr=USD&margin_fixed=0&format=HTML&redirected=1";


                                //    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);


                                ////    System.Net.WebResponse res = request.GetResponse();
                                //   StreamReader stIn = new StreamReader(request.GetResponse().GetResponseStream());

                                //   string strResponse = stIn.ReadToEnd();
                                //   int indexOfRate = strResponse.IndexOf(@"Average&nbsp;(1&nbsp;days):");
                                //   string strCurRate = strResponse.Substring(indexOfRate + 98, 7);
                                //   stIn.Close();

                                //   ConvertionRate = Convert.ToDouble(strCurRate);

                                // ConvertionRate = currencyConvertor.ConversionRate((Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency)currencyRatioCountries[country], Easynet.Edge.UI.WebPages.CurrencyConvertor.Currency.USD);
                            }
                        }
                        catch (Exception e)
                        {

                            Log.Write("Cannot convert currency: " + e.Message, Easynet.Edge.Core.Utilities.LogMessageType.Error);
                            return false;
                        }
                        string cost = string.Empty;
                        // double ConvertionRate = Convert.ToDouble( ConvertionRates[country]);
                        for (int rowsCounter = firstRow; rowsCounter < dt.Rows.Count; rowsCounter++)
                        {
                            if (dt.Rows[rowsCounter][0].ToString().Equals(""))
                            {
                            }
                            else
                            {
                                sBuilder.Append("Yahoo\t" + base._account);
                                sBuilder.Append("\t" + rowString);



                                for (int k = 0; k < headersHash.Count; k++)
                                {
                                    int columNumber = Convert.ToInt32(headersHash[k]);
                                    if (columNumber != -1)
                                    {


                                        if (accountNamesDS.Tables.Count == 0)
                                        {
                                            base._account = "-1";
                                        }
                                        else
                                            base._account = FindAccountName(accountIDStr, "Yahoo_Account_Name", dt.Rows[rowsCounter][7].ToString());



                                        if (columnToConvertToUSD == columNumber)//CONVERT TO USD!
                                        {
                                            cost = dt.Rows[rowsCounter][columNumber].ToString().Replace(",", ".");
                                            //    cost = (Convert.ToDouble(cost) * ConvertionRate).ToString();
                                            sBuilder.Append("\t" + (Convert.ToDouble(cost) * ConvertionRate).ToString());
                                        }
                                        else
                                            sBuilder.Append("\t" + dt.Rows[rowsCounter][columNumber].ToString());
                                    }
                                }

                                sBuilder.Append("\t" + ConvertionRate);
                                sBuilder.Append("\t" + cost);

                                _wrtTxtFile.WriteLine(sBuilder);
                                sBuilder = new StringBuilder();
                            }
                        }
                    }
                    _wrtTxtFile.Close();

                    CopyFile();
                    _wrtTxtFile.Dispose();
                    return true;
                }
                catch (Exception ex)
                { return false; }
                finally
                {
                    _wrtTxtFile.Close();
                    _wrtTxtFile.Dispose();
                }
            }

        }
    }
}