using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Security;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core
{
	public interface ISmartNetPrincipal : IPrincipal
	{
	}


	public class SmartNetIdentity : ClaimsIdentity
	{
		[SecuritySafeCritical]
		public SmartNetIdentity(int customerId, string name, string type) 
			: base(new GenericIdentity(name, type))
		{
			CustomerId = customerId;
		}

		public int CustomerId { get; private set; }

		public override bool IsAuthenticated { get { return CustomerId != 0; } }
	}


	public class SmartNetPrincipal : ISmartNetPrincipal
	{
		public SmartNetPrincipal(Customer customer, string type)
		{
			this.Identity = new SmartNetIdentity(customer.Id, customer.Username, type);
		}

		public bool IsInRole(string role)
		{
			return (Identity != null && Identity.IsAuthenticated && role.HasValue() && Roles.IsUserInRole(Identity.Name, role));
		}

		public IIdentity Identity { get; private set; }
	}
}
