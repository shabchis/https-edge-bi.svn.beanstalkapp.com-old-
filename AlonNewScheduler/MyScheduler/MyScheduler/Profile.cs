using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	public class Profile
	{
		public int ID;
		public int ProfileID;
		public TimeSpan MaxDevitation;
		public string Name;

		public List<ServiceConfigration> Services;
	}
}
