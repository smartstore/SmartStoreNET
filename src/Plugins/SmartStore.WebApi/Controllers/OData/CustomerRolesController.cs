using System.Web.Http;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCustomers")]
	public class CustomerRolesController : WebApiEntityController<CustomerRole, ICustomerService>
	{
		protected override void Insert(CustomerRole entity)
		{
			Service.InsertCustomerRole(entity);
		}
		protected override void Update(CustomerRole entity)
		{
			if (entity != null && entity.IsSystemRole)
			{
				throw this.ExceptionForbidden();
			}

			Service.UpdateCustomerRole(entity);
		}
		protected override void Delete(CustomerRole entity)
		{
			if (entity != null && entity.IsSystemRole)
			{
				throw this.ExceptionForbidden();
			}

			Service.DeleteCustomerRole(entity);
		}

		[WebApiQueryable]
		public SingleResult<CustomerRole> GetCustomerRole(int key)
		{
			return GetSingleResult(key);
		}
	}
}