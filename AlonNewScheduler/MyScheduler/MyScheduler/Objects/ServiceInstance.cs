using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Services;
using Legacy = Easynet.Edge.Core.Services; 
using Easynet.Edge.Core.Configuration;

namespace MyScheduler.Objects
{
	/// <summary>
	/// Date of scheduling
	/// </summary>
	public class ServiceInstance
	{
		public int ID;
		public string ServiceName;
		public int BaseConfigurationID;
		public int ProfileID;
		public DateTime StartTime;
		public DateTime EndTime;
		public int MaxConcurrentPerConfiguration;
		public int MaxCuncurrentPerProfile;
		public int Priority;
		public TimeSpan MaxDeviationBefore;
		public TimeSpan MaxDeviationAfter;
		public TimeSpan ActualDeviation;
		public double Odds;
		public ServiceStatus State;
		public bool Deleted;
		
		public Legacy.ServiceInstance LegacyInstance;
		
		public ServiceOutcome Outcome
		{
			get { return this.LegacyInstance.Outcome; }
		}
		//internal ActiveServiceElement LegacyConfiguration;
		
	}
	/// <summary>
	/// service-hour 
	/// </summary>
	public struct ServiceHourStruct
	{
		public TimeSpan SuitableHour;
		public SchedulingData Service;
	}
	public enum ServiceStatus
	{
		Scheduled,
		Running,
		Ended
	}
}
