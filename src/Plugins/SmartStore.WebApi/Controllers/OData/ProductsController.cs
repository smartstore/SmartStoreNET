using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Services;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductsController : WebApiEntityController<Product, IProductService>
	{
		private readonly Lazy<IPriceCalculationService> _priceCalculationService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IProductAttributeService> _productAttributeService;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICatalogSearchService> _catalogSearchService;
		private readonly Lazy<SearchSettings> _searchSettings;

		public ProductsController(
			Lazy<IPriceCalculationService> priceCalculationService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IProductAttributeService> productAttributeService,
			Lazy<ICategoryService> categoryService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICatalogSearchService> catalogSearchService,
			Lazy<SearchSettings> searchSettings)
		{
			_priceCalculationService = priceCalculationService;
			_urlRecordService = urlRecordService;
			_productAttributeService = productAttributeService;
			_categoryService = categoryService;
			_manufacturerService = manufacturerService;
			_catalogSearchService = catalogSearchService;
			_searchSettings = searchSettings;
		}

		protected override IQueryable<Product> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted && !x.IsSystemProduct
				select x;

			return query;
		}
		
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Create)]
        protected override void Insert(Product entity)
		{
			Service.InsertProduct(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Product>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Update)]
        protected override void Update(Product entity)
		{
			Service.UpdateProduct(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Product>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Delete)]
        protected override void Delete(Product entity)
		{
			Service.DeleteProduct(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
		{
			return GetSingleResult(key);
		}

        // Navigation properties.

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        public HttpResponseMessage NavigationProductCategories(int key, int relatedKey)
		{
			ProductCategory productCategory = null;
			var productCategories = _categoryService.Value.GetProductCategoriesByProductId(key, true);

			if (Request.Method == HttpMethod.Delete)
			{
				if (relatedKey == 0)
				{
					productCategories.Each(x => _categoryService.Value.DeleteProductCategory(x));
				}
				else if ((productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey)) != null)
				{
					_categoryService.Value.DeleteProductCategory(productCategory);
				}

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (productCategory == null)
				{
					productCategory = ReadContent<ProductCategory>() ?? new ProductCategory();
					productCategory.ProductId = key;
					productCategory.CategoryId = relatedKey;

					_categoryService.Value.InsertProductCategory(productCategory);

					return Request.CreateResponse(HttpStatusCode.Created, productCategory);
				}
			}

			return Request.CreateResponseForEntity(productCategory, relatedKey);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public HttpResponseMessage NavigationProductManufacturers(int key, int relatedKey)
		{
			ProductManufacturer productManufacturer = null;
			var productManufacturers = _manufacturerService.Value.GetProductManufacturersByProductId(key, true);

			if (Request.Method == HttpMethod.Delete)
			{
				if (relatedKey == 0)
				{
					productManufacturers.Each(x => _manufacturerService.Value.DeleteProductManufacturer(x));
				}
				else if ((productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey)) != null)
				{
					_manufacturerService.Value.DeleteProductManufacturer(productManufacturer);
				}

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (productManufacturer == null)
				{
					productManufacturer = ReadContent<ProductManufacturer>() ?? new ProductManufacturer();
					productManufacturer.ProductId = key;
					productManufacturer.ManufacturerId = relatedKey;

					_manufacturerService.Value.InsertProductManufacturer(productManufacturer);

					return Request.CreateResponse(HttpStatusCode.Created, productManufacturer);
				}
			}

			return Request.CreateResponseForEntity(productManufacturer, relatedKey);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        public HttpResponseMessage NavigationProductPictures(int key, int relatedKey)
        {
            ProductMediaFile productPicture = null;
            var productPictures = Service.GetProductPicturesByProductId(key);

            if (Request.Method == HttpMethod.Delete)
            {
                if (relatedKey == 0)
                {
                    productPictures.Each(x => Service.DeleteProductPicture(x));
                }
                else if ((productPicture = productPictures.FirstOrDefault(x => x.MediaFileId == relatedKey)) != null)
                {
                    Service.DeleteProductPicture(productPicture);
                }

                return Request.CreateResponse(HttpStatusCode.NoContent);
            }

            productPicture = productPictures.FirstOrDefault(x => x.MediaFileId == relatedKey);

            if (Request.Method == HttpMethod.Post)
            {
                if (productPicture == null)
                {
                    productPicture = ReadContent<ProductMediaFile>() ?? new ProductMediaFile();
                    productPicture.ProductId = key;
                    productPicture.MediaFileId = relatedKey;

                    Service.InsertProductPicture(productPicture);

                    return Request.CreateResponse(HttpStatusCode.Created, productPicture);
                }
            }

            return Request.CreateResponseForEntity(productPicture, relatedKey);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetRelatedEntity(key, x => x.DeliveryTime);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public SingleResult<QuantityUnit> GetQuantityUnit(int key)
		{
			return GetRelatedEntity(key, x => x.QuantityUnit);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Country> GetCountryOfOrigin(int key)
		{
			return GetRelatedEntity(key, x => x.CountryOfOrigin);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Download.Read)]
        public SingleResult<Download> GetSampleDownload(int key)
		{
			return GetRelatedEntity(key, x => x.SampleDownload);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductCategory> GetProductCategories(int key)
		{
			return GetRelatedCollection(key, x => x.ProductCategories);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductManufacturer> GetProductManufacturers(int key)
		{
			return GetRelatedCollection(key, x => x.ProductManufacturers);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductMediaFile> GetProductPictures(int key)
		{
			return GetRelatedCollection(key, x => x.ProductPictures);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductSpecificationAttribute> GetProductSpecificationAttributes(int key)
		{
			return GetRelatedCollection(key, x => x.ProductSpecificationAttributes);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductTag> GetProductTags(int key)
		{
			return GetRelatedCollection(key, x => x.ProductTags);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<TierPrice> GetTierPrices(int key)
		{
			return GetRelatedCollection(key, x => x.TierPrices);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<Discount> GetAppliedDiscounts(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedDiscounts);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductVariantAttribute> GetProductVariantAttributes(int key)
		{
			return GetRelatedCollection(key, x => x.ProductVariantAttributes);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int key)
		{
			return GetRelatedCollection(key, x => x.ProductVariantAttributeCombinations);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductBundleItem> GetProductBundleItems(int key)
		{
			return GetRelatedCollection(key, x => x.ProductBundleItems);
		}

		// Actions.

		[HttpPost, WebApiQueryable(PagingOptional = true)]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<Product> Search([ModelBinder(typeof(WebApiCatalogSearchQueryModelBinder))] CatalogSearchQuery query)
		{
			CatalogSearchResult result = null;

			this.ProcessEntity(() =>
			{
				if (query.Term == null || query.Term.Length < _searchSettings.Value.InstantSearchTermMinLength)
				{
					throw new SmartException($"The minimum length for the search term is {_searchSettings.Value.InstantSearchTermMinLength} characters.");
				}

				result = _catalogSearchService.Value.Search(query);
			});

			return result.Hits.AsQueryable();
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        private decimal? CalculatePrice(int key, bool lowestPrice)
		{
			string requiredProperties = "TierPrices, AppliedDiscounts, ProductBundleItems";
			var entity = GetExpandedEntity(key, requiredProperties);
			var customer = Services.WorkContext.CurrentCustomer;
			decimal? result = null;

			this.ProcessEntity(() =>
			{
				if (lowestPrice)
				{
					if (entity.ProductType == ProductType.GroupedProduct)
					{
						Product lowestPriceProduct;
						var searchQuery = new CatalogSearchQuery()
							.VisibleOnly()
							.HasParentGroupedProduct(entity.Id);

						var query = _catalogSearchService.Value.PrepareQuery(searchQuery, GetExpandedEntitySet(requiredProperties));
						var associatedProducts = query.OrderBy(x => x.DisplayOrder).ToList();

						result = _priceCalculationService.Value.GetLowestPrice(entity, customer, null, associatedProducts, out lowestPriceProduct);
					}
					else
					{
						bool displayFromMessage;
						result = _priceCalculationService.Value.GetLowestPrice(entity, customer, null, out displayFromMessage);
					}
				}
				else
				{
					result = _priceCalculationService.Value.GetPreselectedPrice(entity, customer, Services.WorkContext.WorkingCurrency, null);
				}
			});

			return result;
		}

		[HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public decimal? FinalPrice(int key)
		{
			return CalculatePrice(key, false);
		}

		[HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public decimal? LowestPrice(int key)
		{
			return CalculatePrice(key, true);
		}

		[HttpPost, WebApiQueryable(PagingOptional = true)]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public IQueryable<ProductVariantAttributeCombination> CreateAttributeCombinations(int key)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				_productAttributeService.Value.CreateAllProductVariantAttributeCombinations(entity);
			});

			return entity.ProductVariantAttributeCombinations.AsQueryable();
		}

		[HttpPost, WebApiQueryable(PagingOptional = true)]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public IQueryable<ProductVariantAttribute> ManageAttributes(int key, ODataActionParameters parameters)
		{
			var entity = GetExpandedEntity(key, x => x.ProductVariantAttributes);
			var result = new List<ProductVariantAttributeValue>();

			this.ProcessEntity(() =>
			{
				var synchronize = parameters.GetValueSafe<bool>("Synchronize");
				var data = (parameters["Attributes"] as IEnumerable<ManageAttributeType>)
                    .Where(x => x.Name.HasValue())
                    .ToList();

                var attributeNames = new HashSet<string>(data.Select(x => x.Name), StringComparer.InvariantCultureIgnoreCase);
                var pagedAttributes = _productAttributeService.Value.GetAllProductAttributes(0, 1);
                var attributesData = pagedAttributes.SourceQuery
                    .Where(x => attributeNames.Contains(x.Name))
                    .Select(x => new { x.Id, x.Name })
                    .ToList();
                var allAttributesDic = attributesData.ToDictionarySafe(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var srcAttr in data)
				{
                    var attributeId = 0;
                    if (allAttributesDic.TryGetValue(srcAttr.Name, out var attributeData))
                    {
                        attributeId = attributeData.Id;
                    }
                    else
                    {
                        var attribute = new ProductAttribute { Name = srcAttr.Name };
                        _productAttributeService.Value.InsertProductAttribute(attribute);
                        attributeId = attribute.Id;
                    }

                    var productAttribute = entity.ProductVariantAttributes.FirstOrDefault(x => x.ProductAttribute.Name.IsCaseInsensitiveEqual(srcAttr.Name));
					if (productAttribute == null)
					{
						productAttribute = new ProductVariantAttribute
						{
							ProductId = entity.Id,
							ProductAttributeId = attributeId,
							AttributeControlTypeId = srcAttr.ControlTypeId,
							DisplayOrder = entity.ProductVariantAttributes.OrderByDescending(x => x.DisplayOrder).Select(x => x.DisplayOrder).FirstOrDefault() + 1,
							IsRequired = srcAttr.IsRequired
						};

						entity.ProductVariantAttributes.Add(productAttribute);
						Service.UpdateProduct(entity);
					}
					else if (synchronize)
					{
						if (srcAttr.Values.Count <= 0 && productAttribute.ShouldHaveValues())
						{
							_productAttributeService.Value.DeleteProductVariantAttribute(productAttribute);
						}
						else
						{
							productAttribute.AttributeControlTypeId = srcAttr.ControlTypeId;
							productAttribute.IsRequired = srcAttr.IsRequired;

							Service.UpdateProduct(entity);
						}
					}

					var maxDisplayOrder = productAttribute.ProductVariantAttributeValues
                        .OrderByDescending(x => x.DisplayOrder)
                        .Select(x => x.DisplayOrder)
                        .FirstOrDefault();

					foreach (var srcVal in srcAttr.Values.Where(x => x.Name.HasValue()))
					{
						var value = productAttribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Name.IsCaseInsensitiveEqual(srcVal.Name));
						if (value == null)
						{
							value = new ProductVariantAttributeValue
							{
								ProductVariantAttributeId = productAttribute.Id,
								Name = srcVal.Name,
								Alias = srcVal.Alias,
								Color = srcVal.Color,
								PriceAdjustment = srcVal.PriceAdjustment,
								WeightAdjustment = srcVal.WeightAdjustment,
								IsPreSelected = srcVal.IsPreSelected,
								DisplayOrder = ++maxDisplayOrder
							};

							productAttribute.ProductVariantAttributeValues.Add(value);
							Service.UpdateProduct(entity);
						}
						else if (synchronize)
						{
							value.Alias = srcVal.Alias;
							value.Color = srcVal.Color;
							value.PriceAdjustment = srcVal.PriceAdjustment;
							value.WeightAdjustment = srcVal.WeightAdjustment;
							value.IsPreSelected = srcVal.IsPreSelected;

							Service.UpdateProduct(entity);
						}
					}

					if (synchronize)
					{
						foreach (var dstVal in productAttribute.ProductVariantAttributeValues.ToList())
						{
                            if (!srcAttr.Values.Any(x => x.Name.IsCaseInsensitiveEqual(dstVal.Name)))
                            {
                                _productAttributeService.Value.DeleteProductVariantAttributeValue(dstVal);
                            }
						}
					}
				}
			});

			return entity.ProductVariantAttributes.AsQueryable();
		}
	}
}
