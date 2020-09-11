using SmartStore.Core.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Localization
{
    public class Text : IText
    {
        private readonly ILocalizationService _localizationService;

        public Text(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public LocalizedString Get(string key, params object[] args)
        {
            return GetEx(key, 0, args);
        }

        public LocalizedString GetEx(string key, int languageId, params object[] args)
        {
            try
            {
                var value = _localizationService.GetResource(key, languageId);

                if (string.IsNullOrEmpty(value))
                {
                    return new LocalizedString(key);
                }

                if (args == null || args.Length == 0)
                {
                    return new LocalizedString(value);
                }

                return new LocalizedString(string.Format(value, args), key, args);
            }
            catch { }

            return new LocalizedString(key);
        }
    }
}
