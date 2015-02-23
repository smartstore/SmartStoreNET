using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Plugins;

namespace SmartStore.Web.Framework.Plugins
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class LicenseRequiredAttribute : ActionFilterAttribute
	{
		public ICommonServices CommonService { get; set; }
		public IPluginFinder PluginFinder { get; set; }
		public ILicenseStorageService LicenseStorageService { get; set; }
		public ILogger Logger { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext == null || filterContext.HttpContext == null)
				return;

			if (!DataSettings.DatabaseIsInstalled())
				return;

			var request = filterContext.HttpContext.Request;
			if (request == null)
				return;

			var assembly = filterContext.Controller.GetType().Assembly;
			var descriptor = PluginFinder.GetPluginDescriptorByAssembly(assembly);
			var storeId = CommonService.StoreContext.CurrentStore.Id;

			string failureMessage;

			if (!LicenseCheckerHelper.HasActiveLicense(descriptor, storeId, out failureMessage, CommonService, LicenseStorageService, Logger))
			{
				string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
				string actionName = filterContext.ActionDescriptor.ActionName;

				if (request.IsAjaxRequest())
				{
					CommonService.Notifier.Add(NotifyType.Error, new LocalizedString(failureMessage));

					filterContext.Result = new JsonResult
					{
						JsonRequestBehavior = JsonRequestBehavior.AllowGet,
						Data = new
						{
							error = true,
							controller = controllerName,
							action = actionName,
							message = failureMessage
						}
					};
				}
				else
				{
					var dic = new Dictionary<string, object>
					{
						{ "SystemName", descriptor.SystemName },
						{ "ControllerName", controllerName },
						{ "ActionName", actionName },
						{ "IsChildAction", filterContext.IsChildAction },
						{ "Message", failureMessage }
					};

					var master = (filterContext.IsChildAction ? null : "~/Views/Shared/_ColumnsOne.cshtml");

					filterContext.Result = new ViewResult
					{
						ViewName = "LicenseRequired",
						MasterName = master,
						ViewData = new ViewDataDictionary<Dictionary<string, object>>(dic),
						TempData = filterContext.Controller.TempData
					};

					//var urlHelper = new UrlHelper(request.RequestContext);
					//var url = urlHelper.Action("LicenseRequired", "Home", new { area = "" });

					//filterContext.Controller.TempData["LicenseRequiredData"] = dic;

					//filterContext.Result = new RedirectResult(url, false);
				}
			}
		}
	}
}
