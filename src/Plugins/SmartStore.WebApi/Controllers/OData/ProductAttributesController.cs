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
    public class ProductAttributesController : WebApiEntityController<ProductAttribute, IProductAttributeService>
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
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Create)]
        public IHttpActionResult Post(ProductAttribute entity)
        {
            var result = Insert(entity, () => Service.InsertProductAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Update)]
        public async Task<IHttpActionResult> Put(int key, ProductAttribute entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateProductAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<ProductAttribute> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductAttribute(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteProductAttribute(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IHttpActionResult GetProductAttributeOptionsSets(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ProductAttributeOptionsSets));
        }

        #endregion
    }
}
