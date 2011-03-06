using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edge.Core.Schema
{
	public enum DataState
	{
		Unchanged,
		Modified,
		Added,
		Deleted,
		Detached
	}
}
