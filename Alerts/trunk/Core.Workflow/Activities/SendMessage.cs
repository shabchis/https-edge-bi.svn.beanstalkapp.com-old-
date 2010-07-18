using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Easynet.Edge.Core.Messaging;
using System.Collections.Generic;
using Easynet.Edge.Core.Services;

namespace Easynet.Edge.Core.Workflow
{
	public partial class SendMessage: BaseActivity
	{

		public SendMessage()
		{
			InitializeComponent();
		}



        public static DependencyProperty UrgencyProperty = DependencyProperty.Register("Urgency", typeof(MessageUrgency), typeof(SendMessage));

        public MessageUrgency Urgency
        {
            get
            {
                return ((MessageUrgency)base.GetValue(SendMessage.UrgencyProperty));
            }
            set
            {
                base.SetValue(SendMessage.UrgencyProperty, value);
            }
        }


        public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SendMessage));

        public string Title
        {
            get
            {
                return ((string)base.GetValue(SendMessage.TitleProperty));
            }
            set
            {
                base.SetValue(SendMessage.TitleProperty, value);
            }
        }


        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SendMessage));

        public string Text
        {
            get
            {
                return ((string)base.GetValue(SendMessage.TextProperty));
            }
            set
            {
                base.SetValue(SendMessage.TextProperty, value);
            }
        }


        public static DependencyProperty RecipientsProperty = DependencyProperty.Register("Recipients", typeof(List<string>), typeof(SendMessage));

        public List<string> Recipients
        {
            get
            {
                return ((List<string>)base.GetValue(SendMessage.RecipientsProperty));
            }
            set
            {
                base.SetValue(SendMessage.RecipientsProperty, value);
            }
        }


        public static DependencyProperty AttachmentsProperty = DependencyProperty.Register("Attachments", typeof(List<string>), typeof(SendMessage));

        public List<string> Attachments
        {
            get
            {
                return ((List<string>)base.GetValue(SendMessage.AttachmentsProperty));
            }
            set
            {
                base.SetValue(SendMessage.AttachmentsProperty, value);
            }
        }



        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {

            /* Get the overriding data from previous activities (if there was any) */
            if (ParentWorkflow.InternalParameters.ContainsKey("MessageTitle"))
                Title = ParentWorkflow.InternalParameters["MessageTitle"].ToString();

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageText"))
                Text = ParentWorkflow.InternalParameters["MessageText"].ToString();

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageUrgency"))
                Urgency = (MessageUrgency)ParentWorkflow.InternalParameters["MessageUrgency"];

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageAttachments"))
                Attachments = (List<string>)ParentWorkflow.InternalParameters["MessageAttachments"];

            if (ParentWorkflow.InternalParameters.ContainsKey("MessageRecipients"))
                Recipients = (List<string>)ParentWorkflow.InternalParameters["MessageRecipients"];
            /* End - Data override */

            //Send the message based on the message text, title, recipients and urgency.
            using (ServiceClient<IMessagingService> msging = new ServiceClient<IMessagingService>())
            {
                Message msg = new Message(Title, Text, Attachments);
                msging.Service.SendMultiple(Recipients, Urgency, msg);
            }

            return ActivityExecutionStatus.Closed;
        }


	}
}
