using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edge.Core
{
	public interface IReader: IDisposable
	{
		object Current { get; }
		bool Read();
	}
}
