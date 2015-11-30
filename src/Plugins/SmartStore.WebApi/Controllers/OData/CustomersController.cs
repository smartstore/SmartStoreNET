using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCustomers")]
	public class CustomersController : WebApiEntityController<Customer, ICustomerService>
	{
		protected override IQueryable<Customer> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}
		protected override void Insert(Customer entity)
		{
			Service.InsertCustomer(entity);
		}
		protected override void Update(Customer entity)
		{
			Service.UpdateCustomer(entity);
		}
		protected override void Delete(Customer entity)
		{
			Service.DeleteCustomer(entity);
		}

		[WebApiQueryable]
		public SingleResult<Customer> GetCustomer(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public Address GetBillingAddress(int key)
		{
			return GetExpandedProperty<Address>(key, x => x.BillingAddress);
		}

		public Address GetShippingAddress(int key)
		{
			return GetExpandedProperty<Address>(key, x => x.ShippingAddress);
		}

		//public Language GetLanguage(int key)
		//{
		//	return GetExpandedProperty<Language>(key, x => x.Language);
		//}

		//public Currency GetCurrency(int key)
		//{
		//	return GetExpandedProperty<Currency>(key, x => x.Currency);
		//}

		[WebApiQueryable]
		public IQueryable<Order> GetOrders(int key)
		{
			var entity = GetExpandedEntity<ICollection<Order>>(key, x => x.Orders);

			return entity.Orders.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<ReturnRequest> GetReturnRequests(int key)
		{
			var entity = GetExpandedEntity<ICollection<ReturnRequest>>(key, x => x.ReturnRequests);

			return entity.ReturnRequests.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<Address> GetAddresses(int key)
		{
			var entity = GetExpandedEntity<ICollection<Address>>(key, x => x.Addresses);

			return entity.Addresses.AsQueryable();
		}

		// actions

		//[HttpGet, Queryable]
		//public IQueryable<GenericAttribute> GetGenericAttributes(int key)
		//{
		//	var query = GenericAttributes(key, "Customer");

		//	return query;
		//}
	}
}
