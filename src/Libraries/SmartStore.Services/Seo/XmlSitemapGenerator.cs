using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Topics;

namespace SmartStore.Services.Seo
{
	public partial class XmlSitemapGenerator : IXmlSitemapGenerator
    {
		/// <summary>
		/// Key for seo sitemap
		/// </summary>
		/// <remarks>
		/// {0} : sitemap index
		/// {1} : current store id
		/// {2} : current language id
		/// </remarks>
		public const string XMLSITEMAP_DOCUMENT_KEY = "sitemap:xml-idx{0}-{1}-{2}";
		public const string XMLSITEMAP_PATTERN_KEY = "sitemap:xml*";

		private const string SiteMapsNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
		private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

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
		private readonly SeoSettings _seoSettings;
		private readonly SecuritySettings _securitySettings;
		private readonly ICommonServices _services;
		private readonly UrlHelper _urlHelper;

		public XmlSitemapGenerator(
			ICategoryService categoryService,
            IProductService productService, 
			IManufacturerService manufacturerService,
            ITopicService topicService,
			ILanguageService languageService,
			ICustomerService customerService,
			ICatalogSearchService catalogSearchService,
			SeoSettings commonSettings, 
			SecuritySettings securitySettings,
			ICommonServices services,
			UrlHelper urlHelper)
        {
            _categoryService = categoryService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _topicService = topicService;
			_languageService = languageService;
			_customerService = customerService;
			_catalogSearchService = catalogSearchService;
            _seoSettings = commonSettings;
			_securitySettings = securitySettings;
			_services = services;
			_urlHelper = urlHelper;

			Logger = NullLogger.Instance;
        }

		public ILogger Logger
		{
			get;
			set;
		}

		public void Invalidate()
		{
			_services.Cache.RemoveByPattern(XMLSITEMAP_PATTERN_KEY);
		}

		public bool IsGenerated
		{
			get
			{
				string cacheKey = XMLSITEMAP_DOCUMENT_KEY.FormatInvariant(0, 
					_services.StoreContext.CurrentStore.Id, 
					_services.WorkContext.WorkingLanguage.Id);

				return _services.Cache.Contains(cacheKey);
			}
		}

		public string GetSitemap(int? index = null)
		{
			var storeId = _services.StoreContext.CurrentStore.Id;
			var langId = _services.WorkContext.WorkingLanguage.Id;
			var cache = _services.Cache;		

			string cacheKey = XMLSITEMAP_DOCUMENT_KEY.FormatInvariant(0, storeId, langId);

			if (!cache.Contains(cacheKey))
			{
				// The main sitemap document with index 0 does not exist, meaning: the whole sitemap
				// needs to be created and cached by partitions.
				lock (String.Intern(cacheKey))
				{
					var prevCustomer = _services.WorkContext.CurrentCustomer;
					var bot = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);

					try
					{
						// no need to vary xml sitemap by customer roles: it's relevant to crawlers only.
						_services.WorkContext.CurrentCustomer = bot;

						// we need a scoped lock, because we're going to split the cache entries.
						var documents = Generate();

						for (int i = 0; i < documents.Count; i++)
						{
							// Put segment into cache
							cacheKey = XMLSITEMAP_DOCUMENT_KEY.FormatInvariant(i, storeId, langId);
							cache.Put(cacheKey, documents[i], TimeSpan.FromDays(1));
						}
					}
					finally
					{
						// Undo impersonation
						_services.WorkContext.CurrentCustomer = prevCustomer;
					}
				}
			}

			var page = index ?? 0;
			cacheKey = XMLSITEMAP_DOCUMENT_KEY.FormatInvariant(page, storeId, langId);
			
			if (cache.Contains(cacheKey))
			{
				return cache.Get<string>(cacheKey);
			}

			return null;
		}

		/// <summary>
		/// Gets the collection of XML sitemap documents for the current site. If there are less than 1.000 sitemap 
		/// nodes, only one sitemap document will exist in the collection, otherwise a sitemap index document will be 
		/// the first entry in the collection and all other entries will be sitemap XML documents.
		/// </summary>
		/// <returns>A collection of XML sitemap documents.</returns>
		protected IList<string> Generate()
		{
			var protocol = _services.StoreContext.CurrentStore.ForceSslForAllPages ? "https" : "http";

			var nodes = new List<XmlSitemapNode>();

			using (var scope = new DbContextScope(autoDetectChanges: false, forceNoTracking: true, proxyCreation: false, lazyLoading: false))
			{

				if (_seoSettings.XmlSitemapIncludesCategories)
				{
					nodes.AddRange(GetCategoryNodes(0, protocol));
				}

				if (_seoSettings.XmlSitemapIncludesManufacturers)
				{
					nodes.AddRange(GetManufacturerNodes(protocol));
				}

				if (_seoSettings.XmlSitemapIncludesTopics)
				{
					nodes.AddRange(GetTopicNodes(protocol));
				}

				if (_seoSettings.XmlSitemapIncludesProducts)
				{
					nodes.AddRange(GetProductNodes(protocol));
				}
			}

			var customNodes = GetCustomNodes(protocol);
			if (customNodes != null)
			{
				nodes.AddRange(customNodes);
			}

			var documents = GetSiteMapDocuments(nodes.AsReadOnly());

			return documents;
		}

		protected virtual List<string> GetSiteMapDocuments(IReadOnlyCollection<XmlSitemapNode> nodes)
		{
			var protocol = _services.StoreContext.CurrentStore.ForceSslForAllPages ? "https" : "http";

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
			var url = _urlHelper.RouteUrl("SitemapSEO", new { index = index }, protocol);
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

		protected virtual IEnumerable<XmlSitemapNode> GetCategoryNodes(int parentCategoryId, string protocol)
		{
			var categories = _categoryService.GetAllCategories(showHidden: false, storeId: _services.StoreContext.CurrentStore.Id);

			_services.DbContext.DetachAll();

			return categories.Select(x => 
			{
				var node = new XmlSitemapNode
				{
					Loc = _urlHelper.RouteUrl("Category", new { SeName = x.GetSeName() }, protocol),
					LastMod = x.UpdatedOnUtc,
					//ChangeFreq = ChangeFrequency.Weekly,
					//Priority = 0.8f
				};

				// TODO: add hreflang links if LangCount is > 1 and PrependSeoCode is true

				return node;
			});
		}

		protected virtual IEnumerable<XmlSitemapNode> GetManufacturerNodes(string protocol)
		{
			var manufacturers = _manufacturerService.GetAllManufacturers(false);

			_services.DbContext.DetachAll();

			return manufacturers.Select(x =>
			{
				var node = new XmlSitemapNode
				{
					Loc = _urlHelper.RouteUrl("Manufacturer", new { SeName = x.GetSeName() }, protocol),
					LastMod = x.UpdatedOnUtc,
					//ChangeFreq = ChangeFrequency.Weekly,
					//Priority = 0.8f
				};

				// TODO: add hreflang links if LangCount is > 1 and PrependSeoCode is true

				return node;
			});
		}

		protected virtual IEnumerable<XmlSitemapNode> GetTopicNodes(string protocol)
		{
			var topics = _topicService.GetAllTopics(_services.StoreContext.CurrentStore.Id).ToList().FindAll(t => t.IncludeInSitemap && !t.RenderAsWidget);

			_services.DbContext.DetachAll();

			return topics.Select(x =>
			{
				var node = new XmlSitemapNode
				{
					Loc = _urlHelper.RouteUrl("Topic", new { SeName = x.GetSeName() }, protocol),
					LastMod = DateTime.UtcNow,
					//ChangeFreq = ChangeFrequency.Weekly,
					//Priority = 0.8f
				};

				// TODO: add hreflang links if LangCount is > 1 and PrependSeoCode is true

				return node;
			});
		}

		protected virtual IEnumerable<XmlSitemapNode> GetProductNodes(string protocol)
		{
			var nodes = new List<XmlSitemapNode>();

			var searchQuery = new CatalogSearchQuery()
				.VisibleOnly()
				.VisibleIndividuallyOnly(true)
				.HasStoreId(_services.StoreContext.CurrentStoreIdIfMultiStoreMode);

			var query = _catalogSearchService.PrepareQuery(searchQuery);
			query = query.OrderByDescending(x => x.CreatedOnUtc);

			for (var pageIndex = 0; pageIndex < 9999999; ++pageIndex)
			{
				var products = new PagedList<Product>(query, pageIndex, 1000);

				nodes.AddRange(products.Select(x =>
				{
					var node = new XmlSitemapNode
					{
						Loc = _urlHelper.RouteUrl("Product", new { SeName = x.GetSeName() }, protocol),
						LastMod = x.UpdatedOnUtc,
						//ChangeFreq = ChangeFrequency.Weekly,
						//Priority = 0.8f
					};

					// TODO: add hreflang links if LangCount is > 1 and PrependSeoCode is true
					return node;
				}));

				_services.DbContext.DetachAll();

				if (!products.HasNextPage)
					break;
			}

			return nodes;
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
    }
}
