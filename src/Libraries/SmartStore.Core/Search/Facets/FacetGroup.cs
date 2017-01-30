using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search.Facets
{
	public class FacetGroup
	{
		private readonly Dictionary<string, Facet> _facets;

		public FacetGroup(
			string key,
			string label,
			bool isMultiSelect,
			int displayOrder,
			IEnumerable<Facet> facets)
		{
			Guard.NotNull(key, nameof(key));
			Guard.NotNull(facets, nameof(facets));

			Key = key;
			Label = label;
			IsMultiSelect = isMultiSelect;
			DisplayOrder = displayOrder;

			_facets = new Dictionary<string, Facet>(StringComparer.OrdinalIgnoreCase);

			facets.Each(x =>
			{
				x.FacetGroup = this;
				x.Children.Each(y => y.FacetGroup = this);

				try
				{
					_facets.Add(x.Key, x);
				}
				catch (Exception exception)
				{
					exception.Dump();
				}
			});
		}

		public string Key
		{
			get;
			private set;
		}

		public string Label
		{
			get;
			private set;
		}

		public bool IsMultiSelect
		{
			get;
			private set;
		}

		public int DisplayOrder
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
