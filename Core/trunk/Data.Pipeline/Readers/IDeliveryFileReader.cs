using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Data.Pipeline
{
	public interface IDeliveryFileReader:IRowReader
	{
		public DeliveryFile File { get; set; }
	}
}
