using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Controllers
{
	
	[AdminThemed]
    [RequireHttpsByConfig(SslRequirement.Yes)]
    [AdminValidateIpAddress]
    [CustomerLastActivity]
    [StoreIpAddress]
    public abstract class AdminControllerBase : SmartController
    { 
        /// <summary>
        /// Initialize controller
        /// </summary>
        /// <param name="requestContext">Request context</param>
        protected override void Initialize(RequestContext requestContext)
        {
			var routeData = requestContext.RouteData;
			if (routeData != null && !routeData.DataTokens.ContainsKey("ParentActionViewContext"))
			{
				EngineContext.Current.Resolve<IWorkContext>().IsAdmin = true;
			}
            base.Initialize(requestContext);
        }
        
        /// <summary>
        /// Add locales for localizable entities
        /// </summary>
        /// <typeparam name="TLocalizedModelLocal">Localizable model</typeparam>
        /// <param name="languageService">Language service</param>
        /// <param name="locales">Locales</param>
        protected virtual void AddLocales<TLocalizedModelLocal>(ILanguageService languageService, IList<TLocalizedModelLocal> locales) where TLocalizedModelLocal : ILocalizedModelLocal
        {
            AddLocales(languageService, locales, null);
        }

        /// <summary>
        /// Add locales for localizable entities
        /// </summary>
        /// <typeparam name="TLocalizedModelLocal">Localizable model</typeparam>
        /// <param name="languageService">Language service</param>
        /// <param name="locales">Locales</param>
        /// <param name="configure">Configure action</param>
        protected virtual void AddLocales<TLocalizedModelLocal>(ILanguageService languageService, IList<TLocalizedModelLocal> locales, Action<TLocalizedModelLocal, int> configure) where TLocalizedModelLocal : ILocalizedModelLocal
        {
            foreach (var language in languageService.GetAllLanguages(true))
            {
                var locale = Activator.CreateInstance<TLocalizedModelLocal>();
                locale.LanguageId = language.Id;
                if (configure != null)
                {
                    configure.Invoke(locale, locale.LanguageId);
                }
                locales.Add(locale);
            }
        }

        /// <summary>
        /// Access denied view
        /// </summary>
        /// <returns>Access denied view</returns>
        protected ActionResult AccessDeniedView()
        {
            return RedirectToAction("AccessDenied", "Security", new { pageUrl = this.Request.RawUrl, area = "Admin" });
        }

		/// <summary>
		/// Renders default access denied view as a partial
		/// </summary>
		protected ActionResult AccessDeniedPartialView() 
		{
			return PartialView("~/Administration/Views/Security/AccessDenied.cshtml");
		}

    }
}
