using System;
using System.Collections.Generic;

namespace SmartStore.Core
{
	public interface IApplicationEnvironment
	{
		string EnvironmentIdentifier { get; }
	}
}
