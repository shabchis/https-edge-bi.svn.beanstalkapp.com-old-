using System;
using Easynet.Edge.Services.DataRetrieval.DataReader;

namespace Easynet.Edge.Services.DataRetrieval
{
	/// <summary>
	/// This class represents organic rankings row and contain all the relevent  
	/// fields of BackOffice table.
	/// </summary>
	/// <author>Yaniv Kahana</author>
	/// <creation_date>15/09/2008</creation_date>
	public class OrganicRankingsRow: SourceDataRow
	{
		#region Members
		/*=========================*/

		private string _description;
		private string _title;
		private string _url;

		/*=========================*/
		#endregion

		#region Access Methods
		/*=========================*/

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		public void ApplyValue(string name, string value)
		{
			switch (name)
			{
				case "Url": Url = value; break;
				case "Title": Title = value; break;
				case "Description": Description = value; break;
			};
		}

		/*=========================*/
		#endregion
	}

}
