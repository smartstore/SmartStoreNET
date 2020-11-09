using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Autofac;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Catalog;
using SmartStore.Services.Seo;
using SmartStore.Data.Utilities;

namespace SmartStore.Services.Search
{
    public partial class CatalogSearchService : SearchServiceBase, ICatalogSearchService, IXmlSitemapPublisher
    {
        private readonly ICommonServices _services;
        private readonly IIndexManager _indexManager;
        private readonly Lazy<IProductService> _productService;
        private readonly ILogger _logger;
        private readonly IPriceFormatter _priceFormatter;
        private readonly UrlHelper _urlHelper;

        public CatalogSearchService(
            ICommonServices services,
            IIndexManager indexManager,
            Lazy<IProductService> productService,
            ILogger logger,
            IPriceFormatter priceFormatter,
            UrlHelper urlHelper)
        {
            _services = services;
            _indexManager = indexManager;
            _productService = productService;
            _logger = logger;
            _priceFormatter = priceFormatter;
            _urlHelper = urlHelper;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Bypasses the index provider and directly searches in the database
        /// </summary>
        /// <param name="searchQuery">Search query</param>
        /// <param name="loadFlags">LOad flags</param>
        /// <returns>Catalog search result</returns>
        protected virtual CatalogSearchResult SearchDirect(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None)
        {
            // Fallback to linq search.
            var linqCatalogSearchService = _services.Container.ResolveNamed<ICatalogSearchService>("linq");

            var result = linqCatalogSearchService.Search(searchQuery, loadFlags, true);
            ApplyFacetLabels(result.Facets);

            return result;
        }

        protected virtual void ApplyFacetLabels(IDictionary<string, FacetGroup> facets)
        {
            if (facets == null || facets.Count == 0)
            {
                return;
            }

            FacetGroup group;
            var rangeMinTemplate = T("Search.Facet.RangeMin").Text;
            var rangeMaxTemplate = T("Search.Facet.RangeMax").Text;
            var rangeBetweenTemplate = T("Search.Facet.RangeBetween").Text;

            // Apply "price" labels.
            if (facets.TryGetValue("price", out group))
            {
                // TODO: formatting without decimals would be nice
                foreach (var facet in group.Facets)
                {
                    var val = facet.Value;

                    if (val.Value == null && val.UpperValue != null)
                    {
                        val.Label = rangeMaxTemplate.FormatInvariant(FormatPrice(val.UpperValue.Convert<decimal>()));
                    }
                    else if (val.Value != null && val.UpperValue == null)
                    {
                        val.Label = rangeMinTemplate.FormatInvariant(FormatPrice(val.Value.Convert<decimal>()));
                    }
                    else if (val.Value != null && val.UpperValue != null)
                    {
                        val.Label = rangeBetweenTemplate.FormatInvariant(
                            FormatPrice(val.Value.Convert<decimal>()),
                            FormatPrice(val.UpperValue.Convert<decimal>()));
                    }
                }
            }

            // Apply "rating" labels.
            if (facets.TryGetValue("rating", out group))
            {
                foreach (var facet in group.Facets)
                {
                    facet.Value.Label = T(facet.Key == "1" ? "Search.Facet.1StarAndMore" : "Search.Facet.XStarsAndMore", facet.Value.Value).Text;
                }
            }

            // Apply "numeric range" labels.
            var numericRanges = facets
                .Where(x => x.Value.TemplateHint == FacetTemplateHint.NumericRange)
                .Select(x => x.Value);

            foreach (var numericRange in numericRanges)
            {
                foreach (var facet in numericRange.SelectedFacets)
                {
                    var val = facet.Value;
                    var labels = val.Label.SplitSafe("~");

                    if (val.Value == null && val.UpperValue != null)
                    {
                        val.Label = rangeMaxTemplate.FormatInvariant(labels.SafeGet(0));
                    }
                    else if (val.Value != null && val.UpperValue == null)
                    {
                        val.Label = rangeMinTemplate.FormatInvariant(labels.SafeGet(0));
                    }
                    else if (val.Value != null && val.UpperValue != null)
                    {
                        val.Label = rangeBetweenTemplate.FormatInvariant(labels.SafeGet(0), labels.SafeGet(1));
                    }
                }
            }
        }

        protected virtual string FormatPrice(decimal price)
        {
            return _priceFormatter.FormatPrice(price, true, false);
        }

        public CatalogSearchResult Search(
            CatalogSearchQuery searchQuery,
            ProductLoadFlags loadFlags = ProductLoadFlags.None,
            bool direct = false)
        {
            Guard.NotNull(searchQuery, nameof(searchQuery));
            Guard.NotNegative(searchQuery.Take, nameof(searchQuery.Take));

            var provider = _indexManager.GetIndexProvider("Catalog");

            if (!direct && provider != null)
            {
                var indexStore = provider.GetIndexStore("Catalog");
                if (indexStore.Exists)
                {
                    var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);
                    var stepPrefix = searchEngine.GetType().Name + " - ";

                    int totalCount = 0;
                    int[] hitsEntityIds = null;
                    string[] spellCheckerSuggestions = null;
                    IEnumerable<ISearchHit> searchHits;
                    Func<IList<Product>> hitsFactory = null;
                    IDictionary<string, FacetGroup> facets = null;

                    _services.EventPublisher.Publish(new CatalogSearchingEvent(searchQuery, false));

                    if (searchQuery.Take > 0)
                    {
                        using (_services.Chronometer.Step(stepPrefix + "Search"))
                        {
                            totalCount = searchEngine.Count();
                            // Fix paging boundaries
                            if (searchQuery.Skip > 0 && searchQuery.Skip >= totalCount)
                            {
                                searchQuery.Slice((totalCount / searchQuery.Take) * searchQuery.Take, searchQuery.Take);
                            }
                        }

                        if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithHits))
                        {
                            using (_services.Chronometer.Step(stepPrefix + "Hits"))
                            {
                                searchHits = searchEngine.Search();
                            }

                            using (_services.Chronometer.Step(stepPrefix + "Collect"))
                            {
                                hitsEntityIds = searchHits.Select(x => x.EntityId).ToArray();
                                hitsFactory = () => _productService.Value.GetProductsByIds(hitsEntityIds, loadFlags);
                            }
                        }

                        if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithFacets))
                        {
                            try
                            {
                                using (_services.Chronometer.Step(stepPrefix + "Facets"))
                                {
                                    facets = searchEngine.GetFacetMap();
                                    ApplyFacetLabels(facets);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }
                        }
                    }

                    if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithSuggestions))
                    {
                        try
                        {
                            using (_services.Chronometer.Step(stepPrefix + "Spellcheck"))
                            {
                                spellCheckerSuggestions = searchEngine.CheckSpelling();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Spell checking should not break the search.
                            _logger.Error(ex);
                        }
                    }

                    var result = new CatalogSearchResult(
                        searchEngine,
                        searchQuery,
                        totalCount,
                        hitsEntityIds,
                        hitsFactory,
                        spellCheckerSuggestions,
                        facets);

                    var searchedEvent = new CatalogSearchedEvent(searchQuery, result);
                    _services.EventPublisher.Publish(searchedEvent);

                    return searchedEvent.Result;
                }
                else if (searchQuery.Origin.IsCaseInsensitiveEqual("Search/Search"))
                {
                    IndexingRequiredNotification(_services, _urlHelper);
                }
            }

            return SearchDirect(searchQuery);
        }

        public IQueryable<Product> PrepareQuery(CatalogSearchQuery searchQuery, IQueryable<Product> baseQuery = null)
        {
            var linqCatalogSearchService = _services.Container.ResolveNamed<ICatalogSearchService>("linq");
            return linqCatalogSearchService.PrepareQuery(searchQuery, baseQuery);
        }

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesProducts)
                return null;

            var searchQuery = new CatalogSearchQuery()
                .VisibleOnly(_services.WorkContext.CurrentCustomer)
                .WithVisibility(ProductVisibility.Full)
                .HasStoreId(context.RequestStoreId);

            var query = PrepareQuery(searchQuery);

            return new ProductXmlSitemapResult { Query = query, Context = context };
        }

        class ProductXmlSitemapResult : XmlSitemapProvider
        {
            public IQueryable<Product> Query { get; set; }
            public XmlSitemapBuildContext Context { get; set; }

            public override int GetTotalCount()
            {
                return Query.Count();
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                var pager = new FastPager<Product>(Query.AsNoTracking(), Context.MaximumNodeCount);

                while (pager.ReadNextPage(x => new { x.Id, x.UpdatedOnUtc }, x => x.Id, out var products))
                {
                    if (Context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    foreach (var x in products)
                    {
                        yield return new NamedEntity { EntityName = "Product", Id = x.Id, LastMod = x.UpdatedOnUtc };
                    }
                }
            }

            public override int Order => int.MaxValue;
        }

        #endregion
    }
}
