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
    public class ProductAttributeOptionsController : WebApiEntityController<ProductAttributeOption, IProductAttributeService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        public IHttpActionResult Post(ProductAttributeOption entity)
        {
            var result = Insert(entity, () => Service.InsertProductAttributeOption(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        public async Task<IHttpActionResult> Put(int key, ProductAttributeOption entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateProductAttributeOption(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        public async Task<IHttpActionResult> Patch(int key, Delta<ProductAttributeOption> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductAttributeOption(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteProductAttributeOption(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IHttpActionResult GetProductAttributeOptionsSet(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.ProductAttributeOptionsSet));
        }

        #endregion
    }
}
