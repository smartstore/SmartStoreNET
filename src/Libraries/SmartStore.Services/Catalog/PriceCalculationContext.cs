using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Cargo data to reduce database round trips during price calculation
    /// </summary>
    public class PriceCalculationContext
    {
        protected List<int> _productIds;
        private List<int> _productIdsTierPrices;
        private List<int> _productIdsAppliedDiscounts;
        private List<int> _bundledProductIds;
        private List<int> _groupedProductIds;

        private Func<int[], Multimap<int, ProductVariantAttribute>> _funcAttributes;
        private Func<int[], Multimap<int, ProductVariantAttributeCombination>> _funcAttributeCombinations;
        private Func<int[], Multimap<int, TierPrice>> _funcTierPrices;
        private Func<int[], Multimap<int, ProductCategory>> _funcProductCategories;
        private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
        private Func<int[], Multimap<int, Discount>> _funcAppliedDiscounts;
        private Func<int[], Multimap<int, ProductBundleItem>> _funcProductBundleItems;
        private Func<int[], Multimap<int, Product>> _funcAssociatedProducts;

        private LazyMultimap<ProductVariantAttribute> _attributes;
        private LazyMultimap<ProductVariantAttributeCombination> _attributeCombinations;
        private LazyMultimap<TierPrice> _tierPrices;
        private LazyMultimap<ProductCategory> _productCategories;
        private LazyMultimap<ProductManufacturer> _productManufacturers;
        private LazyMultimap<Discount> _appliedDiscounts;
        private LazyMultimap<ProductBundleItem> _productBundleItems;
        private LazyMultimap<Product> _associatedProducts;

        public PriceCalculationContext(IEnumerable<Product> products,
            Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
            Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
            Func<int[], Multimap<int, TierPrice>> tierPrices,
            Func<int[], Multimap<int, ProductCategory>> productCategories,
            Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
            Func<int[], Multimap<int, Discount>> appliedDiscounts,
            Func<int[], Multimap<int, ProductBundleItem>> productBundleItems,
            Func<int[], Multimap<int, Product>> associatedProducts)
        {
            if (products == null)
            {
                _productIds = new List<int>();
                _productIdsTierPrices = new List<int>();
                _productIdsAppliedDiscounts = new List<int>();
                _bundledProductIds = new List<int>();
                _groupedProductIds = new List<int>();
            }
            else
            {
                _productIds = new List<int>(products.Select(x => x.Id));
                _productIdsTierPrices = new List<int>(products.Where(x => x.HasTierPrices).Select(x => x.Id));
                _productIdsAppliedDiscounts = new List<int>(products.Where(x => x.HasDiscountsApplied).Select(x => x.Id));
                _bundledProductIds = new List<int>(products.Where(x => x.ProductType == ProductType.BundledProduct).Select(x => x.Id));
                _groupedProductIds = new List<int>(products.Where(x => x.ProductType == ProductType.GroupedProduct).Select(x => x.Id));
            }

            _funcAttributes = attributes;
            _funcAttributeCombinations = attributeCombinations;
            _funcTierPrices = tierPrices;
            _funcProductCategories = productCategories;
            _funcProductManufacturers = productManufacturers;
            _funcAppliedDiscounts = appliedDiscounts;
            _funcProductBundleItems = productBundleItems;
            _funcAssociatedProducts = associatedProducts;
        }

        public IReadOnlyList<int> ProductIds => _productIds;

        public void Clear()
        {
            _attributes?.Clear();
            _attributeCombinations?.Clear();
            _tierPrices?.Clear();
            _productCategories?.Clear();
            _productManufacturers?.Clear();
            _appliedDiscounts?.Clear();
            _productBundleItems?.Clear();
            _associatedProducts?.Clear();

            _bundledProductIds.Clear();
            _groupedProductIds.Clear();
        }

        public LazyMultimap<ProductVariantAttribute> Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    _attributes = new LazyMultimap<ProductVariantAttribute>(keys => _funcAttributes(keys), _productIds);
                }
                return _attributes;
            }
        }

        public LazyMultimap<ProductVariantAttributeCombination> AttributeCombinations
        {
            get
            {
                if (_attributeCombinations == null)
                {
                    _attributeCombinations = new LazyMultimap<ProductVariantAttributeCombination>(keys => _funcAttributeCombinations(keys), _productIds);
                }
                return _attributeCombinations;
            }
        }

        public LazyMultimap<TierPrice> TierPrices
        {
            get
            {
                if (_tierPrices == null)
                {
                    _tierPrices = new LazyMultimap<TierPrice>(keys => _funcTierPrices(keys), _productIdsTierPrices);
                }
                return _tierPrices;
            }
        }

        public LazyMultimap<ProductCategory> ProductCategories
        {
            get
            {
                if (_productCategories == null)
                {
                    _productCategories = new LazyMultimap<ProductCategory>(keys => _funcProductCategories(keys), _productIds);
                }
                return _productCategories;
            }
        }

        public LazyMultimap<ProductManufacturer> ProductManufacturers
        {
            get
            {
                if (_productManufacturers == null)
                {
                    _productManufacturers = new LazyMultimap<ProductManufacturer>(keys => _funcProductManufacturers(keys), _productIds);
                }
                return _productManufacturers;
            }
        }

        public LazyMultimap<Discount> AppliedDiscounts
        {
            get
            {
                if (_appliedDiscounts == null)
                {
                    _appliedDiscounts = new LazyMultimap<Discount>(keys => _funcAppliedDiscounts(keys), _productIdsAppliedDiscounts);
                }
                return _appliedDiscounts;
            }
        }

        public LazyMultimap<ProductBundleItem> ProductBundleItems
        {
            get
            {
                if (_productBundleItems == null)
                {
                    _productBundleItems = new LazyMultimap<ProductBundleItem>(keys => _funcProductBundleItems(keys), _bundledProductIds);
                }
                return _productBundleItems;
            }
        }

        public LazyMultimap<Product> AssociatedProducts
        {
            get
            {
                if (_associatedProducts == null)
                {
                    _associatedProducts = new LazyMultimap<Product>(keys => _funcAssociatedProducts(keys), _groupedProductIds);
                }

                return _associatedProducts;
            }
        }

        public void Collect(IEnumerable<int> productIds)
        {
            Attributes.Collect(productIds);
            AttributeCombinations.Collect(productIds);
            TierPrices.Collect(productIds);
            ProductCategories.Collect(productIds);
            AppliedDiscounts.Collect(productIds);
            ProductBundleItems.Collect(productIds);
            AssociatedProducts.Collect(productIds);
        }
    }
}
