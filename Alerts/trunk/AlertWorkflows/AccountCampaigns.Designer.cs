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
	partial class AccountCampaigns
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
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference3 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference4 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference5 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference6 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference7 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference8 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference9 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference10 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference11 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Collections.Hashtable hashtable1 = new System.Collections.Hashtable();
            System.Collections.Hashtable hashtable2 = new System.Collections.Hashtable();
            this.sendMail1 = new Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail();
            this.generateAdgATFallingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateAdgATRisingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateGWFallingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateGWRisingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateKwFallingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateKwRisingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateAdgFallingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateAdgroupRisingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateCampaignFallingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.generateCampaignRisingReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport();
            this.somethingToReport = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifAdgATSmaller = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifAdgATBigger = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifAdgGWSmaller = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifAdgGWBigger = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifKwSmaller = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifKwBigger = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifAdgBigger = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifSmaller = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifBigger = new System.Workflow.Activities.IfElseBranchActivity();
            this.beforeSendMail = new System.Workflow.Activities.IfElseActivity();
            this.sendMessage1 = new Easynet.Edge.Core.Workflow.SendMessage();
            this.generateMasterReport = new Easynet.Edge.Services.Alerts.AlertCustomActivities.MasterReport();
            this.checkAdgATDeltaSmaller = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgATDeltaBigger = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgGWDeltaSmaller = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgGWDeltaBigger = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgKwDeltaSmaller = new System.Workflow.Activities.IfElseActivity();
            this.checkAdKwDeltaBigger = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgDeltaSmaller = new System.Workflow.Activities.IfElseActivity();
            this.checkAdgDeltaBigger = new System.Workflow.Activities.IfElseActivity();
            this.checkDeltaSmaller = new System.Workflow.Activities.IfElseActivity();
            this.checkDeltaBigger = new System.Workflow.Activities.IfElseActivity();
            this.adgroupAdtexts = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupAdtexts();
            this.adgroupGateways = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupGateways();
            this.adgroupKeywords = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupKeywords();
            this.campaignAdgroups = new Easynet.Edge.Services.Alerts.AlertCustomActivities.CampaignAdgroups();
            this.campaigns = new Easynet.Edge.Services.Alerts.AlertCustomActivities.AccountCampaigns();
            // 
            // sendMail1
            // 
            this.sendMail1.MessageBody = null;
            this.sendMail1.Name = "sendMail1";
            this.sendMail1.Override = false;
            this.sendMail1.Recipient = null;
            // 
            // generateAdgATFallingReport
            // 
            this.generateAdgATFallingReport.Name = "generateAdgATFallingReport";
            this.generateAdgATFallingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Adtext;
            this.generateAdgATFallingReport.ReportHeading = "Falling";
            this.generateAdgATFallingReport.ReportHeadings = "Campaign,Adgroup,AdText,AdText_Description";
            this.generateAdgATFallingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateAdgATRisingReport
            // 
            this.generateAdgATRisingReport.Name = "generateAdgATRisingReport";
            this.generateAdgATRisingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Adtext;
            this.generateAdgATRisingReport.ReportHeading = "Rising";
            this.generateAdgATRisingReport.ReportHeadings = "Campaign,Adgroup,AdText,AdText_Description";
            this.generateAdgATRisingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // generateGWFallingReport
            // 
            this.generateGWFallingReport.Name = "generateGWFallingReport";
            this.generateGWFallingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Gateway;
            this.generateGWFallingReport.ReportHeading = "Falling";
            this.generateGWFallingReport.ReportHeadings = "Campaign,Adgroup,Gateway,Gateway_ID";
            this.generateGWFallingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateGWRisingReport
            // 
            this.generateGWRisingReport.Name = "generateGWRisingReport";
            this.generateGWRisingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Gateway;
            this.generateGWRisingReport.ReportHeading = "Rising";
            this.generateGWRisingReport.ReportHeadings = "Campaign,Adgroup,Gateway,Gateway_ID";
            this.generateGWRisingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // generateKwFallingReport
            // 
            this.generateKwFallingReport.Name = "generateKwFallingReport";
            this.generateKwFallingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Keyword;
            this.generateKwFallingReport.ReportHeading = "Falling";
            this.generateKwFallingReport.ReportHeadings = "Campaign,Adgroup,Keyword";
            this.generateKwFallingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateKwRisingReport
            // 
            this.generateKwRisingReport.Name = "generateKwRisingReport";
            this.generateKwRisingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Keyword;
            this.generateKwRisingReport.ReportHeading = "Rising";
            this.generateKwRisingReport.ReportHeadings = "Campaign,Adgroup,Keyword";
            this.generateKwRisingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // generateAdgFallingReport
            // 
            this.generateAdgFallingReport.Name = "generateAdgFallingReport";
            this.generateAdgFallingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Adgroup;
            this.generateAdgFallingReport.ReportHeading = "Falling";
            this.generateAdgFallingReport.ReportHeadings = "Campaign,Adgroup";
            this.generateAdgFallingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateAdgroupRisingReport
            // 
            this.generateAdgroupRisingReport.Name = "generateAdgroupRisingReport";
            this.generateAdgroupRisingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Adgroup;
            this.generateAdgroupRisingReport.ReportHeading = "Rising";
            this.generateAdgroupRisingReport.ReportHeadings = "Campaign,Adgroup";
            this.generateAdgroupRisingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // generateCampaignFallingReport
            // 
            this.generateCampaignFallingReport.Name = "generateCampaignFallingReport";
            this.generateCampaignFallingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Campaign;
            this.generateCampaignFallingReport.ReportHeading = "Falling";
            this.generateCampaignFallingReport.ReportHeadings = "Campaign";
            this.generateCampaignFallingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Smaller;
            // 
            // generateCampaignRisingReport
            // 
            this.generateCampaignRisingReport.Name = "generateCampaignRisingReport";
            this.generateCampaignRisingReport.ReportEntityType = Easynet.Edge.Alerts.Core.EntityTypes.Campaign;
            this.generateCampaignRisingReport.ReportHeading = "Rising";
            this.generateCampaignRisingReport.ReportHeadings = "Campaign";
            this.generateCampaignRisingReport.ReportMeasureDiff = Easynet.Edge.Alerts.Core.MeasureDiff.Large;
            // 
            // somethingToReport
            // 
            this.somethingToReport.Activities.Add(this.sendMail1);
            ruleconditionreference1.ConditionName = "checkReport";
            this.somethingToReport.Condition = ruleconditionreference1;
            this.somethingToReport.Name = "somethingToReport";
            // 
            // ifAdgATSmaller
            // 
            this.ifAdgATSmaller.Activities.Add(this.generateAdgATFallingReport);
            ruleconditionreference2.ConditionName = "adgATSmaller";
            this.ifAdgATSmaller.Condition = ruleconditionreference2;
            this.ifAdgATSmaller.Name = "ifAdgATSmaller";
            // 
            // ifAdgATBigger
            // 
            this.ifAdgATBigger.Activities.Add(this.generateAdgATRisingReport);
            ruleconditionreference3.ConditionName = "adgATBigger";
            this.ifAdgATBigger.Condition = ruleconditionreference3;
            this.ifAdgATBigger.Name = "ifAdgATBigger";
            // 
            // ifAdgGWSmaller
            // 
            this.ifAdgGWSmaller.Activities.Add(this.generateGWFallingReport);
            ruleconditionreference4.ConditionName = "ifAdgGWSmaller";
            this.ifAdgGWSmaller.Condition = ruleconditionreference4;
            this.ifAdgGWSmaller.Name = "ifAdgGWSmaller";
            // 
            // ifAdgGWBigger
            // 
            this.ifAdgGWBigger.Activities.Add(this.generateGWRisingReport);
            ruleconditionreference5.ConditionName = "adgGWBigger";
            this.ifAdgGWBigger.Condition = ruleconditionreference5;
            this.ifAdgGWBigger.Name = "ifAdgGWBigger";
            // 
            // ifKwSmaller
            // 
            this.ifKwSmaller.Activities.Add(this.generateKwFallingReport);
            ruleconditionreference6.ConditionName = "checkKwSmaller";
            this.ifKwSmaller.Condition = ruleconditionreference6;
            this.ifKwSmaller.Name = "ifKwSmaller";
            // 
            // ifKwBigger
            // 
            this.ifKwBigger.Activities.Add(this.generateKwRisingReport);
            ruleconditionreference7.ConditionName = "kwBiggerCond";
            this.ifKwBigger.Condition = ruleconditionreference7;
            this.ifKwBigger.Name = "ifKwBigger";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.generateAdgFallingReport);
            ruleconditionreference8.ConditionName = "adgSmaller";
            this.ifElseBranchActivity1.Condition = ruleconditionreference8;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // ifAdgBigger
            // 
            this.ifAdgBigger.Activities.Add(this.generateAdgroupRisingReport);
            ruleconditionreference9.ConditionName = "adgDeltaBigger";
            this.ifAdgBigger.Condition = ruleconditionreference9;
            this.ifAdgBigger.Name = "ifAdgBigger";
            // 
            // ifSmaller
            // 
            this.ifSmaller.Activities.Add(this.generateCampaignFallingReport);
            ruleconditionreference10.ConditionName = "ifSmallerCond";
            this.ifSmaller.Condition = ruleconditionreference10;
            this.ifSmaller.Name = "ifSmaller";
            // 
            // ifBigger
            // 
            this.ifBigger.Activities.Add(this.generateCampaignRisingReport);
            ruleconditionreference11.ConditionName = "ifBiggerCond";
            this.ifBigger.Condition = ruleconditionreference11;
            this.ifBigger.Name = "ifBigger";
            // 
            // beforeSendMail
            // 
            this.beforeSendMail.Activities.Add(this.somethingToReport);
            this.beforeSendMail.Name = "beforeSendMail";
            // 
            // sendMessage1
            // 
            this.sendMessage1.Attachments = null;
            this.sendMessage1.Enabled = false;
            this.sendMessage1.Name = "sendMessage1";
            this.sendMessage1.Recipients = null;
            this.sendMessage1.Text = null;
            this.sendMessage1.Title = null;
            this.sendMessage1.Urgency = Easynet.Edge.Core.Messaging.MessageUrgency.Low;
            // 
            // generateMasterReport
            // 
            this.generateMasterReport.HasReport = false;
            this.generateMasterReport.Name = "generateMasterReport";
            // 
            // checkAdgATDeltaSmaller
            // 
            this.checkAdgATDeltaSmaller.Activities.Add(this.ifAdgATSmaller);
            this.checkAdgATDeltaSmaller.Name = "checkAdgATDeltaSmaller";
            // 
            // checkAdgATDeltaBigger
            // 
            this.checkAdgATDeltaBigger.Activities.Add(this.ifAdgATBigger);
            this.checkAdgATDeltaBigger.Name = "checkAdgATDeltaBigger";
            // 
            // checkAdgGWDeltaSmaller
            // 
            this.checkAdgGWDeltaSmaller.Activities.Add(this.ifAdgGWSmaller);
            this.checkAdgGWDeltaSmaller.Name = "checkAdgGWDeltaSmaller";
            // 
            // checkAdgGWDeltaBigger
            // 
            this.checkAdgGWDeltaBigger.Activities.Add(this.ifAdgGWBigger);
            this.checkAdgGWDeltaBigger.Name = "checkAdgGWDeltaBigger";
            // 
            // checkAdgKwDeltaSmaller
            // 
            this.checkAdgKwDeltaSmaller.Activities.Add(this.ifKwSmaller);
            this.checkAdgKwDeltaSmaller.Name = "checkAdgKwDeltaSmaller";
            // 
            // checkAdKwDeltaBigger
            // 
            this.checkAdKwDeltaBigger.Activities.Add(this.ifKwBigger);
            this.checkAdKwDeltaBigger.Name = "checkAdKwDeltaBigger";
            // 
            // checkAdgDeltaSmaller
            // 
            this.checkAdgDeltaSmaller.Activities.Add(this.ifElseBranchActivity1);
            this.checkAdgDeltaSmaller.Name = "checkAdgDeltaSmaller";
            // 
            // checkAdgDeltaBigger
            // 
            this.checkAdgDeltaBigger.Activities.Add(this.ifAdgBigger);
            this.checkAdgDeltaBigger.Name = "checkAdgDeltaBigger";
            // 
            // checkDeltaSmaller
            // 
            this.checkDeltaSmaller.Activities.Add(this.ifSmaller);
            this.checkDeltaSmaller.Name = "checkDeltaSmaller";
            // 
            // checkDeltaBigger
            // 
            this.checkDeltaBigger.Activities.Add(this.ifBigger);
            this.checkDeltaBigger.Name = "checkDeltaBigger";
            // 
            // adgroupAdtexts
            // 
            this.adgroupAdtexts.Name = "adgroupAdtexts";
            // 
            // adgroupGateways
            // 
            this.adgroupGateways.Name = "adgroupGateways";
            // 
            // adgroupKeywords
            // 
            this.adgroupKeywords.Name = "adgroupKeywords";
            // 
            // campaignAdgroups
            // 
            this.campaignAdgroups.Name = "campaignAdgroups";
            // 
            // campaigns
            // 
            this.campaigns.Name = "campaigns";
            // 
            // AccountCampaigns
            // 
            this.Activities.Add(this.campaigns);
            this.Activities.Add(this.campaignAdgroups);
            this.Activities.Add(this.adgroupKeywords);
            this.Activities.Add(this.adgroupGateways);
            this.Activities.Add(this.adgroupAdtexts);
            this.Activities.Add(this.checkDeltaBigger);
            this.Activities.Add(this.checkDeltaSmaller);
            this.Activities.Add(this.checkAdgDeltaBigger);
            this.Activities.Add(this.checkAdgDeltaSmaller);
            this.Activities.Add(this.checkAdKwDeltaBigger);
            this.Activities.Add(this.checkAdgKwDeltaSmaller);
            this.Activities.Add(this.checkAdgGWDeltaBigger);
            this.Activities.Add(this.checkAdgGWDeltaSmaller);
            this.Activities.Add(this.checkAdgATDeltaBigger);
            this.Activities.Add(this.checkAdgATDeltaSmaller);
            this.Activities.Add(this.generateMasterReport);
            this.Activities.Add(this.sendMessage1);
            this.Activities.Add(this.beforeSendMail);
            this.ConditionValues = hashtable1;
            this.ConnectionString = "";
            this.Name = "AccountCampaigns";
            this.Parameters = hashtable2;
            this.Template = false;
            this.WorkflowGUID = new System.Guid("d8a3f427-0eb3-4d0d-886d-1e1811cff07a");
            this.WorkflowID = -1;
            this.WorkflowName = "Account Campaigns Status";
            this.WorkflowType = typeof(Easynet.Edge.Alerts.Core.BaseAlertWorkflow);
            this.CanModifyActivities = false;

		}

		#endregion

        private IfElseBranchActivity ifBigger;
        private IfElseActivity checkDeltaBigger;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateCampaignRisingReport;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateCampaignFallingReport;
        private IfElseBranchActivity ifSmaller;
        private IfElseActivity checkDeltaSmaller;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.MasterReport generateMasterReport;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.CampaignAdgroups campaignAdgroups;
        private IfElseBranchActivity ifAdgBigger;
        private IfElseActivity checkAdgDeltaBigger;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateAdgFallingReport;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateAdgroupRisingReport;
        private IfElseBranchActivity ifElseBranchActivity1;
        private IfElseActivity checkAdgDeltaSmaller;
        private IfElseBranchActivity somethingToReport;
        private IfElseActivity beforeSendMail;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupKeywords adgroupKeywords;
        private IfElseBranchActivity ifKwBigger;
        private IfElseActivity checkAdKwDeltaBigger;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateKwRisingReport;
        private IfElseBranchActivity ifKwSmaller;
        private IfElseActivity checkAdgKwDeltaSmaller;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateKwFallingReport;
        private IfElseBranchActivity ifAdgGWBigger;
        private IfElseActivity checkAdgGWDeltaBigger;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupAdtexts adgroupAdtexts;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AdgroupGateways adgroupGateways;
        private IfElseBranchActivity ifAdgGWSmaller;
        private IfElseActivity checkAdgGWDeltaSmaller;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateGWFallingReport;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateGWRisingReport;
        private IfElseBranchActivity ifAdgATSmaller;
        private IfElseBranchActivity ifAdgATBigger;
        private IfElseActivity checkAdgATDeltaSmaller;
        private IfElseActivity checkAdgATDeltaBigger;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateAdgATFallingReport;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.GenerateReport generateAdgATRisingReport;
        private Easynet.Edge.Core.Workflow.SendMessage sendMessage1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.SendMail sendMail1;
        private Easynet.Edge.Services.Alerts.AlertCustomActivities.AccountCampaigns campaigns;
























































































    }
}
