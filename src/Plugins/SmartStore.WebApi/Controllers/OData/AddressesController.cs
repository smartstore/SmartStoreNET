using System;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services.Common;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCustomers")]
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
				return null;
			});
		}

		protected override void Insert(Address entity)
		{
			Service.InsertAddress(entity);
			PublishOrderUpdated(entity.Id);
		}
		protected override void Update(Address entity)
		{
			Service.UpdateAddress(entity);
			PublishOrderUpdated(entity.Id);
		}
		protected override void Delete(Address entity)
		{
			int entityId = (entity == null ? 0 : entity.Id);

			Service.DeleteAddress(entity);
			PublishOrderUpdated(entityId);
		}

		[WebApiQueryable]
		public SingleResult<Address> GetAddress(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public Country GetCountry(int key)
		{
			return GetExpandedProperty<Country>(key, x => x.Country);
		}

		public StateProvince GetStateProvince(int key)
		{
			return GetExpandedProperty<StateProvince>(key, x => x.StateProvince);
		}
	}
}
