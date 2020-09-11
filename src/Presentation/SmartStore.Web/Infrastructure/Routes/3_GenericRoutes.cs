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
            // Main generic path route
            routes.MapGenericPathRoute("GenericUrl",
                "{*generic_se_name}",
                new { controller = "Common", action = "GenericUrl" },
                new[] { "SmartStore.Web.Controllers" });

            GenericPathRoute.RegisterPaths(
                new GenericPath
                {
                    EntityName = "Product",
                    IdParamName = "productid",
                    Order = int.MinValue,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("Product"), new { controller = "Product", action = "ProductDetails" }, new[] { "SmartStore.Web.Controllers" })
                },
                new GenericPath
                {
                    EntityName = "Category",
                    IdParamName = "categoryid",
                    Order = int.MinValue + 1,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("Category"), new { controller = "Catalog", action = "Category" }, new[] { "SmartStore.Web.Controllers" })
                },
                new GenericPath
                {
                    EntityName = "Manufacturer",
                    IdParamName = "manufacturerid",
                    Order = int.MinValue + 2,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("Manufacturer"), new { controller = "Catalog", action = "Manufacturer" }, new[] { "SmartStore.Web.Controllers" })
                },
                new GenericPath
                {
                    EntityName = "Topic",
                    IdParamName = "topicId",
                    Order = int.MinValue + 3,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("Topic"), new { controller = "Topic", action = "TopicDetails" }, new[] { "SmartStore.Web.Controllers" })
                },
                new GenericPath
                {
                    EntityName = "NewsItem",
                    IdParamName = "newsItemId",
                    Order = int.MinValue + 4,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("NewsItem"), new { controller = "News", action = "NewsItem" }, new[] { "SmartStore.Web.Controllers" })
                },
                new GenericPath
                {
                    EntityName = "BlogPost",
                    IdParamName = "blogPostId",
                    Order = int.MinValue + 5,
                    Route = routes.CreateLocalizedRoute(UrlTemplateFor("BlogPost"), new { controller = "Blog", action = "BlogPost" }, new[] { "SmartStore.Web.Controllers" })
                }
            );

            string UrlTemplateFor(string entityName)
            {
                var url = "{SeName}";
                var prefix = GenericPathRoute.GetUrlPrefixFor(entityName);
                if (prefix.HasValue())
                {
                    return prefix + "/" + "{SeName}";
                }

                return url;
            }
        }

        public int Priority { get; } = int.MinValue + 100;
    }
}
