using SmartStore.Core;
using SmartStore.Core.Caching;
using Telerik.Web.Mvc.Infrastructure;

namespace SmartStore.Web.Framework.Localization
{
    public class TelerikLocalizationServiceFactory : ILocalizationServiceFactory
    {
        private readonly IWorkContext _workContext;
        private readonly Services.Localization.ILocalizationService _localizationService;
		private readonly IRequestCache _requestCache;

		public TelerikLocalizationServiceFactory(
			IWorkContext workContext, 
			Services.Localization.ILocalizationService localizationService,
			IRequestCache requestCache)
        {
            _workContext = workContext;
            _localizationService = localizationService;
			_requestCache = requestCache;
        }

        public ILocalizationService Create(string componentName, System.Globalization.CultureInfo culture)
        {
            return new TelerikLocalizationService(componentName, _workContext.WorkingLanguage.Id, _localizationService, _requestCache);
        }
    }
}