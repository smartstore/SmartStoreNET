using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services.Media;

namespace SmartStore.Services.Catalog
{

    public partial class ProductAttributeService : IProductAttributeService
    {
        #region Constants
        private const string PRODUCTATTRIBUTES_ALL_KEY = "SmartStore.productattribute.all";
        private const string PRODUCTVARIANTATTRIBUTES_ALL_KEY = "SmartStore.productvariantattribute.all-{0}";
        private const string PRODUCTVARIANTATTRIBUTEVALUES_ALL_KEY = "SmartStore.productvariantattributevalue.all-{0}";
        private const string PRODUCTATTRIBUTES_PATTERN_KEY = "SmartStore.productattribute.";
        private const string PRODUCTVARIANTATTRIBUTES_PATTERN_KEY = "SmartStore.productvariantattribute.";
        private const string PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY = "SmartStore.productvariantattributevalue.";
        private const string PRODUCTATTRIBUTES_BY_ID_KEY = "SmartStore.productattribute.id-{0}";
        private const string PRODUCTVARIANTATTRIBUTES_BY_ID_KEY = "SmartStore.productvariantattribute.id-{0}";
        private const string PRODUCTVARIANTATTRIBUTEVALUES_BY_ID_KEY = "SmartStore.productvariantattributevalue.id-{0}";

        #endregion

        #region Fields

        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductVariantAttribute> _productVariantAttributeRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _productVariantAttributeCombinationRepository;
        private readonly IRepository<ProductVariantAttributeValue> _productVariantAttributeValueRepository;
		private readonly IRepository<ProductBundleItemAttributeFilter> _productBundleItemAttributeFilterRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
		private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productAttributeRepository">Product attribute repository</param>
        /// <param name="productVariantAttributeRepository">Product variant attribute mapping repository</param>
        /// <param name="productVariantAttributeCombinationRepository">Product variant attribute combination repository</param>
        /// <param name="productVariantAttributeValueRepository">Product variant attribute value repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductAttributeService(ICacheManager cacheManager,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductVariantAttribute> productVariantAttributeRepository,
            IRepository<ProductVariantAttributeCombination> productVariantAttributeCombinationRepository,
            IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
			IRepository<ProductBundleItemAttributeFilter> productBundleItemAttributeFilterRepository,
            IEventPublisher eventPublisher,
			IPictureService pictureService)
        {
            _cacheManager = cacheManager;
            _productAttributeRepository = productAttributeRepository;
            _productVariantAttributeRepository = productVariantAttributeRepository;
            _productVariantAttributeCombinationRepository = productVariantAttributeCombinationRepository;
            _productVariantAttributeValueRepository = productVariantAttributeValueRepository;
			_productBundleItemAttributeFilterRepository = productBundleItemAttributeFilterRepository;
            _eventPublisher = eventPublisher;
			_pictureService = pictureService;
        }

        #endregion

		#region Utilities

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

		#endregion

		#region Methods

		#region Product attributes

		public virtual void DeleteProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Delete(productAttribute);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productAttribute);
        }

        public virtual IList<ProductAttribute> GetAllProductAttributes()
        {
            string key = PRODUCTATTRIBUTES_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from pa in _productAttributeRepository.Table
                            orderby pa.Name
                            select pa;
                var productAttributes = query.ToList();
                return productAttributes;
            });
        }

        public virtual ProductAttribute GetProductAttributeById(int productAttributeId)
        {
            if (productAttributeId == 0)
                return null;

            string key = string.Format(PRODUCTATTRIBUTES_BY_ID_KEY, productAttributeId);
            return _cacheManager.Get(key, () => { 
                return _productAttributeRepository.GetById(productAttributeId); 
            });
        }

        public virtual void InsertProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Insert(productAttribute);
            
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productAttribute);
        }

        public virtual void UpdateProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            _productAttributeRepository.Update(productAttribute);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

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

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productVariantAttribute);
        }

		public virtual IList<ProductVariantAttribute> GetProductVariantAttributesByProductId(int productId)
        {
			string key = string.Format(PRODUCTVARIANTATTRIBUTES_ALL_KEY, productId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pva in _productVariantAttributeRepository.Table
                            orderby pva.DisplayOrder
							where pva.ProductId == productId
                            select pva;
                var productVariantAttributes = query.ToList();
                return productVariantAttributes;
            });
        }

		public virtual Multimap<int, ProductVariantAttribute> GetProductVariantAttributesByProductIds(int[] productIds, AttributeControlType? controlType)
		{
			Guard.ArgumentNotNull(() => productIds);

			var query = from pva in _productVariantAttributeRepository.TableUntracked.Expand(x => x.ProductVariantAttributeValues)
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

            string key = string.Format(PRODUCTVARIANTATTRIBUTES_BY_ID_KEY, productVariantAttributeId);

            return _cacheManager.Get(key, () =>
			{ 
                return _productVariantAttributeRepository.GetById(productVariantAttributeId); 
            });
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

            _productVariantAttributeRepository.Insert(productVariantAttribute);
            
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVariantAttribute);
        }

        public virtual void UpdateProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

            _productVariantAttributeRepository.Update(productVariantAttribute);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

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

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productVariantAttributeValue);
        }

        public virtual IList<ProductVariantAttributeValue> GetProductVariantAttributeValues(int productVariantAttributeId)
        {
            string key = string.Format(PRODUCTVARIANTATTRIBUTEVALUES_ALL_KEY, productVariantAttributeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pvav in _productVariantAttributeValueRepository.Table
                            orderby pvav.DisplayOrder
                            where pvav.ProductVariantAttributeId == productVariantAttributeId
                            select pvav;
                var productVariantAttributeValues = query.ToList();
                return productVariantAttributeValues;
            });
        }

        public virtual ProductVariantAttributeValue GetProductVariantAttributeValueById(int productVariantAttributeValueId)
        {
            if (productVariantAttributeValueId == 0)
                return null;

           string key = string.Format(PRODUCTVARIANTATTRIBUTEVALUES_BY_ID_KEY, productVariantAttributeValueId);
           return _cacheManager.Get(key, () =>
           {
               return _productVariantAttributeValueRepository.GetById(productVariantAttributeValueId);
           });
        }

        public virtual void InsertProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

            _productVariantAttributeValueRepository.Insert(productVariantAttributeValue);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVariantAttributeValue);
        }

        public virtual void UpdateProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

            _productVariantAttributeValueRepository.Update(productVariantAttributeValue);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);

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

            _productVariantAttributeCombinationRepository.Delete(combination);

            //event notification
            _eventPublisher.EntityDeleted(combination);
        }

		public virtual IList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(int productId)
        {
			if (productId == 0)
                return new List<ProductVariantAttributeCombination>();

            var query = from pvac in _productVariantAttributeCombinationRepository.Table
                        orderby pvac.Id
						where pvac.ProductId == productId
                        select pvac;

            var combinations = query.ToList();
            return combinations;
        }

		public virtual Multimap<int, ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int[] productIds)
		{
			Guard.ArgumentNotNull(() => productIds);

			var query =
				from pvac in _productVariantAttributeCombinationRepository.TableUntracked
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
				from pvac in _productVariantAttributeCombinationRepository.Table
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
            
            var combination = _productVariantAttributeCombinationRepository.GetById(productVariantAttributeCombinationId);
            return combination;
        }

		public virtual ProductVariantAttributeCombination GetProductVariantAttributeCombinationBySku(string sku)
		{
			if (sku.IsEmpty())
				return null;

			var combination = _productVariantAttributeCombinationRepository.Table.FirstOrDefault(x => x.Sku == sku);
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

            _productVariantAttributeCombinationRepository.Insert(combination);

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

            _productVariantAttributeCombinationRepository.Update(combination);

            //event notification
            _eventPublisher.EntityUpdated(combination);
        }

		public virtual void CreateAllProductVariantAttributeCombinations(Product product)
		{
			// delete all existing combinations
			foreach(var itm in GetAllProductVariantAttributeCombinations(product.Id))
			{
				DeleteProductVariantAttributeCombination(itm);
			}

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

				foreach (var values in resultMatrix)
				{
					string attrXml = "";
					foreach (var x in values) 
                    {
						attrXml = attributes[values.IndexOf(x)].AddProductAttribute(attrXml, x.Id.ToString());
					}

					var combination = new ProductVariantAttributeCombination()
					{
						ProductId = product.Id,
						AttributesXml = attrXml,
						StockQuantity = 10000,
						AllowOutOfStockOrders = true,
						IsActive = true
					};

					_productVariantAttributeCombinationRepository.Insert(combination);
					_eventPublisher.EntityInserted(combination);
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
				from c in _productVariantAttributeCombinationRepository.Table
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

		#endregion
	}
}
