using System;
using System.Collections;
using System.Collections.Concurrent;
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
	/// <summary>
	/// Provides information about localization
	/// </summary>
	public partial class LocalizationService : ILocalizationService
    {
        #region Constants
        private const string LOCALESTRINGRESOURCES_ALL_KEY = "SmartStore.lsr.all-{0}";
        private const string LOCALESTRINGRESOURCES_PATTERN_KEY = "SmartStore.lsr.";
        #endregion

        #region Fields

        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;

		private int _notFoundLogCount = 0;
		private int? _defaultLanguageId;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="logger">Logger</param>
        /// <param name="workContext">Work context</param>
        /// <param name="lsrRepository">Locale string resource repository</param>
        /// <param name="languageService">Language service</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="eventPublisher">Event published</param>
        public LocalizationService(ICacheManager cacheManager,
            ILogger logger, IWorkContext workContext,
            IRepository<LocaleStringResource> lsrRepository, 
            ILanguageService languageService,
            LocalizationSettings localizationSettings, IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._logger = logger;
            this._workContext = workContext;
            this._lsrRepository = lsrRepository;
            this._languageService = languageService;
			this._dbContext = lsrRepository.Context;
            this._localizationSettings = localizationSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

		public virtual void ClearCache()
		{
			_cacheManager.RemoveByPattern(LOCALESTRINGRESOURCES_PATTERN_KEY);
		}

        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void DeleteLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");

            // cache
            this.GetResourceValues(localeStringResource.LanguageId).Remove(localeStringResource.ResourceName);

            // db
            _lsrRepository.Delete(localeStringResource);

            //event notification
            _eventPublisher.EntityDeleted(localeStringResource);
        }

		/// <summary>
		/// Deletes all string resources with its key beginning with rootKey.
		/// </summary>
		/// <param name="key">e.g. Plugins.Import.Biz</param>
		/// <returns>Number of deleted string resources</returns>
		public virtual int DeleteLocaleStringResources(string key, bool keyIsRootKey = true) {
			int result = 0;
			if (key.HasValue()) 
            {
				try 
                {
					string sqlDelete = "Delete From LocaleStringResource Where ResourceName Like '{0}%'".FormatWith(key.EndsWith(".") || !keyIsRootKey ? key : key + ".");
					result = _dbContext.ExecuteSqlCommand(sqlDelete);

                    _cacheManager.RemoveByPattern(LOCALESTRINGRESOURCES_PATTERN_KEY);
				}
				catch (Exception exc) 
                {
					exc.Dump();
				}
			}
			return result;
		}

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeStringResourceId">Locale string resource identifier</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceById(int localeStringResourceId)
        {
            if (localeStringResourceId == 0)
                return null;

            var localeStringResource = _lsrRepository.GetById(localeStringResourceId);
            return localeStringResource;
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName)
        {
            if (_workContext.WorkingLanguage != null)
                return GetLocaleStringResourceByName(resourceName, _workContext.WorkingLanguage.Id);

            return null;
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId, bool logIfNotFound = true)
        {
            var query = from lsr in _lsrRepository.Table
                        orderby lsr.ResourceName
                        where lsr.LanguageId == languageId && lsr.ResourceName == resourceName
                        select lsr;
            var localeStringResource = query.FirstOrDefault();

            if (localeStringResource == null && logIfNotFound)
                _logger.Warning(string.Format("Resource string ({0}) not found. Language ID = {1}", resourceName, languageId));

            return localeStringResource;
        }

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        public virtual IList<LocaleStringResource> GetAllResources(int languageId)
        {
            var query = from l in _lsrRepository.Table
                        orderby l.ResourceName
                        where l.LanguageId == languageId
                        select l;
            var locales = query.ToList();
            return locales;
        }

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void InsertLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");
            
            _lsrRepository.Insert(localeStringResource);

            //// cache
            var holder = this.GetResourceValues(localeStringResource.LanguageId) as ConcurrentDictionary<string, Tuple<int, string>>;
            holder.TryAdd(
                localeStringResource.ResourceName,
                new Tuple<int, string>(localeStringResource.Id, localeStringResource.ResourceValue));

            //event notification
            _eventPublisher.EntityInserted(localeStringResource);
        }

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        public virtual void UpdateLocaleStringResource(LocaleStringResource localeStringResource)
        {
            if (localeStringResource == null)
                throw new ArgumentNullException("localeStringResource");

            var modProps = _lsrRepository.GetModifiedProperties(localeStringResource);

            _lsrRepository.Update(localeStringResource);

            // cache (TODO)
            var holder = this.GetResourceValues(localeStringResource.LanguageId);
            object origKey = null;
            if (modProps.TryGetValue("ResourceName", out origKey))
            {
                holder.Remove((string)origKey);
            }
            else
            {
                holder.Remove(localeStringResource.ResourceName);
            }
            
            var holder2 = holder as ConcurrentDictionary<string, Tuple<int, string>>;
            holder2.TryAdd(
                localeStringResource.ResourceName,
                new Tuple<int, string>(localeStringResource.Id, localeStringResource.ResourceValue));

            // event notification
            _eventPublisher.EntityUpdated(localeStringResource);
        }

        public virtual IDictionary<string, Tuple<int, string>> GetResourceValues(int languageId, bool forceAll = false)
        {
            Action<ConcurrentDictionary<string, Tuple<int, string>>> loadAllAction = (d) =>
            {
				var query = from l in _lsrRepository.TableUntracked
							orderby l.ResourceName
							where l.LanguageId == languageId
							select l;
				var resources = query.ToList();

                foreach (var res in resources)
                {
                    var resourceName = res.ResourceName.ToLowerInvariant();
                    d.AddOrUpdate(
                        resourceName, 
                        (k) => new Tuple<int, string>(res.Id, res.ResourceValue), 
                        (k, v) => { d[k] = v; return v; });
                }

                // perf: add a dummy item indicating that data is fully loaded
                d.TryAdd("!!___EOF___!!", new Tuple<int, string>(0, string.Empty));
            };

            string cacheKey = string.Format(LOCALESTRINGRESOURCES_ALL_KEY, languageId);
            var dict = _cacheManager.Get(cacheKey, () => {
                var result = new ConcurrentDictionary<string, Tuple<int, string>>(8, 2000, StringComparer.CurrentCultureIgnoreCase);
                if (forceAll || _localizationSettings.LoadAllLocaleRecordsOnStartup)
                {
                    loadAllAction(result);
                }
                return result;
            });

            if (forceAll && !_localizationSettings.LoadAllLocaleRecordsOnStartup && !dict.ContainsKey("!!___EOF___!!"))
            {
                // In this case the resources MIGHT not be loaded completely
                // from the DB, but cached already. So load all and enforce merge.
                loadAllAction(dict);
            }

            return dict;
        }


        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey, int languageId = 0, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
        {
            if (languageId <= 0)
            {
                if (_workContext.WorkingLanguage == null)
                    return defaultValue;

                languageId = _workContext.WorkingLanguage.Id;
            }
            
            string result = string.Empty;
            if (resourceKey == null)
                resourceKey = string.Empty;
            resourceKey = resourceKey.Trim().ToLowerInvariant();

            var holder = this.GetResourceValues(languageId) as ConcurrentDictionary<string, Tuple<int, string>>;

            Tuple<int, string> lsr = null;
            try
            {
                lsr = holder.GetOrAdd(resourceKey, (x) =>
                {
                    var query = from l in _lsrRepository.Table
                                where l.ResourceName == resourceKey && l.LanguageId == languageId
                                select l;
                    var res = query.FirstOrDefault();
					if (res != null)
						return new Tuple<int, string>(res.Id, res.ResourceValue);
					return null;
                });
            }
            catch { }

            if (lsr != null)
                result = lsr.Item2;

            if (String.IsNullOrEmpty(result))
            {
				if (logIfNotFound)
				{
					if (_notFoundLogCount < 50)
					{
						_logger.Warning(string.Format("Resource string ({0}) does not exist. Language ID = {1}", resourceKey, languageId));
					}
					else if (_notFoundLogCount == 50)
					{
						_logger.Warning("Too many language resources do not exist (> 50). Stopped logging missing resources to prevent performance drop.");
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

        /// <summary>
        /// Export language resources to xml
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportResourcesToXml(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Language");
            xmlWriter.WriteAttributeString("Name", language.Name);

            var resources = GetAllResources(language.Id);
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

		/// <summary>
		/// Resolves a resource file for the specified language and processes the import
		/// </summary>
		/// <param name="language">Language</param>
		/// <returns>The culture code of the processed resource file</returns>
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
						var res = new LocaleStringResource()
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

		/// <summary>
		/// Import language resources from XML file
		/// </summary>
		/// <param name="language">Language</param>
		/// <param name="xmlDocument">XML document</param>
		/// <param name="rootKey">Prefix for resource key name</param>
		public virtual void ImportResourcesFromXml(
            Language language, 
            XmlDocument xmlDocument, 
            string rootKey = null, 
            bool sourceIsPlugin = false, 
            ImportModeFlags mode = ImportModeFlags.Insert | ImportModeFlags.Update,
            bool updateTouchedResources = false)
		{            
			using (var scope = new DbContextScope(autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				var toAdd = new List<LocaleStringResource>();
				var toUpdate = new List<LocaleStringResource>();
				var nodes = xmlDocument.SelectNodes(@"//Language/LocaleResource");

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

					// do not use "Insert"/"Update" methods because they clear cache
					// let's bulk insert
					var resource = language.LocaleStringResources.Where(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
					if (resource != null)
					{
						if (mode.HasFlag(ImportModeFlags.Update))
						{
							if (updateTouchedResources || !resource.IsTouched.GetValueOrDefault())
							{
								resource.ResourceValue = value;
								resource.IsTouched = null;
								toUpdate.Add(resource);
							}
						}
					}
					else
					{
						if (mode.HasFlag(ImportModeFlags.Insert))
						{
							toAdd.Add(
								new LocaleStringResource()
								{
									LanguageId = language.Id,
									ResourceName = name,
									ResourceValue = value,
									IsFromPlugin = sourceIsPlugin
								});
						}
					}
				}

				_lsrRepository.AutoCommitEnabled = true;
				_lsrRepository.InsertRange(toAdd, 500);
				toAdd.Clear();

				_lsrRepository.UpdateRange(toUpdate);
				toUpdate.Clear();

				// clear cache
				_cacheManager.RemoveByPattern(LOCALESTRINGRESOURCES_PATTERN_KEY);
			}
		}

        public virtual XmlDocument FlattenResourceFile(XmlDocument source)
        {
            Guard.ArgumentNotNull(() => source);

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

        #endregion

        #region Classes

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

        #endregion
    }
}
