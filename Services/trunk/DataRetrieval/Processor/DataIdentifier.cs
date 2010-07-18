using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easynet.Edge.Services.DataRetrieval.Processor
{
	class DataIdentifier
	{
		#region Fields
		/*=========================*/

		private int _channelID = -1;
		private int _dayCode = 0;
		private int _accountID = -1;

		/*=========================*/
		#endregion   

		#region Constructors
		/*=========================*/

		public DataIdentifier(int accountID, int dayCode, int channelID)
		{
			_channelID = channelID;
			_dayCode = dayCode;
			_accountID = accountID;
		}

		/*=========================*/
		#endregion   

		#region Access Methods
		/*=========================*/

		public int ChannelID
		{
			get { return _channelID; }
			set { _channelID = value; }
		}
		
		public int AccountID
		{
			get { return _accountID; }
			set { _accountID = value; }
		}
		
		public int DayCode
		{
			get { return _dayCode; }
			set { _dayCode = value; }
		}

		/*=========================*/
		#endregion   

		public override bool Equals(object obj)
		{
			if ((this.AccountID == ((DataIdentifier)obj).AccountID) &&
				(this.ChannelID == ((DataIdentifier)obj).ChannelID) &&
				(this.DayCode == ((DataIdentifier)obj).DayCode))
			{
				return true;
			}
			else
				return false;

		}
	}
}
