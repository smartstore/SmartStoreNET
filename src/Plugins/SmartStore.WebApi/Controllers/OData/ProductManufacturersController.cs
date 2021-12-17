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
    public class ProductManufacturersController : WebApiEntityController<ProductManufacturer, IManufacturerService>
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
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public IHttpActionResult Post(ProductManufacturer entity)
        {
            var result = Insert(entity, () => Service.InsertProductManufacturer(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public async Task<IHttpActionResult> Put(int key, ProductManufacturer entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateProductManufacturer(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public async Task<IHttpActionResult> Patch(int key, Delta<ProductManufacturer> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductManufacturer(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteProductManufacturer(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult GetManufacturer(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.Manufacturer));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetProduct(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.Product));
        }

        #endregion
    }
}
