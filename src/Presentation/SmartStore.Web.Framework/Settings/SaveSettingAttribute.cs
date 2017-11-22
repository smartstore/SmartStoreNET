using System;
using System.Linq;
using System.Web.Routing;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Settings
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class SaveSettingAttribute : LoadSettingAttribute
	{
		private FormCollection _form;
		private IDisposable _settingsWriteBatch;

		public SaveSettingAttribute() 
			: base(true)
		{
		}

		public SaveSettingAttribute(bool updateParameterFromStore)
			: base(updateParameterFromStore)
		{
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.Controller.ViewData.ModelState.IsValid)
			{
				return;
			}

			base.OnActionExecuting(filterContext);

			// Find the required FormCollection parameter in ActionDescriptor.GetParameters()
			var formParam = FindActionParameters<FormCollection>(filterContext.ActionDescriptor, requireDefaultConstructor: false, throwIfNotFound: false).FirstOrDefault();
			_form = formParam != null 
				? (FormCollection)filterContext.ActionParameters[formParam.ParameterName]
				: BindFormCollection(filterContext.Controller.ControllerContext);


			_settingsWriteBatch = Services.Settings.BeginScope();
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (_settingsWriteBatch != null)
			{
				_settingsWriteBatch.Dispose();
				_settingsWriteBatch = null;
			}

			if (!filterContext.Controller.ViewData.ModelState.IsValid)
			{
				return;
			}

			var redirectResult = filterContext.Result as RedirectToRouteResult;
			if (redirectResult != null)
			{
				var controllerName = redirectResult.RouteValues["controller"] as string;
				var areaName = redirectResult.RouteValues["area"] as string;
				if (controllerName.IsCaseInsensitiveEqual("security") && areaName.IsCaseInsensitiveEqual("admin"))
				{
					// Insufficient permission. Get outta here, because the action did not run. We must not save.
					return;
				}
			}

			var settingHelper = new StoreDependingSettingHelper(filterContext.Controller.ViewData);

			foreach (var param in _settingParams)
			{
				settingHelper.UpdateSettings(param.Instance, _form, _storeId, Services.Settings);
			}

			base.OnActionExecuted(filterContext);
		}

		private FormCollection BindFormCollection(ControllerContext controllerContext)
		{
			var bindingContext = new ModelBindingContext
			{
				ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(FormCollection)),
				ModelState = controllerContext.Controller.ViewData.ModelState,
				ValueProvider = controllerContext.Controller.ValueProvider
			};

			var modelBinder = ModelBinders.Binders.GetBinder(typeof(FormCollection));

			return (FormCollection)modelBinder.BindModel(controllerContext, bindingContext);
		}
	}
}
