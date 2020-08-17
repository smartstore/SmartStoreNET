using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class OrderNotesController : WebApiEntityController<OrderNote, IOrderService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Order.Read)]
		public IQueryable<OrderNote> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<OrderNote> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Read)]
		public IHttpActionResult GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Update)]
		public IHttpActionResult Post(OrderNote entity)
		{
			var result = Insert(entity, () =>
			{
				var order = Service.GetOrderById(entity.OrderId);
				if (order == null || order.Deleted)
				{
					throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(entity.OrderId));
				}
				if (entity.Note.IsEmpty())
				{
					throw Request.BadRequestException("Missing or empty order note text.");
				}

				entity.CreatedOnUtc = DateTime.UtcNow;

				order.OrderNotes.Add(entity);
				Service.UpdateOrder(order);

				if (entity.DisplayToCustomer && this.GetQueryStringValue("customernotification", true))
				{
					Services.MessageFactory.SendNewOrderNoteAddedCustomerNotification(entity, order.CustomerLanguageId);
				}
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Update)]
		public IHttpActionResult Put()
		{
			return StatusCode(HttpStatusCode.NotImplemented);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Update)]
		public IHttpActionResult Patch()
		{
			return StatusCode(HttpStatusCode.NotImplemented);
		}

		[WebApiAuthenticate(Permission = Permissions.Order.Update)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteOrderNote(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<Order> GetOrder(int key)
		{
			return GetRelatedEntity(key, x => x.Order);
		}

        #endregion
    }
}
