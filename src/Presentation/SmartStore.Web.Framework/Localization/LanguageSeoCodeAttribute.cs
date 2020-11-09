using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Filters;

namespace SmartStore.Web.Framework.Localization
{
    /// <summary>
    /// Attribute which ensures that store URL contains a language SEO code if "SEO friendly URLs with multiple languages" setting is enabled
    /// </summary>
    public class LanguageSeoCodeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public Lazy<IWorkContext> WorkContext { get; set; }
        public Lazy<ILanguageService> LanguageService { get; set; }
        public Lazy<LocalizationSettings> LocalizationSettings { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext?.HttpContext?.Request;
            if (request == null)
                return;

            // Don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            // ensure that this route is registered and localizable (LocalizedRoute in RouteProvider.cs)
            if (!(filterContext.RouteData?.Route is LocalizedRoute))
                return;

            if (!DataSettings.DatabaseIsInstalled())
                return;

            var localizationSettings = LocalizationSettings.Value;
            if (!localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                return;

            // Process current URL
            var workContext = WorkContext.Value;
            var languageService = LanguageService.Value;
            var workingLanguage = workContext.WorkingLanguage;
            var helper = new LocalizedUrlHelper(request, true);
            var defaultSeoCode = languageService.GetDefaultLanguageSeoCode();

            if (helper.IsLocalizedUrl(out var seoCode))
            {
                if (!languageService.IsPublishedLanguage(seoCode))
                {
                    // Language is not defined in system or not assigned to store
                    if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.ReturnHttp404)
                    {
                        filterContext.Result = HandleExceptionFilter.Create404Result(filterContext);

                        var seoCodeReplacement = localizationSettings.DefaultLanguageRedirectBehaviour == DefaultLanguageRedirectBehaviour.PrependSeoCodeAndRedirect
                            ? workingLanguage.GetTwoLetterISOLanguageName()
                            : string.Empty;

                        filterContext.RequestContext.RouteData.DataTokens["SeoCodeReplacement"] = seoCodeReplacement;
                    }
                    else if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.FallbackToWorkingLanguage)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), !request.IsLocal);
                    }
                }
                else
                {
                    // Redirect default language (if desired)
                    if (seoCode == defaultSeoCode && localizationSettings.DefaultLanguageRedirectBehaviour == DefaultLanguageRedirectBehaviour.StripSeoCode)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), !request.IsLocal);
                    }
                }

                // Already localized URL, skip the rest
                return;
            }

            // Keep default language prefixless (if desired)
            if (workingLanguage.UniqueSeoCode == defaultSeoCode && (int)(localizationSettings.DefaultLanguageRedirectBehaviour) > 0)
            {
                return;
            }

            // Add language code to URL
            helper.PrependSeoCode(workingLanguage.UniqueSeoCode);
            filterContext.Result = new RedirectResult(helper.GetAbsolutePath());
        }
    }
}
