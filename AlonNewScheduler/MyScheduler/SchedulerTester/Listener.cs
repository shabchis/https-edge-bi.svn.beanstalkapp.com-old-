using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Scheduling;
using System.ServiceModel;
using Easynet.Edge.Core.Configuration;
using MyScheduler.Objects;
using MyScheduler;
using Easynet.Edge.Core.Services;

namespace SchedulerTester
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
   public class Listener : IScheduleManager, IDisposable
    {
        ServiceHost _wcfHost;
        Scheduler _scheduler;


        public void Start()
        {
            _wcfHost = new ServiceHost(this);
            _wcfHost.Open();
        }
        public Listener(Scheduler scheduler)
        {
            _scheduler = scheduler;


        }


        public void BuildSchedule()
        {
            throw new NotImplementedException();
        }

        public bool AddToSchedule(string serviceName, int accountID, DateTime targetTime, Easynet.Edge.Core.SettingsCollection options)
        {
            bool respond = true;
            try
            {
                ServiceConfiguration myServiceConfiguration = new ServiceConfiguration();
                ServiceConfiguration baseConfiguration = new ServiceConfiguration();
                ActiveServiceElement activeServiceElement = new ActiveServiceElement(ServicesConfiguration.Accounts.GetAccount(accountID).Services[serviceName]);
                if (options != null)
                {
                    foreach (string option in options.Keys)
                        activeServiceElement.Options[option] = options[option];
                }
                ServiceElement serviceElement = ServicesConfiguration.Services[serviceName];

                //base configuration;
                baseConfiguration.Name = serviceElement.Name;
                baseConfiguration.MaxConcurrent = serviceElement.MaxInstances;
                baseConfiguration.MaxCuncurrentPerProfile = serviceElement.MaxInstancesPerAccount;


                //configuration per profile
                myServiceConfiguration = new ServiceConfiguration();
                myServiceConfiguration.Name = activeServiceElement.Name;
                if (activeServiceElement.Options.ContainsKey("ServicePriority"))
                    myServiceConfiguration.priority = int.Parse(activeServiceElement.Options["ServicePriority"]);
                myServiceConfiguration.MaxConcurrent = (activeServiceElement.MaxInstances == 0) ? 9999 : activeServiceElement.MaxInstances;
                myServiceConfiguration.MaxCuncurrentPerProfile = (activeServiceElement.MaxInstancesPerAccount == 0) ? 9999 : activeServiceElement.MaxInstancesPerAccount;
                myServiceConfiguration.LegacyConfiguration = activeServiceElement;
                //        //scheduling rules 
                myServiceConfiguration.SchedulingRules.Add(new SchedulingRule()
                {
                    Scope = SchedulingScope.UnPlanned,
                    SpecificDateTime = DateTime.Now,
                    MaxDeviationAfter = new TimeSpan(0, 0, 45, 0, 0),
                    Hours=new List<TimeSpan>()
                });
                myServiceConfiguration.SchedulingRules[0].Hours.Add(new TimeSpan(0, 0, 0, 0));
                myServiceConfiguration.BaseConfiguration = baseConfiguration;
                Profile profile = new Profile()
                {
                    ID = accountID,
                    Name = accountID.ToString(),
                    Settings = new Dictionary<string, object>()
                };
                profile.Settings.Add("AccountID", accountID);
                myServiceConfiguration.SchedulingProfile = profile;
                _scheduler.AddNewServiceToSchedule(myServiceConfiguration);
            }
            catch (Exception ex)
            {
                respond = false;
                Easynet.Edge.Core.Utilities.Log.Write("AddManualServiceListner", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
  
                
            }      
          
           

            return respond;
        }

        public bool FormAddToSchedule(string serviceName, int accountID, DateTime targetTime, Easynet.Edge.Core.SettingsCollection options, ServicePriority servicePriority)
        {
            bool respond = true;
            try
            {
                ServiceConfiguration myServiceConfiguration = new ServiceConfiguration();
                ServiceConfiguration baseConfiguration = new ServiceConfiguration();
                ActiveServiceElement activeServiceElement = new ActiveServiceElement(ServicesConfiguration.Accounts.GetAccount(accountID).Services[serviceName]);
                if (options != null)
                {
                    foreach (string option in options.Keys)
                        activeServiceElement.Options[option] = options[option];
                }
                ServiceElement serviceElement = ServicesConfiguration.Services[serviceName];
                
                //base configuration;
                baseConfiguration.Name = serviceElement.Name;
                baseConfiguration.MaxConcurrent = serviceElement.MaxInstances;
                baseConfiguration.MaxCuncurrentPerProfile = serviceElement.MaxInstancesPerAccount;


                //configuration per profile
                myServiceConfiguration = new ServiceConfiguration();
                myServiceConfiguration.Name = activeServiceElement.Name;
                myServiceConfiguration.priority = (int)servicePriority;
                if (activeServiceElement.Options.ContainsKey("ServicePriority"))
                    myServiceConfiguration.priority = int.Parse(activeServiceElement.Options["ServicePriority"]);
                myServiceConfiguration.MaxConcurrent = (activeServiceElement.MaxInstances == 0) ? 9999 : activeServiceElement.MaxInstances;
                myServiceConfiguration.MaxCuncurrentPerProfile = (activeServiceElement.MaxInstancesPerAccount == 0) ? 9999 : activeServiceElement.MaxInstancesPerAccount;
                myServiceConfiguration.LegacyConfiguration = activeServiceElement;
                //        //scheduling rules 
                myServiceConfiguration.SchedulingRules.Add(new SchedulingRule()
                {
                    Scope = SchedulingScope.UnPlanned,
                    SpecificDateTime = targetTime,
                    MaxDeviationAfter = new TimeSpan(0, 0, 45, 0, 0),
                    Hours = new List<TimeSpan>()
                });
                myServiceConfiguration.SchedulingRules[0].Hours.Add(new TimeSpan(0, 0, 0, 0));
                myServiceConfiguration.BaseConfiguration = baseConfiguration;
                Profile profile = new Profile()
                {
                    ID = accountID,
                    Name = accountID.ToString(),
                    Settings = new Dictionary<string, object>()
                };
                profile.Settings.Add("AccountID", accountID);
                myServiceConfiguration.SchedulingProfile = profile;
                _scheduler.AddNewServiceToSchedule(myServiceConfiguration);
            }
            catch (Exception ex)
            {
                respond = false;
                Easynet.Edge.Core.Utilities.Log.Write("AddManualServiceListner", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);


            }



            return respond;
        }
        public void Dispose()
        {
            if (_wcfHost != null)
                ((IDisposable)_wcfHost).Dispose();
        }
    }
}
