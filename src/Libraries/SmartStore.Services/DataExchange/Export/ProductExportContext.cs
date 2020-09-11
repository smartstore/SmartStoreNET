using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.DataExchange.Export
{
    /// <summary>
    /// Cargo data to reduce database round trips during work with product batches (export, list model creation etc.)
    /// </summary>
    public class ProductExportContext : PriceCalculationContext
    {
        private Func<int[], Multimap<int, ProductMediaFile>> _funcProductPictures;
        private Func<int[], Multimap<int, ProductTag>> _funcProductTags;
        private Func<int[], Multimap<int, ProductSpecificationAttribute>> _funcSpecificationAttributes;
        private Func<int[], Multimap<int, Download>> _funcDownloads;

        private LazyMultimap<ProductMediaFile> _productPictures;
        private LazyMultimap<ProductTag> _productTags;
        private LazyMultimap<ProductSpecificationAttribute> _specificationAttributes;
        private LazyMultimap<Download> _downloads;

        public ProductExportContext(
            IEnumerable<Product> products,
            Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
            Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
            Func<int[], Multimap<int, ProductSpecificationAttribute>> specificationAttributes,
            Func<int[], Multimap<int, TierPrice>> tierPrices,
            Func<int[], Multimap<int, ProductCategory>> productCategories,
            Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
            Func<int[], Multimap<int, Discount>> appliedDiscounts,
            Func<int[], Multimap<int, ProductBundleItem>> productBundleItems,
            Func<int[], Multimap<int, Product>> associatedProducts,
            Func<int[], Multimap<int, ProductMediaFile>> productPictures,
            Func<int[], Multimap<int, ProductTag>> productTags,
            Func<int[], Multimap<int, Download>> downloads)
            : base(products,
                attributes,
                attributeCombinations,
                tierPrices,
                productCategories,
                productManufacturers,
                appliedDiscounts,
                productBundleItems,
                associatedProducts)
        {
            _funcProductPictures = productPictures;
            _funcProductTags = productTags;
            _funcSpecificationAttributes = specificationAttributes;
            _funcDownloads = downloads;
        }

        public new void Clear()
        {
            _productPictures?.Clear();
            _productTags?.Clear();
            _specificationAttributes?.Clear();
            _downloads?.Clear();

            base.Clear();
        }

        public LazyMultimap<ProductMediaFile> ProductPictures
        {
            get
            {
                if (_productPictures == null)
                {
                    _productPictures = new LazyMultimap<ProductMediaFile>(keys => _funcProductPictures(keys), _productIds);
                }
                return _productPictures;
            }
        }

        public LazyMultimap<ProductTag> ProductTags
        {
            get
            {
                if (_productTags == null)
                {
                    _productTags = new LazyMultimap<ProductTag>(keys => _funcProductTags(keys), _productIds);
                }
                return _productTags;
            }
        }

        public LazyMultimap<ProductSpecificationAttribute> SpecificationAttributes
        {
            get
            {
                if (_specificationAttributes == null)
                {
                    _specificationAttributes = new LazyMultimap<ProductSpecificationAttribute>(keys => _funcSpecificationAttributes(keys), _productIds);
                }
                return _specificationAttributes;
            }
        }

        public LazyMultimap<Download> Downloads
        {
            get
            {
                if (_downloads == null)
                {
                    _downloads = new LazyMultimap<Download>(keys => _funcDownloads(keys), _productIds);
                }
                return _downloads;
            }
        }
    }
}
