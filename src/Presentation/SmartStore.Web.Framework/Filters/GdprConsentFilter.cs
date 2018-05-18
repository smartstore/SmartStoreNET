using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.UI;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Filters
{
	public class GdprConsentFilter : IActionFilter, IResultFilter
	{
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly PrivacySettings _privacySettings;

		// TODO: needs to be public & not static so plugins can add own interceptable actions
		public static string[] s_interceptableActions = new string[] {
			"Register",
			"ContactUs",
			"AskQuestion",
			"EmailAFriend",
			"Reviews",
			"BlogPost",
			"EmailWishlist",
			"NewsItem",
			"PostCreate",
			"TopicCreate",
		};

		public GdprConsentFilter(
			IGenericAttributeService genericAttributeService,
			ICommonServices services,
			Lazy<IWidgetProvider> widgetProvider,
			PrivacySettings privacySettings)
		{
			_genericAttributeService = genericAttributeService;
			_services = services;
			_widgetProvider = widgetProvider;
			_privacySettings = privacySettings;
		}

		private static bool IsInterceptableAction(string actionName)
		{
			return s_interceptableActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!_privacySettings.DisplayGdprConsentOnForms)
				return;

			if (filterContext?.ActionDescriptor == null || filterContext?.HttpContext?.Request == null)
				return;

			string actionName = filterContext.ActionDescriptor.ActionName;
			string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

			if (!IsInterceptableAction(actionName))
				return;

			var customer = _services.WorkContext.CurrentCustomer;
			var hasConsentedToGdpr = filterContext.HttpContext.Request.Form["GdprConsent"];
			
			if (filterContext.HttpContext.Request.HttpMethod.Equals("POST") && hasConsentedToGdpr != null)
			{
				if (hasConsentedToGdpr.Contains("true"))
				{
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.HasConsentedToGdpr, true);
				}
				else
				{
					// add a validation message
					filterContext.Controller.ViewData.ModelState.AddModelError("", _services.Localization.GetResource("GdprConsent.ValidationMessage"));
					return;
				}
			}
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{

		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_privacySettings.DisplayGdprConsentOnForms)
				return;

			if (filterContext.IsChildAction)
				return;

			var result = filterContext.Result;

			// should only run on a full view rendering result or HTML ContentResult
			if (!result.IsHtmlViewResult())
				return;
			
			_widgetProvider.Value.RegisterAction(
				new[] { "gdpr_consent" },
				"GdprConsent",
				"Common",
				new { area = "" });
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}
}