using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Search.Facets
{
	public class FacetGroup
	{
		private readonly Dictionary<string, Facet> _facets;

		public FacetGroup(FacetDescriptor descriptor, IEnumerable<Facet> facets)
		{
			Guard.NotNull(descriptor, nameof(descriptor));
			Guard.NotNull(descriptor.Key, nameof(descriptor.Key));
			Guard.NotNull(facets, nameof(facets));

			Key = descriptor.Key;
			IsMultiSelect = descriptor.IsMultiSelect;

			_facets = new Dictionary<string, Facet>(StringComparer.OrdinalIgnoreCase);

			var orderedFacets = descriptor.OrderBy == FacetDescriptor.Sorting.HitsDesc
				? facets.OrderByDescending(x => x.HitCount)
				: facets.OrderBy(x => x.Key);

			orderedFacets.Each(x =>
			{
				x.FacetGroup = this;
				_facets.Add(x.Key, x);
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
