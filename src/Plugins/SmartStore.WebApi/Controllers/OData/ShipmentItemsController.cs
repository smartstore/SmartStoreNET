using System.Web.Http;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ShipmentItemsController : WebApiEntityController<ShipmentItem, IShipmentService>
	{
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
		protected override void Insert(ShipmentItem entity)
		{
			Service.InsertShipmentItem(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        protected override void Update(ShipmentItem entity)
		{
			Service.UpdateShipmentItem(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        protected override void Delete(ShipmentItem entity)
		{
			Service.DeleteShipmentItem(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<ShipmentItem> GetShipmentItem(int key)
		{
			return GetSingleResult(key);
		}
	}
}
