using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Data.Entity;
using System.Web.Mvc;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Core.IO;
using SmartStore.Utilities;
using SmartStore.Collections;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Seo
{
    public partial class XmlSitemapGenerator : IXmlSitemapGenerator
    {
        private const string SiteMapsNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";
        private const string SiteMapFileNamePattern = "sitemap-{0}.xml";
        private const string LockFileNamePattern = "sitemap-{0}-{1}.lock";

        /// <summary>
        /// The maximum number of sitemaps a sitemap index file can contain.
        /// </summary>
        private const int MaximumSiteMapCount = 50000;

        /// <summary>
        /// The maximum number of sitemap nodes allowed in a sitemap file. The absolute maximum allowed is 50,000 
        /// according to the specification. See http://www.sitemaps.org/protocol.html but the file size must also be 
        /// less than 10MB. After some experimentation, a maximum of 2.000 nodes keeps the file size below 10MB.
        /// </summary>
        internal const int MaximumSiteMapNodeCount = 2000;

        /// <summary>
        /// The maximum size of a sitemap file in bytes (10MB).
        /// </summary>
        private const int MaximumSiteMapSizeInBytes = 10485760;

        private readonly IEnumerable<Lazy<IXmlSitemapPublisher>> _publishers;
        private readonly ILanguageService _languageService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICommonServices _services;
        private readonly ILockFileManager _lockFileManager;
        private readonly UrlHelper _urlHelper;

        private readonly IVirtualFolder _tenantFolder;
        private readonly string _baseDir;

        public XmlSitemapGenerator(
            IEnumerable<Lazy<IXmlSitemapPublisher>> publishers,
            ILanguageService languageService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            ICommonServices services,
            ILockFileManager lockFileManager,
            UrlHelper urlHelper)
        {
            _publishers = publishers;
            _languageService = languageService;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _services = services;
            _lockFileManager = lockFileManager;
            _urlHelper = urlHelper;

            _tenantFolder = _services.ApplicationEnvironment.TenantFolder;
            _baseDir = _tenantFolder.Combine("Sitemaps");

            Logger = NullLogger.Instance;

        }

        public ILogger Logger { get; set; }

        public virtual async Task<XmlSitemapPartition> GetSitemapPartAsync(int index = 0)
        {
            return await GetSitemapPartAsync(index, false);
        }

        private async Task<XmlSitemapPartition> GetSitemapPartAsync(int index, bool isRetry)
        {
            Guard.NotNegative(index, nameof(index));

            var store = _services.StoreContext.CurrentStore;
            var language = _services.WorkContext.WorkingLanguage;

            var exists = SitemapFileExists(store.Id, language.Id, index, out var path, out var name);

            if (exists)
            {
                return new XmlSitemapPartition
                {
                    Index = index,
                    Name = name,
                    LanguageId = language.Id,
                    StoreId = store.Id,
                    ModifiedOnUtc = _tenantFolder.GetFileLastWriteTimeUtc(path),
                    Stream = _tenantFolder.OpenFile(path)
                };
            }

            if (isRetry)
            {
                var msg = "Could not generate XML sitemap. Index: {0}, Date: {1}".FormatInvariant(index, DateTime.UtcNow);
                Logger.Error(msg);
                throw new SmartException(msg);
            }

            if (index > 0)
            {
                // File with index greater 0 has been requested, but it does not exist.
                // Now we have to determine whether just the passed index is out of range
                // or the files has never been created before.
                // If the main file (index 0) exists, the action should return NotFoundResult,
                // otherwise the rebuild process should be started or waited for.

                if (SitemapFileExists(store.Id, language.Id, 0, out path, out name))
                {
                    throw new IndexOutOfRangeException("The sitemap file '{0}' does not exist.".FormatInvariant(name));
                }
            }

            // The main sitemap document with index 0 does not exist, meaning: the whole sitemap
            // needs to be created and cached by partitions.

            var wasRebuilding = false;
            var lockFilePath = GetLockFilePath(store.Id, language.Id);

            while (IsRebuilding(lockFilePath))
            {
                // The rebuild process is already running, either started
                // by the task scheduler or another HTTP request.
                // We should wait for completion.

                wasRebuilding = true;
                Thread.Sleep(1000);
            }

            if (!wasRebuilding)
            {
                // No lock. Rebuild now.
                var buildContext = new XmlSitemapBuildContext(store, new[] { language }, _services.Settings, _services.StoreService.IsSingleStoreMode())
                {
                    CancellationToken = CancellationToken.None
                };

                await RebuildAsync(buildContext);
            }

            // DRY: call self to get sitemap partition object
            return await GetSitemapPartAsync(index, true);
        }

        private bool SitemapFileExists(int storeId, int languageId, int index, out string path, out string name)
        {
            path = BuildSitemapFilePath(storeId, languageId, index, out name);

            // Does not work reliably with symlinks due to framework caching
            //var exists = _tenantFolder.FileExists(path);

            var exists = File.Exists(_tenantFolder.MapPath(path));

            if (!exists)
            {
                path = null;
                name = null;
            }

            return exists;
        }

        private string BuildSitemapFilePath(int storeId, int languageId, int index, out string fileName)
        {
            fileName = SiteMapFileNamePattern.FormatInvariant(index);
            return _tenantFolder.Combine(BuildSitemapDirPath(storeId, languageId), fileName);
        }

        private string BuildSitemapDirPath(int storeId, int languageId)
        {
            return _tenantFolder.Combine(_baseDir, storeId + "/" + languageId);
        }

        private string GetLockFilePath(int storeId, int languageId)
        {
            var fileName = LockFileNamePattern.FormatInvariant(storeId, languageId);
            return _tenantFolder.Combine(_baseDir, fileName);
        }

        public virtual async Task RebuildAsync(XmlSitemapBuildContext ctx)
        {
            Guard.NotNull(ctx, nameof(ctx));

            // Double seo code otherwise
            _urlHelper.RequestContext.RouteData.DataTokens["SeoCodeReplacement"] = string.Empty;

            var languageData = new Dictionary<int, LanguageData>();

            foreach (var language in ctx.Languages)
            {
                var lockFilePath = GetLockFilePath(ctx.Store.Id, language.Id);

                if (_lockFileManager.TryAcquireLock(lockFilePath, out var lockFile))
                {
                    // Process only languages that are unlocked right now
                    // It is possible that an HTTP request triggered the generation
                    // of a language specific sitemap.

                    var sitemapDir = BuildSitemapDirPath(ctx.Store.Id, language.Id);
                    var data = new LanguageData
                    {
                        Store = ctx.Store,
                        Language = language,
                        LockFile = lockFile,
                        LockFilePath = lockFilePath,
                        TempDir = sitemapDir + "~",
                        FinalDir = sitemapDir,
                        BaseUrl = BuildBaseUrl(ctx.Store, language)
                    };

                    _tenantFolder.TryDeleteDirectory(data.TempDir);
                    _tenantFolder.CreateDirectory(data.TempDir);

                    languageData[language.Id] = data;
                }
            }

            if (languageData.Count == 0)
            {
                Logger.Warn("XML sitemap rebuild already in process.");
                return;
            }

            var languages = languageData.Values.Select(x => x.Language);
            var languageIds = languages.Select(x => x.Id).Concat(new[] { 0 }).ToArray();

            // All sitemaps grouped by language
            var sitemaps = new Multimap<int, XmlSitemapNode>();

            var compositeFileLock = new ActionDisposable(() =>
            {
                foreach (var data in languageData.Values)
                {
                    data.LockFile.Release();
                }
            });

            using (compositeFileLock)
            {
                // Impersonate
                var prevCustomer = _services.WorkContext.CurrentCustomer;
                // no need to vary xml sitemap by customer roles: it's relevant to crawlers only.
                _services.WorkContext.CurrentCustomer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);

                try
                {
                    var nodes = new List<XmlSitemapNode>();

                    var providers = CreateProviders(ctx);
                    var total = providers.Sum(x => x.GetTotalCount());

                    var totalSegments = (int)Math.Ceiling(total / (double)MaximumSiteMapNodeCount);
                    var hasIndex = totalSegments > 1;
                    var indexNodes = new Multimap<int, XmlSitemapNode>();
                    var segment = 0;
                    var numProcessed = 0;

                    CheckSitemapCount(totalSegments);

                    using (new DbContextScope(autoDetectChanges: false, forceNoTracking: true, proxyCreation: false, lazyLoading: false))
                    {
                        var entities = EnlistEntities(providers);

                        foreach (var batch in entities.Slice(MaximumSiteMapNodeCount))
                        {
                            if (ctx.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            segment++;
                            numProcessed = segment * MaximumSiteMapNodeCount;
                            ctx.ProgressCallback?.Invoke(numProcessed, total, "{0} / {1}".FormatCurrent(numProcessed, total));

                            var slugs = GetUrlRecordCollectionsForBatch(batch.Select(x => x.Entry), languageIds);

                            foreach (var data in languageData.Values)
                            {
                                var language = data.Language;
                                var baseUrl = data.BaseUrl;

                                // Create all node entries for this segment
                                var entries = batch
                                    .Where(x => x.Entry.LanguageId.GetValueOrDefault() == 0 || x.Entry.LanguageId.Value == language.Id)
                                    .Select(x => x.Provider.CreateNode(_urlHelper, baseUrl, x.Entry, slugs[x.Entry.EntityName], language));
                                sitemaps[language.Id].AddRange(entries.Where(x => x != null));

                                // Create index node for this segment/language combination
                                if (hasIndex)
                                {
                                    indexNodes[language.Id].Add(new XmlSitemapNode
                                    {
                                        LastMod = sitemaps[language.Id].Select(x => x.LastMod).Where(x => x.HasValue).DefaultIfEmpty().Max(),
                                        Loc = GetSitemapIndexUrl(segment, baseUrl),
                                    });
                                }

                                if (segment % 5 == 0 || segment == totalSegments)
                                {
                                    // Commit every 5th segment (10.000 nodes) temporarily to disk to minimize RAM usage
                                    var documents = GetSiteMapDocuments((IReadOnlyCollection<XmlSitemapNode>)sitemaps[language.Id]);
                                    await SaveTempAsync(documents, data, segment - documents.Count + (hasIndex ? 1 : 0));

                                    documents.Clear();
                                    sitemaps.RemoveAll(language.Id);
                                }
                            }

                            slugs.Clear();

                            //GC.Collect();
                            //GC.WaitForPendingFinalizers();
                        }

                        // Process custom nodes
                        if (!ctx.CancellationToken.IsCancellationRequested)
                        {
                            ctx.ProgressCallback?.Invoke(numProcessed, total, "Processing custom nodes".FormatCurrent(numProcessed, total));
                            ProcessCustomNodes(ctx, sitemaps);

                            foreach (var data in languageData.Values)
                            {
                                if (sitemaps.ContainsKey(data.Language.Id) && sitemaps[data.Language.Id].Count > 0)
                                {
                                    var documents = GetSiteMapDocuments((IReadOnlyCollection<XmlSitemapNode>)sitemaps[data.Language.Id]);
                                    await SaveTempAsync(documents, data, (segment + 1) - documents.Count + (hasIndex ? 1 : 0));
                                }
                                else if (segment == 0)
                                {
                                    // Ensure that at least one entry exists. Otherwise,
                                    // the system will try to rebuild again.
                                    var homeNode = new XmlSitemapNode { LastMod = DateTime.UtcNow, Loc = data.BaseUrl };
                                    var documents = GetSiteMapDocuments(new List<XmlSitemapNode> { homeNode });
                                    await SaveTempAsync(documents, data, 0);
                                }

                            }
                        }
                    }

                    ctx.CancellationToken.ThrowIfCancellationRequested();

                    ctx.ProgressCallback?.Invoke(totalSegments, totalSegments, "Finalizing...'");

                    foreach (var data in languageData.Values)
                    {
                        // Create index documents (if any)
                        if (hasIndex && indexNodes.Any())
                        {
                            var indexDocument = CreateSitemapIndexDocument(indexNodes[data.Language.Id]);
                            await SaveTempAsync(new List<string> { indexDocument }, data, 0);
                        }

                        // Save finally (actually renames temp folder)
                        SaveFinal(data);
                    }
                }
                finally
                {
                    // Undo impersonation
                    _services.WorkContext.CurrentCustomer = prevCustomer;
                    sitemaps.Clear();

                    foreach (var data in languageData.Values)
                    {
                        if (_tenantFolder.DirectoryExists(data.TempDir))
                        {
                            _tenantFolder.TryDeleteDirectory(data.TempDir);
                        }
                    }

                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }
            }
        }

        private string BuildBaseUrl(Store store, Language language)
        {
            var host = _services.StoreService.GetHost(store).EnsureEndsWith("/");

            var locSettings = _services.Settings.LoadSetting<LocalizationSettings>(store.Id);
            if (locSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var defaultLangId = _languageService.GetDefaultLanguageId(store.Id);
                if (language.Id != defaultLangId || locSettings.DefaultLanguageRedirectBehaviour < DefaultLanguageRedirectBehaviour.StripSeoCode)
                {
                    host += language.GetTwoLetterISOLanguageName() + "/";
                }
            }

            return host;
        }

        private async Task SaveTempAsync(List<string> documents, LanguageData data, int start)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                // Save segment to disk
                var fileName = SiteMapFileNamePattern.FormatInvariant(i + start);
                var filePath = _tenantFolder.Combine(data.TempDir, fileName);

                await _tenantFolder.CreateTextFileAsync(filePath, documents[i]);
            }
        }

        private void SaveFinal(LanguageData data)
        {
            // Delete current sitemap dir
            _tenantFolder.TryDeleteDirectory(data.FinalDir);

            var source = _tenantFolder.MapPath(data.TempDir);
            var dest = _tenantFolder.MapPath(data.FinalDir);

            // Move/Rename new (temp) dir to current
            System.IO.Directory.Move(source, dest);

            int retries = 0;
            while (!SitemapFileExists(data.Store.Id, data.Language.Id, 0, out _, out _))
            {
                if (retries > 20)
                {
                    break;
                }

                // IO breathe: directly after a folder rename a file check fails. Wait a sec...
                Task.Delay(500).Wait();
                retries++;
            }
        }

        private IEnumerable<NodeEntry> EnlistEntities(XmlSitemapProvider[] providers)
        {
            var result = Enumerable.Empty<NodeEntry>();
            foreach (var provider in providers)
            {
                result = result.Concat(provider.Enlist().Select(x => new NodeEntry { Entry = x, Provider = provider }));
            }

            return result;
        }

        private IDictionary<string, UrlRecordCollection> GetUrlRecordCollectionsForBatch(IEnumerable<NamedEntity> batch, int[] languageIds)
        {
            var result = new Dictionary<string, UrlRecordCollection>();

            if (batch.First().EntityName == "Product")
            {
                // nothing comes after product
                int min = batch.Last().Id;
                int max = batch.First().Id;

                result["Product"] = _urlRecordService.GetUrlRecordCollection("Product", languageIds, new[] { min, max }, true, true);
            }

            var entityGroups = batch.ToMultimap(x => x.EntityName, x => x.Id);
            foreach (var group in entityGroups)
            {
                var isRange = group.Key == "Product";
                var entityIds = isRange ? new[] { group.Value.Last(), group.Value.First() } : group.Value.ToArray();

                result[group.Key] = _urlRecordService.GetUrlRecordCollection(group.Key, languageIds, entityIds, isRange, isRange);
            }

            return result;
        }

        protected virtual List<string> GetSiteMapDocuments(IReadOnlyCollection<XmlSitemapNode> nodes)
        {
            int siteMapCount = (int)Math.Ceiling(nodes.Count / (double)MaximumSiteMapNodeCount);
            CheckSitemapCount(siteMapCount);

            var siteMaps = Enumerable
                .Range(0, siteMapCount)
                .Select(x =>
                {
                    return new KeyValuePair<int, IEnumerable<XmlSitemapNode>>(
                        x + 1,
                        nodes.Skip(x * MaximumSiteMapNodeCount).Take(MaximumSiteMapNodeCount));
                });

            var siteMapDocuments = new List<string>(siteMapCount);

            foreach (var kvp in siteMaps)
            {
                siteMapDocuments.Add(this.GetSitemapDocument(kvp.Value));
            }

            return siteMapDocuments;
        }

        /// <summary>
        /// Gets the sitemap XML document for the specified set of nodes.
        /// </summary>
        /// <param name="nodes">The sitemap nodes.</param>
        /// <returns>The sitemap XML document for the specified set of nodes.</returns>
        private string GetSitemapDocument(IEnumerable<XmlSitemapNode> nodes)
        {
            //var languages = _languageService.GetAllLanguages();

            XNamespace ns = SiteMapsNamespace;
            XNamespace xhtml = XhtmlNamespace;

            XElement root = new XElement(
                ns + "urlset",
                new XAttribute(XNamespace.Xmlns + "xhtml", xhtml));

            foreach (var node in nodes)
            {
                // url
                var xel = new XElement
                (
                    ns + "url",
                    // url/loc
                    new XElement(ns + "loc", node.Loc),
                    // url/lastmod
                    node.LastMod == null ? null : new XElement(
                        ns + "lastmod",
                        node.LastMod.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    // url/changefreq
                    node.ChangeFreq == null ? null : new XElement(
                        ns + "changefreq",
                        node.ChangeFreq.Value.ToString().ToLowerInvariant()),
                    // url/priority
                    node.Priority == null ? null : new XElement(
                        ns + "priority",
                        node.Priority.Value.ToString("F1", CultureInfo.InvariantCulture))
                );

                if (node.Links != null)
                {
                    foreach (var entry in node.Links)
                    {
                        // url/xhtml:link[culture]
                        xel.Add(new XElement
                        (
                            xhtml + "link",
                            new XAttribute("rel", "alternate"),
                            new XAttribute("hreflang", entry.Lang),
                            new XAttribute("href", entry.Href)
                        ));
                    }
                }

                root.Add(xel);
            }

            XDeclaration declaration = new XDeclaration("1.0", "UTF-8", "yes");
            XDocument document = new XDocument(root);
            var xml = declaration.ToString() + document.ToString(SaveOptions.DisableFormatting);
            CheckDocumentSize(xml);

            return xml;
        }

        /// <summary>
        /// Gets the sitemap index XML document, containing links to all the sitemap XML documents.
        /// </summary>
        /// <param name="siteMaps">The collection of sitemaps containing their index and nodes.</param>
        /// <returns>The sitemap index XML document, containing links to all the sitemap XML documents.</returns>
        private string CreateSitemapIndexDocument(IEnumerable<XmlSitemapNode> nodes)
        {
            XNamespace ns = SiteMapsNamespace;

            XElement root = new XElement(ns + "sitemapindex");

            foreach (var node in nodes)
            {
                var xel = new XElement(
                    ns + "sitemap",
                    new XElement(ns + "loc", node.Loc),
                    node.LastMod.HasValue ?
                        new XElement(
                            ns + "lastmod",
                            node.LastMod.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")) :
                        null);

                root.Add(xel);
            }

            var document = new XDocument(root);
            var xml = document.ToString(SaveOptions.DisableFormatting);
            CheckDocumentSize(xml);

            return xml;
        }

        private string GetSitemapIndexUrl(int index, string baseUrl)
        {
            var url = _urlHelper.RouteUrl("XmlSitemap", new { index }).TrimStart('/');
            return baseUrl + url;
        }

        private XmlSitemapProvider[] CreateProviders(XmlSitemapBuildContext context)
        {
            return _publishers
                .Select(x => x.Value.PublishXmlSitemap(context))
                .Where(x => x != null)
                .OrderBy(x => x.Order)
                .ToArray();
        }

        protected void ProcessCustomNodes(XmlSitemapBuildContext ctx, Multimap<int, XmlSitemapNode> sitemaps)
        {
            // For inheritors
        }

        /// <summary>
        /// Checks the size of the XML sitemap document. If it is over 10MB, logs an error.
        /// </summary>
        /// <param name="sitemapXml">The sitemap XML document.</param>
        private void CheckDocumentSize(string siteMapXml)
        {
            if (siteMapXml.Length >= MaximumSiteMapSizeInBytes)
            {
                Logger.Error(new InvalidOperationException($"Sitemap exceeds the maximum size of 10MB. This is because you have unusually long URL's. Consider reducing the MaximumSitemapNodeCount. Size:<{siteMapXml.Length}>"));
            }
        }

        /// <summary>
        /// Checks the count of the number of sitemaps. If it is over 50,000, logs an error.
        /// </summary>
        /// <param name="sitemapCount">The sitemap count.</param>
        private void CheckSitemapCount(int sitemapCount)
        {
            if (sitemapCount > MaximumSiteMapCount)
            {
                var ex = new InvalidOperationException($"Sitemap index file exceeds the maximum number of allowed sitemaps of 50,000. Count:<{sitemapCount}>");
                Logger.Warn(ex, ex.Message);
            }
        }

        public bool IsRebuilding(int storeId, int languageId)
        {
            return IsRebuilding(GetLockFilePath(storeId, languageId));
        }

        private bool IsRebuilding(string lockFilePath)
        {
            return _lockFileManager.IsLocked(lockFilePath);
        }

        public virtual bool IsGenerated(int storeId, int languageId)
        {
            return SitemapFileExists(storeId, languageId, 0, out _, out _);
        }

        public virtual void Invalidate(int storeId, int languageId)
        {
            var dir = BuildSitemapDirPath(storeId, languageId);

            if (_tenantFolder.DirectoryExists(dir))
            {
                _tenantFolder.DeleteDirectory(dir);
            }
        }

        #region Nested classes

        struct NodeEntry
        {
            public NamedEntity Entry { get; set; }
            public XmlSitemapProvider Provider { get; set; }
        }

        class LanguageData
        {
            public Store Store { get; set; }
            public Language Language { get; set; }
            public ILockFile LockFile { get; set; }
            public string LockFilePath { get; set; }
            public string TempDir { get; set; }
            public string FinalDir { get; set; }
            public string BaseUrl { get; set; }
        }

        #endregion
    }
}
