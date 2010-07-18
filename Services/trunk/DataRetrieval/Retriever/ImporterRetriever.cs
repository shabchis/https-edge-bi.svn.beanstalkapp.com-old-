using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.Services.DataRetrieval.Retriever
{
	/// <summary>
	/// This service activate by file system watcher when a new csv file have 
	/// been upload to the the relevent directory.
	/// The service move the file to csv directory and add his new path
	/// to the source DB.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>1/08/2009</creation_date>
	class ImporterRetriever :BaseRetriever
	{

		#region Consts
		/*=========================*/

		private const string PpcImportServiceType = "PpcImport";

		/*=========================*/
		#endregion

		#region Constructor
		/*=========================*/

		/// <summary>
		/// Initalize the parent fields with const values of this class.
		/// </summary>
		public ImporterRetriever()
		{
			serviceType = PpcImportServiceType;
		}

		/*=========================*/
		#endregion   

		protected override string WriteResultToFile()
		{
			string fileName;

			// Get path of the directory where the csv file have been upload.
			string path = GetConfigurationOptionsField("SourceFilePath");

			if (string.IsNullOrEmpty(path))
				throw new Exception("No source file path was specified for importer.");			

			string targetDirectory = GetConfigurationOptionsField("TargetDirectory");

			// Move the file from csv input direcoty to archive directory.
			if (!string.IsNullOrEmpty(targetDirectory))
				fileName = WriteResultToFile(path, DateTime.Today, targetDirectory);
			else
				fileName = WriteResultToFile(path, DateTime.Today);

			_requiredDay = DateTime.Today;

			return fileName;
		}


		protected override ServiceOutcome DoWork()
		{
			try
			{
				InitalizeAccountID();
				SaveReport();
			}
			catch (Exception ex)
			{
				Log.Write(string.Empty, ex);
				return ServiceOutcome.Failure;
			}

			return ServiceOutcome.Success;
		}
	}
}
