using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyScheduler;

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
	struct SchedulingData
	{
		public ServiceConfiguration Configuration;
		public SchedulingRule Rule;
		public int SelectedDay = 1;
		//public int SelectedHour;
		// public.... ?

		public override string ToString()
		{
			/*
			// Hash code example:
			string s1, s2;
			s1 = "blah blah";
			s2 = "blah blah";

			s1.GetHashCode() == s2.GetHashCode(); // this is true!!
			*/

			return String.Format("{Configuration: 12, Rule: {Hours: 12323, ... }, SelectedDay: 12323}");
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is SchedulingData)
				return obj.GetHashCode() == this.GetHashCode();
			else
				return false;
		}

		public static bool operator ==(SchedulingData sd1, SchedulingData sd2)
		{
			return sd1.Equals(sd2);
		}

		public static bool operator !=(SchedulingData sd1, SchedulingData sd2)
		{
			return !sd1.Equals(sd2);
		}
	}

	public enum SchedulingScope
	{
		Day,
		Week,
		Month
	}
}
