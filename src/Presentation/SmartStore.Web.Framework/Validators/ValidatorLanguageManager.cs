using System.Globalization;
using System.Web.Hosting;
using FluentValidation.Resources;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Services;

namespace SmartStore.Web.Framework.Validators
{
    public class ValidatorLanguageManager : LanguageManager
    {
        private readonly bool _canResolveServices;

        public ValidatorLanguageManager()
        {
            _canResolveServices = DataSettings.DatabaseIsInstalled() && HostingEnvironment.IsHosted;
        }

        public override string GetString(string key, CultureInfo culture = null)
        {
            string result = base.GetString(key, culture);

            if (_canResolveServices)
            {
                // (Perf) although FV expects a culture parameter, we gonna ignore it.
                // It's highly unlikely that it is anything different than our WorkingLanguage.
                var services = EngineContext.Current.Resolve<ICommonServices>();
                result = services.Localization.GetResource("Validation." + key, logIfNotFound: false, defaultValue: result, returnEmptyIfNotFound: true);
            }

            return result;
        }
    }
}
