using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;

namespace SmartStore.Utilities
{

    public static partial class CommonHelper
    {
		
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
			Guard.ArgumentNotNull(() => path);

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
					var dir = Directory.GetParent(baseDirectory);
					while (true)
					{
						if (dir == null || IsSolutionRoot(dir))
							break;

						dir = dir.Parent;
					}

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
			return Misc.TryAction<T>(delegate
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
			return Misc.TryAction<object>(delegate { return value.Convert(to, culture); }, out convertedValue);
		}

    }
}
