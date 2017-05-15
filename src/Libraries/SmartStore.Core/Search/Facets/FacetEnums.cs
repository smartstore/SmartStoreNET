namespace SmartStore.Core.Search.Facets
{
	public enum FacetTemplateHint
	{
		/// <summary>
		/// Render facets as checkboxes
		/// </summary>
		Checkboxes = 0,

		/// <summary>
		/// Custom facet rendering like color or picture boxes
		/// </summary>
		Custom,

		/// <summary>
		/// Render facets as a numeric range filter
		/// </summary>
		NumericRange
	}
}
