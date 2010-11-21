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
using System.Xml;

namespace Easynet.Edge.UI.WebPages
{
    public partial class WebImporterPage : PageBase
    {
        protected string ErrorMessage = null;
        protected string SuccessMessage = null;

		public static string UploadRoot = AppSettings.Get(typeof(WebImporter), "UploadRoot");
		Dictionary<string, string> UploadedFileNames
		{
			get
			{
				Dictionary<string, string> list = (Dictionary<string, string>)Session["UploadedFileNames"];
				if (list == null)
					Session["UploadedFileNames"] = list = new Dictionary<string, string>();
				return list;
			}
		}
		
		void _addFile_Click(object sender, EventArgs e)
        {
			// empty filename
            if (String.IsNullOrEmpty(_fileUpload.PostedFile.FileName))
				return;

			// Ignore this file if it already exists in listbox
			if (_listboxFiles.Items.FindByText(_fileUpload.PostedFile.FileName) != null)
				return;

            //if 'BO' or 'Bing' and one file already selected
			if
			(
				(
					_sourceSelector.SelectedValue.Equals("BO") ||
					_sourceSelector.SelectedValue.Equals("Bing")
				)
                && _listboxFiles.Items.Count > 0
			)
            {
                //replace the current file
				_listboxFiles.Items.Clear();
				UploadedFileNames.Clear();
            }

			// Get (and create if it doesn't exist) a folder for this session
			string userUploadRoot = Path.Combine(UploadRoot, Session.SessionID);
			try
			{
				if (!Directory.Exists(userUploadRoot))
					Directory.CreateDirectory(userUploadRoot);
			}
			catch (Exception ex)
			{
				ErrorMessage = "Could not create an upload directory for this session.";
				Log.Write("WebImporter", ErrorMessage, ex);
				return;
			}

			string uploadPath = Path.Combine(userUploadRoot, _fileUpload.FileName);
			
			// some hack - change non-yahoo tsv to csv
			if (Path.GetExtension(uploadPath).ToLower().Equals(".tsv") && !_sourceSelector.SelectedValue.ToLower().Equals("yahoo"))
				savePath = Path.ChangeExtension(uploadPath, "csv");

			try
			{
				_fileUpload.PostedFile.SaveAs(uploadPath);
			}
			catch (Exception ex)
			{
				ErrorMessage = "Failed to upload file: " + ex.Message;
				Log.Write("WebImporter", String.Format("Failed to upload file to {0}", uploadPath), ex);
				return;
			}

			// Add it to the list for the user
			_listboxFiles.Items.Add(_fileUpload.PostedFile.FileName);

			// keep a name conversion reference for later
			UploadedFileNames[_fileUpload.PostedFile.FileName] = uploadPath;

			// TODO:
			/*
			int count = _listboxFiles.Items.Count;
			if (count > 0)
			{
				string outputFileName = originalFileName;
				string name = Path.GetFileName(outputFileName);
				string exten = Path.GetExtension(name);
				//saveFilePathTextBox.Text = name.Remove(name.Length - exten.Length, exten.Length);
				// DORON: Converted saveFilePathTextBox to a HiddenField
				saveFilePathTextBox.Value = Request.QueryString["accountID"] + "_" + name.Remove(name.Length - exten.Length, exten.Length) + DayCode.ToDayCode(DateTime.Today) + "_" + DateTime.Now.TimeOfDay.Hours + "_" + DateTime.Now.TimeOfDay.Minutes + "_" + DateTime.Now.TimeOfDay.Seconds;
			}
			*/

			// enable submit after we ensure one file is up
            _submit.Enabled = true;
        }

		void _removeFile_Click(object sender, EventArgs e)
        {
			string userUploadRoot = Path.Combine(UploadRoot, Session.SessionID);
            string fileName = _listboxFiles.SelectedItem.Text;
            
			// Remove from list
			_listboxFiles.Items.Remove(_listboxFiles.SelectedItem);
			
			// Delete file
			if (UploadedFileNames.ContainsKey(fileName))
			{
				string uploaded = UploadedFileNames[fileName];
				UploadedFileNames.Remove(uploaded);
				try
				{
					if (File.Exists(uploaded))
						File.Delete(uploaded);
				}
				catch (Exception ex)
				{
					Log.Write(
						"WebImporter",
						String.Format("User tried to remove an uploaded file from the list, but it could not be deleted: {0}", uploaded),
						ex,
						LogMessageType.Warning);
				}
			}
				
        }

		void _submit_Click(object sender, EventArgs e)
        {
			if (_listboxFiles.Items.Count == 0)
			{
				ErrorMessage = "Please upload at least one file before submitting.";
				return;
			}

			// Build a list of files
			string delimetedList = string.Empty;
			foreach (string file in UploadedFileNames.Values)
			{
				delimetedList += file;
				if (file < UploadedFileNames.Count-1)
					delimetedList += ',';
			}

			//SourceType = manager.DoWork(accountID, listOfFiles, saveFilePathTextBox.Value, SourceType);
			try
			{
				using (ServiceClient<IScheduleManager> scheduleManager = new ServiceClient<IScheduleManager>())
				{
					scheduleManager.Service.AddToSchedule("WebImporter", this.AccountID, DateTime.Now, new SettingsCollection()
					{
						{ "SourceType", _sourceSelector.SelectedValue},
						{ "ImportFiles", delimetedList }
					});
				}
			}
			catch(Exception ex)
			{
				ErrorMessage = "Failed to import: " + ex.Message;
				Log.Write("WebImporter", "Failed to trigger WebImporter from ScheduleManager.", ex, LogMessageType.Error);
				return;
			}

			SuccessMessage = SourceType;
			_listboxFiles.Items.Clear();            
        }

        void _sourceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
			_listboxFiles.Items.Clear();
			UploadedFileNames.Clear();
		}

		internal static void Cleanup(System.Web.SessionState.HttpSessionState Session)
		{
			try
			{
				string path = Path.Combine(UploadRoot, Session.SessionID);
				if (Directory.Exists(path))
					Directory.Delete(path, false);
			}
			catch
			{
				Log.Write("WebImporter", String.Format("Failed to cleanup after user session {0}.", Session.SessionID), ex, LogMessageType.Warning);
			}
	}
}
