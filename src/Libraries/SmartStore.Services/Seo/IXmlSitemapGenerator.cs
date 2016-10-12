using System.Collections.Generic;

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
		/// Removes the sitemap from the cache for a rebuild.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Indicates whether the sitemap has been generated and cached.
		/// </summary>
		bool IsGenerated { get; }
	}
}
