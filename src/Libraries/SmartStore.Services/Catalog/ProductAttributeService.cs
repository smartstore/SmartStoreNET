using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services.Media;
using SmartStore.Core;
using SmartStore.Data.Caching;
using SmartStore.Services.Seo;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Catalog
{
    public partial class ProductAttributeService : IProductAttributeService
    {
		// 0 = ProductId, 1 = PageIndex, 2 = PageSize
		private const string PRODUCTVARIANTATTRIBUTES_COMBINATIONS_BY_ID_KEY = "SmartStore.productvariantattribute.combinations.id-{0}-{1}-{2}";
		private const string PRODUCTVARIANTATTRIBUTES_PATTERN_KEY = "SmartStore.productvariantattribute.";

		private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductVariantAttribute> _productVariantAttributeRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _pvacRepository;
        private readonly IRepository<ProductVariantAttributeValue> _productVariantAttributeValueRepository;
		private readonly IRepository<ProductBundleItemAttributeFilter> _productBundleItemAttributeFilterRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRequestCache _requestCache;
		private readonly IPictureService _pictureService;

        public ProductAttributeService(IRequestCache requestCache,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductVariantAttribute> productVariantAttributeRepository,
            IRepository<ProductVariantAttributeCombination> pvacRepository,
            IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
			IRepository<ProductBundleItemAttributeFilter> productBundleItemAttributeFilterRepository,
            IEventPublisher eventPublisher,
			IPictureService pictureService)
        {
            _requestCache = requestCache;
            _productAttributeRepository = productAttributeRepository;
            _productVariantAttributeRepository = productVariantAttributeRepository;
            _pvacRepository = pvacRepository;
            _productVariantAttributeValueRepository = productVariantAttributeValueRepository;
			_productBundleItemAttributeFilterRepository = productBundleItemAttributeFilterRepository;
            _eventPublisher = eventPublisher;
			_pictureService = pictureService;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		private IList<ProductVariantAttribute> GetSwitchedLoadedAttributeMappings(ICollection<int> productVariantAttributeIds)
		{
			if (productVariantAttributeIds != null && productVariantAttributeIds.Count > 0)
			{
				if (productVariantAttributeIds.Count == 1)
				{
					var pva = GetProductVariantAttributeById(productVariantAttributeIds.ElementAt(0));
					if (pva != null)
					{
						return new List<ProductVariantAttribute> { pva };
					}
				}
				else
				{
					return _productVariantAttributeRepository.GetMany(productVariantAttributeIds).ToList();
				}
			}

			return new List<ProductVariantAttribute>();
		}

		#region Product attributes

		public virtual void DeleteProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Delete(productAttribute);

            //cache
            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productAttribute);
        }

        public virtual IList<ProductAttribute> GetAllProductAttributes()
        {
			var query = from pa in _productAttributeRepository.Table
						orderby pa.DisplayOrder, pa.Name
						select pa;
			var productAttributes = query.ToListCached("db.prodattrs.all");
			return productAttributes;
		}

        public virtual ProductAttribute GetProductAttributeById(int productAttributeId)
        {
            if (productAttributeId == 0)
                return null;

			return _productAttributeRepository.GetByIdCached(productAttributeId, "db.prodattr.id-" + productAttributeId);
		}

        public virtual void InsertProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

			var alias = SeoExtensions.GetSeName(productAttribute.Alias);
			if (alias.HasValue() && _productAttributeRepository.TableUntracked.Any(x => x.Alias == alias))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			productAttribute.Alias = alias;

			_productAttributeRepository.Insert(productAttribute);
            
            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productAttribute);
        }

        public virtual void UpdateProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

			var alias = SeoExtensions.GetSeName(productAttribute.Alias);
			if (alias.HasValue() && _productAttributeRepository.TableUntracked.Any(x => x.Alias == alias && x.Id != productAttribute.Id))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			productAttribute.Alias = alias;

			_productAttributeRepository.Update(productAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productAttribute);
        }

        #endregion

        #region Product variant attributes mappings (ProductVariantAttribute)

        public virtual void DeleteProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

            _productVariantAttributeRepository.Delete(productVariantAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productVariantAttribute);
        }

		public virtual IList<ProductVariantAttribute> GetProductVariantAttributesByProductId(int productId)
        {
			var query = from pva in _productVariantAttributeRepository.Table
						orderby pva.DisplayOrder
						where pva.ProductId == productId
						select pva;
			var productVariantAttributes = query.ToListCached("db.prodvarattrs.all-" + productId);
			return productVariantAttributes;
		}

		public virtual Multimap<int, ProductVariantAttribute> GetProductVariantAttributesByProductIds(int[] productIds, AttributeControlType? controlType)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query = 
				from pva in _productVariantAttributeRepository.TableUntracked.Expand(x => x.ProductAttribute).Expand(x => x.ProductVariantAttributeValues)
				where productIds.Contains(pva.ProductId)
				select pva;

			if (controlType.HasValue)
			{
				query = query.Where(x => x.AttributeControlTypeId == ((int)controlType.Value));
			}

			var map = query
				.OrderBy(x => x.ProductId)
				.ThenBy(x => x.DisplayOrder)
				.ToList()
				.ToMultimap(x => x.ProductId, x => x);

			return map;
		}

        public virtual ProductVariantAttribute GetProductVariantAttributeById(int productVariantAttributeId)
        {
            if (productVariantAttributeId == 0)
                return null;

			return _productVariantAttributeRepository.GetByIdCached(productVariantAttributeId, "db.prodvarattr.id-" + productVariantAttributeId);
		}

		public virtual IList<ProductVariantAttribute> GetProductVariantAttributesByIds(IEnumerable<int> productVariantAttributeIds, IEnumerable<ProductVariantAttribute> attributes = null)
		{
			if (productVariantAttributeIds != null)
			{
				if (attributes != null)
				{
					var ids = new List<int>();
					var result = new List<ProductVariantAttribute>();

					foreach (var id in productVariantAttributeIds)
					{
						var pva = attributes.FirstOrDefault(x => x.Id == id);
						if (pva == null)
							ids.Add(id);
						else
							result.Add(pva);
					}

					var newLoadedMappings = GetSwitchedLoadedAttributeMappings(ids);

					result.AddRange(newLoadedMappings);

					return result;
				}

				return GetSwitchedLoadedAttributeMappings(productVariantAttributeIds.ToList());
			}

			return new List<ProductVariantAttribute>();
		}

        public virtual IEnumerable<ProductVariantAttributeValue> GetProductVariantAttributeValuesByIds(params int[] productVariantAttributeValueIds)
        {
            if (productVariantAttributeValueIds == null || productVariantAttributeValueIds.Length == 0)
            {
                return Enumerable.Empty<ProductVariantAttributeValue>();
            }

            return _productVariantAttributeValueRepository.GetMany(productVariantAttributeValueIds);
        }

        public virtual void InsertProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

			var existingAttribute = _productVariantAttributeRepository.TableUntracked.Expand(x => x.ProductAttribute).FirstOrDefault(
				x => x.ProductId == productVariantAttribute.ProductId && x.ProductAttributeId == productVariantAttribute.ProductAttributeId);

			if (existingAttribute != null)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingAttribute.ProductAttribute.Name.NaIfEmpty()));
			}

			_productVariantAttributeRepository.Insert(productVariantAttribute);
            
            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVariantAttribute);
        }

        public virtual void UpdateProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

			var existingAttribute = _productVariantAttributeRepository.TableUntracked.Expand(x => x.ProductAttribute).FirstOrDefault(
				x => x.ProductId == productVariantAttribute.ProductId && x.ProductAttributeId == productVariantAttribute.ProductAttributeId);

			if (existingAttribute != null && existingAttribute.Id != productVariantAttribute.Id)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingAttribute.ProductAttribute.Name.NaIfEmpty()));
			}

			_productVariantAttributeRepository.Update(productVariantAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productVariantAttribute);
        }

        #endregion

        #region Product variant attribute values (ProductVariantAttributeValue)

        public virtual void DeleteProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

            _productVariantAttributeValueRepository.Delete(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productVariantAttributeValue);
        }

        public virtual IList<ProductVariantAttributeValue> GetProductVariantAttributeValues(int productVariantAttributeId)
        {
			var query = from pvav in _productVariantAttributeValueRepository.Table
						orderby pvav.DisplayOrder
						where pvav.ProductVariantAttributeId == productVariantAttributeId
						select pvav;

			var productVariantAttributeValues = query.ToListCached("db.prodvarattrvalues.all-" + productVariantAttributeId);
			return productVariantAttributeValues;
		}

        public virtual ProductVariantAttributeValue GetProductVariantAttributeValueById(int productVariantAttributeValueId)
        {
            if (productVariantAttributeValueId == 0)
                return null;

			return _productVariantAttributeValueRepository.GetByIdCached(productVariantAttributeValueId, "db.prodvarattrval.id-" + productVariantAttributeValueId);
		}

        public virtual void InsertProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

			var existingValue = _productVariantAttributeValueRepository.TableUntracked.FirstOrDefault(
				x => x.ProductVariantAttributeId == productVariantAttributeValue.ProductVariantAttributeId && x.Name == productVariantAttributeValue.Name);

			if (existingValue != null)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingValue.Name.NaIfEmpty()));
			}

			var alias = SeoExtensions.GetSeName(productVariantAttributeValue.Alias);
			if (alias.HasValue() && _productVariantAttributeValueRepository.TableUntracked.Any(x => x.Alias == alias))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			productVariantAttributeValue.Alias = alias;

			_productVariantAttributeValueRepository.Insert(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVariantAttributeValue);
        }

        public virtual void UpdateProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

			var existingValue = _productVariantAttributeValueRepository.TableUntracked.FirstOrDefault(
				x => x.ProductVariantAttributeId == productVariantAttributeValue.ProductVariantAttributeId && x.Name == productVariantAttributeValue.Name);

			if (existingValue != null && existingValue.Id != productVariantAttributeValue.Id)
			{
				throw new SmartException(T("Common.Error.OptionAlreadyExists", existingValue.Name.NaIfEmpty()));
			}

			var alias = SeoExtensions.GetSeName(productVariantAttributeValue.Alias);
			if (alias.HasValue() && _productVariantAttributeValueRepository.TableUntracked.Any(x => x.Alias == alias && x.Id != productVariantAttributeValue.Id))
			{
				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}

			productVariantAttributeValue.Alias = alias;

			_productVariantAttributeValueRepository.Update(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productVariantAttributeValue);
        }

        #endregion

        #region Product variant attribute combinations (ProductVariantAttributeCombination)
        
		private void CombineAll(List<List<ProductVariantAttributeValue>> toCombine, List<List<ProductVariantAttributeValue>> result, int y, List<ProductVariantAttributeValue> tmp)
		{
			var combine = toCombine[y];

			for (int i = 0; i < combine.Count; ++i)
			{
				var lst = new List<ProductVariantAttributeValue>(tmp);
				lst.Add(combine[i]);

				if (y == (toCombine.Count - 1))
					result.Add(lst);
				else
					CombineAll(toCombine, result, y + 1, lst);
			}
		}

        public virtual void DeleteProductVariantAttributeCombination(ProductVariantAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            _pvacRepository.Delete(combination);

            //event notification
            _eventPublisher.EntityDeleted(combination);
        }

		public virtual IPagedList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(
			int productId, 
			int pageIndex, 
			int pageSize,
			bool untracked = true)
		{
			if (productId == 0)
			{
				return new PagedList<ProductVariantAttributeCombination>(new List<ProductVariantAttributeCombination>(), pageIndex, pageSize);
			}

			string key = string.Format(PRODUCTVARIANTATTRIBUTES_COMBINATIONS_BY_ID_KEY, productId, 0, int.MaxValue);
			return _requestCache.Get(key, () =>
			{
				var query = from pvac in (untracked ? _pvacRepository.TableUntracked : _pvacRepository.Table)
							orderby pvac.Id
							where pvac.ProductId == productId
							select pvac;

				var combinations = new PagedList<ProductVariantAttributeCombination>(query, pageIndex, pageSize);
				return combinations;
			});
		}

		public virtual IList<int> GetAllProductVariantAttributeCombinationPictureIds(int productId)
		{
			var pictureIds = new List<int>();

			if (productId == 0)
				return pictureIds;

			var query = from pvac in _pvacRepository.TableUntracked
						where
							pvac.ProductId == productId
							&& pvac.IsActive
							&& !String.IsNullOrEmpty(pvac.AssignedPictureIds)
						select pvac.AssignedPictureIds;

			var data = query.ToList();
			if (data.Any())
			{
				int id;
				var ids = string.Join(",", data).SplitSafe(",").Distinct();

				foreach (string str in ids)
				{
					if (int.TryParse(str, out id) && !pictureIds.Exists(i => i == id))
						pictureIds.Add(id);
				}
			}

			return pictureIds;
		}

		public virtual Multimap<int, ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int[] productIds)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query =
				from pvac in _pvacRepository.TableUntracked
				where productIds.Contains(pvac.ProductId)
				select pvac;

			var map = query
				.OrderBy(x => x.ProductId)
				.ToList()
				.ToMultimap(x => x.ProductId, x => x);

			return map;
		}

		public virtual decimal? GetLowestCombinationPrice(int productId)
		{
			if (productId == 0)
				return null;

			var query =
				from pvac in _pvacRepository.Table
				where pvac.ProductId == productId && pvac.Price != null && pvac.IsActive
				orderby pvac.Price ascending
				select pvac.Price;

			var price = query.FirstOrDefault();
			return price;
		}

        public virtual ProductVariantAttributeCombination GetProductVariantAttributeCombinationById(int productVariantAttributeCombinationId)
        {
            if (productVariantAttributeCombinationId == 0)
                return null;
            
            var combination = _pvacRepository.GetById(productVariantAttributeCombinationId);
            return combination;
        }

		public virtual ProductVariantAttributeCombination GetProductVariantAttributeCombinationBySku(string sku)
		{
			if (sku.IsEmpty())
				return null;

			var combination = _pvacRepository.Table.FirstOrDefault(x => x.Sku == sku);
			return combination;
		}

        public virtual void InsertProductVariantAttributeCombination(ProductVariantAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

			//if (combination.IsDefaultCombination)
			//{
			//	EnsureSingleDefaultVariant(combination);
			//}

            _pvacRepository.Insert(combination);

            //event notification
            _eventPublisher.EntityInserted(combination);
        }

        public virtual void UpdateProductVariantAttributeCombination(ProductVariantAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

			//if (combination.IsDefaultCombination)
			//{
			//	EnsureSingleDefaultVariant(combination);
			//}
			//else
			//{
			//	// check if it was default before modification...
			//	// but make it Type-Safe (resistant to code refactoring ;-))
			//	Expression<Func<ProductVariantAttributeCombination, bool>> expr = x => x.IsDefaultCombination;
			//	string propertyToCheck = expr.ExtractPropertyInfo().Name;

			//	object originalValue = null;
			//	if (_productVariantAttributeCombinationRepository.GetModifiedProperties(combination).TryGetValue(propertyToCheck, out originalValue))
			//	{
			//		bool wasDefault = (bool)originalValue;
			//		if (wasDefault) 
			//		{
			//			// we can't uncheck the default variant within a combination list,
			//			// we would't have a default combination anymore.
			//			combination.IsDefaultCombination = true;
			//		}
			//	}
			//}

            _pvacRepository.Update(combination);

            //event notification
            _eventPublisher.EntityUpdated(combination);
        }

		public virtual void CreateAllProductVariantAttributeCombinations(Product product)
		{
			// delete all existing combinations
			_pvacRepository.DeleteAll(x => x.ProductId == product.Id);

			var attributes = GetProductVariantAttributesByProductId(product.Id);
			if (attributes == null || attributes.Count <= 0)
				return;

			var toCombine = new List<List<ProductVariantAttributeValue>>();
			var resultMatrix = new List<List<ProductVariantAttributeValue>>();
			var tmp = new List<ProductVariantAttributeValue>();

			foreach (var attr in attributes)
			{
				var attributeValues = attr.ProductVariantAttributeValues.ToList();
				if (attributeValues.Count > 0)
					toCombine.Add(attributeValues);
			}

			if (toCombine.Count > 0)
			{
				CombineAll(toCombine, resultMatrix, 0, tmp);

				using (var scope = new DbContextScope(ctx: _pvacRepository.Context, autoCommit: false, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false))
				{
					ProductVariantAttributeCombination combination = null;

					var idx = 0;
					foreach (var values in resultMatrix)
					{
						idx++;

						string attrXml = "";
						for (var i = 0; i < values.Count; ++i)
						{
							var value = values[i];
							attrXml = attributes[i].AddProductAttribute(attrXml, value.Id.ToString());
						}

						combination = new ProductVariantAttributeCombination
						{
							ProductId = product.Id,
							AttributesXml = attrXml,
							StockQuantity = 10000,
							AllowOutOfStockOrders = true,
							IsActive = true
						};

						_pvacRepository.Insert(combination);
					}

					scope.Commit();

					if (combination != null)
					{
						// Perf: publish event for last one only
						_eventPublisher.EntityInserted(combination);
					}
				}

			}

			//foreach (var y in resultMatrix) {
			//	StringBuilder sb = new StringBuilder();
			//	foreach (var x in y) {
			//		sb.AppendFormat("{0} ", x.Name);
			//	}
			//	sb.ToString().Dump();
			//}
		}

		public virtual bool VariantHasAttributeCombinations(int productId)
		{
			if (productId == 0)
				return false;

			var query =
				from c in _pvacRepository.Table
				where c.ProductId == productId
				select c;

			return query.Select(x => x.Id).Any();
		}

        #endregion

		#region Product bundle item attribute filter

		public virtual void InsertProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter)
		{
			if (attributeFilter == null)
				throw new ArgumentNullException("attributeFilter");

			if (attributeFilter.AttributeId != 0 && attributeFilter.AttributeValueId != 0)
			{
				_productBundleItemAttributeFilterRepository.Insert(attributeFilter);

				_eventPublisher.EntityInserted(attributeFilter);
			}
		}

		public virtual void UpdateProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter)
		{
			if (attributeFilter == null)
				throw new ArgumentNullException("attributeFilter");

			_productBundleItemAttributeFilterRepository.Update(attributeFilter);

			_eventPublisher.EntityUpdated(attributeFilter);
		}

		public virtual void DeleteProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter)
		{
			if (attributeFilter == null)
				throw new ArgumentNullException("attributeFilter");

			_productBundleItemAttributeFilterRepository.Delete(attributeFilter);

			_eventPublisher.EntityDeleted(attributeFilter);
		}

		public virtual void DeleteProductBundleItemAttributeFilter(ProductBundleItem bundleItem)
		{
			if (bundleItem != null && bundleItem.Id != 0)
			{
				var attributeFilterQuery =
					from x in _productBundleItemAttributeFilterRepository.Table
					where x.BundleItemId == bundleItem.Id
					select x;

				attributeFilterQuery.ToList().Each(x => DeleteProductBundleItemAttributeFilter(x));
			}
		}

		public virtual void DeleteProductBundleItemAttributeFilter(int attributeId, int attributeValueId)
		{
			var attributeFilterQuery =
				from x in _productBundleItemAttributeFilterRepository.Table
				where x.AttributeId == attributeId && x.AttributeValueId == attributeValueId
				select x;

			attributeFilterQuery.ToList().Each(x => DeleteProductBundleItemAttributeFilter(x));
		}

		public virtual void DeleteProductBundleItemAttributeFilter(int attributeId)
		{
			var attributeFilterQuery =
				from x in _productBundleItemAttributeFilterRepository.Table
				where x.AttributeId == attributeId
				select x;

			attributeFilterQuery.ToList().Each(x => DeleteProductBundleItemAttributeFilter(x));
		}

		#endregion
	}
}
