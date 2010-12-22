using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdgeBI.Data.Pipeline
{
	public interface IRowReader: IDisposable
	{
		object CurrentRow { get; }
		bool Read();
	}
}
