using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using System.Reflection;
using System.Diagnostics;
using Easynet.Edge.UI.WebPages.Classes.Convertors;

namespace Easynet.Edge.UI.WebPages.Converters
{

    
    public  class BaseConvertor
    {
        protected System.Collections.IDictionary convertionRateDic = new Dictionary<string, double>();
         
          public List<string> headersStrings = new List<string>();
         protected string CurrecnyCode;
        protected string DateFormat;


        private int AccountID;
        public int accountID
        {
            get { return AccountID; }
            set { AccountID = value; }
        }


        private string ErrorAccountString;
        public string errorAccountString
        {
            get { return ErrorAccountString  ; }
            set { ErrorAccountString = value; }
        }

        private string ProcessorSaveFilePath;
        public string processorSaveFilePath
        {
            get { return ProcessorSaveFilePath; }
            set { ProcessorSaveFilePath = value; }
        }



        protected void CopyFile()
        {
            try
            {
                string fromFile = saveFilePath + saveFileName;
                string toFile = processorSaveFilePath + saveFileName;
                if (fromFile.Equals(toFile))
                {
                    toFile += ".txt";
                }
                System.IO.FileInfo fi = new System.IO.FileInfo(fromFile);
                fi.CopyTo(toFile, true);
            }
            catch (Exception ex)
            {
               
            }
        }
         
        protected string _sqlCommand = @"select  Account_Name,accountSettings
                                    FROM [dbo].[User_GUI_Account]
                                    where accountSettings != '' ";

        protected int columnToConvertToUSD = -1;
        protected int destURLColumn = -1;
        protected DataSet accountNamesDS;
        public string convertionType;
        public string saveFileName;

        public BaseConvertor( )
        {
            processorSaveFilePath = saveFilePath;
        }
        public BaseConvertor (string _CurrencyCode, string dateformat)
        {
            processorSaveFilePath = saveFilePath;
             
            DateFormat = dateformat;
           CurrecnyCode =   _CurrencyCode;
        }
       

        public BaseConvertor(string account)
        {
            _account = account;
        }
        protected string _account;
        protected List<System.Data.DataTable> listTables;

        protected System.Data.OleDb.OleDbConnection _connection = new System.Data.OleDb.OleDbConnection();
        protected bool isValidDate;

        public string saveFilePath;
        public string SaveFilePath
        {
            get { return saveFilePath; }

            set { saveFilePath = value; }
        }

          ~BaseConvertor()
        {
            try
            {
             //   write.Close();
            }
            catch (Exception e) { }
            finally { 
               // write.Dispose();
            }
        }


          public string ReadHeaders(System.Xml.XmlAttributeCollection coll)
          {
              string ret = "";
              for (int i = 0; i < coll.Count; i++)
              {
                  ret += coll[i].Name + @" "; //list[0].SelectNodes("tmpHeader")[0].Attributes[0].Name
                  headersStrings.Add(coll[i].Value);
              }
              return ret;
          }



         public virtual System.Collections.Hashtable BuildHeadersOrder(List<string> headersStrings, DataTable dt, int firstRowIndex,string CostHeaderName)
          {
              MyLogger.Instance.Write(" 25  " + CostHeaderName);
              MyLogger.Instance.Write( " 26 " + headersStrings.Count.ToString());
              MyLogger.Instance.Write( " 27 " + dt.Columns.Count.ToString());
              try
              {
                  int hashTableKey = 0;
                  System.Collections.Hashtable ret = new System.Collections.Hashtable();
                  for (int i = 0; i < headersStrings.Count; i++)
                  {
                      for (int j = 0; j < dt.Columns.Count; j++)
                      {
                          MyLogger.Instance.Write("-> " + dt.Rows[firstRowIndex][j].ToString().ToLower());
                             
                          if (dt.Rows[firstRowIndex][j].ToString().ToLower().Equals(headersStrings[i].ToLower()))
                          {

                              ret.Add(hashTableKey, j);
                              hashTableKey++;
                                

                              if (dt.Rows[firstRowIndex][j].ToString().Equals(CostHeaderName))
                                  columnToConvertToUSD = j;
                          }
                      }
                  }
                  return ret;
              }
              catch (Exception ex)
              {
                  return null;
              }
          }
         //protected System.Collections.Hashtable buildHeadersOrder(List<string> headersStrings, DataTable dt, int firstRowIndex)
         // {
         //     try
         //     {
         //         System.Collections.Hashtable ret = new System.Collections.Hashtable();
         //         for (int i = 0; i < headersStrings.Count; i++)
         //         {
         //             for (int j = 0; j < dt.Columns.Count; j++)
         //             {
         //                 if (dt.Rows[8][j].ToString().Equals(headersStrings[i]))
         //                 {
         //                     ret.Add(i, j);
         //                 }
         //             }



         //         }
         //         return ret;
         //     }
         //     catch (Exception ex)
         //     {
         //         return null;
         //     }
         // }


         private static void PrintTable(DataTable dt)
         {
             DataTableReader dtReader = dt.CreateDataReader();
             while (dtReader.Read())
             {
                 for (int i = 0; i < dtReader.FieldCount; i++)
                 {
                     MyLogger.Instance.Write ( 
                         dtReader.GetName(i).Trim()+ " . "
                         +dtReader.GetValue(i).ToString().Trim());
                 }
                  
             }
             dtReader.Close();
         }


     
          protected virtual string FindAccountName(string key,string accoutnSettingKey,string campaignName)
          {
               
              MyLogger.Instance.Write("FindAccountName key: " + key);
              MyLogger.Instance.Write("FindAccountName accoutnSettingKey: " + accoutnSettingKey);
              MyLogger.Instance.Write("FindAccountName campaignName: " + campaignName);
              PrintTable (accountNamesDS.Tables[0] );


              if (accountNamesDS.Tables.Count == 0)
              {
                  return ErrorAccountString;
              }
             try
             {
                 var query = from r in accountNamesDS.Tables[0].AsEnumerable().AsQueryable()
                             where (r.Field<string>("accountSettings").Contains(key + "##")
                               && r.Field<string>("accountSettings").Contains(campaignName))
                             select r.Field<string>("Account_Name") + "<<>>" + r.Field<string>("accountSettings");
                 if (query.Count() > 0)
                     MyLogger.Instance.Write("FindAccountName var result in query1: " + query);
                 if (query.Count() == 0) //regular Account Name
                 {
                     var query2 = from r in accountNamesDS.Tables[0].AsEnumerable()

                                  where r.Field<string>("accountSettings").Contains(accoutnSettingKey + ":" + key)

                                  select r.Field<string>("Account_Name") ;
                     if (query2.Count() > 0)
                       MyLogger.Instance.Write("FindAccountName var result in query2: " + query2);

                     var query3 = from r in accountNamesDS.Tables[0].AsEnumerable()

                                  where r.Field<string>("accountSettings").Contains(key+"##")

                                  select r.Field<string>("Account_Name");

                     if (query3.Count() > 0)
                      MyLogger.Instance.Write("FindAccountName var result in query3: " + query3);
                   
                     IEnumerable<string> onlyAccount = query2.Except(query3);
                     if (onlyAccount.Count() == 1)
                     {//error - cannot be more than 1
                         MyLogger.Instance.Write("FindAccountName var result in onlyAccount: " + onlyAccount);
                         MyLogger.Instance.Write("onlyAccount.First(): " + onlyAccount.First());                 
                         return onlyAccount.First();
                     }

                     {
                         return ErrorAccountString;
                         
                     }
                 }
                 else //special account from campaign
                 {
                     foreach (var result in query)
                     {
                         MyLogger.Instance.Write("FindAccountName var result in queryffff: " + result);
                         //   bindingTable.Rows.Add(new object[] { result.ManagerEmail, result.EmailAddress });

                         SettingsCollection sc = new SettingsCollection(result);
                         foreach (var item in sc)
                         {
                             MyLogger.Instance.Write("FindAccountName item.Key: " + item.Key +" . item.Value:"+item.Value);
                             
                         }
                         if (sc[accoutnSettingKey].Contains(key))
                         {
                             MyLogger.Instance.Write("FindAccountName accoutnSettingKey: " + accoutnSettingKey);
                             string[] strArray = sc[accoutnSettingKey].Split(new string[] { "||" }, 200, StringSplitOptions.RemoveEmptyEntries);
                             if (strArray.Length > 0)
                             {
                                 MyLogger.Instance.Write("FindAccountName z: " );
                                 foreach (string values in strArray)
                                 {
                                     if (values.Contains(key))
                                     {
                                         MyLogger.Instance.Write("FindAccountName r  key: " + key);
                                         string newValue = values.Replace(key + "##", "");


                                         string[] Array = newValue.Split(new string[] { "|" }, 200, StringSplitOptions.RemoveEmptyEntries);
                                         foreach (string item in Array)
                                         {
                                             if (campaignName.Equals(item))
                                                 return result.Substring(0, result.IndexOf("<<>>"));
                                         }

                                     }
                                 }
                             }

                         }
                         //return "notfound";

                     } 
                 }
                 return "notfound";
 
                 
             }
             catch (Exception ex)
             {
                 MyLogger.Instance.Write("FindAccountName Exception ex" + ex);
                 return "";
             }
          }


          protected void FillAccountNames2DataSet(  )
          {

              lock (Easynet.Edge.Core.Data.DataManager.ConnectionString)
              {
                 // string prevConnString = Easynet.Edge.Core.Data.DataManager.ConnectionString;
                 // Easynet.Edge.Core.Data.DataManager.ConnectionString = @"Data Source=qa;Initial Catalog=easynet_OLTP;Integrated Security=False;User ID=edge2ui;PWD=Uzi!2009;";

                  using (Easynet.Edge.Core.Data.DataManager.Current.OpenConnection())
                  {
                      try
                      {


                          System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(_sqlCommand);
                          command.CommandType = CommandType.Text;

                          System.Data.SqlClient.SqlDataAdapter adapter1 = new System.Data.SqlClient.SqlDataAdapter(command);
                          adapter1.SelectCommand.Connection = new System.Data.SqlClient.SqlConnection();
                          adapter1.SelectCommand.Connection.ConnectionString = Easynet.Edge.Core.Data.DataManager.ConnectionString;


                          try
                          {
                              accountNamesDS = new DataSet();

                              adapter1.Fill(accountNamesDS);
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

          }
        public List<string> uploadFilePath;
        public List<string> UploadFilePath
        {
            get { return uploadFilePath; }

            set { uploadFilePath = value; }
        }

        protected System.Xml.XmlNodeList GetConvertionTypeNode()
        {
            System.Xml.XmlNode convertorsXML = (System.Xml.XmlNode)System.Configuration.ConfigurationManager.GetSection("convertionTypes");
            System.Xml.XmlNodeList list = convertorsXML.SelectNodes(this.convertionType);
            return list;
        }

       
        protected bool OpenConnection()
        {
            try
            {
                
                //write.WriteLine("    OpenConnection "  );
                if (_connection.ConnectionString != null)
                    _connection.Open();
 
                return true;
            }
            catch (Exception e)
            {
                MyLogger.Instance.Write("Failed to OpenConnection " + e.Message);
                //write.WriteLine("Failed to OpenConnection " + e.Message);
              //  Easynet.Edge.UI.WebPages.Classes.Convertors.MyLogger.Instance.Write(@"D:\log.txt", "Failed to OpenConnection " + e.Message);
                      
                return false;
            }
        }
       
        protected bool InitConnectionString(string soureFilePath)        
        {

            MyLogger.Instance.Write("InitConnectionString. soureFilePath: " + soureFilePath);
            if (System.IO.Path.GetExtension(soureFilePath) == ".xlsx")
            {
                _connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + soureFilePath + ";Extended Properties='Excel 12.0;HDR=Yes;'";
                return true;
            }
            else if (System.IO.Path.GetExtension(soureFilePath) == ".xls"  )
            {
                _connection.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + soureFilePath + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;ImportMixedTypes=Text;TypeGuessRows=60'";
                return true;
            }
            else if (System.IO.Path.GetExtension(soureFilePath) == ".csv")
            {
            //soureFilePath = @"C:\Convertor Files\1#csv";
            
             //   string connectionString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; Extended Properties=""text;HDR=YES;FMT=Delimited""", fileName);

                _connection.ConnectionString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;"+  soureFilePath+@";Extended Properties=""text;HDR=YES;FMT=Delimited""");
              //  _connection.ConnectionString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + soureFilePath + @";Extended Properties=""Excel 8.0;HDR=Yes;IMEX=0""");

              //  _connection.ConnectionString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=""" + soureFilePath + @""";Extended Properties='text;HDR=Yes;FMT=Delimited'");
                _connection.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + soureFilePath + ";Extended Properties='text;HDR=Yes;FMT=Delimited'";
          
              //   _connection.ConnectionString = string.Format(@"Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + soureFilePath + @";Extended Properties=""text;HDR=No""");
                // _connection.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + soureFilePath + ";Extended Properties=text;HDR=YES;FMT=Delimited;IMEX=1;ImportMixedTypes=Text;TypeGuessRows=60'";
                return true;
            }

            else return false;
        }
        public virtual bool DoWork(List<string> soureFileList, string saveFilePath)
        {
            return true;

        }

        public virtual bool DoWork()
        {
            return DoWork(this.uploadFilePath, this.saveFilePath + this.saveFileName);

        }


        public static string checkDateValidation(DateTime datetime)
        {

            string[] Split = new string[3];
            string correctDateFormat = null;
            Split[0] = datetime.Day.ToString();
            Split[1] = datetime.Month.ToString();
            Split[2] = datetime.Year.ToString().Substring(0, 4);
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


        public List<DataTable> ReadXLSFile(List<string> listFiles)
        {

                
                listTables = new List<DataTable>();
                DataSet dataSet1 = new DataSet();
                
                DataTable dt = new DataTable();
                String[] sheetName = null;
                System.Data.OleDb.OleDbDataAdapter dataAdapter1;

           

            try
            {
                foreach (var item in listFiles)
                {

                    if (System.IO.Path.GetExtension(item).ToLower().Equals(".csv"))
                    {
                        dt = CSVReader.GetDataTable(item);
                        if (null == dt)
                        {
                            //log
                            return null;
                        }
                        
                        listTables.Add(dt);
                    }

                    else
                    { //not cvs file -> XLSX file
                        //reading Excel sheets names
                        if (InitConnectionString(item) == false)
                        {

                            MyLogger.Instance.Write(" InitConnectionString(item) == false");
                           // Easynet.Edge.UI.WebPages.Classes.Convertors.MyLogger.Instance.Write(@"D:\log.txt", "InitConnectionString(item) == false");
                            return null;

                        }

                        if (OpenConnection() == false)
                        {
                            MyLogger.Instance.Write("OpenConnection() == false");
                          //  Easynet.Edge.UI.WebPages.Classes.Convertors.MyLogger.Instance.Write(@"D:\log.txt", "Cannot open connection !");

                            //write.WriteLine("Cannot open connection !");
                            return null;

                        }


                        //write.WriteLine("  open connection !");
                        dt = _connection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                        sheetName = new String[dt.Rows.Count];
                        int i = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            sheetName[i] = row["TABLE_NAME"].ToString();
                            i++;
                        }

                        dataAdapter1 = new System.Data.OleDb.OleDbDataAdapter("select * from [" + sheetName[0] + "]", _connection);

                        dataSet1 = new DataSet();
                        dataAdapter1.Fill(dataSet1);

                        //  write.WriteLine(dataSet1.Tables[0].Rows.Count);
                        listTables.Add(dataSet1.Tables[0]);
                        MyLogger.Instance.Write(" dataSet1.Tables[0]: rows count: " + dataSet1.Tables[0].Rows.Count);

                        _connection.Close();
                    }
                }
                _connection.Close();
              //  Easynet.Edge.UI.WebPages.Classes.Convertors.MyLogger.Instance.Write(@"D:\log.txt", "_connection.Close();");
                      
                return listTables;
            }
            catch (Exception ex)
            {
                MyLogger.Instance.Write("error ReadXLSFile: " + ex.Message);
               // Easynet.Edge.UI.WebPages.Classes.Convertors.MyLogger.Instance.Write(@"D:\log.txt", "error ReadXLSFile: " + ex.Message);
                //WriteToEventLog("Converter: \n" + ex.ToString());
                _connection.Close();
                _connection.Dispose();
                dt.Dispose();
                dataSet1.Dispose();
                return null;
            }
        }

    }


    public static class DataSetLinqOperators
    {
        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
        {
            return new ObjectShredder<T>().Shred(source, null, null);
        }

        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source,
                                                    DataTable table, LoadOption? options)
        {
            return new ObjectShredder<T>().Shred(source, table, options);
        } 

    }

    public class ObjectShredder<T>
    {
        private FieldInfo[] _fi;
        private PropertyInfo[] _pi;
        private Dictionary<string, int> _ordinalMap;
        private Type _type;

        public ObjectShredder()
        {
            _type = typeof(T);
            _fi = _type.GetFields();
            _pi = _type.GetProperties();
            _ordinalMap = new Dictionary<string, int>();
        }

        public DataTable Shred(IEnumerable<T> source, DataTable table, LoadOption? options)
        {
            if (typeof(T).IsPrimitive)
            {
                return ShredPrimitive(source, table, options);
            }


            if (table == null)
            {
                table = new DataTable(typeof(T).Name);
            }

            // now see if need to extend datatable base on the type T + build ordinal map
            table = ExtendTable(table, typeof(T));

            table.BeginLoadData();
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (options != null)
                    {
                        table.LoadDataRow(ShredObject(table, e.Current), (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(ShredObject(table, e.Current), true);
                    }
                }
            }
            table.EndLoadData();
            return table;
        }

        public DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
        {
            if (table == null)
            {
                table = new DataTable(typeof(T).Name);
            }

            if (!table.Columns.Contains("accountSettings"))
            {
                table.Columns.Add("accountSettings", typeof(T));
            }

            table.BeginLoadData();
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                Object[] values = new object[table.Columns.Count];
                while (e.MoveNext())
                {
                    values[table.Columns["accountSettings"].Ordinal] = e.Current;

                    if (options != null)
                    {
                        table.LoadDataRow(values, (LoadOption)options);
                    }
                    else
                    {
                        table.LoadDataRow(values, true);
                    }
                }
            }
            table.EndLoadData();
            return table;
        }

        public DataTable ExtendTable(DataTable table, Type type)
        {
            // value is type derived from T, may need to extend table.
            foreach (FieldInfo f in type.GetFields())
            {
                if (!_ordinalMap.ContainsKey(f.Name))
                {
                    DataColumn dc = table.Columns.Contains(f.Name) ? table.Columns[f.Name]
                        : table.Columns.Add(f.Name, f.FieldType);
                    _ordinalMap.Add(f.Name, dc.Ordinal);
                }
            }
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (!_ordinalMap.ContainsKey(p.Name))
                {
                    DataColumn dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name]
                        : table.Columns.Add(p.Name, p.PropertyType);
                    _ordinalMap.Add(p.Name, dc.Ordinal);
                }
            }
            return table;
        }

        public void WriteToEventLog(string message)
        {
            string EventLogSource = "xlsxConverter";
            EventLog elog = new EventLog();
            if (!EventLog.SourceExists(EventLogSource))
            {
                EventLog.CreateEventSource(EventLogSource, EventLogSource);
            }
            elog.Source = EventLogSource;
            //elog.EnableRaisingEvents = true;
            elog.WriteEntry(message, EventLogEntryType.Error);
        }


        public object[] ShredObject(DataTable table, T instance)
        {

            FieldInfo[] fi = _fi;
            PropertyInfo[] pi = _pi;

            if (instance.GetType() != typeof(T))
            {
                ExtendTable(table, instance.GetType());
                fi = instance.GetType().GetFields();
                pi = instance.GetType().GetProperties();
            }

            Object[] values = new object[table.Columns.Count];
            foreach (FieldInfo f in fi)
            {
                values[_ordinalMap[f.Name]] = f.GetValue(instance);
            }

            foreach (PropertyInfo p in pi)
            {
                values[_ordinalMap[p.Name]] = p.GetValue(instance, null);
            }
            return values;
        }
    }








}