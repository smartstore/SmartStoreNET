using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class ShippingMethodsController : WebApiEntityController<ShippingMethod, IShippingService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Create)]
        public IHttpActionResult Post(ShippingMethod entity)
        {
            var result = Insert(entity, () => Service.InsertShippingMethod(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Update)]
        public async Task<IHttpActionResult> Put(int key, ShippingMethod entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateShippingMethod(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<ShippingMethod> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateShippingMethod(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteShippingMethod(entity));
            return result;
        }
    }
}
