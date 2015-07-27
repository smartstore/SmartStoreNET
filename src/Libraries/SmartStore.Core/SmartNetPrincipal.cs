using System.Security.Principal;
using System.Web.Security;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core
{
	public interface ISmartNetPrincipal : IPrincipal
	{
		int CustomerId { get; set; }
	}


	public class SmartNetPrincipal : ISmartNetPrincipal
	{
		public SmartNetPrincipal(Customer customer, string type)
		{
			this.Identity = new GenericIdentity(customer.Username, type);
			this.CustomerId = customer.Id;
		}

		public bool IsInRole(string role)
		{
			return Identity != null && Identity.IsAuthenticated && role.HasValue() && Roles.IsUserInRole(Identity.Name, role);
		}

		public IIdentity Identity { get; private set; }
		public int CustomerId { get; set; }
	}
}
