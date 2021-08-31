using SmartStore.ComponentModel;
using SmartStore.ConfigurableExportTest.Models;
using SmartStore.ConfigurableExportTest.Settings;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using System.Web.Mvc;



namespace SmartStore.Controllers
{
    public class ConfigurableExportController : AdminControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IGenericAttributeService _genericAttributeService;

        public ConfigurableExportController(
            ICommonServices services,
            IGenericAttributeService genericAttributeService)
        {
            _services = services;
            _genericAttributeService = genericAttributeService;
        }


        [AdminAuthorize]
        [ChildActionOnly]
        [LoadSetting]
        public ActionResult Configure(ConfigurableExportSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);



            return View(model);
        }


        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [SaveSetting]
        public ActionResult Configure(ConfigurableExportSettings settings, ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }


            MiniMapper.Map(model, settings);
            return RedirectToConfiguration("SmartStore.ConfigurableExportTest");
        }




    }
}