using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Controllers
{
    public abstract class ManageController : SmartController
    {
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
        /// Save the store mappings for an entity.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="selectedStoreIds">Selected store identifiers.</param>
        protected virtual void SaveStoreMappings<T>(T entity, int[] selectedStoreIds) where T : BaseEntity, IStoreMappingSupported
        {
            Guard.NotNull(entity, nameof(entity));

            Services.Resolve<IStoreMappingService>().SaveStoreMappings(entity, selectedStoreIds);
        }

        /// <summary>
        /// Save the ACL mappings for an entity.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <param name="model">Model representation of ACL selection</param>
        protected virtual void SaveAclMappings<T>(T entity, params int[] selectedCustomerRoleIds) where T : BaseEntity, IAclSupported
        {
            Services.Resolve<IAclService>().SaveAclMappings(entity, selectedCustomerRoleIds ?? Array.Empty<int>());
        }

        /// <summary>
        /// Access denied view
        /// </summary>
        /// <returns>Access denied view</returns>
        [SuppressMessage("ReSharper", "Mvc.AreaNotResolved")]
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
