using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.WebApi.Caching;

namespace SmartStore.Web.Framework.WebApi.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class WebApiAuthenticateAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected HmacAuthentication _hmac = new HmacAuthentication();

        /// <summary>
        /// The system name of the permission
        /// </summary>
        public string Permission { get; set; }

        protected string CreateContentMd5Hash(HttpRequestMessage request)
        {
            if (request != null && request.Content != null)
            {
                byte[] contentBytes = request.Content.ReadAsByteArrayAsync().Result;

                if (contentBytes != null && contentBytes.Length > 0)
                {
                    return _hmac.CreateContentMd5Hash(contentBytes);
                }
            }

            return "";
        }

        protected virtual bool HasPermission(IDependencyScope dependencyScope, Customer customer)
        {
            var result = true;

            if (Permission.HasValue())
            {
                try
                {
                    var permissionService = (IPermissionService)dependencyScope.GetService(typeof(IPermissionService));
                    result = permissionService.Authorize(Permission, customer);
                }
                catch { }
            }

            return result;
        }

        protected virtual void LogUnauthorized(HttpActionContext actionContext, IDependencyScope dependencyScope, HmacResult result, Customer customer)
        {
            try
            {
                var localization = (ILocalizationService)dependencyScope.GetService(typeof(ILocalizationService));
                var loggerFactory = (ILoggerFactory)dependencyScope.GetService(typeof(ILoggerFactory));
                var logger = loggerFactory.GetLogger(this.GetType());

                var strResult = result.ToString();
                var description = localization.GetResource("Admin.WebApi.AuthResult." + strResult, 0, false, strResult);

                logger.Warn(
                    new SecurityException("{0}\r\n{1}".FormatInvariant(description, actionContext.Request.Headers.ToString())),
                    localization.GetResource("Admin.WebApi.UnauthorizedRequest").FormatInvariant(strResult)
                );
            }
            catch (Exception ex)
            {
                ex.Dump();
            }
        }

        protected virtual Customer GetCustomer(IDependencyScope dependencyScope, int customerId)
        {
            Customer customer = null;

            try
            {
                var customerService = (ICustomerService)dependencyScope.GetService(typeof(ICustomerService));
                customer = customerService.GetCustomerById(customerId);
            }
            catch (Exception ex)
            {
                ex.Dump();
            }

            return customer;
        }

        protected virtual HmacResult IsAuthenticated(
            HttpActionContext actionContext,
            IDependencyScope dependencyScope,
            WebApiControllingCacheData controllingData,
            DateTime utcNow,
            out Customer customer)
        {
            customer = null;

            DateTime headDateTime;
            var request = HttpContext.Current.Request;
            var authorization = actionContext.Request.Headers.Authorization;

            if (request == null)
                return HmacResult.FailedForUnknownReason;

            if (controllingData.ApiUnavailable)
                return HmacResult.ApiUnavailable;

            if (authorization == null || authorization.Scheme.IsEmpty() || authorization.Parameter.IsEmpty())
                return HmacResult.InvalidAuthorizationHeader;

            string headContentMd5 = request.Headers["Content-Md5"] ?? request.Headers["Content-MD5"];
            string headTimestamp = request.Headers[WebApiGlobal.Header.Date];
            string headPublicKey = request.Headers[WebApiGlobal.Header.PublicKey];
            string signatureConsumer = authorization.Parameter;

            if (string.IsNullOrWhiteSpace(headPublicKey))
                return HmacResult.UserInvalid;

            if (!_hmac.IsAuthorizationHeaderValid(authorization.Scheme, signatureConsumer))
                return HmacResult.InvalidAuthorizationHeader;

            if (!_hmac.ParseTimestamp(headTimestamp, out headDateTime))
                return HmacResult.InvalidTimestamp;

            int maxMinutes = (controllingData.ValidMinutePeriod <= 0 ? WebApiGlobal.DefaultTimePeriodMinutes : controllingData.ValidMinutePeriod);

            if (Math.Abs((headDateTime - utcNow).TotalMinutes) > maxMinutes)
                return HmacResult.TimestampOutOfPeriod;

            var cacheUserData = WebApiCachingUserData.Data();

            var apiUser = cacheUserData.FirstOrDefault(x => x.PublicKey == headPublicKey);
            if (apiUser == null)
                return HmacResult.UserUnknown;

            if (!apiUser.Enabled)
                return HmacResult.UserDisabled;

            if (!controllingData.NoRequestTimestampValidation && apiUser.LastRequest.HasValue && headDateTime <= apiUser.LastRequest.Value)
                return HmacResult.TimestampOlderThanLastRequest;

            var context = new WebApiRequestContext
            {
                HttpMethod = request.HttpMethod,
                HttpAcceptType = request.Headers["Accept"],
                PublicKey = headPublicKey,
                SecretKey = apiUser.SecretKey,
                Url = HttpUtility.UrlDecode(request.Url.AbsoluteUri.ToLower())
            };

            var contentMd5 = CreateContentMd5Hash(actionContext.Request);

            if (headContentMd5.HasValue() && headContentMd5 != contentMd5)
                return HmacResult.ContentMd5NotMatching;

            var messageRepresentation = _hmac.CreateMessageRepresentation(context, contentMd5, headTimestamp);

            if (string.IsNullOrEmpty(messageRepresentation))
                return HmacResult.MissingMessageRepresentationParameter;

            var signatureProvider = _hmac.CreateSignature(apiUser.SecretKey, messageRepresentation);

            if (signatureProvider != signatureConsumer)
            {
                if (controllingData.AllowEmptyMd5Hash)
                {
                    messageRepresentation = _hmac.CreateMessageRepresentation(context, null, headTimestamp);

                    signatureProvider = _hmac.CreateSignature(apiUser.SecretKey, messageRepresentation);

                    if (signatureProvider != signatureConsumer)
                        return HmacResult.InvalidSignature;
                }
                else
                {
                    return HmacResult.InvalidSignature;
                }
            }

            customer = GetCustomer(dependencyScope, apiUser.CustomerId);
            if (customer == null)
                return HmacResult.UserUnknown;

            if (!customer.Active || customer.Deleted)
                return HmacResult.UserIsInactive;

            if (!HasPermission(dependencyScope, customer))
                return HmacResult.UserHasNoPermission;

            //var headers = HttpContext.Current.Response.Headers;
            //headers.Add(ApiHeaderName.LastRequest, apiUser.LastRequest.HasValue ? apiUser.LastRequest.Value.ToString("o") : "");

            apiUser.LastRequest = headDateTime;

            return HmacResult.Success;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var result = HmacResult.FailedForUnknownReason;
            var controllingData = WebApiCachingControllingData.Data();
            var dependencyScope = actionContext.Request.GetDependencyScope();
            var utcNow = DateTime.UtcNow;
            Customer customer = null;

            try
            {
                result = IsAuthenticated(actionContext, dependencyScope, controllingData, utcNow, out customer);
            }
            catch (Exception ex)
            {
                ex.Dump();
            }

            if (result == HmacResult.Success)
            {
                // Inform core about the authentication. Note, you cannot use IWorkContext.set_CurrentCustomer here.
                HttpContext.Current.User = new SmartStorePrincipal(customer, HmacAuthentication.Scheme1);

                var response = HttpContext.Current.Response;

                response.AddHeader(WebApiGlobal.Header.AppVersion, SmartStoreVersion.CurrentFullVersion);
                response.AddHeader(WebApiGlobal.Header.Version, controllingData.Version);
                response.AddHeader(WebApiGlobal.Header.MaxTop, controllingData.MaxTop.ToString());
                response.AddHeader(WebApiGlobal.Header.Date, utcNow.ToString("o"));
                response.AddHeader(WebApiGlobal.Header.CustomerId, customer.Id.ToString());

                response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

                var headers = actionContext.Response.Headers;
                var authorization = actionContext.Request.Headers.Authorization;

                // See RFC-2616
                var scheme = _hmac.GetWwwAuthenticateScheme(authorization != null ? authorization.Scheme : null);
                headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(scheme));

                headers.Add(WebApiGlobal.Header.AppVersion, SmartStoreVersion.CurrentFullVersion);
                headers.Add(WebApiGlobal.Header.Version, controllingData.Version);
                headers.Add(WebApiGlobal.Header.MaxTop, controllingData.MaxTop.ToString());
                headers.Add(WebApiGlobal.Header.Date, utcNow.ToString("o"));
                headers.Add(WebApiGlobal.Header.HmacResultId, ((int)result).ToString());
                headers.Add(WebApiGlobal.Header.HmacResultDescription, result.ToString());

                if (result == HmacResult.UserHasNoPermission && Permission.HasValue())
                {
                    headers.Add(WebApiGlobal.Header.MissingPermission, Permission);
                }

                if (controllingData.LogUnauthorized)
                {
                    LogUnauthorized(actionContext, dependencyScope, result, customer);
                }
            }
        }

        /// <remarks>we should never get here... just for security reason</remarks>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var message = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            throw new HttpResponseException(message);
        }
    }
}
