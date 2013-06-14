using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Configuration
{
	public struct StoreDependingSetting<T>
	{
		public StoreDependingSetting(bool overrideForStore)
			: this()
		{
			Value = default(T);
			OverrideForStore = overrideForStore;
		}

		public T Value;
		public bool OverrideForStore;
	}
}
