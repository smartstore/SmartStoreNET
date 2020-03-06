using System.Web.Http;
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
        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        protected override void Insert(CustomerRoleMapping entity)
        {
            Service.InsertCustomerRoleMapping(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        protected override void Update(CustomerRoleMapping entity)
        {
            Service.UpdateCustomerRoleMapping(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.EditRole)]
        protected override void Delete(CustomerRoleMapping entity)
        {
            Service.DeleteCustomerRoleMapping(entity);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public SingleResult<CustomerRoleMapping> GetCustomerRoleMapping(int key)
        {
            return GetSingleResult(key);
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