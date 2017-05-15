using System;
using System.Web.Mvc;
using SmartStore.Clickatell.Models;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Clickatell.Controllers
{
	[AdminAuthorize]
    public class SmsClickatellController : PluginControllerBase
    {
		private readonly ICommonServices _services;
        private readonly IPluginFinder _pluginFinder;

        public SmsClickatellController(
			ICommonServices services,
			IPluginFinder pluginFinder)
        {
			_services = services;
            _pluginFinder = pluginFinder;
        }

		public ActionResult Configure()
		{
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<ClickatellSettings>(storeScope);

			var model = new SmsClickatellModel
			{
				Enabled = settings.Enabled,
				PhoneNumber = settings.PhoneNumber,
				ApiId = settings.ApiId
			};

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _services.Settings);

			return View(model);
        }

        [HttpPost, FormValueRequired("save")]
        public ActionResult Configure(SmsClickatellModel model, FormCollection form)
        {
			if (ModelState.IsValid)
			{
				var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
				int storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
				var settings = _services.Settings.LoadSetting<ClickatellSettings>(storeScope);

				settings.Enabled = model.Enabled;
				settings.PhoneNumber = model.PhoneNumber;
				settings.ApiId = model.ApiId;

				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _services.Settings);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			}

			return Configure();
		}

        [HttpPost, ActionName("Configure"), FormValueRequired("test-sms")]
        public ActionResult TestSms(SmsClickatellModel model)
        {
            try
            {
                if (model.TestMessage.IsEmpty())
                {
					model.TestSucceeded = false;
					model.TestSmsResult = T("Plugins.Sms.Clickatell.EnterMessage");
                }
                else
                {
                    var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.Clickatell");
                    var plugin = pluginDescriptor.Instance() as ClickatellSmsProvider;

					plugin.SendSms(model.TestMessage);

					model.TestSucceeded = true;
					model.TestSmsResult = T("Plugins.Sms.Clickatell.TestSuccess");
                }
            }
            catch (Exception exception)
            {
				model.TestSucceeded = false;
				model.TestSmsResult = T("Plugins.Sms.Clickatell.TestFailed");
				model.TestSmsDetailResult = exception.Message;
            }

            return View("Configure", model);
        }
    }
}