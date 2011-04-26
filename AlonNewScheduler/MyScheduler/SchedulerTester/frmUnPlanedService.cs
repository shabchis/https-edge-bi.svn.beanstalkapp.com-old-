using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyScheduler;
using MyScheduler.Objects;

namespace SchedulerTester
{
    public partial class frmUnPlanedService : Form
    {
        private Scheduler _scheduler;
        public frmUnPlanedService(Scheduler scheduler)
        {
            InitializeComponent();
            _scheduler = scheduler;
        }

        

        private void FillComboBoxes()
        {
            List<ServiceConfiguration> serviceConfigurations= _scheduler.GetAllExistServices();
           
            foreach (ServiceConfiguration serviceConfiguration in serviceConfigurations)
            {
                servicesCmb.Items.Add(string.Format("{0}:{1}",serviceConfiguration.SchedulingProfile.Name,serviceConfiguration.Name));
                            
            }
           

        }

        private void frmUnPlanedService_Load(object sender, EventArgs e)
        {
            FillComboBoxes();
        }
    }
    
}

