using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Services.DataRetrieval.Retriever;

namespace Easynet.Edge.Services.Importer
{
	/// <summary>
	/// This service activate by file system watcher when a new csv file have 
	/// been upload to the the relevent directory.
	/// The service move the file to csv directory and add his new path
	/// to the source DB.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>22/08/2008</creation_date>
	
	public class PpcImportRetrieverService : RetrieverService
	{
		#region Consts
		/*=========================*/
		private const string PpcImportServiceType = "PpcImport";
		/*=========================*/
		#endregion

		#region Service Override Methods
		/*=========================*/

		/// <summary>
		/// Main entry point of the service.
		/// </summary>
		/// <returns></returns>
		protected override ServiceOutcome DoWork() 	
		{
			// Get path of the directory where the csv file have been upload.
			string path = (Instance.ParentInstance != null ? Instance.ParentInstance.Configuration.Options : Instance.Configuration.Options)
				["SourceFilePath"];

			if (path == null)
			{
				Log.Write("No source file path was specified.", null);
				return ServiceOutcome.Failure;
			}

			string fileName;

			// Move the file from csv input direcoty to archive directory.
			if (Instance.Configuration.Options["TargetDirectory"] != null)
			{
				fileName = WriteResultToFile(path, DateTime.Today, Instance.Configuration.Options["TargetDirectory"].ToString());
			}
			else
			{
				fileName = WriteResultToFile(path, DateTime.Today);
			}

			if (fileName != string.Empty)
			{
				// Save the file path in the DB
				SaveFilePathToDB(PpcImportServiceType, fileName, DateTime.Today, true);
			}
			else
			{
				throw new Exception("FileName is empty.");
			}

			return ServiceOutcome.Success;
		}

		/*=========================*/
		#endregion
	}
}
