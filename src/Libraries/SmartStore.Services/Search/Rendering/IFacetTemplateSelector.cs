using System;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Rendering
{
    public interface IFacetTemplateSelector : IOrdered
    {
        RouteInfo GetTemplateRoute(FacetGroup facetGroup);
    }
}
