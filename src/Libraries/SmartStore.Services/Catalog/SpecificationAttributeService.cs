using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Catalog
{
    public partial class SpecificationAttributeService : ISpecificationAttributeService
    {
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IEventPublisher _eventPublisher;

        public SpecificationAttributeService(
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IEventPublisher eventPublisher)
        {
            _specificationAttributeRepository = specificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _eventPublisher = eventPublisher;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region Specification attribute

        public virtual SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId)
        {
            if (specificationAttributeId == 0)
                return null;

            return _specificationAttributeRepository.GetById(specificationAttributeId);
        }

        public virtual IQueryable<SpecificationAttribute> GetSpecificationAttributes()
        {
            var query =
                from sa in _specificationAttributeRepository.Table
                orderby sa.DisplayOrder, sa.Name
                select sa;

            return query;
        }

        public virtual IQueryable<SpecificationAttribute> GetSpecificationAttributesByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return null;

            var query =
                from sa in _specificationAttributeRepository.Table
                where ids.Contains(sa.Id)
                orderby sa.DisplayOrder, sa.Name
                select sa;

            return query;
        }

        public virtual void DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            _specificationAttributeRepository.Delete(specificationAttribute);
        }

        public virtual void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            _specificationAttributeRepository.Insert(specificationAttribute);
        }

        public virtual void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            _specificationAttributeRepository.Update(specificationAttribute);
        }

        #endregion

        #region Specification attribute option

        public virtual SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOptionId)
        {
            if (specificationAttributeOptionId == 0)
                return null;

            return _specificationAttributeOptionRepository.GetById(specificationAttributeOptionId);
        }

        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int specificationAttributeId)
        {
            if (specificationAttributeId == 0)
            {
                return new List<SpecificationAttributeOption>();
            }

            var query = from sao in _specificationAttributeOptionRepository.Table
                        orderby sao.DisplayOrder
                        where sao.SpecificationAttributeId == specificationAttributeId
                        select sao;

            var specificationAttributeOptions = query.ToList();
            return specificationAttributeOptions;
        }

        public virtual Multimap<int, SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttributeIds(int[] specificationAttributeIds)
        {
            Guard.NotNull(specificationAttributeIds, nameof(specificationAttributeIds));

            var options = _specificationAttributeOptionRepository.TableUntracked
                .Where(x => specificationAttributeIds.Contains(x.SpecificationAttributeId))
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            var map = options.ToMultimap(x => x.SpecificationAttributeId, x => x);
            return map;
        }

        public virtual void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            _specificationAttributeOptionRepository.Delete(specificationAttributeOption);
        }

        public virtual void InsertSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            _specificationAttributeOptionRepository.Insert(specificationAttributeOption);
        }

        public virtual void UpdateSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            _specificationAttributeOptionRepository.Update(specificationAttributeOption);
        }

        #endregion

        #region Product specification attribute

        public virtual void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            _productSpecificationAttributeRepository.Delete(productSpecificationAttribute);
        }

        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId)
        {
            return GetProductSpecificationAttributesByProductId(productId, null, null);
        }

        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId, bool? allowFiltering, bool? showOnProductPage)
        {
            if (productId == 0)
            {
                return new List<ProductSpecificationAttribute>();
            }

            if (allowFiltering.HasValue || showOnProductPage.HasValue)
            {
                // Note: Join or Expand of SpecificationAttribute, both provides the same SQL.
                var joinedQuery =
                    from psa in _productSpecificationAttributeRepository.Table
                    join sao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals sao.Id
                    where psa.ProductId == productId
                    select new
                    {
                        ProductAttribute = psa,
                        Attribute = sao.SpecificationAttribute
                    };

                if (allowFiltering.HasValue)
                {
                    joinedQuery = joinedQuery.Where(x =>
                        (x.ProductAttribute.AllowFiltering == null && x.Attribute.AllowFiltering == allowFiltering.Value) ||
                        (x.ProductAttribute.AllowFiltering != null && x.ProductAttribute.AllowFiltering == allowFiltering.Value)
                    );
                }

                if (showOnProductPage.HasValue)
                {
                    joinedQuery = joinedQuery.Where(x =>
                        (x.ProductAttribute.ShowOnProductPage == null && x.Attribute.ShowOnProductPage == showOnProductPage.Value) ||
                        (x.ProductAttribute.ShowOnProductPage != null && x.ProductAttribute.ShowOnProductPage == showOnProductPage.Value)
                    );
                }

                var query = joinedQuery.Select(x => x.ProductAttribute).OrderBy(x => x.DisplayOrder);
                return query.ToListCached();
            }
            else
            {
                var query = _productSpecificationAttributeRepository.Table
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.DisplayOrder);

                return query.ToListCached();
            }
        }

        public virtual Multimap<int, ProductSpecificationAttribute> GetProductSpecificationAttributesByProductIds(int[] productIds)
        {
            Guard.NotNull(productIds, nameof(productIds));

            if (!productIds.Any())
            {
                return new Multimap<int, ProductSpecificationAttribute>();
            }

            var query = _productSpecificationAttributeRepository.TableUntracked
                .Expand(x => x.SpecificationAttributeOption.SpecificationAttribute)
                .Where(x => productIds.Contains(x.ProductId));

            var map = query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .ToMultimap(x => x.ProductId, x => x);

            return map;
        }

        public virtual ProductSpecificationAttribute GetProductSpecificationAttributeById(int productSpecificationAttributeId)
        {
            if (productSpecificationAttributeId == 0)
                return null;

            var productSpecificationAttribute = _productSpecificationAttributeRepository.GetById(productSpecificationAttributeId);
            return productSpecificationAttribute;
        }

        public virtual void InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            _productSpecificationAttributeRepository.Insert(productSpecificationAttribute);
        }

        public virtual void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            _productSpecificationAttributeRepository.Update(productSpecificationAttribute);
        }

        #endregion
    }
}
