using System;
using System.Collections.Generic;

namespace SmartStore.Core
{
	public interface IHostNameProvider
	{
		string GetHostName();
	}
}
