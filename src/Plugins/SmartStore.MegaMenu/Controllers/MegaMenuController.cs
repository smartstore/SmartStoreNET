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

namespace SmartStore.Controllers
{
    public class MegaMenuController : Controller
    {
        private readonly ICommonServices _commonServices;
        private readonly CatalogHelper _helper;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductService _productService;

        public MegaMenuController(
            ICommonServices commonServices,
            CatalogHelper helper,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IProductService productService)
        {
            _commonServices = commonServices;
            _helper = helper;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _productService = productService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_commonServices.StoreService, _commonServices.WorkContext);
            var settings = _commonServices.Settings.LoadSetting<MegaMenuSettings>(storeScope);

            var model = new ConfigurationModel();
            //model.ConfigSetting = settings.ConfigSetting;

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

            //settings.ConfigSetting = model.ConfigSetting;

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _commonServices.Settings);

            _commonServices.Settings.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
            var settings = _commonServices.Settings.LoadSetting<MegaMenuSettings>(_commonServices.StoreContext.CurrentStore.Id);

            var model = new MegaMenuNavigationModel();
            //model.ConfigSetting = settings.ConfigSetting;

            return View(model);
        }


        [AdminAuthorize]
        public ActionResult AdminEditTab(int categoryId)
        {
            var model = new AdminEditTabModel();
            
            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[MegaMenu]" };
            return result;
        }
        
        [HttpPost]
        public ActionResult RotatorProducts(int catId)
        {
            var products = new List<Product>();

            // TODO: get real products 
            var rotatorProducts = _productService.GetProductsByIds(new int[] { 1, 2, 3, 4 });

            foreach (var product in rotatorProducts)
            {
                //ensure has ACL permission and appropriate store mapping
                if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
                    products.Add(product);
            }

            if (products.Count == 0)
                return Content("");

            var model = _helper.PrepareProductOverviewModels(products, true, true, 100, false, false, false, false, true).ToList();

            return PartialView(model);
        }
    }
}