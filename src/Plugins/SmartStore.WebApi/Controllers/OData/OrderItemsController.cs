using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;

namespace SmartStore.WebApi.Controllers.OData
{
    public class OrderItemsController : WebApiEntityController<OrderItem, IOrderService>
	{
		protected override IQueryable<OrderItem> GetEntitySet()
		{
			var query =
				from x in Repository.Table
				where !x.Order.Deleted
				select x;

			return query;
		}

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Order.Read)]
		public IQueryable<OrderItem> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<OrderItem> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
		public IHttpActionResult Post(OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
		public IHttpActionResult Put(int key, OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
		public IHttpActionResult Patch(int key, Delta<OrderItem> model)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteOrderItem(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<Order> GetOrder(int key)
		{
			return GetRelatedEntity(key, x => x.Order);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
		{
			return GetRelatedEntity(key, x => x.Product);
		}

        #endregion

        #region Actions

        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public OrderItemInfo Infos(int key)
		{
			var result = new OrderItemInfo();
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				result.ItemsCanBeAddedToShipmentCount = entity.GetItemsCanBeAddedToShipmentCount();
				result.ShipmentItemsCount = entity.GetShipmentItemsCount();
				result.DispatchedItemsCount = entity.GetDispatchedItemsCount();
				result.NotDispatchedItemsCount = entity.GetNotDispatchedItemsCount();
				result.DeliveredItemsCount = entity.GetDeliveredItemsCount();
				result.NotDeliveredItemsCount = entity.GetNotDeliveredItemsCount();
			});

			return result;
		}

		#endregion
	}
}
