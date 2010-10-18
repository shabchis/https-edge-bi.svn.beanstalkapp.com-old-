
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
using Easynet.Edge.UI.WebPages.Classes.Convertors;
 
 
 

 

namespace Easynet.Edge.UI.WebPages.Converters
{
    public partial class _888Convertor : BaseConvertor  
    {
        List<string> _headers = new List<string>();
        public _888Convertor() { }
       // private static bool sourceChosen = false;
      //  private static bool destinationChosen = false;
      //  private static bool isValidDate = true;

        public _888Convertor(string CurrencyCode, string dateformat)
            : base(CurrencyCode, dateformat)
        {          
        }


        //public override bool DoWork()
        //{
        //    return DoWork(this.uploadFilePath,this.saveFilePath);
        //}

        //public override bool DoWork()
        //{
        //   return  DoWork(this.uploadFilePath, this.saveFilePath+this.saveFileName);
        //}



        protected  string FindAccountName(string key, string accoutnSettingKey)
        {
            try
            {
//                OLD _sqlCommand = @"select [Gateway_id]  ,[Account_Name]
//  FROM [easynet_OLTP].[dbo].[UserProcess_GUI_Gateway],[easynet_OLTP].[dbo].[User_GUI_Account]
//  where  [Gateway_id] = "+key +@"and 
//  [easynet_OLTP].[dbo].[UserProcess_GUI_Gateway].[Account_ID] =
//   [easynet_OLTP].[dbo].[User_GUI_Account].[Account_ID] ";

                _sqlCommand = @"select  [easynet_OLTP].[dbo].[User_GUI_Account].[Account_Name]  
                  FROM [easynet_OLTP].[dbo].[UserProcess_GUI_Gateway],[easynet_OLTP].[dbo].[User_GUI_Account]
                  where  [Gateway_id] =" + key + @" and [easynet_OLTP].[dbo].[UserProcess_GUI_Gateway].account_id > 0 "

                              
                +
                @"and   [easynet_OLTP].[dbo].[UserProcess_GUI_Gateway].[Account_ID] =
                   [easynet_OLTP].[dbo].[User_GUI_Account].[Account_ID]
                   and 
                   
                   SCope_ID in (


                select  SCope_ID
                  FROM   [easynet_OLTP].[dbo].[User_GUI_Account]
                  where  
                   [easynet_OLTP].[dbo].[User_GUI_Account].[Account_ID] = "+this.accountID.ToString() +@"
                   
                   )
                   ";

                FillAccountNames2DataSet();
                if (accountNamesDS.Tables.Count == 0)
                {
                    MyLogger.Instance.Write("0 rows for key: " + key + " ,accoutnSettingKey:" + accoutnSettingKey);
                    //check in acccount table in accountSettings field for bo_publisher_name

                    _sqlCommand = @"select  [easynet_OLTP].[dbo].[User_GUI_Account].[AccountSettings], [easynet_OLTP].[dbo].[User_GUI_Account].[Account_Name] 
                  FROM [easynet_OLTP].[dbo].[User_GUI_Account])";



                    FillAccountNames2DataSet();
                    if (accountNamesDS.Tables.Count == 0 || accountNamesDS.Tables.Count > 1)
                    {
                        MyLogger.Instance.Write("0 rows for key: " + key + " ,accoutnSettingKey:" + accoutnSettingKey);

                        return errorAccountString;
                    }
                    else
                        return accountNamesDS.Tables[0].Rows[0][1].ToString();
                }
                else
                    if (accountNamesDS.Tables.Count > 1)
                    { //more than 2 rows {
                        MyLogger.Instance.Write("2 rows");
                        //in case of 2 rows or more we'll return the first one that found.
                        return accountNamesDS.Tables[0].Rows[0][0].ToString();
                    }
                    else
                    {
                        if (accountNamesDS.Tables[0].Rows.Count == 1)
                        {
                            MyLogger.Instance.Write("1 row: " + accountNamesDS.Tables[0].Rows[0][0].ToString());
                            return accountNamesDS.Tables[0].Rows[0][0].ToString();
                        }
                    }
                return errorAccountString;

            }
            catch (Exception ex)
            {
                MyLogger.Instance.Write("------------- Exception :" + ex);
                return errorAccountString;
            }
        }


        public override bool DoWork(List<string> soureFilePath, string saveFilePath)
        {
            MyLogger.Instance.Write(" DoWork start");
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            StreamWriter wrtTxtFile = null;
            DataSet dataSet1 = new DataSet();
            
            //OleDbDataAdapter dataAdapter1;
            DataTable dt = new DataTable();
           // String[] arrayStrSheetNames = null;

            ReadXLSFile(soureFilePath);

            

            string rowString = "";
            string tmpHeader = "";
            //reading the table Headers



            //OLD System.Xml.XmlNode convertorsXML = (System.Xml.XmlNode)System.Configuration.ConfigurationManager.GetSection("convertionTypes");
           //OLD System.Xml.XmlNodeList list = convertorsXML.SelectNodes("_888bo");


            System.Xml.XmlNodeList list = GetConvertionTypeNode();


            //  list =  list[0];

        //    if (list[0].SelectNodes("tmpHeader")[0].Value.Equals("_888Convertor"))  
            
         /*   string strSerial = string.Empty;
            string strDate = string.Empty; 
            for (int i = 0; i < list[0].SelectNodes("tmpHeader").Count; i++)
			{
                if( list[0].SelectNodes("tmpHeader")[i].InnerText.Equals("serial") )
                    strSerial = list[0].SelectNodes("tmpHeader")[i].Attributes[0].Value;

                if( list[0].SelectNodes("tmpHeader")[i].InnerText.Equals("date") )
                    strDate = list[0].SelectNodes("tmpHeader")[i].Attributes[0].Value;
			 
			}*/
            string AccountName = string.Empty;
            string AccountNameRow = string.Empty;
            string ClientSpecific = string.Empty;

             
            int TableHeaderCounterFromConfig = Convert.ToInt32(list[0].SelectNodes("TableHeaderCounter")[0].InnerText);
            //		list[0].SelectNodes("tmpHeader")[0].Attributes["serial"].Value	"Gateway_id"	string
            //int TableHeaderCounterFromConfig = list[0].SelectNodes("tmpHeader")[0].Attributes["serial"].Value);

            ClientSpecific = list[0].SelectNodes("tmpHeader")[0].Attributes["ClientSpecific"].Value;
          
            dt = listTables[0];

            MyLogger.Instance.Write("ssssssssss");
            System.Xml.XmlAttributeCollection item = null;
            int ClientNo = 1;
            for (int TableHeaderCounter = 0; TableHeaderCounter < dt.Columns.Count; TableHeaderCounter++)
            {
                if (TableHeaderCounter < TableHeaderCounterFromConfig)
                {
                    string temp = dt.Columns[TableHeaderCounter].ColumnName.ToString();
                    temp = temp.Replace(" ", "_");

                    if (TableHeaderCounter == 1)
                    {
                        rowString += "AccountName"+ "\t";
                    }

                    item = list[0].SelectNodes("tmpHeader")[0].Attributes;
                    for (int i = 0; i < item.Count; i++)
                    {   
                        //if(temp.ToLower().Contains(item[i].Name.ToLower()))
                        if (item[i].Name.ToLower().Equals(temp.ToLower()))
                        {
                            tmpHeader = item[i].Value;
                            _headers.Add(temp);
                            rowString += tmpHeader + "\t";
                        }
                    }
                }
                else
                {
                    rowString += ClientSpecific + ClientNo + "\t";
                    ClientNo++;
                }

            }
            //for (int TableHeaderCounter = 0; TableHeaderCounter < TableHeaderCounterFromConfig-3; TableHeaderCounter++)
            //{
                 
            //    string temp = dt.Columns[TableHeaderCounter].ColumnName.ToString();
            //    temp = temp.Replace(" ", "_");

            //     try
            //    {
            //        if (TableHeaderCounter == 1)
            //        {
            //            rowString += "AccountName"+ "\t";
            //        }

            //        item = list[0].SelectNodes("tmpHeader")[0].Attributes;
                    
            //        for (int i = 0; i < item.Count; i++)
            //        {
            //            if(item[i].Name.ToLower().Equals(temp.ToLower()))
            //            {
                            
            //               tmpHeader = item[i].Value;
            //               _headers.Add(temp);
            //               rowString += tmpHeader + "\t";
            //            }
            //        }
                     
            //    }

                   
            //      catch (Exception ex) { }

                
            //}



           
            //int ClientNo = 1;
            
          
            //for (int TableHeaderCounter = TableHeaderCounterFromConfig; TableHeaderCounter < dt.Columns.Count; TableHeaderCounter++)
            //{
            //    rowString += ClientSpecific + ClientNo + "\t";
            //    ClientNo++;
            //}

            MyLogger.Instance.Write("rowString: " + rowString);
            //Writing headers to file
            try
            {

                wrtTxtFile = new StreamWriter(saveFilePath,
                    false, Encoding.Unicode);
            
            }
            catch (Exception ex)
            {
              //  WriteToEventLog("Converter: \n" + ex.ToString());
                wrtTxtFile.Close();
                wrtTxtFile.Dispose();
            }
           
            wrtTxtFile.WriteLine(rowString);
            rowString = String.Empty;
            //reading current row
            try
            {
                DateTime date = new DateTime();
                for (int rowsCounter = 0; rowsCounter < dt.Rows.Count; rowsCounter++)
                {
                    if (dt.Rows[rowsCounter][0].ToString().Equals(""))
                    {
                    }
                    else
                    {
                        for (int columnsCounter = 0; columnsCounter <= TableHeaderCounterFromConfig; columnsCounter++)
                        {

                            if ((_headers.Contains(dt.Columns[columnsCounter].Caption) && (columnsCounter == 0))) //first column - date
                            {
                                //   rowString = dt.Rows[rowsCounter][columnsCounter].ToString();

                                if (this.DateFormat.Equals("yyyy/mm/dd"))
                                {
                                    string[] strDate = dt.Rows[rowsCounter][columnsCounter].ToString().Split(@"/".ToCharArray());
                                    if (strDate[0].Length >= 4)
                                    {
                                        date = new DateTime(Convert.ToInt32(strDate[0].Substring(0, 4))
                                      , Convert.ToInt32(strDate[1])
                                         , Convert.ToInt32(strDate[2]));
                                    }
                                    else
                                        date = new DateTime(Convert.ToInt32(strDate[2].Substring(0, 4))
                                           , Convert.ToInt32(strDate[0])
                                              , Convert.ToInt32(strDate[1]));

                                }
                                else
                                {
                                    string[] strDate = dt.Rows[rowsCounter][columnsCounter].ToString().Split(@"/".ToCharArray());
                                    if (strDate[0].Length >= 4)
                                    {
                                        date = new DateTime(Convert.ToInt32(strDate[0].Substring(0, 4))
                                      , Convert.ToInt32(strDate[2])
                                         , Convert.ToInt32(strDate[1]));
                                    }
                                    else
                                        date = new DateTime(Convert.ToInt32(strDate[2].Substring(0, 4))
                                           , Convert.ToInt32(strDate[1])
                                              , Convert.ToInt32(strDate[0]));

                                }
                                //  DateTime.ParseExact(dt.Rows[rowsCounter][columnsCounter].ToString(),base.DateFormat,null);
                                //  date = (DateTime)dt.Rows[rowsCounter][columnsCounter];
                                rowString = checkDateValidation(date, rowsCounter, dt.Rows.Count, ref wrtTxtFile);
                                if (rowString == null)  //date is not valid
                                {
                                    isValidDate = true;   //initilize
                                    return true;
                                }
                            }
                            else if (_headers.Contains(dt.Columns[columnsCounter].Caption))
                            {
                                //if (columnsCounter == 1) //account = serial
                                //{
                                //  rowString += "\t" + dt.Rows[rowsCounter][columnsCounter].ToString();
                                string accoutnName = FindAccountName(dt.Rows[rowsCounter][columnsCounter].ToString(), " ");
                                rowString += "\t" + accoutnName; //adding accountName
                                rowString += "\t" + dt.Rows[rowsCounter][columnsCounter].ToString();//adding the current culomn in table
                                //}
                            }
                        }
                        for (int columnsCounter2 = TableHeaderCounterFromConfig; columnsCounter2 < dt.Columns.Count; columnsCounter2++)
                        {
                            rowString += "\t" + dt.Rows[rowsCounter][columnsCounter2].ToString();
                        }
                        rowString = rowString.Replace("$", "");
                        wrtTxtFile.WriteLine(rowString);
                        rowString = String.Empty;
                        
                        //for (int columnsCounter = 0; columnsCounter < TableHeaderCounterFromConfig - 5; columnsCounter++)
                        //{
                        //    if (dt.Rows[rowsCounter][columnsCounter].ToString().Equals(""))
                        //    {

                        //    }
                        //    else
                        //    {

                        //         if (columnsCounter == 1) //account = serial
                        //        {
                        //            //  rowString += "\t" + dt.Rows[rowsCounter][columnsCounter].ToString();
                        //            string accoutnName = FindAccountName(dt.Rows[rowsCounter][columnsCounter].ToString(), " ");
                        //            rowString += "\t" + accoutnName;
                        //        }


                        //        if (columnsCounter == 0) //first column - date
                        //        {
                        //            //   rowString = dt.Rows[rowsCounter][columnsCounter].ToString();

                        //            if (this.DateFormat.Equals("yyyy/mm/dd"))
                        //            {
                        //                string[] strDate = dt.Rows[rowsCounter][columnsCounter].ToString().Split(@"/".ToCharArray());
                        //                date = new DateTime(Convert.ToInt32(strDate[2].Substring(0, 4))
                        //                   , Convert.ToInt32(strDate[0])
                        //                      , Convert.ToInt32(strDate[1]));

                        //            }
                        //            else
                        //            {
                        //                string[] strDate = dt.Rows[rowsCounter][columnsCounter].ToString().Split(@"/".ToCharArray());
                        //                date = new DateTime(Convert.ToInt32(strDate[2].Substring(0, 4))
                        //                   , Convert.ToInt32(strDate[1])
                        //                      , Convert.ToInt32(strDate[0]));

                        //            }
                        //            //  DateTime.ParseExact(dt.Rows[rowsCounter][columnsCounter].ToString(),base.DateFormat,null);
                        //            //  date = (DateTime)dt.Rows[rowsCounter][columnsCounter];
                        //            rowString = checkDateValidation(date, rowsCounter, dt.Rows.Count, ref wrtTxtFile);
                        //            if (rowString == null)  //date is not valid
                        //            {
                        //                isValidDate = true;   //initilize
                        //                return true;
                        //            }
                        //        }
                               
                        //        else
                        //        {
                        //            rowString += "\t" + dt.Rows[rowsCounter][columnsCounter].ToString();
                        //        }
                        //    }

                        //}
                        //for (int columnsCounter2 = TableHeaderCounterFromConfig; columnsCounter2 < dt.Columns.Count; columnsCounter2++)
                        //{

                        //    rowString += "\t" + dt.Rows[rowsCounter][columnsCounter2].ToString();
                        //}
                        //rowString = rowString.Replace("$", "");
                        //wrtTxtFile.WriteLine(rowString);
                        //rowString = String.Empty;
                    }
                }
                wrtTxtFile.Close();

                CopyFile();

                wrtTxtFile.Dispose();
            }
            catch (Exception ex)
            {
                MyLogger.Instance.Write("Exception1: " + ex);
                wrtTxtFile.Close();
                wrtTxtFile.Dispose();
            }
          
            return true;
        }
        private static string checkDateValidation(DateTime rowString,int rowsCounter, int rowsNum,ref StreamWriter wrtTxtFile)
        {

            string[] Split = new string[3];
            string correctDateFormat = null;
            Split[0] = rowString.Day.ToString();
            Split[1] = rowString.Month.ToString();
            Split[2] = rowString.Year.ToString().Substring(0, 4);
            if (Split[0].Length == 1 && Split[1].Length == 1)
            {
                correctDateFormat = Split[2] + "0" + Split[1] + "0" + Split[0];
            }
            else if (Split[0].Length == 2 && Split[1].Length == 2)
            {
                correctDateFormat = Split[2] + Split[1] + Split[0];
            }
            else if (Split[0].Length == 2 && Split[1].Length == 1)
            {
                correctDateFormat = Split[2] + "0" + Split[1] + Split[0];
            }
            else if (Split[0].Length == 1 && Split[1].Length == 2)
            {
                correctDateFormat = Split[2] + Split[1] + "0" + Split[0];
            }
            return correctDateFormat;
        }

      
        
    }
}
