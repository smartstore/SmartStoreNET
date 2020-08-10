using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class RelatedProductsController : WebApiEntityController<RelatedProduct, IProductService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
		public IQueryable<RelatedProduct> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<RelatedProduct> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
		public IHttpActionResult Post(RelatedProduct entity)
		{
			var result = Insert(entity, () => Service.InsertRelatedProduct(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
		public async Task<IHttpActionResult> Put(int key, RelatedProduct entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateRelatedProduct(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
		public async Task<IHttpActionResult> Patch(int key, Delta<RelatedProduct> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateRelatedProduct(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteRelatedProduct(entity));
			return result;
		}
	}
}
