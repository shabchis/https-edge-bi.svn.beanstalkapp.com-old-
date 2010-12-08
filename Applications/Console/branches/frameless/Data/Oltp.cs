using System;
using System.Data;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Data.SqlClient;
using Easynet.Edge.BusinessObjects;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Easynet.Edge.Core;

namespace Easynet.Edge.UI.Data
{ 
    public partial class Oltp
	{
		public class GatewayReferenceType
		{
			public const int Creative = 0;
			public const int Keyword = 1;
		}

		public class SerpProfileType
		{
			public const string PPC = "PPC";
			public const string SEO = "SEO";
		}

		/// <summary>
		/// Converts a table (usually the GetChanges of another table) into a typed table
		/// </summary>
		/// <typeparam name="TableType"></typeparam>
		/// <param name="table"></param>
		/// <returns></returns>
		public static TableType Prepare<TableType>(DataTable table) where TableType: DataTable, new()
		{
			DataTable changes = table.GetChanges();
			if (changes == null)
				return null;

			TableType independentTable = new TableType();

			foreach(DataRow row in changes.Rows)
				independentTable.ImportRow(row);

			return independentTable;
		}

		public partial class AccountRow
		{
			SettingsCollection _settings;
			public SettingsCollection Settings
			{
				get
				{
					if (_settings == null)
						_settings = new SettingsCollection(this.IsAccountSettingsTextNull() ? string.Empty : this.AccountSettingsText);
					return _settings;
				}
			}
		}

		public partial class SegmentRow
		{
			public SegmentAssociationFlags AssociationFlags
			{
				get { return (SegmentAssociationFlags) this.Association; }
				set { this.Association = (int)value; }
			}
		}

		public partial class SegmentValueRow : DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[]{"Value"};

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] {propertyName});
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));

			}

			#endregion
		}

		public partial class AdunitRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[]{"Name", "ChannelID"};

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] {propertyName});
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));

			}

			#endregion
		}

		public partial class GatewayRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Name", "ChannelID" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
				{
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs(prop));

						if (prop == "Name")
							PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
					}
				}

			}

			#endregion

			public string DisplayName
			{
				get
				{
					if (this.IsNameNull() || String.IsNullOrEmpty(this.Name))
						return this.Identifier.ToString();
					else
						return String.Format("{0} ({1})", this.Name, this.Identifier);
				}
			}
		}

		public partial class KeywordRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "IsMonitored" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion
		}

		public partial class CreativeRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Title", "Desc1", "Desc2", "DisplayDescription" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));

			}

			#endregion

			public string DisplayDescription
			{
				get
				{
					return this.Desc1 + " " +this.Desc2;
				}
			}
		}

		public partial class PageRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Title", "URL", "IsMonitored" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));

			}

			#endregion

			public string DisplayName
			{
				get
				{
					return
						!IsTitleNull() && Title.Trim().Length > 0 ? this.Title :
						!IsURLNull() && URL.Trim().Length > 0 ? this.URL :
						String.Format("(page #{0})", this.GK);
				}
			}
		}

		public partial class SerpProfileRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Name", "IsActive" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion
		}

		public partial class SerpProfileKeywordRow : DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "TotalResults" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion
		}

		public partial class SerpProfileDomainRow : DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Domain" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion
		}
		public partial class SerpProfileDomainGroupRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Name" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion
		}

		public partial class AdgroupKeywordRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Keyword", "PageGK", "AdunitID", "PageDisplay"};

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion

			public string MatchTypeDisplay
			{
				get { return (this.IsMatchTypeNull() ? Easynet.Edge.BusinessObjects.MatchType.Unidentified : (MatchType) this.MatchType).ToString(); }
			}

			public string DestinationUrlDisplay
			{
				get
				{
					return (this.IsDestinationURLNull() ||
							this.DestinationURL.ToLower() == "default url" ||
							this.DestinationURL == "0") ?
					null : 
					this.DestinationURL;
				}
			}
		}

		public partial class AdgroupRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Targets",};

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion

			TargetsRow _targets;
			public TargetsRow Targets
			{
				get { return _targets; }
				set
				{
					_targets = value;
					this.OnPropertyChanged("Targets");
				}
			}
		}

		public partial class CampaignRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "Targets" };

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion

			TargetsRow _targets;
			public TargetsRow Targets
			{
				get { return _targets; }
				set
				{
					_targets = value;
					this.OnPropertyChanged("Targets");
				}
			}

			public object Tag { get; set; }
		}

		public partial class AdgroupCreativeRow: DataRow, INotifyPropertyChanged, IPropertyChangeNotifier
		{
			string[] _notifyAbout = new string[] { "PageGK", "PageDisplay"};

			#region INotifyPropertyChanged Members

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region IPropertyChangeNotifier Members

			public void OnPropertyChanged(string propertyName)
			{
				OnPropertyChanged(new string[] { propertyName });
			}

			public void OnPropertyChanged(string[] properties)
			{
				foreach (string prop in properties)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			public void OnAllPropertiesChanged()
			{
				foreach (string prop in _notifyAbout)
					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}

			#endregion

			public string DisplayDescription
			{
				get
				{
					return this.Desc1 + " " + this.Desc2;
				}
			}

			public string DisplayTitle
			{
				get
				{
					return this.IsTitleNull() || String.IsNullOrEmpty(this.Title) ?
						"(missing title)" :
						this.Title;
				}
			}

			public string DisplayDesc1
			{
				get
				{
					return this.IsDesc1Null() || String.IsNullOrEmpty(this.Desc1) ?
						"(missing description)" :
						this.Desc1;
				}
			}

			public string DestinationUrlDisplay
			{
				get
				{
					return (this.IsDestinationURLNull() ||
							this.DestinationURL.ToLower() == "default url" ||
							this.DestinationURL == "0") ?
					null : 
					this.DestinationURL;
				}
			}
			
			/// Hacks for data binding purposes.
			
			object _pages = null;
			public object PageValues
			{
				get { return _pages; }
				set { _pages = value; }
			}

			object _segment1Visibility;
			public object Segment1Visibility
			{
				get { return _segment1Visibility; }
				set { _segment1Visibility = value; }
			}

			object _segment2Visibility;
			public object Segment2Visibility
			{
				get { return _segment2Visibility; }
				set { _segment2Visibility = value; }
			}

			object _segment3Visibility;
			public object Segment3Visibility
			{
				get { return _segment3Visibility; }
				set { _segment3Visibility = value; }
			}

			object _segment4Visibility;
			public object Segment4Visibility
			{
				get { return _segment4Visibility; }
				set { _segment4Visibility = value; }
			}

			object _segment5Visibility;
			public object Segment5Visibility
			{
				get { return _segment5Visibility; }
				set { _segment5Visibility = value; }
			}

			string _segment1Name = null;
			public string Segment1Name
			{
				get { return _segment1Name; }
				set { _segment1Name = value; }
			}

			string _segment2Name = null;
			public string Segment2Name
			{
				get { return _segment2Name; }
				set { _segment2Name = value; }
			}

			string _segment3Name = null;
			public string Segment3Name
			{
				get { return _segment3Name; }
				set { _segment3Name = value; }
			}

			string _segment4Name = null;
			public string Segment4Name
			{
				get { return _segment4Name; }
				set { _segment4Name = value; }
			}

			string _segment5Name = null;
			public string Segment5Name
			{
				get { return _segment5Name; }
				set { _segment5Name = value; }
			}

			object _segment1Values = null;
			public object Segment1Values
			{
				get { return _segment1Values; }
				set { _segment1Values = value; }
			}

			object _segment2Values = null;
			public object Segment2Values
			{
				get { return _segment2Values; }
				set { _segment2Values = value; }
			}

			object _segment3Values = null;
			public object Segment3Values
			{
				get { return _segment3Values; }
				set { _segment3Values = value; }
			}

			object _segment4Values = null;
			public object Segment4Values
			{
				get { return _segment4Values; }
				set { _segment4Values = value; }
			}

			object _segment5Values = null;
			public object Segment5Values
			{
				get { return _segment5Values; }
				set { _segment5Values = value; }
			}

			//public string PageDisplayDisplay
			//{
			//    get { return this.IsPageDisplayNull() || String.IsNullOrEmpty(this.PageDisplay) ? "(none)" : this.PageDisplay; }
			//}
		}
	
	}

	public interface IPropertyChangeNotifier
	{
		void OnPropertyChanged(string propertyName);
		void OnPropertyChanged(string[] properties);
		void OnAllPropertiesChanged();
	}

	[Serializable]
	public class Measure: ISerializable
	{
		public int MeasureID { get; private set; }
		public string FieldName { get; private set; }
		public string DisplayName { get; set; }
		public bool IsAbsolute { get; private set; }

		private string _valueRowColumnName;
		private TargetsRow _valueRow;

		public void SetValueSource(TargetsRow row)
		{
			_valueRow = row;
		}

		public double? Value
		{
			get
			{
				return _valueRow[FieldName];
			}
			set
			{
				_valueRow[FieldName] = value;
			}
		}

		#region ISerializable Members

		public Measure(IDataRecord record)
		{
			// Backwards compatibility
			string fieldNameColumn = "OLTP_Name";
			for (int i = 0; i < record.FieldCount; i++)
			{
				if (record.GetName(i).ToLower() == "FieldName".ToLower())
				{
					fieldNameColumn = "FieldName";
					break;
				}
			}
			
			MeasureID = record.GetInt32(record.GetOrdinal("MeasureID"));
			FieldName = record.GetString(record.GetOrdinal(fieldNameColumn));
			DisplayName = record.GetString(record.GetOrdinal("DisplayName"));
			IsAbsolute = record.GetBoolean(record.GetOrdinal("IsAbsolute"));
		}

		protected Measure(SerializationInfo info, StreamingContext context)
		{
			MeasureID = info.GetInt32("MeasureID");
			FieldName = info.GetString("FieldName");
			DisplayName = info.GetString("DisplayName");
			IsAbsolute = info.GetBoolean("IsAbsolute");

		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MeasureID", MeasureID);
			info.AddValue("FieldName", FieldName);
			info.AddValue("DisplayName", DisplayName);
			info.AddValue("IsAbsolute", IsAbsolute);
		}

		public Measure Clone()
		{
			return (Measure) this.MemberwiseClone();
		}

		#endregion
	}

	/// <summary>
	/// Targets table returns a TargetsRow (GetCampaignTargets and GetAdgroupTargets) that wraps a row
	/// from the targets table. It is important to note that each call to one of these methods generates a
	/// NEW TargetsRow object, and so the internal row, not the object, should be used for identity and comparisons.
	/// </summary>
	public class TargetsTable
	{
		DataTable _table;
		DataRow _disabledIndicatorRow;
		Dictionary<int, DataRow> _campaignTargets = new Dictionary<int,DataRow>();
		Dictionary<int, DataRow> _adgroupTargets = new Dictionary<int,DataRow>();
		//Dictionary<int, List<DataRow>> _campaignAdgroupTargets = new Dictionary<int,List<DataRow>>();

		public TargetsTable(DataTable table)
		{
			_table = table;

			// Empty indicator row is to distinguish between "enabled, but no data available" (= null) and  "disabled"
			_disabledIndicatorRow = _table.NewRow();
			
			foreach(DataRow row in _table.Rows)
				RegisterRow(row);
		}

		internal void RegisterRow(DataRow row)
		{
			int campaignGK = (int) row["CampaignGK"];
			int adgroupGK = (int) row["AdgroupGK"];
			
			// If there already is an existing campaign row, delete it -- either way its not relevant any more
			// (if an adgroup row is being registered, campaign must be deleted; if a campaign row, it is being replaced)
			DataRow existingCampaignRow;
			if (_campaignTargets.TryGetValue(campaignGK, out existingCampaignRow) && existingCampaignRow != _disabledIndicatorRow)
				existingCampaignRow.Delete();

			if (adgroupGK < 0)
			{
				_campaignTargets[campaignGK] = row;
			}
			else
			{
				// Set the campaign as the empty indicator row, so that IsActive is automatically set to false
				_campaignTargets[campaignGK] = _disabledIndicatorRow;

				// Add the adgroup
				_adgroupTargets.Add(adgroupGK, row);

				//AssociateAdgroupWithCampaign(row, campaignGK);
			}
		}

		//private void AssociateAdgroupWithCampaign(DataRow adgroup, int campaignGK)
		//{
		//    List<DataRow> adgroups;
		//    if (!_campaignAdgroupTargets.TryGetValue(campaignGK, out adgroups))
		//    {
		//        adgroups = new List<DataRow>();
		//        _campaignAdgroupTargets.Add(campaignGK, adgroups);
		//    }

		//    if (!adgroups.Contains(adgroup))
		//        adgroups.Add(adgroup);
		//}

		//private void DisassociateAdgroupWithCampaign(DataRow adgroup, int campaignGK)
		//{
		//    List<DataRow> adgroups;
		//    if (!_campaignAdgroupTargets.TryGetValue(campaignGK, out adgroups))
		//        return;

		//    if (adgroups.Contains(adgroup))
		//        adgroups.Remove(adgroup);
		//}

		//public void DeleteAdgroupTargets(int campaignGK)
		//{
		//    List<DataRow> adgroups;
		//    if (!_campaignAdgroupTargets.TryGetValue(campaignGK, out adgroups))
		//        return;

		//    foreach (DataRow adgroup in adgroups)
		//        adgroup.Delete();
		//}

		public TargetsRow GetCampaignTargets(int campaignGK)
		{
			DataRow row = null;
			_campaignTargets.TryGetValue(campaignGK, out row);
			TargetsRow targetsRow = new TargetsRow(campaignGK, -1, this, row);

			// Mark it as inactive
			if (row == _disabledIndicatorRow)
			{
				targetsRow.IsActive = false;
			}

			return targetsRow;
		}

		public TargetsRow GetAdgroupTargets(int campaignGK, int adgroupGK)
		{
			DataRow row = null;
			_adgroupTargets.TryGetValue(adgroupGK, out row);
			return new TargetsRow(campaignGK, adgroupGK, this, row);
		}

		public void DeleteCampaignTargets(int campaignGK)
		{
			DataRow row = null;
			if (_campaignTargets.TryGetValue(campaignGK, out row))
			{
				row.Delete();
				if (row.RowState == DataRowState.Detached)
				{
					_campaignTargets.Remove(campaignGK);
				}
			}
		}

		public void DeleteAdgroupTargets(int adgroupGK)
		{
			DataRow row = null;
			if (_adgroupTargets.TryGetValue(adgroupGK, out row))
			{
				int campaignGK = (int) row["CampaignGK"];
				row.Delete();
				if (row.RowState == DataRowState.Detached)
				{
					_adgroupTargets.Remove(adgroupGK);
					//DisassociateAdgroupWithCampaign(row, campaignGK);
				}
			}
		}

		public DataTable InnerTable
		{
			get { return _table; }
		}

	}

	public class TargetsRow: INotifyPropertyChanged, IPropertyChangeNotifier
	{
		TargetsTable _table;
		DataRow _row;
		int _campaignGK;
		int _adgroupGK;
		bool _isActive = true;

		string[] _notifyAbout = new string[] { "Item[]", "IsActive"};

		#region INotifyPropertyChanged Members

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IPropertyChangeNotifier Members

		public void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new string[] { propertyName });
		}

		public void OnPropertyChanged(string[] properties)
		{
			foreach (string prop in properties)
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		public void OnAllPropertiesChanged()
		{
			foreach (string prop in _notifyAbout)
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		#endregion

		internal TargetsRow(int campaignGK, int adgroupGK, TargetsTable table, DataRow row)
		{
			_campaignGK = campaignGK;
			_adgroupGK = adgroupGK;
			_table = table;
			_row = row;
		}

		public bool IsActive
		{
			get { return _isActive; }
			set { _isActive = value; this.OnPropertyChanged("IsActive");  }
		}

		public bool IsEmpty
		{
			get { return _row == null || _row.RowState == DataRowState.Detached || _row.RowState == DataRowState.Deleted; }
		}

		public DataRow InnerRow
		{
			get { return _row; }
		}

		public double? this[string measure]
		{
			get
			{
				return _row == null || _row.RowState == DataRowState.Detached || _row.RowState == DataRowState.Deleted || _row.IsNull(measure) ?
					null :
					new Nullable<double>((double)_row[measure]); 
			}
			set
			{
				if (_row == null || _row.RowState == DataRowState.Detached)
				{
					// For missing or detached rows, create a new row
					_row = _table.InnerTable.NewRow();
					_row["CampaignGK"] = _campaignGK;
					_row["AdgroupGK"] = _adgroupGK;
					_table.InnerTable.Rows.Add(_row);
					_table.RegisterRow(_row);
				}
				else if (_row.RowState == DataRowState.Deleted)
				{
					// For deleted rows, transform into a clean row
					_row.RejectChanges();
					foreach(DataColumn col in _table.InnerTable.Columns)
						if (col.ColumnName != "CampaignGK" && col.ColumnName != "AdgroupGK")
							_row[col] = DBNull.Value;
				}

				_row[measure] = value == null ? (object) DBNull.Value : (object) value.Value;

				// Check if this null value being passed makes all measures null
				if (value == null)
				{
					bool hasValues = false;
					foreach (DataColumn col in _table.InnerTable.Columns)
					{
						if (col.ColumnName == "CampaignGK" || col.ColumnName == "AdgroupGK")
							continue;

						hasValues |= !(_row[col] is DBNull);
					}

					// No values left, delete this row
					if (!hasValues)
						_row.Delete();
				}
				
				this.OnPropertyChanged("Item[]");
			}
		}

		public void Reject()
		{
			if (_row == null)
				return;

			_row.RejectChanges();
			this.OnPropertyChanged("Item[]");
		}
	}
}
/*

	if (PropertyChanged != null)
		PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Name"));

 
 , System.ComponentModel.INotifyPropertyChanged
 
 
 */






namespace Easynet.Edge.UI.Data.OltpTableAdapters {
  
    public partial class CampaignTableAdapter {
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}
    }

	public partial class AdgroupTableAdapter
	{
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}
	}

	public partial class AdgroupCreativeTableAdapter
	{
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}
	}
	public partial class GatewayTableAdapter
	{
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}

		public SqlCommand UpdateCommand
		{
			get { return _adapter.UpdateCommand; }
		}
		public SqlCommand InsertCommand
		{
			get { return _adapter.InsertCommand; }
		}
	}

	public partial class SerpProfileTableAdapter
	{
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}
	}

	public partial class SegmentValueTableAdapter
	{
		public SqlConnection CurrentConnection
		{
			get
			{
				return this.Connection;
			}
		}
		public SqlTransaction CurrentTransaction
		{
			get { return this.Transaction; }
			set { this.Transaction = value; }
		}

		public SqlDataAdapter InnerAdapter
		{
			get { return this.Adapter; }
		}
	}
}
