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
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;
using System.ComponentModel;

using Easynet.Edge.Core.Workflow;

namespace Easynet.Edge.Core.Workflow
{
	public class SendMessageDesigner : BaseActivityDesigner
	{

        #region Overrides
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            _me = (SendMessage)activity;
            _title = "Send Message";
        }
        #endregion

        #region Private Methods
        private void OnSettings(object sender, EventArgs e)
        {
        }
        #endregion

    }
}

