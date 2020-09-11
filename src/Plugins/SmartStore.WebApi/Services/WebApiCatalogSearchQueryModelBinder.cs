using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Web.Framework.WebApi.Caching;

namespace SmartStore.WebApi.Services
{
    public class WebApiCatalogSearchQueryModelBinder : IModelBinder
    {
        private CatalogSearchQuery NormalizeQuery(CatalogSearchQuery query)
        {
            var controllingData = WebApiCachingControllingData.Data();

            query = query
                .BuildFacetMap(false)
                .Slice(query.Skip, Math.Min(query.Take, controllingData.MaxTop));

            return query;
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;
            if (modelType != typeof(CatalogSearchQuery))
            {
                bindingContext.Model = NormalizeQuery(new CatalogSearchQuery());
                return true;
            }

            var dependencyScope = actionContext.Request.GetDependencyScope();
            var factory = (ICatalogSearchQueryFactory)dependencyScope.GetService(typeof(ICatalogSearchQueryFactory));

            if (factory.Current != null)
            {
                bindingContext.Model = NormalizeQuery(factory.Current);
                return true;
            }

            bindingContext.Model = NormalizeQuery(factory.CreateFromQuery());
            return true;
        }
    }
}