using System;
using System.Web;
using System.Web.Security;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Authentication
{
    public partial class FormsAuthenticationService : IAuthenticationService
    {
        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly TimeSpan _expirationTimeSpan;

        private Customer _cachedCustomer;

        public FormsAuthenticationService(HttpContextBase httpContext, ICustomerService customerService, CustomerSettings customerSettings)
        {
            _httpContext = httpContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _expirationTimeSpan = FormsAuthentication.Timeout;
        }

        public virtual void SignIn(Customer customer, bool createPersistentCookie)
        {
            var now = DateTime.UtcNow.ToLocalTime();
			var name = _customerSettings.CustomerLoginType != CustomerLoginType.Email ? customer.Username : customer.Email;


			var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
				name,
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
				name,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
			{
				HttpOnly = true,
				Secure = FormsAuthentication.RequireSSL,
				Path = FormsAuthentication.FormsCookiePath
			};

			if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            _httpContext.Response.Cookies.Add(cookie);
            _cachedCustomer = customer;
        }

        public virtual void SignOut()
        {
            _cachedCustomer = null;
            FormsAuthentication.SignOut();
        }

        public virtual Customer GetAuthenticatedCustomer()
        {
			if (_cachedCustomer != null)
				return _cachedCustomer;

			if (_httpContext?.Request == null || !_httpContext.Request.IsAuthenticated || _httpContext.User == null)
				return null;

			Customer customer = null;
			SmartStoreIdentity ident = null;

			if (_httpContext.User.Identity is FormsIdentity formsIdentity)
			{
				customer = GetAuthenticatedCustomerFromTicket(formsIdentity.Ticket);
			}
			else if ((ident = _httpContext.User.Identity as SmartStoreIdentity) != null)
			{
				customer = _customerService.GetCustomerById(ident.CustomerId);
			}

			if (customer != null && customer.Active && !customer.Deleted && customer.IsRegistered())
			{
				_cachedCustomer = customer;
			}

			return _cachedCustomer;
		}

        public virtual Customer GetAuthenticatedCustomerFromTicket(FormsAuthenticationTicket ticket)
        {
			Guard.NotNull(ticket, nameof(ticket));

            var usernameOrEmail = ticket.UserData;

            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            Customer customer = null;

            if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
            {
                customer = _customerService.GetCustomerByEmail(usernameOrEmail);
            }
            else if (_customerSettings.CustomerLoginType == CustomerLoginType.Username)
            {
                customer = _customerService.GetCustomerByUsername(usernameOrEmail);
            }
            else
            {
                customer = _customerService.GetCustomerByEmail(usernameOrEmail) ?? _customerService.GetCustomerByUsername(usernameOrEmail);
            }

            return customer;
        }
    }
}