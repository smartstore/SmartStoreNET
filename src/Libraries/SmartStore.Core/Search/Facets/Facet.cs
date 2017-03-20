using System.Collections.Generic;
using System.Diagnostics;

namespace SmartStore.Core.Search.Facets
{
	[DebuggerDisplay("Key: {Key}, IsChoice: {IsChoice}, HitCount: {HitCount}, Value: {Value}")]
	public class Facet
	{
		public Facet(FacetValue value)
			: this(value.ToString(), value)
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
			Published = true;
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

		/// <summary>
		/// Gets or sets whether the facet can be selected
		/// </summary>
		public bool IsChoice
		{
			get;
			set;
		}

		public bool Published
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

		public IList<Facet> Children
		{
			get;
			set;
		}
	}
}
