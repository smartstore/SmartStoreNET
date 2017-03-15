using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SmartStore.Core.Search.Facets
{
	public enum FacetGroupKind
	{
		Unknown = -1,
		Category,
		Brand,
		Price,
		Rating,
		DeliveryTime,
		Stock,
		Attribute,
		Variant
	}

	[DebuggerDisplay("Key: {Key}, Label: {Label}, Kind: {Kind}")]
	public class FacetGroup
	{
		private readonly Dictionary<string, Facet> _facets;
		private FacetGroupKind? _kind;

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

		public FacetGroupKind Kind
		{
			get
			{
				if (_kind == null)
				{
					if (Key.StartsWith("attrid"))
					{
						_kind = FacetGroupKind.Attribute;
					}
					else if (Key.StartsWith("variantid"))
					{
						_kind = FacetGroupKind.Variant;
					}
					else if (Key == "categoryid" || Key == "notfeaturedcategoryid")
					{
						_kind = FacetGroupKind.Category;
					}
					else if (Key == "manufacturerid")
					{
						_kind = FacetGroupKind.Brand;
					}
					else if (Key == "price")
					{
						_kind = FacetGroupKind.Price;
					}
					else if (Key == "rating")
					{
						_kind = FacetGroupKind.Rating;
					}
					else if (Key == "deliveryid")
					{
						_kind = FacetGroupKind.DeliveryTime;
					}
					else
					{
						_kind = FacetGroupKind.Unknown;
					}
				}

				return _kind.Value;
			}
		}
	}
}
