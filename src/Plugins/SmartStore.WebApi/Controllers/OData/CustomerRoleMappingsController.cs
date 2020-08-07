using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CustomerRoleMappingsController : WebApiEntityController<CustomerRoleMapping, ICustomerService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public IQueryable<CustomerRoleMapping> Get()
        {
            return GetEntitySet();
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public SingleResult<CustomerRoleMapping> Get(int key)
        {
            return GetSingleResult(key);
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        public IHttpActionResult Post(CustomerRoleMapping entity)
        {
            var result = Insert(entity, () => Service.InsertCustomerRoleMapping(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        public async Task<IHttpActionResult> Put(int key, CustomerRoleMapping entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateCustomerRoleMapping(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        public async Task<IHttpActionResult> Patch(int key, Delta<CustomerRoleMapping> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateCustomerRoleMapping(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteCustomerRoleMapping(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Customer> GetCustomer(int key)
        {
            return GetRelatedEntity(key, x => x.Customer);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public SingleResult<CustomerRole> GetCustomerRole(int key)
        {
            return GetRelatedEntity(key, x => x.CustomerRole);
        }

        #endregion
    }
}