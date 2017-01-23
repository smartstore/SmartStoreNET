using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search.Facets
{
	public class FacetGroup
	{
		private readonly Dictionary<string, Facet> _facets;

		public FacetGroup(FacetDescriptor descriptor, IEnumerable<Facet> facets)
			: this(descriptor.Key, facets)
		{
			IsMultiSelect = descriptor.IsMultiSelect;
		}

		public FacetGroup(string key, IEnumerable<Facet> facets)
		{
			Guard.NotNull(key, nameof(key));
			Guard.NotNull(facets, nameof(facets));

			Key = key;

			_facets = new Dictionary<string, Facet>(StringComparer.OrdinalIgnoreCase);

			facets.Each(x =>
			{
				if (!_facets.ContainsKey(x.Key))
				{
					x.FacetGroup = this;
					_facets.Add(x.Key, x);
				}
			});
		}

		public string Key
		{
			get;
			private set;
		}

		public bool IsMultiSelect
		{
			get;
			private set;
		}

		public IEnumerable<Facet> Facets
		{
			get
			{
				return _facets.Values;
			}
		}

		public Facet GetFacet(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			return _facets.Get(key);
		}
	}
}
