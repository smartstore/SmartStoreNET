using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Search;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Models.Entity;

namespace SmartStore.Web.Controllers
{
	public partial class EntityController : PublicControllerBase
    {
		private readonly ICommonServices _services;
		private readonly ICatalogSearchService _catalogSearchService;
        private readonly CatalogSettings _catalogSettings;
		private readonly MediaSettings _mediaSettings;
		private readonly SearchSettings _searchSettings;
		private readonly IPictureService _pictureService;
		private readonly IManufacturerService _manufacturerService;
		private readonly ICategoryService _categoryService;
		private readonly IProductService _productService;
		private readonly CatalogHelper _catalogHelper;

		public EntityController(
			ICommonServices services,
			ICatalogSearchService catalogSearchService,
			CatalogSettings catalogSettings,
			MediaSettings mediaSettings,
			SearchSettings searchSettings,
			IPictureService pictureService,
			IManufacturerService manufacturerService,
			ICategoryService categoryService,
			IProductService productService,
			CatalogHelper catalogHelper)
        {
			_services = services;
			_catalogSearchService = catalogSearchService;
            _catalogSettings = catalogSettings;
			_mediaSettings = mediaSettings;
			_searchSettings = searchSettings;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productService = productService;
			_catalogHelper = catalogHelper;
        }

		#region Entity Picker

		public ActionResult Picker(EntityPickerModel model)
		{
            model.PageSize = 96; // _commonSettings.EntityPickerPageSize;
			model.AllString = T("Admin.Common.All");

			if (model.Entity.IsCaseInsensitiveEqual("product"))
			{
				model.AvailableCategories = _categoryService.GetCategoryTree(includeHidden: true)
					.FlattenNodes(false)
					.Select(x => new SelectListItem { Text = x.GetCategoryNameIndented(), Value = x.Id.ToString() })
					.ToList();

				model.AvailableManufacturers = _manufacturerService.GetAllManufacturers(true)
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.AvailableStores = _services.StoreService.GetAllStores()
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
			}

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult Picker(EntityPickerModel model, FormCollection form)
		{
            model.PageSize = 96; // _commonSettings.EntityPickerPageSize;
			model.PublishedString = T("Common.Published");
			model.UnpublishedString = T("Common.Unpublished");

			try
			{
				var disableIf = model.DisableIf.SplitSafe(",").Select(x => x.ToLower().Trim()).ToList();
				var disableIds = model.DisableIds.SplitSafe(",").Select(x => x.ToInt()).ToList();

				using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
				{
					if (model.Entity.IsCaseInsensitiveEqual("product"))
					{
						#region Product

						model.SearchTerm = model.ProductName.TrimSafe();

						var hasPermission = _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog);
						var storeLocation = _services.WebHelper.GetStoreLocation(false);
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
							searchQuery = searchQuery.VisibleOnly(_services.WorkContext.CurrentCustomer);
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

						var query = _catalogSearchService.PrepareQuery(searchQuery);

						var products = query
							.Select(x => new
							{
								x.Id,
								x.Sku,
								x.Name,
								x.Published,
								x.ProductTypeId,
								x.MainPictureId
							})
							.OrderBy(x => x.Name)
							.Skip(model.PageIndex * model.PageSize)
							.Take(model.PageSize)
							.ToList();

						var productIds = products.Select(x => x.Id).ToArray();
						var allPictureIds = products.Select(x => x.MainPictureId.GetValueOrDefault());
						var allPictureInfos = _pictureService.GetPictureInfos(allPictureIds,
							_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage,
							!_catalogSettings.HideProductDefaultPictures,
							storeLocation);

						model.SearchResult = products
							.Select(x =>
							{
								var item = new EntityPickerModel.SearchResultModel
								{
									Id = x.Id,
									ReturnValue = (model.ReturnField.IsCaseInsensitiveEqual("sku") ? x.Sku : x.Id.ToString()),
									Title = x.Name,
									Summary = x.Sku,
									SummaryTitle = "{0}: {1}".FormatInvariant(sku, x.Sku.NaIfEmpty()),
									Published = (hasPermission ? x.Published : (bool?)null)
								};

								if (disableIfNotSimpleProduct)
								{
									item.Disable = (x.ProductTypeId != (int)ProductType.SimpleProduct);
								}
								else if (disableIfGroupedProduct)
								{
									item.Disable = (x.ProductTypeId == (int)ProductType.GroupedProduct);
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

								item.ImageUrl = allPictureInfos.Get(x.MainPictureId.GetValueOrDefault())?.Url;

								return item;
							})
							.ToList();

						#endregion
					}
				}
			}
			catch (Exception exception)
			{
				NotifyError(exception.ToAllMessages());
			}

			return PartialView("Picker.List", model);
		}

		#endregion
	}
}
