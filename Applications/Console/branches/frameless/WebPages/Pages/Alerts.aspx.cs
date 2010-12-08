using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Workflow;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Drawing;
using System.Text;

namespace Easynet.Edge.UI.WebPages
{

    public partial class AlertsPage : PageBase
	{
		#region Definitions

		public class AlertSessionInputs
		{
			public int FlowID = -1;
			public DataTable Flows = null;
			public AlertType AlertType = AlertType.Campaign;
			public DateTime FromDate;
			public DateTime ToDate = DateTime.MinValue;

			public AlertSessionInputs()
			{
				string initialDate = AppSettings.Get(typeof(AlertsPage), "InitialDate", false);
				if (initialDate != null)
					FromDate = DayCode.GenerateDateTime(initialDate);
			}
		}

		public enum AlertType : int
		{
			Campaign = 1,
			Adgroup = 2,
			Keyword = 3,
			Text = 4,
			Gateway = 5
		}

        public const int summarySize = 2;
        public const int TRESH_ORANGE_NUMBER = -50;
        public const int TRESH_GREEN_NUMBER = 100;
        public const int TRESH_RED_NUMBER = -75;

		/// <summary>
		/// Retrieves inputs from the session
		/// </summary>
		public AlertSessionInputs SessionInputs
		{
			get
			{
				AlertSessionInputs inputs = Session["AlertsSessionInputs"] as AlertSessionInputs;
				if (inputs == null)
					Session["AlertsSessionInputs"] = inputs = new AlertSessionInputs();
				return inputs;
			}
		}

		#endregion

		#region Startup

		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit(EventArgs e)
		{
			_flowSelector.SelectedValue = SessionInputs.FlowID.ToString();
			_alertTypeSelector.SelectedValue = ((int)SessionInputs.AlertType).ToString();

			if (!IsPostBack)
				_fromDate.SelectedDate = SessionInputs.FromDate;

			_fromDate.SelectionChanged += new EventHandler((sender, ev) => SessionInputs.FromDate = _fromDate.SelectedDate);

			base.OnInit(e);

			/*
			// Load the details of the alerts workflows
			if (SessionInputs.Flows == null)
			{
				using (DataManager.Current.OpenConnection())
				{
					SqlCommand cmd = DataManager.CreateCommand(@"
						select
							WorkflowID, WorkflowName,Template
						from
							Edge2Alerts.dbo.Workflows 
						order by
							WorkflowID"
					);

					SqlDataAdapter sda = new SqlDataAdapter(cmd);
					SessionInputs.Flows = new DataTable();
					sda.Fill(flows);
				}
			}
			*/
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
		}

      
		//
		protected void _submit_Click(object sender, EventArgs e)
        {
			SessionInputs.FlowID = Int32.Parse(_flowSelector.SelectedValue);
			SessionInputs.FromDate = _fromDate.SelectedDate;
			SessionInputs.ToDate = SessionInputs.FromDate.AddDays(1);
			SessionInputs.AlertType = (AlertType)Int32.Parse(_alertTypeSelector.SelectedValue);

			_baseDateMessage.Text = "";
			_currentDateMessage.Text = "";

			RetrieveData(null, false, SessionInputs.AlertType);

			_reportTitle.Text = "Alert period: " + _flowSelector.SelectedItem.Text;
			_reportTitle.Visible = false;
		}

		#endregion

		#region Data retrieveal

		void RetrieveData(string sortingExpression, bool useSession, AlertType alertType)
        {
            DataView totalsView = useSession ? Session["TotalsView"] as DataView : null;
			DataView summaryView = useSession ? Session["SummaryView"] as DataView : null;

            //=========================
            // RUN THE COMMANDS

			if (totalsView == null)
            {
				DataSet dataSet = new DataSet();
                DataTable totals = new DataTable();
				DataTable summaryTable = new DataTable();
				summaryTable.Columns.Add("Summary");

				using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Easynet.Edge.UI.Data.Properties.Settings.Edge2AlertsConnectionString"].ConnectionString))
				{
					// ............................
					// SP_Alerts_GetAlertData
					
					SqlCommand command = DataManager.CreateCommand("SP_Alerts_GetAlertData(@AlertDate:Char, @accountID:Char, @WFType:Char)", CommandType.StoredProcedure);
					command.Parameters["@AlertDate"].Value = DayCode.ToDayCode(SessionInputs.FromDate);
					command.Parameters["@accountID"].Value = this.AccountID;
					command.Parameters["@WFType"].Value = SessionInputs.FlowID;
					command.Connection = connection;

					SqlDataAdapter adapter = new SqlDataAdapter(command);
					connection.Open();
					
					DataSet tempDataSet = new DataSet();
					adapter.Fill(tempDataSet);

					if (tempDataSet.Tables.Count > 0)
					{
						totals = tempDataSet.Tables[0];

						if (totals.Rows.Count > 0 && _alertTypeSelector.Items[0].Selected == true)
						{
							FillSummaryDataGrid(totals, ref summaryTable, AlertType.Campaign);
							if (summaryTable.Rows.Count > 0)
							{
								DataRow newDataRow = summaryTable.NewRow();
								newDataRow[0] = "Campaigns:";
								summaryTable.Rows.InsertAt(newDataRow, 0);
							}
						} 
					}

					// ............................
					// SP_Alerts_GetAlertDataAdGroups

					command.CommandText = "SP_Alerts_GetAlertDataAdGroups";
					adapter.Fill(dataSet);
				}

				DataTable adGroupsResult = dataSet.Tables.Count > 0 ? dataSet.Tables[0] : new DataTable();
				DataTable gatewaysResult = dataSet.Tables.Count > 1 ? dataSet.Tables[1] : new DataTable();
				DataTable keywordResult = dataSet.Tables.Count > 3 ? dataSet.Tables[3] : new DataTable();
				DataTable adtextsResult = dataSet.Tables.Count > 2 ? dataSet.Tables[2] : new DataTable();

				FillSummaryOrSomethingLikeThat(adGroupsResult, summaryTable, 1, AlertType.Adgroup, "Ad Groups:");
				FillSummaryOrSomethingLikeThat(keywordResult, summaryTable, 2, AlertType.Keyword, "Keywords:");
				FillSummaryOrSomethingLikeThat(adtextsResult, summaryTable, 3, AlertType.Text, "AdText:");
				FillSummaryOrSomethingLikeThat(gatewaysResult, summaryTable, 4, AlertType.Gateway, "Gateways:");

                //Rename headers for campaigns - [sorta-refactored with guesswork by Doron]
				RenameColumnHeaders(totals, 1, 5);
				RenameColumnHeaders(adGroupsResult, 2, 7);
				RenameColumnHeaders(keywordResult, 5, 9);
				RenameColumnHeaders(adtextsResult, 5, 9);
				RenameColumnHeaders(gatewaysResult, 5, 9);

				RemoveCols(totals, new string[] { "MeasurecolumnNames", "measure_id" });
				RemoveCols(adGroupsResult, new string[] { "MeasurecolumnNames", "measure_id", "group_gk" });
				RemoveCols(keywordResult, new string[] { "MeasurecolumnNames", "measure_id", "group_gk", "Keyword_gk" });
				RemoveCols(adtextsResult, new string[] { "MeasurecolumnNames", "measure_id", "group_gk", "Adtext_gk" });
				RemoveCols(gatewaysResult, new string[] { "MeasurecolumnNames", "measure_id", "group_gk", "Gateway_id" });
                
				totalsView = new DataView(totals);
                Session["TotalsView"] = totalsView;

				summaryView = new DataView(summaryTable);
				Session["SummaryView"] = summaryTable;
            }

			//=========================
			
			// apply sorting
            if (sortingExpression != null)
            {
                totalsView.Sort = sortingExpression + " desc";
            }

			// Bind view
			_dataGrid.DataSource = totalsView;
			_dataGrid.DataBind();

			_summaryGrid.DataSource = summaryView;
			_summaryGrid.DataBind();
		}

		#endregion

		#region Noam's bits & pieces

		/// <summary>
		/// Refactored from Noam copy-paste crap.
		/// </summary>
		private static void RemoveCols(DataTable table, string[] colsToRemove)
		{
			foreach (string col in colsToRemove)
				if (table.Columns.Contains(col))
					table.Columns.Remove(col);
		}

		/// <summary>
		/// Refactored from Noam copy-paste crap.
		/// </summary>
		/// <param name="index">Don't know what this parameter is, guessed it from hard-coded values</param>
		/// <param name="offset">Don't know what this parameter is, guessed it from hard-coded values</param>
		private static void RenameColumnHeaders(DataTable table, int index, int offset)
		{
			if (table.Rows.Count > 0)
			{
				int columnsCount = (table.Columns.Count - index) / 5;

				for (int i = 0; i < columnsCount; i++)
				{
					string[] valuesOfColumns = table.Rows[0][(i * 5) + offset].ToString().Split(',');
					table.Columns[(i * 5) + (offset-3)].ColumnName = valuesOfColumns[0];
					table.Columns[(i * 5) + (offset-2)].ColumnName = valuesOfColumns[1];
					table.Columns[(i * 5) + (offset-1)].ColumnName = valuesOfColumns[2];
				}
			}
		}

		/// <summary>
		/// Refactored from Noam copy-paste crap.
		/// </summary>
		private void FillSummaryOrSomethingLikeThat(DataTable inputTable, DataTable summaryTable, int alertTypeIndex, AlertType alertType, string alertName)
		{
			if (inputTable.Rows.Count > 0 && _alertTypeSelector.Items[alertTypeIndex].Selected == true)
			{
				int currentRowsCount = summaryTable.Rows.Count;
				FillSummaryDataGrid(inputTable, ref summaryTable, alertType);

				if (summaryTable.Rows.Count > currentRowsCount)
				{
					DataRow newDataRow = summaryTable.NewRow();
					newDataRow[0] = alertName;
					summaryTable.Rows.InsertAt(newDataRow, currentRowsCount);
				}

			}
		}

		void FillSummaryDataGrid(DataTable in_dataTable, ref DataTable outDataTable, AlertType tableType)
		{

			int offset = 0;
			int counter = 0;
			string str;
			int i = 0;
			if (tableType.Equals(AlertType.Adgroup))
				offset = 2;
			if (tableType.Equals(AlertType.Text) || tableType.Equals(AlertType.Gateway) || tableType.Equals(AlertType.Keyword))
				offset = 4;
			for (i = 0; i < summarySize; i++)
			{

				//  DataSet.Tables(0).Rows(0).Item["FieldName"] == DBNull.Value
				if (i < in_dataTable.Rows.Count)
					if (Convert.ToDouble(in_dataTable.Rows[i][4 + offset]) > TRESH_GREEN_NUMBER)
					{
						DataRow dataRow = outDataTable.NewRow();

						str = "Campaigns: " +
							in_dataTable.Rows[i]["campaign_name"] + " -> "; ;

						if (tableType.Equals(AlertType.Adgroup))
							str += " Adgroup_name :" + in_dataTable.Rows[i]["Adgroup_name"] + " ->";

						if (tableType.Equals(AlertType.Gateway))
							str += " Gateway_name :" + in_dataTable.Rows[i]["Gateway_name"] + " ->";

						if (tableType.Equals(AlertType.Text))
							str += " Adtext_name :" + in_dataTable.Rows[i]["Adtext_name"] + " ->";

						if (tableType.Equals(AlertType.Keyword))
							str += " Keyword_name :" + in_dataTable.Rows[i]["Keyword_name"] + " ->";

						str += in_dataTable.Rows[i][4 + offset] + " % RISE Change in "
							+ in_dataTable.Rows[i][5 + offset].ToString().Split(new char[] { ',' })[0];
						dataRow["Summary"] = str.ToString();
						outDataTable.Rows.Add(dataRow);
						counter++;
					}

			}
			for (int k = in_dataTable.Rows.Count; in_dataTable.Rows.Count - k < summarySize; k--)
			{
				if (k > counter)
				{
					if (in_dataTable.Rows[k - 1] != null)
						if (Convert.ToDouble(in_dataTable.Rows[k - 1][4 + offset]) < TRESH_RED_NUMBER)
						{
							DataRow dataRow = outDataTable.NewRow();
							str = "Campaigns: " +
								in_dataTable.Rows[k - 1]["campaign_name"] + " -> ";

							if (tableType.Equals(AlertType.Adgroup))
								str += " Adgroup_name :" + in_dataTable.Rows[k - 1]["Adgroup_name"] + " ->";

							if (tableType.Equals(AlertType.Gateway))
								str += " Gateway_name :" + in_dataTable.Rows[k - 1]["Gateway_name"] + " ->";

							if (tableType.Equals(AlertType.Text))
								str += " Adtext_name :" + in_dataTable.Rows[k - 1]["Adtext_name"] + " ->";

							if (tableType.Equals(AlertType.Keyword))
								str += " Keyword_name :" + in_dataTable.Rows[k - 1]["Keyword_name"] + " ->";

							str += in_dataTable.Rows[k - 1][4 + offset] + " % DECLINE Change in "
								+ in_dataTable.Rows[k - 1][5 + offset].ToString().Split(new char[] { ',' })[0];
							dataRow["Summary"] = str.ToString();
							outDataTable.Rows.Add(dataRow);
						}

				}
			}
		}
		#endregion

		#region DataGrid events

		// Sorting commands
		protected void _dataGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
		{
			//RetrieveData(e.SortExpression, true, AlertType.Campaign);
		}

		protected void _summaryGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
		{
			//RetrieveData(e.SortExpression, true, AlertType.Campaign);
		}

		protected void _summaryGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (e.Item.Cells[0].Text.Contains("RISE"))
				{
					e.Item.Cells[0].ForeColor = Color.DarkGreen;
				}
				else
				{
					if (e.Item.Cells[0].Text.Contains("DECLINE"))
					{
						e.Item.Cells[0].ForeColor = Color.Red;
					}
					else
					{
						if (e.Item.Cells[0].Text.Contains("Ad Groups:")
							|| e.Item.Cells[0].Text.Contains("Campaigns:")
							|| e.Item.Cells[0].Text.Contains("Keywords:")
							|| e.Item.Cells[0].Text.Contains("Gateways:")
							|| e.Item.Cells[0].Text.Contains("Text:")
							)
						{
							e.Item.Cells[0].ForeColor = Color.White;
							e.Item.Cells[0].BackColor = Color.Gray;
							e.Item.Cells[0].Font.Bold = true;
						}
					}
				}
			}
        }

		protected void _dataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			/*
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView rv = (DataRowView)e.Item.DataItem;

                int columnsCount = e.Item.Cells.Count;
                columnsCount--;

                int precentIndex = 3;//3 for campaigns  
                if (type.Equals(1))//1 for type ad groups
                {
                    columnsCount--;
                    precentIndex = 4; //4 for ad groups  
                }
                else if (type.Equals(2))//2 for type Key words  || gateways || ad text
                {
                    columnsCount = columnsCount - 2;
                    precentIndex = 5; //4 for ad groups  
                }
                columnsCount = columnsCount / 3;
                for (int j = 0; j < columnsCount; j++)
                {
                    Double precent = Convert.ToDouble(rv.Row.ItemArray[j * 3 + precentIndex]);
                    if (precent > TRESH_GREEN_NUMBER)
                    {
                        e.Item.Cells[j * 3 + precentIndex].BackColor = Color.LightGreen;
                    }
                    else if (precent >= TRESH_RED_NUMBER && precent < TRESH_ORANGE_NUMBER)
                    {
                        e.Item.Cells[j * 3 + precentIndex].BackColor = Color.Yellow;
                    }
                    else if (precent <= TRESH_RED_NUMBER)
                    {
                        e.Item.Cells[j * 3 + precentIndex].BackColor = Color.Red;
                    }
                }


                // e.Item.Cells[6].BackColor = Color.Red;
            }
            else if (e.Item.ItemType == ListItemType.Header)
            {
                // e.Item.Cells.Count
                int precentIndex = 3;//3 for campaigns  
                int columnsCount = e.Item.Cells.Count;
                columnsCount--;

                if (type.Equals(1))//1 for type ad groups
                {
                    columnsCount--;
                    precentIndex = 4; //4 for ad groups  
                }

                else if (type.Equals(2) || type.Equals(3) || type.Equals(4))//2 for type key words
                {
                    columnsCount = columnsCount - 2;
                    precentIndex = 5;
                }
            }
			*/
		}
		#endregion

	}
}

