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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Easynet.Edge.Core.Workflow
{

    [ServiceContract]
    public interface IAlertEngine
    {
        [OperationContract(IsOneWay=true)]
        void RunExistingFlow(int id, Hashtable parameters, Hashtable conditionValues);

        [OperationContract(IsOneWay = true)]
        void RunNewFlow(Type wfType, Hashtable parameters, Hashtable conditionValues);

        [OperationContract(IsOneWay = true)]
        void RunXomlFlow(string xomlFile, Hashtable parameters, Hashtable conditionValues);

        [OperationContract(IsOneWay = true)]
        void RunCompiledFlow(string dllFile, Hashtable parameters, Hashtable conditionValues);
    }

    [ServiceContract]
    public interface IAlertPreprocessor
    {
        [OperationContract(IsOneWay = true)]
        void Process();

        [OperationContract(IsOneWay=true)]
        void Process(int accountID, DateTime from, DateTime to);
    }



    /// <summary>
    /// Represents a sequential workflow which allows the programmer to host activities in.
    /// Provides the service of adding parameters into it.
    /// </summary>
    public class BaseSequentialWorkflow : SequentialWorkflowActivity
    {

        #region Members
        private Hashtable _parameters = new Hashtable();
        private Hashtable _conditionValues = new Hashtable();
        private Hashtable _internalParameters = new Hashtable(); //Used to pass information between activities.
        private int _workflowID = -1;
        private string _workflowName = String.Empty;
        private bool _template = false;
        private Type _workflowType = typeof(BaseSequentialWorkflow);
        private string _connectionString = String.Empty;
        private WorkflowConditionValues _wfConditionValues = new WorkflowConditionValues();
        private WorkflowParameters _wfParameters = new WorkflowParameters();
        private Guid _instanceID = Guid.Empty;
        #endregion

        #region Properties
        public Hashtable Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }

        public Hashtable ConditionValues
        {
            get
            {
                return _conditionValues;
            }
            set
            {
                _conditionValues = value;
            }
        }

        public int WorkflowID
        {
            get
            {
                return _workflowID;
            }
            set
            {
                _workflowID = value;
            }
        }

        public Guid WorkflowGUID
        {
            get
            {
                return Guid.NewGuid();
/*                try
                {
                    Guid g = this.WorkflowInstanceId;
                    return g;
                }
                catch (Exception ex)
                {
                    Guid g = new Guid();
                    return g;
                }*/
            }
            set
            {
                _instanceID = value;
            }
        }

        public string WorkflowName
        {
            get
            {
                return _workflowName;
            }
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _workflowName = value;
            }
        }

        public bool Template
        {
            get
            {
                return _template;
            }
            set
            {
                _template = value;
            }
        }

        public Type WorkflowType
        {
            get
            {
                return _workflowType;
            }
            set
            {
                if (value != null)
                    _workflowType = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _connectionString = value;
            }
        }

        public Hashtable InternalParameters
        {
            get
            {
                return _internalParameters;
            }
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Serializes the workflow class into the database.
        /// </summary>
        public virtual void Serialize()
        {
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            try
            {
                DataManager.ConnectionString = _connectionString;
                bool bFound = false;

                //Serialize the workflow into the database
                using (DataManager.Current.OpenConnection())
                {
                    string sql = "SELECT * FROM Workflows WHERE WorkflowID = " + this.WorkflowID.ToString();

                    //First see if we already exist in the database.
                    SqlCommand cmd = DataManager.CreateCommand(sql);
                    SqlDataReader sr = cmd.ExecuteReader();
                    if (sr.HasRows)
                        bFound = true;

                    sr.Close();
                    sr.Dispose();

                    string wfSql = String.Empty;
                    if (bFound)
                    {
                        Update();
                    }
                    else
                    {
                        Insert();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Failed to serialize workflow into database.", ex);
                throw new Exception("Failed to serialize. Exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Deserializes the class from the database.
        /// </summary>
        public virtual void Deserialize()
        {
            //Fill ourselves based on our internal ID.
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            try
            {
                DataManager.ConnectionString = _connectionString;
                using (DataManager.Current.OpenConnection())
                {
                    string sql = "SELECT * FROM Workflows WHERE WorkflowID = " + this.WorkflowID.ToString();

                    //First see if we already exist in the database.
                    SqlCommand cmd = DataManager.CreateCommand(sql);
                    SqlDataReader sr = cmd.ExecuteReader();
                    if (!sr.HasRows)
                        throw new Exception("Could not locate a workflow with ID: " + _workflowID.ToString());

                    while (sr.Read())
                    {
                        if (sr.IsDBNull(sr.GetOrdinal("WorkflowID")))
                            throw new Exception("Invalid workflow ID. Cannot be NULL.");

                        if (sr.IsDBNull(sr.GetOrdinal("WorkflowGUID")))
                            throw new Exception("Invalid workflow GUID. Cannot be NULL.");

                        if (sr.IsDBNull(sr.GetOrdinal("WorkflowType")))
                            throw new Exception("Invalid workflow type. Cannot be NULL.");

                        if (sr.IsDBNull(sr.GetOrdinal("Template")))
                            throw new Exception("Invalid template column. Cannot be NULL.");

                        if (!sr.IsDBNull(sr.GetOrdinal("WorkflowName")))
                            _workflowName = sr["WorkflowName"].ToString();

                        _workflowType = Type.GetType(sr["WorkflowType"].ToString());
                        _template = Convert.ToBoolean(sr["Template"]);
                    }

                    sr.Close();
                    sr.Dispose();
                }

                //Now load the conditions and parameters (if we have any)
                _wfParameters.ConnectionString = _connectionString;
                _parameters = _wfParameters.Get(_workflowID);

                _wfConditionValues.ConnectionString = _connectionString;
                _conditionValues = _wfConditionValues.Get(_workflowID);
            }
            catch (Exception ex)
            {
                Log.Write("Failed to de-serialize workflow (ID: " + _workflowID.ToString() + ") from database.", ex);
                throw new Exception("Failed to de-serialize workflow (ID: " + _workflowID.ToString() + ") from database. Exception: " + ex.ToString());
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Appends the parameter list argument into our internal collection. Please note
        /// that if a specific parameter is already in our internal collection, the parameter
        /// in the internal collection is overridden.
        /// </summary>
        /// <param name="parameters">The parameters to append</param>
        public void AppendParameters(Hashtable parameters)
        {
            //Nothing to do.
            if (parameters == null ||
                parameters.Count <= 0)
                return;

            IDictionaryEnumerator ide = parameters.GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                if (_parameters.ContainsKey(ide.Key))
                {
                    //We already have a parameter like that. This means that we override
                    //what we have internally.
                    _parameters[ide.Key] = ide.Value;
                }
                else
                {
                    //Didn't find it, just add it.
                    _parameters.Add(ide.Key, ide.Value);
                }
            }
        }

        /// <summary>
        /// Appends the condition values argument into our internal collection. Please note
        /// that if a specific condition value already exists in our internal collection, that
        /// condition value is overridden.
        /// </summary>
        /// <param name="values">The condition values to append</param>
        public void AppendConditionValues(Hashtable values)
        {
            //Nothing to do.
            if (values == null ||
                values.Count <= 0)
                return;

            IDictionaryEnumerator ide = values.GetEnumerator();
            ide.Reset();

            while (ide.MoveNext())
            {
                if (_conditionValues.ContainsKey(ide.Key))
                {
                    //We already have a condition value like that. This means that we override
                    //what we have internally.
                    _conditionValues[ide.Key] = ide.Value;
                }
                else
                {
                    //Didn't find it, just add it.
                    _conditionValues.Add(ide.Key, ide.Value);
                }
            }
        }

        /// <summary>
        /// Returns the condition values as a settings string.
        /// </summary>
        /// <returns>String</returns>
        public string GetConditionValuesAsString()
        {
            string ret = String.Empty;
            IDictionaryEnumerator ide = _parameters.GetEnumerator();
            while (ide.MoveNext())
            {
                if (ret == String.Empty)
                    ret = ide.Key.ToString() + ":" + ide.Value.ToString();
                else
                    ret += ";" + ide.Key.ToString() + ":" + ide.Value.ToString();
            }

            return ret;
        }

        /// <summary>
        /// Returns the parameters as a setting string.
        /// </summary>
        /// <returns>String</returns>
        public string GetParametersAsString()
        {
            string ret = String.Empty;
            IDictionaryEnumerator ide = _conditionValues.GetEnumerator();
            while (ide.MoveNext())
            {
                if (ret == String.Empty)
                    ret = ide.Key.ToString() + ":" + ide.Value.ToString();
                else
                    ret += ";" + ide.Key.ToString() + ":" + ide.Value.ToString();
            }

            return ret;
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Updates the workflow instance in the database (in case we change the name or
        /// something). Also updates the condition values and parameters. 
        /// </summary>
        protected void Update()
        {
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            //Update the current workflow, condition values and parameters.
            DataManager.ConnectionString = _connectionString;

            using (DataManager.Current.OpenConnection())
            {
                int temp = 0;
                if (_template)
                    temp = 1;

                //Workflows table.
                string sql = "UPDATE Workflows SET WorkflowName = '" + _workflowName + "', WorkflowType = '" + _workflowType.ToString() + "', Template = " + temp;
                sql += " WHERE WorkflowID = " + _workflowID.ToString() + " AND WorkflowGUID = '" + this.WorkflowGUID.ToString() + "'";

                SqlCommand cmd = DataManager.CreateCommand(sql);
                cmd.ExecuteNonQuery();

                _wfParameters.WorkflowID = this.WorkflowID;
                _wfParameters.ConnectionString = _connectionString;
                _wfParameters.Update(_parameters);

                _wfConditionValues.WorkflowID = this.WorkflowID;
                _wfConditionValues.ConnectionString = _connectionString;
                _wfConditionValues.Update(_conditionValues);
            }
        }

        /// <summary>
        /// Inserts a new entry of this workflow into the database. Also inserts the data
        /// related to condition and parameters.
        /// </summary>
        protected void Insert()
        {
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            //Update the current workflow, condition values and parameters.
            DataManager.ConnectionString = _connectionString;

            using (DataManager.Current.OpenConnection())
            {
                int temp = 0;
                if (_template)
                    temp = 1;

                string sql = "INSERT INTO Workflows (WorkflowGUID,WorkflowName,WorkflowType,Template) ";
                sql += " VALUES('" + WorkflowGUID.ToString() + "','" + WorkflowName + "','" + WorkflowType.AssemblyQualifiedName + "'," + temp.ToString() + ")";
                sql += "  SET @Ident = SCOPE_IDENTITY()";

                SqlCommand cmd = DataManager.CreateCommand(sql, System.Data.CommandType.Text);

                SqlParameter identParam = new SqlParameter("@Ident", SqlDbType.Int);
                identParam.Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(identParam);

                cmd.ExecuteNonQuery();
                _workflowID = Convert.ToInt32(identParam.Value);

                _wfConditionValues.WorkflowID = WorkflowID;
                _wfConditionValues.ConnectionString = _connectionString;
                _wfConditionValues.Insert(_conditionValues);

                _wfParameters.WorkflowID = WorkflowID;
                _wfParameters.ConnectionString = _connectionString;
                _wfParameters.Insert(_parameters);
            }
        }
        #endregion

    }

    /// <summary>
    /// Represents a single activity in the platform.
    /// </summary>
    public class BaseActivity : Activity
    {

        #region Members
        /// <summary>
        /// Points to the parent workflow where the specific activity is located.
        /// </summary>
        private BaseSequentialWorkflow _parentWorkflow = null;
        #endregion

        #region Properties
        public BaseSequentialWorkflow ParentWorkflow
        {
            get
            {
                if (_parentWorkflow != null)
                    return _parentWorkflow;
                else
                {
                    _parentWorkflow = GetParentSequentialWorkflow();
                    return _parentWorkflow;
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Returns the sequential workflow in which this activity is hosted.
        /// </summary>
        /// <returns>EdgeSequentialWorkflow class</returns>
        protected BaseSequentialWorkflow GetParentSequentialWorkflow()
        {
            if (_parentWorkflow != null)
                return _parentWorkflow;

            BaseSequentialWorkflow ret = null;
            CompositeActivity a = this.Parent;
            while (a.Parent != null)
            {
                a = a.Parent;
            }

            ret = (BaseSequentialWorkflow)a;
            return ret;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the value related to a specific condition name.
        /// </summary>
        /// <param name="conditionName">The name of the condition</param>
        /// <returns>The value of the condition (needs to be converted to the "real" type in the condition)</returns>
        public object GetConditionValue(string conditionName)
        {
            BaseSequentialWorkflow esw = GetParentSequentialWorkflow();

            if (esw == null)
                return null;

            if (esw.ConditionValues.ContainsKey(conditionName))
                return esw.ConditionValues[conditionName];
            else
                return null;
        }
        #endregion

    }

    [Serializable]
    public class BaseActivityDesigner : ActivityDesigner
    {

        #region Members
        protected BaseActivity _me = null;
        private Rectangle pageRect;
        private Rectangle newRect;
        private Rectangle frameRect;
        protected string _title = String.Empty;
        protected Bitmap _icon = null;
        #endregion

        #region Properties
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

        public Bitmap Icon
        {
            set
            {
                _icon = value;
            }
        }
        #endregion

        #region Overrides
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            _me = (BaseActivity)activity;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (_icon == null)
                throw new Exception("Invalid icon. You must set an icon from the child version of this class.");

            frameRect = new Rectangle(this.Location.X, this.Location.Y, this.Size.Width - 5, this.Size.Height - 5);
            Rectangle shadowRect = new Rectangle(frameRect.X + 5, frameRect.Y + 5, frameRect.Width, frameRect.Height);
            this.pageRect = new Rectangle(frameRect.X + 4, frameRect.Y + 24, frameRect.Width - 8, frameRect.Height - 28);
            Rectangle titleRect = new Rectangle(frameRect.X + 16, frameRect.Y + 4, frameRect.Width / 2 + 10, 20);

            Brush frameBrush = new LinearGradientBrush(frameRect, Color.MediumSlateBlue, Color.DarkBlue, 45);

            e.Graphics.FillPath(Brushes.LightGray, RoundedRect(shadowRect));
            e.Graphics.FillPath(frameBrush, RoundedRect(frameRect));
            e.Graphics.FillPath(new LinearGradientBrush(pageRect, Color.White, Color.WhiteSmoke, 45), RoundedRect(pageRect));
            e.Graphics.DrawString("", new Font("Ariel", 9), Brushes.White, titleRect);
            frameRect.Inflate(20, 20);

            e.Graphics.DrawString(_title, new Font("Ariel", 9), Brushes.White, new Rectangle(this.pageRect.X + 5, this.pageRect.Y - 20, this.pageRect.Width, this.pageRect.Height));
            e.Graphics.DrawImage(_icon, new Rectangle(pageRect.X + 30, pageRect.Y + 5, 24, 24));
            this.newRect = new Rectangle(pageRect.X + pageRect.Width / 2 + 15, pageRect.Y + 10, 24, 24);
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            return new Size(65, 65);
        }
        #endregion

        #region Private Methods
        private GraphicsPath RoundedRect(Rectangle frame)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 7;
            int diameter = radius * 2;

            Rectangle arc = new Rectangle(frame.Left, frame.Top, diameter, diameter);

            path.AddArc(arc, 180, 90);

            arc.X = frame.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = frame.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = frame.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            return path;
        }
        #endregion

    }


    /// <summary>
    /// Provides database access to the workflow parameters table.
    /// </summary>
    public class WorkflowParameters
    {

        #region Members
        private int _workflowID = -1;
        private string _connectionString = String.Empty;
        #endregion

        #region Properties
        public int WorkflowID
        {
            set
            {
                if (value > 0)
                    _workflowID = value;
            }
            get
            {
                return _workflowID;
            }
        }

        public string ConnectionString
        {
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _connectionString = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the workflow parameters table with the list of parameters passed as an 
        /// argument.
        /// </summary>
        /// <param name="parameters">The prameters collection to update</param>
        public void Update(Hashtable parameters)
        {
            if (parameters == null)
                throw new ArgumentException("Invalid parameters collection. Either it is empty or null");

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            //If we got no parameters (i.e. Parameters.Count = 0) then we need to delete the parameters.
            if (parameters.Count <= 0)
            {
                Delete();
                return;
            }
            else
            {
                //Delete and re-insert everything.
                Delete();
                Insert(parameters);
            }
        }

        /// <summary>
        /// Inserts the workflow parameters passed in the argument into the database.
        /// </summary>
        /// <param name="parameters">The parameters collection to insert</param>
        public void Insert(Hashtable parameters)
        {
            //Can't insert no parameters.
            if (parameters == null ||
                parameters.Count <= 0)
                return;

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;
            using (DataManager.Current.OpenConnection())
            {
                IDictionaryEnumerator ide = parameters.GetEnumerator();
                ide.Reset();

                while (ide.MoveNext())
                {
                    string sql = "INSERT INTO WorkflowParameters (WorkflowID, WorkflowParameterName, WorkflowParameterValue) ";
                    sql += "VALUES(" + WorkflowID.ToString() + ",'" + ide.Key.ToString() + "','" + ide.Value.ToString() + "')";

                    SqlCommand cmd = DataManager.CreateCommand(sql);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes the collection of parameters related to this specific workflow ID from
        /// the database.
        /// </summary>
        public void Delete()
        {
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;
            using (DataManager.Current.OpenConnection())
            {
                string sql = "DELETE WorkflowParameters WHERE WorkflowID = " + WorkflowID.ToString();
                SqlCommand del = DataManager.CreateCommand(sql);
                del.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Returns a hashtable representing the workflow parameters from the database.
        /// </summary>
        /// <param name="workflowID">The Workflow ID</param>
        /// <returns>A hashtable containing the workflow parameters</returns>
        public Hashtable Get(int workflowID)
        {
            if (workflowID <= 0)
                throw new ArgumentException("Invalid workflow ID parameter. Cannot be 0 or less.");

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;

            Hashtable ret = new Hashtable();

            using (DataManager.Current.OpenConnection())
            {
                string sql = "SELECT * FROM WorkflowParameters WHERE WorkflowID = " + workflowID.ToString();
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader sdr = cmd.ExecuteReader();

                int wfParamID = -1;
                string name = String.Empty;
                object val = null;

                while (sdr.Read())
                {
                    if (sdr.IsDBNull(sdr.GetOrdinal("WorkflowParameterID")))
                        throw new Exception("Invalid row. The workflow parameter ID cannot be null.");

                    wfParamID = Convert.ToInt32(sdr["WorkflowParameterID"]);

                    if (sdr.IsDBNull(sdr.GetOrdinal("WorkflowParameterName")))
                        throw new Exception("Invalid row (ID: " + wfParamID.ToString() + "). Workflow parameter name cannot be null.");

                    name = sdr["WorkflowParameterName"].ToString();

                    if (!sdr.IsDBNull(sdr.GetOrdinal("WorkflowParameterValue")))
                        val = sdr["WorkflowParameterValue"];

                    if (!ret.ContainsKey(name))
                        ret.Add(name, val);
                    else
                        throw new Exception("Trying to add parameter: " + name + ". More than once to parameter collection.");
                }

                sdr.Close();
                sdr.Dispose();
            }

            return ret;
        }
        #endregion

    }

    /// <summary>
    /// Provides access to the Workflow Condition Values database table.
    /// </summary>
    public class WorkflowConditionValues
    {

        #region Members
        private int _workflowID = -1;
        private string _connectionString = String.Empty;
        #endregion

        #region Properties
        public int WorkflowID
        {
            set
            {
                if (value > 0)
                    _workflowID = value;
            }
            get
            {
                return _workflowID;
            }
        }

        public string ConnectionString
        {
            set
            {
                if (value != null &&
                    value != String.Empty)
                    _connectionString = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the existing condition values collection into the database.
        /// </summary>
        /// <param name="conditionValues">The condition values to update</param>
        public void Update(Hashtable conditionValues)
        {
            if (conditionValues == null)
                throw new ArgumentException("Invalid condition values collection. Either it is empty or null");

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            //If we got no parameters (i.e. conditionValues.Count = 0) then we need to delete the conditionValues.
            if (conditionValues.Count <= 0)
            {
                Delete();
                return;
            }
            else
            {
                //Delete and re-insert everything.
                Delete();
                Insert(conditionValues);
            }
        }

        /// <summary>
        /// Inserts a new set of condition values into the database.
        /// </summary>
        /// <param name="conditionValues">The condition values to insert.</param>
        public void Insert(Hashtable conditionValues)
        {
            //Can't insert no conditionValues.
            if (conditionValues == null ||
                conditionValues.Count <= 0)
                return;

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;
            using (DataManager.Current.OpenConnection())
            {
                IDictionaryEnumerator ide = conditionValues.GetEnumerator();
                ide.Reset();

                while (ide.MoveNext())
                {
                    string sql = "INSERT INTO WorkflowConditionValues (WorkflowID, WorkflowConditionName, WorkflowConditionValue) ";
                    sql += "VALUES(" + WorkflowID.ToString() + ",'" + ide.Key.ToString() + "','" + ide.Value.ToString() + "')";

                    SqlCommand cmd = DataManager.CreateCommand(sql);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Delets all the condition values for the specific workflow ID.
        /// </summary>
        public void Delete()
        {
            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;
            using (DataManager.Current.OpenConnection())
            {
                string sql = "DELETE WorkflowConditionValues WHERE WorkflowID = " + WorkflowID.ToString();
                SqlCommand del = DataManager.CreateCommand(sql);
                del.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Returns the condition values as a hashtable for the workflow ID stated in the
        /// arguments to the function
        /// </summary>
        /// <param name="workflowID">The workflow ID</param>
        /// <returns>A hashtable containing the condition values</returns>
        public Hashtable Get(int workflowID)
        {
            if (workflowID <= 0)
                throw new ArgumentException("Invalid workflow ID parameter. Cannot be 0 or less.");

            if (_connectionString == String.Empty ||
                _connectionString == null)
                throw new Exception("Invalid connection string. Cannot be empty or null.");

            DataManager.ConnectionString = _connectionString;

            Hashtable ret = new Hashtable();

            using (DataManager.Current.OpenConnection())
            {
                string sql = "SELECT * FROM WorkflowConditionValues WHERE WorkflowID = " + workflowID.ToString();
                SqlCommand cmd = DataManager.CreateCommand(sql);
                SqlDataReader sdr = cmd.ExecuteReader();

                int wfCondID = -1;
                string name = String.Empty;
                object val = null;

                while (sdr.Read())
                {
                    if (sdr.IsDBNull(sdr.GetOrdinal("WorkflowConditionID")))
                        throw new Exception("Invalid row. The workflow condition ID cannot be null.");

                    wfCondID = Convert.ToInt32(sdr["WorkflowConditionID"]);

                    if (sdr.IsDBNull(sdr.GetOrdinal("WorkflowConditionName")))
                        throw new Exception("Invalid row (ID: " + wfCondID.ToString() + "). Workflow condition name cannot be null.");

                    if (sdr.IsDBNull(sdr.GetOrdinal("WorkflowConditionValue")))
                        throw new Exception("Invalid row (ID: " + wfCondID.ToString() + "). Workflow condition value cannot be null.");

                    name = sdr["WorkflowConditionName"].ToString();
                    val = sdr["WorkflowConditionValue"];

                    if (!ret.ContainsKey(name))
                        ret.Add(name, val);
                    else
                        throw new Exception("Trying to add condition: " + name + ". More than once to condition values collection.");
                }

                sdr.Close();
                sdr.Dispose();
            }

            return ret;
        }
        #endregion

    }

    /// <summary>
    /// This class is responsible for writing the flow status into the database.
    /// </summary>
    public class WorkflowStatusManager
    {

        #region Public Methods
        public void UpdateStatus(WorkflowStatusCodes statusCode, BaseSequentialWorkflow bsw, DateTime startTime, string resultInfo)
        {
            DataManager.ConnectionString = bsw.ConnectionString;
            using (DataManager.Current.OpenConnection())
            {
                if (resultInfo.Contains("'"))
                    resultInfo = resultInfo.Replace("'", "");

                string sql = @"INSERT INTO AlertEngineHistory (wf_id,start_run_time,end_run_time,result,result_info) 
                                VALUES(" + bsw.WorkflowID.ToString() + ",'" + startTime.ToString("yyyyMMdd HH:mm:ss") + "','" +
                                         DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "'," + Convert.ToInt32(statusCode) +
                                         ",'" + resultInfo + "')";

                SqlCommand alertHistory = DataManager.CreateCommand(sql);
                alertHistory.ExecuteNonQuery();
            }
        }
        #endregion

    }


    public enum WorkflowStatusCodes
    {
        Success = 1,
        Failed = -1,
    }
}
