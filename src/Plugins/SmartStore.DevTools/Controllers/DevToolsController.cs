using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Security;
using SmartStore.DevTools.Blocks;
using SmartStore.DevTools.Models;
using SmartStore.DevTools.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.DevTools.Controllers
{
    public class DevToolsController : SmartController
    {
        [AdminAuthorize, Permission(DevToolsPermissions.Read)]
        [ChildActionOnly, LoadSetting]
        public ActionResult Configure(ProfilerSettings settings)
        {
            var model = MiniMapper.Map<ProfilerSettings, ConfigurationModel>(settings);

            return View(model);
        }

        [AdminAuthorize, Permission(DevToolsPermissions.Update)]
        [HttpPost, ChildActionOnly, SaveSetting]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ConfigurationModel model, ProfilerSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return RedirectToConfiguration("SmartStore.DevTools");
        }

        [ChildActionOnly]
        public ActionResult MiniProfiler()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult MachineName()
        {
            ViewBag.EnvironmentIdentifier = Services.ApplicationEnvironment.EnvironmentIdentifier;

            return View();
        }

        [ChildActionOnly]
        public ActionResult WidgetZone(string widgetZone)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<ProfilerSettings>(storeScope);

            if (settings.DisplayWidgetZones)
            {
                ViewData["widgetZone"] = widgetZone;

                return View();
            }

            return new EmptyResult();
        }

        [ChildActionOnly]
        public ActionResult SampleBlock(SampleBlock block)
        {
            // Do something here with your block instance and return a result that should be rendered by the Page Builder.
            return View(block);
        }

        [AdminAuthorize, AdminThemed]
        public ActionResult BackendExtension()
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            return View(model);
        }

        [AdminAuthorize]
        public ActionResult ProductEditTab(int productId, FormCollection form)
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[DevTools]" };
            return result;
        }

        public ActionResult MyDemoWidget()
        {
            return Content("Hello world! This is a sample widget created for demonstration purposes by Dev-Tools plugin.");
        }

        [ChildActionOnly, AllowAnonymous]
        public ActionResult MyCustomFacetTemplate(FacetGroup facetGroup, string templateName)
        {
            /// Just a "proxy" for our <see cref="Services.CustomFacetTemplateSelector" />.
            return PartialView(templateName, facetGroup);
        }
    }
}