using System;
using System.Linq;
using System.Web.Http;
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

		private void PublishOrderUpdated(int addressId)
		{
			this.ProcessEntity(() =>
			{
				if (addressId != 0)
				{
					var orders = _orderRepository.Value.TableUntracked
						.Where(x => x.BillingAddressId == addressId || x.ShippingAddressId == addressId)
						.ToList();

					foreach (var order in orders)
					{
						_eventPublisher.Value.PublishOrderUpdated(order);
					}
				}
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Customer.Create)]
		protected override void Insert(Address entity)
		{
			Service.InsertAddress(entity);
			PublishOrderUpdated(entity.Id);
		}

        [WebApiAuthenticate(Permission = Permissions.Customer.Update)]
        protected override void Update(Address entity)
		{
			Service.UpdateAddress(entity);
			PublishOrderUpdated(entity.Id);
		}

        [WebApiAuthenticate(Permission = Permissions.Customer.Delete)]
        protected override void Delete(Address entity)
		{
			int entityId = (entity == null ? 0 : entity.Id);

			Service.DeleteAddress(entity);
			PublishOrderUpdated(entityId);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Address> GetAddress(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

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
	}
}
