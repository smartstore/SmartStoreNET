using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core.Caching;
using SmartStore.Utilities;

namespace SmartStore.Core.Localization
{
	public class LocalizationFileResolver : ILocalizationFileResolver
	{
		private readonly ICacheManager _cache;

		public LocalizationFileResolver(ICacheManager cache)
		{
			_cache = cache;
		}

		public LocalizationFileResolveResult Resolve(
			string culture,
			string virtualPath, 
			string pattern, 
			bool cache = true, 
			string fallbackCulture = "en")
		{
			Guard.NotEmpty(culture, nameof(culture));
			Guard.NotEmpty(virtualPath, nameof(virtualPath));
			Guard.NotEmpty(pattern, nameof(pattern));

			if (pattern.IndexOf('*') < 0)
			{
				throw new ArgumentException("The pattern must contain a wildcard char for substitution, e.g. 'lang-*.js'.", nameof(pattern));
			}

			virtualPath = FixPath(virtualPath);
			var cacheKey = "core:locfile:" + virtualPath.ToLower() + pattern + "/" + culture;
			string result = null;

			if (cache && _cache.Contains(cacheKey)) 
			{
				result = _cache.Get<string>(cacheKey);
				return result != null ? CreateResult(result, virtualPath, pattern) : null;
			}

			if (!LocalizationHelper.IsValidCultureCode(culture))
			{
				throw new ArgumentException($"'{culture}' is not a valid culture code.", nameof(culture));
			}

			var ci = CultureInfo.GetCultureInfo(culture);
			var directory = new DirectoryInfo(CommonHelper.MapPath(virtualPath, false));

			if (!directory.Exists)
			{
				throw new DirectoryNotFoundException($"Path '{virtualPath}' does not exist.");
			}

			// 1: Match passed culture
			result = ResolveMatchingFile(ci, directory, pattern);

			if (result == null && fallbackCulture.HasValue() && culture != fallbackCulture)
			{
				if (!LocalizationHelper.IsValidCultureCode(fallbackCulture))
				{
					throw new ArgumentException($"'{culture}' is not a valid culture code.", nameof(fallbackCulture));
				}

				// 2: Match fallback culture
				ci = CultureInfo.GetCultureInfo(fallbackCulture);
				result = ResolveMatchingFile(ci, directory, pattern);
			}

			if (cache)
			{
				_cache.Put(cacheKey, result, TimeSpan.FromHours(24));
			}

			if (result.HasValue())
			{
				return CreateResult(result, virtualPath, pattern);
			}

			return null;
		}

		private string ResolveMatchingFile(CultureInfo ci, DirectoryInfo directory, string pattern)
		{
			string result = null;

			// 1: Exact match
			// -----------------------------------------------------
			var fileName = pattern.Replace("*", ci.Name);
			if (File.Exists(Path.Combine(directory.FullName, fileName)))
			{
				result = ci.Name;
			}

			// 2: Match neutral culture, e.g. de-DE > de
			// -----------------------------------------------------
			if (result == null && !ci.IsNeutralCulture && ci.Parent != null)
			{
				ci = ci.Parent;
				fileName = pattern.Replace("*", ci.Name);
				if (File.Exists(Path.Combine(directory.FullName, fileName)))
				{
					result = ci.Name;
				}
			}

			// 2: Match any region, e.g. de-DE > de-CH
			// -----------------------------------------------------
			if (result == null && ci.IsNeutralCulture)
			{
				// Convert pattern to Regex: "lang-*.js" > "^lang.(.+?).js$"
				var rgPattern = "^" + pattern.Replace("*", @"(.+?)") + "$";
				var rgFileName = new Regex(rgPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

				foreach (var fi in directory.EnumerateFiles(pattern.Replace("*", ci.Name + "-*"), SearchOption.TopDirectoryOnly))
				{
					var culture = rgFileName.Match(fi.Name).Groups[1].Value;
					if (LocalizationHelper.IsValidCultureCode(culture))
					{
						result = culture;
						break;
					}
				}
			}

			return result;
		}

		private string FixPath(string virtualPath)
		{
			return VirtualPathUtility.ToAppRelative(virtualPath).EnsureEndsWith("/");
		}

		private LocalizationFileResolveResult CreateResult(string culture, string virtualPath, string pattern)
		{
			var fileName = pattern.Replace("*", culture);
			return new LocalizationFileResolveResult
			{
				Culture = culture,
				VirtualPath = VirtualPathUtility.ToAbsolute(virtualPath + fileName)
			};
		}
	}
}
