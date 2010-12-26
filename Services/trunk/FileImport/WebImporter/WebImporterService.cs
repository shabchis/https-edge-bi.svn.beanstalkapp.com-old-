using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.IO;
using Easynet.Edge.Core.Data;

namespace Easynet.Edge.Services.FileImport
{
	/// <summary>
	/// Service that contains the original ASP.NET converter code written by Noam Fein / Liran Amir.
	/// To be replaced ASAP with proper code (via data pipeline).
	/// </summary>
	public class WebImporterService: Service
	{
		protected override ServiceOutcome DoWork()
		{
			// Get configuration options
			string sourceType = Instance.Configuration.Options["SourceType"];
			string importFilesVal = Instance.Configuration.Options["ImportFiles"];
			string[] importFiles = importFilesVal == null ? null : importFilesVal.Split(',');
			if (importFilesVal == null || importFiles.Length < 1)
				throw new ConfigurationException("ImportFiles list is empty.");

			if (sourceType.StartsWith("DirectUpload"))
			{
				// Special case for direct drop into destionation folder
				string directUploadPath = Instance.Configuration.Options[sourceType + ".Path"];
				if (directUploadPath == null)
					throw new ConfigurationException(sourceType + ".Path is not configured on the service.");

				foreach (string file in importFiles)
				{
					string saveFileName = String.Format("{0:00000} {1:yyyyMMdd}@{1:hhmm} {2}",
						Instance.AccountID,
						DateTime.Now,
						Path.GetFileName(file)
						);

					File.Copy(file, Path.Combine(directUploadPath, saveFileName), true);
				}
			}
			else
			{
				// Legacy Noam/Liran converter crap
				string saveFileName = String.Format("{0:00000} {1:yyyyMMdd}@{1:hhmm} {2}.txt",
					Instance.AccountID,
					DateTime.Now,
					Path.GetFileNameWithoutExtension(importFiles[0])
					);
				Converters.BOConvertorManager manager = new Converters.BOConvertorManager();
				manager.DoWork(Instance.AccountID.ToString(), new List<string>(importFiles), saveFileName, sourceType);
			}
			
			// We can't really tell if it was success or not, but we try
			return ServiceOutcome.Success;
		}
	}
}
