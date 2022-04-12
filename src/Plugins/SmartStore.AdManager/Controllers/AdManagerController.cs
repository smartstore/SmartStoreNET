using SmartStore.AdManager.Models;
using SmartStore.AdManager.Settings;
using SmartStore.ComponentModel;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using System.Web.Mvc;



namespace SmartStore.Controllers
{
    public class AdManagerController : AdminControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IGenericAttributeService _genericAttributeService;

        public AdManagerController(
            ICommonServices services,
            IGenericAttributeService genericAttributeService)
        {
            _services = services;
            _genericAttributeService = genericAttributeService;
        }


        [AdminAuthorize]
        [ChildActionOnly]
        [LoadSetting]
        public ActionResult Configure(AdManagerSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);



            return View(model);
        }

        [AdminAuthorize]
        [ChildActionOnly]
        [LoadSetting]
        public ActionResult TestView(int shopID)
        {
            
            return View("TestView");
        }


        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [SaveSetting]
        public ActionResult Configure(AdManagerSettings settings, ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }


            MiniMapper.Map(model, settings);
            return RedirectToConfiguration("SmartStore.AdManager");
        }




    }
}