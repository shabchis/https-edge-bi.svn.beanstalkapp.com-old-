using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using System.ServiceModel.Web;
using System.ServiceModel.Description;

namespace EdgeBI.Service
{
	public class APIHostService : Easynet.Edge.Core.Services.Service
	{
		WebServiceHost _host = null;
		protected override void OnInit()
		{
			_host = new WebServiceHost(typeof(APIRestService), new Uri("http://localhost:8080/EdgeObjects"));
			ServiceDebugBehavior sdb = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
			sdb.IncludeExceptionDetailInFaults = true;

			sdb.HttpHelpPageEnabled = true;




			_host.Open();
		}
		protected override ServiceOutcome DoWork()
		{
			return ServiceOutcome.Unspecified;
		}
	}
}
