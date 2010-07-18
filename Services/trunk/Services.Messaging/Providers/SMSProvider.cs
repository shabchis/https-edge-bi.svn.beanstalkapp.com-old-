using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easynet.Edge.Core.Messaging;

namespace Easynet.Edge.Services.Messaging.Providers
{
    public class SMSProvider : Provider
    {
        public override void Initialize()
        {
            _type = "sms";
        }

        public override void Send(string recipient, Message msg)
        {
            //base.Send(recipient, msg);
        }
    }
}
