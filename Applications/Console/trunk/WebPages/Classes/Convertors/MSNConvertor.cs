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
    public class MSNConvertor : BaseConvertor
    {

        string gg;
        private double ConvertionRate=-1;
        public MSNConvertor(string CurrencyCode, string dateformat)
            : base(CurrencyCode, dateformat)
        {
        }
         

        public override bool DoWork(List<string> soureFilePath, string saveFilePath)
        {
          
            
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            StreamWriter wrtTxtFile = null;
            DataSet dataSet1 = new DataSet();
            DataTable dt = new DataTable();
            ReadXLSFile(soureFilePath);
            string dateFormat = string.Empty;

            if (listTables.Count == 0)
            {
                //  MyLogger.Instance.Write(@"D:\log.txt", "empty table");
            }

            DateTime date = new DateTime();
            StringBuilder sBuilder = new StringBuilder();
            string rowString = "";

            _sqlCommand = @"Select Account_Name,AccountSettings FROM [easynet_OLTP].[dbo].[User_GUI_Account] where AccountSettings   like '%Bing_Account_Name%'";
                    



            string header;
            try
            {
               
                System.Xml.XmlNodeList list = GetConvertionTypeNode();
               
                       
              //  header = list[0].SelectNodes("tmpHeader")[0].Attributes["headers"].Value;
               //OLD dateFormat = (list[0].SelectNodes("DateFormat")[0]).InnerText;
                dateFormat = base.DateFormat;
                
                
                header = ReadHeaders(list[0].SelectNodes("tmpHeader")[0].Attributes);

                
                header = "Channel AccountName day_code " + header;
              
                header = header.Replace(@" ", "\t");
                date = DateTime.Now;

               
                if (saveFilePath != "")
                    wrtTxtFile = new StreamWriter(saveFilePath, false, Encoding.Unicode);
                else
                {//default
                    wrtTxtFile = new StreamWriter(saveFilePath,//saveFilePath + "\\" + "CSV_EF_Yahoo_" + date.Day.ToString() + "." + date.Month.ToString() + "." + date.Year.ToString() + ".xls",//+DateTime.Now.Month.ToString().Substring(0,3)+"_"+  "Dec_22.12.09.xls",
                  false, Encoding.Unicode);
                }

            }
            catch (Exception ex)
            {

                MyLogger.Instance.Write( " ex6 " + ex);
                // WriteToEventLog("Converter: \n" + ex.ToString());
                if (wrtTxtFile != null)
                {
                    wrtTxtFile.Close();
                    wrtTxtFile.Dispose();
                }
                return false;
            }

            //MyLogger.Instance.Write(@"sssssssssssssss()", "ecccccccccccccrrerA");
            try
            {
                
                if (base.CurrecnyCode != string.Empty)
                {
                    header += "CostConversionRate\tCostBeforeConversion";
                    
                }

               
                wrtTxtFile.WriteLine(header);
                //  wrtTxtFile.WriteLine("Channel\tAccountName\tDay_Code\tHeadline\tDesc1\tDestURL\tAdgroup\tCampaign\tImps\tClicks\tCost\tPos");
                
                try
                {
                   // Get Accounts names into DataSet
                    //{
                    //    FillAccountNames2DataSet();

                    //}
                    
                    //MyLogger.Instance.Write(@"111", "eoo  ");
                    FillAccountNames2DataSet();


                     
                    //MyLogger.Instance.Write(@"22", "e999 ");
                    for (int i = 0; i < listTables.Count; i++)
                    {

                       
                        dt = listTables[i];
                        System.Collections.Hashtable headersHash = base.BuildHeadersOrder(headersStrings, dt,8,"Spend");


                            
                        if (columnToConvertToUSD == -1)
                        {
                            MyLogger.Instance.Write(" ERROR:  columnToConvertToUSD == -1");
                            return false;
                        }




                        MyLogger.Instance.Write(" rows:  "+dt.Rows.Count);
                        // double ConvertionRate = Convert.ToDouble( ConvertionRates[country]);
                        for (int rowsCounter = 9; rowsCounter < dt.Rows.Count; rowsCounter++)
                        {
                          
                            string cost = string.Empty;

                            if (dt.Rows[rowsCounter][0].ToString() == "")
                            {
                                rowsCounter=1+dt.Rows.Count;
                            }
                            else
                            {
                                string account = base._account;


                                account = FindAccountName(dt.Rows[rowsCounter][1].ToString(), "Bing_Account_Name", dt.Rows[rowsCounter][4].ToString());
                                
                                if(account.Equals(""))
                                {


                                    MyLogger.Instance.Write(" cannot find account  " + dt.Rows[rowsCounter][4].ToString());
                                    account=base.errorAccountString;
                                }
                                sBuilder.Append("bing\t" + account);

                                string tempDate = dt.Rows[rowsCounter][0].ToString();
                                string[] str = tempDate.Split( @"/".ToCharArray());
                                //date = new DateTime(Convert.ToInt32(str[2]), Convert.ToInt32(str[0]), Convert.ToInt32(str[1]));

                                if (dateFormat.Substring(0, 1).Equals("m"))
                                {
                                    string temp = str[0];
                                    str[0] = str[1];
                                    str[1] = temp;
                                }

                                date = new DateTime(Convert.ToInt32(str[2]), Convert.ToInt32(str[1]), Convert.ToInt32(str[0]));
                            //    date = DateTime.ParseExact(tempDate, dateFormat, null);
                              // date = DateTime.ParseExact(dt.Rows[rowsCounter][0].ToString(), "MM/dd/yyyy", null);
                                //  date = (DateTime)dt.Rows[rowsCounter][0];
                                rowString = checkDateValidation(date);


                                if (convertionRateDic[rowString] == null)
                                {
                                    ConvertionRate = Easynet.Edge.UI.WebPages.Classes.Convertors.CurrencyManager.Convert(base.CurrecnyCode, "USD", rowString);
                                    convertionRateDic.Add(rowString, ConvertionRate);
                                }
                                else
                                {
                                    ConvertionRate =Convert.ToDouble(convertionRateDic[rowString]);
                                }


                                //if (rowsCounter == 8000)
                                //    date.AddDays(1);
                                if (rowString == null)  //date is not valid
                                {
                                    isValidDate = true;   //initilize
                                    return true;
                                }


                              

                                sBuilder.Append("\t" + rowString);

                                for (int k = 0; k < headersHash.Count; k++)
                                {
                                    int columNumber = Convert.ToInt32(headersHash[k]);
                                    if (columNumber != -1)
                                    {
                                        if (columnToConvertToUSD == columNumber)//CONVERT TO USD!
                                        {
                                            cost = dt.Rows[rowsCounter][columNumber].ToString();
                                            //    cost = (Convert.ToDouble(cost) * ConvertionRate).ToString();
                                            sBuilder.Append("\t" + (Convert.ToDouble(cost) * ConvertionRate).ToString());
                                        }
                                        else
                                        {
                                           

                                            string value = dt.Rows[rowsCounter][columNumber].ToString();
                                            int indexOfHttp = value.IndexOf("http");
                                            if (indexOfHttp != -1 && value.IndexOf("param1") != -1)
                                                value = value.Remove(0, indexOfHttp);
                                            sBuilder.Append("\t" + value);
                                        }
                                    }
                                }
                             /*   sBuilder.Append("\t" + dt.Rows[rowsCounter][2].ToString()
                                + "\t" + dt.Rows[rowsCounter][3].ToString()
                                + "\t" + dt.Rows[rowsCounter][4].ToString()
                                + "\t" + dt.Rows[rowsCounter][5].ToString()
                                + "\t" + dt.Rows[rowsCounter][7].ToString()                           
                                + "\t" + dt.Rows[rowsCounter][8].ToString()
                                + "\t" + dt.Rows[rowsCounter][9].ToString()
                                + "\t" + dt.Rows[rowsCounter][12].ToString()
                                  + "\t" + dt.Rows[rowsCounter][13].ToString());
                                */
                                if (ConvertionRate != -1)
                                {
                                    sBuilder.Append("\t" + ConvertionRate);
                                    sBuilder.Append("\t" + cost);
                                }

                                wrtTxtFile.WriteLine(sBuilder);
                                sBuilder = new StringBuilder();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MyLogger.Instance.Write("e  " + e.Message);
                    //MyLogger.Instance.Write(@"sssxxxx)", "e  " + e.Message);
                    string g = e.Message;
                }

              
             
                wrtTxtFile.Close();


                CopyFile();


                wrtTxtFile.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //MyLogger.Instance.Write(@"awerrrrrxx)", "e  " + ex.Message);
                return false; }

        }
    }
}
