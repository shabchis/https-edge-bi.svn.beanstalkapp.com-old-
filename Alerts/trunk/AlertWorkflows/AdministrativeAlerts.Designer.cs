using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace Easynet.Edge.Services.Alerts.AlertWorkflows
{
	partial class AdministrativeAlerts
	{
		#region Designer generated code
		
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
		private void InitializeComponent()
		{
            this.CanModifyActivities = true;
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference1 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Collections.Hashtable hashtable1 = new System.Collections.Hashtable();
            System.Collections.Hashtable hashtable2 = new System.Collections.Hashtable();
            this.sendMail2 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.sendMail1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.checkForDiff = new System.Workflow.Activities.IfElseActivity();
            this.administrativeReport1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AdministrativeReport();
            this.loadBOData1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.LoadBOData();
            this.loadOLTPData1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.LoadOLTPData();
            this.panoramaCSVFile1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.PanoramaCSVFile();
            this.adwordsCSVFile1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AdwordsCSVFile();
            // 
            // sendMail2
            // 
            this.sendMail2.MessageBody = null;
            this.sendMail2.Name = "sendMail2";
            this.sendMail2.Override = false;
            this.sendMail2.Recipient = null;
            // 
            // sendMail1
            // 
            this.sendMail1.MessageBody = null;
            this.sendMail1.Name = "sendMail1";
            this.sendMail1.Override = false;
            this.sendMail1.Recipient = null;
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.sendMail2);
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.sendMail1);
            ruleconditionreference1.ConditionName = "send_to_all";
            this.ifElseBranchActivity1.Condition = ruleconditionreference1;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // checkForDiff
            // 
            this.checkForDiff.Activities.Add(this.ifElseBranchActivity1);
            this.checkForDiff.Activities.Add(this.ifElseBranchActivity2);
            this.checkForDiff.Name = "checkForDiff";
            // 
            // administrativeReport1
            // 
            this.administrativeReport1.DifferenceFound = false;
            this.administrativeReport1.Name = "administrativeReport1";
            // 
            // loadBOData1
            // 
            this.loadBOData1.Name = "loadBOData1";
            // 
            // loadOLTPData1
            // 
            this.loadOLTPData1.Name = "loadOLTPData1";
            // 
            // panoramaCSVFile1
            // 
            this.panoramaCSVFile1.Name = "panoramaCSVFile1";
            // 
            // adwordsCSVFile1
            // 
            this.adwordsCSVFile1.Name = "adwordsCSVFile1";
            // 
            // AdministrativeAlerts
            // 
            this.Activities.Add(this.adwordsCSVFile1);
            this.Activities.Add(this.panoramaCSVFile1);
            this.Activities.Add(this.loadOLTPData1);
            this.Activities.Add(this.loadBOData1);
            this.Activities.Add(this.administrativeReport1);
            this.Activities.Add(this.checkForDiff);
            this.ConditionValues = hashtable1;
            this.ConnectionString = "";
            this.Name = "AdministrativeAlerts";
            this.Parameters = hashtable2;
            this.Template = false;
            this.WorkflowGUID = new System.Guid("af79004d-1636-4a55-b77c-da9f2dd620fa");
            this.WorkflowID = -1;
            this.WorkflowName = "";
            this.WorkflowType = typeof(Easynet.Edge.Core.Workflow.BaseSequentialWorkflow);
            this.CanModifyActivities = false;

		}

		#endregion

        private IfElseBranchActivity ifElseBranchActivity2;
        private IfElseBranchActivity ifElseBranchActivity1;
        private IfElseActivity checkForDiff;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.LoadOLTPData loadOLTPData1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail2;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AdministrativeReport administrativeReport1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.PanoramaCSVFile panoramaCSVFile1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.LoadBOData loadBOData1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AdwordsCSVFile adwordsCSVFile1;





    }
}
