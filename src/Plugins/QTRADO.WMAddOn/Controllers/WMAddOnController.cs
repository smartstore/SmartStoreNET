using System.Web.Mvc;

using QTRADO.WMAddOn.Models;
using QTRADO.WMAddOn.Settings;

using SmartStore.ComponentModel;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;



namespace SmartStore.Controllers
{
    public class WMAddOnController : AdminControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IGenericAttributeService _genericAttributeService;

        public WMAddOnController(
            ICommonServices services,
            IGenericAttributeService genericAttributeService)
        {
            _services = services;
            _genericAttributeService = genericAttributeService;
        }


        [AdminAuthorize]
        [ChildActionOnly]
        [LoadSetting]
        public ActionResult Configure(WMAddOnSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);



            return View(model);
        }


        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [SaveSetting]
        public ActionResult Configure(WMAddOnSettings settings, ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }


            MiniMapper.Map(model, settings);
            return RedirectToConfiguration("QTRADO.WMAddOn");
        }




        [AdminAuthorize]
        public ActionResult AdminEditTab(int entityId, string entityName)
        {
            var model = new AdminEditTabModel();

            // Simple values can be stored and obtained as GenericAttributes. More complex values should implement their own domain objects.
            // Get saved value from GenericAttribute. (Value persitence can be found in the ModelBoundEventConsumer.)
            model.IsActive = _genericAttributeService.GetAttribute<bool>(entityName, entityId, "WMAddOnIsActive");
            model.EntityId = entityId;
            model.EntityName = entityName;

            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[WMAddOn]" };
            return result;
        }

    }
}