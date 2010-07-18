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
using System.Configuration;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Services.DataRetrieval.Configuration;
using System.Collections.Generic;


using Easynet.Edge.Alerts.Core;
using OfficeOpenXml;
using System.IO;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class AdwordsCSVFile: BaseAlertActivity
	{
		public AdwordsCSVFile()
		{
			InitializeComponent();
		}


        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //ParentWorkflow.Parameters.Add("SourceFilePath", @"c:\temp\alerts\admin\report062009Monthly.csv");

            //Parse the CSV file, and create the data structure for it.
            if (!ParentWorkflow.Parameters.ContainsKey("SourceFilePath"))
                throw new Exception("Could not find the path of the CSV file.");

            string fileName = ParentWorkflow.Parameters["SourceFilePath"].ToString();
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Could not find the CSV file at: " + fileName);

            //creating list of accounts names to be reunite


            StreamReader sr = File.OpenText(fileName);
            string line = String.Empty;
            int count = 1;
            bool month = false;
            //Loop on the file, and build a hash-table per account. We assume that each
            //account only appears ONCE!.
            Hashtable ht = new Hashtable();
            Dictionary<string, string> aggregatedAccountsProperties = new Dictionary<string, string>();

            //getting aggregation information from configuration
            FieldElementSection fes = (FieldElementSection)ConfigurationManager.GetSection("AggregatedAccountsProperties");
            bool isAccntExist = false;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (count == AccountAllMeasures.DATE_RANGE_ROW)
                {
                    //Based on what's written in the line, see if we're a daily report
                    //or a monthly report.
                    if (line.ToLower().Contains("last month"))
                    {
                        month = true;
                    }

                    ParentWorkflow.InternalParameters.Add("Monthly", month);
                }

                if (count >= AccountAllMeasures.ADWORDS_CSV_START_ROW)
                {
                    //We have reached the actual data... start parsing.
                    AccountAllMeasures aam = new AccountAllMeasures(line);
                    //Do not add empty accounts.
                    if (aam.AccountName == String.Empty)
                        continue;

                    if (!ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
                        ParentWorkflow.InternalParameters.Add("ReportDate", aam.CompareDate);

                    if (aam.AccountName != String.Empty)
                    {
                        isAccntExist = false;
                        //object section = ConfigurationManager.GetSection("AggregatedAccountsProperties");
                        //if configuration hasn't been already read  
                        if (aggregatedAccountsProperties.Count == 0)
                        {
                            // Initalize the dictionary with the properties of the accounts.
                            foreach (FieldElement fe in fes.Fields)
                            {
                                string[] parsed = fe.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                foreach (string st in parsed)
                                {
                                    aggregatedAccountsProperties.Add(st, fe.Key);
                                }
                            }
                        }
                        if (aggregatedAccountsProperties.ContainsKey(aam.AccountName))
                            sumAccounts(aam, aggregatedAccountsProperties[aam.AccountName], ref isAccntExist);
                        else
                            sumAccounts(aam, aam.AccountName, ref isAccntExist);


                        if (!isAccntExist)
                        {
                            //if destination account is not in _results we'll add the account that should be aggregated with destination name
                            if(aggregatedAccountsProperties.ContainsKey(aam.AccountName))
                                aam.AccountName = aggregatedAccountsProperties[aam.AccountName];
                            _results.Add(aam);
                            ht.Add(aam.AccountName, aam);
                        }
                    }
                }
                count++;
            }

            sr.Close();
            sr.Dispose();

            if (!ParentWorkflow.InternalParameters.ContainsKey("CSVResults"))
                ParentWorkflow.InternalParameters.Add("CSVResults", ht);

            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not delete file: " + fileName + ". Exception: " + ex.ToString());
                throw ex;
            }

            return ActivityExecutionStatus.Closed;
        }
        private void sumAccounts(AccountAllMeasures aam, string destAccountName, ref bool isAccntExist)
        {
            foreach (AccountAllMeasures ob in _results)
            {
                if (ob.AccountName.Equals(destAccountName))
                {
                    //temporary until we fix it in db for any account
                    //if (aam.AccountName.Length < ob.AccountName.Length)
                    //    ob.AccountName = aam.AccountName;

                    ob.BONewActivationsCompare += aam.BONewActivationsCompare;
                    ob.BONewActivationsCurrent += aam.BONewActivationsCurrent;
                    ob.BONewUsersCompare += aam.BONewUsersCompare;
                    ob.BONewUsersCurrent += aam.BONewUsersCurrent;
                    ob.ClicksChangeRatio += aam.ClicksChangeRatio;
                    ob.ClicksCompare += aam.ClicksCompare;
                    ob.ClicksCurrent += aam.ClicksCurrent;
                    ob.ConversionRateCompare += aam.ConversionRateCompare;
                    ob.ConversionRateCurrent += aam.ConversionRateCurrent;
                    ob.ConversionsCompare += aam.ConversionsCompare;
                    ob.ConversionsCurrent += aam.ConversionsCurrent;
                    ob.CostCompare += aam.CostCompare;
                    ob.CostCurrent += aam.CostCurrent;
                    ob.CostPerBONewUsersCompare += aam.CostPerBONewUsersCompare;
                    ob.CostPerBONewUsersCurrent += aam.CostPerBONewUsersCurrent;
                    ob.CostPerConversionsCompare += aam.CostPerConversionsCompare;
                    ob.CostPerConversionsCurrent += aam.CostPerConversionsCurrent;
                    ob.CPCCompare += aam.CPCCompare;
                    ob.CPCCurrent += aam.CPCCurrent;
                    ob.CTRCompare += aam.CTRCompare;
                    ob.CTRCurrent += aam.CTRCurrent;
                    ob.ImpsCompare += aam.ImpsCompare;
                    ob.ImpsCurrent += aam.ImpsCurrent;
                    ob.LeadsCompare += aam.LeadsCompare;
                    ob.LeadsCurrent += aam.LeadsCurrent;
                    ob.PurcasesCompare += aam.PurcasesCompare;
                    ob.PurchasesCurrent += aam.PurchasesCurrent;
                    ob.SignupsCompare += aam.SignupsCompare;
                    ob.SignupsCurrent += aam.SignupsCurrent;
                    isAccntExist = true;
                    return;
                }
            }
        }
    }
}
