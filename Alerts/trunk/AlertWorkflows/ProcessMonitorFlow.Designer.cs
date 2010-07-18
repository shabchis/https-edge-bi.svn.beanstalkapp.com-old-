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
	partial class ProcessMonitorFlow
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
            this.sendMail1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.NotAlive = new System.Workflow.Activities.IfElseBranchActivity();
            this.checkIfAlive = new System.Workflow.Activities.IfElseActivity();
            this.keepAlive1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.KeepAlive();
            // 
            // sendMail1
            // 
            this.sendMail1.MessageBody = null;
            this.sendMail1.Name = "sendMail1";
            this.sendMail1.Override = true;
            this.sendMail1.Recipient = "rnd@easynet.co.il";
            // 
            // NotAlive
            // 
            this.NotAlive.Activities.Add(this.sendMail1);
            ruleconditionreference1.ConditionName = "notAlive";
            this.NotAlive.Condition = ruleconditionreference1;
            this.NotAlive.Name = "NotAlive";
            // 
            // checkIfAlive
            // 
            this.checkIfAlive.Activities.Add(this.NotAlive);
            this.checkIfAlive.Name = "checkIfAlive";
            // 
            // keepAlive1
            // 
            this.keepAlive1.Alive = false;
            this.keepAlive1.Name = "keepAlive1";
            // 
            // ProcessMonitorFlow
            // 
            this.Activities.Add(this.keepAlive1);
            this.Activities.Add(this.checkIfAlive);
            this.ConditionValues = hashtable1;
            this.ConnectionString = "";
            this.Name = "ProcessMonitorFlow";
            this.Parameters = hashtable2;
            this.Template = false;
            this.WorkflowGUID = new System.Guid("a393418e-65b8-4024-b4f0-dd09cdb7684f");
            this.WorkflowID = -1;
            this.WorkflowName = "";
            this.WorkflowType = typeof(Easynet.Edge.Core.Workflow.BaseSequentialWorkflow);
            this.CanModifyActivities = false;

		}

		#endregion

        private IfElseBranchActivity NotAlive;
        private IfElseActivity checkIfAlive;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.KeepAlive keepAlive1;



    }
}
