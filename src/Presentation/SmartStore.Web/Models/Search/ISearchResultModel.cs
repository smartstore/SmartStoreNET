using System.Web.Mvc;
using Autofac.Integration.Mvc;
using SmartStore.Services.Search;

namespace SmartStore.Web.Models.Search
{
    public interface ISearchResultModel
    {
        CatalogSearchResult SearchResult { get; }
    }

    public interface IForumSearchResultModel
    {
        ForumSearchResult SearchResult { get; }
    }

    [ModelBinderType(typeof(ISearchResultModel), typeof(IForumSearchResultModel))]
    public class SearchResultModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;

            if (typeof(ISearchResultModel).IsAssignableFrom(modelType))
            {
                var model = controllerContext.GetRootControllerContext().Controller.ViewData.Model as ISearchResultModel;
                return model;
            }
            else if (typeof(IForumSearchResultModel).IsAssignableFrom(modelType))
            {
                var model = controllerContext.GetRootControllerContext().Controller.ViewData.Model as IForumSearchResultModel;
                return model;
            }

            return null;
        }
    }
}