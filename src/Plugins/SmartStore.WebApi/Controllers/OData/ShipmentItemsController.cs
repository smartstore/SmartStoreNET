using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Order.Read)]
		public IQueryable<ShipmentItem> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<ShipmentItem> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Read)]
		public HttpResponseMessage GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
		public IHttpActionResult Post(ShipmentItem entity)
		{
			var result = Insert(entity, () => Service.InsertShipmentItem(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
		public async Task<IHttpActionResult> Put(int key, ShipmentItem entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateShipmentItem(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
		public async Task<IHttpActionResult> Patch(int key, Delta<ShipmentItem> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateShipmentItem(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteShipmentItem(entity));
			return result;
		}
	}
}
