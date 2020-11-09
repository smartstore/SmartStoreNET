using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.Html;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore
{
    public static class StringExtensions
    {
        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        private delegate void ActionLine(TextWriter textWriter, string line);

        #region Char extensions

        [DebuggerStepThrough]
        public static int ToInt(this char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }
            else if (value >= 'a' && value <= 'f')
            {
                return (value - 'a') + 10;
            }
            else if (value >= 'A' && value <= 'F')
            {
                return (value - 'A') + 10;
            }

            return -1;
        }

        [DebuggerStepThrough]
        public static string ToUnicode(this char c)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                WriteCharAsUnicode(c, w);
                return w.ToString();
            }
        }

        internal static void WriteCharAsUnicode(char c, TextWriter writer)
        {
            Guard.NotNull(writer, "writer");

            char h1 = ((c >> 12) & '\x000f').ToHex();
            char h2 = ((c >> 8) & '\x000f').ToHex();
            char h3 = ((c >> 4) & '\x000f').ToHex();
            char h4 = (c & '\x000f').ToHex();

            writer.Write('\\');
            writer.Write('u');
            writer.Write(h1);
            writer.Write(h2);
            writer.Write(h3);
            writer.Write(h4);
        }

        public static char TryRemoveDiacritic(this char c)
        {
            var normalized = c.ToString().Normalize(NormalizationForm.FormD);
            if (normalized.Length > 1)
            {
                return normalized[0];
            }

            return c;
        }

        #endregion

        #region String extensions

        [DebuggerStepThrough]
        public static T ToEnum<T>(this string value, T defaultValue)
        {
            if (!value.HasValue())
            {
                return defaultValue;
            }
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        [DebuggerStepThrough]
        public static string ToSafe(this string value, string defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return (defaultValue ?? string.Empty);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EmptyNull(this string value)
        {
            return (value ?? string.Empty).Trim();
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NullEmpty(this string value)
        {
            return (string.IsNullOrEmpty(value)) ? null : value;
        }

        /// <summary>
        /// Formats a string to an invariant culture
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatInvariant(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.InvariantCulture, format, objects);
        }

        /// <summary>
        /// Formats a string to the current culture.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatCurrent(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentCulture, format, objects);
        }

        /// <summary>
        /// Formats a string to the current UI culture.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatCurrentUI(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentUICulture, format, objects);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, params object[] args)
        {
            return FormatWith(format, CultureInfo.CurrentCulture, args);
        }

        [DebuggerStepThrough]
        public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, format, args);
        }

        /// <summary>
        /// Determines whether this instance and another specified System.String object have the same value.
        /// </summary>
        /// <param name="value">The string to check equality.</param>
        /// <param name="comparing">The comparing with string.</param>
        /// <returns>
        /// <c>true</c> if the value of the comparing parameter is the same as this string; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCaseSensitiveEqual(this string value, string comparing)
        {
            return string.CompareOrdinal(value, comparing) == 0;
        }

        /// <summary>
        /// Determines whether this instance and another specified System.String object have the same value.
        /// </summary>
        /// <param name="value">The string to check equality.</param>
        /// <param name="comparing">The comparing with string.</param>
        /// <returns>
        /// <c>true</c> if the value of the comparing parameter is the same as this string; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCaseInsensitiveEqual(this string value, string comparing)
        {
            return string.Compare(value, comparing, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the string is null, empty or all whitespace.
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Determines whether the string is all white space. Empty string will return false.
        /// </summary>
        /// <param name="value">The string to test whether it is all white space.</param>
        /// <returns>
        /// 	<c>true</c> if the string is all white space; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsWhiteSpace(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <remarks>to get equivalent result to PHPs md5 function call Hash("my value", Encoding.ASCII, false).</remarks>
        [DebuggerStepThrough]
        public static string Hash(this string value, Encoding encoding, bool toBase64 = false)
        {
            if (value.IsEmpty())
                return value;

            using (var md5 = MD5.Create())
            {
                byte[] data = encoding.GetBytes(value);

                if (toBase64)
                {
                    byte[] hash = md5.ComputeHash(data);
                    return Convert.ToBase64String(hash);
                }
                else
                {
                    return md5.ComputeHash(data).ToHexString().ToLower();
                }
            }
        }

        /// <summary>
        /// Mask by replacing characters with asterisks.
        /// </summary>
        /// <param name="value">The string</param>
        /// <param name="length">Number of characters to leave untouched.</param>
        /// <returns>The mask string</returns>
        [DebuggerStepThrough]
        public static string Mask(this string value, int length)
        {
            if (value.HasValue())
                return value.Substring(0, length) + new String('*', value.Length - length);

            return value;
        }

        private static bool IsWebUrlInternal(string value, bool schemeIsOptional)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            if (schemeIsOptional && value.StartsWith("//"))
            {
                value = "http:" + value;
            }

            return Uri.IsWellFormedUriString(value, UriKind.Absolute) &&
                (value.StartsWith("http://") || value.StartsWith("https://") || value.StartsWith("ftp://"));
        }

        [DebuggerStepThrough]
        public static bool IsWebUrl(this string value)
        {
            return IsWebUrlInternal(value, false);
        }

        [DebuggerStepThrough]
        public static bool IsWebUrl(this string value, bool schemeIsOptional)
        {
            return IsWebUrlInternal(value, schemeIsOptional);
        }

        [DebuggerStepThrough]
        public static bool IsEmail(this string value)
        {
            return !String.IsNullOrEmpty(value) && RegularExpressions.IsEmail.IsMatch(value.Trim());
        }

        [DebuggerStepThrough]
        public static bool IsNumeric(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            return !RegularExpressions.IsNotNumber.IsMatch(value) &&
                   !RegularExpressions.HasTwoDot.IsMatch(value) &&
                   !RegularExpressions.HasTwoMinus.IsMatch(value) &&
                   RegularExpressions.IsNumeric.IsMatch(value);
        }

        /// <summary>
        /// Ensures that a string only contains numeric values
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Input string with only numeric values, empty string if input is null or empty</returns>
        [DebuggerStepThrough]
        public static string EnsureNumericOnly(this string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            return new String(str.Where(c => Char.IsDigit(c)).ToArray());
        }

        [DebuggerStepThrough]
        public static bool IsAlpha(this string value)
        {
            return RegularExpressions.IsAlpha.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static bool IsAlphaNumeric(this string value)
        {
            return RegularExpressions.IsAlphaNumeric.IsMatch(value);
        }

        [DebuggerStepThrough]
        public static string Truncate(this string value, int maxLength, string suffix = "")
        {
            if (suffix == null)
                throw new ArgumentNullException(nameof(suffix));

            Guard.IsPositive(maxLength, nameof(maxLength));

            int subStringLength = maxLength - suffix.Length;

            if (subStringLength <= 0)
                throw Error.Argument(nameof(maxLength), "Length of suffix string is greater or equal to maximumLength");

            if (value != null && value.Length > maxLength)
            {
                var truncatedString = value.Substring(0, subStringLength);
                // in case the last character is a space
                truncatedString = truncatedString.Trim();
                truncatedString += suffix;

                return truncatedString;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Removes all redundant whitespace (empty lines, double space etc.).
        /// Use ~! literal to keep whitespace wherever necessary.
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>The compacted string</returns>
        public static string Compact(this string input, bool removeEmptyLines = false)
        {
            Guard.NotNull(input, nameof(input));

            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;
            var lines = GetLines(input.Trim(), true, removeEmptyLines).ToArray();

            foreach (var line in lines)
            {
                var len = line.Length;
                var psbLine = PooledStringBuilder.Rent();
                var sbLine = (StringBuilder)psbLine;
                var isChar = false;
                var isLiteral = false; // When we detect the ~! literal
                int i = 0;
                var eof = false;

                for (i = 0; i < len; i++)
                {
                    var c = line[i];

                    eof = i == len - 1;

                    if (Char.IsWhiteSpace(c))
                    {
                        // Space, Tab etc.
                        if (isChar)
                        {
                            // If last char not empty, append the space.
                            sbLine.Append(' ');
                        }

                        isLiteral = false;
                        isChar = false;
                    }
                    else
                    {
                        // Char or Literal (~!)

                        isLiteral = c == '~' && !eof && line[i + 1] == '!';
                        isChar = true;

                        if (isLiteral)
                        {
                            sbLine.Append(' ');
                            i++; // skip next "!" char
                        }
                        else
                        {
                            sbLine.Append(c);
                        }
                    }
                }

                // Append the compacted and trimmed line
                sb.AppendLine(sbLine.ToString().Trim().Trim(','));
                psbLine.Return();
            }

            return psb.ToStringAndReturn().Trim();
        }

        /// <summary>
        /// Splits the input string by carriage return.
        /// </summary>
        /// <param name="input">The string to split</param>
        /// <returns>A sequence with string items per line</returns>
        public static IEnumerable<string> GetLines(this string input, bool trimLines = false, bool removeEmptyLines = false)
        {
            if (input.IsEmpty())
            {
                yield break;
            }

            using (var sr = new StringReader(input))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (trimLines)
                    {
                        line = line.Trim();
                    }

                    if (removeEmptyLines && IsEmpty(line))
                    {
                        continue;
                    }

                    yield return line;
                }
            }
        }

        /// <summary>
        /// Ensure that a string starts with a string.
        /// </summary>
        /// <param name="value">The target string</param>
        /// <param name="startsWith">The string the target string should start with</param>
        /// <returns>The resulting string</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureStartsWith(this string value, string startsWith)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (startsWith == null)
                throw new ArgumentNullException(nameof(startsWith));

            return value.StartsWith(startsWith) ? value : (startsWith + value);
        }

        /// <summary>
        /// Ensures the target string ends with the specified string.
        /// </summary>
        /// <param name="endWith">The target.</param>
        /// <param name="value">The value.</param>
        /// <returns>The target string with the value string at the end.</returns>
        [DebuggerStepThrough]
        public static string EnsureEndsWith(this string value, string endWith)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (endWith == null)
                throw new ArgumentNullException(nameof(endWith));

            if (value.Length >= endWith.Length)
            {
                if (string.Compare(value, value.Length - endWith.Length, endWith, 0, endWith.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return value;

                string trimmedString = value.TrimEnd(null);

                if (string.Compare(trimmedString, trimmedString.Length - endWith.Length, endWith, 0, endWith.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return value;
            }

            return value + endWith;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string UrlDecode(this string value)
        {
            return HttpUtility.UrlDecode(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AttributeEncode(this string value)
        {
            return HttpUtility.HtmlAttributeEncode(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string HtmlEncode(this string value)
        {
            return HttpUtility.HtmlEncode(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string HtmlDecode(this string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        [Obsolete("The 'removeTags' parameter is not supported anymore. Use the parameterless method instead.")]
        public static string RemoveHtml(this string source, ICollection<string> removeTags)
        {
            return RemoveHtml(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveHtml(this string source)
        {
            return HtmlUtils.StripTags(source).Trim().HtmlDecode();
        }

        /// <summary>
        /// Replaces pascal casing with spaces. For example "CustomerId" would become "Customer Id".
        /// Strings that already contain spaces are ignored.
        /// </summary>
        /// <param name="value">String to split</param>
        /// <returns>The string after being split</returns>
        [DebuggerStepThrough]
        public static string SplitPascalCase(this string value)
        {
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;
            char[] ca = value.ToCharArray();

            sb.Append(ca[0]);

            for (int i = 1; i < ca.Length - 1; i++)
            {
                char c = ca[i];
                if (char.IsUpper(c) && (char.IsLower(ca[i + 1]) || char.IsLower(ca[i - 1])))
                {
                    sb.Append(" ");
                }
                sb.Append(c);
            }

            if (ca.Length > 1)
            {
                sb.Append(ca[ca.Length - 1]);
            }

            return psb.ToStringAndReturn();
        }

        /// <summary>
        /// Splits a string into a string array
        /// </summary>
        /// <param name="value">String value to split</param>
        /// <param name="separator">If <c>null</c> then value is searched for a common delimiter like pipe, semicolon or comma</param>
        /// <returns>String array</returns>
        [DebuggerStepThrough]
        public static string[] SplitSafe(this string value, string separator)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];

            // Do not use separator.IsEmpty() here because whitespace like " " is a valid separator.
            // an empty separator "" returns array with value.
            if (separator == null)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var c = value[i];
                    if (c == ';' || c == ',' || c == '|')
                    {
                        return value.Split(new char[] { c }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (c == '\r' && (i + 1) < value.Length & value[i + 1] == '\n')
                    {
                        return value.GetLines(false, true).ToArray();
                    }
                }

                return new string[] { value };
            }
            else
            {
                return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>Splits a string into two strings</summary>
        /// <returns>true: success, false: failure</returns>
        [DebuggerStepThrough]
        [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
        public static bool SplitToPair(this string value, out string leftPart, out string rightPart, string delimiter, bool splitAfterLast = false)
        {
            leftPart = value;
            rightPart = "";

            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(delimiter))
            {
                return false;
            }

            var idx = splitAfterLast
                ? value.LastIndexOf(delimiter)
                : value.IndexOf(delimiter);

            if (idx == -1)
            {
                return false;
            }

            leftPart = value.Substring(0, idx);
            rightPart = value.Substring(idx + delimiter.Length);

            return true;
        }

        [DebuggerStepThrough]
        public static string EncodeJsString(this string value)
        {
            return EncodeJsString(value, '"', true);
        }

        [DebuggerStepThrough]
        public static string EncodeJsString(this string value, char delimiter, bool appendDelimiters)
        {
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;
            using (var w = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                EncodeJsString(w, value, delimiter, appendDelimiters);
                var result = w.ToString();
                psb.Return();
                return result;
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnclosedIn(this string value, string enclosedIn)
        {
            return value.IsEnclosedIn(enclosedIn, StringComparison.CurrentCulture);
        }

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string enclosedIn, StringComparison comparisonType)
        {
            if (String.IsNullOrEmpty(enclosedIn))
                return false;

            if (enclosedIn.Length == 1)
                return value.IsEnclosedIn(enclosedIn, enclosedIn, comparisonType);

            if (enclosedIn.Length % 2 == 0)
            {
                int len = enclosedIn.Length / 2;
                return value.IsEnclosedIn(
                    enclosedIn.Substring(0, len),
                    enclosedIn.Substring(len, len),
                    comparisonType);

            }

            return false;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnclosedIn(this string value, string start, string end)
        {
            return value.IsEnclosedIn(start, end, StringComparison.CurrentCulture);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnclosedIn(this string value, string start, string end, StringComparison comparisonType)
        {
            return value.StartsWith(start, comparisonType) && value.EndsWith(end, comparisonType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveEncloser(this string value, string encloser)
        {
            return value.RemoveEncloser(encloser, StringComparison.CurrentCulture);
        }

        public static string RemoveEncloser(this string value, string encloser, StringComparison comparisonType)
        {
            if (value.IsEnclosedIn(encloser, comparisonType))
            {
                int len = encloser.Length / 2;
                return value.Substring(
                    len,
                    value.Length - (len * 2));
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveEncloser(this string value, string start, string end)
        {
            return value.RemoveEncloser(start, end, StringComparison.CurrentCulture);
        }

        public static string RemoveEncloser(this string value, string start, string end, StringComparison comparisonType)
        {
            if (value.IsEnclosedIn(start, end, comparisonType))
                return value.Substring(
                    start.Length,
                    value.Length - (start.Length + end.Length));

            return value;
        }

        /// <summary>Debug.WriteLine</summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dump(this string value, bool appendMarks = false)
        {
            Debug.WriteLine(value);
            Debug.WriteLineIf(appendMarks, "------------------------------------------------");
        }

        /// <summary>Smart way to create a HTML attribute with a leading space.</summary>
        /// <param name="value">Name of the attribute.</param>
        /// <param name="name"></param>
        /// <param name="htmlEncode"></param>
        [SuppressMessage("ReSharper", "StringCompareIsCultureSpecific.3")]
        public static string ToAttribute(this string value, string name, bool htmlEncode = true)
        {
            if (name.IsEmpty())
                return "";

            if (value == "" && name != "value" && !name.StartsWith("data"))
                return "";

            if (name == "maxlength" && (value == "" || value == "0"))
                return "";

            if (name == "checked" || name == "disabled" || name == "multiple")
            {
                if (value == "" || string.Compare(value, "false", true) == 0)
                    return "";
                value = (string.Compare(value, "true", true) == 0 ? name : value);
            }

            if (name.StartsWith("data"))
                name = name.Insert(4, "-");

            return string.Format(" {0}=\"{1}\"", name, htmlEncode ? HttpUtility.HtmlEncode(value) : value);
        }

        /// <summary>
        /// Appends grow and uses delimiter if the string is not empty.
        /// </summary>
        [DebuggerStepThrough]
        public static string Grow(this string value, string grow, string delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return (string.IsNullOrEmpty(grow) ? "" : grow);

            if (string.IsNullOrEmpty(grow))
                return (string.IsNullOrEmpty(value) ? "" : value);

            return value + delimiter + grow;
        }

        /// <summary>
        /// Left-pads a string. Always returns empty string if source is null or empty.
        /// </summary>
        [DebuggerStepThrough]
        public static string LeftPad(this string value, string format = null, char pad = ' ', int count = 1)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            Guard.NotNull(pad, nameof(pad));

            if (count < 1)
                return value;

            var left = new String(pad, count);
            var right = value;

            if (!string.IsNullOrWhiteSpace(format))
            {
                right = string.Format(CultureInfo.InvariantCulture, format, value);
            }

            return left + right;
        }

        /// <summary>
        /// Returns n/a if string is empty else self.
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NaIfEmpty(this string value)
        {
            return (string.IsNullOrWhiteSpace(value) ? "n/a" : value);
        }

        /// <summary>
        /// Replaces substring with position x1 to x2 by replaceBy.
        /// </summary>
        [DebuggerStepThrough]
        public static string Replace(this string value, int x1, int x2, string replaceBy = null)
        {
            if (!string.IsNullOrWhiteSpace(value) && x1 > 0 && x2 > x1 && x2 < value.Length)
            {
                return value.Substring(0, x1) + (replaceBy.EmptyNull()) + value.Substring(x2 + 1);
            }

            return value;
        }

        [DebuggerStepThrough]
        public static string Replace(this string value, string oldValue, string newValue, StringComparison comparisonType)
        {
            try
            {
                int startIndex = 0;
                while (true)
                {
                    startIndex = value.IndexOf(oldValue, startIndex, comparisonType);
                    if (startIndex == -1)
                        break;

                    value = value.Substring(0, startIndex) + newValue + value.Substring(startIndex + oldValue.Length);

                    startIndex += newValue.Length;
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return value;
        }

        /// <summary>
        /// Replaces digits in a string with culture native digits (if digit substitution for culture is required)
        /// </summary>
        [DebuggerStepThrough]
        public static string ReplaceNativeDigits(this string value, IFormatProvider provider = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            provider = provider ?? NumberFormatInfo.CurrentInfo;
            var nfi = NumberFormatInfo.GetInstance(provider);

            if (nfi.DigitSubstitution == DigitShapes.None)
            {
                return value;
            }

            var nativeDigits = nfi.NativeDigits;
            var rg = new Regex(@"\d");

            var result = rg.Replace(value, m => nativeDigits[m.Value.ToInt()]);
            return result;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimSafe(this string value)
        {
            return (value.HasValue() ? value.Trim() : value);
        }

        [DebuggerStepThrough]
        public static string Slugify(this string value, bool allowSpace = false, char[] allowChars = null)
        {
            string res = string.Empty;
            var psb = PooledStringBuilder.Rent();

            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var sb = (StringBuilder)psb;
                    bool space = false;
                    char ch;

                    for (int i = 0; i < value.Length; ++i)
                    {
                        ch = value[i];

                        if (ch == ' ' || ch == '-')
                        {
                            if (allowSpace && ch == ' ')
                                sb.Append(' ');
                            else if (!space)
                                sb.Append('-');
                            space = true;
                            continue;
                        }

                        space = false;

                        if ((ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 90) || (ch >= 97 && ch <= 122) || ch == '_')
                        {
                            sb.Append(ch);
                            continue;
                        }

                        if (allowChars != null && allowChars.Contains(ch))
                        {
                            sb.Append(ch);
                            continue;
                        }

                        if ((int)ch >= 128)
                        {
                            switch (ch)
                            {
                                case 'ä': sb.Append("ae"); break;
                                case 'ö': sb.Append("oe"); break;
                                case 'ü': sb.Append("ue"); break;
                                case 'ß': sb.Append("ss"); break;
                                case 'Ä': sb.Append("AE"); break;
                                case 'Ö': sb.Append("OE"); break;
                                case 'Ü': sb.Append("UE"); break;
                                default:
                                    var c2 = ch.TryRemoveDiacritic();
                                    if ((c2 >= 'a' && c2 <= 'z') || (c2 >= 'A' && c2 <= 'Z'))
                                    {
                                        sb.Append(c2);
                                    }
                                    break;
                            }
                        }
                    }   // for

                    if (sb.Length > 0)
                    {
                        res = sb.ToString().Trim(new char[] { ' ', '-' });

                        Regex pat = new Regex(@"(-{2,})"); // remove double SpaceChar
                        res = pat.Replace(res, "-");
                        res = res.Replace("__", "_");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Dump();
            }
            finally
            {
                psb.Return();
            }

            return (res.Length > 0 ? res : "null");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SanitizeHtmlId(this string value)
        {
            return System.Web.Mvc.TagBuilder.CreateSanitizedId(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.IsMatch(input, pattern, options);
        }

        [DebuggerStepThrough]
        public static bool IsMatch(this string input, string pattern, out Match match, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            match = Regex.Match(input, pattern, options);
            return match.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RegexRemove(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, string.Empty, options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToValidFileName(this string input, string replacement = "-")
        {
            return string.Join(
                replacement ?? "-",
                input.ToSafe().Split(Path.GetInvalidFileNameChars()));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToValidPath(this string input, string replacement = "-")
        {
            return string.Join(
                replacement ?? "-",
                input.ToSafe().Split(Path.GetInvalidPathChars()));
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToIntArray(this string s)
        {
            return Array.ConvertAll(s.SplitSafe(","), v => int.Parse(v.Trim()));
        }

        [DebuggerStepThrough]
        public static bool ToIntArrayContains(this string s, int value, bool defaultValue)
        {
            if (s == null)
                return defaultValue;
            var arr = s.ToIntArray();
            if (arr == null || arr.Count() <= 0)
                return defaultValue;

            return arr.Contains(value);
        }

        [DebuggerStepThrough]
        public static string RemoveInvalidXmlChars(this string s)
        {
            if (s.IsEmpty())
                return s;

            return Regex.Replace(s, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
        }

        [DebuggerStepThrough]
        public static string ReplaceCsvChars(this string s)
        {
            if (s.IsEmpty())
            {
                return "";
            }

            s = s.Replace(';', ',');
            s = s.Replace('\r', ' ');
            s = s.Replace('\n', ' ');
            return s.Replace("'", "");
        }

        [DebuggerStepThrough]
        public static string HighlightKeywords(this string input, string keywords, string preMatch = "<strong>", string postMatch = "</strong>")
        {
            Guard.NotNull(preMatch, nameof(preMatch));
            Guard.NotNull(postMatch, nameof(postMatch));

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(keywords))
            {
                return input;
            }

            var pattern = String.Join("|", keywords.Trim().Split(' ', '-')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Regex.Escape(x))
                .Distinct());

            if (!string.IsNullOrWhiteSpace(pattern))
            {
                var rg = new Regex(pattern, RegexOptions.IgnoreCase);
                input = rg.Replace(input, m => preMatch + m.Value.EmptyNull().HtmlEncode() + postMatch);
            }

            return input;
        }

        #endregion

        #region Helper

        private static void EncodeJsChar(TextWriter writer, char c, char delimiter)
        {
            switch (c)
            {
                case '\t':
                    writer.Write(@"\t");
                    break;
                case '\n':
                    writer.Write(@"\n");
                    break;
                case '\r':
                    writer.Write(@"\r");
                    break;
                case '\f':
                    writer.Write(@"\f");
                    break;
                case '\b':
                    writer.Write(@"\b");
                    break;
                case '\\':
                    writer.Write(@"\\");
                    break;
                //case '<':
                //case '>':
                //case '\'':
                //  StringUtils.WriteCharAsUnicode(writer, c);
                //  break;
                case '\'':
                    // only escape if this charater is being used as the delimiter
                    writer.Write((delimiter == '\'') ? @"\'" : @"'");
                    break;
                case '"':
                    // only escape if this charater is being used as the delimiter
                    writer.Write((delimiter == '"') ? "\\\"" : @"""");
                    break;
                default:
                    if (c > '\u001f')
                        writer.Write(c);
                    else
                        WriteCharAsUnicode(c, writer);
                    break;
            }
        }

        private static void EncodeJsString(TextWriter writer, string value, char delimiter, bool appendDelimiters)
        {
            // leading delimiter
            if (appendDelimiters)
                writer.Write(delimiter);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    EncodeJsChar(writer, value[i], delimiter);
                }
            }

            // trailing delimiter
            if (appendDelimiters)
                writer.Write(delimiter);
        }

        #endregion
    }

}
