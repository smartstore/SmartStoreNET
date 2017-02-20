using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Specification attribute service
	/// </summary>
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

			// (delete localized properties of options)
			var options = GetSpecificationAttributeOptionsBySpecificationAttribute(specificationAttribute.Id);
			options.Each(x => DeleteSpecificationAttributeOption(x));

            _specificationAttributeRepository.Delete(specificationAttribute);

            //event notification
            _eventPublisher.EntityDeleted(specificationAttribute);
        }

        public virtual void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

			var alias = SeoExtensions.GetSeName(specificationAttribute.Alias);
			if (alias.HasValue() && _specificationAttributeRepository.TableUntracked.Any(x => x.Alias == alias))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			specificationAttribute.Alias = alias;

			_specificationAttributeRepository.Insert(specificationAttribute);

            //event notification
            _eventPublisher.EntityInserted(specificationAttribute);
        }

        public virtual void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

			var alias = SeoExtensions.GetSeName(specificationAttribute.Alias);
			if (alias.HasValue() && _specificationAttributeRepository.TableUntracked.Any(x => x.Alias == alias && x.Id != specificationAttribute.Id))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			specificationAttribute.Alias = alias;

			_specificationAttributeRepository.Update(specificationAttribute);

            //event notification
            _eventPublisher.EntityUpdated(specificationAttribute);
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
            var query = from sao in _specificationAttributeOptionRepository.Table
                        orderby sao.DisplayOrder
                        where sao.SpecificationAttributeId == specificationAttributeId
                        select sao;
            var specificationAttributeOptions = query.ToList();
            return specificationAttributeOptions;
        }

        public virtual void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            _specificationAttributeOptionRepository.Delete(specificationAttributeOption);

            //event notification
            _eventPublisher.EntityDeleted(specificationAttributeOption);
        }

        public virtual void InsertSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

			var alias = SeoExtensions.GetSeName(specificationAttributeOption.Alias);
			if (alias.HasValue() && _specificationAttributeOptionRepository.TableUntracked.Any(x => x.Alias == alias))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			specificationAttributeOption.Alias = alias;

			_specificationAttributeOptionRepository.Insert(specificationAttributeOption);

            //event notification
            _eventPublisher.EntityInserted(specificationAttributeOption);
        }

        public virtual void UpdateSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

			var alias = SeoExtensions.GetSeName(specificationAttributeOption.Alias);
			if (alias.HasValue() && _specificationAttributeOptionRepository.TableUntracked.Any(x => x.Alias == alias && x.Id != specificationAttributeOption.Id))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			specificationAttributeOption.Alias = alias;

			_specificationAttributeOptionRepository.Update(specificationAttributeOption);

            //event notification
            _eventPublisher.EntityUpdated(specificationAttributeOption);
        }

        #endregion

        #region Product specification attribute

        public virtual void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            _productSpecificationAttributeRepository.Delete(productSpecificationAttribute);

            //event notification
            _eventPublisher.EntityDeleted(productSpecificationAttribute);
        }

        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId)
        {
            return GetProductSpecificationAttributesByProductId(productId, null, null);
        }

        public virtual IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId, bool? allowFiltering, bool? showOnProductPage)
        {
			var query = _productSpecificationAttributeRepository.Table.Where(psa => psa.ProductId == productId);

			if (allowFiltering.HasValue)
				query = query.Where(psa => psa.AllowFiltering == allowFiltering.Value);

			if (showOnProductPage.HasValue)
				query = query.Where(psa => psa.ShowOnProductPage == showOnProductPage.Value);

			query = query.OrderBy(psa => psa.DisplayOrder);

			var productSpecificationAttributes = query.ToListCached();
			return productSpecificationAttributes;
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

			var existingAttribute = _productSpecificationAttributeRepository.TableUntracked.FirstOrDefault(
				x => x.ProductId == productSpecificationAttribute.ProductId && x.SpecificationAttributeOptionId == productSpecificationAttribute.SpecificationAttributeOptionId);

			if (existingAttribute != null)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingAttribute.SpecificationAttributeOption?.Name.NaIfEmpty()));
			}

            _productSpecificationAttributeRepository.Insert(productSpecificationAttribute);

            //event notification
            _eventPublisher.EntityInserted(productSpecificationAttribute);
        }

        public virtual void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

			var existingAttribute = _productSpecificationAttributeRepository.TableUntracked.FirstOrDefault(
				x => x.ProductId == productSpecificationAttribute.ProductId && x.SpecificationAttributeOptionId == productSpecificationAttribute.SpecificationAttributeOptionId);

			if (existingAttribute != null && existingAttribute.Id != productSpecificationAttribute.Id)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingAttribute.SpecificationAttributeOption?.Name.NaIfEmpty()));
			}

			_productSpecificationAttributeRepository.Update(productSpecificationAttribute);

            //event notification
            _eventPublisher.EntityUpdated(productSpecificationAttribute);
        }

		public virtual void UpdateProductSpecificationMapping(int specificationAttributeId, string field, bool value) {
			if (specificationAttributeId == 0 || field.IsEmpty())
				return;

			bool isAllowFiltering = field.IsCaseInsensitiveEqual("AllowFiltering");
			bool isShowOnProductPage = field.IsCaseInsensitiveEqual("ShowOnProductPage");

			if (!isAllowFiltering && !isShowOnProductPage)
				return;

			var optionIds = (
				from sao in _specificationAttributeOptionRepository.Table
				where sao.SpecificationAttributeId == specificationAttributeId
				select sao.Id).ToList();


			foreach (int optionId in optionIds) {
				var query = 
					from psa in _productSpecificationAttributeRepository.Table
					where psa.SpecificationAttributeOptionId == optionId
					select psa;

				if (isAllowFiltering) {
					query = query.Where(a => a.AllowFiltering != value);
				}
				else if (isShowOnProductPage) {
					query = query.Where(a => a.ShowOnProductPage != value);
				}

				var attributes = query.ToList();

				foreach (var attribute in attributes) {
					if (isAllowFiltering) {
						attribute.AllowFiltering = value;
					}
					else if (isShowOnProductPage) {
						attribute.ShowOnProductPage = value;
					}

					UpdateProductSpecificationAttribute(attribute);
				}
			}
		}

        #endregion
    }
}
