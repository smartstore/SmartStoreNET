using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.ComponentModel;

namespace SmartStore.Utilities
{
    public static partial class CommonHelper
    {
		private static bool? _isDevEnvironment;
		
		/// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }

		/// <summary>
		/// Returns an random interger number within a specified rage
		/// </summary>
		/// <param name="min">Minimum number</param>
		/// <param name="max">Maximum number</param>
		/// <returns>Result</returns>
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
			Guard.NotNull(path, nameof(path));

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
			return TryConvert<T>(value, CultureInfo.InvariantCulture, out convertedValue);
		}

		public static bool TryConvert<T>(object value, CultureInfo culture, out T convertedValue)
		{
			return TryAction<T>(delegate
			{
				return value.Convert<T>(culture);
			}, out convertedValue);
		}

		public static bool TryConvert(object value, Type to, out object convertedValue)
		{
			return TryConvert(value, to, CultureInfo.InvariantCulture, out convertedValue);
		}

		public static bool TryConvert(object value, Type to, CultureInfo culture, out object convertedValue)
		{
			return TryAction<object>(delegate { return value.Convert(to, culture); }, out convertedValue);
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
			Guard.NotNull(obj, nameof(obj));

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
	}
}
