using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SmartStore.Core.Localization
{
    public static class LocalizationHelper
    {
        private readonly static HashSet<string> _cultureCodes =
            new HashSet<string>(
                CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures | CultureTypes.UserCustomCulture)
                .Select(x => x.IetfLanguageTag)
                .Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCultureCode(string locale)
        {
            return locale.HasValue() && _cultureCodes.Contains(locale);
        }

        /// <summary>
        /// Enumerates all parent cultures, excluding the top-most invariant culture
        /// </summary>
        /// <param name="locale">The ISO culture code, e.g. de-DE, en-US or just en</param>
        /// <returns>Parent cultures</returns>
        public static IEnumerable<CultureInfo> EnumerateParentCultures(string locale)
        {
            if (locale.IsEmpty() || !_cultureCodes.Contains(locale))
            {
                return Enumerable.Empty<CultureInfo>();
            }

            return EnumerateParentCultures(CultureInfo.GetCultureInfo(locale));
        }

        /// <summary>
        /// Enumerates all parent cultures, excluding the top-most invariant culture
        /// </summary>
        /// <param name="culture">The culture info to enumerate parents for</param>
        /// <returns>Parent cultures</returns>
        public static IEnumerable<CultureInfo> EnumerateParentCultures(CultureInfo culture)
        {
            if (culture == null)
            {
                yield break;
            }

            while (culture.Parent.TwoLetterISOLanguageName != "iv")
            {
                yield return culture.Parent;
                culture = culture.Parent;
            }
        }

        public static bool TryGetCultureInfoForLocale(string locale, out CultureInfo culture)
        {
            culture = null;

            try
            {
                culture = CultureInfo.GetCultureInfoByIetfLanguageTag(locale);
                return culture != null;
            }
            catch
            {
                return false;
            }
        }

        public static string GetLanguageNativeName(string locale)
        {
            if (TryGetCultureInfoForLocale(locale, out var culture))
            {
                return culture.NativeName;
            }

            return null;
        }

        public static string GetLanguageNativeName(string locale, bool englishName, bool shortName = false)
        {
            try
            {
                if (locale.HasValue())
                {
                    var info = CultureInfo.GetCultureInfoByIetfLanguageTag(locale);
                    if (info != null)
                    {
                        if (shortName)
                        {
                            info = info.Parent;
                        }

                        if (englishName)
                        {
                            return info.EnglishName;
                        }
                        else
                        {
                            return info.NativeName;
                        }
                        
                    }
                }
            }
            catch 
            { 
            }

            return null;
        }

        public static string NormalizeLanguageDisplayName(string languageName, bool stripRegion = false, CultureInfo culture = null)
        {
            if (string.IsNullOrEmpty(languageName) || languageName.Length == 0)
            {
                return languageName;
            }

            // First char to upper.
            if (char.IsLower(languageName[0]))
            {
                languageName = (culture ?? CultureInfo.InvariantCulture).TextInfo.ToTitleCase(languageName);
            }

            var bracketIndex = languageName.IndexOfAny(new[] { '(', '[' });
            var hasRegion = bracketIndex > -1;
            var endBracket = ')';

            if (hasRegion)
            {
                if (languageName[bracketIndex] == '[') 
                {
                    endBracket = ']';
                }
                
                if (stripRegion)
                {
                    languageName = languageName.Substring(0, bracketIndex).TrimEnd();
                }
            }

            // Remove everything after ',' within Region part
            if (hasRegion && !stripRegion)
            {
                var commaIndex = languageName.IndexOf(',');
                if (commaIndex > -1)
                {
                    languageName = languageName.Substring(0, commaIndex) + endBracket;
                }
            }

            return languageName;
        }

        public static string GetCurrencySymbol(string locale)
        {
            try
            {
                if (locale.HasValue())
                {
                    var info = new RegionInfo(locale);
                    if (info != null)
                        return info.CurrencySymbol;
                }
            }
            catch { }

            return null;
        }
    }
}
