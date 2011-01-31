using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	class Program
	{

		static void Main()
		{
			List<ServiceConfigration> services = new List<ServiceConfigration>();


			ServiceConfigration sc1 = new ServiceConfigration() { ID = 1, ConfigurationID=77, Name="service1",priority=10, MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 1 ,ProfileID=88}, Rule = new SchedulingRule() { time =DateTime.Now } };
			services.Add(sc1);
			ServiceConfigration sc12 = new ServiceConfigration() { ID = 2, ConfigurationID = 77, Name = "service2", priority = 11, AverageExecutionTime = new TimeSpan(0, 5, 0), MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 2, ProfileID = 88 }, Rule = new SchedulingRule() { time = DateTime.Now } };
			
			services.Add(sc12);

			ServiceConfigration sc2 = new ServiceConfigration() { ID = 3, ConfigurationID = 77, Name = "service3", priority = 2, MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 3, ProfileID = 88 }, Rule = new SchedulingRule() { time = DateTime.Now } };
			services.Add(sc2);
			ServiceConfigration sc21 = new ServiceConfigration() { ID = 4, ConfigurationID = 77, Name = "service4", priority = 1, MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 4, ProfileID = 88 }, Rule = new SchedulingRule() { time = DateTime.Now } };
			services.Add(sc21);

			ServiceConfigration sc3 = new ServiceConfigration() { ID = 5, ConfigurationID = 77, Name = "service5", priority = 1, MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 5, ProfileID = 88 }, Rule = new SchedulingRule() { time = DateTime.Now } };
			services.Add(sc3);
			ServiceConfigration sc31 = new ServiceConfigration() { ID = 6, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 2, MaxCuncurrentPerProfile = 2, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = DateTime.Now } };
			services.Add(sc31);

			Scheduler s = new Scheduler(services);
			s.CreateSchedule();

			
		}
	}
}
