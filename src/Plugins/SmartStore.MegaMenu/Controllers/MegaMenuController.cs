using System.Web.Mvc;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Security;
using SmartStore.MegaMenu.Models;
using SmartStore.MegaMenu.Settings;
using SmartStore.Web.Controllers;
using SmartStore.Core.Domain.Catalog;
using System.Collections.Generic;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Catalog;
using System.Linq;
using SmartStore.MegaMenu.Services;
using SmartStore.MegaMenu.Domain;
using System;
using SmartStore.Services.Localization;

namespace SmartStore.Controllers
{
    public class MegaMenuController : AdminControllerBase
    {
        private readonly ICommonServices _commonServices;
        private readonly CatalogHelper _helper;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly IMegaMenuService _megaMenuService;
        private readonly ILanguageService _languageService;

        public MegaMenuController(
            ICommonServices commonServices,
            CatalogHelper helper,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductService productService,
            IPermissionService permissionService,
            IMegaMenuService megaMenuService,
            ILanguageService languageService)
        {
            _commonServices = commonServices;
            _helper = helper;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _productService = productService;
            _permissionService = permissionService;
            _megaMenuService = megaMenuService;
            _languageService = languageService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_commonServices.StoreService, _commonServices.WorkContext);
            var settings = _commonServices.Settings.LoadSetting<MegaMenuSettings>(storeScope);

            var model = new ConfigurationModel();
            model.ProductRotatorCycle = settings.ProductRotatorCycle;
            model.ProductRotatorDuration = settings.ProductRotatorDuration;
            model.ProductRotatorInterval = settings.ProductRotatorInterval;
            model.MenuMinHeight = settings.MenuMinHeight;

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _commonServices.Settings);

            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            ModelState.Clear();

            var storeScope = this.GetActiveStoreScopeConfiguration(_commonServices.StoreService, _commonServices.WorkContext);
            var settings = _commonServices.Settings.LoadSetting<MegaMenuSettings>(storeScope);

            settings.ProductRotatorCycle = model.ProductRotatorCycle;
            settings.ProductRotatorDuration = model.ProductRotatorDuration;
            settings.ProductRotatorInterval = model.ProductRotatorInterval;
            settings.MenuMinHeight = model.MenuMinHeight;

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _commonServices.Settings);

            _commonServices.Settings.ClearCache();

            return Configure();
        }

        [AdminAuthorize]
        public ActionResult AdminEditTab(int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(categoryId) ?? new MegaMenuRecord { CategoryId = categoryId };
            megaMenuRecord.CategoryId = categoryId;

            // prepare model
            var model = new DropdownConfigurationModel();

            model.AllowSubItemsColumnWrap = megaMenuRecord.AllowSubItemsColumnWrap;
            model.BgAlignX = megaMenuRecord.BgAlignX;
            model.BgAlignY = megaMenuRecord.BgAlignY;
            model.BgLink = megaMenuRecord.BgLink;
            model.BgOffsetX = megaMenuRecord.BgOffsetX;
            model.BgOffsetY = megaMenuRecord.BgOffsetY;
            model.BgPictureId = megaMenuRecord.BgPictureId;
            model.CategoryId = megaMenuRecord.CategoryId;
            model.DisplayBgPicture = megaMenuRecord.DisplayBgPicture;
            model.DisplayCategoryPicture = megaMenuRecord.DisplayCategoryPicture;
            model.DisplaySubItemsInline = megaMenuRecord.DisplaySubItemsInline;
            model.FavorInMegamenu = megaMenuRecord.FavorInMegamenu;
            model.HtmlColumnSpan = megaMenuRecord.HtmlColumnSpan;
            model.EntityId = megaMenuRecord.Id;
            model.IsActive = megaMenuRecord.IsActive;
            model.MaxItemsPerColumn = megaMenuRecord.MaxItemsPerColumn;
            model.MaxRotatorItems = megaMenuRecord.MaxRotatorItems;
            model.MaxSubItemsPerCategory = megaMenuRecord.MaxSubItemsPerCategory;
            model.MinChildCategoryThreshold = megaMenuRecord.MinChildCategoryThreshold;
            model.RotatorHeading = megaMenuRecord.RotatorHeading;
            model.SubItemsWrapTolerance = megaMenuRecord.SubItemsWrapTolerance;
            model.Summary = megaMenuRecord.Summary;
            model.TeaserHtml = megaMenuRecord.TeaserHtml;
            model.TeaserRotatorItemSelectType = megaMenuRecord.TeaserRotatorItemSelectType;
            model.TeaserRotatorProductIds = megaMenuRecord.TeaserRotatorProductIds;
            model.TeaserType = megaMenuRecord.TeaserType;

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.BgLink = megaMenuRecord.GetLocalized(x => x.BgLink, languageId, false, false);
                locale.RotatorHeading = megaMenuRecord.GetLocalized(x => x.RotatorHeading, languageId, false, false);
                locale.Summary = megaMenuRecord.GetLocalized(x => x.Summary, languageId, false, false);
                locale.TeaserHtml = megaMenuRecord.GetLocalized(x => x.TeaserHtml, languageId, false, false);
            });
            
            // make enums
            //var availableTeaserTypes = new List<SelectListItem>();
            //var teaserRotatorItemSelectType = new List<SelectListItem>();
            //var availableAlignmentsX = new List<SelectListItem>();
            //var availableAlignmentsY = new List<SelectListItem>();

            //availableTeaserTypes.Add(new SelectListItem { Text = "None", Value = TeaserType.None.ToString(), Selected = megaMenuRecord.TeaserType.Equals(TeaserType.None) });
            //availableTeaserTypes.Add(new SelectListItem { Text = "Html", Value = TeaserType.Html.ToString(), Selected = megaMenuRecord.TeaserType.Equals(TeaserType.Html) });
            //availableTeaserTypes.Add(new SelectListItem { Text = "Rotator", Value = TeaserType.Rotator.ToString(), Selected = megaMenuRecord.TeaserType.Equals(TeaserType.Rotator) });

            //teaserRotatorItemSelectType.Add(new SelectListItem { Text = "Custom", Value = TeaserRotatorItemSelectType.Custom.ToString(), Selected = megaMenuRecord.TeaserRotatorItemSelectType.Equals(TeaserRotatorItemSelectType.Custom) });
            //teaserRotatorItemSelectType.Add(new SelectListItem { Text = "Top", Value = TeaserRotatorItemSelectType.Top.ToString(), Selected = megaMenuRecord.TeaserRotatorItemSelectType.Equals(TeaserRotatorItemSelectType.Top) });
            //teaserRotatorItemSelectType.Add(new SelectListItem { Text = "Random", Value = TeaserRotatorItemSelectType.Random.ToString(), Selected = megaMenuRecord.TeaserRotatorItemSelectType.Equals(TeaserRotatorItemSelectType.Random) });
            //teaserRotatorItemSelectType.Add(new SelectListItem { Text = "DeepTop", Value = TeaserRotatorItemSelectType.DeepTop.ToString(), Selected = megaMenuRecord.TeaserRotatorItemSelectType.Equals(TeaserRotatorItemSelectType.DeepTop) });
            //teaserRotatorItemSelectType.Add(new SelectListItem { Text = "DeepRandom", Value = TeaserRotatorItemSelectType.DeepRandom.ToString(), Selected = megaMenuRecord.TeaserRotatorItemSelectType.Equals(TeaserRotatorItemSelectType.DeepRandom) });

            //availableAlignmentsX.Add(new SelectListItem { Text = "Left", Value = AlignX.Left.ToString(), Selected = megaMenuRecord.BgAlignX.Equals(AlignX.Left) });
            //availableAlignmentsX.Add(new SelectListItem { Text = "Center", Value = AlignX.Center.ToString(), Selected = megaMenuRecord.BgAlignX.Equals(AlignX.Center) });
            //availableAlignmentsX.Add(new SelectListItem { Text = "Right", Value = AlignX.Right.ToString(), Selected = megaMenuRecord.BgAlignX.Equals(AlignX.Right) });

            //availableAlignmentsY.Add(new SelectListItem { Text = "Top", Value = AlignY.Top.ToString(), Selected = megaMenuRecord.BgAlignY.Equals(AlignY.Top) });
            //availableAlignmentsY.Add(new SelectListItem { Text = "Center", Value = AlignY.Center.ToString(), Selected = megaMenuRecord.BgAlignY.Equals(AlignY.Center) });
            //availableAlignmentsY.Add(new SelectListItem { Text = "Bottom", Value = AlignY.Bottom.ToString(), Selected = megaMenuRecord.BgAlignY.Equals(AlignY.Bottom) });

            //var result = PartialView(megaMenuRecord);

            var result = PartialView(model);

            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[MegaMenu]" };

            //init list values
            //ViewData["AvailableTeaserTypes"] = availableTeaserTypes;
            //ViewData["TeaserRotatorItemSelectType"] = teaserRotatorItemSelectType;
            //ViewData["AvailableAlignmentsX"] = availableAlignmentsX;
            //ViewData["AvailableAlignmentsY"] = availableAlignmentsY;
            
            return result;
        }
        
        [HttpPost]
        public ActionResult RotatorProducts(int catId)
        {
            var products = new List<Product>();
            var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(catId) ?? new MegaMenuRecord();

            var rotatorProducts = new List<Product>();

            if (megaMenuRecord.TeaserRotatorItemSelectType == TeaserRotatorItemSelectType.Custom && megaMenuRecord.TeaserRotatorProductIds != null)
            {
                rotatorProducts = _productService.GetProductsByIds(megaMenuRecord.TeaserRotatorProductIds.Split(',').Select(Int32.Parse).ToArray()).ToList();
            }

            // TODO: get elems for other TeaserRotatorItemSelectType
            foreach (var product in rotatorProducts)
            {
                //ensure has ACL permission and appropriate store mapping
                if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
                    products.Add(product);
            }

            if (products.Count == 0)
                return Content("");

            var model = _helper.PrepareProductOverviewModels(products, true, true, 100, false, false, false, false, true)
                .Take(megaMenuRecord.MaxRotatorItems)
                .ToList();

            return PartialView(model);
        }
    }
}