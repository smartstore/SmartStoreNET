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
					throw new HttpResponseException(HttpStatusCode.Forbidden);
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
					throw new HttpResponseException(HttpStatusCode.Forbidden);
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
					throw new HttpResponseException(HttpStatusCode.Forbidden);
				}

				Service.DeleteCustomer(entity);
			});

			return result;
		}

		#region Navigation properties

		[WebApiQueryable(PagingOptional = true)]
		[WebApiAuthenticate(Permission = Permissions.Customer.Read)]
		public HttpResponseMessage GetAddresses(int key, int relatedKey = 0 /*addressId*/)
		{
			var addresses = GetRelatedCollection(key, x => x.Addresses);

			if (relatedKey != 0)
			{
				var address = addresses.FirstOrDefault(x => x.Id == relatedKey);

				return Request.CreateResponseForEntity(address, relatedKey);
			}

			return Request.CreateResponseForEntity(addresses, key);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.EditAddress)]
		public HttpResponseMessage PostAddresses(int key, int relatedKey /*addressId*/)
		{
			var entity = GetExpandedEntity(key, x => x.Addresses);
			var address = entity.Addresses.FirstOrDefault(x => x.Id == relatedKey);

			if (address == null)
			{
				// No assignment yet.
				address = _addressService.Value.GetAddressById(relatedKey);
				if (address == null)
				{
					throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(relatedKey));
				}

				entity.Addresses.Add(address);
				Service.UpdateCustomer(entity);

				return Request.CreateResponse(HttpStatusCode.Created, address);
			}

			return Request.CreateResponse(HttpStatusCode.OK, address);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.EditAddress)]
		public HttpResponseMessage DeleteAddresses(int key, int relatedKey = 0 /*addressId*/)
		{
			var entity = GetExpandedEntity(key, x => x.Addresses);

			if (relatedKey == 0)
			{
				// Remove assignments of all addresses.
				entity.BillingAddress = null;
				entity.ShippingAddress = null;
				entity.Addresses.Clear();
				Service.UpdateCustomer(entity);
			}
			else
			{
				// Remove assignment of certain address.
				var address = _addressService.Value.GetAddressById(relatedKey);
				if (address != null)
				{
					entity.RemoveAddress(address);
					Service.UpdateCustomer(entity);
				}
			}

			return Request.CreateResponse(HttpStatusCode.NoContent);
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
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public IQueryable<CustomerRoleMapping> GetCustomerRoleMappings(int key)
        {
            return GetRelatedCollection(key, x => x.CustomerRoleMappings);
        }

        #endregion
    }
}
