using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
	public interface IIconExplorer
	{
		ICollection<IconDescription> All { get; }
		Multimap<string, string> SearchMap { get; }
		IconDescription GetIconByName(string name);
		IEnumerable<IconDescription> FindIcons(string searchTerm);
	}

	public class IconExplorer : IIconExplorer
	{
		private IDictionary<string, IconDescription> _icons;
		private readonly Multimap<string, string> _searchMap = new Multimap<string, string>(StringComparer.OrdinalIgnoreCase, x => new HashSet<string>(x));
		private readonly object _lock = new object();

		private readonly IApplicationEnvironment _env;

		public IconExplorer(IApplicationEnvironment env)
		{
			_env = env;
			Logger = NullLogger.Instance;
		}

		public ILogger Logger
		{
			get;
			set;
		}

		private void EnsureIsLoaded()
		{
			if (_icons == null)
			{
				lock (_lock)
				{
					if (_icons == null)
					{
						try
						{
							_icons = LoadIconsMetadata();
						}
						catch (Exception ex)
						{
							Logger.Error(ex);
							_icons = new Dictionary<string, IconDescription>();
						}

						foreach (var kvp in _icons)
						{
							var key = kvp.Key;
							var value = kvp.Value;

							value.Name = key;

							if (value.SearchInfo?.Terms?.Length > 0)
							{
								foreach (var term in value.SearchInfo.Terms)
								{
									_searchMap.Add(term, key);
								}
							}
						}
					}
				}
			}
		}

		private Dictionary<string, IconDescription> LoadIconsMetadata()
		{
			var fi = new FileInfo(_env.AppDataFolder.MapPath("icons.json"));

			if (!fi.Exists)
			{
				throw new FileNotFoundException("Icons metadata file does not exist.", fi.FullName);
			}

			// (Perf) look up minified version of metadata file
			var hashCode = HashCodeCombiner.Start().Add(fi, false).CombinedHashString;
			var fiMin = new FileInfo(Path.Combine(FileSystemHelper.TempDir(), "icons.{0}.json".FormatInvariant(hashCode)));
			var path = fiMin.Exists ? fiMin.FullName : fi.FullName;

			var json = File.ReadAllText(path, Encoding.GetEncoding(1252));
			var icons = JsonConvert.DeserializeObject<Dictionary<string, IconDescription>>(json);

			if (!fiMin.Exists)
			{
				// minified file did not exist: save it.
				var settings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore, Formatting = Formatting.None };
				json = JsonConvert.SerializeObject(icons, settings);
				File.WriteAllText(fiMin.FullName, json, Encoding.GetEncoding(1252));
			}

			return icons;
		}

		public ICollection<IconDescription> All
		{
			get
			{
				EnsureIsLoaded();
				return _icons.Values;
			}
		}

		public Multimap<string, string> SearchMap
		{
			get
			{
				EnsureIsLoaded();
				return _searchMap;
			}
		}

		public IconDescription GetIconByName(string name)
		{
			Guard.NotEmpty(name, nameof(name));
			EnsureIsLoaded();

			_icons.TryGetValue(name, out var description);
			return description;
		}

		public IEnumerable<IconDescription> FindIcons(string searchTerm)
		{
			Guard.NotEmpty(searchTerm, nameof(searchTerm));
			EnsureIsLoaded();

			if (_searchMap.ContainsKey(searchTerm))
			{
				var names = _searchMap[searchTerm];
				foreach (var name in names)
				{
					if (_icons.TryGetValue(name, out var description))
					{
						yield return description;
					}
				}
			}
		}
	}
}
