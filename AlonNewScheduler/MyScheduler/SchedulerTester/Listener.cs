using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Scheduling;
using System.ServiceModel;
using Easynet.Edge.Core.Configuration;

namespace SchedulerTester
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    class Listener : IScheduleManager, IDisposable
    {
        ServiceHost _wcfHost;

        public void Start()
        {
            _wcfHost = new ServiceHost(this);
            _wcfHost.Open();
        }


        public void BuildSchedule()
        {
            throw new NotImplementedException();
        }

        public bool AddToSchedule(string serviceName, int accountID, DateTime targetTime, Easynet.Edge.Core.SettingsCollection options)
        {
            ActiveServiceElement config = new ActiveServiceElement(ServicesConfiguration.Accounts.GetAccount(accountID).Services[serviceName]);
            foreach (string option in options.Keys)
                config.Options[option] = options[option];

            return false;
        }


        public void Dispose()
        {
            if (_wcfHost != null)
                ((IDisposable)_wcfHost).Dispose();
        }
    }
}
