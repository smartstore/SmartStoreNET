using System;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Web.Infrastructure
{
    public partial class StoreRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			var idConstraint = new MinRouteConstraint(1);
			
			/* Common
			----------------------------------------*/
			
            routes.MapLocalizedRoute("HomePage",
				"",
				new { controller = "Home", action = "Index"},
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Register",
				"register/",
				new { controller = "Customer", action = "Register" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Login",
				"login/",
				new { controller = "Customer", action = "Login" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Logout",
				"logout/",
				new { controller = "Customer", action = "Logout" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ContactUs",
				"contactus",
				new { controller = "Home", action = "ContactUs" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ShoppingCart",
				"cart/",
				new { controller = "ShoppingCart", action = "Cart" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Wishlist",
				"wishlist/{customerGuid}",
				new { controller = "ShoppingCart", action = "Wishlist", customerGuid = UrlParameter.Optional },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Topic",
				"t/{SystemName}",
				new { controller = "Topic", action = "TopicDetails" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("TopicPopup",
				"t-popup/{SystemName}",
				new { controller = "Topic", action = "TopicDetailsPopup" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Search",
				"search/",
				new { controller = "Search", action = "Search" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("InstantSearch",
				"instantsearch",
				new { controller = "Search", action = "InstantSearch" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeDevice",
				"changedevice/{dontusemobileversion}",
				new { controller = "Common", action = "ChangeDevice" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeCurrency",
				"changecurrency/{customercurrency}",
				new { controller = "Common", action = "CurrencySelected" },
				new { customercurrency = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapRoute("ChangeLanguage",
				"changelanguage/{langid}",
				new { controller = "Common", action = "SetLanguage" },
				new { langid = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeTaxType",
				"changetaxtype/{customertaxtype}",
				new { controller = "Common", action = "TaxTypeSelected" },
				new { customertaxtype = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

			/* Catalog
			----------------------------------------*/

			routes.MapLocalizedRoute("ManufacturerList",
				"manufacturer/all/",
				new { controller = "Catalog", action = "ManufacturerAll" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ProductsByTag",
				"producttag/{productTagId}/{SeName}",
				new { controller = "Catalog", action = "ProductsByTag", SeName = UrlParameter.Optional },
				new { productTagId = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ProductTagsAll",
				"producttag/all/",
				new { controller = "Catalog", action = "ProductTagsAll" },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("RecentlyViewedProducts",
                "recentlyviewedproducts/",
				new { controller = "Catalog", action = "RecentlyViewedProducts" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("RecentlyAddedProducts",
                "newproducts/",
				new { controller = "Catalog", action = "RecentlyAddedProducts" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("RecentlyAddedProductsRSS",
                "newproducts/rss",
				new { controller = "Catalog", action = "RecentlyAddedProductsRss" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("CompareProducts",
                "compareproducts/",
				new { controller = "Catalog", action = "CompareProducts" },
                new[] { "SmartStore.Web.Controllers" });


			/* Shopping Cart
			----------------------------------------*/

			// add product to cart (without any attributes and options). used on catalog pages.
			routes.MapLocalizedRoute("AddProductToCartSimple",
				"cart/addproductsimple/{productId}",
				new { controller = "ShoppingCart", action = "AddProductSimple" },
				new { productId = idConstraint },
                new[] { "SmartStore.Web.Controllers" });

            // add product to cart (with attributes and options). used on the product details pages.
			routes.MapLocalizedRoute("AddProductToCart",
				"cart/addproduct/{productId}/{shoppingCartTypeId}",
				new { controller = "ShoppingCart", action = "AddProduct" },
				new { productId = idConstraint, shoppingCartTypeId = idConstraint },
                new[] { "SmartStore.Web.Controllers" });


			/* Checkout
			----------------------------------------*/

            routes.MapLocalizedRoute("Checkout",
                "checkout/",
                new { controller = "Checkout", action = "Index" },
                new[] { "SmartStore.Web.Controllers" });


			/* Newsletter
			----------------------------------------*/

            routes.MapLocalizedRoute("NewsletterActivation",
                "newsletter/subscriptionactivation/{token}/{active}",
                new { controller = "Newsletter", action = "SubscriptionActivation" },
                new { token = new GuidConstraint(false) },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("SubscribeNewsletter", // COMPAT: subscribenewsletter >> newsletter/subscribe
                "Newsletter/Subscribe",
				new { controller = "Newsletter", action = "Subscribe" },
                new[] { "SmartStore.Web.Controllers" });


			/* Customer
			----------------------------------------*/

            routes.MapLocalizedRoute("AccountActivation",
                "customer/activation",
                new { controller = "Customer", action = "AccountActivation" },                            
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("CustomerProfile",
                "profile/{id}",
                new { controller = "Profile", action = "Index", id = UrlParameter.Optional },
				new { id = idConstraint },
                new[] { "SmartStore.Web.Controllers" });


			/* Blog
			----------------------------------------*/

            routes.MapLocalizedRoute("Blog",
                "blog",
                new { controller = "Blog", action = "List" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BlogByTag",
                "blog/tag/{tag}",
                new { controller = "Blog", action = "BlogByTag" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BlogByMonth",
                "blog/month/{month}",
                new { controller = "Blog", action = "BlogByMonth" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BlogRSS",
                "blog/rss/{languageId}",
                new { controller = "Blog", action = "ListRss" },
				new { languageId = idConstraint },
                new[] { "SmartStore.Web.Controllers" });


			/* Boards
			----------------------------------------*/

            routes.MapLocalizedRoute("Boards",
                "boards",
                new { controller = "Boards", action = "Index" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("BoardPostCreate",
				"boards/postcreate/{id}/{quote}",
				new { controller = "Boards", action = "PostCreate", quote = UrlParameter.Optional },
				new { id = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("TopicSlug",
                "boards/topic/{id}/{slug}",
                new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional },
				new { id = idConstraint },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("TopicSlugPaged",
				"boards/topic/{id}/{slug}/page/{page}",
				new { controller = "Boards", action = "Topic" },
				new { id = idConstraint, page = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("ForumSlug",
                "boards/forum/{id}/{slug}",
                new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional },
				new { id = idConstraint },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ForumSlugPaged",
				"boards/forum/{id}/{slug}/page/{page}",
				new { controller = "Boards", action = "Forum" },
				new { id = idConstraint, page = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("ForumGroupSlug",
				"boards/forumgroup/{id}/{slug}",
				new { controller = "Boards", action = "ForumGroup", slug = UrlParameter.Optional },
				new { id = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BoardSearch",
                "boards/search",
                new { controller = "Boards", action = "Search" },
                new[] { "SmartStore.Web.Controllers" });


			/* Misc
			----------------------------------------*/

			routes.MapLocalizedRoute("RegisterResult",
				"registerresult/{resultId}",
				new { controller = "Customer", action = "RegisterResult" },
				new { resultId = idConstraint },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("PrivateMessages",
                "privatemessages/{tab}",
                new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
				new { tab = @"inbox|sent" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("NewsArchive",
                "news",
                new { controller = "News", action = "List" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("NewsRss",
				"news/rss/{languageId}",
				new { controller = "News", action = "rss", languageId = UrlParameter.Optional },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("Sitemap",
                "sitemap",
                new { controller = "Home", action = "Sitemap" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("SitemapSEO",
                "sitemap.xml",
				new { controller = "Home", action = "SitemapSeo" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("StoreClosed",
				"storeclosed",
				new { controller = "Home", action = "StoreClosed" },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapRoute("robots.txt",
                "robots.txt",
                new { controller = "Common", action = "RobotsTextFile" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Settings",
				"settings",
				new { controller = "Common", action = "Settings" },
				new[] { "SmartStore.Web.Controllers" });

        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }

}
