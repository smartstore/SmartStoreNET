using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartStore.Core.Localization
{
    public static class LocalizationHelper
    {
        private readonly static HashSet<string> _cultureCodes =
            new HashSet<string>(
                CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures | CultureTypes.UserCustomCulture)
                .Select(x => x.IetfLanguageTag)
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        public static bool IsValidCultureCode(string cultureCode)
        {
            return _cultureCodes.Contains(cultureCode);
        }

    }
}
