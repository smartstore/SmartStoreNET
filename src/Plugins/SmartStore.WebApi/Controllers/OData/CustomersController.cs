using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCustomers")]
	public class CustomersController : WebApiEntityController<Customer, ICustomerService>
	{
		private readonly Lazy<IAddressService> _addressService;

		public CustomersController(
			Lazy<IAddressService> addressService)
		{
			_addressService = addressService;
		}

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

		#region Navigation properties

		/// <summary>
		/// Handle address assignments
		/// </summary>
		/// <param name="key">Customer id</param>
		/// <param name="relatedKey">Address id</param>
		/// <returns>Address</returns>
		public HttpResponseMessage NavigationAddresses(int key, int relatedKey)
		{
			var entity = GetExpandedEntity(key, x => x.Addresses);
			var address = _addressService.Value.GetAddressById(relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (address != null && entity.Addresses.FindAddress(address) == null)
				{
					entity.Addresses.Add(address);

					Service.UpdateCustomer(entity);

					return Request.CreateResponse(HttpStatusCode.Created, address);
				}
			}
			else if (Request.Method == HttpMethod.Delete)
			{
				if (address != null)
				{
					entity.RemoveAddress(address);

					Service.UpdateCustomer(entity);
				}

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			return Request.CreateResponseForEntity(address, relatedKey);
		}

		/// <summary>
		/// Handle customer role assignments
		/// </summary>
		/// <param name="key">Customer id</param>
		/// <param name="relatedKey">Customer role id</param>
		/// <returns>Customer role</returns>
		public HttpResponseMessage NavigationCustomerRoles(int key, int relatedKey)
		{
			var entity = GetExpandedEntity(key, x => x.CustomerRoles);
			var customerRole = Service.GetCustomerRoleById(relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (customerRole != null && !entity.CustomerRoles.Any(x => x.Id == customerRole.Id))
				{
					entity.CustomerRoles.Add(customerRole);

					Service.UpdateCustomer(entity);

					return Request.CreateResponse(HttpStatusCode.Created, customerRole);
				}
			}
			else if (Request.Method == HttpMethod.Delete)
			{
				if (customerRole != null && entity.CustomerRoles.Any(x => x.Id == customerRole.Id))
				{
					entity.CustomerRoles.Remove(customerRole);

					Service.UpdateCustomer(entity);
				}

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			return Request.CreateResponseForEntity(customerRole, relatedKey);
		}


		[WebApiQueryable]
		public SingleResult<Address> GetBillingAddress(int key)
		{
			return GetRelatedEntity(key, x => x.BillingAddress);
		}

		[WebApiQueryable]
		public SingleResult<Address> GetShippingAddress(int key)
		{
			return GetRelatedEntity(key, x => x.ShippingAddress);
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
			return GetRelatedCollection(key, x => x.Orders);
		}

		[WebApiQueryable]
		public IQueryable<ReturnRequest> GetReturnRequests(int key)
		{
			return GetRelatedCollection(key, x => x.ReturnRequests);
		}

		[WebApiQueryable]
		public IQueryable<Address> GetAddresses(int key)
		{
			return GetRelatedCollection(key, x => x.Addresses);
		}

		[WebApiQueryable]
		public IQueryable<CustomerRole> GetCustomerRoles(int key)
		{
			return GetRelatedCollection(key, x => x.CustomerRoles);
		}

		#endregion
	}
}
