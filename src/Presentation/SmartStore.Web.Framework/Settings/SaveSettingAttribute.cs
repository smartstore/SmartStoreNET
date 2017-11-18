using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Settings
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class SaveSettingAttribute : FilterAttribute, IActionFilter
	{
		private int _storeId;
		private ISettings _settings;
		private FormCollection _form;
		private bool _modelStateValid;

		public ICommonServices Services { get; set; }

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_modelStateValid = filterContext.Controller.ViewData.ModelState.IsValid;

			if (!_modelStateValid)
			{
				return;
			}

			// Find the required ISettings concrete type in ActionDescriptor.GetParameters()
			var settingsParam = LoadSettingAttribute.FindParameter<ISettings>(filterContext, this, true);

			// Get the current configured store id
			_storeId = filterContext.Controller.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);

			// Load settings for the settings type obtained with FindSettingsParameter()
			_settings = Services.Settings.LoadSetting(settingsParam.ParameterType, _storeId);

			if (_settings == null)
			{
				throw new InvalidOperationException($"Could not load settings for type '{settingsParam.ParameterType.FullName}'.");
			}

			// Find the required FormCollection parameter in ActionDescriptor.GetParameters()
			var formParam = LoadSettingAttribute.FindParameter<FormCollection>(filterContext, this, false);
			_form = (FormCollection)filterContext.ActionParameters[formParam.ParameterName];

			// Replace settings from action parameters with our loaded settings
			filterContext.ActionParameters[settingsParam.ParameterName] = _settings;
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!_modelStateValid)
			{
				return;
			}

			var settingHelper = new StoreDependingSettingHelper(filterContext.Controller.ViewData);

			settingHelper.UpdateSettings(_settings, _form, _storeId, Services.Settings);

			var viewResult = filterContext.Result as ViewResultBase;

			if (viewResult != null)
			{
				var model = viewResult.Model;

				if (model == null)
				{
					throw new InvalidOperationException($"Could not obtain a model instance to override keys for'.");
				}

				settingHelper.GetOverrideKeys(_settings, model, _storeId, Services.Settings);
			}
		}
	}
}
