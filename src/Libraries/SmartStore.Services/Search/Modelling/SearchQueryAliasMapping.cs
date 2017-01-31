namespace SmartStore.Services.Search.Modelling
{
	public partial class SearchQueryAliasMapping
	{
		public SearchQueryAliasMapping(int fieldId, int valueId)
		{
			Guard.IsPositive(valueId, nameof(valueId));

			FieldId = fieldId;
			ValueId = valueId;
		}

		public int FieldId { get; private set; }
		public int ValueId { get; private set; }
	}
}
