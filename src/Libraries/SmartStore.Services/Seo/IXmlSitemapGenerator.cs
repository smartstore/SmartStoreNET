using SmartStore.Services.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial interface IXmlSitemapGenerator
    {
        /// <summary>
        /// Gets the sitemap partition
        /// </summary>
        /// <param name="index">
        /// The page index. 0 retrieves the first document, which can be a sitemap INDEX document
        /// (when the sitemap size exceeded the limits). An index greater 0 retrieves the
        /// sitemap XML document at this index, but only when the sitemap is actually indexed (otherwise <c>null</c> is returned)
        /// </param>
        /// <returns>Sitemap partition</returns>
        Task<XmlSitemapPartition> GetSitemapPartAsync(int index = 0);

        /// <summary>
        /// Rebuilds the collection of XML sitemap documents for a store/language combination. If there are less than 1.000 sitemap 
        /// nodes, only one sitemap document will exist in the collection, otherwise a sitemap index document will be 
        /// the first entry in the collection and all other entries will be sitemap XML documents.
        /// </summary>
        /// <param name="ctx">The build context</param>
        /// <remarks>
        /// During rebuilding, requests are being served from the existing cache.
        /// Once rebuild is completed, the cache is updated.
        /// </remarks>
        Task RebuildAsync(XmlSitemapBuildContext ctx);

        /// <summary>
        /// Determines whether a rebuild is already running.
        /// </summary>
        bool IsRebuilding(int storeId, int languageId);

        /// <summary>
        /// Indicates whether the sitemap has been generated and cached.
        /// </summary>
        bool IsGenerated(int storeId, int languageId);

        /// <summary>
        /// Removes the sitemap from the cache for a rebuild.
        /// </summary>
        void Invalidate(int storeId, int languageId);
    }
}
