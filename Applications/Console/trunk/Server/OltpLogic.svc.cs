using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Easynet.Edge.UI.Data;
using Easynet.Edge.UI.Data.OltpTableAdapters;
using System.Data.SqlClient;
using Easynet.Edge.Core.Data;
using Easynet.Edge.Core.Services;
using Easynet.Edge.Core.Scheduling;
using Easynet.Edge.Core;
using Easynet.Edge.Core.Utilities;
using Easynet.Edge.BusinessObjects;
using Easynet.Edge.Services.DataRetrieval;
using Easynet.Edge.Core.Configuration;
using System.Security.Authentication;

namespace Easynet.Edge.UI.Server
{
	/// <summary>
	/// 
	/// </summary>
	[ServiceBehavior(InstanceContextMode=InstanceContextMode.PerSession)]  
	public class OltpLogic: IOltpLogic
	{
		Oltp.UserRow CurrentUser;

		#region Private
		/*=========================*/

		private TableAdapter From<TableAdapter>() where TableAdapter: System.ComponentModel.Component, new()
		{
			return new TableAdapter();
		}

		/*=========================*/
		#endregion

		#region User
		/*=========================*/

		public Oltp.UserDataTable User_LoginByID(int userID)
		{
			Oltp.UserDataTable table = From<UserTableAdapter>().Get(userID);
			if (table.Rows.Count < 1)
				throw new AuthenticationException("Incorrect user ID.");

			CurrentUser = (Oltp.UserRow) table.Rows[0];
			return table;
		}

		static bool Encrypt = bool.Parse(AppSettings.GetAbsolute("Easynet.Edge.UI.Server.User.EncryptedPasswords"));
		public Oltp.UserDataTable User_LoginByEmail(string email, string password)
		{
			string pass = Encrypt ? Encryptor.Encrypt(password) : password;
			Oltp.UserDataTable table = From<UserTableAdapter>().GetByEmail(email, pass);
			if (table.Rows.Count < 1)
				throw new AuthenticationException("Incorrect user/password.");

			CurrentUser = (Oltp.UserRow)table.Rows[0];
			return table;
		}

		public Oltp.UserDataTable User_GetByGroup(int groupID)
		{
			return From<UserTableAdapter>().GetByGroup(groupID);
		}

		public Oltp.UserDataTable User_GetUsersWithPermissions(int accountID)
		{
			return From<UserTableAdapter>().GetUsersWithPermissions(accountID);
		}

		public Oltp.UserDataTable User_GetUsersWithoutPermissions(int accountID)
		{
			return From<UserTableAdapter>().GetUsersWithoutPermissions(accountID);
		}

		public DataTable User_GetAllPermissions()
		{
			string cmdText = @"User_GetAllPermissions(@userID:Int)";

			DataTable tbl = new DataTable("UserPermissions");
			
			GatewayTableAdapter tempAdapter = From<GatewayTableAdapter>();
			tempAdapter.CurrentConnection.Open();
			DataManager.Current.OpenConnection(tempAdapter.CurrentConnection);

			using (tempAdapter.CurrentConnection)
			{
				SqlCommand cmd = DataManager.CreateCommand(cmdText, CommandType.StoredProcedure);
				cmd.Parameters["@userID"].Value = CurrentUser.ID;
				SqlDataAdapter adapter = new SqlDataAdapter(cmd);
				adapter.Fill(tbl);
			}

			return tbl;
		}

		public Oltp.UserDataTable User_Save(Oltp.UserDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<UserTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.UserGroupDataTable UserGroup_Get(int groupID)
		{
			return From<UserGroupTableAdapter>().Get(groupID);
		}

		public Oltp.UserGroupDataTable UserGroup_Save(Oltp.UserGroupDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<UserGroupTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.UserGroupDataTable UserGroup_GetGroupsWithPermissions(int accountID)
		{
			return From<UserGroupTableAdapter>().GetGroupsWithPermissions(accountID);
		}

		public Oltp.UserGroupDataTable UserGroup_GetGroupsWithoutPermissions(int accountID)
		{
			return From<UserGroupTableAdapter>().GetGroupsWithoutPermissions(accountID);
		}
		
		/*=========================*/
		#endregion

		#region Account permissions
		/*=========================*/

		public Oltp.AccountPermissionDataTable AccountPermission_Get(int accountID, int targetID, bool isGroup)
		{
			return From<AccountPermissionTableAdapter>().Get(accountID, targetID, isGroup);
		}

		public Oltp.AccountPermissionDataTable AccountPermission_Save(Oltp.AccountPermissionDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<AccountPermissionTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public int AccountPermission_RemovePermissions(int accountID, int targetID, bool isGroup)
		{
			return From<AccountPermissionTableAdapter>().RemovePermissions(accountID, targetID, isGroup);
		}

		//public bool AccountPermission_CheckUserPermission(int accountID, int userID, string permissionType)
		//{
		//	object s = From<AccountPermissionTableAdapter>().CheckUserPermission(accountID, userID, permissionType);
		//	return s != null;
		//}

		/*=========================*/
		#endregion

		public Measure[] Measure_Get(int accountID)
		{
			return Measure_Get(accountID, true);
		}

		private Measure[] Measure_Get(int accountID, bool openConnection)
		{
			SqlConnection connection = null;

			if (openConnection)
			{
				GatewayTableAdapter adapter = From<GatewayTableAdapter>();
				adapter.CurrentConnection.Open();
				DataManager.Current.OpenConnection(adapter.CurrentConnection);
				connection = adapter.CurrentConnection;
			}

			List<Measure> list = new List<Measure>();
			using (connection)
			{
				SqlCommand cmd = DataManager.CreateCommand("select * from Measure where AccountID in (-1, @accountID:Int) and IsTarget=1");
				cmd.Parameters["@accountID"].Value = accountID;
				using (SqlDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
						list.Add(new Measure(reader));
				}
			}
			
			return list.ToArray();
		}

		public DataTable CampaignTargets_Get(int accountID)
		{
			GatewayTableAdapter tempAdapter = From<GatewayTableAdapter>();
			tempAdapter.CurrentConnection.Open();
			DataManager.Current.OpenConnection(tempAdapter.CurrentConnection);

			DataTable targetsTable = new DataTable("TargetsTable");
			using (tempAdapter.CurrentConnection)
			{
				Measure[] measures = Measure_Get(accountID, false);
				string measureFields = string.Empty;
				for (int i = 0; i < measures.Length; i++)
				{
					Measure m = measures[i];
					bool isLast = i == measures.Length-1;
					measureFields += m.FieldName + (isLast ? null : ", ");
				}

				string cmdText = String.Format
				(
					@"select
						CampaignGK,
						AdgroupGK,
						{0}
					from
						User_GUI_CampaignTargets a
					where
						AccountID = @accountID:Int and 
						SegmentID = -1",
					measureFields
				);

				SqlCommand cmd = DataManager.CreateCommand(cmdText);
				cmd.Parameters["@accountID"].Value = accountID;

				SqlDataAdapter adapter = new SqlDataAdapter(cmd);
				adapter.Fill(targetsTable);

			}

			return targetsTable;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="table"></param>
		public void CampaignTargets_Save(int accountID, DataTable table)
		{
			GatewayTableAdapter tempAdapter = From<GatewayTableAdapter>();
			tempAdapter.CurrentConnection.Open();
			DataManager.Current.OpenConnection(tempAdapter.CurrentConnection);

			// Execute the commands on the table
			using (tempAdapter.CurrentConnection)
			{
				Measure[] measures = Measure_Get(accountID, false);
				string insertMeasureFields = string.Empty;
				string insertMeasureValues = string.Empty;
				string updateMeasureFields = string.Empty;

				// Build dynamic query from measures
				for (int i = 0; i < measures.Length; i++)
				{
					Measure m = measures[i];
					bool isLast = i == measures.Length-1;
					insertMeasureFields += m.FieldName + (isLast ? null : ", ");
					insertMeasureValues += String.Format("@{0}:float", m.FieldName) + (isLast ? null : ", ");
					updateMeasureFields += String.Format("{0} = @{0}:float", m.FieldName) + (isLast ? null : ", ");
				}

				string updateText = String.Format("update User_GUI_CampaignTargets set {0} where AccountID = @accountID:Int and CampaignGK = @campaignGK:Int and AdgroupGK = @adgroupGK:Int and SegmentID = -1", updateMeasureFields);
				string insertText = String.Format("insert into User_GUI_CampaignTargets(AccountID, CampaignGK, AdgroupGK, SegmentID, {0} ) values (@accountID:Int, @campaignGK:Int, @adgroupGK:Int, -1, {1})", insertMeasureFields, insertMeasureValues);
				string deleteText = "delete from User_GUI_CampaignTargets where AccountID = @accountID:Int and CampaignGK = @campaignGK:Int and AdgroupGK = @adgroupGK:Int and SegmentID = -1";

				SqlDataAdapter adapter = new SqlDataAdapter();
				adapter.UpdateCommand = DataManager.CreateCommand(updateText);
				adapter.InsertCommand = DataManager.CreateCommand(insertText);
				adapter.DeleteCommand = DataManager.CreateCommand(deleteText);

				// Fixed mappings
				adapter.UpdateCommand.Parameters["@accountID"].Value = accountID;
				adapter.InsertCommand.Parameters["@accountID"].Value = accountID;
				adapter.DeleteCommand.Parameters["@accountID"].Value = accountID;
				adapter.UpdateCommand.Parameters["@campaignGK"].SourceColumn = "CampaignGK";
				adapter.UpdateCommand.Parameters["@adgroupGK"].SourceColumn = "AdgroupGK";
				adapter.InsertCommand.Parameters["@campaignGK"].SourceColumn = "CampaignGK";
				adapter.InsertCommand.Parameters["@adgroupGK"].SourceColumn = "AdgroupGK";
				adapter.DeleteCommand.Parameters["@campaignGK"].SourceColumn = "CampaignGK";
				adapter.DeleteCommand.Parameters["@adgroupGK"].SourceColumn = "AdgroupGK";

				// Dynamic mappings (targets)
				for (int i = 0; i < measures.Length; i++)
				{
					Measure m = measures[i];
					adapter.UpdateCommand.Parameters[String.Format("@{0}", m.FieldName)].SourceColumn = m.FieldName;
					adapter.InsertCommand.Parameters[String.Format("@{0}", m.FieldName)].SourceColumn = m.FieldName;
				}

				adapter.Update(table);
			}


		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.ChannelDataTable Channel_Get()
		{
			return From<ChannelTableAdapter>().Get();
		}

		public Oltp.CampaignStatusDataTable CampaignStatus_Get()
		{
			return From<CampaignStatusTableAdapter>().Get();
		}

		/// <summary>
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.SearchEngineDataTable SearchEngine_Get()
		{
			return From<SearchEngineTableAdapter>().Get();
		}

		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.AccountDataTable Account_Get()
		{
			return From<AccountTableAdapter>().Get(CurrentUser.ID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.AccountDataTable Account_Save(Oltp.AccountDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<AccountTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.RelatedAccountDataTable RelatedAccount_Get(int accountID)
		{
			return From<RelatedAccountTableAdapter>().Get(accountID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public void RelatedAccount_Save(Oltp.RelatedAccountDataTable table)
		{
			From<RelatedAccountTableAdapter>().Update(table);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="adunitID"></param>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_GetByIdentifier(int accountID, long identifier)
		{
			return From<GatewayTableAdapter>().GetByIdentifier(accountID, identifier);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="adunitID"></param>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_Get(int adunitID)
		{
			return From<GatewayTableAdapter>().Get(adunitID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="adunitID"></param>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_GetGateways(int accountID, int? channelID, int?[] segments)
		{
			if (segments != null && segments.Length < 5)
				throw new ArgumentException("Segments array must be contain 5 values. Use null for each segment value that is not required.", "segments");

			return From<GatewayTableAdapter>().GetGateways(
				accountID,
				channelID,
				segments == null ? null : segments[0],
				segments == null ? null : segments[1],
				segments == null ? null : segments[2],
				segments == null ? null : segments[3],
				segments == null ? null : segments[4]
				);

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ranges"></param>
		/// <param name="channelID"></param>
		/// <param name="segments"></param>
		public int[] Gateway_BatchProperties(int accountID, long[][] ranges, int? channelID, long? pageGK, int?[] segments)
		{
			if (segments != null && segments.Length < 5)
				throw new ArgumentException("Segments array must contain 5 values. Use null for each segment value that is not required.", "segments");
			
			int existing = 0;
			int affected = 0;

			GatewayTableAdapter adapter = From<GatewayTableAdapter>();
			adapter.CurrentConnection.Open();
			DataManager.Current.OpenConnection(adapter.CurrentConnection);

			try
			{
				adapter.CurrentTransaction = adapter.CurrentConnection.BeginTransaction("BatchGatewayProperites");
				DataManager.Current.StartTransaction(adapter.CurrentTransaction);

				SqlCommand countExistingCmd = DataManager.CreateCommand(@"
					select count(1)
					from UserProcess_GUI_Gateway
					where
						Account_ID = @accountID:Int and
						Gateway_id between @fromID:BigInt and @toID:BigInt
				");
				countExistingCmd.Connection = adapter.CurrentConnection;
				countExistingCmd.Transaction = adapter.CurrentTransaction;

				#region Not used...
				/*
				const string updateCmdText = @"
				update UserProcess_GUI_Gateway
				set
					Channel_ID = isnull(@channelID, Channel_ID),
					Page_GK = isnull(@pageGK, Page_GK),
					Segment1 = isnull(@segment1, Segment1),
					Segment2 = isnull(@segment2, Segment2),
					Segment3 = isnull(@segment3, Segment3),
					Segment4 = isnull(@segment4, Segment4),
					Segment5 = isnull(@segment5, Segment5)
				where
					Account_ID = @accountID and
					Gateway_id between @fromID and @toID
				";
				*/
				#endregion


				foreach (long[] range in ranges)
				{
					long rangeLower = range[0];
					long rangeUpper = range.Length > 1 ? range[1] : range[0];

					// Count existing GKs within the range
					countExistingCmd.Parameters["@accountID"].Value = accountID;
					countExistingCmd.Parameters["@fromID"].Value = rangeLower;
					countExistingCmd.Parameters["@toID"].Value = rangeUpper;

					object count = countExistingCmd.ExecuteScalar();
					try { existing += Convert.ToInt32(count); }
					catch { }

					// Activate the GKs of these gateways, creating them even if they don't exist
					if (existing < rangeUpper-rangeLower+1)
					{
						for (long identifier = rangeLower; identifier <= rangeUpper; identifier++)
						{
							long gk = GkManager.GetGatewayGK(accountID, identifier);
						}
					}

					affected += adapter.BatchProperties(
						accountID,
						rangeLower,
						rangeUpper,
						channelID,
						pageGK,
						segments == null ? null : segments[0],
						segments == null ? null : segments[1],
						segments == null ? null : segments[2],
						segments == null ? null : segments[3],
						segments == null ? null : segments[4]);
				}


				adapter.CurrentTransaction.Commit();
			}
			catch
			{
				try { adapter.CurrentTransaction.Rollback(); }
				catch { };
				throw;
			}
			finally
			{
				DataManager.Current.ClearTransaction();

				if (adapter.CurrentConnection.State == ConnectionState.Open)
					adapter.CurrentConnection.Close();
			}

			return new int[]{affected, existing};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_Save(Oltp.GatewayDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			GatewayTableAdapter adapter = From<GatewayTableAdapter>();

			try
			{
				adapter.Update(table);
			}
			catch
			{
				Dictionary<string, object> paramValues = new Dictionary<string, object>();
				foreach (SqlParameter param in adapter.UpdateCommand.Parameters)
					paramValues.Add(param.ParameterName, param.Value);

				throw;
			}

			// Return only new rows
			return hasNew ? table : null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="adunitID"></param>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_GetByReference(int refType, long refID)
		{
			return From<GatewayTableAdapter>().GetByReference(refType, refID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="fromID"></param>
		/// <param name="toID"></param>
		/// <returns></returns>
		public int Gateway_CountByRange(int accountID, long fromID, long toID, int[] otherAccounts)
		{
			int total = 0;
			int? curval = From<GatewayTableAdapter>().CountByRange(accountID, fromID, toID);
			total += curval == null ? 0 : curval.Value;
			foreach (int otherAccount in otherAccounts)
			{
				int? val = From<GatewayTableAdapter>().CountByRange(accountID, fromID, toID);
				total += val == null ? 0 : val.Value;
			}

			return total;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromID"></param>
		/// <param name="toID"></param>
		/// <param name="returnTable"></param>
		/// <returns></returns>
		public Oltp.GatewayDataTable Gateway_CreateRange(int fromID, int toID, int accountID, int adunitID, long pageGK, string destinationBaseUrl, int[] otherAccounts, bool returnTable, out int count)
		{
			Oltp.GatewayDataTable newGateways = new Oltp.GatewayDataTable();
			int first = fromID < toID ? fromID : toID;
			int last = fromID > toID ? fromID : toID;

			// Get any gateways defined within this range
			Oltp.GatewayDataTable existingGateways = From<GatewayTableAdapter>().GetByIdentifierRange(accountID, first, last);
			if (otherAccounts != null)
			{
				foreach (int otherAccount in otherAccounts)
					existingGateways.Merge(From<GatewayTableAdapter>().GetByIdentifierRange(otherAccount, first, last));
			}

			for (int i = first; i <= last; i++)
			{
				// Ignore if it exists
				if (existingGateways.Select(String.Format("Identifier = {0}", i)).Length > 0)
					continue;

				// Create the gateway with default data
				Oltp.GatewayRow gw = newGateways.NewGatewayRow();
				gw.AccountID = accountID;
				gw.Identifier = i;
				gw.Name = i.ToString();
				gw.DestinationURL = String.Format(destinationBaseUrl, i);

				if (pageGK > -1)
					gw.PageGK = pageGK;
				else
					gw.SetPageGKNull();

				newGateways.AddGatewayRow(gw);
			}

			if (newGateways.Rows.Count > 0)
				count = From<GatewayTableAdapter>().Update(newGateways);
			else
				count = 0;

			if (returnTable)
			{
				return newGateways;
			}
			else
			{
				return null;
			}
		}

		private int[] GetCrossCheckAccounts(Oltp.GatewayReservationRow reservation)
		{
			if (reservation.CrossCheckAccounts == null)
				return new int[0];

			// Get other accounts to check against
			string[] otherAccountStrings = reservation.CrossCheckAccounts.Split(',');
			List<int> otherAccounts = new List<int>();
			for (int i = 0; i < otherAccountStrings.Length; i++)
			{
				int otherAccount;
				if (Int32.TryParse(otherAccountStrings[i].Trim(), out otherAccount))
					otherAccounts.Add(otherAccount);
			}

			return otherAccounts.ToArray();
		}


		public Oltp.GatewayDataTable Gateway_CreateQuantity(int quantity, int accountID, int adunitID, long pageGK, string destinationBaseUrl, bool returnTable)
		{
			Oltp.GatewayReservationDataTable reservations = From<GatewayReservationTableAdapter>().GetByPage(accountID, pageGK);
			if (reservations.Count == 0)
				return new Oltp.GatewayDataTable();

			Oltp.GatewayDataTable existingGateways = new Oltp.GatewayDataTable();
			foreach (Oltp.GatewayReservationRow reservation in reservations.Rows)
			{
				int[] otherAccounts = GetCrossCheckAccounts(reservation);
				existingGateways.Merge(From<GatewayTableAdapter>().GetByIdentifierRange(accountID, reservation.FromGateway, reservation.ToGateway));
				if (otherAccounts != null)
				{
					foreach (int otherAccount in otherAccounts)
					{
						existingGateways.Merge(From<GatewayTableAdapter>().GetByIdentifierRange(otherAccount, reservation.FromGateway, reservation.ToGateway));
					}
				}
			}


			int curRsvIndex = 0;
			Oltp.GatewayReservationRow curRsv = reservations.Rows[curRsvIndex] as Oltp.GatewayReservationRow;
			long nextIdentifier = curRsv.FromGateway;

			Oltp.GatewayDataTable newGateways = new Oltp.GatewayDataTable();
			for (int i = 0; i < quantity; i++)
			{
				bool exit = false;
				bool okay = false;
				while (!okay)
				{
					if (nextIdentifier > curRsv.ToGateway)
					{
						if (curRsvIndex == reservations.Rows.Count -1)
						{
							exit = true;
							break;
						}
						else
						{
							curRsv = reservations.Rows[++curRsvIndex] as Oltp.GatewayReservationRow;
							nextIdentifier = curRsv.FromGateway;
						}
					}

					if (existingGateways.Select(String.Format("Identifier = {0}", nextIdentifier)).Length > 0)
					{
						nextIdentifier++;
					}
					else
					{
						okay = true;
					}
				}

				if (exit == true)
					break;

				// Create the gateway with default data
				Oltp.GatewayRow gw = newGateways.NewGatewayRow();
				gw.AccountID = accountID;
				gw.Identifier = (int) nextIdentifier;
				gw.Name = nextIdentifier.ToString();
				gw.DestinationURL = String.Format(destinationBaseUrl, nextIdentifier);

				if (pageGK > -1)
					gw.PageGK = pageGK;
				else
					gw.SetPageGKNull();

				newGateways.AddGatewayRow(gw);
				nextIdentifier++;
			}

			From<GatewayTableAdapter>().Update(newGateways);

			if (returnTable)
			{
				return newGateways;
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayID"></param>
		/// <param name="adunitID"></param>
		/// <returns></returns>
		public int Gateway_Associate(long gatewayGK, int adunitID)
		{
			return From<GatewayTableAdapter>().AssociateGateway(adunitID, gatewayGK);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentAdunitID"></param>
		/// <param name="newAdunitID"></param>
		/// <returns></returns>
		public int Gateway_AssociateByAdunit(int currentAdunitID, int newAdunitID)
		{
			return From<GatewayTableAdapter>().AssociateGateways(newAdunitID, currentAdunitID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayIDs"></param>
		/// <param name="newAdunitID"></param>
		/// <returns></returns>
		public int Gateway_AssociateBatch(long[] gatewayGKs, int newAdunitID)
		{
			GatewayTableAdapter adapter = From<GatewayTableAdapter>();
			int count = 0;

			foreach (long gatewayGK in gatewayGKs)
			{
				count += adapter.AssociateGateway(newAdunitID, gatewayGK);
			}

			return count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <returns></returns>
		public Oltp.GatewayReservationDataTable GatewayReservation_Get(int accountID)
		{
			return From<GatewayReservationTableAdapter>().Get(accountID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Oltp.GatewayReservationDataTable GatewayReservation_Save(Oltp.GatewayReservationDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<GatewayReservationTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}


		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="pageGK"></param>
		/// <returns></returns>
		public Oltp.GatewayReservationDataTable GatewayReservation_GetByPage(int accountID, long pageGK)
		{
			return From<GatewayReservationTableAdapter>().GetByPage(accountID, pageGK);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public Oltp.GatewayReservationDataTable GatewayReservation_GetByIdentifier(int accountID, long identifier)
		{
			return From<GatewayReservationTableAdapter>().GetByIdentifier(accountID, identifier);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="fromID"></param>
		/// <param name="toID"></param>
		/// <returns></returns>
		public Oltp.GatewayReservationDataTable GatewayReservation_GetByOverlap(int accountID, long fromID, long toID, int[] otherAccounts)
		{
			Oltp.GatewayReservationDataTable results = From<GatewayReservationTableAdapter>().GetByOverlap(accountID, fromID, toID);

			if (results.Count < 1)
			{
				foreach (int otherAccount in otherAccounts)
				{
					results.Merge(From<GatewayReservationTableAdapter>().GetByOverlap(accountID, fromID, toID));
				}
			}

			return results;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="keywordFilter"></param>
		/// <returns></returns>
		public Oltp.KeywordDataTable Keyword_Get(int accountID, bool includeRelated, string keywordFilter, bool includeUnmonitored)
		{
			return From<KeywordTableAdapter>().Get(accountID, includeRelated, keywordFilter, includeUnmonitored);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="keywordFilter"></param>
		/// <returns></returns>
		public Oltp.KeywordDataTable Keyword_GetSingle(long keywordGK)
		{
			return From<KeywordTableAdapter>().GetSingle(keywordGK);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="phrases"></param>
		/// <returns></returns>
		public Oltp.KeywordDataTable Keyword_FindForPhrases(int accountID, string[] phrases)
		{
			Oltp.KeywordDataTable results = new Oltp.KeywordDataTable();
			foreach (string phrase in phrases)
			{
				Oltp.KeywordDataTable tbl = From<KeywordTableAdapter>().Get(accountID, false, phrase.Trim(), true);
				if (tbl.Rows.Count > 0 && results.FindByGK((tbl.Rows[0] as Oltp.KeywordRow).GK) == null)
				{
					results.ImportRow(tbl.Rows[0]);
				}
			}

			return results;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public Oltp.KeywordDataTable Keyword_Save(Oltp.KeywordDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<KeywordTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="keywordFilter"></param>
		/// <returns></returns>
		public Oltp.CreativeDataTable Creative_Get(int accountID, string titleFilter, bool includeUnmonitored)
		{
			return From<CreativeTableAdapter>().Get(accountID, titleFilter, includeUnmonitored);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="keywordFilter"></param>
		/// <returns></returns>
		public Oltp.CreativeDataTable Creative_GetSingle(long creativeGK)
		{
			return From<CreativeTableAdapter>().GetSingle(creativeGK);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public Oltp.CreativeDataTable Creative_Save(Oltp.CreativeDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<CreativeTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="accountID"></param>
		/// <param name="keywordFilter"></param>
		/// <returns></returns>
		public Oltp.PageDataTable Page_Get(int accountID, string pageFilter, bool includeUnmonitored, long resultLimit)
		{
			return From<PageTableAdapter>().Get(accountID, pageFilter, includeUnmonitored, resultLimit < 0 ? long.MaxValue : resultLimit);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public Oltp.PageDataTable Page_Save(Oltp.PageDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<PageTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}
		public Oltp.CampaignDataTable Campaign_Get(int accountID, int? channelID, int? statusID, string filter, bool filterByAdgroup)
		{
			return From<CampaignTableAdapter>().Get(accountID, channelID, statusID, filter, filterByAdgroup);
		}

		public Oltp.CampaignDataTable Campaign_GetIndividualCampaigns(long[] campaignGKs)
		{
			List<long> resultGKs = new List<long>();
			Oltp.CampaignDataTable results  = new Oltp.CampaignDataTable();
			foreach (long gk in campaignGKs)
			{
				Oltp.CampaignDataTable tbl = From<CampaignTableAdapter>().GetSingle(gk);
				if (tbl.Rows.Count > 0 && !resultGKs.Contains((tbl.Rows[0] as Oltp.CampaignRow).GK))
				{
					resultGKs.Add((tbl.Rows[0] as Oltp.CampaignRow).GK);
					results.ImportRow(tbl.Rows[0]);
				}
			}

			return results;
		}

		public Oltp.CampaignDataTable Campaign_GetSingle(long campaignGK)
		{
			return From<CampaignTableAdapter>().GetSingle(campaignGK);
		}

		public Oltp.CampaignDataTable Campaign_Save(Oltp.CampaignDataTable table, bool useBackOffice)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			List<SqlCommand> commands = new List<SqlCommand>();

			SqlCommand updateAdgroups = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroup
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Campaign_GK = @campaignGK:BigInt"
			);

			SqlCommand updateKeywords = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupKeyword
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Campaign_GK = @campaignGK:BigInt"
			);

			SqlCommand updateCreatives = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupCreative
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Campaign_GK = @campaignGK:BigInt"
			);

			SqlCommand updateSites = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupSite
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Campaign_GK = @campaignGK:BigInt"
			);

			commands.Add(updateAdgroups);
			commands.Add(updateKeywords);
			commands.Add(updateCreatives);
			commands.Add(updateSites);
			
			if (useBackOffice)
			{
				SqlCommand updateGateways = DataManager.CreateCommand
				(
					@"
					update	UserProcess_GUI_Gateway
					set		Channel_ID = @channelID:Int,
							Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
							Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
							Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
							Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
							Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
							LastUpdated = getdate()
					where	Account_ID = @accountID:Int and Campaign_GK = @campaignGK:BigInt"
				);

				commands.Add(updateGateways);
			}

			// Create transaction and assign it
			CampaignTableAdapter adapter = From<CampaignTableAdapter>();
			adapter.CurrentConnection.Open();
			adapter.CurrentTransaction = adapter.CurrentConnection.BeginTransaction("CampaignUpdateChildSegments");
			foreach (SqlCommand cmd in commands)
			{
				cmd.Connection = adapter.CurrentConnection;
				cmd.Transaction = adapter.CurrentTransaction;
			}

			
			Oltp.SegmentRow[] segments = new Oltp.SegmentRow[5];
			int cutAccountID = -99;

			try
			{
				foreach (Oltp.CampaignRow camp in table.Rows)
				{
					if (camp.AccountID != cutAccountID)
					{
						cutAccountID = camp.AccountID;
						Oltp.SegmentDataTable segmentTable = Segment_Get(camp.AccountID, false);

						// Find segments
						foreach (Oltp.SegmentRow seg in segmentTable)
							segments[seg.SegmentNumber-1] = seg;
					}

					foreach (SqlCommand cmd in commands)
					{
						cmd.Parameters["@accountID"].Value = camp.AccountID;
						cmd.Parameters["@campaignGK"].Value = camp.GK;
						cmd.Parameters["@channelID"].Value = camp.ChannelID;
						cmd.Parameters["@segment1"].Value = camp.IsSegment1Null() || segments[0] == null || (segments[0].AssociationFlags & SegmentAssociationFlags.Campaign) == 0 || camp[table.Segment1Column, DataRowVersion.Current].Equals(camp[table.Segment1Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) camp.Segment1;
						cmd.Parameters["@segment2"].Value = camp.IsSegment2Null() || segments[1] == null || (segments[1].AssociationFlags & SegmentAssociationFlags.Campaign) == 0 || camp[table.Segment2Column, DataRowVersion.Current].Equals(camp[table.Segment2Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) camp.Segment2;
						cmd.Parameters["@segment3"].Value = camp.IsSegment3Null() || segments[2] == null || (segments[2].AssociationFlags & SegmentAssociationFlags.Campaign) == 0 || camp[table.Segment3Column, DataRowVersion.Current].Equals(camp[table.Segment3Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) camp.Segment3;
						cmd.Parameters["@segment4"].Value = camp.IsSegment4Null() || segments[3] == null || (segments[3].AssociationFlags & SegmentAssociationFlags.Campaign) == 0 || camp[table.Segment4Column, DataRowVersion.Current].Equals(camp[table.Segment4Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) camp.Segment4;
						cmd.Parameters["@segment5"].Value = camp.IsSegment5Null() || segments[4] == null || (segments[4].AssociationFlags & SegmentAssociationFlags.Campaign) == 0 || camp[table.Segment5Column, DataRowVersion.Current].Equals(camp[table.Segment5Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) camp.Segment5;
						int result = cmd.ExecuteNonQuery();
					}
				}
				
				adapter.Update(table);
				adapter.CurrentTransaction.Commit();
			}
			catch
			{
				try { adapter.CurrentTransaction.Rollback(); }
				catch { };

				throw;
			}
			finally
			{
				if (adapter.CurrentConnection.State == ConnectionState.Open)
					adapter.CurrentConnection.Close();
			}

			// Return only new rows
			return hasNew ? table : null;
		}

		public void Campaign_Merge(int accountID, long targetCampaignGK, long[] otherCampaignGKs)
		{
			SqlCommand cmd = DataManager.CreateCommand
			(
				"Sp_Campaign_name_assignment(@account_id:NVarChar, @new_Campaign_gk:NVarChar, @old_Campaign_gk:NVarChar)",
				CommandType.StoredProcedure
			);

			cmd.Parameters["@account_id"].Value = accountID;
			cmd.Parameters["@new_Campaign_gk"].Value = targetCampaignGK;
			cmd.CommandTimeout = 360000;

			CampaignTableAdapter adapter = From<CampaignTableAdapter>();
			adapter.CurrentConnection.Open();
			try
			{
				adapter.CurrentTransaction = adapter.CurrentConnection.BeginTransaction("CampaignUpdateChildSegments");
				cmd.Connection = adapter.CurrentConnection;
				cmd.Transaction = adapter.CurrentTransaction;

				foreach (long campaignGK in otherCampaignGKs)
				{
					cmd.Parameters["@old_Campaign_gk"].Value = campaignGK;
					cmd.ExecuteNonQuery();
				}

				adapter.CurrentTransaction.Commit();
			}
			catch
			{
				try
				{
					if (adapter.CurrentTransaction != null)
						adapter.CurrentTransaction.Rollback();
				}
				catch { };

				throw;
			}
			finally
			{
				if (adapter.CurrentConnection.State == ConnectionState.Open)
					adapter.CurrentConnection.Close();
			}
		}

		public Oltp.AdgroupDataTable Adgroup_Get(long campaignGK, string filter)
		{
			return From<AdgroupTableAdapter>().Get(campaignGK, filter);
		}

		public Oltp.AdgroupDataTable Adgroup_GetSingle(long adgroupGK)
		{
			return From<AdgroupTableAdapter>().GetSingle(adgroupGK);
		}

		public Oltp.AdgroupDataTable Adgroup_GetByKeyword(long keywordGK)
		{
			return From<AdgroupTableAdapter>().GetByKeyword(keywordGK);
		}

		public Oltp.AdgroupDataTable Adgroup_GetByCreative(long creativeGK)
		{
			return From<AdgroupTableAdapter>().GetByCreative(creativeGK);
		}

		public Oltp.AdgroupDataTable Adgroup_Save(Oltp.AdgroupDataTable table, bool useBackOffice)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			List<SqlCommand> commands = new List<SqlCommand>();

			SqlCommand updateKeywords = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupKeyword
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Adgroup_GK = @adgroupGK:BigInt"
			);
		
			SqlCommand updateCreatives = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupCreative
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Adgroup_GK = @adgroupGK:BigInt"
			);

			SqlCommand updateSites = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupSite
				set		Channel_ID = @channelID:Int,
						Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Adgroup_GK = @adgroupGK:BigInt"
			);

			commands.Add(updateKeywords);
			commands.Add(updateCreatives);
			commands.Add(updateSites);

			if (useBackOffice)
			{
				SqlCommand updateGateways = DataManager.CreateCommand
				(
					@"
					update	UserProcess_GUI_Gateway
					set		Channel_ID = @channelID:Int,
							Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
							Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
							Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
							Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
							Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
							LastUpdated = getdate()
					where	Account_ID = @accountID:Int and Adgroup_GK = @adgroupGK:BigInt"
				);

				commands.Add(updateGateways);
			}

			// Create transaction and assign it
			AdgroupTableAdapter adapter = From<AdgroupTableAdapter>();
			adapter.CurrentConnection.Open();
			adapter.CurrentTransaction = adapter.CurrentConnection.BeginTransaction("AdgroupUpdateChildSegments");
			foreach (SqlCommand cmd in commands)
			{
				cmd.Connection = adapter.CurrentConnection;
				cmd.Transaction = adapter.CurrentTransaction;
				cmd.CommandTimeout = (int) TimeSpan.FromMinutes(5).TotalMilliseconds;
			}

			Oltp.SegmentRow[] segments = new Oltp.SegmentRow[5];
			int cutAccountID = -99;

			try
			{
				foreach (Oltp.AdgroupRow adg in table.Rows)
				{
					if (adg.AccountID != cutAccountID)
					{
						cutAccountID = adg.AccountID;
						Oltp.SegmentDataTable segmentTable = Segment_Get(adg.AccountID, false);

						// Find segments
						foreach (Oltp.SegmentRow seg in segmentTable)
							segments[seg.SegmentNumber-1] = seg;
					}

					foreach (SqlCommand cmd in commands)
					{
						cmd.Parameters["@accountID"].Value = adg.AccountID;
						cmd.Parameters["@adgroupGK"].Value = adg.GK;
						cmd.Parameters["@channelID"].Value = adg.ChannelID;
						cmd.Parameters["@segment1"].Value = adg.IsSegment1Null() || segments[0] == null || (segments[0].AssociationFlags & SegmentAssociationFlags.Adgroup) == 0 || adg[table.Segment1Column, DataRowVersion.Current].Equals(adg[table.Segment1Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adg.Segment1;
						cmd.Parameters["@segment2"].Value = adg.IsSegment2Null() || segments[1] == null || (segments[1].AssociationFlags & SegmentAssociationFlags.Adgroup) == 0 || adg[table.Segment2Column, DataRowVersion.Current].Equals(adg[table.Segment2Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adg.Segment2;
						cmd.Parameters["@segment3"].Value = adg.IsSegment3Null() || segments[2] == null || (segments[2].AssociationFlags & SegmentAssociationFlags.Adgroup) == 0 || adg[table.Segment3Column, DataRowVersion.Current].Equals(adg[table.Segment3Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adg.Segment3;
						cmd.Parameters["@segment4"].Value = adg.IsSegment4Null() || segments[3] == null || (segments[3].AssociationFlags & SegmentAssociationFlags.Adgroup) == 0 || adg[table.Segment4Column, DataRowVersion.Current].Equals(adg[table.Segment4Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adg.Segment4;
						cmd.Parameters["@segment5"].Value = adg.IsSegment5Null() || segments[4] == null || (segments[4].AssociationFlags & SegmentAssociationFlags.Adgroup) == 0 || adg[table.Segment5Column, DataRowVersion.Current].Equals(adg[table.Segment5Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adg.Segment5;
						int result = cmd.ExecuteNonQuery();
					}
				}

				adapter.Update(table);
				adapter.CurrentTransaction.Commit();
			}
			catch
			{
				try { adapter.CurrentTransaction.Rollback(); }
				catch { };

				throw;
			}
			finally
			{
				if (adapter.CurrentConnection.State == ConnectionState.Open)
					adapter.CurrentConnection.Close();
			}

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.AdgroupKeywordDataTable AdgroupKeyword_Get(long adgroupGK)
		{
			return From<AdgroupKeywordTableAdapter>().Get(adgroupGK);
		}

		public Oltp.AdgroupKeywordDataTable AdgroupKeyword_Save(Oltp.AdgroupKeywordDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<AdgroupKeywordTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.AdgroupKeywordDataTable AdgroupKeyword_GetSingle(long adgroupKeywordGK)
		{
			return From<AdgroupKeywordTableAdapter>().GetSingle(adgroupKeywordGK);
		}

		public Oltp.AdgroupCreativeDataTable AdgroupCreative_Get(long adgroupGK)
		{
			return From<AdgroupCreativeTableAdapter>().Get(adgroupGK);
		}

		public Oltp.AdgroupCreativeDataTable AdgroupCreative_Save(Oltp.AdgroupCreativeDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;

			SqlCommand cmd = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupCreative
				set		Segment1 = (case when @segment1:Int is null then Segment1 else @segment1 end),
						Segment2 = (case when @segment2:Int is null then Segment2 else @segment2 end),
						Segment3 = (case when @segment3:Int is null then Segment3 else @segment3 end),
						Segment4 = (case when @segment4:Int is null then Segment4 else @segment4 end),
						Segment5 = (case when @segment5:Int is null then Segment5 else @segment5 end),
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and Creative_GK = @creativeGK:BigInt"
			); 

			SqlCommand pageCmd = DataManager.CreateCommand
			(
				@"
				update	UserProcess_GUI_PaidAdgroupCreative
				set		Page_GK = @pageGK:BigInt,
						LastUpdated = getdate()
				where	Account_ID = @accountID:Int and PPC_Creative_GK = @adgroupCreativeGK:BigInt"
			);

			// Create transaction and assign it
			AdgroupCreativeTableAdapter adapter = From<AdgroupCreativeTableAdapter>();
			adapter.CurrentConnection.Open();
			adapter.CurrentTransaction = adapter.CurrentConnection.BeginTransaction("AdgroupCreativeUpdate");
			pageCmd.Connection = cmd.Connection = adapter.CurrentConnection;
			pageCmd.Transaction = cmd.Transaction = adapter.CurrentTransaction;
			
			Oltp.SegmentRow[] segments = new Oltp.SegmentRow[5];
			int cutAccountID = -99;

			List<long> updatedCreativeGKs = new List<long>();
			foreach (Oltp.AdgroupCreativeRow adgCreative in table.Rows)
			{
				// First update the page
				pageCmd.Parameters["@accountID"].Value = adgCreative.AccountID;
				pageCmd.Parameters["@pageGK"].Value = adgCreative.PageGK;
				pageCmd.Parameters["@adgroupCreativeGK"].Value = adgCreative.AdgroupCreativeGK;
				pageCmd.ExecuteNonQuery();

				if(updatedCreativeGKs.Contains(adgCreative.CreativeGK))
					continue;

				if (adgCreative.AccountID != cutAccountID)
				{
					cutAccountID = adgCreative.AccountID;
					Oltp.SegmentDataTable segmentTable = Segment_Get(adgCreative.AccountID, false);

					// Find segments
					foreach (Oltp.SegmentRow seg in segmentTable)
						segments[seg.SegmentNumber-1] = seg;
				}

				cmd.Parameters["@accountID"].Value = adgCreative.AccountID;
				cmd.Parameters["@creativeGK"].Value = adgCreative.CreativeGK;
				cmd.Parameters["@segment1"].Value = adgCreative.IsSegment1Null() || segments[0] == null || (segments[0].AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0 || adgCreative[table.Segment1Column, DataRowVersion.Current].Equals(adgCreative[table.Segment1Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adgCreative.Segment1;
				cmd.Parameters["@segment2"].Value = adgCreative.IsSegment2Null() || segments[1] == null || (segments[1].AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0 || adgCreative[table.Segment2Column, DataRowVersion.Current].Equals(adgCreative[table.Segment2Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adgCreative.Segment2;
				cmd.Parameters["@segment3"].Value = adgCreative.IsSegment3Null() || segments[2] == null || (segments[2].AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0 || adgCreative[table.Segment3Column, DataRowVersion.Current].Equals(adgCreative[table.Segment3Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adgCreative.Segment3;
				cmd.Parameters["@segment4"].Value = adgCreative.IsSegment4Null() || segments[3] == null || (segments[3].AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0 || adgCreative[table.Segment4Column, DataRowVersion.Current].Equals(adgCreative[table.Segment4Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adgCreative.Segment4;
				cmd.Parameters["@segment5"].Value = adgCreative.IsSegment5Null() || segments[4] == null || (segments[4].AssociationFlags & SegmentAssociationFlags.AdgroupCreative) == 0 || adgCreative[table.Segment5Column, DataRowVersion.Current].Equals(adgCreative[table.Segment5Column, DataRowVersion.Original]) ? (object) DBNull.Value : (object) adgCreative.Segment5;
				int result = cmd.ExecuteNonQuery();

				updatedCreativeGKs.Add(adgCreative.CreativeGK);
			}
			
			adapter.CurrentTransaction.Commit();
			
			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.AdgroupCreativeDataTable AdgroupCreative_GetSingle(long adgroupCreativeGK)
		{
			return From<AdgroupCreativeTableAdapter>().GetSingle(adgroupCreativeGK);
		}


		public Oltp.SerpProfileDataTable SerpProfile_Get(int accountID, string profileType)
		{
			return From<SerpProfileTableAdapter>().Get(accountID, profileType);
		}

		public bool SerpProfile_CanRunNow(int accountID, int profileID)
		{
			SqlCommand cmd = DataManager.CreateCommand("select count(*) from Rankings_Data where Account_ID = @accountID:Int and ProfileID = @profileID:Int and Day_Code = @dayCode:Int");
			cmd.Parameters["@accountID"].Value = accountID;
			cmd.Parameters["@profileID"].Value = profileID;
			cmd.Parameters["@dayCode"].Value = DayCode.ToDayCode(DateTime.Now);

			SerpProfileTableAdapter adapter = new SerpProfileTableAdapter();
			cmd.Connection = adapter.CurrentConnection;
			cmd.Connection.Open();

			bool retVal;
			using (cmd.Connection)
			{
				object val = cmd.ExecuteScalar();
				retVal = !(val is int && ((int)val) > 0);
			}

			return retVal;
		}

		public void SerpProfile_RunNow(int accountID, int profileID)
		{
			// Schedule a google organic
			using (ServiceClient<IOrganicServiceDelegator> serviceDelegator = new ServiceClient<IOrganicServiceDelegator>())
			{
				serviceDelegator.Service.RunOrganicProfile(accountID, profileID);
			}
		}

		public Oltp.SerpProfileDataTable SerpProfile_Save(Oltp.SerpProfileDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			From<SerpProfileTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.SerpProfileKeywordDataTable SerpProfileKeyword_Get(int profileID)
		{
			return From<SerpProfileKeywordTableAdapter>().Get(profileID);
		}

		public void SerpProfileKeyword_Save(Oltp.SerpProfileKeywordDataTable table)
		{
			From<SerpProfileKeywordTableAdapter>().Update(table);
		}

		public Oltp.SerpProfileDomainGroupDataTable SerpProfileDomainGroup_Get(int profileID)
		{
			return From<SerpProfileDomainGroupTableAdapter>().Get(profileID);
		}

		public Oltp.SerpProfileDomainGroupDataTable SerpProfileDomainGroup_Save(Oltp.SerpProfileDomainGroupDataTable table)
		{
			From<SerpProfileDomainGroupTableAdapter>().Update(table);
			table.DefaultView.Sort = table.DisplayPositionColumn.ColumnName;

			Oltp.SerpProfileDomainGroupDataTable newTable = new Oltp.SerpProfileDomainGroupDataTable();
			foreach (DataRow r in table.DefaultView.ToTable().Rows)
				newTable.ImportRow(r);
			newTable.AcceptChanges();

			return newTable;
		}

		public Oltp.SerpProfileDomainDataTable SerpProfileDomain_Get(int profileID)
		{
			return From<SerpProfileDomainTableAdapter>().Get(profileID);
		}

		public Oltp.SerpProfileDomainDataTable SerpProfileDomain_Save(Oltp.SerpProfileDomainDataTable table)
		{
			From<SerpProfileDomainTableAdapter>().Update(table);
			//table.DefaultView.Sort = String.Format("{0} DESC, {1}",
			//    table.IsAccountDomainColumn.ColumnName,
			//    table.DisplayPositionColumn.ColumnName);

			//Oltp.SerpProfileDomainDataTable newTable = new Oltp.SerpProfileDomainDataTable();
			//foreach(DataRow r in table.DefaultView.ToTable().Rows)
			//    newTable.ImportRow(r);
			//newTable.AcceptChanges();

			//return newTable;
			return table;
		}

		public Oltp.SerpProfileSearchEngineDataTable SerpProfileSearchEngine_Get(int profileID)
		{
			return From<SerpProfileSearchEngineTableAdapter>().Get(profileID);
		}

		public Oltp.SerpProfileSearchEngineDataTable SerpProfileSearchEngine_Save(Oltp.SerpProfileSearchEngineDataTable table)
		{
			From<SerpProfileSearchEngineTableAdapter>().Update(table);
			table.DefaultView.Sort = table.DisplayPositionColumn.ColumnName;

			Oltp.SerpProfileSearchEngineDataTable newTable = new Oltp.SerpProfileSearchEngineDataTable();
			foreach (DataRow r in table.DefaultView.ToTable().Rows)
				newTable.ImportRow(r);
			newTable.AcceptChanges();

			return newTable;
		}

		const int C_PageSegmentID = 999;
		public Oltp.SegmentDataTable Segment_Get(int accountID, bool includePages)
		{
			Oltp.SegmentDataTable table = From<SegmentTableAdapter>().Get(accountID);

			if (includePages)
			{
				//................................
				// Simulate page as a segment
				Oltp.SegmentRow pageSegment = table.NewSegmentRow();
				pageSegment.AccountID = -1;
				pageSegment.SegmentID = C_PageSegmentID;
				pageSegment.AssociationFlags = SegmentAssociationFlags.AdgroupCreative;
				pageSegment.Name = "Landing Page";
				pageSegment.SegmentNumber = C_PageSegmentID;
				pageSegment.Description = "The landing page to which the user is directed.";

				table.Rows.Add(pageSegment);
				table.AcceptChanges();
				//................................
			}

			return table;
		}

		public Oltp.SegmentDataTable Segment_Save(Oltp.SegmentDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;

			//................................
			// Simulate page as a segment
			DataRow[] rs = table.Select(String.Format("SegmentID = {0}", C_PageSegmentID));
			if (rs.Length > 0)
			{
				table.Rows.Remove(rs[0]);
			}
			//................................

			From<SegmentTableAdapter>().Update(table);

			// Return only new rows
			return hasNew ? table : null;
		}

		public Oltp.SegmentValueDataTable SegmentValue_Get(int accountID, int segmentID)
		{
			Oltp.SegmentValueDataTable table;
			
			if (segmentID == C_PageSegmentID)
			{
				//................................
				// Simulate page as a segment
				Oltp.PageDataTable pageTable = Page_Get(accountID, null, true, -1);
				table = new Oltp.SegmentValueDataTable();
				
				foreach (Oltp.PageRow page in pageTable.Rows)
				{
					Oltp.SegmentValueRow pageValueRow = table.NewSegmentValueRow();
					pageValueRow.SegmentID = C_PageSegmentID;
					pageValueRow.AccountID = accountID;
					pageValueRow.ValueID = Convert.ToInt32(page.GK);
					pageValueRow.Value = page.DisplayName;

					table.Rows.Add(pageValueRow);
				}

				table.AcceptChanges();
				//................................
			}
			else
			{
				table = From<SegmentValueTableAdapter>().Get(accountID, segmentID);
			}

			return table;
		}

		/// <summary>
		/// Saves the segment value table. If there are simulated page segments, regular segments aren't saved
		/// and pages are saved instead.
		/// </summary>
		public Oltp.SegmentValueDataTable SegmentValue_Save(Oltp.SegmentValueDataTable table)
		{
			bool hasNew = table.GetChanges(DataRowState.Added) != null;
			int accountID = -1;
			int segmentID = -1;

			//................................
			// Simulate page as a segment			
			Oltp.PageDataTable pageTable = null;
			foreach (Oltp.SegmentValueRow value in table.Rows)
			{
				accountID = value.AccountID;
				segmentID = value.SegmentID;

				if (value.RowState == DataRowState.Unchanged)
					continue;

				if (value.SegmentID != C_PageSegmentID)
					continue;

				if (pageTable == null)
					pageTable = new Oltp.PageDataTable();

				Oltp.PageRow page = pageTable.NewPageRow();
				page.Title = value.Value;
				page.AccountID = value.AccountID;
				page.GK = value.ValueID;
				pageTable.AddPageRow(page);
				page.AcceptChanges();

				if (value.RowState == DataRowState.Added)
					page.SetAdded();
				else if (value.RowState == DataRowState.Modified)
					page.SetModified();
				else if (value.RowState == DataRowState.Deleted)
					page.Delete();
			}

			// If there is a page table defined, it means that we found page segments, so save them
			// and then re-construct a simulated segment value table that represents them
			if (pageTable != null)
			{
				// Save the fabricated page table
				Oltp.PageDataTable newPageTable = Page_Save(pageTable);
				pageTable = newPageTable != null ? newPageTable : pageTable;

				table = new Oltp.SegmentValueDataTable();
				foreach (Oltp.PageRow page in pageTable.Rows)
				{
					Oltp.SegmentValueRow valueRow = table.NewSegmentValueRow();
					valueRow.SegmentID = C_PageSegmentID;
					valueRow.AccountID = page.AccountID;
					valueRow.ValueID = Convert.ToInt32(page.GK);
					valueRow.Value = page.Title;

					table.Rows.Add(valueRow);
				}
				table.AcceptChanges();
			}

			//................................

			else
			{
				SegmentValueTableAdapter adapter = From<SegmentValueTableAdapter>();
				adapter.InnerAdapter.UpdateCommand = DataManager.CreateCommand(@"
					update SegmentValue set Value = @Value:NVarChar where ValueID = @ValueID:Int"
				);
				adapter.InnerAdapter.UpdateCommand.Parameters["@Value"].SourceColumn = "Value";
				adapter.InnerAdapter.UpdateCommand.Parameters["@ValueID"].SourceColumn = "ValueID";
				adapter.InnerAdapter.UpdateCommand.Connection = adapter.CurrentConnection;
				adapter.Update(table);

				if (hasNew)
					table = SegmentValue_Get(accountID, segmentID);
			}

			// Return only new rows
			return hasNew ? table : null;
		}

		/*=========================*/
		//#endregion
	}
}
