using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi.Caching;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Services
{
    public class WebApiPluginService : IWebApiPluginService
    {
        private readonly IRepository<GenericAttribute> _genericAttributes;
        private readonly IRepository<Customer> _customers;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;

        public WebApiPluginService(
            IRepository<GenericAttribute> genericAttributes,
            IRepository<Customer> customers,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService)
        {
            _genericAttributes = genericAttributes;
            _customers = customers;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
        }

        public IQueryable<Customer> GetCustomers()
        {
            var registeredRoleId = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id;

            var query =
                from c in _customers.Table
                join a in
                    (
                        from a in _genericAttributes.Table
                        where a.KeyGroup == "Customer" && a.Key == WebApiCachingUserData.Key
                        select a
                    )
                on c.Id equals a.EntityId into ga
                from a in ga.DefaultIfEmpty()
                where !c.Deleted && c.CustomerRoleMappings.Select(rm => rm.CustomerRoleId).Contains(registeredRoleId)
                orderby a.Value descending
                select c;

            return query;
        }

        public bool CreateKeys(int customerId)
        {
            if (customerId != 0)
            {
                var hmac = new HmacAuthentication();
                var userData = WebApiCachingUserData.Data();
                string key1, key2;

                for (int i = 0; i < 9999; ++i)
                {
                    if (hmac.CreateKeys(out key1, out key2) && !userData.Exists(x => x.PublicKey.IsCaseInsensitiveEqual(key1)))
                    {
                        var apiUser = new WebApiUserCacheData
                        {
                            CustomerId = customerId,
                            PublicKey = key1,
                            SecretKey = key2,
                            Enabled = true
                        };

                        RemoveKeys(customerId);

                        var attribute = new GenericAttribute
                        {
                            EntityId = customerId,
                            KeyGroup = "Customer",
                            Key = WebApiCachingUserData.Key,
                            Value = apiUser.ToString()
                        };

                        _genericAttributeService.InsertAttribute(attribute);

                        WebApiCachingUserData.Remove();
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveKeys(int customerId)
        {
            if (customerId != 0)
            {
                var data = (
                    from a in _genericAttributes.Table
                    where a.EntityId == customerId && a.KeyGroup == "Customer" && a.Key == WebApiCachingUserData.Key
                    select a).ToList();

                if (data.Count > 0)
                {
                    foreach (var itm in data)
                        _genericAttributeService.DeleteAttribute(itm);

                    WebApiCachingUserData.Remove();
                }
            }
        }

        public void EnableOrDisableUser(int customerId, bool enable)
        {
            if (customerId != 0)
            {
                var cacheData = WebApiCachingUserData.Data();
                var apiUser = cacheData.FirstOrDefault(x => x.CustomerId == customerId);

                if (apiUser != null)
                {
                    apiUser.Enabled = enable;

                    var attribute = _genericAttributeService.GetAttributeById(apiUser.GenericAttributeId);
                    if (attribute != null)
                    {
                        attribute.Value = apiUser.ToString();
                        _genericAttributeService.UpdateAttribute(attribute);
                    }
                }
            }
        }
    }
}
