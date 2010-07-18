using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Easynet.Edge.UI.WebPages.Classes.Convertors
{
    public class MyLogger
    {
        //private string filePath = @"D:\Convertor Files\t.txt";
        private static MyLogger instance;
        public System.IO.StreamWriter writer;
        private MyLogger() { }

        public static MyLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MyLogger();
                }
                return instance;
            }
        }

        public void Write(string filePath, string message)
        {
            //if (writer == null)
            //    writer = new System.IO.StreamWriter(filePath);
           
            //writer.WriteLine(DateTime.Now.ToString() + ":  " + message);
            //writer.Flush();
             
        }

        public void Write(string message)
        {

            //if (writer == null)
            //    writer = new System.IO.StreamWriter(filePath);

            //writer.WriteLine(DateTime.Now.ToString() + ":  " + message);
            //writer.Flush();

        }


        //~MyLogger()
        //{
           
        //    writer.Close();
        //     writer.Dispose();
        //}
    }

}
