
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
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.UI.WebPages.Converters
{
    public class FacebookConvertor : BaseConvertor
    {

        public FacebookConvertor(string CurrencyCode, string dateformat)
            : base(CurrencyCode, dateformat)
        {
        }

       
        public FacebookConvertor(string account)
            : base(account)
        { 
        }


        public override System.Collections.Hashtable BuildHeadersOrder( List<string> headersStrings,DataTable dt,int firstRow,string nothing)
        {
            try
            {
                System.Collections.Hashtable ret = new System.Collections.Hashtable();
                for (int i = 0; i < headersStrings.Count; i++)
                {
                 
                    int index =   dt.Columns.IndexOf(headersStrings[i]);
                    if(index !=-1)
                    ret.Add(i, index);
                }
                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string ReadHeaders(System.Xml.XmlAttributeCollection  coll  )
        {
            string ret="";
            for (int i = 0; i < coll.Count; i++)
            {
                ret += coll[i].Name+@" "; //list[0].SelectNodes("tmpHeader")[0].Attributes[0].Name
                headersStrings.Add(coll[i].Value);
            }

           

            return ret;
        }
        public override bool DoWork(List<string> soureFilePath, string saveFilePath)
        {  
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            StreamWriter wrtTxtFile = null;
            DataSet dataSet1 = new DataSet();
            DataTable dt = new DataTable();
            ReadXLSFile(soureFilePath);


            DateTime date = new DateTime();
            StringBuilder sBuilder = new StringBuilder();
            string rowString = "";
            string header;


            
           
             
         
            try
            {
                System.Xml.XmlNodeList list = GetConvertionTypeNode();
                
               // header = list[0].SelectNodes("tmpHeader")[0].Attributes["headers"].Value;

        
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

                int headersCount = list[0].SelectNodes("tmpHeader")[0].Attributes.Count; 
            }
            catch (Exception ex)
            { 
                if (wrtTxtFile != null)
                {
                    wrtTxtFile.Close();
                    wrtTxtFile.Dispose();
                }
                return false;
            }

            try
            {
                 wrtTxtFile.WriteLine(header);
                //  wrtTxtFile.WriteLine("Channel\tAccountName\tDay_Code\tHeadline\tDesc1\tDestURL\tAdgroup\tCampaign\tImps\tClicks\tCost\tPos");

                try
                { 
                    for (int i = 0; i < listTables.Count; i++)
                    { 
                        dt = listTables[i];
                        System.Collections.Hashtable headersHash =  BuildHeadersOrder(headersStrings, dt,0,"Cost");
                         
                        // double ConvertionRate = Convert.ToDouble( ConvertionRates[country]);
                        for (int rowsCounter = 0; rowsCounter < dt.Rows.Count; rowsCounter++)
                        {
                            if (dt.Rows[rowsCounter][0].ToString() != "")
                            {
                                sBuilder.Append("Facebook\t" + base._account);
                               

                           //      date = DateTime.ParseExact(dt.Rows[rowsCounter][0].ToString(), "MM/dd/yyyy", null);
                               //  date = (DateTime)dt.Rows[rowsCounter][0];
                                 string tempDate = dt.Rows[rowsCounter][0].ToString();
                                 string[] str = tempDate.Split(@"/".ToCharArray());
                                 if (str[2].Length > 4)
                                     str[2] = str[2].Substring(0, 4);
                                 date = new DateTime(Convert.ToInt32(str[2]), Convert.ToInt32(str[0]), Convert.ToInt32(str[1]));


                                rowString = checkDateValidation(date);
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
                                        sBuilder.Append("\t" + dt.Rows[rowsCounter][columNumber].ToString());
                                    }
                                } 

                                wrtTxtFile.WriteLine(sBuilder);
                                sBuilder = new StringBuilder();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                  //  write.WriteLine("error    :  "+e.Message);
                    string g = e.Message;
                }
                wrtTxtFile.Close();
                wrtTxtFile.Dispose();


              //  write.Close();
                return true;
            }
            catch (Exception e)
            {
              //  write.WriteLine("error    :  " + e.Message);
                return false; }

        }
    }
}
