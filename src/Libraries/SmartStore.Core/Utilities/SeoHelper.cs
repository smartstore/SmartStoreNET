using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Utilities
{
    public static class SeoHelper
    {
        private static readonly object _lock = new object();
        private static Dictionary<char, string> _userSeoCharacterTable;

        /// <summary>
        /// Get SEO friendly string
        /// </summary>
        /// <param name="name">String to be converted</param>
        /// <param name="convertNonWesternChars">A value indicating whether non western chars should be converted</param>
        /// <param name="allowUnicodeChars">A value indicating whether Unicode chars are allowed</param>
        /// <param name="charConversions">Raw data of semicolon separated char conversions</param>
        /// <returns>SEO friendly string</returns>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSeName(string name, bool convertNonWesternChars, bool allowUnicodeChars, string charConversions = null)
        {
            return GetSeName(name, convertNonWesternChars, allowUnicodeChars, true, charConversions);
        }

        /// <summary>
        /// Get SEO friendly string
        /// </summary>
        /// <param name="name">String to be converted</param>
        /// <param name="convertNonWesternChars">A value indicating whether non western chars should be converted</param>
        /// <param name="allowUnicodeChars">A value indicating whether Unicode chars are allowed</param>
        /// <param name="allowForwardSlash">A value indicating whether the forward slash (/) is allowed. Should be false for physical file names.</param>
        /// <param name="charConversions">Raw data of semicolon separated char conversions</param>
        /// <returns>SEO friendly string</returns>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static string GetSeName(
            string name,
            bool convertNonWesternChars,
            bool allowUnicodeChars,
            bool allowForwardSlash,
            string charConversions = null)
        {
            // Return empty value if text is null
            if (name == null) return "";

            const int maxlen = 400;

            if (charConversions != null && _userSeoCharacterTable == null)
            {
                InitializeUserSeoCharacterTable(charConversions);
            }

            // Normalize
            name = name.ToLowerInvariant();

            var len = name.Length;
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;
            var prevdash = false;

            char c;

            for (int i = 0; i < len; i++)
            {
                c = name[i];

                if (charConversions != null && _userSeoCharacterTable != null && _userSeoCharacterTable.TryGetValue(c, out string userChar))
                {
                    sb.Append(userChar);
                    prevdash = false;
                }
                else if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '/')
                {
                    if (c != '/' || allowForwardSlash)
                    {
                        sb.Append(c);
                        prevdash = (c == '_');
                    }
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else
                {
                    var category = CharUnicodeInfo.GetUnicodeCategory(c);

                    if (category >= UnicodeCategory.ConnectorPunctuation && category <= UnicodeCategory.MathSymbol)
                    {
                        if (!prevdash && sb.Length > 0)
                        {
                            sb.Append('-');
                            prevdash = true;
                        }
                    }
                    else if ((int)c >= 128)
                    {
                        var prevlen = sb.Length;
                        var c2 = c;

                        if (convertNonWesternChars)
                        {
                            c2 = c.TryRemoveDiacritic();
                        }

                        if ((allowUnicodeChars && Char.IsLetterOrDigit(c2)) || (c2 >= 'a' && c2 <= 'z'))
                        {
                            sb.Append(c2);
                        }

                        if (prevlen != sb.Length) prevdash = false;
                    }
                }

                if (i >= maxlen) break;
            }

            if (prevdash)
            {
                len = sb.Length;
                return psb.ToStringAndReturn().Substring(0, len - 1).Trim('/');
            }
            else
            {
                return psb.ToStringAndReturn().Trim('/');
            }
        }

        public static void ResetUserSeoCharacterTable()
        {
            if (_userSeoCharacterTable != null)
            {
                _userSeoCharacterTable.Clear();
                _userSeoCharacterTable = null;
            }
        }

        private static void InitializeUserSeoCharacterTable(string charConversions)
        {
            lock (_lock)
            {
                if (_userSeoCharacterTable == null)
                {
                    _userSeoCharacterTable = new Dictionary<char, string>();

                    foreach (var conversion in charConversions.SplitSafe(Environment.NewLine))
                    {
                        if (conversion.SplitToPair(out var strLeft, out var strRight, ";") && strLeft.HasValue() && !_userSeoCharacterTable.ContainsKey(strLeft[0]))
                        {
                            _userSeoCharacterTable.Add(strLeft[0], strRight);
                        }
                    }
                }
            }
        }
    }
}
