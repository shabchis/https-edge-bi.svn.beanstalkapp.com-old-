﻿using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Alerts.Core;

namespace Easynet.Edge.Services.Alerts.AlertWorkflows
{
	public sealed partial class AdministrativeAlerts: BaseAlertWorkflow
	{
		public AdministrativeAlerts()
		{
			InitializeComponent();
		}
	}

}
