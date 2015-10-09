using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductBundleItemsController : WebApiEntityController<ProductBundleItem, IProductService>
	{
		protected override void Insert(ProductBundleItem entity)
		{
			Service.InsertBundleItem(entity);
		}
		protected override void Update(ProductBundleItem entity)
		{
			Service.UpdateBundleItem(entity);
		}
		protected override void Delete(ProductBundleItem entity)
		{
			Service.DeleteBundleItem(entity);
		}

		[WebApiQueryable]
		public SingleResult<ProductBundleItem> GetProductBundleItem(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Product> GetProduct(int key)
		{
			return GetRelatedEntity(key, x => x.Product);
		}

		[WebApiQueryable]
		public SingleResult<Product> GetBundleProduct(int key)
		{
			return GetRelatedEntity(key, x => x.BundleProduct);
		}
	}
}
