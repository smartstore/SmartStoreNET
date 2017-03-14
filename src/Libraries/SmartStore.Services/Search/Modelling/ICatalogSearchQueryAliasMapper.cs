using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Modelling
{
	public interface ICatalogSearchQueryAliasMapper
	{
		/// <summary>
		/// Clears all cached attribute mappings
		/// </summary>
		void ClearAttributeCache();

		/// <summary>
		/// Gets the attribute id by attribute alias
		/// </summary>
		/// <param name="attributeAlias">Attribute alias</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute identifier</returns>
		int GetAttributeIdByAlias(string attributeAlias, int languageId = 0);

		/// <summary>
		/// Gets the attribute option id by option alias
		/// </summary>
		/// <param name="optionAlias">Attribute option alias</param>
		/// <param name="attributeId">Attribute identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute option identifier</returns>
		int GetAttributeOptionIdByAlias(string optionAlias, int attributeId, int languageId = 0);

		/// <summary>
		/// Gets the attribute alias by attribute id
		/// </summary>
		/// <param name="attributeId">Attribute identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute alias</returns>
		string GetAttributeAliasById(int attributeId, int languageId = 0);

		/// <summary>
		/// Get the attribute option alias by option id
		/// </summary>
		/// <param name="optionId">Option identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Attribute option alias</returns>
		string GetAttributeOptionAliasById(int optionId, int languageId = 0);

		/// <summary>
		/// Clears all cached variant mappings
		/// </summary>
		void ClearVariantCache();

		/// <summary>
		/// Gets the variant id by variant alias
		/// </summary>
		/// <param name="variantAlias">Variant alias</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Variant identifier</returns>
		int GetVariantIdByAlias(string variantAlias, int languageId = 0);

		/// <summary>
		/// Gets the variant option id by option alias
		/// </summary>
		/// <param name="optionAlias">Variant option alias</param>
		/// <param name="variantId">Variant identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Variant option identifier</returns>
		int GetVariantOptionIdByAlias(string optionAlias, int variantId, int languageId = 0);

		/// <summary>
		/// Gets the variant alias by variant id
		/// </summary>
		/// <param name="variantId">Variant identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Variant alias</returns>
		string GetVariantAliasById(int variantId, int languageId = 0);

		/// <summary>
		/// Get the variant option alias by value id
		/// </summary>
		/// <param name="optionId">Option identifier</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Variant option alias</returns>
		string GetVariantOptionAliasById(int optionId, int languageId = 0);

		/// <summary>
		/// Clears all cached common facet mappings
		/// </summary>
		void ClearCommonFacetCache();

		/// <summary>
		/// Get the common facet alias by facet group kind
		/// </summary>
		/// <param name="kind">Facet group kind</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Common facet alias</returns>
		string GetCommonFacetAliasByGroupKind(FacetGroupKind kind, int languageId);
	}
}
