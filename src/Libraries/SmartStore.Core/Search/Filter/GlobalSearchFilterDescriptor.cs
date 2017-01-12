using Newtonsoft.Json;

namespace SmartStore.Core.Search.Filter
{
	[JsonObject(MemberSerialization.OptOut)]
	public class GlobalSearchFilterDescriptor
	{
		/// <summary>
		/// Gets or sets the field name
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the filter is disabled
		/// </summary>
		public bool Disabled { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the friendly name of the filter
		/// </summary>
		[JsonIgnore]
		public string FriendlyName { get; set; }
	}
}
