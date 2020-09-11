using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class DeliveryTimesController : WebApiEntityController<DeliveryTime, IDeliveryTimeService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Create)]
        public IHttpActionResult Post(DeliveryTime entity)
        {
            var result = Insert(entity, () => Service.InsertDeliveryTime(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Update)]
        public async Task<IHttpActionResult> Put(int key, DeliveryTime entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateDeliveryTime(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<DeliveryTime> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateDeliveryTime(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteDeliveryTime(entity));
            return result;
        }
    }
}
