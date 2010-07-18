using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.Services.ScheduleManagement
{
	
	class ScheduleBuildingCueService: Service
	{
		protected override ServiceOutcome DoWork()
		{
			ServiceClient<IScheduleManager> client = new ServiceClient<IScheduleManager>();
			try
			{
				// Request the manager to build the schedule
				using (client)
				{
					client.Service.BuildSchedule();
				}
				return ServiceOutcome.Success;
			}
			catch(Exception ex)
			{
				Log.Write("ScheduleManager refused the request to build the schedule.", ex);
				return ServiceOutcome.Failure;
			}
		}
	}
}
