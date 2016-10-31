namespace SmartStore.Core.Search.Facets
{
	public class Facet
	{
		public Facet(string key, FacetValue value, long hitCount)
		{
			Guard.NotEmpty(key, nameof(key));
			Guard.NotNull(value, nameof(value));
			Guard.NotNull(value.Value, nameof(value.Value));

			Key = key;
			Value = value;
			HitCount = hitCount;
		}

		public string Key
		{
			get;
			private set;
		}

		public FacetValue Value
		{
			get;
			private set;
		}

		public long HitCount
		{
			get;
			private set;
		}

		public FacetGroup FacetGroup
		{
			get;
			internal set;
		}
	}
}
