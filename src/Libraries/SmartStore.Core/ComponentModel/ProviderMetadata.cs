using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.ComponentModel
{
	public class ProviderMetadata
	{
		public string SystemName { get; set; }
		public string ResourceKey { get; set; }

		public string GetSettingKey(string name)
		{
			return "PluginSetting.{0}.{1}".FormatWith(SystemName, name);
		}
	}
}
