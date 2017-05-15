using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Utilities;

namespace SmartStore
{
	public static class ConversionExtensions
    {
        #region Object

        public static T Convert<T>(this object value)
        {
			return (T)(Convert(value, typeof(T)) ?? default(T));
		}

		public static T Convert<T>(this object value, T defaultValue)
		{
			return (T)(Convert(value, typeof(T)) ?? defaultValue);
		}

		public static T Convert<T>(this object value, CultureInfo culture)
        {
			return (T)(Convert(value, typeof(T), culture) ?? default(T));
		}

		public static T Convert<T>(this object value, T defaultValue, CultureInfo culture)
		{
			return (T)(Convert(value, typeof(T), culture) ?? defaultValue);
		}

		public static object Convert(this object value, Type to)
        {
			return value.Convert(to, CultureInfo.InvariantCulture);
        }

		public static object Convert(this object value, Type to, CultureInfo culture)
		{
			Guard.NotNull(to, nameof(to));

			if (value == null || value == DBNull.Value || to.IsInstanceOfType(value))
			{
				return value == DBNull.Value ? null : value;
			}

			Type from = value.GetType();

			if (culture == null)
			{
				culture = CultureInfo.InvariantCulture;
            }

			// get a converter for 'to' (value -> to)
			var converter = TypeConverterFactory.GetConverter(to);
			if (converter != null && converter.CanConvertFrom(from))
			{
				return converter.ConvertFrom(culture, value);
			}

			// try the other way round with a 'from' converter (to <- from)
			converter = TypeConverterFactory.GetConverter(from);
			if (converter != null && converter.CanConvertTo(to))
			{
				return converter.ConvertTo(culture, null, value, to);
			}

			// use Convert.ChangeType if both types are IConvertible
			if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(to))
			{
				return System.Convert.ChangeType(value, to, culture);
			}

			throw Error.InvalidCast(from, to);
		}

        #endregion

        #region int

        public static char ToHex(this int value)
        {
            if (value <= 9)
            {
                return (char)(value + 48);
            }
            return (char)((value - 10) + 97);
        }

        #endregion

        #region String

        public static int ToInt(this string value, int defaultValue = 0)
        {
            int result;
			if (CommonHelper.TryConvert(value, out result))
			{
				return result;
			}

			return defaultValue;
        }

		public static char ToChar(this string value, bool unescape = false, char defaultValue = '\0')
		{
			char result;
			if (value.HasValue() && char.TryParse(unescape ? Regex.Unescape(value) : value, out result))
			{
				return result;
			}
			return defaultValue;
		}

		public static float ToFloat(this string value, float defaultValue = 0)
        {
            float result;
			if (CommonHelper.TryConvert(value, out result))
			{
				return result;
			}

			return defaultValue;
        }

        public static bool ToBool(this string value, bool defaultValue = false)
        {
            bool result;
			if (CommonHelper.TryConvert(value, out result))
			{
				return result;
			}

			return defaultValue;
        }

        public static DateTime? ToDateTime(this string value, DateTime? defaultValue)
        {
            return value.ToDateTime(null, defaultValue);
        }

        public static DateTime? ToDateTime(this string value, string[] formats, DateTime? defaultValue)
        {
            return value.ToDateTime(formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, defaultValue);
        }

        public static DateTime? ToDateTime(this string value, string[] formats, IFormatProvider provider, DateTimeStyles styles, DateTime? defaultValue)
        {
            DateTime result;

            if (formats.IsNullOrEmpty())
            {
                if (DateTime.TryParse(value, provider, styles, out result))
                {
                    return result;
                }
            }

            if (DateTime.TryParseExact(value, formats, provider, styles, out result))
            {
                return result;
            }

            return defaultValue;
        }

		/// <summary>
		/// Parse ISO-8601 UTC timestamp including milliseconds.
		/// </summary>
		/// <remarks>
		/// Dublicate can be found in HmacAuthentication class.
		/// </remarks>
		public static DateTime? ToDateTimeIso8601(this string value)
		{
			if (value.HasValue())
			{
				DateTime dt;
				if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
					return dt;

				if (DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
					return dt;
			}
			return null;
		}

        [DebuggerStepThrough]
        public static Version ToVersion(this string value, Version defaultVersion = null)
        {
            try
            {
                return new Version(value);
            }
            catch
            {
                return defaultVersion ?? new Version("1.0");
            }
        }

        #endregion

        #region Stream

        public static byte[] ToByteArray(this Stream stream)
        {
			Guard.NotNull(stream, nameof(stream));

			if (stream is MemoryStream)
			{
				return ((MemoryStream)stream).ToArray();
			}
			else
			{
				using (var streamReader = new MemoryStream())
				{
					stream.CopyTo(streamReader);
					return streamReader.ToArray();
				}
			}
		}

		public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
		{
			Guard.NotNull(stream, nameof(stream));

			if (stream is MemoryStream)
			{
				return ((MemoryStream)stream).ToArray();
			}
			else
			{
				using (var streamReader = new MemoryStream())
				{
					await stream.CopyToAsync(streamReader);
					return streamReader.ToArray();
				}
			}
		}

		public static string AsString(this Stream stream)
		{
			return stream.AsString(Encoding.UTF8);
		}

		public static Task<string> AsStringAsync(this Stream stream)
		{
			return stream.AsStringAsync(Encoding.UTF8);
		}

		public static string AsString(this Stream stream, Encoding encoding)
        {
			Guard.NotNull(encoding, nameof(encoding));

			// convert stream to string
			string result;

			if (stream.CanSeek)
			{
				stream.Position = 0;
			}

            using (StreamReader sr = new StreamReader(stream, encoding))
            {
                result = sr.ReadToEnd();
            }

            return result;
        }

		public static Task<string> AsStringAsync(this Stream stream, Encoding encoding)
		{
			Guard.NotNull(encoding, nameof(encoding));

			// convert stream to string
			Task<string> result;

			if (stream.CanSeek)
			{
				stream.Position = 0;
			}

			using (StreamReader sr = new StreamReader(stream, encoding))
			{
				result = sr.ReadToEndAsync();
			}

			return result;
		}

		#endregion

		#region ByteArray

		/// <summary>
		/// Converts a byte array into an object.
		/// </summary>
		/// <param name="bytes">Object to deserialize. May be null.</param>
		/// <returns>Deserialized object, or null if input was null.</returns>
		public static object ToObject(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }

        public static Stream ToStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

		/// <summary>
		/// Computes the MD5 hash of a byte array
		/// </summary>
		/// <param name="value">The byte array to compute the hash for</param>
		/// <returns>The hash value</returns>
		[DebuggerStepThrough]
		public static string Hash(this byte[] value, bool toBase64 = false)
        {
			Guard.NotNull(value, nameof(value));

			using (MD5 md5 = MD5.Create())
            {

                if (toBase64)
                {
                    byte[] hash = md5.ComputeHash(value);
                    return System.Convert.ToBase64String(hash);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    byte[] hashBytes = md5.ComputeHash(value);
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2").ToLower());
                    }

                    return sb.ToString();
                }
            }
        }

		/// <summary>
		/// Compresses the input buffer with <see cref="GZipStream"/>
		/// </summary>
		/// <param name="buffer">Decompressed input</param>
		/// <returns>The compressed result</returns>
		public static byte[] Zip(this byte[] buffer)
		{
			Guard.NotNull(buffer, nameof(buffer));

			using (var compressedStream = new MemoryStream())
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
			{
				zipStream.Write(buffer, 0, buffer.Length);
				zipStream.Close();
				return compressedStream.ToArray();
			}
		}

		/// <summary>
		/// Decompresses the input buffer with <see cref="GZipStream"/> decompression
		/// </summary>
		/// <param name="buffer">Compressed input</param>
		/// <returns>The decompressed result</returns>
		public static byte[] UnZip(this byte[] buffer)
		{
			Guard.NotNull(buffer, nameof(buffer));

			using (var compressedStream = new MemoryStream(buffer))
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			using (var resultStream = new MemoryStream())
			{
				zipStream.CopyTo(resultStream);
				return resultStream.ToArray();
			}
		}

		#endregion

		#region Enumerable: Collections/List/Dictionary...

		public static T ToObject<T>(this IDictionary<string, object> values) where T : class
        {
            return (T)values.ToObject(typeof(T));
        }

        public static object ToObject(this IDictionary<string, object> values, Type objectType)
        {
            Guard.NotEmpty(values, nameof(values));
            Guard.NotNull(objectType, nameof(objectType));

            if (!DictionaryConverter.CanCreateType(objectType))
            {
                throw Error.Argument(
                    "objectType",
                    "The type '{0}' must be a class and have a parameterless default constructor in order to deserialize properly.",
                    objectType.FullName);
            }

            return DictionaryConverter.SafeCreateAndPopulate(objectType, values);
        }

        #endregion

    }

}
