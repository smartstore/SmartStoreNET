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
        Variant,
        Forum,
        Customer,
        Date
    }

    [DebuggerDisplay("Key: {Key}, Label: {Label}, Kind: {Kind}")]
    public class FacetGroup
    {
        private readonly Dictionary<string, Facet> _facets;
        private FacetGroupKind? _kind;

        public FacetGroup()
            : this(string.Empty, string.Empty, string.Empty, false, false, 0, Enumerable.Empty<Facet>())
        {
        }

        public FacetGroup(
            string scope,
            string key,
            string label,
            bool isMultiSelect,
            bool hasChildren,
            int displayOrder,
            IEnumerable<Facet> facets)
        {
            Guard.NotNull(scope, nameof(scope));
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(facets, nameof(facets));

            Scope = scope;
            Key = key;
            Label = label;
            IsMultiSelect = isMultiSelect;
            DisplayOrder = displayOrder;
            IsScrollable = true;

            _facets = new Dictionary<string, Facet>(StringComparer.OrdinalIgnoreCase);

            facets.Each(x =>
            {
                x.FacetGroup = this;
                x.Children.Each(y => y.FacetGroup = this);

                try
                {
                    _facets.Add(x.Key, x);
                }
                catch (Exception ex)
                {
                    ex.Dump();
                }
            });
        }

        public static FacetGroupKind GetKindByKey(string scope, string key)
        {
            if (key.StartsWith("attrid"))
            {
                return FacetGroupKind.Attribute;
            }
            else if (key.StartsWith("variantid"))
            {
                return FacetGroupKind.Variant;
            }

            switch (key)
            {
                case "categoryid":
                case "notfeaturedcategoryid":
                    return FacetGroupKind.Category;
                case "manufacturerid":
                    return FacetGroupKind.Brand;
                case "price":
                    return FacetGroupKind.Price;
                case "rating":
                    return FacetGroupKind.Rating;
                case "deliveryid":
                    return FacetGroupKind.DeliveryTime;
                case "available":
                    return FacetGroupKind.Availability;
                case "createdon":
                    return scope == "Catalog" ? FacetGroupKind.NewArrivals : FacetGroupKind.Date;
                case "forumid":
                    return FacetGroupKind.Forum;
                case "customerid":
                    return FacetGroupKind.Customer;
                default:
                    return FacetGroupKind.Unknown;
            }
        }

        public string Scope
        {
            get;
            private set;
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

        public bool IsScrollable
        {
            get;
            set;
        }

        public IEnumerable<Facet> Facets => _facets.Values;

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
                    _kind = GetKindByKey(Scope, Key);
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
