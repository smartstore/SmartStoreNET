using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Events;
using SmartStore.Data;
using SmartStore.Services.Logging;
using SmartStore.Core.Plugins;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Web.Mvc;

namespace SmartStore.Services.Localization
{
    /// <summary>
    /// Provides information about localization
    /// </summary>
    public partial class LocalizationService : ILocalizationService
    {
        #region Constants
        private const string LOCALSTRINGRESOURCES_ALL_KEY = "SmartStore.lsr.all-{0}";
        private const string LOCALSTRINGRESOURCES_PATTERN_KEY = "SmartStore.lsr.";
        #endregion

        #region Fields

        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly CommonSettings _commonSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;

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
            IDataProvider dataProvider, IDbContext dbContext, CommonSettings commonSettings,
            LocalizationSettings localizationSettings, IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._logger = logger;
            this._workContext = workContext;
            this._lsrRepository = lsrRepository;
            this._languageService = languageService;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._commonSettings = commonSettings;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._commonSettings = commonSettings;
            this._localizationSettings = localizationSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

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
		/// <remarks>codehint: sm-add</remarks>
		/// <param name="key">e.g. Plugins.Import.Biz</param>
		/// <returns>Number of deleted string resources</returns>
		public virtual int DeleteLocaleStringResources(string key, bool keyIsRootKey = true) {
			int result = 0;
			if (key.HasValue()) {
				try {
					string sqlDelete = "Delete From LocaleStringResource Where ResourceName Like '{0}%'".FormatWith(key.EndsWith(".") || !keyIsRootKey ? key : key + ".");
					result = _dbContext.ExecuteSqlCommand(sqlDelete);

                    _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);
				}
				catch (Exception exc) {
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
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId,
            bool logIfNotFound = true)
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
            string key = string.Format(LOCALSTRINGRESOURCES_ALL_KEY, languageId);
            return _cacheManager.Get(key, () =>
            {
                var dict = new ConcurrentDictionary<string, Tuple<int, string>>(8, 2000, StringComparer.CurrentCultureIgnoreCase);
                
                if (forceAll || _localizationSettings.LoadAllLocaleRecordsOnStartup)
                {
                    var query = from l in _lsrRepository.Table
                                orderby l.ResourceName
                                where l.LanguageId == languageId
                                select l;
                    var locales = query.ToList();

                    foreach (var locale in locales)
                    {
                        var resourceName = locale.ResourceName.ToLowerInvariant();
                        dict.TryAdd(resourceName, new Tuple<int, string>(locale.Id, locale.ResourceValue));
                    }
                }

                return dict;
                

                //return dictionary;
            });
        }


        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether to empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>A string representing the requested resource string.</returns>
        public virtual string GetResource(string resourceKey, int languageId = 0, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
        {
            // codehint: sm-edit
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
					if (res != null)	// codehint: sm-edit (null case)
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
                    _logger.Warning(string.Format("Resource string ({0}) is not found. Language ID = {1}", resourceKey, languageId));
                
                if (!String.IsNullOrEmpty(defaultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    if (!returnEmptyIfNotFound)
                        result = resourceKey;
                }
            }
            return result;
        }

		/// <remarks>codehint: sm-add</remarks>
		public virtual SelectListItem GetResourceToSelectListItem(string resourceKey, int languageId = 0, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
		{
			string resource = GetResource(resourceKey, languageId, logIfNotFound, defaultValue, returnEmptyIfNotFound);
			return new SelectListItem() { Text = resource, Value = resource };
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

		/// <summary>
		/// Import language resources from XML file
		/// </summary>
		/// <remarks>codehint: sm-edit</remarks>
		/// <param name="language">Language</param>
		/// <param name="xmlDocument">XML document</param>
		/// <param name="rootKey">Prefix for resource key name</param>
		public virtual void ImportResourcesFromXml(
            Language language, 
            XmlDocument xmlDocument, 
            string rootKey = null, 
            bool sourceIsPlugin = false, 
            DataImportModeFlags mode = DataImportModeFlags.Insert | DataImportModeFlags.Update,
            bool updateTouchedResources = false)
		{            
            var autoCommit = _lsrRepository.AutoCommitEnabled;
            var validateOnSave = _lsrRepository.Context.ValidateOnSaveEnabled;
            var autoDetectChanges = _lsrRepository.Context.AutoDetectChangesEnabled;
            var proxyCreation = _lsrRepository.Context.ProxyCreationEnabled;

            try
            {
                _lsrRepository.Context.ValidateOnSaveEnabled = false;
                _lsrRepository.Context.AutoDetectChangesEnabled = false;
                _lsrRepository.Context.ProxyCreationEnabled = false;

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
                        if (mode.IsSet<DataImportModeFlags>(DataImportModeFlags.Update))
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
                        if (mode.IsSet<DataImportModeFlags>(DataImportModeFlags.Insert))
                        {
                            toAdd.Add(
                                new LocaleStringResource()
                                {
                                    LanguageId = language.Id,
                                    ResourceName = name,
                                    ResourceValue = value,
                                    IsFromPlugin = true
                                });
                        }
                    }
                }

                _lsrRepository.AutoCommitEnabled = true;
                _lsrRepository.InsertRange(toAdd, 500);
                toAdd.Clear();

                _lsrRepository.AutoCommitEnabled = false;
                toUpdate.Each(x =>
                {
                    _lsrRepository.Update(x);
                });
                _lsrRepository.Context.SaveChanges();
                toUpdate.Clear();

                //clear cache
                _cacheManager.RemoveByPattern(LOCALSTRINGRESOURCES_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _lsrRepository.AutoCommitEnabled = autoCommit;
                _lsrRepository.Context.ValidateOnSaveEnabled = validateOnSave;
                _lsrRepository.Context.AutoDetectChangesEnabled = autoDetectChanges;
                _lsrRepository.Context.ProxyCreationEnabled = proxyCreation;
            }

		}

		/// <summary>
		/// Import plugin resources from xml files in plugin's localization directory.
		/// </summary>
		/// <remarks>codehint: sm-add</remarks>
		/// <param name="pluginDescriptor">Descriptor of the plugin</param>
		/// <param name="forceToList">Load them into list rather than into database</param>
		public virtual void ImportPluginResourcesFromXml(PluginDescriptor pluginDescriptor, List<LocaleStringResource> forceToList = null, bool updateTouchedResources = true)
		{
			string pluginDir = pluginDescriptor.OriginalAssemblyFile.Directory.FullName;
			string localizationDir = Path.Combine(pluginDir, "Localization");

			if (!System.IO.Directory.Exists(localizationDir))
				return;

			if (forceToList == null && updateTouchedResources)
				DeleteLocaleStringResources(pluginDescriptor.ResourceRootKey);

			var languages = _languageService.GetAllLanguages(true);
			var doc = new XmlDocument();
            
			foreach (var filePath in System.IO.Directory.EnumerateFiles(localizationDir, "*.xml"))
			{
				Match match = Regex.Match(Path.GetFileName(filePath), Regex.Escape("resources.") + "(.*?)" + Regex.Escape(".xml"));
				string languageCode = match.Groups[1].Value;

				Language language = languages.Where(l => l.LanguageCulture.IsCaseInsensitiveEqual(languageCode)).FirstOrDefault();
                if (language != null)
                {
                    language = _languageService.GetLanguageById(language.Id);
                }

				if (languageCode.HasValue() && language != null)
				{
					doc.Load(filePath);

					if (forceToList == null)
					{
						ImportResourcesFromXml(language, doc, pluginDescriptor.ResourceRootKey, true, updateTouchedResources: updateTouchedResources);
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
								forceToList.Add(res);
						}
					}
				}
			}
		}

        #endregion
    }
}
