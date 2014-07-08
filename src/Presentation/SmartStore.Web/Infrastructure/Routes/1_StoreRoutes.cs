using System;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Web.Infrastructure
{
    public partial class StoreRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
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

			routes.MapLocalizedRoute("MyAccount",
				"customer/myaccount",
				new { controller = "Customer", action = "MyAccount" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Topic",
				"t/{SystemName}",
				new { controller = "Topic", action = "TopicDetails" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("TopicPopup",
				"t-popup/{SystemName}",
				new { controller = "Topic", action = "TopicDetailsPopup" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ProductSearch",
				"search/",
				new { controller = "Catalog", action = "Search" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ProductSearchAutoComplete",
				"catalog/searchtermautocomplete",
				new { controller = "Catalog", action = "SearchTermAutoComplete" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeDevice",
				"changedevice/{dontusemobileversion}",
				new { controller = "Common", action = "ChangeDevice" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeCurrency",
				"changecurrency/{customercurrency}",
				new { controller = "Common", action = "CurrencySelected" },
				new { customercurrency = @"\d+" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapRoute("ChangeLanguage",
				"changelanguage/{langid}",
				new { controller = "Common", action = "SetLanguage" },
				new { langid = @"\d+" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ChangeTaxType",
				"changetaxtype/{customertaxtype}",
				new { controller = "Common", action = "TaxTypeSelected" },
				new { customertaxtype = @"\d+" },
				new[] { "SmartStore.Web.Controllers" });


			/* Catalog
			----------------------------------------*/

			// manufacturer list (TODO: Controller)
			routes.MapLocalizedRoute("ManufacturerList",
				"manufacturer/all/",
				new { controller = "Catalog", action = "ManufacturerAll" },
				new[] { "SmartStore.Web.Controllers" });

			// product reviews (TODO: Controller)
			routes.MapLocalizedRoute("ProductReviews",
				"productreviews/{productId}",
				new { controller = "Catalog", action = "ProductReviews" },
				new[] { "SmartStore.Web.Controllers" });

			// products (TODO: Controller)
            routes.MapLocalizedRoute("RecentlyViewedProducts",
                "recentlyviewedproducts/",
                new { controller = "Catalog", action = "RecentlyViewedProducts" },
                new[] { "SmartStore.Web.Controllers" });

			// (TODO: Controller)
            routes.MapLocalizedRoute("RecentlyAddedProducts",
                "newproducts/",
                new { controller = "Catalog", action = "RecentlyAddedProducts" },
                new[] { "SmartStore.Web.Controllers" });

			// (TODO: Controller)
            routes.MapLocalizedRoute("RecentlyAddedProductsRSS",
                "newproducts/rss",
                new { controller = "Catalog", action = "RecentlyAddedProductsRss" },
                new[] { "SmartStore.Web.Controllers" });

			// comparing products (TODO: Controller)
            routes.MapLocalizedRoute("AddProductToCompare",
                "compareproducts/add/{productId}",
                new { controller = "Catalog", action = "AddProductToCompareList" },
                new { productId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

			// (TODO: Controller)
            routes.MapLocalizedRoute("CompareProducts",
                "compareproducts/",
                new { controller = "Catalog", action = "CompareProducts" },
                new[] { "SmartStore.Web.Controllers" });

			// (TODO: Controller)
            routes.MapLocalizedRoute("RemoveProductFromCompareList",
                "compareproducts/remove/{productId}",
                new { controller = "Catalog", action = "RemoveProductFromCompareList"},
                new[] { "SmartStore.Web.Controllers" });

			// (TODO: Controller)
            routes.MapLocalizedRoute("ClearCompareList",
                "clearcomparelist/",
                new { controller = "Catalog", action = "ClearCompareList" },
                new[] { "SmartStore.Web.Controllers" });

			// product email a friend (TODO: Controller)
            routes.MapLocalizedRoute("ProductEmailAFriend",
                "productemailafriend/{productId}",
                new { controller = "Catalog", action = "ProductEmailAFriend" },
                new { productId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

			// ask product question form (TODO: Controller)
            routes.MapLocalizedRoute("ProductAskQuestion",
                "productaskquestion/{productId}",
                new { controller = "Catalog", action = "ProductAskQuestion" },
                new { productId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapRoute("SetProductReviewHelpfulness",
                "setproductreviewhelpfulness",
                new { controller = "Catalog", action = "SetProductReviewHelpfulness" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BackInStockSubscribePopup",
				"backinstocksubscribe/{productId}",
                new { controller = "Catalog", action = "BackInStockSubscribePopup" },
				new { productId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("DeleteBackInStockSubscription",
                "backinstocksubscribe/delete/{subscriptionId}",
                new { controller = "Customer", action = "DeleteBackInStockSubscription" },
                new { subscriptionId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("RegisterResult",
                "registerresult/{resultId}",
                new { controller = "Customer", action = "RegisterResult" },
                new { resultId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("EmailWishlist",
                "emailwishlist",
                new { controller = "ShoppingCart", action = "EmailWishlist" },
                new[] { "SmartStore.Web.Controllers" });

			// add product to cart (without any attributes and options). used on catalog pages.
			routes.MapLocalizedRoute("AddProductToCart-Catalog",
				"addproducttocart/catalog/{productId}",
				new { controller = "ShoppingCart", action = "AddProductToCart_Catalog" },
                new { productId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            // add product to cart (with attributes and options). used on the product details pages.
			routes.MapLocalizedRoute("AddProductToCart-Details",
				"addproducttocart/details/{productId}/{shoppingCartTypeId}",
				new { controller = "ShoppingCart", action = "AddProductToCart_Details" },
				new { productId = @"\d+", shoppingCartTypeId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            // product attributes with "upload file" type
            routes.MapLocalizedRoute("UploadFileProductAttribute",
				"uploadfileproductattribute/{productId}/{productAttributeId}",
                new { controller = "ShoppingCart", action = "UploadFileProductAttribute" },
				new { productId = @"\d+", productAttributeId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });


			/* Product tags
			----------------------------------------*/

			routes.MapLocalizedRoute("ProductsByTag",
				"producttag/{productTagId}/{SeName}",
				new { controller = "Catalog", action = "ProductsByTag", SeName = UrlParameter.Optional },
				new { productTagId = @"\d+" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ProductTagsAll",
				"producttag/all/",
				new { controller = "Catalog", action = "ProductTagsAll" },
				new[] { "SmartStore.Web.Controllers" });


			/* Checkout
			----------------------------------------*/

            routes.MapLocalizedRoute("Checkout",
                "checkout/",
                new { controller = "Checkout", action = "Index" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("CheckoutOnePage",
                "onepagecheckout/",
                new { controller = "Checkout", action = "OnePageCheckout" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("ReturnRequest",
				"returnrequest/{orderId}",
				new { controller = "ReturnRequest", action = "ReturnRequest" },
				new { orderId = @"\d+" },
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
                new { controller = "Profile", action = "Index" },
                new { id = @"\d+"},
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
                new { languageId = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });


			/* Boards
			----------------------------------------*/

            routes.MapLocalizedRoute("Boards",
                "boards",
                new { controller = "Boards", action = "Index" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("TopicSlug",
                "boards/topic/{id}/{slug}",
                new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional },
                new { id = @"\d+"},
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("ForumSlug",
                "boards/forum/{id}/{slug}",
                new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional },
                new { id = @"\d+" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("ForumGroupSlug",
				"boards/forumgroup/{id}/{slug}",
				new { controller = "Boards", action = "ForumGroup", slug = UrlParameter.Optional },
				new { id = @"\d+" },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BoardSearch",
                "boards/search",
                new { controller = "Boards", action = "Search" },
                new[] { "SmartStore.Web.Controllers" });


			/* Misc
			----------------------------------------*/

            routes.MapLocalizedRoute("PrivateMessages",
                "privatemessages/{tab}",
                new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("NewsArchive",
                "news",
                new { controller = "News", action = "List" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("Sitemap",
                "sitemap",
                new { controller = "Home", action = "Sitemap" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("SitemapSEO",
                "sitemap.xml",
				new { controller = "Home", action = "SitemapSeo" },
                new[] { "SmartStore.Web.Controllers" });

			//store closed
			routes.MapLocalizedRoute("StoreClosed",
				"storeclosed",
				new { controller = "Home", action = "StoreClosed" },
				new[] { "SmartStore.Web.Controllers" });

            routes.MapRoute("robots.txt",
                "robots.txt",
                new { controller = "Common", action = "RobotsTextFile" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Config",
				"config",
				new { controller = "Common", action = "Config" },
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
