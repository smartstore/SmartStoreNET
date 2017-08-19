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
            this._httpContext = httpContext;
            this._customerService = customerService;
            this._customerSettings = customerSettings;
            this._expirationTimeSpan = FormsAuthentication.Timeout;
        }


        public virtual void SignIn(Customer customer, bool createPersistentCookie)
        {
            var now = DateTime.UtcNow.ToLocalTime();

            var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
                _customerSettings.UsernamesEnabled ? customer.Username : customer.Email,
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
                _customerSettings.UsernamesEnabled ? customer.Username : customer.Email,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
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
			FormsIdentity formsIdentity = null;
			SmartStoreIdentity smartNetIdentity = null;

			if ((formsIdentity = _httpContext.User.Identity as FormsIdentity) != null)
			{
				customer = GetAuthenticatedCustomerFromTicket(formsIdentity.Ticket);
			}
			else if ((smartNetIdentity = _httpContext.User.Identity as SmartStoreIdentity) != null)
			{
				customer = _customerService.GetCustomerById(smartNetIdentity.CustomerId);
			}

			if (customer != null && customer.Active && !customer.Deleted && customer.IsRegistered())
			{
				_cachedCustomer = customer;
			}

			return _cachedCustomer;
		}

        public virtual Customer GetAuthenticatedCustomerFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var usernameOrEmail = ticket.UserData;

            if (String.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            var customer = _customerSettings.UsernamesEnabled
                ? _customerService.GetCustomerByUsername(usernameOrEmail)
                : _customerService.GetCustomerByEmail(usernameOrEmail);

            return customer;
        }
    }
}