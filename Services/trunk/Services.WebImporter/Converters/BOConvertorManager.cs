using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace Easynet.Edge.Services.WebImporter.Converters
{
    public class BOConvertorManager
    {
        private List<BaseConvertor> listOfConvertorsToRun;

        public void DoWork(string accountID, List<string> uploadFilesList,string saveFileName, string sourcType)
        {
            Init(accountID, sourcType);

			if (listOfConvertorsToRun == null || listOfConvertorsToRun.Count < 1)
				return;

            for (int i = 0; i < listOfConvertorsToRun.Count; i++)
            {
                listOfConvertorsToRun[i].uploadFilePath = uploadFilesList;
                listOfConvertorsToRun[i].saveFileName = saveFileName;
                listOfConvertorsToRun[i].DoWork();
            }
        }


        private void Init(string accountID, string sourcType)
        {
            System.Xml.XmlNode myXML = (System.Xml.XmlNode)ConfigurationManager.GetSection("convertAccounts");

            System.Xml.XmlNodeList list = myXML.ChildNodes;
            string erroAccount="-1";


            foreach (System.Xml.XmlNode item in list)
	        {
                if (item.Attributes["id"].Value.Equals(accountID))
                {
                    for (int i = 0; i < item.ChildNodes.Count; i++)
                    {
                        string convertorData = item.ChildNodes[i].InnerText;
                        if (convertorData.Contains(sourcType))
                        {
                            string CurrencyCode,DateFormat;
                            try
                            {
                                CurrencyCode = item.ChildNodes[i].Attributes["money"].Value;
                            }
                            catch
                            {
                                CurrencyCode = "";
                            }

                            try
                            {
                                DateFormat = item.ChildNodes[i].Attributes["DateFormat"].Value;
                            }
                            catch
                            {
                                DateFormat = "";
                            }

                            try
                            {
                                erroAccount = item.ChildNodes[i].Attributes["errorAccount"].Value;
                            }
                            catch
                            {
                                erroAccount = "";
                            }


                            

                            myXML = (System.Xml.XmlNode)ConfigurationManager.GetSection("convertionTypes");

                            System.Xml.XmlNodeList list2 = ((System.Xml.XmlNode)ConfigurationManager.GetSection("convertionTypes")).SelectNodes(convertorData);

                            string className = list2[0].SelectNodes("class")[0].InnerText;
                            if (className.Equals(""))//Class does not exist!
                            {
                                //write to log
                                return;
                            }
                            if (className[0].Equals("_"))//Class does not exist!
                            { 
                                className = className.Remove(0,1);
                            }
                          // string accoutName = GetAccountNameForRows( list2[0]);

                         //  BaseConvertor myConvertor = InitConvertor(accoutName, className);
                            BaseConvertor myConvertor = InitConvertor(className, CurrencyCode, DateFormat);
                            myConvertor.accountID =Convert.ToInt32(accountID);
                            

                            myConvertor.errorAccountString = erroAccount;
                            //    BaseConvertor myConvertor = InitConvertor(item.ChildNodes[i].Attributes["AccountNameRows"].Value, item.ChildNodes[i].InnerText);
                            if (myConvertor != null)
                            {
                                //   myConvertor. = item.ChildNodes[i].Attributes["FileSavePath"];


                                try
                                {
                                    myConvertor.processorSaveFilePath = list2[0].SelectNodes("FileSavePath")[0].InnerText;
                                }
                                catch { }
                                myConvertor.saveFilePath = list2[0].SelectNodes("ProcessorFilePath")[0].InnerText;
                                
                                myConvertor.convertionType = convertorData;

                                //   myConvertor.saveFilePath = item.ChildNodes[i].Attributes["FileSavePath"].Value;
                                //     myConvertor._acc
                                if (listOfConvertorsToRun == null)
                                    listOfConvertorsToRun = new List<BaseConvertor>();
                                listOfConvertorsToRun.Add(myConvertor);
                            }
                        }
                        
                    }
                  //item.ChildNodes[1].InnerText	"Bing"	string

                    break;
                }
                
	        }
        }
       
		private BaseConvertor InitConvertor(string convertor,string CurrencyCode,string dateformat)
        {


            BaseConvertor myConvertor = null;
            if (convertor.Equals("888Convertor"))
            {
                myConvertor = new _888Convertor(CurrencyCode,  dateformat);

            }
            else if (convertor.Equals("YahooConvertor"))
            {
                myConvertor = new YahooConvertor(CurrencyCode, dateformat);

            }
            else if (convertor.Equals("MSNConvertor"))
            {
                myConvertor = new MSNConvertor(CurrencyCode, dateformat);

            }
            else if (convertor.Equals("FacebookConvertor"))
            {
                myConvertor = new FacebookConvertor(CurrencyCode, dateformat);

            }
            else if (convertor.Equals("CreativeTXTfileConvertor"))
            {
                myConvertor = new CreativeTXTfileConvertor();
            }
            
            return myConvertor;

        }

    }

    
}
