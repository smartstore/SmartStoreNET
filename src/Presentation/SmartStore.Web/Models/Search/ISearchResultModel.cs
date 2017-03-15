using System;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using SmartStore.Services.Search;

namespace SmartStore.Web.Models.Search
{
	public interface ISearchResultModel
	{
		CatalogSearchResult SearchResult { get; }
	}

	[ModelBinderType(typeof(ISearchResultModel))]
	public class SearchResultModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var modelType = bindingContext.ModelType;

			if (!typeof(ISearchResultModel).IsAssignableFrom(modelType))
			{
				return null;
			}

			var model = controllerContext.GetMasterControllerContext().Controller.ViewData.Model as ISearchResultModel;
			return model;
		}
	}
}