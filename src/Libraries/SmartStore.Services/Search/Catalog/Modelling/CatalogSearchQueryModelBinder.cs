using System.Web.Mvc;
using Autofac.Integration.Mvc;

namespace SmartStore.Services.Search.Modelling
{
    [ModelBinderType(typeof(CatalogSearchQuery))]
    public class CatalogSearchQueryModelBinder : IModelBinder
    {
        private readonly ICatalogSearchQueryFactory _factory;

        public CatalogSearchQueryModelBinder(ICatalogSearchQueryFactory factory)
        {
            _factory = factory;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (_factory.Current != null)
            {
                // Don't bind again for current request.
                return _factory.Current;
            }

            if (controllerContext.IsChildAction)
            {
                // Never attempt to bind in child actions. We require the binding to happen
                // in a parent action. If the child action is part of a request with an already bound
                // 'CatalogSearchQuery', good for you :-) You'll get an instance, but null otherwise.
                return _factory.Current;
            }

            var modelType = bindingContext.ModelType;
            if (modelType != typeof(CatalogSearchQuery))
            {
                return new CatalogSearchQuery();
            }

            var query = _factory.CreateFromQuery();
            return query;
        }
    }
}
