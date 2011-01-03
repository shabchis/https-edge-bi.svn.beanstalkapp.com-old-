using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.Core.Data;
using ZedGraph;

namespace Easynet.Edge.UI.WebPages
{
    public partial class graph : System.Web.UI.Page
    {
        DataTable _dtRank;
        List<int> _datesKwByTime = new List<int>();
        string _keyword;
        string _searchEngine;
        int _fromDate = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            _dtRank = new DataTable();
            makeRankTable(ref _dtRank);
            ZedGraphWeb1.RenderMode = RenderModeType.ImageTag;
            ZedGraphWeb1.IsImageMap = true;
        }
        private void makeRankTable(ref DataTable dtRank)
        {
            String se = Request.QueryString["se"];
            String accntID = Request.QueryString["accntID"];
            String kw = Server.UrlDecode(Request.QueryString["kw"]);
            String profID = Request.QueryString["profID"];
            _keyword = kw;
            _searchEngine = se;

            using (DataManager.Current.OpenConnection())
            {
                SqlCommand dateCmd = DataManager.CreateCommand("select distinct Day_Code from Rankings_Data where Account_ID = @accountID:Int and ProfileID = @profileID:Int order by day_code desc");
                dateCmd.Parameters["@accountID"].Value = Int32.Parse(accntID);
                dateCmd.Parameters["@profileID"].Value = Int32.Parse(profID);

                using (SqlDataReader reader = dateCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _datesKwByTime.Add(Int32.Parse(reader[0].ToString()));
                    }
                }
            }

            SqlCommand resultsCmd = DataManager.CreateCommand(@"RankingSingleGraphData(@profileID:Int, @fromDate:Int, @toDate:Int, @Se:nvarchar, @Kw:nvarchar)", CommandType.StoredProcedure);
            resultsCmd.Parameters["@profileID"].Value = Int32.Parse(profID);
            resultsCmd.Parameters["@toDate"].Value = _datesKwByTime[0];
            if (_datesKwByTime[0] - 600 > _datesKwByTime[_datesKwByTime.Count - 1])
            {
                //past a half a year
                resultsCmd.Parameters["@fromDate"].Value = DayCode.ToDayCode(DayCode.GenerateDateTime(_datesKwByTime[0]).AddMonths(-6));
                _fromDate = DayCode.ToDayCode(DayCode.GenerateDateTime(_datesKwByTime[0]).AddMonths(-6));
            }
            else
            {
                resultsCmd.Parameters["@fromDate"].Value = _datesKwByTime[_datesKwByTime.Count - 1];
            }
            resultsCmd.Parameters["@Se"].Value = se;
            resultsCmd.Parameters["@Kw"].Value = kw;

            SqlDataAdapter adpater = new SqlDataAdapter(resultsCmd);
            using (DataManager.Current.OpenConnection())
            {
                DataManager.Current.AssociateCommands(resultsCmd);
                adpater.Fill(dtRank);
            }
        }
        protected void ZedGraphWeb1_RenderGraph(ZedGraph.Web.ZedGraphWeb webObject, Graphics g, MasterPane pane)
        {
            //Color[] colors = new Color[16] {Color.Silver, Color.Yellow, Color.DarkGreen, Color.Red, Color.Gray, Color.Purple, Color.LightYellow, Color.LightGreen, Color.LightGray, Color.LimeGreen, Color.Magenta, Color.Turquoise, Color.Maroon, Color.Linen, Color.MediumBlue, Color.Olive};
            Color[] colors = new Color[22] {ColorTranslator.FromHtml("#149406"), ColorTranslator.FromHtml("#ee5624"),
            ColorTranslator.FromHtml("#0600ff"), ColorTranslator.FromHtml("#eff14a"), ColorTranslator.FromHtml("#eeab36"),
            ColorTranslator.FromHtml("#7f6300"), ColorTranslator.FromHtml("#a40070"), ColorTranslator.FromHtml("#4ecb2c"),
            ColorTranslator.FromHtml("#41a8ff"), ColorTranslator.FromHtml("#f975fb"), Color.Black, Color.Gray, Color.BurlyWood,
            Color.Chartreuse, Color.CornflowerBlue, Color.Cyan, Color.DarkSalmon, Color.DimGray, Color.Fuchsia, Color.Khaki,
            Color.Ivory, Color.LawnGreen};
            double[] ranks = new double[_datesKwByTime.Count];
            GraphPane myPane = pane[0];
            myPane.Border.Color = System.Drawing.Color.White;
            int firstPos = _searchEngine.IndexOf("-");
            myPane.Title.Text = "Ranks for '" + _keyword + "' keyword - " + _searchEngine.Substring(0,firstPos) + "(" + _searchEngine.Substring(firstPos + 2, _searchEngine.Length - (firstPos + 2)) + ")";
            myPane.XAxis.Title.Text = "Date";
            myPane.YAxis.Title.Text = "Rank";

            //creating dates list
            List<string> dates = new List<string>();
            int datesCounter;
            for (datesCounter = _datesKwByTime.Count - 1; datesCounter > 0; datesCounter--)
            {
                //checking half a year limitation
                if (_fromDate == 0 || _datesKwByTime[datesCounter] <= _fromDate)
                {
                    _datesKwByTime.Remove(_datesKwByTime[datesCounter]);
                }
            }

            //make the same dates list in dd/mm/yyyy format
            for (int i = _datesKwByTime.Count - 1; i >= 0; i--)
            {
                dates.Add(DayCode.GenerateDateTime(_datesKwByTime[i]).ToString("dd/MM/yyyy"));
            }
            //foreach (int day in _datesKwByTime)
            //{
            //    dates.Add(DayCode.GenerateDateTime(day).ToString("dd/MM/yyyy"));
            //}

            //creating all the curves data
            List<LineItem> curvesList = new List<LineItem>();
            List<string> urlsList = new List<string>();
            List<PointPairList> curvesListData = new List<PointPairList>();
            PointPairList pointsList;
            int rowsCounter = _dtRank.Rows.Count - 1;
            datesCounter = _datesKwByTime.Count - 1;
            int daycode = 0;
            int rank = 0;
            int prevRank = (int)_dtRank.Rows[rowsCounter]["Rank"] * -1;
            while (rowsCounter >= 0 && _datesKwByTime.Count > 0)
            {
                pointsList = new PointPairList();
                if (rowsCounter <= 0)
                    break;
                while (_dtRank.Rows[rowsCounter]["Url"].ToString().Equals(_dtRank.Rows[rowsCounter - 1]["Url"].ToString()) &&
                        datesCounter >= 0)
                {
                    //multiply by -1 for proper direction of the graph curve
                    daycode = (int) _dtRank.Rows[rowsCounter]["day_code"];
                    rank = (int)_dtRank.Rows[rowsCounter]["Rank"] * -1;
                    if ((double) daycode == (double)_datesKwByTime[datesCounter])
                    {
                        pointsList.Add((double) daycode, (double) rank);
                        prevRank = rank;
                        rowsCounter--;
                    }
                    //no data on that date so we'll put the previous rank data
                    else
                    {
                        pointsList.Add(Double.Parse(_datesKwByTime[datesCounter].ToString()), (double)prevRank);
                    }
                    //in case we reached the last row from db - we need to fill the same rank data to the rest of dates
                    //if (rowsCounter == 0 && datesCounter > 0)
                    //{
                    //    while (datesCounter > 0)
                    //    {
                    //        pointsList.Add(Double.Parse(_datesKwByTime[datesCounter].ToString()), (double)rank);
                    //        datesCounter--;
                    //    }
                    //    break;
                    //}
                    //else if(rowsCounter == 0 || datesCounter == 0)
                    //{
                    //    break;
                    //}
                    if (rowsCounter == 0)
                        break;
                    datesCounter--;
                }
                daycode = (int)_dtRank.Rows[rowsCounter]["day_code"];
                rank = (int)_dtRank.Rows[rowsCounter]["Rank"] * -1;
                //for first row in db
                for (int i = datesCounter - 1; i >= 0; i--)
                {
                    if ((double)daycode == (double)_datesKwByTime[i])
                    {
                        pointsList.Add((double)daycode, (double)rank);
                        break;
                    }
                    else
                    {
                        pointsList.Add(Double.Parse(_datesKwByTime[i].ToString()), (double)prevRank);
                    }
                }
                //save url pointsList for the legend
                try
                {
                    urlsList.Add(Server.UrlDecode(_dtRank.Rows[rowsCounter + 1]["Url"].ToString()));
                }
                catch 
                {
                    urlsList.Add(Server.UrlDecode(_dtRank.Rows[rowsCounter]["Url"].ToString()));
                }
                curvesListData.Add(pointsList);
                LineItem myCurve = null;
                curvesList.Add(myCurve);
                rowsCounter--;
                datesCounter = _datesKwByTime.Count - 1;
            }

            List<double> dataList = new List<double>();
            List<double> z = new List<double>();
            //for(int i = 0; i < curvesListData[0].Count; i ++)
            //{
            //    dataList.Add(curvesListData[0][i].Y);
            //    z.Add(curvesListData[0][i].X);
            //}
            double[] curveDataArray = new double[dataList.Count];
            //curveDataArray = dataList.ToArray();
            for (int curvesCounter = 0; curvesCounter < curvesList.Count; curvesCounter++)
            {
                dataList.Clear();
                for (int i = 0; i < curvesListData[curvesCounter].Count; i++)
                {
                    dataList.Add(curvesListData[curvesCounter][i].Y);
                }
                curveDataArray = dataList.ToArray();
                curvesList[curvesCounter] = myPane.AddCurve(urlsList[curvesCounter], null, curveDataArray, colors[curvesCounter]);
                curvesList[curvesCounter].Line.Width = 6;

                curvesList[curvesCounter].Line.IsSmooth = true;
                curvesList[curvesCounter].Line.StepType = StepType.RearwardStep;
                curvesList[curvesCounter].Line.IsAntiAlias = true;
                curvesList[curvesCounter].Link = new Link("graph.aspx", "#", "_blank");

                curvesList[curvesCounter].Symbol.Fill = new Fill(colors[curvesCounter]);
                //curvesList[curvesCounter].Symbol.Fill.Type = FillType.GradientByZ;
                //curvesList[curvesCounter].Symbol.Fill.RangeMin = 0;
                //curvesList[curvesCounter].Symbol.Fill.RangeMax = 30;
                curvesList[curvesCounter].Symbol.Type = SymbolType.Circle;
            }

            foreach (CurveItem curve in myPane.CurveList)
            {
                int i = _datesKwByTime.Count;
                string name = curve.Label.Text;
                foreach (PointPair pt in curve.Points as PointPairList)
                {
                    //pt.Tag = (pt.Y * -1).ToString() + " (" + DayCode.GenerateDateTime(pt.X).ToString("dd/MM/yyyy") + ")";
                    //pt.Tag = string.Format("{0} ({1:dd/MM/yyyy})", pt.Y * -1, DayCode.GenerateDateTime(pt.Y));
                    i--;
                    pt.Tag = string.Format("{0} ({1:dd/MM/yyyy})", pt.Y * -1, DayCode.GenerateDateTime(_datesKwByTime[i]));
                }
            }
            string[] datesArray = dates.ToArray();
            myPane.Border.IsVisible = true;
            myPane.Legend.IsVisible = true;
            myPane.Legend.FontSpec.Size = 8;
            float r = myPane.Legend.Location.TopLeft.X;
            myPane.Legend.Position = LegendPos.BottomFlushLeft;

            myPane.YAxis.IsVisible = true;
            myPane.YAxis.Scale.FontSpec.IsAntiAlias = true;
            myPane.XAxis.IsVisible = true;
            // Set the XAxis labels
            myPane.XAxis.Scale.TextLabels = datesArray;
            myPane.XAxis.MajorTic.IsBetweenLabels = true;
            // Set the XAxis to Text type
            myPane.XAxis.Type = AxisType.Text;
            myPane.XAxis.Scale.FontSpec.IsAntiAlias = true;
            myPane.XAxis.Scale.FontSpec.Size = 10;
            myPane.XAxis.Scale.FontSpec.Angle = 45;
            myPane.X2Axis.IsVisible = false;

            myPane.YAxis.Scale.FontSpec.Size = 10;
            double maxRank = checkMaxRank(ref myPane);
            if(maxRank  < -20)
                myPane.YAxis.Scale.Min = maxRank - 10;
            else
            myPane.YAxis.Scale.Min = -20;
            myPane.YAxis.Scale.Max = 0;
            myPane.YAxis.Scale.MajorStepAuto = true;

            pane.AxisChange(g);
        }
        private double checkMaxRank(ref GraphPane myPane)
        {
            double max = 0;

            foreach (CurveItem curve in myPane.CurveList)
            {
                foreach (PointPair pt in curve.Points as PointPairList)
                {
                    if (pt.Y < max)
                        max = pt.Y;
                }
            }
            return max;
        }
    }
}
