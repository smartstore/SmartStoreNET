using System;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Rendering
{
    public interface IFacetTemplateProvider
    {
        RouteInfo GetTemplateRoute(FacetGroup facetGroup);
    }
}
