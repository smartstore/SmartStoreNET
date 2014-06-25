using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Plugins
{
	public class ProviderMetadata
	{
		/// <summary>
		/// Gets or sets the provider type
		/// </summary>
		public Type ProviderType { get; set; }
		
		/// <summary>
		/// Gets or sets the provider system name
		/// </summary>
		public string SystemName { get; set; }

		/// <summary>
		/// Gets or sets the resource root key for user data (e.g. DisplayOrder)
		/// </summary>
		public string ResourceRootKey { get; set; }

		/// <summary>
		/// Gets or sets the provider friendly name
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		/// Gets or sets the provider description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="PluginDescriptor"/> instance in which the provider is implemented
		/// </summary>
		/// <remarks>The value is <c>null</c>, if the provider is part of the application core</remarks>
		public PluginDescriptor PluginDescriptor { get; set; }

		public string GetSettingKey(string name)
		{
			return "PluginSetting.{0}.{1}".FormatWith(SystemName, name);
		}

		public override string ToString()
		{
			return "Provider '{0}' - {1}".FormatCurrent(SystemName, FriendlyName);
		}

		public override bool Equals(object obj)
		{
			var other = obj as ProviderMetadata;
			return other != null &&
				SystemName != null &&
				SystemName.Equals(other.SystemName);
		}

		public override int GetHashCode()
		{
			return SystemName.GetHashCode();
		}
	}
}
