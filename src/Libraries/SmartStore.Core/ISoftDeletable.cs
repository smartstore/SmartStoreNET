using System;
using System.Collections.Generic;

namespace SmartStore.Core
{
	public interface ISoftDeletable
	{
		bool Deleted { get; }
	}
}
