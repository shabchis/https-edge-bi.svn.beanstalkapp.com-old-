using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using Easynet.Edge.Services.DataRetrieval.Retriever;
using System.Data.SqlClient;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections;


namespace EdgeBI.Data.Pipeline
{
    public static class FilesManager 
    {

        public static string _rootPath;
        //public static string GetFileName(ServicesType type)
        //{
        //    string rootFolder = AppSettings.Get(typeof(FilesManager), "RootFolder");
        //    switch (type)
        //    {
        //        case ServicesType.Creative:
        //            return Enum.GetName(typeof(ServicesType),type) + ".zip";
        //            break;
        //        case ServicesType.Adwords:
        //        case ServicesType.Analytics:
        //        case ServicesType.Content:
        //        case ServicesType.Status:
        //            return Enum.GetName(typeof(ServicesType),type) + ".xml";
        //            break;
        //        default:
        //            return Enum.GetName(typeof(ServicesType), ServicesType.None) + ".txt";
        //            break;
        //    }

        //}
        //public static string GetDeliveryFilePath(string RemoteFileServerHost, int accountid, int deliveryid, int channel, string servicetype,string fileName)
        //{
        //    if (accountid <= 0 && deliveryid <= 0 && channel == Channels.None && servicetype == ServicesType.None)
        //        throw new Exception("Function 'SetDeliveryFilesPath' missing one of the required parameter");

        //    if (!CheckHost(RemoteFileServerHost))
        //        throw new Exception("The remote file host not available");
        //    return RemoteFileServerHost + "\\" + GetAccountName(accountid) + "\\" + Enum.GetName(typeof(Channels), channel) + "\\" + DateTime.Today.Year.ToString() + "\\" + DateTime.Today.Month.ToString() + "\\" + DateTime.Today.Day.ToString() + "\\" + deliveryid.ToString() ;
        //}

        public static string GetDeliveryFilePath(string targetDir, DateTime targetDate, int deliveryID, string fileName, int? accountID)
        {
            if (accountID != null)
                targetDir = String.Format(@"{0}\Accounts\{1}",targetDir, accountID );

            string path = String.Format(@"{0}\{1:yyyy}\{1:MM}\{1:dd}\{2}\{3} [{4:yyyymmdd hhmmssfff}]{5}",
                targetDir,
                targetDate,
                deliveryID,
                Path.GetFileNameWithoutExtension(fileName),
                DateTime.Now,
                Path.GetExtension(fileName)
                );
            return path;
        }

        private static string GetAccountName(int accountid)
        {
            //TODO: Get connection from Configuration
            SqlConnection con = new SqlConnection("Data Source=console.edge-bi.com;Initial Catalog=Seperia_DWH;User ID=edge;Password=edgebi!");
            con.Open();
            SqlCommand command;
            SqlDataReader reader;
            command = new SqlCommand("SELECT [Account_Name] FROM [User_GUI_Account] WHERE [Account_ID] = " + accountid, con);
            reader = command.ExecuteReader();
            reader.Read();
            return Convert.ToString(reader["Account_Name"]);
        }

        private static FileInfo SetDeliveryFilePath(string filepath)
        {
            
            try
            {
                FileInfo lFileInfo = new FileInfo(filepath);
                if (!lFileInfo.Directory.Exists)
                {
                    lFileInfo.Directory.Create();
                }
                return lFileInfo;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        //private static bool CheckHost(string uri)
        //{
        //    WebRequest webRequest = WebRequest.Create(uri);  
        //    WebResponse webResponse;
        //    try 
        //    {
        //      webResponse = webRequest.GetResponse();
        //    }
        //    catch //If exception thrown then couldn't get response from address
        //    {
        //      return false;
        //    } 
        //    return true;
        //}


        public static void DownloadFile(string downloadUrl, string FilePath)
        {
            // Open a connection to the URL where the report is available.
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream httpStream = response.GetResponseStream();

            // Open the report file.
            FileInfo lFileInfo = SetDeliveryFilePath(FilePath);
            FileStream fileStream = new FileStream(
                lFileInfo.FullName,
                FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            BinaryReader binaryReader = new BinaryReader(httpStream);
            try
            {
                // Read the report and save it to the file.
                int bufferSize = 100000;
                while (true)
                {
                    // Read report data from API.
                    byte[] buffer = binaryReader.ReadBytes(bufferSize);

                    // Write report data to file.
                    binaryWriter.Write(buffer);

                    // If the end of the report is reached, break out of the 
                    // loop.
                    if (buffer.Length != bufferSize)
                    {
                        break;
                    }
                }
            }
            finally
            {
                // Clean up.
                binaryWriter.Close();
                binaryReader.Close();
                fileStream.Close();
                httpStream.Close();
            }
        }

        public static void ZipFiles(string inputFolderPath, string outputPathAndFile, string password)
        {
            ArrayList ar = GenerateFileList(inputFolderPath); // generate file list
            int TrimLength = (Directory.GetParent(inputFolderPath)).ToString().Length;
            // find number of chars to remove     // from orginal file path
            TrimLength += 1; //remove '\'
            FileStream ostream;
            byte[] obuffer;
            string outPath = inputFolderPath + @"\" + outputPathAndFile;
            ZipOutputStream oZipStream = new ZipOutputStream(File.Create(outPath)); // create zip stream
            if (password != null && password != String.Empty)
                oZipStream.Password = password;
            oZipStream.SetLevel(9); // maximum compression
            ZipEntry oZipEntry;
            foreach (string Fil in ar) // for each file, generate a zipentry
            {
                oZipEntry = new ZipEntry(Fil.Remove(0, TrimLength));
                oZipStream.PutNextEntry(oZipEntry);

                if (!Fil.EndsWith(@"/")) // if a file ends with '/' its a directory
                {
                    ostream = File.OpenRead(Fil);
                    obuffer = new byte[ostream.Length];
                    ostream.Read(obuffer, 0, obuffer.Length);
                    oZipStream.Write(obuffer, 0, obuffer.Length);
                }
            }
            oZipStream.Finish();
            oZipStream.Close();
        }


        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList fils = new ArrayList();
            bool Empty = true;
            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                fils.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0)
                // if directory is completely empty, add it
                {
                    fils.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // recursive
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    fils.Add(obj);
                }
            }
            return fils; // return file list
        }


        public static string UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            string fullPath = string.Empty;
            if(outputFolder == string.Empty || outputFolder  == null)
                outputFolder = Path.GetDirectoryName(zipPathAndFile);
                
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    fullPath = directoryName + "\\" + theEntry.Name;
                    fullPath = fullPath.Replace("\\ ", "\\");
                    string fullDirPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                    FileStream streamWriter = File.Create(fullPath);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
            return fullPath;
        }
    }
}
    