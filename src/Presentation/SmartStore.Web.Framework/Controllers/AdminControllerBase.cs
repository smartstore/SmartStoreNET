using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.Web.Framework.Controllers
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class NonAdminAttribute : Attribute
	{
	}

	[AdminValidateIpAddress(Order = 100)]
	[RequireHttpsByConfig(SslRequirement.Yes, Order = 110)]
    [CustomerLastActivity(Order = 100)]
    [StoreIpAddress(Order = 100)]
	[AdminThemed(Order = -1)]
	public abstract class AdminControllerBase : SmartController
    { 
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			var isNonAdmin = filterContext.ActionDescriptor.HasAttribute<NonAdminAttribute>(true);
			Services.Resolve<IWorkContext>().IsAdmin = !isNonAdmin;
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
		/// Save the store mappings for an entity
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="entity">The entity</param>
		/// <param name="model">Model representation of store selection</param>
		protected virtual void SaveStoreMappings<T>(T entity, IStoreSelector model) where T : BaseEntity, IStoreMappingSupported
		{
			entity.LimitedToStores = model.LimitedToStores;
			Services.Resolve<IStoreMappingService>().SaveStoreMappings(entity, model.SelectedStoreIds);
		}

		/// <summary>
		/// Save the ACL mappings for an entity
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="entity">The entity</param>
		/// <param name="model">Model representation of ACL selection</param>
		protected virtual void SaveAclMappings<T>(T entity, IAclSelector model) where T : BaseEntity, IAclSupported
		{
			entity.SubjectToAcl = model.SubjectToAcl;
			Services.Resolve<IAclService>().SaveAclMappings(entity, model.SelectedCustomerRoleIds);
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
