using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Models.Entity;

namespace SmartStore.Web.Controllers
{
    public partial class EntityController : PublicControllerBase
    {
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly SearchSettings _searchSettings;
        private readonly IMediaService _mediaService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;

        public EntityController(
            ICatalogSearchService catalogSearchService,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            SearchSettings searchSettings,
            IMediaService mediaService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            ICategoryService categoryService)
        {
            _catalogSearchService = catalogSearchService;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
            _searchSettings = searchSettings;
            _mediaService = mediaService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _categoryService = categoryService;
        }

        #region Entity Picker

        public ActionResult Picker(EntityPickerModel model)
        {
            if (model.EntityType.IsCaseInsensitiveEqual("product"))
            {
                ViewBag.AvailableCategories = _categoryService.GetCategoryTree(includeHidden: true)
                    .FlattenNodes(false)
                    .Select(x => new SelectListItem { Text = x.GetCategoryNameIndented(), Value = x.Id.ToString() })
                    .ToList();

                ViewBag.AvailableManufacturers = _manufacturerService.GetAllManufacturers(true)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                    .ToList();

                ViewBag.AvailableStores = Services.StoreService.GetAllStores()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                    .ToList();

                ViewBag.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            }
            else if (model.EntityType.IsCaseInsensitiveEqual("customer"))
            {
                ViewBag.AvailableCustomerSearchTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Name", Value = "Name", Selected = true },
                    new SelectListItem { Text = "Email", Value = "Email" }
                };

                if (Services.Settings.GetSettingByKey<CustomerNumberMethod>("CustomerSettings.CustomerNumberMethod") != CustomerNumberMethod.Disabled)
                {
                    ViewBag.AvailableCustomerSearchTypes.Add(new SelectListItem { Text = T("Account.Fields.CustomerNumber"), Value = "CustomerNumber" });
                }
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Picker(EntityPickerModel model, FormCollection form)
        {
            try
            {
                var languageId = model.LanguageId == 0 ? Services.WorkContext.WorkingLanguage.Id : model.LanguageId;
                var disableIf = model.DisableIf.SplitSafe(",").Select(x => x.ToLower().Trim()).ToList();
                var disableIds = model.DisableIds.SplitSafe(",").Select(x => x.ToInt()).ToList();
                var selected = model.Selected.SplitSafe(",");
                var returnSku = model.ReturnField.IsCaseInsensitiveEqual("sku");

                using (var scope = new DbContextScope(Services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
                {
                    if (model.EntityType.IsCaseInsensitiveEqual("product"))
                    {
                        model.SearchTerm = model.SearchTerm.TrimSafe();

                        var hasPermission = Services.Permissions.Authorize(Permissions.Catalog.Product.Read);
                        var disableIfNotSimpleProduct = disableIf.Contains("notsimpleproduct");
                        var disableIfGroupedProduct = disableIf.Contains("groupedproduct");
                        var labelTextGrouped = T("Admin.Catalog.Products.ProductType.GroupedProduct.Label").Text;
                        var labelTextBundled = T("Admin.Catalog.Products.ProductType.BundledProduct.Label").Text;
                        var sku = T("Products.Sku").Text;

                        var fields = new List<string> { "name" };
                        if (_searchSettings.SearchFields.Contains("sku"))
                            fields.Add("sku");
                        if (_searchSettings.SearchFields.Contains("shortdescription"))
                            fields.Add("shortdescription");

                        var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.SearchTerm)
                            .HasStoreId(model.StoreId);

                        if (!hasPermission)
                        {
                            searchQuery = searchQuery.VisibleOnly(Services.WorkContext.CurrentCustomer);
                        }

                        if (model.ProductTypeId > 0)
                        {
                            searchQuery = searchQuery.IsProductType((ProductType)model.ProductTypeId);
                        }

                        if (model.ManufacturerId != 0)
                        {
                            searchQuery = searchQuery.WithManufacturerIds(null, model.ManufacturerId);
                        }

                        if (model.CategoryId != 0)
                        {
                            var node = _categoryService.GetCategoryTree(model.CategoryId, true);
                            if (node != null)
                            {
                                searchQuery = searchQuery.WithCategoryIds(null, node.Flatten(true).Select(x => x.Id).ToArray());
                            }
                        }

                        List<EntityPickerProduct> products;
                        var skip = model.PageIndex * model.PageSize;

                        if (_searchSettings.UseCatalogSearchInBackend)
                        {
                            searchQuery = searchQuery
                                .Slice(skip, model.PageSize)
                                .SortBy(ProductSortingEnum.NameAsc);

                            var searchResult = _catalogSearchService.Search(searchQuery);
                            products = searchResult.Hits
                                .Select(x => new EntityPickerProduct
                                {
                                    Id = x.Id,
                                    Sku = x.Sku,
                                    Name = x.Name,
                                    Published = x.Published,
                                    ProductTypeId = x.ProductTypeId,
                                    MainPictureId = x.MainPictureId
                                })
                                .ToList();
                        }
                        else
                        {
                            var query = _catalogSearchService.PrepareQuery(searchQuery);

                            products = query
                                .Select(x => new EntityPickerProduct
                                {
                                    Id = x.Id,
                                    Sku = x.Sku,
                                    Name = x.Name,
                                    Published = x.Published,
                                    ProductTypeId = x.ProductTypeId,
                                    MainPictureId = x.MainPictureId
                                })
                                .OrderBy(x => x.Name)
                                .Skip(() => skip)
                                .Take(() => model.PageSize)
                                .ToList();
                        }

                        var fileIds = products
                            .Select(x => x.MainPictureId ?? 0)
                            .Where(x => x != 0)
                            .Distinct()
                            .ToArray();

                        var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

                        model.SearchResult = products
                            .Select(x =>
                            {
                                var item = new EntityPickerModel.SearchResultModel
                                {
                                    Id = x.Id,
                                    Title = x.Name,
                                    Summary = x.Sku,
                                    SummaryTitle = "{0}: {1}".FormatInvariant(sku, x.Sku.NaIfEmpty()),
                                    Published = hasPermission ? x.Published : (bool?)null,
                                    ReturnValue = returnSku ? x.Sku : x.Id.ToString()
                                };

                                item.Selected = selected.Contains(item.ReturnValue);

                                if (disableIfNotSimpleProduct)
                                {
                                    item.Disable = x.ProductTypeId != (int)ProductType.SimpleProduct;
                                }
                                else if (disableIfGroupedProduct)
                                {
                                    item.Disable = x.ProductTypeId == (int)ProductType.GroupedProduct;
                                }

                                if (!item.Disable && disableIds.Contains(x.Id))
                                {
                                    item.Disable = true;
                                }

                                if (x.ProductTypeId == (int)ProductType.GroupedProduct)
                                {
                                    item.LabelText = labelTextGrouped;
                                    item.LabelClassName = "badge-success";
                                }
                                else if (x.ProductTypeId == (int)ProductType.BundledProduct)
                                {
                                    item.LabelText = labelTextBundled;
                                    item.LabelClassName = "badge-info";
                                }

                                files.TryGetValue(x.MainPictureId ?? 0, out var file);
                                item.ImageUrl = _mediaService.GetUrl(file, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, null, !_catalogSettings.HideProductDefaultPictures);

                                return item;
                            })
                            .ToList();
                    }
                    else if (model.EntityType.IsCaseInsensitiveEqual("category"))
                    {
                        var categories = _categoryService.GetAllCategories(model.SearchTerm, showHidden: true);

                        var fileIds = categories
                            .Select(x => x.MediaFileId ?? 0)
                            .Where(x => x != 0)
                            .Distinct()
                            .ToArray();

                        var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

                        model.SearchResult = categories
                            .Select(x =>
                            {
                                var path = ((ICategoryNode)x).GetCategoryPath(_categoryService, languageId, "({0})");
                                var item = new EntityPickerModel.SearchResultModel
                                {
                                    Id = x.Id,
                                    Title = x.Name,
                                    Summary = path,
                                    SummaryTitle = path,
                                    Published = x.Published,
                                    ReturnValue = x.Id.ToString(),
                                    Selected = selected.Contains(x.Id.ToString()),
                                    Disable = disableIds.Contains(x.Id)
                                };

                                if (x.Alias.HasValue())
                                {
                                    item.LabelText = x.Alias;
                                    item.LabelClassName = "badge-secondary";
                                }

                                files.TryGetValue(x.MediaFileId ?? 0, out var file);
                                item.ImageUrl = _mediaService.GetUrl(file, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, null, !_catalogSettings.HideProductDefaultPictures);

                                return item;
                            })
                            .ToList();
                    }
                    else if (model.EntityType.IsCaseInsensitiveEqual("manufacturer"))
                    {
                        var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchTerm, model.PageIndex, model.PageSize, showHidden: true);

                        var fileIds = manufacturers
                            .Select(x => x.MediaFileId ?? 0)
                            .Where(x => x != 0)
                            .Distinct()
                            .ToArray();

                        var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

                        model.SearchResult = manufacturers
                            .Select(x =>
                            {
                                var item = new EntityPickerModel.SearchResultModel
                                {
                                    Id = x.Id,
                                    Title = x.Name,
                                    Published = x.Published,
                                    ReturnValue = x.Id.ToString(),
                                    Selected = selected.Contains(x.Id.ToString()),
                                    Disable = disableIds.Contains(x.Id)
                                };

                                files.TryGetValue(x.MediaFileId ?? 0, out var file);
                                item.ImageUrl = _mediaService.GetUrl(file, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, null, !_catalogSettings.HideProductDefaultPictures);

                                return item;
                            })
                            .ToList();
                    }
                    else if (model.EntityType.IsCaseInsensitiveEqual("customer"))
                    {
                        var registeredRoleId = _customerService.GetCustomerRoleBySystemName("Registered").Id;
                        var searchTermName = string.Empty;
                        var searchTermEmail = string.Empty;
                        var searchTermCustomerNumber = string.Empty;

                        if (model.CustomerSearchType.IsCaseInsensitiveEqual("Name"))
                            searchTermName = model.SearchTerm;
                        else if (model.CustomerSearchType.IsCaseInsensitiveEqual("Email"))
                            searchTermEmail = model.SearchTerm;
                        else if (model.CustomerSearchType.IsCaseInsensitiveEqual("CustomerNumber"))
                            searchTermCustomerNumber = model.SearchTerm;

                        var q = new CustomerSearchQuery
                        {
                            SearchTerm = searchTermName,
                            Email = searchTermEmail,
                            CustomerNumber = searchTermCustomerNumber,
                            CustomerRoleIds = new int[] { registeredRoleId },
                            PageIndex = model.PageIndex,
                            PageSize = model.PageSize
                        };

                        var customers = _customerService.SearchCustomers(q);

                        model.SearchResult = customers
                            .Select(x =>
                            {
                                var fullName = x.GetFullName();

                                var item = new EntityPickerModel.SearchResultModel
                                {
                                    Id = x.Id,
                                    ReturnValue = x.Id.ToString(),
                                    Title = x.Username.NullEmpty() ?? x.Email,
                                    Summary = fullName,
                                    SummaryTitle = fullName,
                                    Published = true,
                                    Selected = selected.Contains(x.Id.ToString()),
                                    Disable = disableIds.Contains(x.Id)
                                };

                                return item;
                            })
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex.ToAllMessages());
            }

            return PartialView("Picker.List", model);
        }

        #endregion
    }
}
