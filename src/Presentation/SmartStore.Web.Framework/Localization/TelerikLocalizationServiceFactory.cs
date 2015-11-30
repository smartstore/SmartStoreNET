using SmartStore.Core;
using Telerik.Web.Mvc.Infrastructure;

namespace SmartStore.Web.Framework.Localization
{
    public class TelerikLocalizationServiceFactory : ILocalizationServiceFactory
    {
        private IWorkContext _workContext;
        private Services.Localization.ILocalizationService _localizationService;

        public TelerikLocalizationServiceFactory(IWorkContext workContext, Services.Localization.ILocalizationService localizationService)
        {
            _workContext = workContext;
            _localizationService = localizationService;
        }
        public ILocalizationService Create(string resourceName, System.Globalization.CultureInfo culture)
        {
            return new TelerikLocalizationService(resourceName, _workContext.WorkingLanguage.Id, _localizationService);
        }
    }
}