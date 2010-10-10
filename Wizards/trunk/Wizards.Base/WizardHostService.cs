using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel.Web;
using System.ServiceModel.Description;

namespace Easynet.Edge.Wizards
{   
	/// <summary>
	/// host the wizardServices using wcf
	/// </summary>
	public class WizardHostService: Service
	{
		WebServiceHost _host = null;

		protected override void OnInit()
		{
			_host = new WebServiceHost(typeof(WizardRestService), new Uri("http://localhost:8080/wizard"));
			ServiceDebugBehavior sdb = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
			sdb.IncludeExceptionDetailInFaults = true;
			
			sdb.HttpHelpPageEnabled = true;
			
			
			
			
			_host.Open();
		}

		
		protected override ServiceOutcome DoWork()
		{
			
			return ServiceOutcome.Unspecified;
		}

		protected override void OnEnded(ServiceOutcome outcome)
		{
			if (_host != null && _host.State == System.ServiceModel.CommunicationState.Opened)
				_host.Close();
		}
	}
	
	
}
