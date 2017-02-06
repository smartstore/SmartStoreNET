namespace SmartStore.Services.Search.Modelling
{
	public interface ICatalogSearchQueryAliasMapper
	{
		/// <summary>
		/// Clears all cached mappings
		/// </summary>
		void ClearCache();

		/// <summary>
		/// Gets the attribute id for an attribute alias
		/// </summary>
		/// <param name="attributeAlias">Attribute alias</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute identifier</returns>
		int GetAttributeIdByAlias(string attributeAlias, int languageId = 0);

		/// <summary>
		/// Gets the attribute option id for an option alias
		/// </summary>
		/// <param name="optionAlias">Attribute option alias</param>
		/// <param name="attributeId">Attribute identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute option identifier</returns>
		int GetOptionIdByAlias(string optionAlias, int attributeId, int languageId = 0);
	}
}
