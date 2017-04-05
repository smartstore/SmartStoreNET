using System.Web.Mvc;
using Autofac.Integration.Mvc;

namespace SmartStore.Services.Catalog.Modelling
{
	[ModelBinderType(typeof(ProductVariantQuery))]
	public class ProductAttributeQueryModelBinder : IModelBinder
	{
		private readonly IProductVariantQueryFactory _factory;

		public ProductAttributeQueryModelBinder(IProductVariantQueryFactory factory)
		{
			_factory = factory;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			if (_factory.Current != null)
			{
				// Don't bind again for current request
				return _factory.Current;
			}

			if (controllerContext.IsChildAction)
			{
				// Never attempt to bind in child actions. We require the binding to happen in a parent action.
				return _factory.Current;
			}

			var modelType = bindingContext.ModelType;

			if (modelType != typeof(ProductVariantQuery))
			{
				return new ProductVariantQuery();
			}

			var query = _factory.CreateFromQuery();
			return query;
		}
	}
}
