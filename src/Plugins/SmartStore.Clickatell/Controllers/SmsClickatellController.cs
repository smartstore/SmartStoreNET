using System;
using System.Web.Mvc;
using SmartStore.Clickatell.Models;
using SmartStore.ComponentModel;
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

		[LoadSetting]
		public ActionResult Configure(ClickatellSettings settings)
		{
			var model = new SmsClickatellModel();
			MiniMapper.Map(settings, model);
			return View(model);
        }

        [HttpPost, SaveSetting, FormValueRequired("save")]
        public ActionResult Configure(ClickatellSettings settings, SmsClickatellModel model, FormCollection form)
        {
			if (ModelState.IsValid)
			{
				MiniMapper.Map(model, settings);
				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			}

			return Configure(settings);
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