using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;

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
            if ((value >= '0') && (value <= '9'))
            {
                return (value - '0');
            }
            if ((value >= 'a') && (value <= 'f'))
            {
                return ((value - 'a') + 10);
            }
            if ((value >= 'A') && (value <= 'F'))
            {
                return ((value - 'A') + 10);
            }
            return -1;
        }

        [DebuggerStepThrough]
        public static string ToUnicode(this char c)
        {
            using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture))
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
            return (defaultValue ?? String.Empty);
        }

        [DebuggerStepThrough]
        public static string EmptyNull(this string value)
        {
            return (value ?? string.Empty).Trim();
        }

        [DebuggerStepThrough]
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
        public static string FormatCurrentUI(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentUICulture, format, objects);
        }

        [DebuggerStepThrough]
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
        public static bool IsCaseInsensitiveEqual(this string value, string comparing)
        {
            return string.Compare(value, comparing, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the string is null, empty or all whitespace.
        /// </summary>
        [DebuggerStepThrough]
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
            Guard.NotNull(value, "value");

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

        private static bool IsWebUrlInternal(this string value, bool schemeIsOptional)
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

			#region Old (obsolete)
			//// Uri.TryCreate() does not accept port numbers in uri strings.
			//if (schemeIsOptional)
			//{
			//	Uri uri;
			//	return Uri.TryCreate(value, UriKind.Absolute, out uri);
			//}

			//return RegularExpressions.IsWebUrl.IsMatch(value.Trim());
			#endregion
		}

		[DebuggerStepThrough]
		public static bool IsWebUrl(this string value)
		{
			return value.IsWebUrlInternal(false);
		}

		[DebuggerStepThrough]
		public static bool IsWebUrl(this string value, bool schemeIsOptional)
		{
			return value.IsWebUrlInternal(schemeIsOptional);
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
            Guard.NotNull(suffix, "suffix");
            Guard.IsPositive(maxLength, nameof(maxLength));

            int subStringLength = maxLength - suffix.Length;

            if (subStringLength <= 0)
                throw Error.Argument("maxLength", "Length of suffix string is greater or equal to maximumLength");

            if (value != null && value.Length > maxLength)
            {
                string truncatedString = value.Substring(0, subStringLength);
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

			var sb = new StringBuilder();
			var lines = GetLines(input.Trim(), true, removeEmptyLines).ToArray();

			foreach (var line in lines)
			{
				var len = line.Length;
				var sbLine = new StringBuilder(len);
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
			}

			return sb.ToString().Trim();
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

		///// <summary>
		///// Removes all redundant whitespace (empty lines, double space etc.).
		///// Use ~! literal to keep whitespace wherever necessary.
		///// </summary>
		///// <param name="input">Input</param>
		///// <returns>The compacted string</returns>
		//public static string Compact(this string input)
		//{
		//	Guard.NotNull(input, nameof(input));

		//	var isNewLine = false;
		//	var isBlank = false;
		//	var isChar = false;
		//	var isLiteral = false; // When we detect the ~! literal
		//	var len = input.Length;
		//	int i = 0;
		//	var eof = false;

		//	var sb = new StringBuilder();

		//	for (i = 0; i < len; i++)
		//	{
		//		var c = input[i];

		//		eof = i == len - 1;

		//		if (Char.IsWhiteSpace(c))
		//		{
		//			if (c == '\r' && !eof && input[i + 1] == '\n')
		//			{
		//				// \r\n detected, don't double-check
		//				continue;
		//			}

		//			if (c == '\r' || c == '\n')
		//			{
		//				// New line
		//				if (i > 0 && sb[sb.Length - 1] == ' ')
		//				{
		//					// If NewLine is detected, trim end (all trailing whitespace)
		//					sb.Remove(sb.Length - 1, 1);
		//				}
		//			}
		//			else
		//			{
		//				// Space, tab etc.	
		//				if (isChar)
		//				{
		//					// If last char not empty, append the space...
		//					sb.Append(' ');
		//				}
		//			}

		//			isLiteral = false;
		//			isChar = false;
		//			isBlank = true;
		//			isNewLine = c == '\r' || c == '\n';
		//		}
		//		else // No WhiteSpace
		//		{
		//			if (isNewLine)
		//			{
		//				// First non-blank char in current line: write NewLine first.
		//				sb.AppendLine();
		//			}

		//			isLiteral = c == '~' && !eof && input[i + 1] == '!';
		//			isChar = true;
		//			isNewLine = false;
		//			isBlank = false;

		//			if (isLiteral)
		//			{
		//				sb.Append(' ');
		//				i++; // skip next "!" char
		//			}
		//			else
		//			{
		//				sb.Append(c);
		//			}
		//		}
		//	}

		//	return sb.ToString();
		//}

		/// <summary>
		/// Ensure that a string starts with a string.
		/// </summary>
		/// <param name="value">The target string</param>
		/// <param name="startsWith">The string the target string should start with</param>
		/// <returns>The resulting string</returns>
		[DebuggerStepThrough]
		public static string EnsureStartsWith(this string value, string startsWith)
		{
			Guard.NotNull(value, "value");
			Guard.NotNull(startsWith, "startsWith");

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
            Guard.NotNull(value, "value");
            Guard.NotNull(endWith, "endWith");

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
        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        [DebuggerStepThrough]
        public static string UrlDecode(this string value)
        {
            return HttpUtility.UrlDecode(value);
        }

        [DebuggerStepThrough]
        public static string AttributeEncode(this string value)
        {
            return HttpUtility.HtmlAttributeEncode(value);
        }

        [DebuggerStepThrough]
        public static string HtmlEncode(this string value)
        {
            return HttpUtility.HtmlEncode(value);
        }

        [DebuggerStepThrough]
        public static string HtmlDecode(this string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

		[Obsolete("The 'removeTags' parameter is not supported anymore. Use the parameterless method instead.")]
		public static string RemoveHtml(this string source, ICollection<string> removeTags)
		{
			return RemoveHtml(source);
		}

		public static string RemoveHtml(this string source)
		{
			if (source.IsEmpty())
				return string.Empty;

			var ignoreTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "script", "style", "svg", "img" };

			var parser = new HtmlParser();
			var doc = parser.Parse(source);

			var treeWalker = doc.CreateTreeWalker(doc.Body, FilterSettings.Text);

			var sb = new StringBuilder();

			var node = treeWalker.ToNext();
			while (node != null)
			{
				if (!ignoreTags.Contains(node.Parent.NodeName))
				{
					var text = node.TextContent;
					if (text.HasValue())
					{
						sb.AppendLine(text);
					}
				}

				node = treeWalker.ToNext();
			}

			return sb.ToString().HtmlDecode();
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
            //return Regex.Replace(input, "([A-Z][a-z])", " $1", RegexOptions.Compiled).Trim();
            StringBuilder sb = new StringBuilder();
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

            return sb.ToString();
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

			// do not use separator.IsEmpty() here because whitespace like " " is a valid separator.
			// an empty separator "" returns array with value.
			if (separator == null)
			{
				separator = "|";

				if (value.IndexOf(separator) < 0)
				{
					if (value.IndexOf(';') > -1)
					{
						separator = ";";
					}
					else if (value.IndexOf(',') > -1)
					{
						separator = ",";
					}
					else if (value.IndexOf(Environment.NewLine) > -1)
					{
						separator = Environment.NewLine;
					}
				}
			}

			return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
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
            StringBuilder sb = new StringBuilder(value != null ? value.Length : 16);
            using (StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                EncodeJsString(w, value, delimiter, appendDelimiters);
                return w.ToString();
            }
        }

        [DebuggerStepThrough]
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
        public static bool IsEnclosedIn(this string value, string start, string end)
        {
            return value.IsEnclosedIn(start, end, StringComparison.CurrentCulture);
        }

        [DebuggerStepThrough]
        public static bool IsEnclosedIn(this string value, string start, string end, StringComparison comparisonType)
        {
            return value.StartsWith(start, comparisonType) && value.EndsWith(end, comparisonType);
        }

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
		
		/// <summary>Appends grow and uses delimiter if the string is not empty.</summary>
        [DebuggerStepThrough]
		public static string Grow(this string value, string grow, string delimiter) 
        {
			if (string.IsNullOrEmpty(value))
				return (string.IsNullOrEmpty(grow) ? "" : grow);

			if (string.IsNullOrEmpty(grow))
				return (string.IsNullOrEmpty(value) ? "" : value);

			return string.Format("{0}{1}{2}", value, delimiter, grow);
		}
		
		/// <summary>Returns n/a if string is empty else self.</summary>
        [DebuggerStepThrough]
		public static string NaIfEmpty(this string value) 
        {
			return (value.HasValue() ? value : "n/a");
		}

		/// <summary>Replaces substring with position x1 to x2 by replaceBy.</summary>
        [DebuggerStepThrough]
		public static string Replace(this string value, int x1, int x2, string replaceBy = null) 
        {
			if (value.HasValue() && x1 > 0 && x2 > x1 && x2 < value.Length) 
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
			Guard.NotNull(value, nameof(value));
			
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
		public static string TrimSafe(this string value) 
        {
			return (value.HasValue() ? value.Trim() : value);
		}

        [DebuggerStepThrough]
		public static string Prettify(this string value, bool allowSpace = false, char[] allowChars = null) 
        {
			string res = "";
			try 
            {
				if (value.HasValue()) 
                {
					StringBuilder sb = new StringBuilder();
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

						if ((ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 90) || (ch >= 97 && ch <= 122)) {
							sb.Append(ch);
							continue;
						}

						if (allowChars != null && allowChars.Contains(ch)) 
                        {
							sb.Append(ch);
							continue;
						}

						switch (ch) {
							case '_': sb.Append(ch); break;

							case 'ä': sb.Append("ae"); break;
							case 'ö': sb.Append("oe"); break;
							case 'ü': sb.Append("ue"); break;
							case 'ß': sb.Append("ss"); break;
							case 'Ä': sb.Append("AE"); break;
							case 'Ö': sb.Append("OE"); break;
							case 'Ü': sb.Append("UE"); break;

							case 'é':
							case 'è':
							case 'ê': sb.Append('e'); break;
							case 'á':
							case 'à':
							case 'â': sb.Append('a'); break;
							case 'ú':
							case 'ù':
							case 'û': sb.Append('u'); break;
							case 'ó':
							case 'ò':
							case 'ô': sb.Append('o'); break;
						}	// switch
					}	// for

					if (sb.Length > 0) 
                    {
						res = sb.ToString().Trim(new char[] { ' ', '-' });

						Regex pat = new Regex(@"(-{2,})");		// remove double SpaceChar
						res = pat.Replace(res, "-");
						res = res.Replace("__", "_");
					}
				}
			}
			catch (Exception exp) 
            {
				exp.Dump();
			}
			return (res.Length > 0 ? res : "null");
		}

        public static string SanitizeHtmlId(this string value)
        {
			return System.Web.Mvc.TagBuilder.CreateSanitizedId(value);
        }

		public static string Sha(this string value, Encoding encoding) 
        {
			if (value.HasValue())
            {
				using (var sha1 = new SHA1CryptoServiceProvider()) 
                {
					byte[] data = encoding.GetBytes(value);

					return sha1.ComputeHash(data).ToHexString();
				}
			}
			return "";
		}

        [DebuggerStepThrough]
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

        public static string RegexRemove(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, string.Empty, options);
        }

        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }

        [DebuggerStepThrough]
        public static string ToValidFileName(this string input, string replacement = "-")
        {
            return input.ToValidPathInternal(false, replacement);
        }

        [DebuggerStepThrough]
        public static string ToValidPath(this string input, string replacement = "-")
        {
            return input.ToValidPathInternal(true, replacement);
        }

        private static string ToValidPathInternal(this string input, bool isPath, string replacement)
        {
            var result = input.ToSafe();

            var invalidChars = new HashSet<char>(isPath ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars());

			var sb = new StringBuilder();
			foreach (var c in input)
			{
				if (invalidChars.Contains(c))
				{
					sb.Append(replacement ?? "-");
				}
				else
				{
					sb.Append(c);
				}
				result = result.Replace(c.ToString(), replacement ?? "-");
			}

			return sb.ToString();
        }

		[DebuggerStepThrough]
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

			if (input.IsEmpty() || keywords.IsEmpty())
			{
				return input;
			}

			var pattern = String.Join("|", keywords.Trim().Split(' ', '-')
				.Select(x => x.Trim())
				.Where(x => x.HasValue())
				.Select(x => Regex.Escape(x))
				.Distinct());

			if (pattern.HasValue())
			{
				var rg = new Regex(pattern, RegexOptions.IgnoreCase);
				input = rg.Replace(input, m => preMatch + m.Value + postMatch);
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
