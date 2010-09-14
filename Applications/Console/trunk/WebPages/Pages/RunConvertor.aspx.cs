using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Workflow;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
 
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using Easynet.Edge.UI.WebPages.Classes.Convertors;

namespace Easynet.Edge.UI.WebPages
{
    public partial class RunConvertor : System.Web.UI.Page
    {
        protected string ErrorMessage = null;
        protected string SuccessMessage = null;
        private Dictionary<string, string> listOfFilesToUpload = new Dictionary<string, string>();
        Converters.BaseConvertor _Convertor;
        protected void Page_Load(object sender, EventArgs e)
        {
            _sourceSelector.TextChanged += new EventHandler(_sourceSelector_TextChanged);
            _sourceSelector.SelectedIndexChanged += new EventHandler(_sourceSelector_SelectedIndexChanged);


            if (Session["listOfFilesToUpload"] != null)
                listOfFilesToUpload = (Dictionary<string, string>)Session["listOfFilesToUpload"];
            _submit.Enabled = false;
        }

       

        public void WriteToEventLog(string message)
        {
            string EventLogSource = "xlsxConverter";
            System.Diagnostics.EventLog elog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(EventLogSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(EventLogSource, EventLogSource);
            }
            elog.Source = EventLogSource;
            //elog.EnableRaisingEvents = true;
            elog.WriteEntry(message, System.Diagnostics.EventLogEntryType.Error);
        }


        void _submit2_Click(object sender, EventArgs e)
        {
            if (fileUpload.PostedFile.FileName == "")// empty filename
            {
            }
            else
            {
                try
                {
                    //if 'BO' or 'Bing' and one file already selected
                    if ((_sourceSelector.Text.Equals("BO") || _sourceSelector.Text.Equals("Bing"))
                        && listboxFiles.Items.Count > 0)
                    {
                        //replace the current file
                        EraseFiles();
                    }



                    string newFileName = string.Empty;
                    System.Xml.XmlNode myXML = (System.Xml.XmlNode)ConfigurationManager.GetSection("convertAccounts");
                    System.Xml.XmlNode SaveFilePathNode = myXML.SelectSingleNode(@"SaveFilePath");
                    string path = SaveFilePathNode.InnerText;
                    if (listboxFiles.Items.Count == 0)
                    {
                        listOfFilesToUpload = new Dictionary<string, string>();
                        Session["listOfFilesToUpload"] = listOfFilesToUpload;
                    }


                    {

                        newFileName = fileUpload.FileName;
                        ListItem item = listboxFiles.Items.FindByText(newFileName);
                        if (item != null)//this file already exist in listbox
                        {
                            //log or something
                        }
                        else
                        {

                            if (Path.GetExtension(fileUpload.PostedFile.FileName).ToLower().Equals(".tsv") && !_sourceSelector.Text.ToLower().Equals("yahoo"))//copy to '.csv'
                            {
                                newFileName = Path.ChangeExtension(path + newFileName, "csv");
                            }
                            else
                            {
                                newFileName = path + newFileName;
                            }
                            string originalFileName = fileUpload.PostedFile.FileName;

                            listOfFilesToUpload.Add(originalFileName, newFileName);


                            try
                            {
                                fileUpload.PostedFile.SaveAs(path + fileUpload.FileName);

                            }
                            catch (Exception ex)
                            {
                                saveFilePathTextBox.Text = "FGGG :" + ex.Message;
                            }





                            listboxFiles.Items.Add(originalFileName);//fileUpload.PostedFile.FileName);

                            try
                            {
                                if (Path.GetExtension(originalFileName) != Path.GetExtension(newFileName))//copy to '.csv'
                                {

                                    System.IO.File.Copy(Path.ChangeExtension(newFileName, "tsv"), newFileName, true);
                                }

                            }
                            catch (Exception ex)
                            {
                                saveFilePathTextBox.Text = "x :" + ex.Message + ". " + Path.ChangeExtension(newFileName, "tsv") + " ," + newFileName;
                            }


                            int count = listboxFiles.Items.Count;
                            if (count > 0)
                            {
                                string outputFileName = listboxFiles.Items[count - 1].Text;
                                string name = Path.GetFileName(outputFileName);
                                string exten = Path.GetExtension(name);
                                //saveFilePathTextBox.Text = name.Remove(name.Length - exten.Length, exten.Length);
                                saveFilePathTextBox.Text = Request.QueryString["accountID"] + "_" + name.Remove(name.Length - exten.Length, exten.Length) + DayCode.ToDayCode(DateTime.Today) + "_" + DateTime.Now.TimeOfDay.Hours + "_" + DateTime.Now.TimeOfDay.Minutes + "_" + DateTime.Now.TimeOfDay.Seconds;
                            }
                        }
                    }
                }



                catch (Exception ex)
                {
                    saveFilePathTextBox.Text = "EX :" + ex.Message;
                }
                _submit.Enabled = true;
            }
        }

        void _submit_Click(object sender, EventArgs e)
        {
           RunRquest(); 
        }

        void RunRquest()
        {
            MyLogger.Instance.Write( "RunRquest()");
            Converters.BOConvertorManager manager = new Easynet.Edge.UI.WebPages.Converters.BOConvertorManager();
            try
            {
                if (listboxFiles.Items.Count == 0)
                {
                    
                    ErrorMessage = "please select a file to upload";
                    return;
                }

                List<string> listOfFiles = new List<string>();
                foreach (var item in listOfFilesToUpload)
                {
                    MyLogger.Instance.Write("item: " + item.ToString());
                   // MyLogger.Instance.Write(@"C:\LOGGER.TXT", "S: " + item.ToString());
                    //  SaveSourceFile(item.ToString());  
                    //  listOfFiles.Add(item.ToString());                      
                    //    listOfFiles.Add(@"C:\Inetpub\wwwroot\webtools\Convertor Files\" + System.IO.Path.GetFileName(item.ToString())); 
                    listOfFiles.Add(item.Value.ToString());
                }
                string accountID = Convert.ToInt32(Request.QueryString["accountID"]).ToString();
                string SourceType = _sourceSelector.Text;
                SourceType = manager.DoWork(accountID, listOfFiles, saveFilePathTextBox.Text, SourceType);
                SuccessMessage = SourceType;
                listboxFiles.Items.Clear();

                saveFilePathTextBox.Text = "";

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void EraseFiles()
        {

            listboxFiles.Items.Clear();
            listOfFilesToUpload.Clear();
            saveFilePathTextBox.Text = "";
        }
        void _sourceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            EraseFiles();
        }

        


        public void Button1_Click(object sender, EventArgs e)
        {
            DataTable tbl = new DataTable();
        }

         
        protected override void OnInit(EventArgs e)
        {
            try
            {
               
               
                _removeFile.Click += new EventHandler(_removeFile_Click);
                _addFileBtn.Click += new EventHandler(_submit2_Click);

                _submit.Click += new EventHandler(_submit_Click);

                base.OnInit(e);


                // if (Session["clients"] == null)
                //{
                //    DataTable tbl;

                //    DataManager.ConnectionString = DataManager.ConnectionString.Replace("hal", "qa");
                //    SqlCommand cmd = DataManager.CreateCommand("getAccountList(@IsRankingAccount:Int)", CommandType.StoredProcedure);
                //    cmd.Parameters["@IsRankingAccount"].Value = 3; //Means only Clients
                //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);


                //    lock (DataManager.ConnectionString)
                //    {
                //        tbl = new DataTable();
                //        using (DataManager.Current.OpenConnection())
                //        {
                //            DataManager.Current.AssociateCommands(cmd);
                //            adapter.Fill(tbl);
                //        }
                //    }

                //    Session["clients"] = tbl;
                //}


                // _clientSelector.DataSource = (DataTable)Session["clients"];
                // _clientSelector.DataTextField = "client_name";
                // _clientSelector.DataValueField = "client_id";
                // _clientSelector.DataBind();

                DataTable dt = new DataTable();

                _sourceSelector.Items.Add("BackOffice");
              
                //_sourceSelector.Items.Add("Facebook");
                _sourceSelector.Items.Add("Bing");
                 _sourceSelector.Items.Add("Yahoo");
                 _sourceSelector.Items.Add("CreativeTXTfile");
                 _sourceSelector.Items.Add("BoTXTfile");
               
            }
            catch (Exception ex)
            {
            }

            //accountSelectorUpdate(_clientSelector.SelectedValue);
            //_accountSelector.Visible = false;
        }

        void _sourceSelector_TextChanged(object sender, EventArgs e)
        {
            string g = "H";
        }

       

        void _removeFile_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = listboxFiles.SelectedItem.Text;
                //  listOfFilesToUpload.Add(fileUpload.PostedFile.FileName, path + newFileName);

                listboxFiles.Items.Remove(listboxFiles.SelectedItem);

                foreach (var item in listOfFilesToUpload)
                {
                    if (item.Key.Equals(fileName))
                    {
                        listOfFilesToUpload.Remove(item.Key);
                        break;
                    }
                }

                Session["listOfFilesToUpload"] = listOfFilesToUpload;
                //   fileUpload.PostedFile.SaveAs(path + newFileName);
                //   listboxFiles.Items.Add(newFileName);//fileUpload.PostedFile.FileName);

            }
            catch (Exception ex)
            { }
        }
        //private void accountSelectorUpdate(string clientSelected)
        //{
        //    DataTable tbl;
        //    SqlCommand cmd = DataManager.CreateCommand("getAccountList(@IsRankingAccount:Int, @clientId:Int)", CommandType.StoredProcedure);
        //    cmd.Parameters["@IsRankingAccount"].Value = 4; //Means only Children
        //    cmd.Parameters["@clientId"].Value = Int32.Parse(_clientSelector.SelectedValue);
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

        //    lock (DataManager.ConnectionString)
        //    {
        //        tbl = new DataTable();
        //        using (DataManager.Current.OpenConnection())
        //        {
        //            DataManager.Current.AssociateCommands(cmd);
        //            adapter.Fill(tbl);
        //        }
        //    }
        //    _accountSelector.DataSource = tbl;
        //    _accountSelector.DataTextField = "account_name";
        //    _accountSelector.DataValueField = "account_id";
        //    _accountSelector.DataBind();

        //    // Add account_id for each name.
        //    for (int i = 0; i < _accountSelector.Items.Count; ++i)
        //        _accountSelector.Items[i].Text += " (" + _accountSelector.Items[i].Value + ")";

        //    if (tbl.Rows.Count > 1)
        //    {
        //        _accountSelector.Visible = true;
        //    }
        //    else
        //        _accountSelector.Visible = false;
        //}

        //protected void _clientSelector_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    accountSelectorUpdate(_clientSelector.SelectedValue);
        //}
    }
    public class SaveFileClass
    {

        public Excel.Workbook wbk;
        public string newFileName;
        public void SaveFileToXLS()
        {
            // Excel.XlFileFormat.xlWorkbookNormal - working!!!
            //not copy ALL rows
            // wbk.SaveAs(newFileName, Excel.XlFileFormat.xlExcel7, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            wbk.SaveAs(newFileName, Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

        }

        public void Stop()
        {
            wbk = null;
        }





    }
}
