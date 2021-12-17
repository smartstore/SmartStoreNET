using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;
using SmartStore.WebApi.Services;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
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
                from x in Repository.Table
                where !x.Deleted && !x.IsSystemProduct
                select x;

            return query;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Create)]
        public IHttpActionResult Post(Product entity)
        {
            var result = Insert(entity, () =>
            {
                Service.InsertProduct(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Update)]
        public async Task<IHttpActionResult> Put(int key, Product entity)
        {
            var result = await UpdateAsync(entity, key, () =>
            {
                Service.UpdateProduct(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Product> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity =>
            {
                Service.UpdateProduct(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteProduct(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public IHttpActionResult GetDeliveryTime(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.DeliveryTime));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public IHttpActionResult GetQuantityUnit(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.QuantityUnit));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetCountryOfOrigin(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.CountryOfOrigin));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Download.Read)]
        public IHttpActionResult GetSampleDownload(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.SampleDownload));
        }


        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductCategories(int key, int relatedKey = 0 /*categoryId*/)
        {
            var productCategories = _categoryService.Value.GetProductCategoriesByProductId(key, true);

            if (relatedKey != 0)
            {
                var productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey);

                return Ok(productCategory);
            }

            return Ok(productCategories);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        public IHttpActionResult PostProductCategories(int key, int relatedKey /*categoryId*/)
        {
            var productCategories = _categoryService.Value.GetProductCategoriesByProductId(key, true);
            var productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey);

            if (productCategory == null)
            {
                productCategory = ReadContent<ProductCategory>() ?? new ProductCategory();
                productCategory.ProductId = key;
                productCategory.CategoryId = relatedKey;

                _categoryService.Value.InsertProductCategory(productCategory);

                return Created(productCategory);
            }

            return Ok(productCategory);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        public IHttpActionResult DeleteProductCategories(int key, int relatedKey = 0 /*categoryId*/)
        {
            var productCategories = _categoryService.Value.GetProductCategoriesByProductId(key, true);

            if (relatedKey == 0)
            {
                productCategories.Each(x => _categoryService.Value.DeleteProductCategory(x));
            }
            else
            {
                var productCategory = productCategories.FirstOrDefault(x => x.CategoryId == relatedKey);
                if (productCategory != null)
                {
                    _categoryService.Value.DeleteProductCategory(productCategory);
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductManufacturers(int key, int relatedKey = 0 /*manufacturerId*/)
        {
            var productManufacturers = _manufacturerService.Value.GetProductManufacturersByProductId(key, true);

            if (relatedKey != 0)
            {
                var productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey);

                return Ok(productManufacturer);
            }

            return Ok(productManufacturers);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public IHttpActionResult PostProductManufacturers(int key, int relatedKey /*manufacturerId*/)
        {
            var productManufacturers = _manufacturerService.Value.GetProductManufacturersByProductId(key, true);
            var productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey);

            if (productManufacturer == null)
            {
                productManufacturer = ReadContent<ProductManufacturer>() ?? new ProductManufacturer();
                productManufacturer.ProductId = key;
                productManufacturer.ManufacturerId = relatedKey;

                _manufacturerService.Value.InsertProductManufacturer(productManufacturer);

                return Created(productManufacturer);
            }

            return Ok(productManufacturer);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public IHttpActionResult DeleteProductManufacturers(int key, int relatedKey = 0 /*manufacturerId*/)
        {
            var productManufacturers = _manufacturerService.Value.GetProductManufacturersByProductId(key, true);

            if (relatedKey == 0)
            {
                productManufacturers.Each(x => _manufacturerService.Value.DeleteProductManufacturer(x));
            }
            else
            {
                var productManufacturer = productManufacturers.FirstOrDefault(x => x.ManufacturerId == relatedKey);
                if (productManufacturer != null)
                {
                    _manufacturerService.Value.DeleteProductManufacturer(productManufacturer);
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductPictures(int key, int relatedKey = 0 /*mediaFileId*/)
        {
            var productPictures = Service.GetProductPicturesByProductId(key);

            if (relatedKey != 0)
            {
                var productPicture = productPictures.FirstOrDefault(x => x.MediaFileId == relatedKey);

                return Ok(productPicture);
            }

            return Ok(productPictures);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        public IHttpActionResult PostProductPictures(int key, int relatedKey /*mediaFileId*/)
        {
            var productPictures = Service.GetProductPicturesByProductId(key);
            var productPicture = productPictures.FirstOrDefault(x => x.MediaFileId == relatedKey);

            if (productPicture == null)
            {
                productPicture = ReadContent<ProductMediaFile>() ?? new ProductMediaFile();
                productPicture.ProductId = key;
                productPicture.MediaFileId = relatedKey;

                Service.InsertProductPicture(productPicture);

                return Created(productPicture);
            }

            return Ok(productPicture);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        public IHttpActionResult DeleteProductPictures(int key, int relatedKey = 0 /*mediaFileId*/)
        {
            var productPictures = Service.GetProductPicturesByProductId(key);

            if (relatedKey == 0)
            {
                productPictures.Each(x => Service.DeleteProductPicture(x));
            }
            else
            {
                var productPicture = productPictures.FirstOrDefault(x => x.MediaFileId == relatedKey);
                if (productPicture != null)
                {
                    Service.DeleteProductPicture(productPicture);
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductSpecificationAttributes(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductSpecificationAttributes));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductTags(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductTags));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetTierPrices(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.TierPrices));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetAppliedDiscounts(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.AppliedDiscounts));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductVariantAttributes(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductVariantAttributes));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductVariantAttributeCombinations(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductVariantAttributeCombinations));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProductBundleItems(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductBundleItems));
        }

        #endregion

        #region Actions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.EntityType<Product>();

            entityConfig.Ignore(x => x.MergedDataValues);

            entityConfig.Collection
                .Action("Search")
                .ReturnsCollectionFromEntitySet<Product>("Products");

            entityConfig
                .Action("FinalPrice")
                .Returns<decimal>();

            entityConfig
                .Action("LowestPrice")
                .Returns<decimal>();

            entityConfig
                .Action("CreateAttributeCombinations")
                .ReturnsCollectionFromEntitySet<ProductVariantAttributeCombination>("ProductVariantAttributeCombinations");

            var manageAttributes = entityConfig
                .Action("ManageAttributes")
                .ReturnsCollectionFromEntitySet<ProductVariantAttribute>("ProductVariantAttributes");
            manageAttributes.Parameter<bool>("Synchronize");
            manageAttributes.CollectionParameter<ManageAttributeType>("Attributes");
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult Search([ModelBinder(typeof(WebApiCatalogSearchQueryModelBinder))] CatalogSearchQuery query)
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

            return Ok(result.Hits.AsQueryable());
        }

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
                        var searchQuery = new CatalogSearchQuery()
                            .VisibleOnly()
                            .HasParentGroupedProduct(entity.Id);

                        var query = _catalogSearchService.Value.PrepareQuery(searchQuery, GetExpandedEntitySet(requiredProperties));
                        var associatedProducts = query.OrderBy(x => x.DisplayOrder).ToList();

                        result = _priceCalculationService.Value.GetLowestPrice(entity, customer, null, associatedProducts, out var _);
                    }
                    else
                    {
                        result = _priceCalculationService.Value.GetLowestPrice(entity, customer, null, out var _);
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
        public IHttpActionResult FinalPrice(int key)
        {
            var price = CalculatePrice(key, false);
            return Ok(price);
        }

        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult LowestPrice(int key)
        {
            var price = CalculatePrice(key, true);
            return Ok(price);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public IHttpActionResult CreateAttributeCombinations(int key)
        {
            var entity = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                _productAttributeService.Value.CreateAllProductVariantAttributeCombinations(entity);
            });

            return Ok(entity.ProductVariantAttributeCombinations.AsQueryable());
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public IHttpActionResult ManageAttributes(int key, ODataActionParameters parameters)
        {
            var entity = GetExpandedEntity(key, x => x.ProductVariantAttributes.Select(y => y.ProductAttribute));
            var result = new List<ProductVariantAttributeValue>();

            this.ProcessEntity(() =>
            {
                var synchronize = parameters.GetValueSafe<bool>("Synchronize");
                var data = (parameters["Attributes"] as IEnumerable<ManageAttributeType>)
                    .Where(x => x.Name.HasValue())
                    .ToList();

                var attributeNames = new HashSet<string>(data.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
                var pagedAttributes = _productAttributeService.Value.GetAllProductAttributes(0, 1);
                var attributesData = pagedAttributes.SourceQuery
                    .Where(x => attributeNames.Contains(x.Name))
                    .Select(x => new { x.Id, x.Name })
                    .ToList();
                var allAttributesDic = attributesData.ToDictionarySafe(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var srcAttr in data)
                {
                    // Product attribute.
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

                    // Product attribute mapping.
                    var productAttribute = entity.ProductVariantAttributes.FirstOrDefault(x => x.ProductAttribute?.Name.IsCaseInsensitiveEqual(srcAttr.Name) ?? false);
                    if (productAttribute == null)
                    {
                        // No mapping to attribute yet.
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
                        // Has already an attribute mapping.
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

                    // Values.
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

            return Ok(entity.ProductVariantAttributes.AsQueryable());
        }

        #endregion
    }
}
