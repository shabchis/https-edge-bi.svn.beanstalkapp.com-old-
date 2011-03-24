using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Configuration;


namespace MyScheduler.Objects
{
	public class ServiceConfiguration
	{
		public int ID;
		public ServiceConfiguration BaseConfiguration;			
		public string Name;
		public int MaxConcurrent;
		public int MaxCuncurrentPerProfile;
		public Profile SchedulingProfile;
		public List<SchedulingRule> SchedulingRules=new List<SchedulingRule>();
		public bool Scheduled = false;
		public TimeSpan AverageExecutionTime=new TimeSpan(0,30,0);
		public TimeSpan MaxExecutionTime = new TimeSpan(0,60, 0);
		public ActiveServiceElement LegacyConfiguration;
		public int priority;
		
	}


}
