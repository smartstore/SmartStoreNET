using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Infrastructure
{
    public partial class SeoRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //generic URLs
            routes.MapGenericPathRoute("GenericUrl",
                "{*generic_se_name}",
                new { controller = "Common", action = "GenericUrl" },
                new[] { "SmartStore.Web.Controllers" });

			// Routes solely needed for URL creation, NOT for route matching.
            routes.MapLocalizedRoute("Product",
                "{SeName}",
                new { controller = "Product", action = "Product" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("Category",
                "{SeName}",
                new { controller = "Catalog", action = "Category" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("Manufacturer",
                "{SeName}",
                new { controller = "Catalog", action = "Manufacturer" },
                new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("Topic",
				"{SeName}",
				new { controller = "Topic", action = "TopicDetails" },
				new[] { "SmartStore.Web.Controllers" });

			routes.MapLocalizedRoute("NewsItem",
	            "{SeName}",
	            new { controller = "News", action = "NewsItem" },
	            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("BlogPost",
                "{SeName}",
                new { controller = "Blog", action = "BlogPost" },
                new[] { "SmartStore.Web.Controllers" });

			// TODO: actually this one's never reached, because the "GenericUrl" route
			// at the top handles this.
			routes.MapLocalizedRoute("PageNotFound",
				"{*path}",
				new { controller = "Error", action = "NotFound" },
				new[] { "SmartStore.Web.Controllers" });
        }

        public int Priority
        {
            get
            {
                return int.MinValue;
            }
        }
    }
}
