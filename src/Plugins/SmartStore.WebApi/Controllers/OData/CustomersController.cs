using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
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
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Create)]
        public IHttpActionResult Post(Customer entity)
        {
            var result = Insert(entity, () => Service.InsertCustomer(entity));
            return result;
        }

        [WebApiQueryable]
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

        [WebApiQueryable]
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

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetAddresses(int key, int relatedKey = 0 /*addressId*/)
        {
            var addresses = GetRelatedCollection(key, x => x.Addresses);

            if (relatedKey != 0)
            {
                var address = addresses.FirstOrDefault(x => x.Id == relatedKey);

                return Ok(address);
            }

            return Ok(addresses);
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditAddress)]
        public IHttpActionResult PostAddresses(int key, int relatedKey /*addressId*/)
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

                return Created(address);
            }

            return Ok(address);
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditAddress)]
        public IHttpActionResult DeleteAddresses(int key, int relatedKey = 0 /*addressId*/)
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

            return StatusCode(HttpStatusCode.NoContent);
        }


        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetBillingAddress(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.BillingAddress));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetShippingAddress(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.ShippingAddress));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetOrders(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.Orders));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public IHttpActionResult GetReturnRequests(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.ReturnRequests));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public IHttpActionResult GetCustomerRoleMappings(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.CustomerRoleMappings));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetRewardPointsHistory(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.RewardPointsHistory));
        }

        #endregion
    }
}
