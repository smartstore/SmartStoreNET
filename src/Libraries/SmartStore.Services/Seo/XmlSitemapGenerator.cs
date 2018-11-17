using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Data.Entity;
using System.Web.Mvc;
using System.Xml.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Tasks;
using SmartStore.Services.Topics;
using SmartStore.Core.IO;
using SmartStore.Utilities.Threading;
using SmartStore.Utilities;
using System.IO;
using System.Threading.Tasks;

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
		/// less than 10MB. After some experimentation, a maximum of 1.000 nodes keeps the file size below 10MB.
		/// </summary>
		private const int MaximumSiteMapNodeCount = 1000;

		/// <summary>
		/// The maximum size of a sitemap file in bytes (10MB).
		/// </summary>
		private const int MaximumSiteMapSizeInBytes = 10485760;

		private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
		private readonly ILanguageService _languageService;
		private readonly ICustomerService _customerService;
		private readonly ICatalogSearchService _catalogSearchService;
		private readonly IUrlRecordService _urlRecordService;
		private readonly SeoSettings _seoSettings;
		private readonly SecuritySettings _securitySettings;
		private readonly ICommonServices _services;
		private readonly ILockFileManager _lockFileManager;
		private readonly UrlHelper _urlHelper;

		private readonly int _storeId;
		private readonly int _langId;
		private readonly IVirtualFolder _tenantFolder;
		private readonly string _baseDir;
		private readonly string _siteMapDir;

		public XmlSitemapGenerator(
			ICategoryService categoryService,
            IProductService productService, 
			IManufacturerService manufacturerService,
            ITopicService topicService,
			ILanguageService languageService,
			ICustomerService customerService,
			ICatalogSearchService catalogSearchService,
			IUrlRecordService urlRecordService,
			SeoSettings commonSettings, 
			SecuritySettings securitySettings,
			ICommonServices services,
			ILockFileManager lockFileManager,
			UrlHelper urlHelper)
        {
            _categoryService = categoryService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _topicService = topicService;
			_languageService = languageService;
			_customerService = customerService;
			_catalogSearchService = catalogSearchService;
			_urlRecordService = urlRecordService;
            _seoSettings = commonSettings;
			_securitySettings = securitySettings;
			_services = services;
			_lockFileManager = lockFileManager;
			_urlHelper = urlHelper;

			_storeId = _services.StoreContext.CurrentStore.Id;
			_langId = _services.WorkContext.WorkingLanguage.Id;
			_tenantFolder = _services.ApplicationEnvironment.TenantFolder;
			_baseDir = _tenantFolder.Combine("Sitemaps");
			_siteMapDir = _tenantFolder.Combine(_baseDir, _storeId + "/" + _langId);

			Logger = NullLogger.Instance;
			
		}

		public ILogger Logger { get; set; }

		public virtual XmlSitemapPartition GetSitemapPart(int index = 0)
		{
			return GetSitemapPart(index, false);
		}

		private XmlSitemapPartition GetSitemapPart(int index, bool isRetry)
		{
			Guard.NotNegative(index, nameof(index));

			var exists = SitemapFileExists(index, out var path, out var name);

			if (exists)
			{
				return new XmlSitemapPartition
				{
					Index = index,
					Name = name,
					LanguageId = _langId,
					StoreId = _storeId,
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

				if (SitemapFileExists(0, out path, out name))
				{
					throw new IndexOutOfRangeException("The sitemap file '{0}' does not exist.".FormatInvariant(name));
				}
			}

			// The main sitemap document with index 0 does not exist, meaning: the whole sitemap
			// needs to be created and cached by partitions.

			if (IsRebuilding)
			{
				// The rebuild process is already running, either started
				// by the task scheduler or another HTTP request.
				// We should wait for completion.

				//while (IsRebuilding)
				//{
				//	//Thread.Sleep(500);
				//	Task.Delay(500).Wait();
				//}
			}
			else
			{
				// No lock. Rebuild now.
				Rebuild(CancellationToken.None);
			}

			// DRY: call self to get sitemap partiition object
			return GetSitemapPart(index, true);
		}

		private bool SitemapFileExists(int index, out string path, out string name)
		{
			path = BuildSitemapFilePath(index, out name);

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

		private string BuildSitemapFilePath(int index, out string fileName)
		{
			fileName = SiteMapFileNamePattern.FormatInvariant(index);
			return _tenantFolder.Combine(_siteMapDir, fileName);
		}

		private string GetLockFilePath()
		{
			var fileName = LockFileNamePattern.FormatInvariant(_storeId, _langId);
			return _tenantFolder.Combine(_baseDir, fileName);
		}

		public virtual void Rebuild(CancellationToken cancellationToken, ProgressCallback callback = null)
		{
			if (!_lockFileManager.TryAcquireLock(GetLockFilePath(), out var lockFile))
			{
				Logger.Warn("XML Sitemap rebuild already in process.");
				return;
			}

			using (lockFile)
			{
				// Impersonate
				var prevCustomer = _services.WorkContext.CurrentCustomer;
				// no need to vary xml sitemap by customer roles: it's relevant to crawlers only.
				_services.WorkContext.CurrentCustomer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);

				try
				{
					var protocol = _services.StoreContext.CurrentStore.ForceSslForAllPages ? "https" : "http";
					var nodes = new List<XmlSitemapNode>();
					var queries = CreateQueries();
					var total = queries.GetTotalRecordCount();

					using (new DbContextScope(autoDetectChanges: false, forceNoTracking: true, proxyCreation: false, lazyLoading: false))
					{
						var entities = EnumerateEntities(queries);

						var segment = 0;
						var numProcessed = 0;
						foreach (var batch in entities.Slice(MaximumSiteMapNodeCount))
						{
							if (cancellationToken.IsCancellationRequested)
							{
								break;
							}

							numProcessed = ++segment * MaximumSiteMapNodeCount;
							callback?.Invoke(numProcessed, total, "{0} / {1}".FormatCurrent(numProcessed, total));

							var firstEntityName = batch.First().EntityName;
							var lastEntityName = batch.Last().EntityName;

							var slugs = GetUrlRecordCollectionsForBatch(batch, _langId);

							nodes.AddRange(batch.Select(x => new XmlSitemapNode
							{
								LastMod = x.LastMod,
								Loc = _urlHelper.RouteUrl(x.EntityName, new { SeName = slugs[x.EntityName].GetSlug(_langId, x.Id, true) }, protocol)
							}));
						}

						if (!cancellationToken.IsCancellationRequested)
						{
							callback?.Invoke(numProcessed, total, "Processing custom nodes".FormatCurrent(numProcessed, total));
							var customNodes = GetCustomNodes(protocol);
							if (customNodes != null)
							{
								nodes.AddRange(customNodes);
							}
						}
					}

					cancellationToken.ThrowIfCancellationRequested();

					callback?.Invoke(total, total, "Finalizing");

					if (nodes.Count == 0)
					{
						// Ensure that at least one entry exists. Otherwise,
						// the system will try to rebuild again.
						nodes.Add(new XmlSitemapNode { LastMod = DateTime.UtcNow, Loc = _urlHelper.RouteUrl("HomePage") });
					}

					var documents = GetSiteMapDocuments(nodes.AsReadOnly(), protocol);

					cancellationToken.ThrowIfCancellationRequested();

					SaveToDisk(documents);
				}
				finally
				{
					// Undo impersonation
					_services.WorkContext.CurrentCustomer = prevCustomer;
				}
			}
		}

		private void SaveToDisk(List<string> documents)
		{
			if (_tenantFolder.DirectoryExists(_siteMapDir))
			{
				_tenantFolder.DeleteDirectory(_siteMapDir);
			}

			_tenantFolder.CreateDirectory(_siteMapDir);

			for (int i = 0; i < documents.Count; i++)
			{
				// Save segment to disk
				var fileName = SiteMapFileNamePattern.FormatInvariant(i);
				var filePath = _tenantFolder.Combine(_siteMapDir, fileName);

				_tenantFolder.CreateTextFile(filePath, documents[i]);
			}
		}

		private IEnumerable<NamedEntity> EnumerateEntities(QueryHolder queries)
		{
			var entities = Enumerable.Empty<NamedEntity>();

			if (queries.Categories != null)
			{
				var categories = queries.Categories.Select(x => new { x.Id, x.UpdatedOnUtc }).ToList();
				foreach (var x in categories)
				{
					yield return new NamedEntity { EntityName = "Category", Id = x.Id, LastMod = x.UpdatedOnUtc };
				}
			}

			if (queries.Manufacturers != null)
			{
				var manufacturers = queries.Manufacturers.Select(x => new { x.Id, x.UpdatedOnUtc }).ToList();
				foreach (var x in manufacturers)
				{
					yield return new NamedEntity { EntityName = "Manufacturer", Id = x.Id, LastMod = x.UpdatedOnUtc };
				}
			}

			if (queries.Topics != null)
			{
				var topics = queries.Topics.Select(x => new { x.Id }).ToList();
				foreach (var x in topics)
				{
					yield return new NamedEntity { EntityName = "Topic", Id = x.Id, LastMod = DateTime.UtcNow };
				}
			}

			if (queries.Products != null)
			{
				var query = queries.Products.AsNoTracking();

				var maxId = int.MaxValue;
				int xxx = 0;
				while (maxId > 1)
				{
					xxx++;
					if (xxx >= 100)
					{
						break;
					}

					var products = queries.Products.AsNoTracking()
						.Where(x => x.Id < maxId)
						.OrderByDescending(x => x.Id)
						.Take(() => 1000)
						.Select(x => new { x.Id, x.UpdatedOnUtc })
						.ToList();

					if (products.Count == 0)
					{
						break;
					}

					maxId = products.Last().Id;

					foreach (var x in products)
					{
						yield return new NamedEntity { EntityName = "Product", Id = x.Id, LastMod = x.UpdatedOnUtc };
					}
				}
			}
		}

		private IDictionary<string, UrlRecordCollection> GetUrlRecordCollectionsForBatch(IEnumerable<NamedEntity> batch, int languageId)
		{
			var result = new Dictionary<string, UrlRecordCollection>();
			var languageIds = new[] { languageId, 0 };

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

		protected virtual List<string> GetSiteMapDocuments(IReadOnlyCollection<XmlSitemapNode> nodes, string protocol)
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

			if (siteMapCount > 1)
			{
				var xml = this.GetSitemapIndexDocument(siteMaps, protocol);
				siteMapDocuments.Add(xml);
			}

			foreach (var kvp in siteMaps)
			{
				var xml = this.GetSitemapDocument(kvp.Value);
				siteMapDocuments.Add(xml);
			}

			return siteMapDocuments;
		}

		/// <summary>
		/// Gets the sitemap index XML document, containing links to all the sitemap XML documents.
		/// </summary>
		/// <param name="siteMaps">The collection of sitemaps containing their index and nodes.</param>
		/// <returns>The sitemap index XML document, containing links to all the sitemap XML documents.</returns>
		private string GetSitemapIndexDocument(IEnumerable<KeyValuePair<int, IEnumerable<XmlSitemapNode>>> siteMaps, string protocol)
		{
			XNamespace ns = SiteMapsNamespace;

			XElement root = new XElement(ns + "sitemapindex");

			foreach (KeyValuePair<int, IEnumerable<XmlSitemapNode>> map in siteMaps)
			{
				// Get the latest LastModified DateTime from the sitemap nodes or null if there is none.
				DateTime? lastModified = map.Value
					.Select(x => x.LastMod)
					.Where(x => x.HasValue)
					.DefaultIfEmpty()
					.Max();

				var xel = new XElement(
					ns + "sitemap",
					new XElement(ns + "loc", this.GetSitemapUrl(map.Key, protocol)),
					lastModified.HasValue ?
						new XElement(
							ns + "lastmod",
							lastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")) :
						null);

				root.Add(xel);
			}

			var document = new XDocument(root);
			var xml = document.ToString(SaveOptions.DisableFormatting);
			CheckDocumentSize(xml);

			return xml;
		}

		private string GetSitemapUrl(int index, string protocol)
		{
			var url = _urlHelper.RouteUrl("XmlSitemap", new { index }, protocol);
			return url;
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

			XDocument document = new XDocument(root);
			var xml = document.ToString(SaveOptions.DisableFormatting);
			CheckDocumentSize(xml);

			return xml;
		}

		private QueryHolder CreateQueries()
		{
			var holder = new QueryHolder();

			if (_seoSettings.XmlSitemapIncludesCategories)
			{
				holder.Categories = _categoryService.GetAllCategories(showHidden: false, storeId: _services.StoreContext.CurrentStore.Id).SourceQuery;
			}

			if (_seoSettings.XmlSitemapIncludesManufacturers)
			{
				holder.Manufacturers = _manufacturerService.GetManufacturers(false).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name);
			}

			if (_seoSettings.XmlSitemapIncludesTopics)
			{
				holder.Topics = _topicService.GetAllTopics(_services.StoreContext.CurrentStore.Id).AlterQuery(q =>
				{
					return q.Where(t => t.IncludeInSitemap && !t.RenderAsWidget);
				}).SourceQuery;
			}

			if (_seoSettings.XmlSitemapIncludesProducts)
			{
				var searchQuery = new CatalogSearchQuery()
					.VisibleOnly()
					.VisibleIndividuallyOnly(true)
					.HasStoreId(_services.StoreContext.CurrentStoreIdIfMultiStoreMode);

				holder.Products = _catalogSearchService.PrepareQuery(searchQuery);
			}

			return holder;
		}

		protected virtual IEnumerable<XmlSitemapNode> GetCustomNodes(string protocol)
		{
			return Enumerable.Empty<XmlSitemapNode>();
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

		public bool IsRebuilding
		{
			get
			{
				return _lockFileManager.IsLocked(GetLockFilePath());
			}
		}

		public virtual bool IsGenerated
		{
			get
			{
				return SitemapFileExists(0, out _, out _);
			}
		}

		public virtual void Invalidate()
		{
			if (_tenantFolder.DirectoryExists(_siteMapDir))
			{
				_tenantFolder.DeleteDirectory(_siteMapDir);
			}
		}

		#region Nested classes

		class QueryHolder
		{
			public IQueryable<Category> Categories { get; set; }
			public IQueryable<Manufacturer> Manufacturers { get; set; }
			public IQueryable<Topic> Topics { get; set; }
			public IQueryable<Product> Products { get; set; }

			public int GetTotalRecordCount()
			{
				int num = 0;
				if (Categories != null) num += Categories.Count();
				if (Manufacturers != null) num += Manufacturers.Count();
				if (Topics != null) num += Topics.Count();
				if (Products != null) num += Products.Count();

				return num;
			}
		}

		class NamedEntity : BaseEntity, ISlugSupported
		{
			public string EntityName { get; set; }
			public DateTime LastMod { get; set; }

			public override string GetEntityName()
			{
				return EntityName;
			}
		}

		#endregion
	}
}
