using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
		Availability,
		NewArrivals,
		Attribute,
		Variant
	}

	[DebuggerDisplay("Key: {Key}, Label: {Label}, Kind: {Kind}")]
	public class FacetGroup
	{
		private readonly Dictionary<string, Facet> _facets;
		private FacetGroupKind? _kind;

		public FacetGroup()
			: this (string.Empty, string.Empty, false, false, 0, Enumerable.Empty<Facet>())
		{
		}

		public FacetGroup(
			string key,
			string label,
			bool isMultiSelect,
			bool hasChildren,
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

		public static FacetGroupKind GetKindByKey(string key)
		{
			if (key.StartsWith("attrid"))
			{
				return FacetGroupKind.Attribute;
			}
			else if (key.StartsWith("variantid"))
			{
				return FacetGroupKind.Variant;
			}
			else if (key == "categoryid" || key == "notfeaturedcategoryid")
			{
				return FacetGroupKind.Category;
			}
			else if (key == "manufacturerid")
			{
				return FacetGroupKind.Brand;
			}
			else if (key == "price")
			{
				return FacetGroupKind.Price;
			}
			else if (key == "rating")
			{
				return FacetGroupKind.Rating;
			}
			else if (key == "deliveryid")
			{
				return FacetGroupKind.DeliveryTime;
			}
			else if (key == "available")
			{
				return FacetGroupKind.Availability;
			}
			else if (key == "createdon")
			{
				return FacetGroupKind.NewArrivals;
			}
			else
			{
				return FacetGroupKind.Unknown;
			}
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

		public bool HasChildren
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

		public IEnumerable<Facet> SelectedFacets
		{
			get
			{
				var parents = _facets.Values.Where(x => x.Value.IsSelected);
				var children = _facets.Values.SelectMany(x => x.Children).Where(x => x.Value.IsSelected);

				return parents.Concat(children);
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
					_kind = GetKindByKey(Key);
				}

				return _kind.Value;
			}
		}

		public FacetTemplateHint TemplateHint
		{
			get;
			set;
		}
	}
}
