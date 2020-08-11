using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class AddressesController : WebApiEntityController<Address, IAddressService>
	{
		private readonly Lazy<IRepository<Order>> _orderRepository;
		private readonly Lazy<IEventPublisher> _eventPublisher;

		public AddressesController(
			Lazy<IRepository<Order>> orderRepository,
			Lazy<IEventPublisher> eventPublisher)
		{
			_orderRepository = orderRepository;
			_eventPublisher = eventPublisher;
		}

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Customer.Read)]
		public IQueryable<Address> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Address> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Read)]
		public HttpResponseMessage GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Create)]
		public IHttpActionResult Post(Address entity)
		{
			var result = Insert(entity, () =>
			{
				Service.InsertAddress(entity);
				PublishOrderUpdated(entity.Id);
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public async Task<IHttpActionResult> Put(int key, Address entity)
		{
			var result = await UpdateAsync(entity, key, () =>
			{
				Service.UpdateAddress(entity);
				PublishOrderUpdated(entity.Id);
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Address> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity =>
			{
				Service.UpdateAddress(entity);
				PublishOrderUpdated(entity.Id);
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity =>
			{
				var entityId = entity?.Id ?? 0;

				Service.DeleteAddress(entity);
				PublishOrderUpdated(entityId);
			});

			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Country> GetCountry(int key)
		{
			return GetRelatedEntity(key, x => x.Country);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<StateProvince> GetStateProvince(int key)
		{
			return GetRelatedEntity(key, x => x.StateProvince);
		}

		#endregion

		private void PublishOrderUpdated(int addressId)
		{
			if (addressId == 0)
			{
				return;
			}

			this.ProcessEntity(() =>
			{
				var orders = _orderRepository.Value.TableUntracked
					.Where(x => x.BillingAddressId == addressId || x.ShippingAddressId == addressId)
					.ToList();

				foreach (var order in orders)
				{
					_eventPublisher.Value.PublishOrderUpdated(order);
				}
			});
		}
	}
}
