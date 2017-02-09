using System.Collections.Generic;

namespace SmartStore.Core.Search.Facets
{
	public class Facet
	{
		public Facet(FacetValue value)
			: this(value.GetStringValue(), value)
		{
		}

		public Facet(string key, FacetValue value)
		{
			Guard.NotEmpty(key, nameof(key));
			Guard.NotNull(value, nameof(value));

			Key = key;
			Value = value;
			Children = new List<Facet>();
			IsChoice = true;
		}

		public string Key
		{
			get;
			private set;
		}

		public string Label
		{
			get;
			set;
		}

		public FacetValue Value
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets whether the facet can be selected
		/// </summary>
		public bool IsChoice
		{
			get;
			set;
		}

		public long HitCount
		{
			get;
			set;
		}

		public FacetGroup FacetGroup
		{
			get;
			internal set;
		}

		public int ParentId
		{
			get;
			set;
		}

		public int DisplayOrder
		{
			get;
			set;
		}

		public IList<Facet> Children
		{
			get;
			set;
		}
	}
}
