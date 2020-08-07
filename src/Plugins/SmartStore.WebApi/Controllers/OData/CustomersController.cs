using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CustomersController : WebApiEntityController<Customer, ICustomerService>
	{
		private readonly Lazy<IAddressService> _addressService;

		public CustomersController(Lazy<IAddressService> addressService)
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

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Customer.Read)]
		public IQueryable<Customer> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Customer> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Create)]
		public IHttpActionResult Post(Customer entity)
		{
			var result = Insert(entity, () => Service.InsertCustomer(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public async Task<IHttpActionResult> Put(int key, Customer entity)
		{
			var result = await UpdateAsync(entity, key, () =>
			{
				if (entity != null && entity.IsSystemAccount)
				{
					throw this.ExceptionForbidden();
				}

				Service.UpdateCustomer(entity);
			});
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Customer> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity =>
			{
				if (entity != null && entity.IsSystemAccount)
				{
					throw this.ExceptionForbidden();
				}

				Service.UpdateCustomer(entity);
			});
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity =>
			{
				if (entity != null && entity.IsSystemAccount)
				{
					throw this.ExceptionForbidden();
				}

				Service.DeleteCustomer(entity);
			});

			return result;
		}

		#region Navigation properties

		/// <summary>
		/// Handle address assignments
		/// </summary>
		/// <param name="key">Customer id</param>
		/// <param name="relatedKey">Address id</param>
		/// <returns>Address</returns>
		[WebApiAuthenticate(Permission = Permissions.Customer.EditAddress)]
        public HttpResponseMessage NavigationAddresses(int key, int relatedKey)
		{
			Address address = null;
			var entity = GetExpandedEntity(key, x => x.Addresses);

			if (Request.Method == HttpMethod.Delete)
			{
				if (relatedKey == 0)
				{
					entity.BillingAddress = null;
					entity.ShippingAddress = null;
					entity.Addresses.Clear();
					Service.UpdateCustomer(entity);
				}
				else if ((address = _addressService.Value.GetAddressById(relatedKey)) != null)
				{
					entity.RemoveAddress(address);
					Service.UpdateCustomer(entity);
				}

				return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			address = _addressService.Value.GetAddressById(relatedKey);

			if (Request.Method == HttpMethod.Post)
			{
				if (address != null && entity.Addresses.FindAddress(address) == null)
				{
					entity.Addresses.Add(address);
					Service.UpdateCustomer(entity);

					return Request.CreateResponse(HttpStatusCode.Created, address);
				}
			}

			return Request.CreateResponseForEntity(address, relatedKey);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Address> GetBillingAddress(int key)
		{
			return GetRelatedEntity(key, x => x.BillingAddress);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
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
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IQueryable<Order> GetOrders(int key)
		{
			return GetRelatedCollection(key, x => x.Orders);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public IQueryable<ReturnRequest> GetReturnRequests(int key)
		{
			return GetRelatedCollection(key, x => x.ReturnRequests);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IQueryable<Address> GetAddresses(int key)
		{
			return GetRelatedCollection(key, x => x.Addresses);
		}

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public IQueryable<CustomerRoleMapping> GetCustomerRoleMappings(int key)
        {
            return GetRelatedCollection(key, x => x.CustomerRoleMappings);
        }

        #endregion
    }
}
