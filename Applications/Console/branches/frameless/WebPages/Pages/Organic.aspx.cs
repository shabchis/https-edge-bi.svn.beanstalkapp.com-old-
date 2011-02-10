using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
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
using OfficeOpenXml;
using System.IO;
using System.Collections.Specialized;
using System.Web.Configuration;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Ionic.Zip;
using ZedGraph;

namespace Easynet.Edge.UI.WebPages
{
	#region Definitions

	public class RankingTable
	{
		public int SearchEngineID;
		public string SearchEngineName;
		public List<string> Columns = new List<string>();
		public List<RankingRow> Rows = new List<RankingRow>();
	}

	public class RankingRow
	{
		public string SearchEngineUrl;
		public string Keyword;
		public Dictionary<string, RankingItem> Items = new Dictionary<string, RankingItem>();
		public RankingItem ClientItem
		{
			get { return Items.Count > 0 ? Items.First(item => item.Value.IsClient).Value : null; }
		}
	}

	public class RankingItem
	{
		public bool IsClient;
		public string TargetDomain;
		public List<Ranking> Rankings = new List<Ranking>();

	}
	public class Ranking
	{
		public int Rank;
		public int RankDiff;
		public string Url;
	}

	#endregion

	public partial class OrganicPage : PageBase
	{
		// Constants
		class Const
		{
			public static int GraphRangeMax = Int32.Parse(AppSettings.Get(typeof(OrganicPage), "GraphRangeMax"));
		}

		// Fields
		List<int> _dates = new List<int>();
		List<string> _datesKwByTime = new List<string>();
		string[] _datesPerWord;

		protected override void OnInit(EventArgs e)
		{
			_submit.Click += new EventHandler(_submit_Click);
			_export.Click += new EventHandler(_export_Click);
			_exportAll.Click += new EventHandler(_export_Click);
			_exportAllByTime.Click += new EventHandler(_export_Click);
			
			base.OnInit(e);

			// Retrieve profiles on first load of page
			if (!IsPostBack)
			{
				using (DataManager.Current.OpenConnection())
				{
					SqlCommand profileCmd = DataManager.CreateCommand("select Profile_ID, Profile_Name from User_GUI_SerpProfile where Account_ID = @accountID:Int order by Profile_Name");
					profileCmd.Parameters[0].Value = this.AccountID;

					using (SqlDataReader reader = profileCmd.ExecuteReader())
					{
						while (reader.Read())
							_profileSelector.Items.Add(new ListItem(reader[1].ToString(), reader[0].ToString()));
					}
				}
			}
		}

		/// <summary>
		/// Update available report dates.
		/// </summary>
		protected void _profileSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_profileSelector.SelectedIndex < 0)
				return;

			int profileID = Int32.Parse(_profileSelector.SelectedValue);

			_dateSelector.Items.Clear();
			_compareSelector.Items.Clear();

			using (DataManager.Current.OpenConnection())
			{
				SqlCommand dateCmd = DataManager.CreateCommand("select distinct Day_Code from Rankings_Data where Account_ID = @accountID:Int and ProfileID = @profileID:Int order by day_code desc");
				dateCmd.Parameters[0].Value = this.AccountID;
				dateCmd.Parameters[1].Value = profileID;

				using (SqlDataReader reader = dateCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						_dates.Add(Int32.Parse(reader[0].ToString()));
						_dateSelector.Items.Add(new ListItem(DayCode.GenerateDateTime(reader[0]).ToString("dd/MM/yyyy"), reader[0].ToString()));
						_compareSelector.Items.Add(new ListItem(DayCode.GenerateDateTime(reader[0]).ToString("dd/MM/yyyy"), reader[0].ToString()));
					}
				}
			}

			// Auto select the first date
			if (_dateSelector.Items.Count > 0)
				_dateSelector.Items[0].Selected = true;
			
			// Auto select the second date if available, otherwise the first
			if (_compareSelector.Items.Count > 1)
				_compareSelector.Items[1].Selected = true;
			else if (_compareSelector.Items.Count != 0)
				_compareSelector.Items[0].Selected = true;
		}

		/// <summary>
		/// Build the report.
		/// </summary>
		void _submit_Click(object sender, EventArgs e)
		{
			//............................
			// Get input vars
			int profileID;
			if (!Int32.TryParse(_profileSelector.SelectedValue, out profileID))
				return;

			int daycode;
			if (!Int32.TryParse(_dateSelector.SelectedValue, out daycode))
				return;

			int compareDaycode;
            if (!Int32.TryParse(_compareSelector.SelectedValue, out compareDaycode))
                compareDaycode = daycode;

            #region Graph
            /*
			int graphRangeCount = _dateSelector.Items.Count > Const.GraphRangeMax ? Const.GraphRangeMax : _dateSelector.Items.Count;
			int graphRangeDayCode;
			if (!Int32.TryParse(_dateSelector.Items[graphRangeCount-1].Value, out graphRangeDayCode))
				graphRangeDayCode = previousDaycode;
            */
            #endregion

            List<RankingTable> rankingTables = GetRankingData(profileID, daycode, compareDaycode);

			_profileSearchEngineRepeater.DataSource = rankingTables;
			_profileSearchEngineRepeater.DataBind();
		}


        public List<RankingTable> GetRankingData(int profileID, int daycode, int compareDaycode)
        {
            //............................
            // Get search engine configuration
            NameValueCollection searchEngineLinkConfig = (NameValueCollection)WebConfigurationManager.GetSection("searchEngineUrls");
            // TODO: get search engines from DB

            //............................
            // RUN THE COMMAND

            // Start building a list of tables, one for every search engine
            List<RankingTable> rankingTables = new List<RankingTable>();

            using (DataManager.Current.OpenConnection())
            {
                #region Graph
                //............................
                // Create graph data table

                /*
				DataTable dtRank = new DataTable();
				SqlCommand graphDataCmd = DataManager.CreateCommand(@"RankingGraphData(@profileID:Int, @fromDate:Int, @toDate:Int)", CommandType.StoredProcedure);
				SqlDataAdapter adpater = new SqlDataAdapter(graphDataCmd);
				graphDataCmd.Parameters["@profileID"].Value = profileID;
				graphDataCmd.Parameters["@fromDate"].Value = graphRangeDayCode;
				graphDataCmd.Parameters["@toDate"].Value = daycode;
				adpater.Fill(dtRank);

				_datesPerWord = new string[Const.GraphRangeMax];
				string[] datesPerWordNewFormat = new string[Const.GraphRangeMax];
				for (int i = graphRangeCount - 1, d = 0; i >= 0; i--, d++)
				{
					_datesPerWord[d] = _dateSelector.Items[i].Text;
					datesPerWordNewFormat[d] = _dateSelector.Items[i].Value;
				}
                */
                #endregion

                //............................
                // Set up the command
                SqlCommand resultsCmd = DataManager.CreateCommand(@"RankingReportCompare(@profileID:Int, @dayCode:Int, @previousDayCode:Int )", CommandType.StoredProcedure);
                resultsCmd.Parameters["@profileID"].Value = profileID;
                resultsCmd.Parameters["@dayCode"].Value = daycode;
                resultsCmd.Parameters["@previousDayCode"].Value = compareDaycode;

                using (SqlDataReader reader = resultsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Get or create the ranking table
                        RankingTable rankingTable;
                        int searchEngineID = (int)reader["SearchEngineID"];
                        if (rankingTables.Count > 0 && rankingTables[rankingTables.Count - 1].SearchEngineID == searchEngineID)
                            rankingTable = rankingTables[rankingTables.Count - 1];
                        else
                            rankingTable = new RankingTable()
                            {
                                SearchEngineID = searchEngineID,
                                SearchEngineName = reader["SearchEngine"] as string
                            };

                        // Get or create a rankings row
                        RankingRow rankingRow;
                        string keyword = reader["Keyword"] as string;
                        if (rankingTable.Rows.Count > 0 && rankingTable.Rows[rankingTable.Rows.Count - 1].Keyword == keyword)
                            rankingRow = rankingTable.Rows[rankingTable.Rows.Count - 1];
                        else
                            rankingRow = new RankingRow()
                            {
                                Keyword = keyword,
                                SearchEngineUrl = GetSearchEngineQueryUrl(searchEngineID, keyword)
                            };
                        #region Graph
                        /*
							if (!reader.IsDBNull("Rank"))
							{
								Dictionary<string, int> ranksPerWord = new Dictionary<string, int>();
								
								//passing over the dataTable and gathering all relevant data by comparing daycode parameter with datesperWord array
								for (int datesCounter = 0; datesCounter < _datesPerWord.Length; datesCounter++)
								{
									for (int i = 0; i < dtRank.Rows.Count; i++)
									{
										if (dtRank.Rows[i]["SearchEngine"].Equals(engineName) &&
											dtRank.Rows[i]["Keyword"].Equals(kw) &&
											dtRank.Rows[i]["Url"].Equals(url) &&
											dtRank.Rows[i]["day_code"].ToString().Equals(datesPerWordNewFormat[datesCounter]))
										{
											//ranksPerWord[rankCounter];
											//datesCounter++;
											//if a double row is retrieved with same searchEngine, kw,url, day_code we'll ignore one row
											try
											{
												ranksPerWord.Add(_datesPerWord[datesCounter], Int32.Parse(dtRank.Rows[i]["Rank"].ToString()));
											}
											catch
											{
											}
										}
									}
								}

								ZedGraph.Web.ZedGraphWeb grph = new ZedGraph.Web.ZedGraphWeb();

								grph.Width = 105;
								grph.Height = 40;

								grph.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(delegate(ZedGraph.Web.ZedGraphWeb webObject, Graphics g, MasterPane pane)
								{
									//double[] ranks = new double[_datesPerWord.Length];
									List<double> ranks = new List<double>();
									GraphPane myPane = pane[0];
									myPane.Border.Color = System.Drawing.Color.White;
									myPane.Border.IsVisible = false;
									myPane.Border.Width = 0;

									// creating ranks data array

									int num;
									int numOfDates = _datesPerWord.Length;
									for (int i = 0; i < numOfDates; i++)
									{
										try
										{
											//maybe there are no ranks
											num = _ranksPerWord[_datesPerWord[i]];
											ranks.Add(num * -1);
											//ranks[i] = num * -1;
										}
										catch
										{
										}

									}
									LineItem myCurve;
									double[] ranks1 = new double[ranks.Count];
									ranks1 = ranks.ToArray();


									// Generate a blue bar with "Completed" in the legend
									myCurve = myPane.AddCurve("Completed", null, ranks1, Color.SeaGreen);
									myCurve.Line.Width = 3;
									myCurve.Line.IsAntiAlias = true;

									myPane.Border.IsVisible = false;
									myPane.Legend.IsVisible = false;
									myPane.YAxis.IsVisible = false;
									myPane.XAxis.IsVisible = false;
									myPane.YAxis.Scale.Min = -20;
									myPane.YAxis.Scale.Max = 0;
									myPane.Border.IsVisible = false;


									pane.AxisChange(g);
								});

								

								GraphsHolder.Controls.Add(grph);

								StringWriter tw = new StringWriter();
								HtmlTextWriter htw = new HtmlTextWriter(tw);
								grph.RenderControl(htw);
								int firstIndexImgSrc;
								int ImgSrcLength;
								string src;
								String imageBtnTag = String.Empty;
								firstIndexImgSrc = tw.ToString().IndexOf(" width=");
								ImgSrcLength = tw.ToString().IndexOf("alt=") - firstIndexImgSrc;
								imageBtnTag = "<input type=\"image\" name=\"";

								//creating a unique identifier for the current graph image
								imageBtnTag += reader["SearchEngine"].ToString() + "&" + reader["Keyword"].ToString() + "\"";
								imageBtnTag += " id=\"";
								imageBtnTag += reader["SearchEngine"].ToString() + "&" + reader["Keyword"].ToString() + "\"";
								imageBtnTag += tw.ToString().Substring(firstIndexImgSrc, ImgSrcLength);
								firstIndexImgSrc = imageBtnTag.IndexOf("src");
								src = imageBtnTag.Substring(firstIndexImgSrc, imageBtnTag.Length - firstIndexImgSrc);
								//imageBtnTag += " value=\"ImageButton_Click\" OnClick=\"window.open('graph.aspx?se=" + reader["SearchEngine"].ToString() + "&accntID=" + Int32.Parse(_accountSelector.SelectedValue) + "&kw=" + reader["Keyword"].ToString() + "&profID=" + _profileSelector.SelectedValue.ToString() + "; return false;')\">";
                                imageBtnTag = String.Format("<a href='#' onclick='window.open(\"graph.aspx?se={0}&accntID={1}&kw={2}&profID={3}\");return false;'><img {4}alt=\"click here for enlargement\" border=0 /></a>", reader["SearchEngine"].ToString(), Int32.Parse((string)Session["accountID"]), Server.UrlEncode(reader["Keyword"].ToString().Replace("\"", "\\\"")), _profileSelector.SelectedValue.ToString(), src);
								//kw += "&nbsp&nbsp";
								kw += "<td align=\"right\">";
								kw += imageBtnTag;
								kw += "</td></tr>";
							}

							//kw += "\n<cc1:ZedGraphWeb ID=\"ZedGraphWeb" + graphCounter + "\" runat=\"server\" Width=\"140\" Height=\"80\" onrendergraph=\"liran1\">\n";
							//kw += "</cc1:ZedGraphWeb>\n";

							currentTable.Rows.Add(currentRow);
						
							*/
                        #endregion

                        // Create a new column if necessary
                        string targetDomain = reader["Target"] as string;
                        if (!rankingTable.Columns.Contains(targetDomain))
                            rankingTable.Columns.Add(targetDomain);

                        // Get or create a rankings item
                        RankingItem rankingItem;
                        if (!rankingRow.Items.TryGetValue(targetDomain, out rankingItem))
                            rankingRow.Items.Add(targetDomain, new RankingItem()
                            {
                                IsClient = rankingTable.Columns.IndexOf(targetDomain) < 1,
                                TargetDomain = targetDomain
                            });

                        // Create the new ranking
                        Ranking ranking = new Ranking()
                        {
                            Url = reader["Url"] as string,
                            Rank = reader["Rank"] is DBNull ? 0 : (int)reader["Rank"],
                            RankDiff = reader["RankDiff"] is DBNull ? 0 : (int)reader["RankDiff"]
                        };
                        rankingItem.Rankings.Add(ranking);
                    }
                }
            }
            return rankingTables;
        }


        #region Rendering helpers
        //..............................................
        public RankingTable RankingTable(RepeaterItem repeater)
		{
			return repeater.DataItem as RankingTable;
		}

		public RankingRow RankingRow(RepeaterItem repeater)
		{
			return repeater.DataItem as RankingRow;
		}

		public RankingItem RankingItem(RepeaterItem repeater)
		{
			if (repeater.DataItem is string)
			{
				// if column name, get the item matching this column from the parent
				string col = repeater.DataItem as string;
				return ((RankingRow)((RepeaterItem)repeater.Parent.Parent).DataItem).Items[col];
			}
			else
			{
				return repeater.DataItem as RankingItem;
			}
		}

		public Ranking Ranking(RepeaterItem repeater)
		{
			return repeater.DataItem as Ranking;
		}

		private string TemplateBind(Control control)
		{
			StringWriter writer = new StringWriter();
			
			control.DataBind();
			control.RenderControl(new HtmlTextWriter(writer));
			
			return writer.ToString();
		}
		
		void SearchEngineTable_PreRender(object sender, EventArgs e)
		{
			DataGrid dataGrid = sender as DataGrid;
			TableRowCollection rows = ((Table)dataGrid.Controls[0]).Rows;
			TableRow header = rows[0];

			// Add duplicate header rows
			for (int i = 32; i < rows.Count - 2; i += 31)
			{
				Duplicate(rows, header, i);
			}

			if (rows.Count > 2)
				Duplicate(rows, header, rows.Count - 1);
		}

		void Duplicate(TableRowCollection rows, TableRow header, int i)
		{
			DataGridItem duplicate = new DataGridItem(i, -1, ListItemType.Header);
			duplicate.SetRenderMethodDelegate(delegate(HtmlTextWriter writer, Control control)
			{
				foreach (TableCell cell in header.Cells)
					cell.RenderControl(writer);
			});
			rows.AddAt(i, duplicate);
        }
        //..............................................
        #endregion

        #region EXPORT STUFF
        //..............................................

		void _export_Click(object sender, EventArgs e)
		{
			#region Obsolete - use for excel template
			//string templateFileName = @"D:\EdgeProject\Applications\EdgeServiceHost\OrganicTemplate.xlsx";

			//if (templateFileName == String.Empty ||
			//    templateFileName == null)
			//    throw new ArgumentException("Invalid template file name. Cannot be null or empty");

			//if (!File.Exists(templateFileName))
			//    throw new FileNotFoundException("Could not find templat file: " + templateFileName);
			#endregion

			// Check if the user select account, profile and date.
			if (Session["accountID"] == null || _dateSelector.SelectedIndex < 0)
				return;

			int daycode;
			if (!Int32.TryParse(_dateSelector.SelectedValue, out daycode))
				return;

			List<int> profileIDs = new List<int>();
			if (sender == _export)
			{
				// Add the selected profileID
				int p;
				if (!Int32.TryParse(_profileSelector.SelectedValue, out p))
					return;

				profileIDs.Add(p);
			}
			else
			{
				// Add all profile IDs
				foreach (ListItem profile in _profileSelector.Items)
				{
					int pID;
					if (Int32.TryParse(profile.Value, out pID))
						profileIDs.Add(pID);
				}
			}

			//string accountName = _accountSelector.SelectedItem.Text.Substring(0, _accountSelector.SelectedItem.Text.LastIndexOf('('));
            string accountName =  (string)Session["accountID"];

			string tempExcelFileName = Server.MapPath(Guid.NewGuid().ToString() + ".xlsx");
			string reportFileName = null;

			ZipFile zipStream = null;
			if (profileIDs.Count > 1)
			{
				zipStream = new ZipFile();
			}


			// Delete the excel file in case its exist and create a new file..
			File.Delete(tempExcelFileName);
			FileInfo newFile = new FileInfo(tempExcelFileName);
			// Create the excel file and initalize it with the data from the DB.
			using (ExcelPackage ep = new ExcelPackage(newFile)) //, template))
			{
				ExcelWorkbook workbook = ep.Workbook;
				// Create worksheet as array so we don't need to initalize for now.
				//ExcelWorksheet[] worksheet = new ExcelWorksheet[1];
				List<ExcelWorksheet> worksheet = new List<ExcelWorksheet>();
				// Start building a list of tables, one for every search engine
				List<DataTable> seTables = new List<DataTable>();
				//for each sheet we need a new row counter so it will be dynamically
				List<int> sheetsRowCounters = new List<int>();
				List<int> sheetsColumnCounters = new List<int>();
				int columsIndex = 2;
				int columsRow = 3;
				try
				{
					//set up all the _dates for the current account
					using (DataManager.Current.OpenConnection())
					{
						SqlCommand dateCmd = DataManager.CreateCommand("select distinct Day_Code from Rankings_Data where Account_ID = @accountID:Int order by day_code desc");
                        dateCmd.Parameters["@accountID"].Value = Int32.Parse((string)Session["accountID"]);

						using (SqlDataReader reader = dateCmd.ExecuteReader())
						{
							while (reader.Read())
							{
								_datesKwByTime.Add(DayCode.GenerateDateTime(reader[0]).ToString("dd/MM/yyyy"));
							}
						}
					}
					foreach (int profileID in profileIDs)
					{
						ListItem profileItem = _profileSelector.Items.FindByValue(profileID.ToString());
						if (profileItem == null)
							continue;
						if (sender == _exportAll || sender == _export)
							BuildExcelReport(workbook, daycode, profileID, worksheet, seTables, sheetsRowCounters, sheetsColumnCounters, ref columsIndex, ref columsRow);
						else if (sender == _exportAllByTime)
							BuildExcelKwByTimeReport(workbook, daycode, profileID, worksheet, seTables, sheetsRowCounters, sheetsColumnCounters, ref columsIndex, ref columsRow);
					}
					ep.Save();
					_warning.Visible = false;
					_processing.Visible = true;
					_processing.Text = "File Created Successfully.";
				}
				catch (Exception ex)
				{
					_processing.Visible = false;
					_warning.Visible = true;
					_warning.Text = "Processing failed.";
				}
				reportFileName = String.Format("{0}_{1}.xlsx",
					EscapeFileName(accountName),
					_dateSelector.SelectedItem.Value);
			}


			Response.Clear();

			// Delete the temp files
			string outputFileName;
			byte[] buffer;

			Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			buffer = File.ReadAllBytes(tempExcelFileName);
			Response.OutputStream.Write(buffer, 0, buffer.Length);

			outputFileName = reportFileName;

			Response.AddHeader("Content-disposition", String.Format("attachment; filename={0}", outputFileName));

			File.Delete(tempExcelFileName);
			Response.End();
		}

		private string EscapeFileName(string path)
		{
			char[] invalidChars = new char[] { '/', '\\', '?', '*', ' ', '+', '<', '>', '"', ':', '|' };
			string escaped = string.Empty;
			for (int i = 0; i < path.Length; i++)
			{
				if (!invalidChars.Contains<char>(path[i]))
					escaped += path[i];
			}
			return escaped;
		}

		/// <summary>
		/// Add new data to and Excel report which contains all the data for the chosen 
		/// account,profile,date that exist on the DB.
		/// </summary>
		/// <param name="workbook">workbook that contain all the worksheets that created.</param>
		private void BuildExcelReport(ExcelWorkbook workbook, int daycode, int profileID, List<ExcelWorksheet> worksheet, List<DataTable> seTables, List<int> sheetsRowCounters, List<int> sheetsColumnCounters, ref int columsIndex, ref int columsRow)
		{
			// Set up the command
			SqlCommand resultsCmd = DataManager.CreateCommand(@"RankingReport(@profileID:Int, @dayCode:Int)", CommandType.StoredProcedure);
			resultsCmd.Parameters["@profileID"].Value = profileID;
			resultsCmd.Parameters["@dayCode"].Value = daycode;

			// Init worksheet location variables.
			int col = 1;
			string url = string.Empty;

			bool isWorksheetExists = false;
			bool isNewProfile = true;
			//position of sheet counter in sheetsRowCounters list 
			int currentSheetCounterPos = 0;
			int i = 0;
			int currentWorksheet = 0;
			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(resultsCmd);

				using (SqlDataReader reader = resultsCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string se = reader["SearchEngine"] as string;
						//get the current worksheet
						if (isNewProfile)
						{
							isNewProfile = false;
						}
						isWorksheetExists = false;
						//find the current editing sheet
						for (i = 0; i < workbook.Worksheets.Count; i++)
						{
							if (workbook.Worksheets[i + 1].Name == se.Replace("/", "-"))
							{
								currentWorksheet = i;
								currentSheetCounterPos = i;
								isWorksheetExists = true;
								break;
							}
						}
						// Get or create the search engine table
						DataTable currentTable = new DataTable();
						foreach (DataTable dt in seTables)
						{
							if (dt.TableName == se)
							{
								currentTable = dt;
								break;
							}
						}
						if (!isWorksheetExists)
						{
							// Create new worksheet with the name of the Search Engine.
							worksheet.Add(workbook.Worksheets.Add(se.Replace("'", "").Replace("/", "-")));  // workbook.Worksheets.Copy("AdWords", se.Replace("'", "").Replace("/", "-"));

							// Insert 
							currentTable = new DataTable(se);
							currentTable.Columns.Add("Keyword");
							seTables.Add(currentTable);


							// Insert column SearchEngine and Keyword
							worksheet.Last().Cell(1, 1).Value = reader["SearchEngine"] as string;
							worksheet.Last().InsertRow(2);

							// Init worksheet location variables
							worksheet.Last().Cell(columsRow, 1).Value = "Profile Name";

							worksheet.Last().Cell(columsRow, 2).Value = "Keyword";
							currentWorksheet = workbook.Worksheets.Count - 1;
							int counter = 0;
							sheetsRowCounters.Add(counter);
							currentSheetCounterPos = currentWorksheet;
							// Init worksheet location variables
							sheetsRowCounters[currentSheetCounterPos] = 3;
							sheetsColumnCounters.Add(counter);
							sheetsColumnCounters[currentSheetCounterPos] = 3;
						}

						// Get or create a keyword row
						string kw = String.Format("{0}", reader["Keyword"]);
						DataRow currentRow;
						if (currentTable.Rows.Count > 0 && currentTable.Rows[currentTable.Rows.Count - 1]["Keyword"].ToString() == kw)
						{
							currentRow = currentTable.Rows[currentTable.Rows.Count - 1];
							url = reader["Url"] as string;

							string nextColumnValue = worksheet[currentWorksheet].Cell(columsRow, col + 2).Value;
							if (nextColumnValue.Contains(" "))
								nextColumnValue = nextColumnValue.Substring(0, nextColumnValue.IndexOf(" "));
							// For the same keyword we can have some rankings.
							// If the current url belong to the next competitor 
							// we increase the colmun if not we write the url in the same colmun.
							if (url == null || (url != null && url.Contains(nextColumnValue) && col < sheetsColumnCounters[currentSheetCounterPos] - 1))
							{
								if (worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], col).Value.Contains(","))
									worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], col).Value = worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], col).Value.Substring(0, worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], col).Value.Length - 2);
								col += 2;
							}
						}
						else
						{
							currentRow = currentTable.NewRow();
							currentRow["Keyword"] = kw;
							currentTable.Rows.Add(currentRow);

							// Insert keyword value.
							++sheetsRowCounters[currentSheetCounterPos];
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 1).Value = _profileSelector.Items.FindByValue(profileID.ToString()).ToString();
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 2).Value = kw.Replace("'", "");
							col = 2;
						}

						// Add domains of the competitors
						string target = reader["Target"] as string;
						bool wasFound;
						int tempCol;
						int colCounter;
						if (!currentTable.Columns.Contains(target + " rank") && !currentTable.Columns.Contains(target + " url"))
						{
							try
							{
								currentTable.Columns.Add(target + " rank");
							}
							catch
							{
							}
							// ugly patch to remove ' from client's sites
							if (target.ToLower().Contains("client"))
								target = "Client Sites rank";
							wasFound = false;
							tempCol = sheetsColumnCounters[currentSheetCounterPos];
							for (colCounter = 3; colCounter < sheetsColumnCounters[currentSheetCounterPos]; colCounter++)
							{
								if (worksheet[currentWorksheet].Cell(columsRow, colCounter).Value == target.Replace("'", "") + " rank")
								{
									wasFound = true;
									break;
								}
							}
							if (wasFound)
								sheetsColumnCounters[currentSheetCounterPos] = colCounter;
							else
							{
								sheetsColumnCounters[currentSheetCounterPos] = tempCol;
								// insert target as columnName.
								if (!target.ToLower().Contains("rank"))
								{
									worksheet[currentWorksheet].Cell(columsRow, sheetsColumnCounters[currentSheetCounterPos]).Value = target.Replace("'", "") + " rank";
								}
								else
								{
									worksheet[currentWorksheet].Cell(columsRow, sheetsColumnCounters[currentSheetCounterPos]).Value = target.Replace("'", "");
								}
								tempCol++;
							}

							try
							{
								currentTable.Columns.Add(target + " url");
							}
							catch
							{
							}

							// ugly patch to remove ' from client's sites
							if (target.ToLower().Contains("client"))
								target = "Client Sites url";
							target = target.Replace("'", "");
							if (wasFound)
								sheetsColumnCounters[currentSheetCounterPos] = colCounter;
							else
							{
								sheetsColumnCounters[currentSheetCounterPos] = tempCol;
								// insert target as columnName.
								if (!target.ToLower().Contains("url"))
								{
									worksheet[currentWorksheet].Cell(columsRow, sheetsColumnCounters[currentSheetCounterPos]).Value = target.Replace("'", "") + " url";
								}
								else
								{
									worksheet[currentWorksheet].Cell(columsRow, sheetsColumnCounters[currentSheetCounterPos]).Value = target.Replace("'", "");
								}
								sheetsColumnCounters[currentSheetCounterPos]++;
							}
						}

						// Add keyword rank and his url (if exist)
						string rank = reader["Rank"] is DBNull ? string.Empty : reader["Rank"].ToString();
						url = reader["Url"] as string;
						if (url != null)
							url = url.Replace("'", "");

						target = reader["Target"] as string;
						wasFound = false;
						tempCol = sheetsColumnCounters[currentSheetCounterPos];
						int columsIndex2 = tempCol;
						for (colCounter = 3; colCounter < columsIndex2; colCounter++)
						{
							if (worksheet[currentWorksheet].Cell(columsRow, colCounter).Value.StartsWith(target.ToLower().Contains("client") ? "Client" : target.Replace("'", "") + " rank"))
							{
								wasFound = true;
								break;
							}
						}
						if (wasFound)
							columsIndex2 = colCounter;
						else
						{
							columsIndex2 = tempCol;
							// insert target as columnName.
							worksheet[currentWorksheet].Cell(columsRow, columsIndex2).Value = target.Replace("'", "") + " rank";
							tempCol++;
						}
						int res;
						if (Int32.TryParse(worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2).Value, out res) &&
							!worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2).Value.Equals("") ||
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2).Value.Contains(","))
						{
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2).Value += (rank == string.Empty) ? "-" : " , " + rank;
						}
						else
						{
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2).Value += (rank == string.Empty) ? "-" : rank;
						}

						if (rank != string.Empty)
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2 + 1).Hyperlink = new Uri(url, UriKind.Absolute);

						worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], columsIndex2 + 1).Value += (rank == string.Empty) ? string.Empty : "  (" + Server.UrlDecode(url) + ")  ";
					}
				}
			}
		}
		/// <summary>
		/// Add new data to and Excel report which contains all the data for the chosen 
		/// account,profile,date that exist on the DB.
		/// </summary>
		/// <param name="workbook">workbook that contain all the worksheets that created.</param>
		private void BuildExcelKwByTimeReport(ExcelWorkbook workbook, int daycode, int profileID, List<ExcelWorksheet> worksheet, List<DataTable> seTables, List<int> sheetsRowCounters, List<int> sheetsColumnCounters, ref int columsIndex, ref int columsRow)
		{
			// Set up the command
			SqlCommand resultsCmd = DataManager.CreateCommand(@"RankingByTimeReport(@profileID:Int)", CommandType.StoredProcedure);
			resultsCmd.Parameters["@profileID"].Value = profileID;

			// Init worksheet location variables.
			int col = 1;
			string url = string.Empty;

			bool isWorksheetExists = false;
			bool isNewProfile = true;
			//position of sheet counter in sheetsRowCounters list 
			int currentSheetCounterPos = 0;
			int i = 0;
			int currentWorksheet = 0;

			using (DataManager.Current.OpenConnection())
			{
				DataManager.Current.AssociateCommands(resultsCmd);

				using (SqlDataReader reader = resultsCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string se = reader["SearchEngine"] as string;
						//get the current worksheet
						if (isNewProfile)
						{
							isNewProfile = false;
						}
						isWorksheetExists = false;
						//find the current editing sheet
						for (i = 0; i < workbook.Worksheets.Count; i++)
						{
							if (workbook.Worksheets[i + 1].Name == se.Replace("/", "-"))
							{
								currentWorksheet = i;
								currentSheetCounterPos = i;
								isWorksheetExists = true;
								break;
							}
						}
						// Get or create the search engine table
						DataTable currentTable = new DataTable();
						foreach (DataTable dt in seTables)
						{
							if (dt.TableName == se)
							{
								currentTable = dt;
								break;
							}
						}
						if (!isWorksheetExists)
						{
							// Create new worksheet with the name of the Search Engine.
							worksheet.Add(workbook.Worksheets.Add(se.Replace("'", "").Replace("/", "-")));  // workbook.Worksheets.Copy("AdWords", se.Replace("'", "").Replace("/", "-"));

							// Insert 
							currentTable = new DataTable(se);
							currentTable.Columns.Add("Keyword");
							seTables.Add(currentTable);


							// Insert column SearchEngine and Keyword
							worksheet.Last().Cell(1, 1).Value = reader["SearchEngine"] as string;
							worksheet.Last().InsertRow(2);

							// Init worksheet location variables
							worksheet.Last().Cell(columsRow, 1).Value = "Profile Name";

							worksheet.Last().Cell(columsRow, 2).Value = "Keyword";
							worksheet.Last().Cell(columsRow, 3).Value = "Url";
							int datesCounter = 1;

							foreach (string dsi in _datesKwByTime)
							{
								worksheet.Last().Cell(columsRow, 3 + datesCounter).Value = dsi;
								datesCounter++;
							}
							currentWorksheet = workbook.Worksheets.Count - 1;
							int counter = 0;
							sheetsRowCounters.Add(counter);
							currentSheetCounterPos = currentWorksheet;
							// Init worksheet location variables
							sheetsRowCounters[currentSheetCounterPos] = 4;
							sheetsColumnCounters.Add(counter);
							sheetsColumnCounters[currentSheetCounterPos] = 4;
						}


						// Get or create a keyword row
						string kw = String.Format("{0}", reader["Keyword"]);
						url = String.Format("{0}", reader["Url"]);
						if (url != null)
							url = url.Replace("'", "");
						DataRow currentRow;
						if (currentTable.Rows.Count > 0 && currentTable.Rows[currentTable.Rows.Count - 1]["Keyword"].ToString() == kw
							&& worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 3).Value == url)
						{
							currentRow = currentTable.Rows[currentTable.Rows.Count - 1];

							string nextColumnValue = worksheet[currentWorksheet].Cell(columsRow, col + 2).Value;
							if (nextColumnValue.Contains(" "))
								nextColumnValue = nextColumnValue.Substring(0, nextColumnValue.IndexOf(" "));
						}
						else
						{
							currentRow = currentTable.NewRow();
							currentRow["Keyword"] = kw;
							currentTable.Rows.Add(currentRow);

							// Insert keyword value.
							++sheetsRowCounters[currentSheetCounterPos];
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 1).Value = _profileSelector.Items.FindByValue(profileID.ToString()).ToString();
							worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 2).Value = kw.Replace("'", "");
							if (reader["Url"].ToString() != null)
								worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], 3).Value = reader["Url"].ToString().Replace("'", "");
							col = 2;
						}
						//find the appropriate date in excel for the current row from db
						int dateCounter = 0;
						for (dateCounter = 3; dateCounter < _dateSelector.Items.Count + 3; dateCounter++)
						{
							if (DayCode.GenerateDateTime(reader["Date"]).ToString("dd/MM/yyyy").Equals(worksheet[currentWorksheet].Cell(3, dateCounter).Value))
								break;
						}
						worksheet[currentWorksheet].Cell(sheetsRowCounters[currentSheetCounterPos], dateCounter).Value = reader["Rank"].ToString();
					}
				}
			}
		}

		//..............................................
		#endregion
	}
}