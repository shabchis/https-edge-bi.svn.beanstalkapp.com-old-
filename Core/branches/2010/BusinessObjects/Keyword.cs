using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Easynet.Edge.Core.Configuration;
using Easynet.Edge.Core.Data;


namespace Easynet.Edge.BusinessObjects
{
	[Obsolete("Keyword class does not yet user Persistence model.")]
	public class Keyword: DataItem
	{
		#region Constants
		/*=========================*/

		public class ColumnNames
		{
			public const string ID = "Keyword_GK";
			public const string AccountID = "Account_ID";
			public const string StringValue = "Keyword";
			public const string IsMonitored = "IsMonitored";
			public const string LastUpdated = "LastUpdated";
			public const string Segment1 = "Keyword_Segment1";
			public const string Segment2 = "Keyword_Segment2";
			public const string Segment3 = "Keyword_Segment3";
			public const string Segment4 = "Keyword_Segment4";
			public const string Segment5 = "Keyword_Segment5";
		}

		/*=========================*/
		#endregion

		#region Fields
		/*=========================*/

		Account _account = null;

		/*=========================*/
		#endregion

		#region Constructors
		/*=========================*/

		public Keyword(Account account, string value)
		{
			if (account == null)
				throw new ArgumentNullException("Account required.");

			_account = account;
			SetField(ColumnNames.AccountID, _account.ID);
			SetField(ColumnNames.StringValue, value);
		}

		/*=========================*/
		#endregion

		#region Init Methods
		/*=========================*/

		public static void BuildInnerTable(DataTable table)
		{
			table.Columns.Add(ColumnNames.ID, typeof(long));
			table.Columns.Add(ColumnNames.AccountID, typeof(int));
			table.Columns.Add(ColumnNames.StringValue, typeof(string));
			table.Columns.Add(ColumnNames.IsMonitored, typeof(bool));
			table.Columns.Add(ColumnNames.Segment1, typeof(string));
			table.Columns.Add(ColumnNames.Segment2, typeof(string));
			table.Columns.Add(ColumnNames.Segment3, typeof(string));
			table.Columns.Add(ColumnNames.Segment4, typeof(string));
			table.Columns.Add(ColumnNames.Segment5, typeof(string));
		}

		protected override void OnInitializeTable()
		{
			BuildInnerTable(InnerTable);
		}

		protected override void OnInitCommands(out SqlCommand selectCmd, out SqlCommand insertCmd, out SqlCommand updateCmd, out SqlCommand deleteCmd)
		{
			// TODO: automate

			selectCmd = DataManager.CreateCommand(AppSettings.Get(this, "Sql.SelectCommand"));
			selectCmd.Parameters["@id"].SourceColumn = ColumnNames.ID;

			insertCmd = DataManager.CreateCommand(AppSettings.Get(this, "Sql.InsertCommand"));
			updateCmd = DataManager.CreateCommand(AppSettings.Get(this, "Sql.UpdateCommand"));
			deleteCmd = DataManager.CreateCommand(AppSettings.Get(this, "Sql.DeleteCommand"));
		}

		protected override void OnSave()
		{
			if (!DataManager.ProxyMode)
			{
				// TODO: custom exception type
				//if (GkManager.Exists(typeof(Keyword), _account.ID, this.StringValue))
				//	throw new Exception(String.Format("Keyword value '{0}' already defined in this account.", this.StringValue));

				// Get a valid ID
				long id = GkManager.GetKeywordGK(_account.ID, this.StringValue);
				SetField(ColumnNames.ID, id);

				// Normal DB save
				base.OnSave();

				// Notify the GkManager
				//GkManager.MarkAsSaved(typeof(Keyword), id);
			}
			else
			{
				base.OnSave();
			}
		}

		protected override void OnDelete()
		{
			// Perform this check on both client and server
			long id = this.ID;
			if (id < 0)
				throw new InvalidOperationException("Cannot delete an unsaved item");

			base.OnDelete();

			//if (!DataManager.ProxyMode)
			//	GkManager.MarkAsNew(typeof(Keyword), id);

		}

		/*=========================*/
		#endregion

		#region Public Properties
		/*=========================*/

		public long ID
		{
			get
			{
				object val = GetField(ColumnNames.ID);
				return val == null ? -1 : (long) val;
			}
		}

		public Account Account
		{
			get
			{
				if (_account == null)
				{
					object val = GetField(ColumnNames.AccountID);
					_account = val == null ? null : Account.WithID((int)val);
				}
				
				return _account;
			}
		}

		public string StringValue
		{
			get
			{
				object val = GetField(ColumnNames.StringValue);
				return val == null ? null : (string) val;
			}
			set
			{
				if (this.ID < 0)
					throw new InvalidOperationException("Can't set this property after it has been saved");

				SetField(ColumnNames.StringValue, value);
			}
		}

		public bool IsMonitored
		{
			get
			{
				object val = GetField(ColumnNames.IsMonitored);
				return val == null ? false : (bool) val;
			}
			set
			{
				SetField(ColumnNames.IsMonitored, value);
			}
		}

		public Dictionary<Segment, string> Segments
		{
			get
			{
				return null;
			}
		}

		/*=========================*/
		#endregion

		#region Public Methods
		/*=========================*/

		//public Keyword Duplicate()
		//{
		//    Keyword dup = new Keyword(_account, this.Row, this.ParentCollection);
		//    return dup;
		//}

		/*=========================*/
		#endregion

		#region Static Methods
		/*=========================*/

		//public static Keyword WithID(long id)
		//{
		//    if (id < 0)
		//        return null;

		//    return new Keyword(id);
		//}

		public static Keyword Get(long id)
		{
			//Keyword kw = WithID(id);
			//if (kw != null)
			//    kw.RetrieveMissingFields();

			return null;//kw;
		}
		/*=========================*/
		#endregion
	}

	#region Obsolete
	//    public class AccountKeywordCollection: DataItemCollection
//    {
//        #region Fields
//        /*=========================*/

//        Account _account;

//        /*=========================*/
//        #endregion

//        #region Constructors
//        /*=========================*/

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="parent"></param>
//        /// <param name="inclusion"></param>
//        internal AccountKeywordCollection(Account account, string filter, int resultLimit, bool includeUnmonitored, bool includeRelated)
//        {
//            if (account == null)
//                throw new ArgumentNullException("account");
//            if (account.ID < 0)
//                throw new InvalidOperationException("Account must be saved first");

//            _account = account;
			
//            InitializeTable();
//            InitCommands();
//        }

//        /*=========================*/
//        #endregion

//        #region Internal Properties
//        /*=========================*/

//        /// <summary>
//        /// 
//        /// </summary>
//        protected override System.Reflection.PropertyInfo PrimaryKey
//        {
//            get
//            {
//                return this.DataItemType.GetProperty("ID");
//            }
//        }

//        /*=========================*/
//        #endregion

//        #region Internal Methods
//        /*=========================*/

//        /// <summary>
//        /// 
//        /// </summary>
//        protected override void OnInitializeTable()
//        {
//            Keyword.BuildInnerTable(InnerTable);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        protected override void OnInitCommands(out SqlCommand selectCmd, out SqlCommand insertCmd, out SqlCommand updateCmd, out SqlCommand deleteCmd)
//        {
//            selectCmd = null;
////                DataManager.CreateCommand(String.Format(@"
////				SELECT {0}   
////						  k.Keyword_GK, k.Account_ID, k.Keyword, k.Keyword_Segment1, k.Keyword_Segment2, k.Keyword_Segment3, k.Keyword_Segment4, k.Keyword_Segment5, k.IsMonitored
////
////				FROM      UserProcess_GUI_Keyword k
////				WHERE     (k.Account_ID = @accountID:Int OR (@includeRelated:Bit = 1 AND k.Account_ID IN (SELECT r.RelatedAccountID FROM User_GUI_RelatedAccount r WHERE r.AccountID = @accountID:_) ) ) AND
////								(@keywordFilter:NVarChar IS NULL OR k.Keyword LIKE @keywordFilter:_) AND
////								(@includeUnmonitored:Bit = 1 OR k.IsMonitored = 1)
////				ORDER BY   k.Keyword
////				", _resultLimit > 0 ? "TOP (@resultLimit:BigInt)" : null));
////            selectCmd.Parameters["@accountID"].Value = _account.ID;
////            selectCmd.Parameters["@keywordFilter"].Value = _filter == null ? (object) DBNull.Value : (object) _filter;
////            selectCmd.Parameters["@includeRelated"].Value = _includeRelated;
////            selectCmd.Parameters["@includeUnmonitored"].Value = _includeUnmonitored;
////            if (_resultLimit > 0)
////                selectCmd.Parameters["@resultLimit"].Value = _resultLimit;

//            insertCmd = DataManager.CreateCommand(@"
//				INSERT INTO UserProcess_GUI_Keyword (ID, Account_ID, Keyword, IsMonitored)
//				VALUES (@id:BigInt, @accountID:Int, @stringValue:NVarChar, @isMonitored:Bit);
//				");
//            insertCmd.Parameters["@accountID"].Value = _account.ID;
//            insertCmd.Parameters["@id"].SourceColumn = Keyword.ColumnNames.ID;
//            insertCmd.Parameters["@stringValue"].SourceColumn = Keyword.ColumnNames.StringValue;
//            insertCmd.Parameters["@isMonitored"].SourceColumn = Keyword.ColumnNames.IsMonitored;

//            updateCmd = DataManager.CreateCommand(@"
//				UPDATE UserProcess_GUI_Keyword
//				SET Keyword = @stringValue:NVarChar, IsMonitored = @isMonitored:Bit
//				WHERE Keyword_GK = @id:BigInt
//				");
//            updateCmd.Parameters["@id"].SourceColumn = Keyword.ColumnNames.ID;
//            updateCmd.Parameters["@stringValue"].SourceColumn = Keyword.ColumnNames.StringValue;
//            updateCmd.Parameters["@isMonitored"].SourceColumn = Keyword.ColumnNames.IsMonitored;

//            deleteCmd = null;
//            //deleteCmd = DataManager.CreateCommand("DELETE FROM UserP);
//            //deleteCmd.Parameters["@id"].SourceColumn = Keyword.ColumnNames.ID;
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="row"></param>
//        /// <returns></returns>
//        protected override DataItem NewItem(DataRow row)
//        {
//            return new Keyword(_account, row, this);
//        }

//        protected override void OnSave()
//        {
//            base.OnSave();
//        }

//        /*=========================*/
//        #endregion

//        #region Public Properties
//        /*=========================*/

//        /// <summary>
//        /// 
//        /// </summary>
//        public override Type DataItemType
//        {
//            get
//            {
//                return typeof(Keyword);
//            }
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        public new Keyword this[int index]
//        {
//            get
//            {
//                return (Keyword) base[index];
//            }
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        public Account Account
//        {
//            get
//            {
//                return _account;
//            }
//        }


//        /*=========================*/
//        #endregion

//        #region Public Methods
//        /*=========================*/

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="user"></param>
//        /// <returns></returns>
//        public Keyword Add(Keyword keyword)
//        {
//            return (Keyword) base.Add(keyword);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="keyword"></param>
//        /// <returns></returns>
//        public Keyword Remove(Keyword keyword)
//        {
//            return (Keyword) base.Remove(keyword);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="index"></param>
//        /// <returns></returns>
//        public new Keyword RemoveAt(int index)
//        {
//            return (Keyword) base.RemoveAt(index);
//        }

//        /*=========================*/
//        #endregion

//        #region Static Methods
//        /*=========================*/

//        public static AccountKeywordCollection Get(Account account)
//        {
//            return null; //Get(account, null, 0, true, false, null);
//        }

//        public static AccountKeywordCollection GetFiltered(Account account, string filter, int resultLimit, bool includeUnmonitored, bool includeRelated, EventHandler<BoundItemRecievedEventArgs> boundItemReceivedHandler)
//        {
//            AccountKeywordCollection collection = null;

//            collection = new AccountKeywordCollection(account, filter, resultLimit, includeUnmonitored, includeRelated);
//            if (boundItemReceivedHandler != null)
//                collection.BoundItemReceived += boundItemReceivedHandler;

//            collection.Bind();

//            return collection;
//        }

//        /*=========================*/
//        #endregion

//        #region Serialization
//        /*=========================*/
		
//        private AccountKeywordCollection(SerializationInfo info, StreamingContext context) : base(info, context)
//        {
//        }

//        protected override void DeserializeData(SerializationInfo info)
//        {
//            _account = (Account) info.GetValue("_account", typeof(Account));
//        }

//        protected override void SerializeData(SerializationInfo info)
//        {
//            info.AddValue("_account", _account);
//        }

//        /*=========================*/
//        #endregion
//    }
	#endregion
}
