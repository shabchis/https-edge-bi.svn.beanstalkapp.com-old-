using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;

using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Messaging;
using Easynet.Edge.Services.Messaging.Providers;
using Easynet.Edge.BusinessObjects;

namespace Easynet.Edge.Services.Messaging
{
    public class MessagingService : Service, IMessagingService
    {

        #region Members
        private Dictionary<string, Provider> _providers = new Dictionary<string, Provider>();
        private SystemUsers _systemUsers = null;
        #endregion

        #region IMessagingService Members

        public void Send(string recipient, MessageUrgency urgency, Message msg)
        {
            List<string> recipients = new List<string>();
            recipients.Add(recipient);

            SendMessage(recipients, urgency, msg);
        }

        public void SendMultiple(List<string> recipients, MessageUrgency urgency, Message msg)
        {
            SendMessage(recipients, urgency, msg);
        }

        #endregion

        #region Protected Overrides
        protected override ServiceOutcome DoWork()
        {
            //Initialize all of the providers.
            EmailProvider ep = new EmailProvider();
            ep.Initialize();

            SMSProvider smsp = new SMSProvider();
            smsp.Initialize();

//            SkypeProvider skp = new SkypeProvider();
//            skp.Initialize();

            EdgeMessengerProvider emp = new EdgeMessengerProvider();
            emp.Initialize();

            _providers.Add(ep.Type, ep);
            _providers.Add(smsp.Type, smsp);
//            _providers.Add(skp.Type, skp);
            _providers.Add(emp.Type, emp);

            _systemUsers = new SystemUsers(AppSettings.GetAbsolute("Easynet.Edge.Core.Domain"));

            return ServiceOutcome.Unspecified;
        }
        #endregion

        #region Private Methods
        private string GetRecipient(string type, string recipient)
        {
            string ret = String.Empty;
            foreach (SystemUser su in _systemUsers)
            {
                if (su.Name == recipient)
                {
                    switch (type)
                    {
                        case "skype":
                            {
                                ret = su.Skype;
                                break;
                            }

                        case "email":
                            {
                                ret = su.Email;
                                break;
                            }

                        case "sms":
                            {
                                ret = su.CellPhone;
                                break;
                            }

                        case "edge":
                            {
                                ret = su.Name;
                                break;
                            }
                    }

                    break;
                }
            }

            return ret;
        }

        private List<Provider> GetProviders(string providerString)
        {
            if (providerString == null ||
                providerString == String.Empty)
                throw new ArgumentException("Invalid provider string. Cannot be null or empty.");

            List<Provider> ret = new List<Provider>();

            string[] providers = providerString.Split('|');
            if (providers.Length <= 0)
                throw new Exception("Invalid provider string. Does not contain any providers!");

            for (int i = 0; i < providers.Length; i++)
            {
                Provider p = _providers[providers[i]];
                ret.Add(p);
            }

            return ret;
        }

        private void SendMessage(List<string> recipients, MessageUrgency urgency, Message msg)
        {
            SettingsCollection urgencyConfig = new SettingsCollection(Instance.Configuration.Options["DeliveryOptions"]);
            IDictionaryEnumerator ide = urgencyConfig.GetEnumerator();

            while (ide.MoveNext())
            {
                if (urgency.ToString() == ide.Key.ToString())
                {
                    //We found an urgency that matches us. Now get the relevant providers.
                    List<Provider> ps = GetProviders(ide.Value.ToString());

                    foreach (Provider p in ps)
                    {
                        foreach (string r in recipients)
                        {
                            p.Send(GetRecipient(p.Type, r), msg);
                        }
                    }
                }
            }
        }
        #endregion

    }
}
