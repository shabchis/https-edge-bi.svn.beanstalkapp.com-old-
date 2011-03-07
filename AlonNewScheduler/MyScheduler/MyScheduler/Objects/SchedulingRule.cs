using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler.Objects
{
	public class SchedulingRule
	{
		public SchedulingScope Scope { get; set; }
		public List<int> Days { get; set; }// { get; }
		public List<TimeSpan> Hours { get; set; }// { get; }
		//public TimeSpan Frequency { get; set; }
		public TimeSpan MaxDeviationBefore { get; set; }
		public TimeSpan MaxDeviationAfter { get; set; }
		// public Dictionary<string,object> ServiceSettings { get; }
	}

	public enum SchedulingScope
	{
		Day,
		Week,
		Month
	}
}
