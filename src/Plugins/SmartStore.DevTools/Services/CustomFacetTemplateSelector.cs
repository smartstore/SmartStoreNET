using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Customers;
using SmartStore.Services.Search.Rendering;

namespace SmartStore.DevTools.Services
{
    public class CustomFacetTemplateSelector : IFacetTemplateSelector
    {
        private readonly IWorkContext _workContext;

        public CustomFacetTemplateSelector(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        // Order in case of multiple implementations (like MegaSearchPlus).
        public int Ordinal => 99;

        public RouteInfo GetTemplateRoute(FacetGroup facetGroup)
        {
            // Provide template for catalog only.
            if (facetGroup.Scope != "Catalog")
            {
                return null;
            }

            // Provide template for specifications attributes only.
            if (facetGroup.Kind != FacetGroupKind.Attribute)
            {
                return null;
            }

            // Provide template for admin only (because this is a developer demo).
            if (!_workContext.CurrentCustomer.IsAdmin())
            {
                return null;
            }

            // TODO: filter by what your template is made for.
            // E.g. merchant configures a specification attribute in your plugin, then you can filter by facetGroup.Key == "attrid<ConfiguredId>"

            if (facetGroup.Label.IsCaseInsensitiveEqual("Dimensions") || 
                facetGroup.Label.IsCaseInsensitiveEqual("Maße"))
            {
                var routeValues = new RouteValueDictionary(new
                {
                    area = "SmartStore.DevTools",
                    templateName = "MyCustomFacetTemplate",
                    facetGroup
                });

                return new RouteInfo("MyCustomFacetTemplate", "DevTools", routeValues);
            }

            return null;
        }
    }
}