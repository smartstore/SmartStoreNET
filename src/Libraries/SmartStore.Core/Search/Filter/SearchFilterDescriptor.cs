using System;
using System.Xml.Serialization;

namespace SmartStore.Core.Search.Filter
{
	public enum GlobalSearchFilterType
	{
		Manufacturer = 0,
		Price,
		ReviewRate,
		Availability,
		DeliveryTime
	}


	[Serializable]
	public abstract class SearchFilterDescriptorBase
	{
		/// <summary>
		/// Gets or sets a value indicating if the filter is enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the friendly name of the filter
		/// </summary>
		[XmlIgnore]
		public string FriendlyName { get; set; }
	}


	[Serializable]
	public class GlobalSearchFilterDescriptor : SearchFilterDescriptorBase
	{
		/// <summary>
		/// Gets or sets the global filter type
		/// </summary>
		public GlobalSearchFilterType Type { get; set; }
	}
}
