using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Easynet.Edge.Core.Messaging;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.Services.Messaging.Providers
{
    public class EdgeMessengerProvider : Provider
    {

        public override void Initialize()
        {
            _type = "edge";
        }

        public override void Send(string recipient, Message msg)
        {
            using (ServiceClient<IMessengerService> messenger = new ServiceClient<IMessengerService>())
            {
                messenger.Service.Send(recipient, msg.MessageText);
            }
        }

    }
}
