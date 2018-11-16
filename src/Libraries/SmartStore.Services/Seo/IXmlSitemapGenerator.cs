using SmartStore.Services.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial interface IXmlSitemapGenerator
    {
		/// <summary>
		/// Gets the sitemap XML
		/// </summary>
		/// <param name="index">
		/// The page index. 0 or <c>null</c> retrieves the first document, which can be a sitemap INDEX document
		/// (when the sitemap size exceeded the limits). An index greater 0 retrieves the
		/// sitemap XML document at this index, but only when the sitemap is actually indexed (otherwise <c>null</c> is returned)
		/// </param>
		/// <returns>Sitemap XML</returns>
		string GetSitemap(int? index = null);

		/// <summary>
		/// Rebuilds the collection of XML sitemap documents for the current site. If there are less than 1.000 sitemap 
		/// nodes, only one sitemap document will exist in the collection, otherwise a sitemap index document will be 
		/// the first entry in the collection and all other entries will be sitemap XML documents.
		/// </summary>
		/// <param name="callback">Optional callback for progress change</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <remarks>
		/// During rebuilding, requests are being served from the existing cache.
		/// Once rebuild is completed, the cache is updated.
		/// </remarks>
		void Rebuild(CancellationToken cancellationToken, ProgressCallback callback = null);

		/// <summary>
		/// Removes the sitemap from the cache for a rebuild.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Indicates whether the sitemap has been generated and cached.
		/// </summary>
		bool IsGenerated { get; }
	}
}
