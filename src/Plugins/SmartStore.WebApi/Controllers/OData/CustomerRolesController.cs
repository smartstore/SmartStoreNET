using System.Web.Http;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CustomerRolesController : WebApiEntityController<CustomerRole, ICustomerService>
	{
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Create)]
		protected override void Insert(CustomerRole entity)
		{
			Service.InsertCustomerRole(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Update)]
        protected override void Update(CustomerRole entity)
		{
			if (entity != null && entity.IsSystemRole)
			{
				throw this.ExceptionForbidden();
			}

			Service.UpdateCustomerRole(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Delete)]
        protected override void Delete(CustomerRole entity)
		{
			if (entity != null && entity.IsSystemRole)
			{
				throw this.ExceptionForbidden();
			}

			Service.DeleteCustomerRole(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Role.Read)]
        public SingleResult<CustomerRole> GetCustomerRole(int key)
		{
			return GetSingleResult(key);
		}
	}
}