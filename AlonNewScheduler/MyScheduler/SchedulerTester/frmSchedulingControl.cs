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
using System.IO;
using legacy = Easynet.Edge.Core.Services;
using System.Diagnostics;
using System.Configuration;
using System.Threading;


namespace SchedulerTester
{
    public partial class frmSchedulingControl : Form
    {
        StringBuilder _strNotScheduled = new StringBuilder();
        private Scheduler _scheduler;
        private Listener _listner;
        private Dictionary<SchedulingData, ServiceInstance> _scheduledServices = new Dictionary<SchedulingData, ServiceInstance>();
        public delegate void SetLogMethod(string lineText);
        public delegate void UpdateGridMethod(legacy.ServiceInstance serviceInstance);
        SetLogMethod setLogMethod;
        UpdateGridMethod updateGridMethod;
        bool _scheduleStarted = false;

        public frmSchedulingControl()
        {
            try
            {
                InitializeComponent();

                setLogMethod = new SetLogMethod(SetLogTextBox);
                updateGridMethod = new UpdateGridMethod(UpdateGridData);
                this.FormClosed += new FormClosedEventHandler(frmSchedulingControl_FormClosed);


            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        void frmSchedulingControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {

                _scheduler.Stop();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    this.Text = System.AppDomain.CurrentDomain.FriendlyName;
                    Thread t = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(new TimeSpan(0,1,0));
                if (!_scheduleStarted)
                {
                    _scheduler.Start();
                }
            }));
                    t.Start();


                }
                catch (Exception)
                {

                    throw;
                }
                _scheduler = new Scheduler(true);
                _listner = new Listener(_scheduler);
                _listner.Start();

                _scheduler.ServiceRunRequiredEvent += new EventHandler(_scheduler_ServiceRunRequiredEvent);
                _scheduler.NewScheduleCreatedEvent += new EventHandler(_scheduler_NewScheduleCreatedEventHandler);
                _scheduler.NotRunServices += new EventHandler(_scheduler_NotRunServices);
                //	_scheduler.Start();
                //test

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        void _scheduler_NotRunServices(object sender, EventArgs e)
        {
            WillNotRunEventArgs ev = (WillNotRunEventArgs)e;
            foreach (SchedulingData notRunService in ev.WillNotRun)
            {
                this.Invoke(setLogMethod, new object[] { string.Format("Service: {0} for account: {1} will not run at all!!!!!!!!", notRunService.Configuration.Name, notRunService.profileID) });
            }

        }

        void _scheduler_ServiceRunRequiredEvent(object sender, EventArgs e)
        {
            //todo: log2
            try
            {

                TimeToRunEventArgs args = (TimeToRunEventArgs)e;
                foreach (MyScheduler.Objects.ServiceInstance serviceInstance in args.ServicesToRun)
                {

                    this.Invoke(setLogMethod, new Object[] { string.Format("Service: {0} is required for running time {1}\r\n", serviceInstance.ServiceName, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

                    //FURTURE: ask system control if it's ok to run the service
                    ////if ok then
                    serviceInstance.LegacyInstance.StateChanged += new EventHandler<Easynet.Edge.Core.Services.ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
                    serviceInstance.LegacyInstance.ChildServiceRequested += new EventHandler<Easynet.Edge.Core.Services.ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
                    serviceInstance.LegacyInstance.Initialize();
                    this.Invoke(setLogMethod, new Object[] { string.Format("\nService: {0} initalized {1}\r\n", serviceInstance.ServiceName, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

                    Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", string.Format("Service: {0} initalized", serviceInstance.ServiceName), Easynet.Edge.Core.Utilities.LogMessageType.Information);


                }
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        void LegacyInstance_ChildServiceRequested(object sender, Easynet.Edge.Core.Services.ServiceRequestedEventArgs e)
        {
            try
            {
                legacy.ServiceInstance instance = (legacy.ServiceInstance)sender;
                this.Invoke(setLogMethod, new Object[] { string.Format("\nChild Service: {0} requestedd {1}\r\n", e.RequestedService.Configuration.Name, DateTime.Now.ToString("dd/MM/yy HH:mm")) });

                e.RequestedService.ChildServiceRequested += new EventHandler<legacy.ServiceRequestedEventArgs>(LegacyInstance_ChildServiceRequested);
                e.RequestedService.StateChanged += new EventHandler<legacy.ServiceStateChangedEventArgs>(LegacyInstance_StateChanged);
                e.RequestedService.Initialize();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }



        void LegacyInstance_StateChanged(object sender, Easynet.Edge.Core.Services.ServiceStateChangedEventArgs e)
        {
            try
            {
                legacy.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
                instance.OutcomeReported += new EventHandler(instance_OutcomeReported);
                this.Invoke(updateGridMethod, new Object[] { instance });


                this.Invoke(setLogMethod, new Object[] { string.Format("\n{0}: {1} is {2} {3}\r\n", instance.AccountID, instance.Configuration.Name, e.StateAfter, DateTime.Now.ToString("dd/MM/yy HH:mm")) });
                if (e.StateAfter == legacy.ServiceState.Ready)
                    instance.Start();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        void instance_OutcomeReported(object sender, EventArgs e)
        {
            legacy.ServiceInstance instance = (Easynet.Edge.Core.Services.ServiceInstance)sender;
            this.Invoke(updateGridMethod, new Object[] { instance });

        }

        void _scheduler_NewScheduleCreatedEventHandler(object sender, EventArgs e)
        {
            try
            {
                this.Invoke(setLogMethod, new Object[] { "Schedule Created:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n" });
                ScheduledInformationEventArgs ee = (ScheduledInformationEventArgs)e;
                _scheduledServices.Clear();
                _strNotScheduled.Clear();



                foreach (KeyValuePair<SchedulingData, ServiceInstance> notSchedInfo in ee.NotScheduledInformation)
                    _strNotScheduled.AppendLine(string.Format("Service {0} with profile {1} not scheduled", notSchedInfo.Value.ServiceName, notSchedInfo.Key.profileID));

                foreach (KeyValuePair<SchedulingData, ServiceInstance> SchedInfo in ee.ScheduleInformation)
                    _scheduledServices.Add(SchedInfo.Key, SchedInfo.Value);

                GetScheduleServices();

                if (!string.IsNullOrEmpty(_strNotScheduled.ToString()))
                    try
                    {
                        Exception notScheduleException = new Exception(string.Format("Some servies could not be schedule:\n{0}", _strNotScheduled.ToString()));
                        //Easynet.Edge.Core.Utilities.Log.Write("Scheduler", "Some services could not be scheduled", null, Easynet.Edge.Core.Utilities.LogMessageType.Information);

                        this.Invoke(setLogMethod, new Object[] { _strNotScheduled.ToString() });
                    }
                    catch (Exception)
                    {


                    }
            }

            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        private void ScheduleBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _strNotScheduled.Clear();
                _scheduledServices.Clear();
                _scheduler.NewSchedule();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }


        }

        private void GetScheduleServices()
        {
            try
            {
                var scheduledServices = _scheduler.GetAlllScheduldServices();
                this.Invoke(new Action<IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>>>(SetGridData), scheduledServices);
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }


        private void UpdateGridData(legacy.ServiceInstance serviceInstance)
        {
            try
            {
                foreach (DataGridViewRow row in scheduleInfoGrid.Rows)
                {
                    if (Object.Equals(row.Tag, serviceInstance))
                    {
                        row.Cells["dynamicStaus"].Value = serviceInstance.State;
                        row.Cells["outCome"].Value = serviceInstance.Outcome;
                        row.Cells["actualEndTime"].Value = serviceInstance.TimeEnded.ToString("dd/MM/yyyy HH:mm:ss");

                        Color color = GetColorByState(serviceInstance.State, serviceInstance.Outcome);
                        row.DefaultCellStyle.BackColor = color;
                    }
                }
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        private static Color GetColorByState(legacy.ServiceState state, legacy.ServiceOutcome outCome, bool deleted = false)
        {
            Color color = Color.White;
            try
            {
                if (deleted != true)
                {
                    switch (state)
                    {

                        case Easynet.Edge.Core.Services.ServiceState.Uninitialized:
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Initializing:
                            color = Color.FromArgb(0xdd, 0xdd, 0xdd); // light gray
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Ready:
                            color = Color.FromArgb(0xee, 0xee, 0xff); // light blue
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Starting:
                            color = Color.Green; // green
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Running:
                            color = Color.FromArgb(0xe0, 0xff, 0xe0); // light green
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Waiting:
                            color = Color.FromArgb(0xee, 0xff, 0xee); // light green (a little lighter)
                            break;
                        case Easynet.Edge.Core.Services.ServiceState.Ended:
                            {
                                if (outCome == legacy.ServiceOutcome.Success)
                                    color = Color.Turquoise;
                                else if (outCome == legacy.ServiceOutcome.Failure)
                                    color = Color.DarkRed;
                                else if (outCome == legacy.ServiceOutcome.Unspecified)
                                    color = Color.Orange;
                                else if (outCome == legacy.ServiceOutcome.Aborted)
                                    color = Color.Purple;
                                break;

                            }

                        case Easynet.Edge.Core.Services.ServiceState.Aborting:
                            color = Color.Red;
                            break;
                        default:
                            break;
                    }
                }
                else
                    color = Color.DarkGray;


            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("GetColorByState", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
            return color;
        }

        private void SetGridData(IEnumerable<KeyValuePair<MyScheduler.Objects.SchedulingData, ServiceInstance>> scheduledServices)
        {
            try
            {

                scheduleInfoGrid.Rows.Clear();
                foreach (var scheduledService in scheduledServices.OrderBy(s => s.Value.StartTime))
                {

                    if (!_scheduledServices.ContainsKey(scheduledService.Key))
                        _scheduledServices.Add(scheduledService.Key, scheduledService.Value);
                    int row = scheduleInfoGrid.Rows.Add(new object[] 
                    { scheduledService.Key.GetHashCode(), scheduledService.Value.ServiceName, scheduledService.Value.ProfileID, 
                        scheduledService.Value.StartTime.ToString("dd/MM/yyy HH:mm:ss"), scheduledService.Value.EndTime.ToString("dd/MM/yyy HH:mm:ss"),
                        scheduledService.Value.LegacyInstance.TimeEnded.ToString("dd/MM/yyy HH:mm:ss"), scheduledService.Value.LegacyInstance.State,
                        scheduledService.Key.Rule.Scope, scheduledService.Value.Deleted, scheduledService.Value.LegacyInstance.Outcome,
                        scheduledService.Value.LegacyInstance.State, scheduledService.Value.Priority ,
                        scheduledService.Value.LegacyInstance.Configuration.Options.ContainsKey("Date")?scheduledService.Value.LegacyInstance.Configuration.Options["Date"] : string.Empty
                    });
                    scheduleInfoGrid.Rows[row].DefaultCellStyle.BackColor = GetColorByState(scheduledService.Value.LegacyInstance.State, scheduledService.Value.LegacyInstance.Outcome, scheduledService.Value.Deleted);
                    scheduleInfoGrid.Rows[row].Tag = scheduledService.Value.LegacyInstance;

                }


            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void endServiceBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to abort service?", "Abort Service", MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in scheduleInfoGrid.SelectedRows)
                    {
                        var scheduleData = from s in _scheduledServices
                                           where s.Key.GetHashCode() == Convert.ToInt32(row.Cells["shceduledID"].Value)
                                           select s.Key;

                        try
                        {
                            _scheduler.AbortRuningService(scheduleData.First());
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show(string.Format("You cannot delete service in this state\n{0}", ex.Message));
                        }

                    }

                    GetScheduleServices();
                }
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        private void ReSchedule()
        {
            try
            {
                _scheduler.ReSchedule();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void getServicesButton_Click(object sender, EventArgs e)
        {
            try
            {
                GetScheduleServices();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void rescheduleBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _strNotScheduled.Clear();
                _scheduledServices.Clear();
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigurationManager.RefreshSection("edge.services");
            frmUnPlannedService f = new frmUnPlannedService(_listner, _scheduler);
            f.Show();
        }

        private void AddUnplanedServiceConfiguration()
        {

        }

        private void deleteServiceFromScheduleBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to Delete service from schedule?", "Delete Service", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in scheduleInfoGrid.SelectedRows)
                    {
                        var scheduleData = from s in _scheduledServices
                                           where s.Key.GetHashCode() == Convert.ToInt32(row.Cells["shceduledID"].Value)
                                           select s.Key;
                        if (_scheduledServices[scheduleData.First()].LegacyInstance.State == Easynet.Edge.Core.Services.ServiceState.Uninitialized)
                        {
                            SchedulingData schedulingDate = scheduleData.First();
                            _scheduler.DeleteScpecificServiceInstance(schedulingDate);
                            row.Cells["deleted"].Value = true;
                            row.DefaultCellStyle.BackColor = GetColorByState(legacy.ServiceState.Aborting, legacy.ServiceOutcome.Aborted, true);


                        }
                        else
                            MessageBox.Show(string.Format("You can't delete service instance with state {0}", _scheduledServices[scheduleData.First()].LegacyInstance.State));

                    }
                    //GetScheduleServices(); 

                }
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }

        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _scheduleStarted = true;
                ConfigurationManager.RefreshSection("edge.services");
                _scheduler.Start();

                logtextBox.Text = logtextBox.Text.Insert(0, "Timer Started:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\n");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void EndBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _scheduler.Stop();
                this.Invoke(setLogMethod, new Object[] { "Timer Stoped" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "r\n" });


            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void SetLogTextBox(string lineText)
        {
            try
            {
                lock (logtextBox)
                {
                    logtextBox.Text = logtextBox.Text.Insert(0, lineText);
                }
            }
            catch (Exception ex)
            {

                Easynet.Edge.Core.Utilities.Log.Write("SchedulingControlForm", ex.Message, ex, Easynet.Edge.Core.Utilities.LogMessageType.Error);
            }
        }

        private void frmSchedulingControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("YOU MUST NOT CLOSE THIS FORM, IF YOU HAVE TO,\nPLEASE TALK WITH SHAY BAR-CHEN OR AMIT BLUMAN,\nARE YOU SURE YOU WANT TO CLOSE THE SCHEDULER?", "Form Closing!", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                result = MessageBox.Show("Are You Sure?", "Form Closing!", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    e.Cancel = false;
                else
                    e.Cancel = true;

            }
            else
                e.Cancel = true;

        }





    }
}
