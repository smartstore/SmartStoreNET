using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class PerformanceSettings : ISettings
    {
        /// <summary>
        /// The number of entries in a single cache segment
        /// when greedy loading is disabled. The larger the catalog,
        /// the smaller this value should be. We recommend segment
        /// size 500 for catalogs smaller than 100.000 items.
        /// </summary>
        public int CacheSegmentSize { get; set; } = 500;

        /// <summary>
        /// By default only instant search prefetches translations.
        /// All other product listings work against the segmented cache.
        /// In very large multilingual catalogs (> 500.000) setting this to true
        /// can result in higher request performance as well as less
        /// resource usage.
        /// </summary>
        public bool AlwaysPrefetchTranslations { get; set; }

        /// <summary>
        /// By default only instant search prefetches url slugs.
        /// All other product listings work against the segmented cache.
        /// In very large catalogs (> 500.000) setting this to true
        /// can result in higher request performance as well as less
        /// resource usage.
        /// </summary>
        public bool AlwaysPrefetchUrlSlugs { get; set; }

        /// <summary>
        /// Maximum number of attribute combinations to be loaded and parsed
        /// to make them unavailable for selection on the product detail page.
        /// </summary>
        public int MaxUnavailableAttributeCombinations { get; set; } = 10000;
    }
}
