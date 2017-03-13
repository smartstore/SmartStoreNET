using Newtonsoft.Json;

namespace SmartStore.Core.Search.Facets
{
	/// <summary>
	/// Represents settings for common facet groups like category, manufacturer, price etc.
	/// </summary>
	[JsonObject(MemberSerialization.OptOut)]
	public class CommonFacetOption
	{
		/// <summary>
		/// Gets or sets the facet group kind
		/// </summary>
		public FacetGroupKind Kind { get; set; }

		/// <summary>
		/// Gets or sets the URL alias (optional)
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the facet is disabled
		/// </summary>
		public bool Disabled { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the friendly name
		/// </summary>
		[JsonIgnore]
		public string FriendlyName { get; set; }
	}
}
