using System;
using System.Collections.Generic;
using System.Linq;

namespace Easynet.Edge.Services.WebImporter.Converters
{
    static class  CSVReader
    {
        static public System.Data.DataTable GetDataTable(string strFileName)
        {

            // string ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\Convertor Files\;Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;ImportMixedTypes=Text;TypeGuessRows=60'";

            //  System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + System.IO.Path.GetDirectoryName(strFileName) + "; Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;ImportMixedTypes=Text;TypeGuessRows=60'");
            System.Data.OleDb.OleDbConnection conn= new System.Data.OleDb.OleDbConnection();

            try
            {
                conn = new System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + System.IO.Path.GetDirectoryName(strFileName) + "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");
                conn.Open();
                string strQuery = "SELECT * FROM [" + System.IO.Path.GetFileName(strFileName) + "]";
                System.Data.OleDb.OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter(strQuery, conn);
                System.Data.DataSet ds = new System.Data.DataSet("CSV File");
                adapter.Fill(ds);
                return ds.Tables[0];
           
               
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                conn.Close();
            }

        }
    }
}
