using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
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

        #region Actions and functions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.EntityType<DeliveryTime>();

            entityConfig.Collection
                .Function("GetDeliveryDate")
                .Returns<SimpleRange<DateTime?>>()
                .Parameter<int>("Id");
        }

        /// GET /DeliveryTimes/GetDeliveryDate(Id=123)
        [HttpGet]
        [WebApiAuthenticate]
        public IHttpActionResult GetDeliveryDate(int id)
        {
            var deliveryTime = Service.GetDeliveryTimeById(id);
            if (deliveryTime == null)
            {
                return NotFound();
            }

            var (min, max) = Service.GetDeliveryDate(deliveryTime);

            var result = new SimpleRange<DateTime?>
            {
                Minimum = min,
                Maximum = max
            };

            return Ok(result);
        }

        #endregion
    }
}
