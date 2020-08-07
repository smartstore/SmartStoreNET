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
    public class ProductAttributeOptionsSetsController : WebApiEntityController<ProductAttributeOptionsSet, IProductAttributeService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
		public IQueryable<ProductAttributeOptionsSet> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttributeOptionsSet> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		public IHttpActionResult Post(ProductAttributeOptionsSet entity)
		{
			var result = Insert(entity, () => Service.InsertProductAttributeOptionsSet(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		public async Task<IHttpActionResult> Put(int key, ProductAttributeOptionsSet entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateProductAttributeOptionsSet(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		public async Task<IHttpActionResult> Patch(int key, Delta<ProductAttributeOptionsSet> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductAttributeOptionsSet(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteProductAttributeOptionsSet(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttribute> GetProductAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.ProductAttribute);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IQueryable<ProductAttributeOption> GetProductAttributeOptions(int key)
		{
			return GetRelatedCollection(key, x => x.ProductAttributeOptions);
		}

        #endregion
    }
}
