using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ServiceModel;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Data.Proxy;
using Easynet.Edge.Core.Workflow;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Utilities;

using System.Runtime.Serialization;

namespace Easynet.Edge.Core.Messaging
{

    [ServiceContract]
    public interface IMessagingService
    {
        [OperationContract(IsOneWay = true)]
        void Send(string recipient, MessageUrgency urgency, Message msg);

        [OperationContract(IsOneWay = true)]
        void SendMultiple(List<string> recipients, MessageUrgency urgency, Message msg);
    }

    [ServiceContract]
    public interface IMessengerService
    {
        [OperationContract(IsOneWay = true)]
        void Send(string recipient, string msg);
    }

    [DataContract]
    public class Message
    {

        private string _msg = String.Empty;
        private string _title = String.Empty;
        private List<string> _attachments = new List<string>();


        public Message()
        {
        }

        public Message(string title)
        {
            _title = title;
        }

        public Message(string title, string msg)
        {
            _title = title;
            _msg = msg;
        }

        public Message(string title, string msg, List<string> attachments)
        {
            _title = title;
            _msg = msg;
            _attachments = attachments;
        }


        [DataMember]
        public string MessageText
        {
            get
            {
                return _msg;
            }
            set
            {
                _msg = value;
            }
        }

        [DataMember]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        [DataMember]
        public List<string> Attachments
        {
            get
            {
                return _attachments;
            }
            set
            {
                _attachments = value;
            }
        }

    }

}
