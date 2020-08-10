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
    public class ProductVariantAttributeValuesController : WebApiEntityController<ProductVariantAttributeValue, IProductAttributeService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
		public IQueryable<ProductVariantAttributeValue> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttributeValue> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public IHttpActionResult Post(ProductVariantAttributeValue entity)
		{
			var result = Insert(entity, () => Service.InsertProductVariantAttributeValue(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Put(int key, ProductVariantAttributeValue entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateProductVariantAttributeValue(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Patch(int key, Delta<ProductVariantAttributeValue> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductVariantAttributeValue(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteProductVariantAttributeValue(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttribute> GetProductVariantAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.ProductVariantAttribute);
		}

		#endregion
	}
}
