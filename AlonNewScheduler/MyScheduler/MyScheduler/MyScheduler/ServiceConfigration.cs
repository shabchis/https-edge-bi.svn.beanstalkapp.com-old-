using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyScheduler
{
	public class ServiceConfigration
	{
		public ServiceConfigration BaseConfiguration;
		public int ConfigurationID;
		public int ID;
		public string Name;
		public int MaxConcurrentPerConfiguration;
		public int MaxCuncurrentPerProfile;
		public Profile SchedulingProfile;
		public SchedulingRule Rule;
		public bool Scheduled = false;
		public TimeSpan AverageExecutionTime=new TimeSpan(0,10,0);
		public TimeSpan MaxWaitingTime = new TimeSpan(0, 10, 0);
		public int priority;
	}

	//class example
	//{
	//    public example()
	//    {
	//        // Create a new service type
	//        ServiceConfigration googleAdwords = new ServiceConfigration()
	//        {
	//            Name="Google Adwords",
	//            Class = "EdgeBI.Services.Google.Adwords",
	//            MaxConcurrent = 5,
	//            MaxCuncurrentPerProfile = 1,
	//            Settings = new Dictionary<string,object>()
	//            {
	//                {"FavoriteColor" , "red"},
	//                {"Username", "default"}
	//            }
	//            SchedulingRules =
	//            [
	//                new SchedulingRule("every day at 04:00")
	//            ]
	//        };

	//        // Create a new profile
	//        Profile easyForex = new Profile()
	//        {
	//            Name = "Easy Forex's Services",
	//            Settings = new Dictionary<string, object>()
	//            {
	//                {"AccountID", 7}
	//            }
	//        };

	//        // Add the service to the profile
	//        easyForex.Services.Add(new ServiceConfigration()
	//        {
	//            BaseConfiguration = googleAdwords,
	//            Settings = new Dictionary<string, object>()
	//            {
	//                {"Username", "EasyForex"}
	//            }
	//            SchedulingRules =
	//            [
	//                new SchedulingRule("every day at 07:00")
	//            ]
	//        });

	//        Scheduler sc = new Scheduler();

	//    }
	//}
}
