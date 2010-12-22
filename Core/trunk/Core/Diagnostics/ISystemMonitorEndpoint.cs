using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Easynet.Edge.Diagnostics.SystemMonitor
{
	[ServiceContract]
	public interface ISystemMonitorEndpoint
	{
		[OperationContract]
		bool IsAlive();
	}
	
}
