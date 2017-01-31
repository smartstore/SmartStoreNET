namespace SmartStore.Services.Search.Modelling
{
	public interface ICatalogSearchQueryAliasMapper
	{
		/// <summary>
		/// Gets an attribute alias mapping
		/// </summary>
		/// <param name="attributeAlias">Attribute alias</param>
		/// <param name="optionAlias">Attribute option alias</param>
		/// <returns>Search query alias mapping</returns>
		SearchQueryAliasMapping GetAttributeByAlias(string attributeAlias, string optionAlias);

		/// <summary>
		/// Adds an attribute alias mapping
		/// </summary>
		/// <param name="attributeAlias">Attribute alias</param>
		/// <param name="optionAlias">Attribute option alias</param>
		/// <param name="mapping">Search query alias mapping to be added</param>
		/// <returns><c>true</c> successfully added, <c>false</c> not added</returns>
		bool AddAttribute(string attributeAlias, string optionAlias, SearchQueryAliasMapping mapping);

		/// <summary>
		/// Removes an attribute alias mapping
		/// </summary>
		/// <param name="attributeAlias">Attribute alias</param>
		/// <param name="optionAlias">Attribute option alias</param>
		/// <returns><c>true</c> successfully removed, <c>false</c> not removed</returns>
		bool RemoveAttribute(string attributeAlias, string optionAlias);

		/// <summary>
		/// Removes all cached attribute alias mappings
		/// </summary>
		void RemoveAllAttributes();
	}
}
