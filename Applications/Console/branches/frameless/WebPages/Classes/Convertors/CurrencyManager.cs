using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Easynet.Edge.UI.WebPages.Classes.Convertors
{
    public static class CurrencyManager
    {
        public static double Convert(string fromCurr, string toCurr, string date)
        {
            string rowString = date;
            double ConvertionRate = -1;
            try
            {
                 
                    string url = @"http://www.oanda.com/currency/historical-rates?date_fmt=us&date="
                        +rowString.Substring(4,2)+@"/"+rowString.Substring(6,2) 
                        +@"/"+rowString.Substring(2,2)  +@"&date1="
                        + rowString.Substring(4,2)+@"/"+rowString.Substring(6,2) 
                        +@"/"+rowString.Substring(2,2)  +@"&exch=" 
                        +fromCurr
                        + @"&expr="+toCurr+@"&margin_fixed=0&format=HTML&redirected=1";


                    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);


                //    System.Net.WebResponse res = request.GetResponse();
                    System.IO.StreamReader stIn = new System.IO.StreamReader(request.GetResponse().GetResponseStream());

                   string strResponse = stIn.ReadToEnd();
                   int indexOfRate = strResponse.IndexOf(@"Average&nbsp;(1&nbsp;days):");
                   string strCurRate = strResponse.Substring(indexOfRate + 98, 7);
                   stIn.Close();

                   ConvertionRate = System.Convert.ToDouble(strCurRate);
                   return ConvertionRate;
            }
            catch (Exception ex)
            {
                return ConvertionRate;
            }
            
        }
    }
}
