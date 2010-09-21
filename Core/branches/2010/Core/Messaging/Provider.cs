using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Core.Messaging
{

    public enum MessageUrgency
    {
        Unknown = -1,
        Low = 0,
        Medium = 1,
        High = 2,
    }


    public abstract class Provider
    {

        protected string _type = String.Empty;
        private Dictionary<string, object> _providerConfiguration = new Dictionary<string, object>();


        public string Type
        {
            get
            {
                return _type;
            }
        }


        public virtual void Initialize()
        {
            throw new NotImplementedException("Not implemented");
        }

        public virtual void Send(string recipient, Message msg)
        {
            throw new NotImplementedException("Not implemented!");
        }


        protected object GetProviderConfiguration(string key)
        {
            if (key == null)
                throw new ArgumentNullException("Invalid communication provider configuration key, cannot be null.");

            if (key == String.Empty)
                throw new ArgumentException("Invalid communication provider configuration key, cannot be null.");

            if (!_providerConfiguration.ContainsKey(key))
                throw new Exception("Could not find key: " + key + " in the communication provider configuration dictionary");

            return _providerConfiguration[key];
        }

        protected void AddProviderConfiguration(string key, object value)
        {
            if (_providerConfiguration.ContainsKey(key))
                throw new InvalidOperationException("Could not add the same key [" + key + "] more than once to the communication provider configuration");

            _providerConfiguration.Add(key, value);
        }

    }
}
