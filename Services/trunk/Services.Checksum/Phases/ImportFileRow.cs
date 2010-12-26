using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EdgeBI.Data.Readers;
using System.Data;

namespace Services.Checksum
{
	public class ImportFileRow
	{
		public int AccountID;
		public Dictionary<string, double> Fields;
	}
}
