using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.ComponentModel;

namespace SmartStore.Utilities
{
    public static partial class CommonHelper
    {
        private static bool? _isDevEnvironment;

        [ThreadStatic]
        private static Random _random;

        private static Random GetRandomizer()
        {
            if (_random == null)
            {
                _random = new Random();
            }

            return _random;
        }

        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var buffer = new int[length];
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = GetRandomizer().Next(10);
            }

            return string.Join("", buffer);
        }

        /// <summary>
        /// Returns a random number within the range <paramref name="min"/> to <paramref name="max"/> - 1.
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number (exclusive!).</param>
        /// <returns>Random integer number.</returns>
        public static int GenerateRandomInteger(int min = 0, int max = 2147483647)
        {
            var randomNumberBuffer = new byte[10];
            new RNGCryptoServiceProvider().GetBytes(randomNumberBuffer);
            return new Random(BitConverter.ToInt32(randomNumberBuffer, 0)).Next(min, max);
        }

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <param name="findAppRoot">Specifies if the app root should be resolved when mapped directory does not exist</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        /// <remarks>
        /// This method is able to resolve the web application root
        /// even when it's called during design-time (e.g. from EF design-time tools).
        /// </remarks>
        public static string MapPath(string path, bool findAppRoot = true)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (HostingEnvironment.IsHosted)
            {
                // hosted
                return HostingEnvironment.MapPath(path);
            }
            else
            {
                // not hosted. For example, running in unit tests or EF tooling
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');

                var testPath = Path.Combine(baseDirectory, path);

                if (findAppRoot /* && !Directory.Exists(testPath)*/)
                {
                    // most likely we're in unit tests or design-mode (EF migration scaffolding)...
                    // find solution root directory first
                    var dir = FindSolutionRoot(baseDirectory);

                    // concat the web root
                    if (dir != null)
                    {
                        baseDirectory = Path.Combine(dir.FullName, "Presentation\\SmartStore.Web");
                        testPath = Path.Combine(baseDirectory, path);
                    }
                }

                return testPath;
            }
        }

        public static bool IsDevEnvironment
        {
            get
            {
                if (!_isDevEnvironment.HasValue)
                {
                    _isDevEnvironment = IsDevEnvironmentInternal();
                }

                return _isDevEnvironment.Value;
            }
        }

        private static bool IsDevEnvironmentInternal()
        {
            if (!HostingEnvironment.IsHosted)
                return true;

            if (HostingEnvironment.IsDevelopmentEnvironment)
                return true;

            if (System.Diagnostics.Debugger.IsAttached)
                return true;

            // if there's a 'SmartStore.NET.sln' in one of the parent folders,
            // then we're likely in a dev environment
            if (FindSolutionRoot(HostingEnvironment.MapPath("~/")) != null)
                return true;

            return false;
        }

        private static DirectoryInfo FindSolutionRoot(string currentDir)
        {
            var dir = Directory.GetParent(currentDir);
            while (true)
            {
                if (dir == null || IsSolutionRoot(dir))
                    break;

                dir = dir.Parent;
            }

            return dir;
        }

        private static bool IsSolutionRoot(DirectoryInfo dir)
        {
            return File.Exists(Path.Combine(dir.FullName, "SmartStoreNET.sln"));
        }

        public static bool TryConvert<T>(object value, out T convertedValue)
        {
            convertedValue = default(T);

            if (TryConvert(value, typeof(T), CultureInfo.InvariantCulture, out object result))
            {
                if (result != null)
                    convertedValue = (T)result;
                return true;
            }

            return false;
        }

        public static bool TryConvert<T>(object value, CultureInfo culture, out T convertedValue)
        {
            convertedValue = default(T);

            if (TryConvert(value, typeof(T), culture, out object result))
            {
                if (result != null)
                    convertedValue = (T)result;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(object value, Type to, out object convertedValue)
        {
            return TryConvert(value, to, CultureInfo.InvariantCulture, out convertedValue);
        }

        public static bool TryConvert(object value, Type to, CultureInfo culture, out object convertedValue)
        {
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            convertedValue = null;

            if (value == null || value == DBNull.Value)
            {
                return to == typeof(string) || to.IsPredefinedSimpleType() == false;
            }

            if (to != typeof(object) && to.IsInstanceOfType(value))
            {
                convertedValue = value;
                return true;
            }

            Type from = value.GetType();

            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }

            try
            {
                // Get a converter for 'to' (value -> to)
                var converter = TypeConverterFactory.GetConverter(to);
                if (converter != null && converter.CanConvertFrom(from))
                {
                    convertedValue = converter.ConvertFrom(culture, value);
                    return true;
                }

                // Try the other way round with a 'from' converter (to <- from)
                converter = TypeConverterFactory.GetConverter(from);
                if (converter != null && converter.CanConvertTo(to))
                {
                    convertedValue = converter.ConvertTo(culture, null, value, to);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static ExpandoObject ToExpando(object value)
        {
            Guard.NotNull(value, nameof(value));

            var anonymousDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(value);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
            {
                expando.Add(item);
            }
            return (ExpandoObject)expando;
        }

        public static IDictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            return FastProperty.ObjectToDictionary(
                obj,
                key => key.Replace("_", "-").Replace("@", ""));
        }

        /// <summary>
        /// Gets a setting from the application's <c>web.config</c> <c>appSettings</c> node
        /// </summary>
        /// <typeparam name="T">The type to convert the setting value to</typeparam>
        /// <param name="key">The key of the setting</param>
        /// <param name="defValue">The default value to return if the setting does not exist</param>
        /// <returns>The casted setting value</returns>
        public static T GetAppSetting<T>(string key, T defValue = default(T))
        {
            Guard.NotEmpty(key, nameof(key));

            var setting = ConfigurationManager.AppSettings[key];

            if (setting == null)
            {
                return defValue;
            }

            return setting.Convert<T>();
        }

        public static bool HasConnectionString(string connectionStringName)
        {
            var conString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (conString != null && conString.ConnectionString.HasValue())
            {
                return true;
            }

            return false;
        }

        private static bool TryAction<T>(Func<T> func, out T output)
        {
            Guard.NotNull(func, nameof(func));

            try
            {
                output = func();
                return true;
            }
            catch
            {
                output = default(T);
                return false;
            }
        }

        public static bool IsTruthy(object value)
        {
            if (value == null)
                return false;

            switch (value)
            {
                case string x:
                    return x.HasValue();
                case bool x:
                    return x == true;
                case DateTime x:
                    return x > DateTime.MinValue;
                case TimeSpan x:
                    return x > TimeSpan.MinValue;
                case Guid x:
                    return x != Guid.Empty;
                case IComparable x:
                    return x.CompareTo(0) != 0;
                case IEnumerable<object> x:
                    return x.Any();
                case IEnumerable x:
                    return x.GetEnumerator().MoveNext();
            }

            if (value.GetType().IsNullable(out var wrappedType))
            {
                return IsTruthy(Convert.ChangeType(value, wrappedType));
            }

            return true;
        }

        public static long GetObjectSizeInBytes(object obj, HashSet<object> instanceLookup = null)
        {
            if (obj == null)
                return 0;

            var type = obj.GetType();
            var genericArguments = type.GetGenericArguments();

            long size = 0;

            if (obj is string str)
            {
                size = Encoding.Default.GetByteCount(str);
            }
            else if (obj is StringBuilder sb)
            {
                size = Encoding.Default.GetByteCount(sb.ToString());
            }
            else if (type.IsEnum)
            {
                size = System.Runtime.InteropServices.Marshal.SizeOf(Enum.GetUnderlyingType(type));
            }
            else if (type.IsPredefinedSimpleType() || type.IsPredefinedGenericType())
            {
                //size = System.Runtime.InteropServices.Marshal.SizeOf(Nullable.GetUnderlyingType(type) ?? type); // crashes often
                size = 8; // mean/average
            }
            else if (obj is Stream stream)
            {
                size = stream.Length;
            }
            else if (obj is IDictionary dic)
            {
                foreach (var item in dic.Values)
                {
                    size += GetObjectSizeInBytes(item, instanceLookup);
                }
            }
            else if (obj is IEnumerable e)
            {
                foreach (var item in e)
                {
                    size += GetObjectSizeInBytes(item, instanceLookup);
                }
            }
            else
            {
                if (instanceLookup == null)
                {
                    instanceLookup = new HashSet<object>(ReferenceEqualityComparer.Default);
                }

                if (!type.IsValueType && instanceLookup.Contains(obj))
                {
                    return 0;
                }

                instanceLookup.Add(obj);

                var serialized = false;

                if (type.IsSerializable && genericArguments.All(x => x.IsSerializable))
                {
                    try
                    {
                        using (var s = new MemoryStream())
                        {
                            var formatter = new BinaryFormatter();
                            formatter.Serialize(s, obj);
                            size = s.Length;

                            serialized = true;
                        }
                    }
                    catch { }
                }

                if (!serialized)
                {
                    // Serialization failed or is not supported: make JSON.
                    var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                    {
                        ContractResolver = SmartContractResolver.Instance,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        MaxDepth = 10,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    size = Encoding.Default.GetByteCount(json);
                }
            }

            return size;
        }
    }
}
