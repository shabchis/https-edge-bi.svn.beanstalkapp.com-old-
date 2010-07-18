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
	partial class SingleCampaign
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
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference2 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Collections.Hashtable hashtable1 = new System.Collections.Hashtable();
            System.Collections.Hashtable hashtable2 = new System.Collections.Hashtable();
            this.sendMail1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.generateReport2 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.sendBiggerMail = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.generateReport1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.smaller = new System.Workflow.Activities.IfElseBranchActivity();
            this.bigger_equal = new System.Workflow.Activities.IfElseBranchActivity();
            this.checkChange = new System.Workflow.Activities.IfElseActivity();
            this.checkCampaignClicks = new Easynet.Edge.Services.Alerts.AlertCustomActivities.Campaign();
            // 
            // sendMail1
            // 
            this.sendMail1.MessageBody = null;
            this.sendMail1.Name = "sendMail1";
            // 
            // generateReport2
            // 
            this.generateReport2.Name = "generateReport2";
            // 
            // sendBiggerMail
            // 
            this.sendBiggerMail.MessageBody = "The calculated difference [DIFF] is bigger than the allowed sync [ALLOWED]";
            this.sendBiggerMail.Name = "sendBiggerMail";
            // 
            // generateReport1
            // 
            this.generateReport1.Name = "generateReport1";
            // 
            // smaller
            // 
            this.smaller.Activities.Add(this.generateReport2);
            this.smaller.Activities.Add(this.sendMail1);
            ruleconditionreference1.ConditionName = "smaller_than";
            this.smaller.Condition = ruleconditionreference1;
            this.smaller.Name = "smaller";
            // 
            // bigger_equal
            // 
            this.bigger_equal.Activities.Add(this.generateReport1);
            this.bigger_equal.Activities.Add(this.sendBiggerMail);
            ruleconditionreference2.ConditionName = "bigger_than";
            this.bigger_equal.Condition = ruleconditionreference2;
            this.bigger_equal.Name = "bigger_equal";
            // 
            // checkChange
            // 
            this.checkChange.Activities.Add(this.bigger_equal);
            this.checkChange.Activities.Add(this.smaller);
            this.checkChange.Name = "checkChange";
            // 
            // checkCampaignClicks
            // 
            this.checkCampaignClicks.Clicks = 0;
            this.checkCampaignClicks.ClicksDelta = 0F;
            this.checkCampaignClicks.Name = "checkCampaignClicks";
            // 
            // SingleCampaign
            // 
            this.Activities.Add(this.checkCampaignClicks);
            this.Activities.Add(this.checkChange);
            this.ConditionValues = hashtable1;
            this.ConnectionString = "";
            this.Name = "SingleCampaign";
            this.Parameters = hashtable2;
            this.Template = false;
            this.WorkflowGUID = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.WorkflowID = -1;
            this.WorkflowName = "";
            this.WorkflowType = typeof(Easynet.Edge.Core.Workflow.BaseSequentialWorkflow);
            this.CanModifyActivities = false;

		}

		#endregion

        private IfElseBranchActivity smaller;
        private IfElseBranchActivity bigger_equal;
        private IfElseActivity checkChange;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendBiggerMail;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateReport1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateReport2;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.Campaign checkCampaignClicks;











    }
}
