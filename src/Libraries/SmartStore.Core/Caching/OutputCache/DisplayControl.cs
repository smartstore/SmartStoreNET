using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Topics;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
    public delegate IEnumerable<string> DisplayControlHandler(BaseEntity entity, IComponentContext ctx);

    public partial class DisplayControl : IDisplayControl
    {
        private static readonly ConcurrentDictionary<Type, DisplayControlHandler> _handlers = new ConcurrentDictionary<Type, DisplayControlHandler>
        {
            [typeof(BlogComment)] = (x, c) => new[] { "b" + ((BlogComment)x).BlogPostId },
            [typeof(BlogPost)] = (x, c) => new[] { "b" + x.Id },
            [typeof(Category)] = (x, c) => new[] { "c" + x.Id },
            [typeof(Manufacturer)] = (x, c) => new[] { "m" + x.Id },
            [typeof(ProductBundleItem)] = (x, c) => new[] { "p" + ((ProductBundleItem)x).ProductId },
            [typeof(ProductMediaFile)] = (x, c) => new[] { "p" + ((ProductMediaFile)x).ProductId },
            [typeof(ProductSpecificationAttribute)] = (x, c) => new[] { "p" + ((ProductSpecificationAttribute)x).ProductId },
            [typeof(ProductVariantAttributeCombination)] = (x, c) => new[] { "p" + ((ProductVariantAttributeCombination)x).ProductId },
            [typeof(TierPrice)] = (x, c) => new[] { "p" + ((TierPrice)x).ProductId },
            [typeof(CrossSellProduct)] = (x, c) => new[] { "p" + ((CrossSellProduct)x).ProductId1, "p" + ((CrossSellProduct)x).ProductId2 },
            [typeof(RelatedProduct)] = (x, c) => new[] { "p" + ((RelatedProduct)x).ProductId1, "p" + ((RelatedProduct)x).ProductId2 },
            [typeof(ProductCategory)] = (x, c) => new[] { "p" + ((ProductCategory)x).CategoryId, "p" + ((ProductCategory)x).ProductId },
            [typeof(ProductManufacturer)] = (x, c) => new[] { "p" + ((ProductManufacturer)x).ManufacturerId, "p" + ((ProductManufacturer)x).ProductId },
            [typeof(NewsItem)] = (x, c) => new[] { "n" + x.Id },
            [typeof(NewsComment)] = (x, c) => new[] { "n" + ((NewsComment)x).NewsItemId },
            [typeof(Topic)] = (x, c) => new[] { "t" + x.Id },
            [typeof(MediaFile)] = (x, c) => new[] { "mf" + x.Id },
            [typeof(SpecificationAttributeOption)] = (x, c) => ((SpecificationAttributeOption)x).ProductSpecificationAttributes.Select(y => "p" + y.ProductId),
            [typeof(ProductTag)] = (x, c) => ((ProductTag)x).Products.Select(y => "p" + y.Id),
            [typeof(Product)] = HandleProduct,
            [typeof(SpecificationAttribute)] = HandleSpecificationAttribute,
            [typeof(ProductVariantAttributeValue)] = HandleProductVariantAttributeValue,
            [typeof(Discount)] = HandleDiscount,
            [typeof(LocalizedProperty)] = HandleLocalizedProperty
        };

        private readonly HashSet<BaseEntity> _entities = new HashSet<BaseEntity>();
        private readonly IComponentContext _componentContext;

        private bool _isIdle;
        private bool? _isUncacheableRequest;

        public DisplayControl(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        #region Static

        public static bool ContainsHandlerFor(Type type)
        {
            Guard.NotNull(type, nameof(type));

            return _handlers.ContainsKey(type);
        }

        public static void RegisterHandlerFor(Type type, DisplayControlHandler handler)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(handler, nameof(handler));

            _handlers.TryAdd(type, handler);
        }

        #endregion

        #region Handlers

        private static IEnumerable<string> HandleProduct(BaseEntity entity, IComponentContext ctx)
        {
            var product = ((Product)entity);
            yield return "p" + entity.Id;
            if (product.ProductType == ProductType.GroupedProduct && product.ParentGroupedProductId > 0)
            {
                yield return "p" + product.ParentGroupedProductId;
            }
        }

        private static IEnumerable<string> HandleSpecificationAttribute(BaseEntity entity, IComponentContext ctx)
        {
            // Determine all affected products (which are assigned to this attribute).
            var specAttrId = ((SpecificationAttribute)entity).Id;
            var affectedProductIds = ctx.Resolve<IDbContext>().Set<ProductSpecificationAttribute>().AsNoTracking()
                .Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == specAttrId)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            foreach (var id in affectedProductIds)
            {
                yield return "p" + id;
            }
        }

        private static IEnumerable<string> HandleProductVariantAttributeValue(BaseEntity entity, IComponentContext ctx)
        {
            var pva = ((ProductVariantAttributeValue)entity).ProductVariantAttribute;
            if (pva != null)
            {
                yield return "p" + pva.ProductId;
            }
        }

        private static IEnumerable<string> HandleDiscount(BaseEntity entity, IComponentContext ctx)
        {
            var discount = (Discount)entity;
            if (discount.DiscountType == DiscountType.AssignedToCategories)
            {
                foreach (var category in discount.AppliedToCategories)
                {
                    yield return "c" + category.Id;
                }
            }
            else if (discount.DiscountType == DiscountType.AssignedToSkus)
            {
                foreach (var product in discount.AppliedToProducts)
                {
                    yield return "p" + product.Id;
                }
            }
        }

        private static IEnumerable<string> HandleLocalizedProperty(BaseEntity entity, IComponentContext ctx)
        {
            var lp = (LocalizedProperty)entity;
            string prefix = null;
            BaseEntity targetEntity = null;

            var dbContext = ctx.Resolve<IDbContext>();

            switch (lp.LocaleKeyGroup)
            {
                case nameof(BlogPost):
                    prefix = "b";
                    break;
                case nameof(Product):
                    prefix = "p";
                    break;
                case nameof(Category):
                    prefix = "c";
                    break;
                case nameof(Manufacturer):
                    prefix = "m";
                    break;
                case nameof(NewsItem):
                    prefix = "n";
                    break;
                case nameof(Topic):
                    prefix = "t";
                    break;
                case nameof(MediaFile):
                    prefix = "mf";
                    break;
                case nameof(SpecificationAttribute):
                    targetEntity = dbContext.Set<SpecificationAttribute>().Find(lp.EntityId);
                    break;
                case nameof(SpecificationAttributeOption):
                    targetEntity = dbContext.Set<SpecificationAttributeOption>().Find(lp.EntityId);
                    break;
                case nameof(ProductVariantAttributeValue):
                    targetEntity = dbContext.Set<ProductVariantAttributeValue>().Find(lp.EntityId);
                    break;
            }

            if (prefix.HasValue())
            {
                yield return prefix + lp.EntityId;
            }
            else if (targetEntity != null)
            {
                var tags = ctx.Resolve<IDisplayControl>().GetCacheControlTagsFor(targetEntity);
                foreach (var tag in tags)
                {
                    yield return tag;
                }
            }
        }

        #endregion

        public IDisposable BeginIdleScope()
        {
            _isIdle = true;
            return new ActionDisposable(() => _isIdle = false);
        }

        public virtual void Announce(BaseEntity entity)
        {
            if (!_isIdle && entity != null)
            {
                _entities.Add(entity);
            }
        }

        public bool IsDisplayed(BaseEntity entity)
        {
            if (entity == null)
                return false;

            return _entities.Contains(entity);
        }

        public void MarkRequestAsUncacheable()
        {
            // First wins: subsequent calls should not be able to cancel this
            if (!_isIdle)
                _isUncacheableRequest = true;
        }

        public bool IsUncacheableRequest => _isUncacheableRequest.GetValueOrDefault() == true;

        public virtual IEnumerable<string> GetCacheControlTagsFor(BaseEntity entity)
        {
            var empty = Enumerable.Empty<string>();

            if (entity == null || entity.IsTransientRecord())
            {
                return empty;
            }

            var type = entity.GetUnproxiedType();

            if (!_handlers.TryGetValue(type, out var handler))
            {
                return empty;
            }

            return handler.Invoke(entity, _componentContext);
        }

        public IEnumerable<string> GetAllCacheControlTags()
        {
            var tags = _entities
                .Where(x => x.Id > 0)
                .SelectMany(x => GetCacheControlTagsFor(x))
                .Where(x => x != null)
                .Distinct()
                .ToArray();

            return tags;
        }
    }
}
