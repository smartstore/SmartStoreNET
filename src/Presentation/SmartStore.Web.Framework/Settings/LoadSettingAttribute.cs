using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Settings
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class LoadSettingAttribute : FilterAttribute, IActionFilter
	{
		private int _storeId;
		private ISettings _settings;

		public ICommonServices Services { get; set; }

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// Find the required ISettings concrete type in ActionDescriptor.GetParameters()
			var settingsParam = FindParameter<ISettings>(filterContext, this, true);

			// Get the current configured store id
			_storeId = filterContext.Controller.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);

			// Load settings for the settings type obtained with FindSettingsParameter()
			_settings = Services.Settings.LoadSetting(settingsParam.ParameterType, _storeId);

			if (_settings == null)
			{
				throw new InvalidOperationException($"Could not load settings for type '{settingsParam.ParameterType.FullName}'.");
			}

			// Replace settings from action parameters with our loaded settings
			filterContext.ActionParameters[settingsParam.ParameterName] = _settings;
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var viewResult = filterContext.Result as ViewResultBase;

			if (viewResult != null)
			{
				var model = viewResult.Model;

				if (model == null)
				{
					throw new InvalidOperationException($"Could not obtain a model instance to override keys for'.");
				}

				var settingsHelper = new StoreDependingSettingHelper(filterContext.Controller.ViewData);
				settingsHelper.GetOverrideKeys(_settings, model, _storeId, Services.Settings);
			}
		}

		internal static ParameterDescriptor FindParameter<T>(ActionExecutingContext filterContext, FilterAttribute attribute, bool requireDefaultConstructor)
		{
			Guard.NotNull(filterContext, nameof(filterContext));
			Guard.NotNull(attribute, nameof(attribute));

			var t = typeof(T);

			var query = filterContext
				.ActionDescriptor
				.GetParameters()
				.Where(x => t.IsAssignableFrom(x.ParameterType));

			var count = query.Count();

			if (count != 1)
			{
				throw new InvalidOperationException(
					$"A controller action method with a '{attribute.GetType().Name}' attribute requires exactly one action parameter of type '{t.Name}' in order to execute properly.");
			}

			var param = query.FirstOrDefault();

			if (requireDefaultConstructor && !param.ParameterType.HasDefaultConstructor())
			{
				throw new InvalidOperationException($"The parameter '{param.ParameterName}' must have a default parameterless constructor.");
			}

			return param;
		}
	}
}
