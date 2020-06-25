using System.Web.Http;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ShipmentsController : WebApiEntityController<Shipment, IShipmentService>
	{
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        protected override void Insert(Shipment entity)
		{
			Service.InsertShipment(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        protected override void Update(Shipment entity)
		{
			Service.UpdateShipment(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        protected override void Delete(Shipment entity)
		{
			Service.DeleteShipment(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<Shipment> GetShipment(int key)
		{
			return GetSingleResult(key);
		}
	}
}
