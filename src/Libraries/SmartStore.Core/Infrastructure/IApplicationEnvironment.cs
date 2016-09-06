using System;
using System.Collections.Generic;
using SmartStore.Core.IO;

namespace SmartStore.Core
{
	public interface IApplicationEnvironment
	{
		string EnvironmentIdentifier { get; }

		IVirtualFolder WebRootFolder { get; }
		IVirtualFolder AppDataFolder { get; }
		IVirtualFolder ThemesFolder { get; }
		IVirtualFolder PluginsFolder { get; }
	}
}
