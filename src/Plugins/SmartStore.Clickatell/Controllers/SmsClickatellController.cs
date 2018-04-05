using System;
using System.Web.Mvc;
using SmartStore.Clickatell.Models;
using SmartStore.ComponentModel;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Clickatell.Controllers
{
	[AdminAuthorize]
    public class SmsClickatellController : PluginControllerBase
    {
		private readonly IPluginFinder _pluginFinder;

        public SmsClickatellController(IPluginFinder pluginFinder)
        {
            _pluginFinder = pluginFinder;
        }

		[LoadSetting]
		public ActionResult Configure(ClickatellSettings settings)
		{
			var model = new SmsClickatellModel();
			MiniMapper.Map(settings, model);

			return View(model);
        }

        [HttpPost, SaveSetting, FormValueRequired("save")]
        public ActionResult Configure(ClickatellSettings settings, SmsClickatellModel model)
        {
			if (!ModelState.IsValid)
			{
				return Configure(settings);
			}

			MiniMapper.Map(model, settings);
			settings.ApiId = model.ApiId.TrimSafe();

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

			return RedirectToConfiguration(ClickatellSmsProvider.SystemName);
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
                    var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(ClickatellSmsProvider.SystemName);
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