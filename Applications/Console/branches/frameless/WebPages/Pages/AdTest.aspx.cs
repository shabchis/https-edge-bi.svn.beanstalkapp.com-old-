using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Utilities;

namespace Easynet.Edge.UI.WebPages
{
    public class selectedParams
    {
        public DateTime fromDat, toDat;
        public string chan, cmapgn, primMeasure, secondMeasure;
        public List<int> AdgroupsToExclude;

        public selectedParams()
        {
        }

        public selectedParams(DateTime fromDate, DateTime toDate, string channel, string campaign, string primaryMeasure, string secondaryMeasure)
        {
            fromDat = fromDate;
            toDat = toDate;
            chan = channel;
            cmapgn = campaign;
            primMeasure = primaryMeasure;
            secondMeasure = secondaryMeasure;
        }
    }


    public partial class AdTest : System.Web.UI.Page
    {
        private Dictionary<string, string> _primMeasure = new Dictionary<string, string>();
        private Dictionary<string, string> _secondMeasure = new Dictionary<string, string>();
        string _connectionString;
        public string _dbName;
        private int _accountID;
        selectedParams _nSelectParams = new selectedParams();

        #region properties
        public int accountSelectedValue 
        { 
            get 
            {
                return _accountID;
                //int result=0;
                //if (Int32.TryParse(this._accountSelector.SelectedItem.Value,out result))
                //{
                //    return Int32.Parse(this._accountSelector.SelectedItem.Value);
                //}
                //else
                //    return -1;
            } 
        }    
        public int accountCurrentIndex 
        { 
            get 
            {
                return this.ViewState["accountCurrIndex"] == null ? -1 : (int)this.ViewState["accountCurrIndex"]; 
            } 
            set 
            {
                this.ViewState["accountCurrIndex"] = value; 
            } 
        }
        public int channelSelectedValue
        {
            get
            {
                int result = 0;
                if (Int32.TryParse(this._channelSelector.SelectedItem.Value, out result))
                {
                    return Int32.Parse(this._channelSelector.SelectedItem.Value);
                }
                else
                    return -1;
            }
        }
        public int channelCurrentIndex
        {
            get
            {
                return this.ViewState["channelCurrIndex"] == null ? -1 : (int)this.ViewState["channelCurrIndex"];
            }
            set
            {
                this.ViewState["channelCurrIndex"] = value;
            }
        }
        public int campaignSelectedValue
        {
            get
            {
                int result = 0;
                if (Int32.TryParse(this._campaignSelector.SelectedItem.Value, out result))
                {
                    return Int32.Parse(this._campaignSelector.SelectedItem.Value);
                }
                else
                    return -1;
            }
        }
        public int campaignCurrentIndex
        {
            get
            {
                return this.ViewState["campaginCurrIndex"] == null ? -1 : (int)this.ViewState["campaginCurrIndex"];
            }
            set
            {
                this.ViewState["campaginCurrIndex"] = value;
            }
        }
        
        public int accountCurrentIndexForAdg
        {
            get
            {
                return this.ViewState["accountCurrIndexForAdg"] == null ? -1 : (int)this.ViewState["accountCurrIndexForAdg"];
            }
            set
            {
                this.ViewState["accountCurrIndexForAdg"] = value;
            }
        }
        public DateTime fromDateSelectedValue
        {
            get
            {
                    return this._fromDate.SelectedDate.Date;
            }
        }
        public DateTime fromDateCurrentIndex
        {
            get
            {
                return this.Session["fromDateCurrIndex"] == null ? DateTime.Today : (DateTime)this.Session["fromDateCurrIndex"];
            }
            set
            {
                this.Session["fromDateCurrIndex"] = value;
            }
        }
        public DateTime toDateSelectedValue
        {
            get
            {
                return this._fromDate.SelectedDate.Date;
            }
        }
        public DateTime toDateCurrentIndex
        {
            get
            {
                return this.Session["toDateCurrIndex"] == null ? DateTime.Today : (DateTime)this.Session["toDateCurrIndex"];
            }
            set
            {
                this.Session["toDateCurrIndex"] = value;
            }
        }
        #endregion

        protected override void OnInit(EventArgs e)
        {
            this._DataGrid.ItemDataBound += new System.Web.UI.WebControls.DataGridItemEventHandler(this.OnItemDataBound);
            base.OnInit(e);
            //_notAdTestingAdgroup.Checked = true;
            _showOnlyLosingAds.Checked = true;

            _accountID = Convert.ToInt32(Request.QueryString["accountID"]);
            if (Session["adTestClients"] == null)
            {
                DataTable tbl;
                SqlCommand cmd = DataManager.CreateCommand("getAccountList(@IsRankingAccount:Int)", CommandType.StoredProcedure);
                cmd.Parameters["@IsRankingAccount"].Value = 3; //Means only Clients
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);


                lock (DataManager.ConnectionString)
                {
                    tbl = new DataTable();
                    using (DataManager.Current.OpenConnection())
                    {
                        DataManager.Current.AssociateCommands(cmd);
                        adapter.Fill(tbl);
                    }
                }

                Session["adTestClients"] = tbl;
            }

            //_clientSelector.DataSource = (DataTable)Session["adTestClients"];
            //_clientSelector.DataTextField = "client_name";
            //_clientSelector.DataValueField = "client_id";
            //_clientSelector.DataBind();

            // Add account_id for each name.
            //for (int i = 0; i < _clientSelector.Items.Count; ++i)
            //    _clientSelector.Items[i].Text += " (" + _clientSelector.Items[i].Value + ")";

            // Initalize Channels selector
            if (Session["adTestChannels"] == null)
            {
                SqlCommand cmd = DataManager.CreateCommand(@"
			        select Channel_ID, Display_Name from Constant_Channel
		            order by Display_Name"
                    );

                SqlDataAdapter adpater = new SqlDataAdapter(cmd);
                DataTable tbl = new DataTable();

                using (DataManager.Current.OpenConnection())
                {
                    DataManager.Current.AssociateCommands(cmd);
                    adpater.Fill(tbl);
                }

                Session["adTestChannels"] = tbl;
            }

            _channelSelector.DataSource = (DataTable)Session["adTestChannels"];
            _channelSelector.DataTextField = "Display_Name";
            _channelSelector.DataValueField = "Channel_ID";
            _channelSelector.DataBind();
            _channelSelector.Items.Insert(0, new ListItem("All Channels", (-1).ToString()));

            //accountSelectorUpdate(_clientSelector.SelectedValue);
            //_accountSelector.Visible = false;

            if (Session["adTestCampaigns"] == null)
            {
                campaignListUpdate();
            }

            //====================================
            //IsAdTest Legend:
            //0 - False it's not shown at all
            //1 - True shown in both primary and secondary
            //2 - True but shown only in secondary
            //====================================
            if (Session["adTestPrimaryMeasures"] == null)
            {
                SqlCommand cmd = DataManager.CreateCommand(@"select MeasureID, FieldName, displayName, IsBo
                                from Measure where IsAdTest = 1");

                SqlDataAdapter adpater = new SqlDataAdapter(cmd);
                DataTable tbl = new DataTable();
                using (DataManager.Current.OpenConnection())
                {
                    DataManager.Current.AssociateCommands(cmd);
                    adpater.Fill(tbl);
                }

                Session["adTestPrimaryMeasures"] = tbl;
            }
            _primaryMeasure.DataSource = (DataTable)Session["adTestPrimaryMeasures"];
            _primaryMeasure.DataTextField = "displayName";
            _primaryMeasure.DataValueField = "FieldName";
            _primaryMeasure.DataBind();

            //====================================
            //IsAdTest Legend:
            //0 - False it's not shown at all
            //1 - True shown in both primary and secondary
            //2 - True but shown only in secondary
            //====================================
            if (Session["adTestSecondaryMeasures"] == null)
            {
                SqlCommand cmd = DataManager.CreateCommand(@"select MeasureID, FieldName, displayName, IsBo
                                from Measure where IsAdTest in (1,2)");

                SqlDataAdapter adpater = new SqlDataAdapter(cmd);
                DataTable tbl = new DataTable();
                using (DataManager.Current.OpenConnection())
                {
                    DataManager.Current.AssociateCommands(cmd);
                    adpater.Fill(tbl);
                }

                Session["adTestSecondaryMeasures"] = tbl;
            }
            _secondaryMeasure.DataSource = (DataTable)Session["adTestSecondaryMeasures"];
            _secondaryMeasure.DataTextField = "displayName";
            _secondaryMeasure.DataValueField = "FieldName";
            _secondaryMeasure.DataBind();
            _secondaryMeasure.Items.Insert(0, new ListItem("No Measure", "-1"));
            updateMeasureDictionaries();
            losingAdThreshold.Text = "15";

        }

        //private void accountSelectorUpdate(string clientSelected)
        //{
        //    DataTable tbl;
        //    SqlCommand cmd = DataManager.CreateCommand("getAccountList(@IsRankingAccount:Int, @clientId:Int)", CommandType.StoredProcedure);
        //    cmd.Parameters["@IsRankingAccount"].Value = 4; //Means only Children
        //    cmd.Parameters["@clientId"].Value = Int32.Parse(_clientSelector.SelectedValue);
        //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

        //    lock (DataManager.ConnectionString)
        //    {
        //        tbl = new DataTable();
        //        using (DataManager.Current.OpenConnection())
        //        {
        //            DataManager.Current.AssociateCommands(cmd);
        //            adapter.Fill(tbl);
        //        }
        //    }
        //    _accountSelector.DataSource = tbl;
        //    _accountSelector.DataTextField = "account_name";
        //    _accountSelector.DataValueField = "account_id";
        //    _accountSelector.DataBind();

        //    // Add account_id for each name.
        //    for (int i = 0; i < _accountSelector.Items.Count; ++i)
        //        _accountSelector.Items[i].Text += " (" + _accountSelector.Items[i].Value + ")";

        //    if (tbl.Rows.Count > 1)
        //    {
        //        _accountSelector.Visible = true;
        //    }
        //    else
        //        _accountSelector.Visible = false;
        //}

        //protected void _clientSelector_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    accountSelectorUpdate(_clientSelector.SelectedValue);
        //}

        private void updateMeasureDictionaries()
        {
            int i;
            for(i = 0; i < _primaryMeasure.Items.Count; i++)
            {
                _primMeasure.Add(_primaryMeasure.Items[i].Text,_primaryMeasure.Items[i].Value);
            }
            for (i = 0; i < _secondaryMeasure.Items.Count; i++)
            {
                _secondMeasure.Add(_secondaryMeasure.Items[i].Text, _secondaryMeasure.Items[i].Value);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //_accountID = Convert.ToInt32( Request.QueryString["accountID"]);
            SqlConnectionStringBuilder constring = new SqlConnectionStringBuilder(DataManager.ConnectionString);
            _connectionString = constring.ToString();
            if (Session["selectedParams"] == null)
            {
                _toDate.SelectedDate = DateTime.Today;
                _fromDate.SelectedDate = DateTime.Today.AddDays(-7);
                selectedParams sp = new selectedParams();
                sp.toDat = _toDate.SelectedDate;
                sp.fromDat = _fromDate.SelectedDate;
                Session["selectedParams"] = sp;
            }
        }
        private void updateAdgroupListToRemove()
        {
            DateTime fromDate = _fromDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _fromDate.SelectedDate;
            DateTime toDate = _toDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _toDate.SelectedDate;

            int fromDateNewFormat = DayCode.ToDayCode(fromDate);
            //-2 - only for adgroupsToExclude query
            int toDateNewFormat = DayCode.ToDayCode(toDate.AddDays(-2));


            //pop up remove adgroups
            SqlCommand cmd = DataManager.CreateCommand(@"
			select LEFT(UPPER(CH.Channel_name),2)+'.'+ PA.campaign + '/ ' + PA.adgroup as adgroup,PA.adgroup_gk
            from dbo.Paid_API_AllColumns PA 
                    inner join dbo.Constant_Channel CH
                            on CH.Channel_id = PA.Channel_id
			where PA.account_id = @AccountID:Int and (@channelId:Int = -1 or PA.channel_id = @channelId:Int) and (@campaignGk:Int = -1 or PA.campaign_gk = @campaignGk) and PA.day_code = @toDate:Int
            group by Channel_name, PA.campaign ,PA.adgroup , PA.adgroup_gk             
            order by Channel_name desc, PA.campaign, PA.adgroup");

            cmd.Parameters["@AccountID"].Value = _accountID;
            cmd.Parameters["@channelId"].Value = Int32.Parse(_channelSelector.SelectedValue);
            //cmd.Parameters["@fromDate"].Value = fromDateNewFormat;
            cmd.Parameters["@toDate"].Value = toDateNewFormat;
            if (!_campaignSelector.SelectedValue.Equals("None") && !_campaignSelector.SelectedValue.Equals(""))
            {
                cmd.Parameters["@campaignGk"].Value = Int32.Parse(_campaignSelector.SelectedValue);
            }
            else
            {
                cmd.Parameters["@campaignGk"].Value = -1;
            }

            SqlDataAdapter adapater = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();
            using (DataManager.Current.OpenConnection())
            {
                DataManager.Current.AssociateCommands(cmd);
                adapater.Fill(tbl);
            }

            _adgroupList.DataSource = tbl;
            _adgroupList.DataTextField = "adgroup";
            _adgroupList.DataValueField = "adgroup_gk";
            _adgroupList.DataBind();
            _adgroupList.Style.Value = "display:";
            
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.accountCurrentIndex != this.accountSelectedValue)
            {
                campaignListUpdate();
            }
            this.accountCurrentIndex = this.accountSelectedValue;
            BindTooltip(_campaignSelector);
        }

        private void campaignListUpdate()
        {
            string tempChannelCommand = String.Empty;
            if (_channelSelector.SelectedValue.Equals("-1"))
            {
                tempChannelCommand = "";
            }
            else
                tempChannelCommand = " and channel_id = " + _channelSelector.SelectedValue;

            SqlCommand cmd = DataManager.CreateCommand(@"select campaign,Campaign_GK,channel_id from UserProcess_GUI_PaidCampaign WHERE account_id = " + _accountID  + tempChannelCommand + " order by campaign");

            SqlDataAdapter adpater = new SqlDataAdapter(cmd);
            DataTable tbl = new DataTable();
            using (DataManager.Current.OpenConnection())
            {
                DataManager.Current.AssociateCommands(cmd);
                adpater.Fill(tbl);
            }
            
            Session["adTestCampaigns"] = tbl;
            _campaignSelector.DataSource = (DataTable)Session["adTestCampaigns"];
            
            _campaignSelector.DataTextField = "campaign";
            _campaignSelector.DataValueField = "Campaign_GK";
            _campaignSelector.DataBind();

            if (_campaignSelector.Items.Count > 1)
            {
                _campaignSelector.Items.Insert(0, new ListItem("All Campaigns", "-1"));
            }
            else if (_campaignSelector.Items.Count == 0)
                _campaignSelector.Items.Insert(0, new ListItem("No Campaigns", "None"));

            //updating the selected index to be on the second in list - the first one after ALL
            if (_campaignSelector.Items.Count > 1)
                _campaignSelector.SelectedIndex = 1;
            else if (_campaignSelector.Items.Count > 0)
                _campaignSelector.SelectedIndex = 0;
            else
                _campaignSelector.SelectedIndex = -1;

            //adding 2 first letters of channel name to campaign name
            if (_channelSelector.SelectedValue == "ALL")
            {
                for (int i = 0; i < _campaignSelector.Items.Count; i++)
                    _campaignSelector.Items[i].Text += " (" + _channelSelector.Items[i].Text.Substring(0,2) + ")";
            }
        }
        public static void BindTooltip(DropDownList ddl)
        {
            for (int i = 0; i < ddl.Items.Count; i++)
            {
                ddl.Items[i].Attributes.Add("title", ddl.Items[i].Text);
            }
        }

        protected void _channelSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            campaignListUpdate();

            selectedParams sp = new selectedParams();
            if (Session["selectedParams"] != null)
            {
                sp = (selectedParams)Session["selectedParams"];
            }
            sp.chan = _channelSelector.SelectedValue;
            Session["selectedParams"] = sp;
            //Session["lastChannel"] = _channelSelector.SelectedValue;
            
            //updateAdgroupListToRemove();
        }
        protected void _DataGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
        {
            string sortExpression = (string)Session["SortExp"];
            string sortDirection = (string)Session["SortDir"];

            if (sortExpression != e.SortExpression)
            {
                sortExpression = e.SortExpression;
                sortDirection = "asc";
            }
            else
            {
                if (sortDirection == "asc")
                    sortDirection = "desc";
                else
                    sortDirection = "asc";
            }

            Session["SortExp"] = sortExpression;
            Session["SortDir"] = sortDirection;
            if (Session["table"].Equals("adTesting"))
            {
                RetrieveData(sortExpression + " " + sortDirection, true);
            }
            else if (Session["table"].Equals("nonAdTesting"))
            {
                nonAdTestingRetrieveData(sortExpression + " " + sortDirection, true);
            }
        }
        protected void _submit_Click(object sender, EventArgs e)
        {
            selectedParams sparams = new selectedParams(_fromDate.SelectedDate,_toDate.SelectedDate,_channelSelector.SelectedValue,_campaignSelector.SelectedValue,_primaryMeasure.SelectedValue,_secondaryMeasure.SelectedValue);

            sparams.AdgroupsToExclude = new List<int>();
            for (int i = 0; i < _adgroupList.Items.Count; i++)
            {
                if (_adgroupList.Items[i].Selected)
                {
                    sparams.AdgroupsToExclude.Add(Int32.Parse(_adgroupList.Items[i].Value));
                }
            }

            Session["selectedParams"] = sparams;
            Session["table"] = "adTesting";
            if (_AdsType.SelectedValue == "Non ad-testing")
            {
                NonAdTesting();
            }
            RetrieveData(null, false);
        }
        private void activateStoredProcedure(ref DataTable result, bool isNonAdTesting)
        {
            selectedParams sParams = (selectedParams)Session["selectedParams"];
            
            DataTable isBoTablePrim = (DataTable)_primaryMeasure.DataSource;
            DataTable isBoTableSecond = (DataTable)_secondaryMeasure.DataSource;
            SqlCommand cmd;
            string comnd = String.Empty;
            SqlDataAdapter adapter;

            DateTime fromDate = _fromDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _fromDate.SelectedDate;
            DateTime toDate = _toDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _toDate.SelectedDate;
            int fromDateNewFormat = DayCode.ToDayCode(fromDate);
            int toDateNewFormat = DayCode.ToDayCode(toDate);

            if (_toExcludeAdGroups.Checked == true)
            {
            //if (_adgroupList.SelectedIndex >= 0)
            //{
                if (sParams.AdgroupsToExclude != null && sParams.AdgroupsToExclude.Count > 0)
                {
                    //Temporary table for adgroups list to exclude
                    comnd = "CREATE TABLE #adGroupsToExclude(adgroup int)" +
                    "insert into #adGroupsToExclude(adgroup)\n" +
                    "VALUES (" + sParams.AdgroupsToExclude[0] + ")\n";

                    //in case we remove all the adgroups that we marked previously
                    for (int i = 1; i < sParams.AdgroupsToExclude.Count; i++)
                    {
                        comnd += "insert into #adGroupsToExclude(adgroup)\n" +
                                 "VALUES (" + sParams.AdgroupsToExclude[i] + ")\n";
                    }
                }
                else
                {
                    comnd = "CREATE TABLE #adGroupsToExclude(adgroup int)";
                }
            }
            else
            {
                comnd = "CREATE TABLE #adGroupsToExclude(adgroup int)";
            }

            cmd = DataManager.CreateCommand(comnd);
            DataManager.ConnectionString = _connectionString;
            lock (DataManager.ConnectionString)
            {
                using (DataManager.Current.OpenConnection())
                {
                    DataManager.Current.AssociateCommands(cmd);
                    object r = cmd.ExecuteNonQuery();
                    ///////////////////////////////

                    DataManager.CommandTimeout = 60000;
                    //activating the strored procedure
                    cmd = DataManager.CreateCommand("SP_adTest(@primMeasure:Nvarchar, " +
                              "@shownPrimMeasure:Nvarchar, @primMeasureValue:Nvarchar, @wasSecondMeasureSelected:Int, @secondMeasure:Nvarchar, @secondMeasureValue:Nvarchar" +
                              "@shownSecondMeasure:Nvarchar, @toExcludeAdGroups:Nvarchar, @accountId:Nvarchar, @fromDate:Nvarchar, " +
                              "@toDate:Nvarchar, @ch:Nvarchar, @campaignGK:Nvarchar, @isPrimMeasureBO:Int, @isSecondMeasureBO:Int, @isNonAdTesting:Int, @queryType:Int)", CommandType.StoredProcedure);

                    cmd.Parameters["@primMeasure"].Value = _primMeasure[_primaryMeasure.SelectedItem.ToString()];
                    cmd.Parameters["@shownPrimMeasure"].Value = _primaryMeasure.SelectedItem.ToString();
                    cmd.Parameters["@primMeasureValue"].Value = _primMeasure[_primaryMeasure.SelectedItem.ToString()];
                    if (isNonAdTesting)
                    {
                        cmd.Parameters["@isNonAdTesting"].Value = 1;
                    }
                    else
                    {
                        cmd.Parameters["@isNonAdTesting"].Value = 0;
                    }
                    if (!_secondaryMeasure.SelectedItem.Value.Equals("-1"))
                    {
                        cmd.Parameters["@wasSecondMeasureSelected"].Value = 1;
                        cmd.Parameters["@secondMeasure"].Value = _secondMeasure[_secondaryMeasure.SelectedItem.ToString()].ToString();
                        cmd.Parameters["@shownSecondMeasure"].Value = _secondaryMeasure.SelectedItem.ToString();
                        cmd.Parameters["@secondMeasureValue"].Value = _secondaryMeasure.SelectedValue.ToString();
                    }
                    else
                    {
                        //second measuer wasn't selected
                        cmd.Parameters["@wasSecondMeasureSelected"].Value = 0;
                        cmd.Parameters["@secondMeasure"].Value = "";
                        cmd.Parameters["@shownSecondMeasure"].Value = "";
                        cmd.Parameters["@secondMeasureValue"].Value = "";
                    }
                    //if (_toExcludeAdGroups.Checked == true)
                    //if (_adgroupList.SelectedIndex >= 0 && sParams.AdgroupsToExclude != null)
                    //{
                    if (_toExcludeAdGroups.Checked == true && sParams.AdgroupsToExclude != null)
                    {
                        cmd.Parameters["@toExcludeAdGroups"].Value = "1";
                    }
                    else
                    {
                        cmd.Parameters["@toExcludeAdGroups"].Value = "0";
                    }
                    cmd.Parameters["@accountId"].Value = _accountID;
                    cmd.Parameters["@fromDate"].Value = fromDateNewFormat.ToString();
                    cmd.Parameters["@toDate"].Value = toDateNewFormat.ToString();
                    cmd.Parameters["@ch"].Value = _channelSelector.SelectedValue;
                    
                    if (!_campaignSelector.SelectedValue.Equals("None"))
                    {
                        cmd.Parameters["@campaignGK"].Value = _campaignSelector.SelectedValue;
                    }
                    else
                    {
                        cmd.Parameters["@campaignGK"].Value = 0;
                    }

                    if ((bool)isBoTablePrim.Rows[_primaryMeasure.SelectedIndex]["IsBo"])
                    {
                        cmd.Parameters["@isPrimMeasureBO"].Value = 1;
                    }
                    else
                    {
                        cmd.Parameters["@isPrimMeasureBO"].Value = 0;
                    }

                    if (_secondaryMeasure.SelectedIndex == 0)
                    {
                        cmd.Parameters["@isSecondMeasureBO"].Value = 0;
                    }
                    else 
                    {
                        if ((bool)isBoTableSecond.Rows[_secondaryMeasure.SelectedIndex - 1]["IsBo"])
                        {
                            cmd.Parameters["@isSecondMeasureBO"].Value = 1;
                        }
                        else
                        {
                            cmd.Parameters["@isSecondMeasureBO"].Value = 0;
                        }
                    }

                    if (cmd.Parameters["@isSecondMeasureBO"].Value == null && cmd.Parameters["@isPrimMeasureBO"].Value == null)
                    {
                        cmd.Parameters["@queryType"].Value = 1;//means both of measures are not bo
                    }
                    else if (cmd.Parameters["@isSecondMeasureBO"].Value == null && cmd.Parameters["@isPrimMeasureBO"].Value != null)
                    {
                        if ((int)cmd.Parameters["@isPrimMeasureBO"].Value == 1)
                        {
                            cmd.Parameters["@queryType"].Value = 2;//means bo
                        }
                        else
                        {
                            cmd.Parameters["@queryType"].Value = 1;//means both of measures are not bo
                        }
                    }
                    else if (cmd.Parameters["@isPrimMeasureBO"].Value == null && cmd.Parameters["@isSecondMeasureBO"].Value != null)
                    {
                        if ((int)cmd.Parameters["@isSecondMeasureBO"].Value == 1)
                        {
                            cmd.Parameters["@queryType"].Value = 2;//means bo
                        }
                        else
                        {
                            cmd.Parameters["@queryType"].Value = 1;//means both of measures are not bo
                        }
                    }
                    else if ((int)cmd.Parameters["@isSecondMeasureBO"].Value == 1 || (int)cmd.Parameters["@isPrimMeasureBO"].Value == 1)
                    {
                        cmd.Parameters["@queryType"].Value = 2;//means bo
                    }
                    else
                    {
                        cmd.Parameters["@queryType"].Value = 1;//means both of measures are not bo
                    }

                    adapter = new SqlDataAdapter(cmd);

                    DataManager.Current.AssociateCommands(cmd);
                    adapter.Fill(result);

                }
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //string n = _primaryMeasure.SelectedValue;

            //if (Session["lastSelectedFromDate"] != null)
            //    if ((DateTime)Session["lastSelectedFromDate"] != _fromDate.SelectedDate)
            //        _fromDate.SelectedDate = (DateTime)Session["lastSelectedFromDate"];

            //if (Session["lastSelectedToDate"] != null)
            //    if ((DateTime)Session["lastSelectedToDate"] != _toDate.SelectedDate)
            //        _toDate.SelectedDate = (DateTime)Session["lastSelectedToDate"];


            //if (Session["lastChannel"] != null)
            //    if ((string)Session["lastChannel"] != _channelSelector.SelectedValue)
            //        _channelSelector.SelectedValue = (string)Session["lastChannel"];

            //if (Session["lastPrimMeasure"] != null)
            //    if ((string)Session["lastPrimMeasure"] != _primaryMeasure.SelectedValue)
            //        _primaryMeasure.SelectedValue = (string)Session["lastPrimMeasure"];

            //if (Session["lastSecondMeasure"] != null)
            //    if ((string)Session["lastSecondMeasure"] != _secondaryMeasure.SelectedValue)
            //        _secondaryMeasure.SelectedValue = (string)Session["lastSecondMeasure"];


            if (Session["selectedParams"] != null)
            {
                selectedParams sp = new selectedParams();
                sp = (selectedParams)Session["selectedParams"];

                if (sp.fromDat != _fromDate.SelectedDate)
                    _fromDate.SelectedDate = sp.fromDat;
                if (sp.toDat != _toDate.SelectedDate)
                    _toDate.SelectedDate = sp.toDat;
                if (sp.chan != _channelSelector.SelectedValue)
                    _channelSelector.SelectedValue = sp.chan;
                //if (sp.cmapgn != _campaignSelector.SelectedValue)
                //    _campaignSelector.SelectedValue = sp.cmapgn;
                if (sp.primMeasure != _primaryMeasure.SelectedValue)
                    _primaryMeasure.SelectedValue = sp.primMeasure;
                if (sp.secondMeasure != _secondaryMeasure.SelectedValue)
                    _secondaryMeasure.SelectedValue = sp.secondMeasure;
            }
        }
        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            //campaignListUpdate();
        }
        private void RetrieveData(string sortingExpression, bool useSession)
        {
            string ch = String.Empty;
            DataView dataView = useSession ? Session["BindingSource"] as DataView : null;

            Session["isNonAdTesting"] = false;
            DataTable result = new DataTable();

            if (dataView == null)
            {
                activateStoredProcedure(ref result,false);

                if (result.Rows.Count == 0)
                {
                    dataView = new DataView(result);
                    Session["BindingSource"] = dataView;

                    _DataGrid.AllowSorting = true;
                    _DataGrid.DataSource = dataView;
                    _DataGrid.DataBind();
                    return;
                }

                result.Columns.Add("Improvement", Type.GetType("System.String"));
                result.Columns.Add("ChanceToBeatOriginal", Type.GetType("System.String"));
                //result.Columns.Add("Z", Type.GetType("System.Double"));
                result.Columns.Add("LosingOrWinning", Type.GetType("System.String"));
                //calculating new columns values
                int bestAdPlace = 0;
                string camp = result.Rows[bestAdPlace]["Campaign"].ToString();
                string adg = result.Rows[bestAdPlace]["Adgroup"].ToString();
                double z = 0;
                string chanceToBeat = String.Empty;
                int pointPlace = 0;
                result.Rows[0]["LosingOrWinning"] = "winning";
                float floatResult = 0;
                for (int i = 1; i < result.Rows.Count; i++)
                {
                    if (result.Rows[i]["Campaign"].ToString().Equals(camp) && result.Rows[i]["Adgroup"].ToString().Equals(adg))
                    {
                        if (!float.TryParse(result.Rows[bestAdPlace]["ConversionRate"].ToString(),out floatResult))
                        {
                            result.Rows[i]["Improvement"] = "--";
                        }
                        else if ((float)(result.Rows[bestAdPlace]["ConversionRate"]) == 0 || result.Rows[i]["ConversionRate"].ToString().Equals(""))
                        {
                            result.Rows[i]["Improvement"] = "--";
                        }
                        else if ((float)(result.Rows[bestAdPlace]["ConversionRate"]) == 0 || (Int64)(result.Rows[i]["Clicks"]) == 0 || (Int64)(result.Rows[bestAdPlace]["Clicks"]) == 0)
                        {
                            result.Rows[i]["ChanceToBeatOriginal"] = "--";
                        }
                        else
                        {
                            result.Rows[i]["Improvement"] = String.Format("{0:0.00}", (float)(result.Rows[i]["ConversionRate"]) / (float)(result.Rows[bestAdPlace]["ConversionRate"]));
                            //Formula = (p1-p0)/Math.Sqrt(p1*(1-p1)/N1+P0*(1-P0)/N0)
                            z = ((float)(result.Rows[bestAdPlace]["ConversionRate"]) - (float)(result.Rows[i]["ConversionRate"])) / Math.Sqrt((float)(result.Rows[bestAdPlace]["ConversionRate"]) * (1 - (float)(result.Rows[bestAdPlace]["ConversionRate"])) / (Int64)(result.Rows[bestAdPlace]["Clicks"]) + (float)(result.Rows[i]["ConversionRate"]) * (1 - (float)(result.Rows[i]["ConversionRate"])) / (Int64)(result.Rows[i]["Clicks"]));
                            chanceToBeat = (100 - (CNDF(z) * 100)).ToString();
                            pointPlace = chanceToBeat.ToString().IndexOf(".");
                            if (Double.Parse(chanceToBeat) != 0)
                            {
                                result.Rows[i]["ChanceToBeatOriginal"] = chanceToBeat.Substring(0, pointPlace + 3) + "%";
                            }
                            else
                            {
                                result.Rows[i]["ChanceToBeatOriginal"] = chanceToBeat + "%";
                            }
                            //result.Rows[i]["Z"] = z;
                        }
                    }
                    else
                    {
                        camp = result.Rows[i]["Campaign"].ToString();
                        adg = result.Rows[i]["Adgroup"].ToString();
                        bestAdPlace = i;
                        result.Rows[i]["LosingOrWinning"] = "winning";
                    }
                }
                //updating the red color places for the worst "chanceToBeat"
                double worstChance = 0;
                double res;
                if (result.Rows[0]["ChanceToBeatOriginal"].ToString() != "" && result.Rows[0]["ChanceToBeatOriginal"].ToString() != "--")
                {
                    if (Double.TryParse(result.Rows[0]["ChanceToBeatOriginal"].ToString().Substring(0, result.Rows[0]["ChanceToBeatOriginal"].ToString().Length - 1), out res))
                    {
                        worstChance = Double.Parse(result.Rows[0]["ChanceToBeatOriginal"].ToString().Substring(0, result.Rows[0]["ChanceToBeatOriginal"].ToString().Length - 1));
                    }
                }

                double doubleResult = 0;
                int bestAdPos = 0;
                int ctrPointPlace = 0;
                if (!primaryThreshold.Text.Equals("") || !losingAdThreshold.Text.Equals(""))
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        if (result.Rows[i]["LosingOrWinning"].ToString().Equals("winning"))
                        {
                            bestAdPos = i;
                        }
                        //truncating CTR field for 2 places after decimal point
                        ctrPointPlace = result.Rows[i]["CTR"].ToString().IndexOf(".");
                        if (ctrPointPlace != -1)
                            result.Rows[i]["CTR"] = result.Rows[i]["CTR"].ToString().Substring(0, ctrPointPlace + 3);

                        //check if threshold has value and if ChanceToBeatOriginal value is not null or empty or eautl to -- etc.
                        //and if the values are under the threshold limit - remove that row from table
                        if (Double.TryParse(result.Rows[i][_primaryMeasure.SelectedItem.ToString()].ToString(), out doubleResult))
                        {
                            if (!primaryThreshold.Text.Equals("") && Double.Parse(result.Rows[i][_primaryMeasure.SelectedItem.ToString()].ToString()) < Double.Parse(primaryThreshold.Text))
                            {
                                result.Rows.Remove(result.Rows[i]);
                                i--;
                                continue;
                            }
                            if ((!losingAdThreshold.Text.Equals("")) && !result.Rows[i]["ChanceToBeatOriginal"].ToString().Equals(""))
                            {
                                if (Double.TryParse(result.Rows[i]["ChanceToBeatOriginal"].ToString().Substring(0, result.Rows[i]["ChanceToBeatOriginal"].ToString().Length - 1), out doubleResult))
                                {
                                    if (Double.Parse(result.Rows[i]["ChanceToBeatOriginal"].ToString().Substring(0, result.Rows[i]["ChanceToBeatOriginal"].ToString().Length - 1)) < Double.Parse(losingAdThreshold.Text))
                                    {
                                        try
                                        {
                                            if (Double.Parse(result.Rows[i][_primaryMeasure.SelectedItem.ToString()].ToString()) > 4 &&
                                                Double.Parse(result.Rows[bestAdPos][_primaryMeasure.SelectedItem.ToString()].ToString()) > 4)
                                            {
                                                if (_secondaryMeasure.SelectedValue != "-1")
                                                {
                                                    try
                                                    {
                                                        //Secondary-goal-conversion-rate (2nd-goal-value / clicks) of compared version <= 0.95 × best version’s secondary-goal-conversion-rate
                                                        if (Double.Parse(result.Rows[i][_secondaryMeasure.SelectedItem.ToString()].ToString()) / Double.Parse(result.Rows[i]["clicks"].ToString()) <=
                                                            0.95 * (Double.Parse(result.Rows[bestAdPos][_secondaryMeasure.SelectedItem.ToString()].ToString()) / Double.Parse(result.Rows[bestAdPos]["clicks"].ToString())))
                                                        {
                                                            result.Rows[i]["LosingOrWinning"] = "losing";
                                                        }
                                                    }
                                                    catch { }
                                                }
                                                else
                                                {
                                                    result.Rows[i]["LosingOrWinning"] = "losing";
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
                if(_showOnlyLosingAds.Checked)
                    findRedAdgroups(ref result); //for removing the other rows - nonRedAdgroups

                //removing all rows which belong to "only one add"
                //---------------------------------//
                //JUST UNTIL WE FIND BETTER QUERY //
                //-------------------------------//
                if (_AdsType.SelectedValue == "Ad-testing")
                {
                    int counter = 0;
                    for (int i = 0; i < result.Rows.Count - 1; i++)
                    {
                        if (result.Rows[i]["Campaign"].Equals(result.Rows[i + 1]["Campaign"]) && result.Rows[i]["Adgroup"].Equals(result.Rows[i + 1]["Adgroup"]))
                        {
                            counter++;
                        }
                        //in case there is one ad in campaign-adgroup - we'll remove that line from table
                        else
                        {
                            if (counter == 0)
                            {
                                result.Rows.Remove(result.Rows[i]);
                                i--;
                            }
                            else
                                counter = 0;
                        }
                    }
                    //in case no rows rest after removing rows of nonAdTesting
                    if (result.Rows.Count == 0)
                    {
                        dataView = new DataView(result);
                        Session["BindingSource"] = dataView;

                        _DataGrid.AllowSorting = true;
                        _DataGrid.DataSource = dataView;
                        _DataGrid.DataBind();
                        return;
                    }
                    //checking the last row
                    if (counter == 0)
                        result.Rows.Remove(result.Rows[result.Rows.Count - 1]);
                }

                dataView = new DataView(result);
                Session["BindingSource"] = dataView;

            }
            if (sortingExpression != null)
			    dataView.Sort = sortingExpression;

		    //_DataGrid.PreRender += new EventHandler(_DataGrid_PreRender);
            //_DataGrid.ItemDataBound += new DataGridItemEventHandler(_DataGrid_ItemDataBound);
            _DataGrid.AllowSorting = true;
            _DataGrid.DataSource = dataView;
            _DataGrid.DataBind();
            //_DataGrid.Columns[_DataGrid.Columns.Count].Visible = false;
		}
        private double CNDF(double x)
        {
            if (x > 6.0)
                return 1.0;
            if (x < -6.0)
                return 0.0;

            double b1 = 0.31938153;
            double b2 = -0.356563782;
            double b3 = 1.781477937;
            double b4 = -1.821255978;
            double b5 = 1.330274429;
            double p = 0.2316419;
            double c2 = 0.3989423;

            double a = Math.Abs(x);
            double t = 1.0 / (1.0 + a * p);
            double b = c2 * Math.Exp((-x) * (x / 2.0));
            double n = ((((b5 * t + b4) * t + b3) * t + b2) * t + b1) * t;
            n = 1.0 - b * n;

            if (x < 0.0)
                n = 1.0 - n;

            return n;
        }
        private void findRedAdgroups(ref DataTable result)
        {
            string camp = result.Rows[0]["Campaign"].ToString();
            string adg = result.Rows[0]["Adgroup"].ToString();
            int firstPos = 0; //represents the first position of every pair of campaign,adgroup
            int currPos = 0;
            bool foundRed = false;
            int i = 0;
            for (i = 1; i < result.Rows.Count; i++)
            {
                currPos = i;
                if (result.Rows[i]["Campaign"].ToString() == camp && result.Rows[i]["Adgroup"].ToString() == adg)
                {
                    if(result.Rows[i]["LosingOrWinning"].Equals("losing"))
                    {
                        foundRed = true;
                    }
                }
                else
                {
                    if (!foundRed && result.Rows.Count > 0)
                    {
                        removeNonRed(ref result, firstPos, currPos - 1);
                        i -= (currPos - firstPos);
                    }
                    firstPos = i;
                    camp = result.Rows[i]["Campaign"].ToString();
                    adg = result.Rows[i]["Adgroup"].ToString();
                    foundRed = false;
                }
            }
            //for last pair of campaign,adgroup
            if (!foundRed && result.Rows.Count > 0)
            {
                try
                {
                    removeNonRed(ref result, firstPos, currPos - 1);
                    i -= (currPos - firstPos);
                }
                catch { }
            }
        }
        private void removeNonRed(ref DataTable result, int firstPos, int lastPos)
        {
            try
            {
                for (int i = firstPos; i <= lastPos; i++)
                {
                    result.Rows.Remove(result.Rows[i]);
                    i--;
                    lastPos--;
                }
            }
            catch
            {
            }
        }
        protected void NonAdTesting()
        {
            Session["table"] = "nonAdTesting";
            nonAdTestingRetrieveData(null,false);
        }
        private void nonAdTestingRetrieveData(string sortingExpression, bool useSession)
        {
            string ch = String.Empty;
            DataView dataView = useSession ? Session["NonAdTestingBindingSource"] as DataView : null;
            DateTime fromDate = _fromDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _fromDate.SelectedDate;
            DateTime toDate = _toDate.SelectedDate == DateTime.MinValue ? DateTime.Today : _toDate.SelectedDate;

            int fromDateNewFormat = DayCode.ToDayCode(fromDate);
            int toDateNewFormat = DayCode.ToDayCode(toDate);
            string comnd = String.Empty;
            if (dataView == null)
            {
                //nonAdTestingQueryString(ref comnd);

                DataTable result = new DataTable();
                activateStoredProcedure(ref result, true);

                //calculating new columns values
                if (result.Rows.Count == 0)
                {

                    dataView = new DataView(result);
                    Session["NonAdTestingBindingSource"] = dataView;
                    _DataGrid.AllowSorting = true;
                    _DataGrid.DataSource = dataView;
                    _DataGrid.DataBind();
                    return;
                }
                double doubleResult = 0;
                if (!primaryThreshold.Text.Equals(""))
                {
                    for (int i = 0; i < result.Rows.Count - 1; i++)
                    {
                        //check if threshold value inserted and if ChanceToBeatOriginal value is not null or empty or eautl to -- etc.
                        //and if the values are under the threshold limit - remove that row from table
                        if (Double.TryParse(result.Rows[i][_primaryMeasure.SelectedItem.ToString()].ToString(), out doubleResult))
                        {
                            if (!primaryThreshold.Text.Equals("") && Double.Parse(result.Rows[i][_primaryMeasure.SelectedItem.ToString()].ToString()) < Double.Parse(primaryThreshold.Text))
                            {
                                result.Rows.Remove(result.Rows[i]);
                                i--;
                            }
                        }
                    }
                }

                dataView = new DataView(result);
                Session["NonAdTestingBindingSource"] = dataView;
                Session["isNonAdTesting"] = true;
            }
            if (sortingExpression != null)
                dataView.Sort = sortingExpression;

            _DataGrid.AllowSorting = true;
            _DataGrid.DataSource = dataView;
            _DataGrid.DataBind();
        }
        private void OnItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
        {
            if (Session["isNonAdTesting"] != null)
            {
                if (!(bool)Session["isNonAdTesting"])
                {
                    DataRowView row = (DataRowView)e.Item.DataItem;

                    //if (row["LosingOrWinning"].ToString().Equals("winning"))
                    //  e.Item.Cells[3].BackColor = System.Drawing.Color.Green;
                    //else if(row["LosingOrWinning"].ToString().Equals("losing"))
                    if (e.Item.Cells[e.Item.Cells.Count - 1].Text == "losing")
                    {
                        e.Item.Cells[3].CssClass = "losing";
                    }
                    else if (e.Item.Cells[e.Item.Cells.Count - 1].Text == "winning")
                    {
                        e.Item.Cells[3].CssClass = "winning";
                    }
                    e.Item.Cells[e.Item.Cells.Count - 1].Visible = false;
                    //conversionRate culomn
                    e.Item.Cells[e.Item.Cells.Count - 5].Visible = false;
                }
            }
        }
        protected void adgroupChooseButton_Click(object sender, EventArgs e)
        {
            updateAdgroupListToRemove();
            _toExcludeAdGroups.Checked = true;
        }

        protected void _fromDate_SelectionChanged(object sender, EventArgs e)
        {
            selectedParams sp = null;
            if (Session["selectedParams"] != null)
            {
                sp = (selectedParams)Session["selectedParams"];
            }
            else
                sp = new selectedParams();

            sp.fromDat = _fromDate.SelectedDate;
            Session["selectedParams"] = sp;


            //Session["lastSelectedFromDate"] = _fromDate.SelectedDate;
        }

        protected void _toDate_SelectionChanged(object sender, EventArgs e)
        {
            selectedParams sp = new selectedParams();
            if (Session["selectedParams"] != null)
            {
                sp = (selectedParams)Session["selectedParams"];
            }
            sp.toDat = _toDate.SelectedDate;
            Session["selectedParams"] = sp;

            //Session["lastSelectedToDate"] = _toDate.SelectedDate;
        }

        //protected void _primaryMeasure_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    selectedParams sp = new selectedParams();
        //    if (Session["selectedParams"] != null)
        //    {
        //        sp = (selectedParams)Session["selectedParams"];
        //    }
        //    sp.primMeasure = _primaryMeasure.SelectedValue;
        //    Session["selectedParams"] = sp;

        //    //Session["lastPrimMeasure"] = _primaryMeasure.SelectedValue;
        //}

        //protected void _secondaryMeasure_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    selectedParams sp = new selectedParams();
        //    if (Session["selectedParams"] != null)
        //    {
        //        sp = (selectedParams)Session["selectedParams"];
        //    }
        //    sp.secondMeasure = _secondaryMeasure.SelectedValue;
        //    Session["selectedParams"] = sp;

        //    //Session["lastSecondMeasure"] = _secondaryMeasure.SelectedValue;
        //}
        protected void _campaignSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            //selectedParams sp = new selectedParams();
            //if (Session["selectedParams"] != null)
            //{
            //    sp = (selectedParams)Session["selectedParams"];
            //}
            //sp.cmapgn = _campaignSelector.SelectedValue;
            //Session["selectedParams"] = sp;
        }
    }
}
