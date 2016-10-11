using System.Collections.Generic;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial interface IXmlSitemapGenerator
    {
		/// <summary>
		/// Gets the collection of XML sitemap documents for the current site. If there are less than 1.000 sitemap 
		/// nodes, only one sitemap document will exist in the collection, otherwise a sitemap index document will be 
		/// the first entry in the collection and all other entries will be sitemap XML documents.
		/// </summary>
		/// <returns>A collection of XML sitemap documents.</returns>
		IList<string> Generate();
	}
}
