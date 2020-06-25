using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductBundleItemsController : WebApiEntityController<ProductBundleItem, IProductService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditBundle)]
		protected override void Insert(ProductBundleItem entity)
		{
			Service.InsertBundleItem(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditBundle)]
        protected override void Update(ProductBundleItem entity)
		{
			Service.UpdateBundleItem(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditBundle)]
        protected override void Delete(ProductBundleItem entity)
		{
			Service.DeleteBundleItem(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductBundleItem> GetProductBundleItem(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
		{
			return GetRelatedEntity(key, x => x.Product);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetBundleProduct(int key)
		{
			return GetRelatedEntity(key, x => x.BundleProduct);
		}
	}
}
