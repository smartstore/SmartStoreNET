using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string FacetToggle(this UrlHelper urlHelper, Facet facet)
        {
            var facetUrlHelper = EngineContext.Current.Resolve<FacetUrlHelper>();
            return facetUrlHelper.Toggle(facet);
        }

        public static string FacetAdd(this UrlHelper urlHelper, params Facet[] facets)
        {
            var facetUrlHelper = EngineContext.Current.Resolve<FacetUrlHelper>();
            return facetUrlHelper.Add(facets);
        }

        public static string FacetRemove(this UrlHelper urlHelper, params Facet[] facets)
        {
            var facetUrlHelper = EngineContext.Current.Resolve<FacetUrlHelper>();
            return facetUrlHelper.Remove(facets);
        }

        public static string GetFacetQueryName(this UrlHelper urlHelper, Facet facet)
        {
            var facetUrlHelper = EngineContext.Current.Resolve<FacetUrlHelper>();
            return facetUrlHelper.GetQueryName(facet);
        }
    }
}
