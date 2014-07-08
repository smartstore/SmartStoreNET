using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Infrastructure
{
    public partial class SeoRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //generic URLs
            routes.MapGenericPathRoute("GenericUrl",
                "{generic_se_name}",
                new { controller = "Common", action = "GenericUrl" },
                new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("Product",
                "{SeName}",
                new { controller = "Catalog", action = "Product" },
				new { /*SeName = @"\w+", productId = @"\d+"*/ },
                new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("Category",
                "{SeName}",
                new { controller = "Catalog", action = "Category" },
				new { /*SeName = @"\w+", categoryId = @"\d+"*/ },
                new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("Manufacturer",
                "{SeName}",
                new { controller = "Catalog", action = "Manufacturer" },
				new { /*SeName = @"\w+", manufacturerId = @"\d+"*/ },
                new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("NewsItem",
	            "{SeName}",
	            new { controller = "News", action = "NewsItem" },
				new { /*SeName = @"\w+", newsItemId = @"\d+"*/ },
	            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("BlogPost",
                "{SeName}",
                new { controller = "Blog", action = "BlogPost" },
				new { /*SeName = @"\w+", blogPostId = @"\d+"*/ },
                new[] { "SmartStore.Web.Controllers" });

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
