using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly PrivacySettings _privacySettings;
        private readonly TimeSpan _expirationTimeSpan;

        private Customer _cachedCustomer;

        public FormsAuthenticationService(
            HttpContextBase httpContext,
            ICustomerService customerService,
            CustomerSettings customerSettings,
            PrivacySettings privacySettings)
        {
            _httpContext = httpContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _privacySettings = privacySettings;
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
                Path = FormsAuthentication.FormsCookiePath,
                Secure = FormsAuthentication.RequireSSL,
                SameSite = FormsAuthentication.RequireSSL ? (SameSiteMode)_privacySettings.SameSiteMode : SameSiteMode.Lax
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

            if (_httpContext?.Request == null || !_httpContext.Request.IsAuthenticated)
                return null;

            Customer customer = null;

            if (_httpContext.User.Identity is FormsIdentity formsIdent)
            {
                customer = GetAuthenticatedCustomerFromTicket(formsIdent.Ticket);
            }
            else if (_httpContext.User.Identity is SmartStoreIdentity ident)
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

            if (!usernameOrEmail.HasValue())
                return null;

            List<Func<string, Customer>> customerResolvers = new List<Func<string, Customer>>(2);

            if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
            {
                customerResolvers.Add(_customerService.GetCustomerByEmail);
            }
            else if (_customerSettings.CustomerLoginType == CustomerLoginType.Username)
            {
                customerResolvers.Add(_customerService.GetCustomerByUsername);
            }
            else
            {
                customerResolvers.Add(_customerService.GetCustomerByUsername);
                var mayBeEmail = usernameOrEmail.IndexOf('@') > -1;
                if (mayBeEmail)
                {
                    customerResolvers.Insert(0, _customerService.GetCustomerByEmail);
                }
                else
                {
                    customerResolvers.Add(_customerService.GetCustomerByEmail);
                }
            }

            return customerResolvers
                .Select(x => x.Invoke(usernameOrEmail))
                .Where(x => x != null)
                .FirstOrDefault();
        }
    }
}