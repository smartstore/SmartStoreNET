using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Data.Caching;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore.Services.Catalog
{
	public partial class ProductAttributeService : IProductAttributeService
    {
		// 0 = ProductId, 1 = PageIndex, 2 = PageSize
		private const string PRODUCTVARIANTATTRIBUTES_COMBINATIONS_BY_ID_KEY = "SmartStore.productvariantattribute.combinations.id-{0}-{1}-{2}";
		private const string PRODUCTVARIANTATTRIBUTES_PATTERN_KEY = "SmartStore.productvariantattribute.*";

		// 0 = Attribute value ids, e.g. 16-254-1245
		private const string PRODUCTVARIANTATTRIBUTEVALUES_BY_IDS_KEY = "SmartStore.productvariantattributevalues.ids-{0}";
		private const string PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY = "SmartStore.productvariantattributevalues*";

		private readonly IRepository<ProductAttribute> _productAttributeRepository;
		private readonly IRepository<ProductAttributeOption> _productAttributeOptionRepository;
		private readonly IRepository<ProductAttributeOptionsSet> _productAttributeOptionsSetRepository;
		private readonly IRepository<ProductVariantAttribute> _productVariantAttributeRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _pvacRepository;
        private readonly IRepository<ProductVariantAttributeValue> _productVariantAttributeValueRepository;
		private readonly IRepository<ProductBundleItemAttributeFilter> _productBundleItemAttributeFilterRepository;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IEventPublisher _eventPublisher;
        private readonly IRequestCache _requestCache;
		private readonly IPictureService _pictureService;

        public ProductAttributeService(
			IRequestCache requestCache,
            IRepository<ProductAttribute> productAttributeRepository,
			IRepository<ProductAttributeOption> productAttributeOptionRepository,
			IRepository<ProductAttributeOptionsSet> productAttributeOptionsSetRepository,
			IRepository<ProductVariantAttribute> productVariantAttributeRepository,
            IRepository<ProductVariantAttributeCombination> pvacRepository,
            IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
			IRepository<ProductBundleItemAttributeFilter> productBundleItemAttributeFilterRepository,
			ILocalizedEntityService localizedEntityService,
			IEventPublisher eventPublisher,
			IPictureService pictureService)
        {
            _requestCache = requestCache;
            _productAttributeRepository = productAttributeRepository;
			_productAttributeOptionRepository = productAttributeOptionRepository;
			_productAttributeOptionsSetRepository = productAttributeOptionsSetRepository;
            _productVariantAttributeRepository = productVariantAttributeRepository;
            _pvacRepository = pvacRepository;
            _productVariantAttributeValueRepository = productVariantAttributeValueRepository;
			_productBundleItemAttributeFilterRepository = productBundleItemAttributeFilterRepository;
			_localizedEntityService = localizedEntityService;
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
					var pva = GetProductVariantAttributeById(productVariantAttributeIds.First());
					if (pva != null)
					{
						return new List<ProductVariantAttribute> { pva };
					}
				}
				else
				{
					return _productVariantAttributeRepository
						.GetMany(productVariantAttributeIds)
						.OrderBy(x => x.DisplayOrder)
						.ToList();
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
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
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

			_productAttributeRepository.Insert(productAttribute);
            
            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

        public virtual void UpdateProductAttribute(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

			_productAttributeRepository.Update(productAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

		public virtual Multimap<string, int> GetExportFieldMappings(string fieldPrefix)
		{
			Guard.NotEmpty(fieldPrefix, nameof(fieldPrefix));

			var result = new Multimap<string, int>(StringComparer.OrdinalIgnoreCase);

			if (!fieldPrefix.EndsWith(":"))
			{
				fieldPrefix = fieldPrefix + ":";
			}

			var mappings = _productAttributeRepository.TableUntracked
				.Where(x => !string.IsNullOrEmpty(x.ExportMappings))
				.Select(x => new
				{
					x.Id,
					x.ExportMappings
				})
				.ToList();

			foreach (var mapping in mappings)
			{
				var rows = mapping.ExportMappings.SplitSafe(Environment.NewLine)
					.Where(x => x.StartsWith(fieldPrefix, StringComparison.InvariantCultureIgnoreCase));

				foreach (var row in rows)
				{
					var exportFieldName = row.Substring(fieldPrefix.Length).TrimEnd();
					if (exportFieldName.HasValue())
					{
						result.Add(exportFieldName, mapping.Id);
					}
				}
			}

			return result;
		}

		#endregion

		#region Product attribute options

		public virtual ProductAttributeOption GetProductAttributeOptionById(int id)
		{
			if (id == 0)
				return null;

			return _productAttributeOptionRepository.GetById(id);
		}

		public virtual IList<ProductAttributeOption> GetProductAttributeOptionsByOptionsSetId(int optionsSetId)
		{
			if (optionsSetId == 0)
				return new List<ProductAttributeOption>();

			var entities = _productAttributeOptionRepository.Table
				.Where(x => x.ProductAttributeOptionsSetId == optionsSetId)
				.OrderBy(x => x.DisplayOrder)
				.ThenBy(x => x.Name)
				.ToList();

			return entities;
		}

		public virtual IList<ProductAttributeOption> GetProductAttributeOptionsByAttributeId(int attributeId)
		{
			if (attributeId == 0)
				return new List<ProductAttributeOption>();

			var entities =
				from o in _productAttributeOptionRepository.Table
				join os in _productAttributeOptionsSetRepository.Table on o.ProductAttributeOptionsSetId equals os.Id
				where os.ProductAttributeId == attributeId
				select o;

			return entities.ToList();
		}

		public virtual void DeleteProductAttributeOption(ProductAttributeOption productAttributeOption)
		{
			Guard.NotNull(productAttributeOption, nameof(productAttributeOption));

			_productAttributeOptionRepository.Delete(productAttributeOption);
		}

		public virtual void InsertProductAttributeOption(ProductAttributeOption productAttributeOption)
		{
			Guard.NotNull(productAttributeOption, nameof(productAttributeOption));

			_productAttributeOptionRepository.Insert(productAttributeOption);
		}

		public virtual void UpdateProductAttributeOption(ProductAttributeOption productAttributeOption)
		{
			Guard.NotNull(productAttributeOption, nameof(productAttributeOption));

			_productAttributeOptionRepository.Update(productAttributeOption);
		}

		#endregion

		#region Product attribute options sets

		public virtual ProductAttributeOptionsSet GetProductAttributeOptionsSetById(int id)
		{
			if (id == 0)
				return null;

			return _productAttributeOptionsSetRepository.GetById(id);
		}

		public virtual IList<ProductAttributeOptionsSet> GetProductAttributeOptionsSetsByAttributeId(int productAttributeId)
		{
			if (productAttributeId == 0)
				return new List<ProductAttributeOptionsSet>();

			var entities = _productAttributeOptionsSetRepository.Table
				.Where(x => x.ProductAttributeId == productAttributeId)
				.OrderBy(x => x.Name)
				.ToList();

			return entities;
		}

		public virtual void DeleteProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet)
		{
			Guard.NotNull(productAttributeOptionsSet, nameof(productAttributeOptionsSet));

			_productAttributeOptionsSetRepository.Delete(productAttributeOptionsSet);
		}

		public virtual void InsertProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet)
		{
			Guard.NotNull(productAttributeOptionsSet, nameof(productAttributeOptionsSet));

			_productAttributeOptionsSetRepository.Insert(productAttributeOptionsSet);
		}

		public virtual void UpdateProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet)
		{
			Guard.NotNull(productAttributeOptionsSet, nameof(productAttributeOptionsSet));

			_productAttributeOptionsSetRepository.Update(productAttributeOptionsSet);
		}

		#endregion

		#region Product variant attributes mappings (ProductVariantAttribute)

		public virtual void DeleteProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

            _productVariantAttributeRepository.Delete(productVariantAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

		public virtual IList<ProductVariantAttribute> GetProductVariantAttributesByProductId(int productId)
        {
			var query = from pva in _productVariantAttributeRepository.Table.Expand(x => x.ProductAttribute)
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

					if (ids.Count > 0)
					{
						var newLoadedMappings = GetSwitchedLoadedAttributeMappings(ids);
						result.AddRange(newLoadedMappings);
					}

					// sort by passed identifier sequence
					return result.OrderBySequence(productVariantAttributeIds).ToList();
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

			Array.Sort(productVariantAttributeValueIds);

			var key = PRODUCTVARIANTATTRIBUTEVALUES_BY_IDS_KEY.FormatInvariant(string.Join("-", productVariantAttributeValueIds));
			return _requestCache.Get(key, () =>
			{
				var validTypeIds = new[]
				{
					(int)AttributeControlType.DropdownList,
					(int)AttributeControlType.RadioList,
					(int)AttributeControlType.Checkboxes,
					(int)AttributeControlType.Boxes
				};

				var query = from x in _productVariantAttributeValueRepository.Table.Expand(y => y.ProductVariantAttribute.ProductAttribute)
						  let attr = x.ProductVariantAttribute
						  where productVariantAttributeValueIds.Contains(x.Id) && validTypeIds.Contains(attr.AttributeControlTypeId)
						  orderby x.ProductVariantAttribute.DisplayOrder, x.DisplayOrder
						  select x;

				return query.ToList();
			});
		}

        public virtual void InsertProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
			Guard.NotNull(productVariantAttribute, nameof(productVariantAttribute));

			_productVariantAttributeRepository.Insert(productVariantAttribute);

			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

        public virtual void UpdateProductVariantAttribute(ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                throw new ArgumentNullException("productVariantAttribute");

			_productVariantAttributeRepository.Update(productVariantAttribute);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

		public virtual int CopyAttributeOptions(ProductVariantAttribute productVariantAttribute, int productAttributeOptionsSetId, bool deleteExistingValues)
		{
			Guard.NotNull(productVariantAttribute, nameof(productVariantAttribute));
			Guard.NotZero(productVariantAttribute.Id, nameof(productVariantAttribute.Id));
			Guard.NotZero(productAttributeOptionsSetId, nameof(productAttributeOptionsSetId));

			if (deleteExistingValues)
			{
				var existingValues = productVariantAttribute.ProductVariantAttributeValues.ToList();
				if (!existingValues.Any())
					existingValues = GetProductVariantAttributeValues(productVariantAttribute.Id).ToList();

				existingValues.Each(x => DeleteProductVariantAttributeValue(x));
			}

			var result = 0;
			var attributeOptions = _productAttributeOptionRepository.TableUntracked
				.Where(x => x.ProductAttributeOptionsSetId == productAttributeOptionsSetId)
				.ToList();

			if (!attributeOptions.Any())
				return result;

			// Do not insert already existing values (identified by name field).
			var existingValueNames = new HashSet<string>(_productVariantAttributeValueRepository.TableUntracked
				.Where(x => x.ProductVariantAttributeId == productVariantAttribute.Id)
				.Select(x => x.Name)
				.ToList());

			Picture picture = null;
			ProductVariantAttributeValue productVariantAttributeValue = null;
			var pictureIds = attributeOptions.Where(x => x.PictureId != 0).Select(x => x.PictureId).Distinct().ToArray();
			var pictures = _pictureService.GetPicturesByIds(pictureIds, true).ToDictionarySafe(x => x.Id);

			using (_localizedEntityService.BeginScope())
			{
				foreach (var option in attributeOptions)
				{
					if (existingValueNames.Contains(option.Name))
						continue;

					productVariantAttributeValue = option.Clone();
					productVariantAttributeValue.PictureId = 0;
					productVariantAttributeValue.ProductVariantAttributeId = productVariantAttribute.Id;

					// Copy picture.
					if (option.PictureId != 0 && pictures.TryGetValue(option.PictureId, out picture))
					{
						var pictureBinary = _pictureService.LoadPictureBinary(picture);

						var newPicture = _pictureService.InsertPicture(
							pictureBinary,
							picture.MimeType,
							picture.SeoFilename,
							picture.IsNew,
							picture.Width ?? 0,
							picture.Height ?? 0,
							picture.IsTransient
						);

						productVariantAttributeValue.PictureId = newPicture.Id;
					}

					// No scope commit, we need new entity id.
					_productVariantAttributeValueRepository.Insert(productVariantAttributeValue);
					++result;

					// Copy localized properties too.
					var optionProperties = _localizedEntityService.GetLocalizedProperties(option.Id, "ProductAttributeOption");

					foreach (var property in optionProperties)
					{
						_localizedEntityService.InsertLocalizedProperty(new LocalizedProperty
						{
							EntityId = productVariantAttributeValue.Id,
							LocaleKeyGroup = "ProductVariantAttributeValue",
							LocaleKey = property.LocaleKey,
							LocaleValue = property.LocaleValue,
							LanguageId = property.LanguageId
						});
					}
				}
			}

			return result;
		}

		#endregion

		#region Product variant attribute values (ProductVariantAttributeValue)

		public virtual void DeleteProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

            _productVariantAttributeValueRepository.Delete(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
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

			_productVariantAttributeValueRepository.Insert(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
        }

        public virtual void UpdateProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue)
        {
            if (productVariantAttributeValue == null)
                throw new ArgumentNullException("productVariantAttributeValue");

			_productVariantAttributeValueRepository.Update(productVariantAttributeValue);

            _requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTES_PATTERN_KEY);
			_requestCache.RemoveByPattern(PRODUCTVARIANTATTRIBUTEVALUES_PATTERN_KEY);
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

			var combination = _pvacRepository.Table.FirstOrDefault(x => x.Sku == sku && x.Product.Deleted == false && !x.Product.IsSystemProduct);
			return combination;
		}

        public virtual void InsertProductVariantAttributeCombination(ProductVariantAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            _pvacRepository.Insert(combination);

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
        }

		public virtual void CreateAllProductVariantAttributeCombinations(Product product)
		{
			// Delete all existing combinations.
			_pvacRepository.DeleteAll(x => x.ProductId == product.Id);

			var attributes = GetProductVariantAttributesByProductId(product.Id);
			if (attributes == null || attributes.Count <= 0)
				return;

			var mappedAttributes = attributes
				.SelectMany(x => x.ProductVariantAttributeValues)
				.ToDictionarySafe(x => x.Id, x => x.ProductVariantAttribute);

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
						var attributesXml = "";

						foreach (var value in values)
						{
							attributesXml = mappedAttributes[value.Id].AddProductAttribute(attributesXml, value.Id.ToString());
						}

						combination = new ProductVariantAttributeCombination
						{
							ProductId = product.Id,
							AttributesXml = attributesXml,
							StockQuantity = 10000,
							AllowOutOfStockOrders = true,
							IsActive = true
						};

						_pvacRepository.Insert(combination);
					}

					scope.Commit();
				}
			}

			//foreach (var y in resultMatrix)
			//{
			//	var sb = new System.Text.StringBuilder();
			//	foreach (var x in y)
			//	{
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
			}
		}

		public virtual void UpdateProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter)
		{
			if (attributeFilter == null)
				throw new ArgumentNullException("attributeFilter");

			_productBundleItemAttributeFilterRepository.Update(attributeFilter);
		}

		public virtual void DeleteProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter)
		{
			if (attributeFilter == null)
				throw new ArgumentNullException("attributeFilter");

			_productBundleItemAttributeFilterRepository.Delete(attributeFilter);
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
