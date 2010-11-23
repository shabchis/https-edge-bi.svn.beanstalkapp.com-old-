using System;
using System.Collections.Generic;
using System.IO;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.UI.WebPages
{
    public partial class WebImporterPage : PageBase
    {
		static string UploadRoot = AppSettings.Get(typeof(WebImporterPage), "UploadRoot");
		static bool CleanupSession = bool.Parse(AppSettings.Get(typeof(WebImporterPage), "CleanupSession"));
		static bool ThresholdEnabled = bool.Parse(AppSettings.Get(typeof(WebImporterPage), "AvailabilityThreshold.Enabled"));
		static TimeSpan ThresholdUploadTime = TimeSpan.Parse(AppSettings.Get(typeof(WebImporterPage), "AvailabilityThreshold.UploadTime"));
		static string ThresholdMessageDefault = AppSettings.Get(typeof(WebImporterPage), "AvailabilityThreshold.MessageDefault");
		static string ThresholdMessageBefore = AppSettings.Get(typeof(WebImporterPage), "AvailabilityThreshold.MessageBefore");
		static string ThresholdMessageAfter = AppSettings.Get(typeof(WebImporterPage), "AvailabilityThreshold.MessageAfter");

		protected string ErrorMessage = null;
        protected string SuccessMessage = null;

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
				Log.Write(this.GetType().Name, ErrorMessage, ex);
				return;
			}

			string uploadPath = Path.Combine(userUploadRoot, _fileUpload.FileName);
			
			// some hack - change non-yahoo tsv to csv
			if (Path.GetExtension(uploadPath).ToLower().Equals(".tsv") && !_sourceSelector.SelectedValue.ToLower().Equals("yahoo"))
				uploadPath = Path.ChangeExtension(uploadPath, "csv");

			try
			{
				_fileUpload.PostedFile.SaveAs(uploadPath);
			}
			catch (Exception ex)
			{
				ErrorMessage = "Failed to upload file: " + ex.Message;
				Log.Write(this.GetType().Name, String.Format("Failed to upload file to {0}", uploadPath), ex);
				return;
			}

			// Add it to the list for the user
			_listboxFiles.Items.Add(_fileUpload.PostedFile.FileName);

			// keep a name conversion reference for later
			UploadedFileNames[_fileUpload.PostedFile.FileName] = uploadPath;

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
					Log.Write(this.GetType().Name,
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
			int count = 0;
			foreach (string file in UploadedFileNames.Values)
			{
				delimetedList += file;
				if (count < UploadedFileNames.Count - 1)
					delimetedList += ',';
				count++;
			}

			string sourceType = _sourceSelector.SelectedValue;
			string serviceName = sourceType.StartsWith("DirectUpload") ?
				"DirectWebImporter" :
				"LegacyWebImporter";

			try
			{
				using (ServiceClient<IScheduleManager> scheduleManager = new ServiceClient<IScheduleManager>())
				{

					scheduleManager.Service.AddToSchedule(serviceName, this.AccountID, DateTime.Now, new SettingsCollection()
					{
						{ "SourceType", sourceType},
						{ "ImportFiles", delimetedList }
					});
				}
			}
			catch(Exception ex)
			{
				ErrorMessage = "Failed to import: " + ex.Message;
				Log.Write(this.GetType().Name, "Failed to add " + serviceName + " to the schedule manager.", ex, LogMessageType.Error);
				return;
			}

			if (ThresholdEnabled)
			{
				if ((DateTime.Now - DateTime.Today) < ThresholdUploadTime)
					SuccessMessage = ThresholdMessageBefore;
				else
					SuccessMessage = ThresholdMessageAfter;
			}
			else
			{
				SuccessMessage = ThresholdMessageDefault;
			}

			_listboxFiles.Items.Clear();            
        }

		internal static void Cleanup(System.Web.SessionState.HttpSessionState Session)
		{
			if (!CleanupSession)
				return;

			try
			{
				string path = Path.Combine(UploadRoot, Session.SessionID);
				if (Directory.Exists(path))
					Directory.Delete(path, false);
			}
			catch(Exception ex)
			{
				Log.Write(typeof(WebImporterPage).Name, String.Format("Failed to cleanup after user session {0}.", Session.SessionID), ex, LogMessageType.Warning);
			}
		}
	}
}
