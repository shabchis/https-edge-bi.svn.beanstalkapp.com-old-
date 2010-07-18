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
using System.Net.Mail;
using Easynet.Edge.Core.Workflow;

namespace Easynet.Edge.Services.Alerts.AlertCustomActivities
{
	public partial class SendMail: BaseActivity
	{

        int _port = -1;
        string _host = String.Empty;

		public SendMail()
		{
			InitializeComponent();
		}


        public static DependencyProperty MessageBodyProperty = DependencyProperty.Register("MessageBody", typeof(string), typeof(SendMail));

        [Category("Properties")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string MessageBody
        {
            get
            {
                return ((string)base.GetValue(SendMail.MessageBodyProperty));
            }
            set
            {
                base.SetValue(SendMail.MessageBodyProperty, value);
            }
        }


        public static DependencyProperty RecipientProperty = DependencyProperty.Register("Recipient", typeof(string), typeof(SendMail));

        [Category("Properties")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Recipient
        {
            get
            {
                return ((string)base.GetValue(SendMail.RecipientProperty));
            }
            set
            {
                base.SetValue(SendMail.RecipientProperty, value);
            }
        }


        public static DependencyProperty OverrideProperty = DependencyProperty.Register("Override", typeof(bool), typeof(SendMail));

        [Category("Properties")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Override
        {
            get
            {
                return ((bool)base.GetValue(SendMail.OverrideProperty));
            }
            set
            {
                base.SetValue(SendMail.OverrideProperty, value);
            }
        }



        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            //Get the email address from the parameter list.
            string emails = String.Empty;
            if (ParentWorkflow.Parameters.ContainsKey("Emails"))
                emails = ParentWorkflow.Parameters["Emails"].ToString();

            if (Recipient != null &&
                Recipient != String.Empty)
            {
                if (emails != String.Empty)
                    emails += "," + Recipient;
                else
                    emails = Recipient;
            }

            //get host and port
            if (ParentWorkflow.Parameters.ContainsKey("SMTPHost"))
                _host = ParentWorkflow.Parameters["SMTPHost"].ToString();
            else
                _host = "mailgw.netvision.net.il"; //default.

            if (ParentWorkflow.Parameters.ContainsKey("SMTPPort"))
                _port = Convert.ToInt32(ParentWorkflow.Parameters["SMTPPort"]);
            else
                _port = 25; //default.

            //Build the message body.
            string body = "The attached report contains the information needed.";

            //Send the email.
            SmtpClient smtp = new SmtpClient(_host, _port);
            smtp.UseDefaultCredentials = true;

            if (emails == String.Empty)
            {
                //See if we have internal emails.
                if (ParentWorkflow.InternalParameters.ContainsKey("Emails"))
                    emails = ParentWorkflow.InternalParameters["Emails"].ToString();

                if (emails == String.Empty)
                    return ActivityExecutionStatus.Closed;
            }

            string cur = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("CurrentDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["CurrentDate"]);
                cur = t.ToString("yyyyMMdd");
            }

            string comp = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("CompareDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["CompareDate"]);
                comp = t.ToString("yyyyMMdd");
            }

            string date = String.Empty;
            if (ParentWorkflow.InternalParameters.ContainsKey("ReportDate"))
            {
                DateTime t = Convert.ToDateTime(ParentWorkflow.InternalParameters["ReportDate"]);
                date = t.ToString("yyyyMMdd");
            }

            string accountName = "Unknown";
            if (ParentWorkflow.InternalParameters.ContainsKey("AccountName"))
                accountName = ParentWorkflow.InternalParameters["AccountName"].ToString();

            string subject = "Message from easynet Alert System (Account: " + accountName + ")";
            if (cur != String.Empty)
                subject += " Current Date: " + cur;

            if (comp != String.Empty)
                subject += " Base Date: " + comp;

            if (date != String.Empty)
                subject += " Report Date: " + date;

            string[] to = emails.Split(',');
            for (int i = 0; i < to.Length; i++)
            {
                MailMessage mm = new MailMessage("alerts@easynet.co.il", to[i]);
                if (!Override)
                {
                    mm.Subject = subject;
                    mm.Body = body;

                    if (ParentWorkflow.InternalParameters["MessageFileName"] != null)
                        mm.Attachments.Add(new Attachment(ParentWorkflow.InternalParameters["MessageFileName"].ToString()));
                    else
                        mm.Body = accountName + ": Has Nothing to report.";
                }
                else
                {
                    mm.Body = "The EdgeServiceHost has died.";
                    mm.Subject = "Message From Alert System.";
                }

                smtp.Send(mm);
            }

            return ActivityExecutionStatus.Closed;
        }
	}
}
