using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Localization;
using System.Globalization;

namespace SmartStore.Services.Localization
{
	public partial class LocalizationService : ILocalizationService
    {
		/// <summary>
		/// 0 = segment (first 3 chars of key), 1 = language id
		/// </summary>
		const string LOCALESTRINGRESOURCES_SEGMENT_KEY = "localization:{0}-lang-{1}";
		const string LOCALESTRINGRESOURCES_SEGMENT_PATTERN = "localization:{0}*";

        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;
        private readonly IDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

		private int _notFoundLogCount = 0;
		private int? _defaultLanguageId;

        public LocalizationService(
			ICacheManager cacheManager,
            ILogger logger, 
			IWorkContext workContext,
            IRepository<LocaleStringResource> lsrRepository, 
            ILanguageService languageService,
			IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _logger = logger;
            _workContext = workContext;
            _lsrRepository = lsrRepository;
            _languageService = languageService;
			_dbContext = lsrRepository.Context;
            _eventPublisher = eventPublisher;
        }

        public virtual void DeleteLocaleStringResource(LocaleStringResource resource)
        {
			Guard.NotNull(resource, nameof(resource));

            // cache
			ClearCachedResourceSegment(resource.ResourceName, resource.LanguageId);
			
            // db
            _lsrRepository.Delete(resource);
        }

		public virtual int DeleteLocaleStringResources(string key, bool keyIsRootKey = true) {
			int result = 0;

			if (key.HasValue()) 
            {
				try 
                {
					var sqlDelete = "Delete From LocaleStringResource Where ResourceName Like '{0}%'".FormatWith(key.EndsWith(".") || !keyIsRootKey ? key : key + ".");
					result = _dbContext.ExecuteSqlCommand(sqlDelete);

					ClearCachedResourceSegment(key);
				}
				catch (Exception exc) 
                {
					exc.Dump();
				}
			}

			return result;
		}

        public virtual LocaleStringResource GetLocaleStringResourceById(int localeStringResourceId)
        {
            if (localeStringResourceId == 0)
                return null;

            var localeStringResource = _lsrRepository.GetById(localeStringResourceId);
            return localeStringResource;
        }

        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName)
        {
            if (_workContext.WorkingLanguage != null)
                return GetLocaleStringResourceByName(resourceName, _workContext.WorkingLanguage.Id);

            return null;
        }

        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId, bool logIfNotFound = true)
        {
            var query = from lsr in _lsrRepository.Table
                        orderby lsr.ResourceName
                        where lsr.LanguageId == languageId && lsr.ResourceName == resourceName
                        select lsr;
            var localeStringResource = query.FirstOrDefault();

            if (localeStringResource == null && logIfNotFound)
                _logger.Warn(string.Format("Resource string ({0}) not found. Language ID = {1}", resourceName, languageId));

            return localeStringResource;
        }

        public virtual IQueryable<LocaleStringResource> All(int languageId)
        {
            var query = from lsr in _lsrRepository.Table
                        orderby lsr.ResourceName
                        where lsr.LanguageId == languageId
                        select lsr;

			return query;
        }

		public virtual IList<LocaleStringResource> GetResourcesByPattern(string pattern, int languageId)
		{
			Guard.NotEmpty(pattern, nameof(pattern));

			var query = from l in _lsrRepository.Table
						where l.ResourceName.StartsWith(pattern) && l.LanguageId == languageId
						select l;

			var resources = query.ToList();
			return resources;
		}

		public virtual void InsertLocaleStringResource(LocaleStringResource resource)
        {
			Guard.NotNull(resource, nameof(resource));

            _lsrRepository.Insert(resource);

			// cache
			ClearCachedResourceSegment(resource.ResourceName, resource.LanguageId);
        }

        public virtual void UpdateLocaleStringResource(LocaleStringResource resource)
        {
			Guard.NotNull(resource, nameof(resource));

            // cache
            object origKey = null;
			if (_dbContext.TryGetModifiedProperty(resource, "ResourceName", out origKey))
			{
				ClearCachedResourceSegment((string)origKey, resource.LanguageId);
			}
			ClearCachedResourceSegment(resource.ResourceName, resource.LanguageId);

			_lsrRepository.Update(resource);
        }

		protected virtual IDictionary<string, string> GetCachedResourceSegment(string forKey, int languageId)
		{
			Guard.NotEmpty(forKey, nameof(forKey));

			var segmentKey = GetSegmentKey(forKey);
			var cacheKey = BuildCacheSegmentKey(segmentKey, languageId);

			return _cacheManager.Get(cacheKey, () => 
			{
				var resources = _lsrRepository.TableUntracked
					.Where(x => x.ResourceName.StartsWith(segmentKey) && x.LanguageId == languageId)
					//.OrderBy(x => x.ResourceName)
					.ToList();

				var dict = new Dictionary<string, string>(resources.Count);

				foreach (var res in resources)
				{
					dict[res.ResourceName.ToLowerInvariant()] = res.ResourceValue;
				}

				return dict;
			});
		}

		/// <summary>
		/// Clears the cached resource segment from the cache
		/// </summary>
		/// <param name="forKey">The resource key for which a segment key should be created</param>
		/// <param name="languageId">Language Id. If <c>null</c>, segments for all cached languages will be invalidated</param>
		protected virtual void ClearCachedResourceSegment(string forKey, int? languageId = null)
		{
			var segmentKey = GetSegmentKey(forKey);

			if (languageId.HasValue && languageId.Value > 0)
			{
				_cacheManager.Remove(BuildCacheSegmentKey(segmentKey, languageId.Value));
			}
			else
			{
				_cacheManager.RemoveByPattern(LOCALESTRINGRESOURCES_SEGMENT_PATTERN.FormatInvariant(segmentKey));
			}
		}

        public virtual string GetResource(
			string resourceKey, 
			int languageId = 0, 
			bool logIfNotFound = true, 
			string defaultValue = "", 
			bool returnEmptyIfNotFound = false)
        {
            if (languageId <= 0)
            {
                if (_workContext.WorkingLanguage == null)
				{
					return defaultValue;
				}
                    
                languageId = _workContext.WorkingLanguage.Id;
            }
            
            string result = string.Empty;    

            resourceKey = resourceKey.EmptyNull().Trim().ToLowerInvariant();

			var cachedSegment = GetCachedResourceSegment(resourceKey, languageId);

            if (!cachedSegment.TryGetValue(resourceKey, out result))
            {
				if (logIfNotFound)
				{
					if (_notFoundLogCount < 50)
					{
						_logger.Warn(string.Format("Resource string ({0}) does not exist. Language ID = {1}", resourceKey, languageId));
					}
					else if (_notFoundLogCount == 50)
					{
						_logger.Warn("Too many language resources do not exist (> 50). Stopped logging missing resources to prevent performance drop.");
					}
					
					_notFoundLogCount++;
				}
                
                if (!String.IsNullOrEmpty(defaultValue))
                {
                    result = defaultValue;
                }
                else
                {
					// try fallback to default language
					if (!_defaultLanguageId.HasValue)
					{
						_defaultLanguageId = _languageService.GetDefaultLanguageId();
					}

					var defaultLangId = _defaultLanguageId.Value;
					if (defaultLangId > 0 && defaultLangId != languageId)
					{
						var fallbackResult = GetResource(resourceKey, defaultLangId, false, resourceKey);
						if (fallbackResult != resourceKey)
						{
							result = fallbackResult;
						}
					}

					if (!returnEmptyIfNotFound && result.IsEmpty())
					{
						result = resourceKey;
					}
                }
            }

            return result;
        }

        public virtual string ExportResourcesToXml(Language language)
        {
			Guard.NotNull(language, nameof(language));

            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Language");
            xmlWriter.WriteAttributeString("Name", language.Name);

			using (var scope = new DbContextScope(forceNoTracking: true))
			{
				var resources = All(language.Id).ToList();
				foreach (var resource in resources)
				{
					if (resource.IsFromPlugin.GetValueOrDefault() == false)
					{
						xmlWriter.WriteStartElement("LocaleResource");
						xmlWriter.WriteAttributeString("Name", resource.ResourceName);
						xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
						xmlWriter.WriteEndElement();
					}
				}
			}

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

		public virtual void ImportPluginResourcesFromXml(
			PluginDescriptor pluginDescriptor,
			IList<LocaleStringResource> targetList = null,
			bool updateTouchedResources = true,
			IList<Language> filterLanguages = null)
		{
			var directory = new DirectoryInfo(Path.Combine(pluginDescriptor.OriginalAssemblyFile.Directory.FullName, "Localization"));

			if (!directory.Exists)
				return;

			if (targetList == null && updateTouchedResources)
			{
				DeleteLocaleStringResources(pluginDescriptor.ResourceRootKey);
			}

			var unprocessedLanguages = new List<Language>();

			var defaultLanguageId = _languageService.GetDefaultLanguageId();
			var languages = filterLanguages ?? _languageService.GetAllLanguages(true);

			string code = null;
			foreach (var language in languages)
			{
				code = ImportPluginResourcesForLanguage(
					language,
					null,
					directory,
					pluginDescriptor.ResourceRootKey,
					targetList,
					updateTouchedResources,
					false);

				if (code == null)
				{
					unprocessedLanguages.Add(language);
				}
			}

			if (filterLanguages == null && unprocessedLanguages.Count > 0)
			{
				// There were unprocessed languages (no corresponding resource file could be found).
				// In order for GetResource() to be able to gracefully fallback to the default language's resources,
				// we need to import resources for the current default language....
				var processedLanguages = languages.Except(unprocessedLanguages).ToList();
				if (!processedLanguages.Any(x => x.Id == defaultLanguageId))
				{
					// ...but only if no resource file could be mapped to the default language before,
					// namely because in this case the following operation would be redundant.
					var defaultLanguage = _languageService.GetLanguageById(_languageService.GetDefaultLanguageId());
					if (defaultLanguage != null)
					{
						ImportPluginResourcesForLanguage(
							defaultLanguage,
							"en-us",
							directory,
							pluginDescriptor.ResourceRootKey,
							targetList,
							updateTouchedResources,
							true);
					}
				}
			}
		}

		private string ImportPluginResourcesForLanguage(
			Language language,
			string fileCode,
			DirectoryInfo directory,
			string resourceRootKey,
			IList<LocaleStringResource> targetList,
			bool updateTouchedResources,
			bool canFallBackToAnyResourceFile)
		{
			var fileNamePattern = "resources.{0}.xml";

			var codeCandidates = GetResourceFileCodeCandidates(
				fileCode ?? language.LanguageCulture,
				directory,
				canFallBackToAnyResourceFile);

			string path = null;
			string code = null;

			foreach (var candidate in codeCandidates)
			{
				var pathCandidate = Path.Combine(directory.FullName, fileNamePattern.FormatInvariant(candidate));
				if (File.Exists(pathCandidate))
				{
					code = candidate;
					path = pathCandidate;
					break;
				}
			}

			if (code != null)
			{
				var doc = new XmlDocument();

				doc.Load(path);
				doc = FlattenResourceFile(doc);

				if (targetList == null)
				{
					ImportResourcesFromXml(language, doc, resourceRootKey, true, updateTouchedResources: updateTouchedResources);
				}
				else
				{
					var nodes = doc.SelectNodes(@"//Language/LocaleResource");
					foreach (XmlNode node in nodes)
					{
						var valueNode = node.SelectSingleNode("Value");
						var res = new LocaleStringResource
						{
							ResourceName = node.Attributes["Name"].InnerText.Trim(),
							ResourceValue = (valueNode == null ? "" : valueNode.InnerText),
							LanguageId = language.Id,
							IsFromPlugin = true
						};

						if (res.ResourceName.HasValue())
						{
							targetList.Add(res);
						}
					}
				}
			}

			return code;
		}

		private IEnumerable<string> GetResourceFileCodeCandidates(string code, DirectoryInfo directory, bool canFallBackToAnyResourceFile)
		{
			// exact match (de-DE)
			yield return code;

			// neutral culture (de)
			var ci = CultureInfo.GetCultureInfo(code);
			if (ci.Parent != null && !ci.IsNeutralCulture)
			{
				code = ci.Parent.Name;
				yield return code;
			}

			var rgFileName = new Regex("^resources.(.+?).xml$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			// any other region with same language (de-*)
			foreach (var fi in directory.EnumerateFiles("resources.{0}-*.xml".FormatInvariant(code), SearchOption.TopDirectoryOnly))
			{
				code = rgFileName.Match(fi.Name).Groups[1].Value;
				if (LocalizationHelper.IsValidCultureCode(code))
				{
					yield return code;
					yield break;
				}
			}

			if (canFallBackToAnyResourceFile)
			{
				foreach (var fi in directory.EnumerateFiles("resources.*.xml", SearchOption.TopDirectoryOnly))
				{
					code = rgFileName.Match(fi.Name).Groups[1].Value;
					if (LocalizationHelper.IsValidCultureCode(code))
					{
						yield return code;
						yield break;
					}
				}
			}
		}

		public virtual int ImportResourcesFromXml(
            Language language, 
            XmlDocument xmlDocument, 
            string rootKey = null, 
            bool sourceIsPlugin = false, 
            ImportModeFlags mode = ImportModeFlags.Insert | ImportModeFlags.Update,
            bool updateTouchedResources = false)
		{            
			using (var scope = new DbContextScope(autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false, forceNoTracking: true, hooksEnabled: false))
			{
				var toAdd = new List<LocaleStringResource>();
				var toUpdate = new List<LocaleStringResource>();
				var nodes = xmlDocument.SelectNodes(@"//Language/LocaleResource");

				var resources = language.LocaleStringResources.ToDictionarySafe(x => x.ResourceName, StringComparer.OrdinalIgnoreCase);

				LocaleStringResource resource;

				foreach (var xel in nodes.Cast<XmlElement>())
				{
					string name = xel.GetAttribute("Name").TrimSafe();
					string value = "";
					var valueNode = xel.SelectSingleNode("Value");
					if (valueNode != null)
						value = valueNode.InnerText;

					if (String.IsNullOrEmpty(name))
						continue;

					if (rootKey.HasValue())
					{
						if (!xel.GetAttributeText("AppendRootKey").IsCaseInsensitiveEqual("false"))
							name = "{0}.{1}".FormatWith(rootKey, name);
					}

					resource = null;

					// do not use "Insert"/"Update" methods because they clear cache
					// let's bulk insert
					//var resource = language.LocaleStringResources.Where(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
					if (resources.TryGetValue(name, out resource))
					{
						if (mode.HasFlag(ImportModeFlags.Update))
						{
							if (updateTouchedResources || !resource.IsTouched.GetValueOrDefault())
							{
								if (value != resource.ResourceValue)
								{
									resource.ResourceValue = value;
									resource.IsTouched = null;
									toUpdate.Add(resource);
								}
							}
						}
					}
					else
					{
						if (mode.HasFlag(ImportModeFlags.Insert))
						{
							toAdd.Add(
								new LocaleStringResource
								{
									LanguageId = language.Id,
									ResourceName = name,
									ResourceValue = value,
									IsFromPlugin = sourceIsPlugin
								});
						}
					}
				}

				//_lsrRepository.AutoCommitEnabled = true;

				if (toAdd.Any() || toUpdate.Any())
				{
					var segmentKeys = new HashSet<string>();

					toAdd.Each(x => segmentKeys.Add(GetSegmentKey(x.ResourceName)));
					toUpdate.Each(x => segmentKeys.Add(GetSegmentKey(x.ResourceName)));

					_lsrRepository.InsertRange(toAdd);
					toAdd.Clear();

					_lsrRepository.UpdateRange(toUpdate);
					toUpdate.Clear();

					int num = _lsrRepository.Context.SaveChanges();

					// clear cache
					foreach (var segmentKey in segmentKeys)
					{
						ClearCachedResourceSegment(segmentKey, language.Id);
					}

					return num;
				}

				return 0;
			}
		}

        public virtual XmlDocument FlattenResourceFile(XmlDocument source)
        {
            Guard.NotNull(source, nameof(source));

            if (source.SelectNodes("//Children").Count == 0)
            {
                // the document contains absolutely NO nesting,
                // so don't bother parsing.
                return source;
            }

            var resources = new List<LocaleStringResourceParent>();

            foreach (XmlNode resNode in source.SelectNodes(@"//Language/LocaleResource"))
            {
                resources.Add(new LocaleStringResourceParent(resNode));
            }

            resources.Sort((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName));

            foreach (var resource in resources)
            {
                RecursivelySortChildrenResource(resource);
            }

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Language", "");

                writer.WriteStartAttribute("Name", "");
                writer.WriteString(source.SelectSingleNode(@"//Language").Attributes["Name"].InnerText.Trim());
                writer.WriteEndAttribute();

                foreach (var resource in resources)
                {
                    RecursivelyWriteResource(resource, writer, null);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }

            var result = new XmlDocument();
            result.LoadXml(sb.ToString());

            return result;
        }

        private void RecursivelyWriteResource(LocaleStringResourceParent resource, XmlWriter writer, bool? parentAppendRootKey)
        {
            //The value isn't actually used, but the name is used to create a namespace.
            if (resource.IsPersistable)
            {
                writer.WriteStartElement("LocaleResource", "");

                writer.WriteStartAttribute("Name", "");
                writer.WriteString(resource.NameWithNamespace);
                writer.WriteEndAttribute();

				if (resource.AppendRootKey.HasValue)
				{
					writer.WriteStartAttribute("AppendRootKey", "");
					writer.WriteString(resource.AppendRootKey.Value ? "true" : "false");
					writer.WriteEndAttribute();
					parentAppendRootKey = resource.AppendRootKey;
				}
				else if (parentAppendRootKey.HasValue)
				{
					writer.WriteStartAttribute("AppendRootKey", "");
					writer.WriteString(parentAppendRootKey.Value ? "true" : "false");
					writer.WriteEndAttribute();
				}

                writer.WriteStartElement("Value", "");
                writer.WriteString(resource.ResourceValue);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            foreach (var child in resource.ChildLocaleStringResources)
            {
				RecursivelyWriteResource(child, writer, resource.AppendRootKey ?? parentAppendRootKey);
            }

        }

        private void RecursivelySortChildrenResource(LocaleStringResourceParent resource)
        {
            ArrayList.Adapter((IList)resource.ChildLocaleStringResources).Sort(new LocalizationService.ComparisonComparer<LocaleStringResourceParent>((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName)));

            foreach (var child in resource.ChildLocaleStringResources)
            {
                RecursivelySortChildrenResource(child);
            }
        }

		private string BuildCacheSegmentKey(string segment, int languageId)
		{
			return String.Format(LOCALESTRINGRESOURCES_SEGMENT_KEY, segment, languageId);
		}

		private string GetSegmentKey(string forKey)
		{
			return forKey.Substring(0, Math.Min(forKey.Length, 3)).ToLowerInvariant();
		}


        private class LocaleStringResourceParent : LocaleStringResource
        {
            public LocaleStringResourceParent(XmlNode localStringResource, string nameSpace = "")
            {
                Namespace = nameSpace;
                var resNameAttribute = localStringResource.Attributes["Name"];
                var resValueNode = localStringResource.SelectSingleNode("Value");

                if (resNameAttribute == null)
                {
                    throw new SmartException("All language resources must have an attribute Name=\"Value\".");
                }
                var resName = resNameAttribute.Value.Trim();
                if (string.IsNullOrEmpty(resName))
                {
                    throw new SmartException("All languages resource attributes 'Name' must have a value.'");
                }
                ResourceName = resName;

				var appendRootKeyAttribute = localStringResource.Attributes["AppendRootKey"];
				if (appendRootKeyAttribute != null)
				{
					AppendRootKey = appendRootKeyAttribute.Value.ToBool(true);
				}

                if (resValueNode == null || string.IsNullOrEmpty(resValueNode.InnerText.Trim()))
                {
                    IsPersistable = false;
                }
                else
                {
                    IsPersistable = true;
                    ResourceValue = resValueNode.InnerText.Trim();
                }

                foreach (XmlNode childResource in localStringResource.SelectNodes("Children/LocaleResource"))
                {
                    ChildLocaleStringResources.Add(new LocaleStringResourceParent(childResource, NameWithNamespace));
                }
            }

            public string Namespace { get; set; }

            public IList<LocaleStringResourceParent> ChildLocaleStringResources = new List<LocaleStringResourceParent>();

            public bool IsPersistable { get; set; }

			public bool? AppendRootKey { get; set; }

            public string NameWithNamespace
            {
                get
                {
                    var newNamespace = Namespace;
                    if (!string.IsNullOrEmpty(newNamespace))
                    {
                        newNamespace += ".";
                    }
                    return newNamespace + ResourceName;
                }
            }
        }

        private class ComparisonComparer<T> : IComparer<T>, IComparer
        {
            private readonly Comparison<T> _comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }

            public int Compare(object o1, object o2)
            {
                return _comparison((T)o1, (T)o2);
            }
        }
    }
}
