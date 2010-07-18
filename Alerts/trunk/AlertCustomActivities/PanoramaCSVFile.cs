using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.IO;

using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class PanoramaCSVFile: BaseAlertActivity
	{
		public PanoramaCSVFile()
		{
			InitializeComponent();
		}


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //First see if we have a panorama file. We assume the file is in the Panorama folder.
            //..\Panorama (D:\Edge\Alerts\Panorama)
            string sourcePath = String.Empty;
            if (ParentWorkflow.Parameters.ContainsKey("SourceFilePath"))
                sourcePath = ParentWorkflow.Parameters["SourceFilePath"].ToString();

            string path = Path.GetDirectoryName(sourcePath);
            string parent = Directory.GetParent(path).FullName;
            if (!parent.EndsWith(@"\"))
                parent += @"\";

            parent += @"Panorama\";
            if (!ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                throw new Exception("No report date found. Cannot get panorama details.");

            DateTime reportDate = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);
            string fileName = reportDate.ToString("d_M_yy") + ".txt";
            parent += fileName;

            //Check if we have this file. If we don't - exit.
            if (!File.Exists(parent))
            {
                parent = parent.Replace(".txt", ".csv");
                if (!File.Exists(parent))
                    return ActivityExecutionStatus.Closed;
            }

            //Load the file and parse it.
            StreamReader sr = File.OpenText(parent);
            string line = String.Empty;
            int count = 0;

            Hashtable ht = new Hashtable();
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (count >= 1)
                {
                    try
                    {
                        //Actually do the parsing.
                        AccountAllMeasures aam = new AccountAllMeasures(line, true);

                        //Do not add empty accounts.
                        if (aam.AccountName == String.Empty)
                            continue;

                        _results.Add(aam);
                        ht.Add(aam.AccountName, aam);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                count++;                
            }

            sr.Close();
            sr.Dispose();

            if (!ParentWorkflow.InternalParameters.ContainsKey("PanoramaCSVResults"))
                ParentWorkflow.InternalParameters.Add("PanoramaCSVResults", ht);

            try
            {
                File.Delete(parent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not delete file: " + fileName + ". Exception: " + ex.ToString());
                throw ex;
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
