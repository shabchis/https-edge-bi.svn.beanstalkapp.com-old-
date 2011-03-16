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
			

			
			Scheduler s = new Scheduler(true);
			Console.WriteLine("Scheduling time is:{0}\n", DateTime.Now.TimeOfDay);
			
			Console.WriteLine("Press any N for next scheduling ,anything else to exit\n");
			while (Console.ReadLine()=="n")
			{
				Console.WriteLine("Press any N for next scheduling ,anything else to exit\n");
				s.CreateSchedule();
			}



			//List<ServiceConfigration> services = new List<ServiceConfigration>();


			//ServiceConfigration sc1 = new ServiceConfigration() { ID = 1, ConfigurationID=77, Name="service1",priority=2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 1 ,ProfileID=88}, Rule = new SchedulingRule() { time =new DateTime(2011, 01, 01, 00, 00, 00) } };
			//services.Add(sc1);
			//ServiceConfigration sc12 = new ServiceConfigration() { ID = 2, ConfigurationID = 77, Name = "service2", priority = 2, /*AverageExecutionTime = new TimeSpan(0, 5, 0),*/ MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 2, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } };

			//services.Add(sc12);

			//ServiceConfigration sc2 = new ServiceConfigration() { ID = 3, ConfigurationID = 77, Name = "service3", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 3, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00)/*.AddHours(1)*/ } };
			//services.Add(sc2);
			//ServiceConfigration sc21 = new ServiceConfigration() { ID = 4, ConfigurationID = 77, Name = "service4", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 4, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } };
			//services.Add(sc21);

			//ServiceConfigration sc3 = new ServiceConfigration() { ID = 5, ConfigurationID = 77, Name = "service5", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 5, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } };
			//services.Add(sc3);
			//ServiceConfigration sc31 = new ServiceConfigration() { ID = 6, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } };
			//services.Add(sc31);

			//services.Add(new ServiceConfigration() { ID = 65151, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 7, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 8, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 9, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 61, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 62, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 63, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 64, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 65, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 66, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 67, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 68, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 69, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 610, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 611, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 612, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 613, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 614, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 615, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 616, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 617, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 628, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 629, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 630, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 631, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 632, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 633, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 6374, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 635, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 636, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 6367, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 638, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });
			//services.Add(new ServiceConfigration() { ID = 699, ConfigurationID = 77, Name = "service6", priority = 2, MaxConcurrentPerConfiguration = 1, MaxCuncurrentPerProfile = 1, SchedulingProfile = new Profile() { ID = 6, ProfileID = 88 }, Rule = new SchedulingRule() { time = new DateTime(2011, 01, 01, 00, 00, 00) } });

			
		}
	}
}
