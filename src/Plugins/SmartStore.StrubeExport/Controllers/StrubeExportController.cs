using SmartStore.ComponentModel;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.DataExchange.Export;
using SmartStore.StrubeExport.Models;
using SmartStore.StrubeExport.Settings;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using System;
using System.Web.Mvc;



namespace SmartStore.Controllers
{
    public class StrubeExportController : AdminControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly Lazy<IExportProfileService> _exportService;

        public StrubeExportController(
            ICommonServices services,
            IGenericAttributeService genericAttributeService,Lazy<IExportProfileService> exportProfileService)
        {
            _services = services;
            _genericAttributeService = genericAttributeService;
            _exportService = exportProfileService;
        }


        [AdminAuthorize]
        [ChildActionOnly]
        [LoadSetting]
        public ActionResult Configure(StrubeExportSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);



            return View(model);
        }


        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [SaveSetting]
        public ActionResult Configure(StrubeExportSettings settings, ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }


            MiniMapper.Map(model, settings);
            return RedirectToConfiguration("SmartStore.StrubeExport");
        }




    }
}