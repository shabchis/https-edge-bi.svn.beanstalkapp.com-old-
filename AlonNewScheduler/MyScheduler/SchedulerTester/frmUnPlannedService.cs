﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyScheduler;
using MyScheduler.Objects;
using Easynet.Edge.Core.Services;

namespace SchedulerTester
{
    public partial class frmUnPlannedService : Form
    {
        private Listener _listner;
        private Scheduler _scheduler;
        public frmUnPlannedService(Listener listner,Scheduler scheduler)
        {
            InitializeComponent();
            _listner = listner;
            _scheduler = scheduler;
        }



        private void FillComboBoxes()
        {
            //services per account
            List<ServiceConfiguration> serviceConfigurations = _scheduler.GetAllExistServices();
             serviceConfigurations = serviceConfigurations.OrderBy((s => s.SchedulingProfile.ID)).ToList();
            foreach (ServiceConfiguration serviceConfiguration in serviceConfigurations)
            {
                servicesCmb.Items.Add(string.Format("{0}    :   {1}", serviceConfiguration.SchedulingProfile.Name,  serviceConfiguration.Name));

            }
            priorityCmb.Items.Add(ServicePriority.Normal);
            priorityCmb.Items.Add(ServicePriority.Low);
            priorityCmb.Items.Add(ServicePriority.High);
            priorityCmb.Items.Add(ServicePriority.Immediate);



        }

        private void frmUnPlanedService_Load(object sender, EventArgs e)
        {
            FillComboBoxes();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string[] serviceAndAccount;
                ServicePriority servicePriority = ServicePriority.Low;

                if (servicesCmb.SelectedItem != null)
                    serviceAndAccount = servicesCmb.SelectedItem.ToString().Split(':');
                else
                    throw new Exception("You must choose service!");
                string account = serviceAndAccount[0].Trim();
                string serviceName = serviceAndAccount[1].Trim();
               

                if (priorityCmb.SelectedItem!=null)
                    switch (priorityCmb.SelectedItem.ToString())
                    {
                        case "Low":
                            {
                                servicePriority = ServicePriority.Low;
                                break;
                            }
                        case "Normal":
                            {
                                servicePriority = ServicePriority.Normal;
                                break;
                            }
                        case "High":
                            {
                                servicePriority = ServicePriority.High;
                                break;
                            }
                        case "Immediate":
                            {
                                servicePriority = ServicePriority.Immediate;
                                break;
                            }
                    }

                _listner.AddToSchedule(serviceName, int.Parse(account), DateTime.Now, new Easynet.Edge.Core.SettingsCollection());

                MessageBox.Show("Service has been added to schedule and will be runinng shortly");
                this.Close();


               
            }
            catch (Exception ex)
            {

                 MessageBox.Show(ex.Message);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }

}

