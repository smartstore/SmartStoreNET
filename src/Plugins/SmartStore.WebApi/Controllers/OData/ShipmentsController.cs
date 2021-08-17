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
    public class ShipmentsController : WebApiEntityController<Shipment, IShipmentService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        public IHttpActionResult Post(Shipment entity)
        {
            var result = Insert(entity, () => Service.InsertShipment(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        public async Task<IHttpActionResult> Put(int key, Shipment entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateShipment(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Shipment> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateShipment(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteShipment(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetShipmentItems(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ShipmentItems));
        }

        #endregion
    }
}
