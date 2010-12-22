using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Data.Pipeline
{
	public class Delivery
	{
		public int ID;
		public string Description;
		public DateTime DateCreated;
		public DateTime DateModified;
		public List<DeliveryFile> Files;
		public Dictionary<string, string> Parameters;

		// according to Alon's Account object - i.e., can represent 'old style' scope/client/account
		public int AccountID;
	}

	public class DeliveryFile
	{
		public int ID;
		public int DeliveryID;
		public string FilePath;
		public DateTime TargetDateTime;
		public int RetrieverInstanceID;
		public int ProcessorInstanceID;
		public Type ReaderType;
		public Dictionary<string, string> Parameters;
		public DateTime DateCreated;
		public DateTime DateModified;

		public IDeliveryFileReader CreateReader()
		{
			throw new NotImplementedException();
		}
	}
}
