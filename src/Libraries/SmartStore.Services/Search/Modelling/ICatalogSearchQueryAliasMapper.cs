namespace SmartStore.Services.Search.Modelling
{
	public interface ICatalogSearchQueryAliasMapper
	{
		SearchQueryAliasMapping GetAttributeByAlias(string attributeAlias, string valueAlias);
	}
}
