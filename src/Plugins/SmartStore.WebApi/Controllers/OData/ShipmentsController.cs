using System.Web.Http;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageOrders")]
	public class ShipmentsController : WebApiEntityController<Shipment, IShipmentService>
	{
		protected override void Insert(Shipment entity)
		{
			Service.InsertShipment(entity);
		}
		protected override void Update(Shipment entity)
		{
			Service.UpdateShipment(entity);
		}
		protected override void Delete(Shipment entity)
		{
			Service.DeleteShipment(entity);
		}

		[WebApiQueryable]
		public SingleResult<Shipment> GetShipment(int key)
		{
			return GetSingleResult(key);
		}
	}
}
