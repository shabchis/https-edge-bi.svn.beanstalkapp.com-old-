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
	partial class AdminAlerts2
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
            this.generateReport2 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateReport1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.sendMail1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.sendMessage1 = new Easynet.Edge.Core.Workflow.SendMessage();
            this.masterReport1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.MasterReport();
            this.ifElseActivity1 = new System.Workflow.Activities.IfElseActivity();
            this.account1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.Account();
            // 
            // generateReport2
            // 
            this.generateReport2.Name = "generateReport2";
            this.generateReport2.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Account;
            this.generateReport2.ReportHeading = "Falling";
            this.generateReport2.ReportHeadings = "Account,";
            this.generateReport2.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateReport1
            // 
            this.generateReport1.Name = "generateReport1";
            this.generateReport1.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Account;
            this.generateReport1.ReportHeading = "Rising";
            this.generateReport1.ReportHeadings = "Account,";
            this.generateReport1.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.generateReport2);
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.generateReport1);
            ruleconditionreference1.ConditionName = "check_clicks";
            this.ifElseBranchActivity1.Condition = ruleconditionreference1;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // sendMail1
            // 
            this.sendMail1.Enabled = false;
            this.sendMail1.MessageBody = null;
            this.sendMail1.Name = "sendMail1";
            this.sendMail1.Override = false;
            this.sendMail1.Recipient = null;
            // 
            // sendMessage1
            // 
            this.sendMessage1.Attachments = null;
            this.sendMessage1.Name = "sendMessage1";
            this.sendMessage1.Recipients = null;
            this.sendMessage1.Text = null;
            this.sendMessage1.Title = null;
            this.sendMessage1.Urgency = Easynet.Edge.Core.Messaging.MessageUrgency.Low;
            // 
            // masterReport1
            // 
            this.masterReport1.HasReport = false;
            this.masterReport1.Name = "masterReport1";
            // 
            // ifElseActivity1
            // 
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity1);
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity2);
            this.ifElseActivity1.Name = "ifElseActivity1";
            // 
            // account1
            // 
            this.account1.Name = "account1";
            // 
            // AdminAlerts2
            // 
            this.Activities.Add(this.account1);
            this.Activities.Add(this.ifElseActivity1);
            this.Activities.Add(this.masterReport1);
            this.Activities.Add(this.sendMessage1);
            this.Activities.Add(this.sendMail1);
            this.ConditionValues = hashtable1;
            this.ConnectionString = "";
            this.Name = "AdminAlerts2";
            this.Parameters = hashtable2;
            this.Template = false;
            this.WorkflowGUID = new System.Guid("7ae59c40-b584-4ff3-8f4f-871fea384180");
            this.WorkflowID = -1;
            this.WorkflowName = "";
            this.WorkflowType = typeof(Easynet.Edge.Core.Workflow.BaseSequentialWorkflow);
            this.CanModifyActivities = false;

		}

		#endregion

        private IfElseBranchActivity ifElseBranchActivity2;
        private IfElseBranchActivity ifElseBranchActivity1;
        private IfElseActivity ifElseActivity1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateReport1;
        private Easynet.Edge.Core.Workflow.SendMessage sendMessage1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.MasterReport masterReport1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateReport2;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.Account account1;













    }
}
