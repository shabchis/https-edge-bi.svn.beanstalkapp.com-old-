using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval
{

	/// <summary>
	/// This class represents BackOffice row and contain all the relevent  
	/// fields of BackOffice table.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public class BackOfficeRow: SourceDataRow
	{
		#region Members
		/*=========================*/

		private Dictionary<string, int> _boValues = new Dictionary<string, int>();

		public Dictionary<string, int> BoValues
		{
			get { return _boValues; }
			set { _boValues = value; }
		}

		private int _gatewayID;
		private int _totalHits;
		private int _BOClicks;
		private int _users;
		private int _newLeads;
		private int _newUsers;
		private int _newActiveUsers;
		private int _activeUsers;
		private double _newNetDepostit;
		private double _totalNetDeposit;
		private int _SAT;
		private int _GSS;
		private int _EV;
		
		/*=========================*/
        #endregion    
		
		#region Access Methods
		/*=========================*/

		public int SAT
		{
			get { return _SAT; }
			set { _SAT = value; }
		}

		public int GSS
		{
			get { return _GSS; }
			set { _GSS = value; }
		}

		public int EV
		{
			get { return _EV; }
			set { _EV = value; }
		}

		public int GatewayID
		{
			get { return _gatewayID; }
			set { _gatewayID = value; }
		}

		public int BOClicks
		{
			get { return _BOClicks; }
			set { _BOClicks = value; }
		}

		public int TotalHits
		{
			get { return _totalHits; }
			set { _totalHits = value; }
		}
			
		public double TotalNetDeposit
		{
			get { return _totalNetDeposit; }
			set { _totalNetDeposit = value; }
		}

		public int Users
		{
			get { return _users; }
			set { _users = value; }
		}	
	
		public int NewLeads
		{
			get { return _newLeads; }
			set { _newLeads = value; }
		}	

		public int NewUsers
		{
			get { return _newUsers; }
			set { _newUsers = value; }
		}
				
		public int NewActiveUsers
		{
			get { return _newActiveUsers; }
			set { _newActiveUsers = value; }
		}

		public int ActiveUsers
		{
			get { return _activeUsers; }
			set { _activeUsers = value; }
		}

		public double NewNetDepostit
		{
			get { return _newNetDepostit; }
			set { _newNetDepostit = value; }
		}

		/*=========================*/
        #endregion   
	}
}
