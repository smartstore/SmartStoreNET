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
    public class ProductSpecificationAttributesController : WebApiEntityController<ProductSpecificationAttribute, ISpecificationAttributeService>
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
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        public IHttpActionResult Post(ProductSpecificationAttribute entity)
        {
            var result = Insert(entity, () => Service.InsertProductSpecificationAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        public async Task<IHttpActionResult> Put(int key, ProductSpecificationAttribute entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateProductSpecificationAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        public async Task<IHttpActionResult> Patch(int key, Delta<ProductSpecificationAttribute> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductSpecificationAttribute(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteProductSpecificationAttribute(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetSpecificationAttributeOption(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.SpecificationAttributeOption));
        }

        #endregion
    }
}
