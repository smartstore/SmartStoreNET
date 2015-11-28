using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Services;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductsController : WebApiEntityController<Product, IProductService>
	{
		private readonly Lazy<IWorkContext> _workContext;
		private readonly Lazy<IPriceCalculationService> _priceCalculationService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IProductAttributeService> _productAttributeService;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IManufacturerService> _manufacturerService;

		public ProductsController(
			Lazy<IWorkContext> workContext,
			Lazy<IPriceCalculationService> priceCalculationService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IProductAttributeService> productAttributeService,
			Lazy<ICategoryService> categoryService,
			Lazy<IManufacturerService> manufacturerService)
		{
			_workContext = workContext;
			_priceCalculationService = priceCalculationService;
			_urlRecordService = urlRecordService;
			_productAttributeService = productAttributeService;
			_categoryService = categoryService;
			_manufacturerService = manufacturerService;
		}

		protected override IQueryable<Product> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}
		protected override void Insert(Product entity)
		{
			Service.InsertProduct(entity);
			
			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Product>(entity, x => x.Name);
				return null;
			});
		}
		protected override void Update(Product entity)
		{
			Service.UpdateProduct(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Product>(entity, x => x.Name);
				return null;
			});
		}
		protected override void Delete(Product entity)
		{
			Service.DeleteProduct(entity);
		}

		[WebApiQueryable]
		public SingleResult<Product> GetProduct(int key)
		{
			return GetSingleResult(key);
		}		

		// navigation properties

		public HttpResponseMessage NavigationProductCategories(int key, int relatedKey)
		{
			var productCategories = _categoryService.Value.GetProductCategoriesByProductId(key, true);
			var productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (productCategory == null)
				{
					productCategory = new ProductCategory { ProductId = key, CategoryId = relatedKey };

					_categoryService.Value.InsertProductCategory(productCategory);

					return Request.CreateResponse(HttpStatusCode.Created, productCategory);
				}
			}
			else if (Request.Method == HttpMethod.Delete)
			{
				if (productCategory != null)
					_categoryService.Value.DeleteProductCategory(productCategory);

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			return Request.CreateResponseForEntity(productCategory, relatedKey);
		}

		public HttpResponseMessage NavigationProductManufacturers(int key, int relatedKey)
		{
			var productManufacturers = _manufacturerService.Value.GetProductManufacturersByProductId(key, true);
			var productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (productManufacturer == null)
				{
					productManufacturer = new ProductManufacturer { ProductId = key, ManufacturerId = relatedKey };

					_manufacturerService.Value.InsertProductManufacturer(productManufacturer);

					return Request.CreateResponse(HttpStatusCode.Created, productManufacturer);
				}
			}
			else if (Request.Method == HttpMethod.Delete)
			{
				if (productManufacturer != null)
					_manufacturerService.Value.DeleteProductManufacturer(productManufacturer);

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			return Request.CreateResponseForEntity(productManufacturer, relatedKey);
		}

		public DeliveryTime GetDeliveryTime(int key)
		{
			return GetExpandedProperty<DeliveryTime>(key, x => x.DeliveryTime);
		}

		public QuantityUnit GetQuantityUnit(int key)
		{
			return GetExpandedProperty<QuantityUnit>(key, x => x.QuantityUnit);
		}

		public Download GetSampleDownload(int key)
		{
			return GetExpandedProperty<Download>(key, x => x.SampleDownload);
		}

		[WebApiQueryable]
		public IQueryable<ProductCategory> GetProductCategories(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductCategory>>(key, x => x.ProductCategories);

			return entity.ProductCategories.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductManufacturer> GetProductManufacturers(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductManufacturer>>(key, x => x.ProductManufacturers);

			return entity.ProductManufacturers.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductPicture> GetProductPictures(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductPicture>>(key, x => x.ProductPictures);

			return entity.ProductPictures.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductSpecificationAttribute> GetProductSpecificationAttributes(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductSpecificationAttribute>>(key, x => x.ProductSpecificationAttributes);

			return entity.ProductSpecificationAttributes.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductTag> GetProductTags(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductTag>>(key, x => x.ProductTags);

			return entity.ProductTags.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<TierPrice> GetTierPrices(int key)
		{
			var entity = GetExpandedEntity<ICollection<TierPrice>>(key, x => x.TierPrices);

			return entity.TierPrices.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<Discount> GetAppliedDiscounts(int key)
		{
			var entity = GetExpandedEntity<ICollection<Discount>>(key, x => x.AppliedDiscounts);

			return entity.AppliedDiscounts.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductVariantAttribute> GetProductVariantAttributes(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductVariantAttribute>>(key, x => x.ProductVariantAttributes);

			return entity.ProductVariantAttributes.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductVariantAttributeCombination>>(key, x => x.ProductVariantAttributeCombinations);

			return entity.ProductVariantAttributeCombinations.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ProductBundleItem> GetProductBundleItems(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductBundleItem>>(key, x => x.ProductBundleItems);

			return entity.ProductBundleItems.AsQueryable();
		}

		// actions

		private decimal? CalculatePrice(int key, bool lowestPrice)
		{
			string requiredProperties = "TierPrices, AppliedDiscounts, ProductBundleItems";
			var entity = GetExpandedEntity(key, requiredProperties);
			decimal? result = null;

			this.ProcessEntity(() =>
			{
				if (lowestPrice)
				{
					if (entity.ProductType == ProductType.GroupedProduct)
					{
						var searchContext = new ProductSearchContext()
						{
							Query = this.GetExpandedEntitySet(requiredProperties),
							ParentGroupedProductId = entity.Id,
							PageSize = int.MaxValue,
							VisibleIndividuallyOnly = false
						};

						Product lowestPriceProduct;
						var associatedProducts = Service.PrepareProductSearchQuery(searchContext);

						result = _priceCalculationService.Value.GetLowestPrice(entity, null, associatedProducts, out lowestPriceProduct);
					}
					else
					{
						bool displayFromMessage;
						result = _priceCalculationService.Value.GetLowestPrice(entity, null, out displayFromMessage);
					}
				}
				else
				{
					result = _priceCalculationService.Value.GetPreselectedPrice(entity, null);
				}
				return null;
			});
			return result;
		}

		[HttpPost]
		public decimal? FinalPrice(int key)
		{
			return CalculatePrice(key, false);
		}

		[HttpPost]
		public decimal? LowestPrice(int key)
		{
			return CalculatePrice(key, true);
		}

		[HttpPost, WebApiQueryable(PagingOptional = true)]
		public IQueryable<ProductVariantAttributeCombination> CreateAttributeCombinations(int key)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				_productAttributeService.Value.CreateAllProductVariantAttributeCombinations(entity);
				return null;
			});

			return entity.ProductVariantAttributeCombinations.AsQueryable();
		}

		[HttpPost, WebApiQueryable(PagingOptional = true)]
		public IQueryable<ProductVariantAttribute> ManageAttributes(int key, ODataActionParameters parameters)
		{
			var entity = GetExpandedEntity<ICollection<ProductVariantAttribute>>(key, x => x.ProductVariantAttributes);
			var result = new List<ProductVariantAttributeValue>();

			this.ProcessEntity(() =>
			{
				bool synchronize = parameters.GetValue<string, bool>("Synchronize");
				var data = (parameters["Attributes"] as IEnumerable<ManageAttributeType>).Where(x => x.Name.HasValue()).ToList();

				var allAttributes = _productAttributeService.Value.GetAllProductAttributes();

				foreach (var srcAttr in data)
				{
					var productAttribute = allAttributes.FirstOrDefault(x => x.Name.IsCaseInsensitiveEqual(srcAttr.Name));

					if (productAttribute == null)
					{
						productAttribute = new ProductAttribute() { Name = srcAttr.Name };
						_productAttributeService.Value.InsertProductAttribute(productAttribute);
					}

					var attribute = entity.ProductVariantAttributes.FirstOrDefault(x => x.ProductAttribute.Name.IsCaseInsensitiveEqual(srcAttr.Name));

					if (attribute == null)
					{
						attribute = new ProductVariantAttribute()
						{
							ProductId = entity.Id,
							ProductAttributeId = productAttribute.Id,
							AttributeControlTypeId = srcAttr.ControlTypeId,
							DisplayOrder = entity.ProductVariantAttributes.OrderByDescending(x => x.DisplayOrder).Select(x => x.DisplayOrder).FirstOrDefault() + 1,
							IsRequired = srcAttr.IsRequired
						};

						entity.ProductVariantAttributes.Add(attribute);
						Service.UpdateProduct(entity);
					}
					else if (synchronize)
					{
						if (srcAttr.Values.Count <= 0)
						{
							_productAttributeService.Value.DeleteProductVariantAttribute(attribute);
						}
						else
						{
							attribute.AttributeControlTypeId = srcAttr.ControlTypeId;
							attribute.IsRequired = srcAttr.IsRequired;

							Service.UpdateProduct(entity);
						}
					}

					int maxDisplayOrder = attribute.ProductVariantAttributeValues.OrderByDescending(x => x.DisplayOrder).Select(x => x.DisplayOrder).FirstOrDefault();

					foreach (var srcVal in srcAttr.Values.Where(x => x.Name.HasValue()))
					{
						var value = attribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Name.IsCaseInsensitiveEqual(srcVal.Name));

						if (value == null)
						{
							value = new ProductVariantAttributeValue()
							{
								ProductVariantAttributeId = attribute.Id,
								Name = srcVal.Name,
								Alias = srcVal.Alias,
								ColorSquaresRgb = srcVal.ColorSquaresRgb,
								PriceAdjustment = srcVal.PriceAdjustment,
								WeightAdjustment = srcVal.WeightAdjustment,
								IsPreSelected = srcVal.IsPreSelected,
								DisplayOrder = ++maxDisplayOrder
							};

							attribute.ProductVariantAttributeValues.Add(value);
							Service.UpdateProduct(entity);
						}
						else if (synchronize)
						{
							value.Alias = srcVal.Alias;
							value.ColorSquaresRgb = srcVal.ColorSquaresRgb;
							value.PriceAdjustment = srcVal.PriceAdjustment;
							value.WeightAdjustment = srcVal.WeightAdjustment;
							value.IsPreSelected = srcVal.IsPreSelected;

							Service.UpdateProduct(entity);
						}
					}

					if (synchronize)
					{
						foreach (var dstVal in attribute.ProductVariantAttributeValues.ToList())
						{
							if (!srcAttr.Values.Any(x => x.Name.IsCaseInsensitiveEqual(dstVal.Name)))
								_productAttributeService.Value.DeleteProductVariantAttributeValue(dstVal);
						}
					}
				}
				return null;
			});

			return entity.ProductVariantAttributes.AsQueryable();
		}
	}
}
