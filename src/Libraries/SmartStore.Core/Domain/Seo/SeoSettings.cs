using System;
using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Seo
{
	public class SeoSettings : ISettings
    {
		public SeoSettings()
		{
			PageTitleSeparator = ". ";
			PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename;
			DefaultTitle = "Shop";
			DefaultMetaKeywords = "";
			DefaultMetaDescription = "";
			AllowUnicodeCharsInUrls = false;
			CanonicalHostNameRule = CanonicalHostNameRule.NoRule;
			LoadAllUrlAliasesOnStartup = true;

			XmlSitemapEnabled = true;
			XmlSitemapIncludesCategories = true;
			XmlSitemapIncludesManufacturers = true;
			XmlSitemapIncludesProducts = true;
			XmlSitemapIncludesTopics = true;

			ExtraRobotsDisallows = new List<string> { "/blog/tag/", "/blog/month/", "/producttags/" };

			ReservedUrlRecordSlugs = new List<string>
			{
				"admin",
				"install",
				"recentlyviewedproducts",
				"newproducts",
				"compareproducts",
				"clearcomparelist",
				"setproductreviewhelpfulness",
				"login",
				"register",
				"logout",
				"cart",
				"wishlist",
				"emailwishlist",
				"checkout",
				"contactus",
				"passwordrecovery",
				"subscribenewsletter",
				"blog",
				"boards",
				"inboxupdate",
				"sentupdate",
				"news",
				"sitemap",
				"sitemapseo",
				"search",
				"config",
				"api",
				"odata"
			};

			SeoNameCharConversion = string.Join(Environment.NewLine, new List<string>
			{
				"ä;ae",
				"ö;oe",
				"ü;ue",
				"Ä;Ae",
				"Ö;Oe",
				"Ü;Ue",
				"ß;ss"
			});
		}

		public string PageTitleSeparator { get; set; }
        public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }
        public string DefaultTitle { get; set; }
        public string DefaultMetaKeywords { get; set; }
        public string DefaultMetaDescription { get; set; }
		public string MetaRobotsContent { get; set; }

		public bool ConvertNonWesternChars { get; set; }
        public bool AllowUnicodeCharsInUrls { get; set; }
		public string SeoNameCharConversion { get; set; }

		public bool CanonicalUrlsEnabled { get; set; }
		public CanonicalHostNameRule CanonicalHostNameRule { get; set; }

        /// <summary>
        /// Slugs (sename) reserved for some other needs
        /// </summary>
        public List<string> ReservedUrlRecordSlugs { get; set; }

        public List<string> ExtraRobotsDisallows { get; set; }

		/// <summary>
		/// A value indicating whether to load all URL records and active slugs on application startup
		/// </summary>
		public bool LoadAllUrlAliasesOnStartup { get; set; }

		public bool RedirectLegacyTopicUrls { get; set; }

		#region XML Sitemap

		public bool XmlSitemapEnabled { get; set; }
		public bool XmlSitemapIncludesCategories { get; set; }
		public bool XmlSitemapIncludesManufacturers { get; set; }
		public bool XmlSitemapIncludesProducts { get; set; }
		public bool XmlSitemapIncludesTopics { get; set; }

		#endregion
	}
}