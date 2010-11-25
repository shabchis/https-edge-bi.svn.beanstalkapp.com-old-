using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Easynet.Edge.Core.Data;
using System.ServiceModel.Web;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;

namespace EdgeBI.Web.DataServices
{
	public class MeasureDataService : IMeasureDataService
	{
		/// <summary>
		/// 
		/// </summary>
		[WebInvoke(
			Method = "GET",
			BodyStyle = WebMessageBodyStyle.Bare,
			ResponseFormat = WebMessageFormat.Xml,
			UriTemplate = "measures?account={accountID}"
		)]
		public List<Measure> GetMeasures(int accountID)
		{
			return GetMeasures(accountID, false);
		}

		/// <summary>
		/// 
		/// </summary>
		internal List<Measure> GetMeasures(int accountID, bool includeBase)
		{
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(AppSettings.Get(this, "Commands.GetMeasures"), System.Data.CommandType.StoredProcedure);
				cmd.Parameters["@accountID"].Value = accountID;
				cmd.Parameters["@includeBase"].Value = includeBase;

				List<Measure> list = new List<Measure>();
				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
						list.Add(new Measure(reader));
				}

				return list;
			}
		}

		//==========================================

		[WebInvoke(
			Method = "GET",
			BodyStyle = WebMessageBodyStyle.Bare,
			ResponseFormat = WebMessageFormat.Xml,
			UriTemplate = "channels"
		)]
		public List<Channel> GetChannels()
		{
			using (DataManager.Current.OpenConnection())
			{
				SqlCommand cmd = DataManager.CreateCommand(AppSettings.Get(this, "Commands.GetChannels"));//, System.Data.CommandType.StoredProcedure);

				List<Channel> list = new List<Channel>();
				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
						list.Add(new Channel(reader));
				}
				return list;
			}
		}

		//==========================================

		[WebInvoke(
			Method = "GET",
			BodyStyle = WebMessageBodyStyle.Bare,
			ResponseFormat = WebMessageFormat.Xml,
			UriTemplate = "data?" +
				"account={account}&" +
				"grouping={grouping}&" +
				"top={top}&" + 
				"measures={measures}&" +
				"ranges={ranges}&" +
				"diff={diff}&" +
				"datasort={dataSort}&" +
				"viewsort={viewSort}&" +
				"format={format}&" +
                "filter={filter}"
		)]
		public List<ObjData> GetData(
			int account,
			DataGrouping grouping,
			int top,
			MeasureRef[] measures,
			DayCodeRange[] ranges,
			MeasureDiff[] diff,
			MeasureSort[] dataSort,
			MeasureSort[] viewSort,
			MeasureFormat[] format,
            MeasureFilter filter
			)
		{
			// Get measures
			List<Measure> measuresList = GetMeasures(account, true);
			Dictionary<int, Measure> measuresByID = measuresList.ToDictionary(m => m.MeasureID);
            EdgeBI.Web.DataServices.DataHandler dataHandler = new EdgeBI.Web.DataServices.DataHandler();
			return dataHandler.GetData(account, grouping, top, measures, ranges, diff, dataSort, viewSort, format, measuresByID,filter);
		}
	}
}
