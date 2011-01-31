using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	class UniqueConfiguration
	{
		public string ID;
		private int _maxPerProfile;
		

		public int MaxPerProfile
		{
			get { return _maxPerProfile; }
			
		}	
		public void SetMaxPerProfile(int maxPerProfile,int maxPerConfiguration)
		{
			if (maxPerProfile < maxPerConfiguration)
				_maxPerProfile = maxPerProfile;
			else
				_maxPerProfile = maxPerConfiguration;
		}

		
	}
	public class TimeRow
	{
		public DateTime FromTime;
		public DateTime ToTime;
		public Dictionary<string, ServiceConfigration> services=new Dictionary<string,ServiceConfigration>();
	}
	
}
