using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WizardForm
{
	public  partial  class WizardPage : UserControl
	{
       protected Timer stepReadyTimer;
		
		public string StepName { get; set; }
        public string StepDescription { get; set; }
        protected const int interval = 1500;
        
		
		
		public WizardPage()
		{
            stepReadyTimer = new Timer();
			InitializeComponent();
			
		}
		public virtual void SetButtons()
		{
			foreach (Control item in this.Parent.Parent.Controls) //the form
			{
				if (item is Button)
				{
                    if (item.Name != "btnExit")
                    {
                        item.Visible = true;
                        item.Enabled = false;
                    }
						
					
					
					
				}
			}
		}
		public virtual void SetTitle()
		{
			this.Parent.Parent.Text = this.StepName;
		}
        public virtual Dictionary<string, object> CollectValues()
        {
            return null;
        }

        private void WizardPage_Load(object sender, EventArgs e)
        {
            
        }

        protected virtual void InitalizePage()
        {
            
        }
        

       
	
	}
	
	

}
