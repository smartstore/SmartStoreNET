using System;
using System.Globalization;
using System.Xml.XPath;
using SmartStore.Core.Domain.Localization;

namespace SmartStore
{
    public static class XPathExtensions
    {
        /// <summary>
        /// Safe way to get the value of a xpath navigator item
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="node">The node</param>
        /// <param name="xpath">Optional xpath</param>
        /// <param name="defaultValue">Optional default value</param>
        /// <param name="language">For localized values. Xpath scheme: Localized/xpath[@culture = 'language.LanguageCulture']</param>
        /// <param name="culture">If null is passed, CultureInfo.InvariantCulture is used.</param>
        /// <returns>The value</returns>
        public static T GetValue<T>(this XPathNavigator node, string xpath = null, T defaultValue = default(T), Language language = null, CultureInfo culture = null)
        {
            try
            {
                if (node != null)
                {
                    if (xpath.IsEmpty())
                    {
                        if (node.Value.HasValue())
                            return node.Value.Convert<T>(culture);

                        return defaultValue;
                    }

                    if (language != null)
                        xpath = "Localized/{0}[@culture = '{1}']".FormatWith(xpath, language.LanguageCulture.EmptyNull().ToLower());

                    var n = node.SelectSingleNode(xpath);

                    if (n != null && n.Value.HasValue())
                        return n.Value.Convert<T>(culture);
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return defaultValue;
        }

        /// <summary>
        /// Safe way to get a string value of a xpath navigator item
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="xpath">Optional xpath</param>
        /// <param name="defaultValue">Optional default value</param>
        /// <returns>The value</returns>
        public static string GetString(this XPathNavigator node, string xpath = null, string defaultValue = null)
        {
            // note: better not merge GetString(...) and GetValue<T>(...)
            try
            {
                if (node != null)
                {
                    if (xpath.IsEmpty())
                        return node.Value;

                    var n = node.SelectSingleNode(xpath);

                    if (n != null)
                        return n.Value;
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return defaultValue;
        }

        /// <summary>
        /// Safe way to get a localized string value of a xpath navigator item
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="language">For localized values. Xpath scheme: Localized/xpath[@culture = 'language.LanguageCulture']</param>
        /// <param name="xpath">Optional xpath</param>
        /// <param name="defaultValue">Optional default value</param>
        /// <returns>The value</returns>
        public static string GetString(this XPathNavigator node, Language language, string xpath, string defaultValue = null)
        {
            return node.GetString(
                "Localized/{0}[@culture = '{1}']".FormatWith(xpath.EmptyNull(), language.LanguageCulture.EmptyNull().ToLower()),
                defaultValue);
        }
    }
}
