using System.Collections.Generic;

namespace SmartStore.Web.Framework.Settings
{
	/// <remarks>codehint: sm-add</remarks>
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
