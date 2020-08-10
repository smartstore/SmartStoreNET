using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductVariantAttributeCombinationsController : WebApiEntityController<ProductVariantAttributeCombination, IProductAttributeService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
		public IQueryable<ProductVariantAttributeCombination> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttributeCombination> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public IHttpActionResult Post(ProductVariantAttributeCombination entity)
		{
			var result = Insert(entity, () => Service.InsertProductVariantAttributeCombination(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Put(int key, ProductVariantAttributeCombination entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateProductVariantAttributeCombination(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Patch(int key, Delta<ProductVariantAttributeCombination> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductVariantAttributeCombination(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteProductVariantAttributeCombination(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetRelatedEntity(key, x => x.DeliveryTime);
		}

		#endregion
	}
}
