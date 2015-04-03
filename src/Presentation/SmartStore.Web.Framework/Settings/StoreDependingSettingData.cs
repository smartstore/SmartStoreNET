using System.Collections.Generic;

namespace SmartStore.Web.Framework.Settings
{
	public class StoreDependingSettingData
	{
		public StoreDependingSettingData()
		{
			OverrideSettingKeys = new List<string>();
		}

		public int ActiveStoreScopeConfiguration { get; set; }
		public List<string> OverrideSettingKeys { get; set; }
		public string RootSettingClass { get; set; }
	}
}
