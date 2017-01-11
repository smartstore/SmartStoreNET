using System;
using System.Xml.Serialization;

namespace SmartStore.Core.Search.Filter
{
	[Serializable]
	public class SearchFilterDescriptor
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
		/// Gets or sets the field name of the filter
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		/// Gets or sets the friendly name of the filter
		/// </summary>
		[XmlIgnore]
		public string FriendlyName { get; set; }
	}
}
