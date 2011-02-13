using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	public class SchedulingRule
	{
		public DateTime time;
		public TimeSpan MaxDeviation = new TimeSpan(1000, 10, 0);
	}
}
