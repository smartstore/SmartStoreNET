using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class ProductVariantAttributeCombinationsController : WebApiEntityController<ProductVariantAttributeCombination, IProductAttributeService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public IHttpActionResult Post(ProductVariantAttributeCombination entity)
        {
            var result = Insert(entity, () => Service.InsertProductVariantAttributeCombination(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        public async Task<IHttpActionResult> Put(int key, ProductVariantAttributeCombination entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateProductVariantAttributeCombination(entity));
            return result;
        }

        [WebApiQueryable]
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
        public IHttpActionResult GetDeliveryTime(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.DeliveryTime));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public IHttpActionResult GetQuantityUnit(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.QuantityUnit));
        }

        #endregion
    }
}
