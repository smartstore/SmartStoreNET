
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
			DefaultTitle = "Your store";
			DefaultMetaKeywords = "";
			DefaultMetaDescription = "";
			AllowUnicodeCharsInUrls = true;
			ReservedUrlRecordSlugs = new List<string>() { "admin", "install", "recentlyviewedproducts", "newproducts", "compareproducts", "clearcomparelist", "setproductreviewhelpfulness", "login", "register", "logout", "cart", "wishlist", "emailwishlist", "checkout", "onepagecheckout", "contactus", "passwordrecovery", "subscribenewsletter", "blog", "boards", "inboxupdate", "sentupdate", "news", "sitemap", "sitemapseo", "search", "config", "api", "odata" };
			ExtraRobotsDisallows = new List<string>();
		}
		
		public string PageTitleSeparator { get; set; }
        public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }
        public string DefaultTitle { get; set; }
        public string DefaultMetaKeywords { get; set; }
        public string DefaultMetaDescription { get; set; }

        public bool ConvertNonWesternChars { get; set; }
        public bool AllowUnicodeCharsInUrls { get; set; }

        public bool CanonicalUrlsEnabled { get; set; }

        /// <summary>
        /// Slugs (sename) reserved for some other needs
        /// </summary>
        public List<string> ReservedUrlRecordSlugs { get; set; }

        public List<string> ExtraRobotsDisallows { get; set; }
    }
}