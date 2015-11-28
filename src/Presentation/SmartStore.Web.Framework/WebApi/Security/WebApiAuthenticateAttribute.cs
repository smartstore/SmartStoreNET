using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
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
					return _hmac.CreateContentMd5Hash(contentBytes);
			}
			return "";
		}
		protected virtual bool HasPermission(HttpActionContext actionContext, Customer customer)
		{
			bool result = true;

			try
			{
				if (Permission.HasValue())
				{
					var permissionService = EngineContext.Current.Resolve<IPermissionService>();

					if (permissionService.GetPermissionRecordBySystemName(Permission) != null)
					{
						result = permissionService.Authorize(Permission, customer);
					}
				}
			}
			catch {	}

			return result;
		}
		protected virtual void LogUnauthorized(HttpActionContext actionContext, HmacResult result, Customer customer)
		{
			try
			{
				var logger = EngineContext.Current.Resolve<ILogger>();
				var localization = EngineContext.Current.Resolve<ILocalizationService>();

				string strResult = result.ToString();
				string description = localization.GetResource("Admin.WebApi.AuthResult." + strResult, 0, false, strResult);

				var logContext = new LogContext
				{
					ShortMessage = localization.GetResource("Admin.WebApi.UnauthorizedRequest").FormatWith(strResult),
					FullMessage = "{0}\r\n{1}".FormatWith(description, actionContext.Request.Headers.ToString()),
					LogLevel = LogLevel.Warning,
					Customer = customer,
					HashNotFullMessage = true,
					HashIpAddress = true
				};

				logger.InsertLog(logContext);
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
		}
		protected virtual Customer GetCustomer(int customerId)
		{
			Customer customer = null;
			try
			{
				customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(customerId);
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return customer;
		}

		protected virtual HmacResult IsAuthenticated(HttpActionContext actionContext, DateTime now, WebApiControllingCacheData cacheControllingData, out Customer customer)
		{
			customer = null;

			var request = HttpContext.Current.Request;
			DateTime headDateTime;

			if (request == null)
				return HmacResult.FailedForUnknownReason;

			if (cacheControllingData.ApiUnavailable)
				return HmacResult.ApiUnavailable;

			string headContentMd5 = request.Headers["Content-Md5"] ?? request.Headers["Content-MD5"];
			string headTimestamp = request.Headers[WebApiGlobal.Header.Date];
			string headPublicKey = request.Headers[WebApiGlobal.Header.PublicKey];
			string scheme = actionContext.Request.Headers.Authorization.Scheme;
			string signatureConsumer = actionContext.Request.Headers.Authorization.Parameter;

			if (string.IsNullOrWhiteSpace(headPublicKey))
				return HmacResult.UserInvalid;

			if (!_hmac.IsAuthorizationHeaderValid(scheme, signatureConsumer))
				return HmacResult.InvalidAuthorizationHeader;

			if (!_hmac.ParseTimestamp(headTimestamp, out headDateTime))
				return HmacResult.InvalidTimestamp;

			int maxMinutes = (cacheControllingData.ValidMinutePeriod <= 0 ? WebApiGlobal.DefaultTimePeriodMinutes : cacheControllingData.ValidMinutePeriod);

			if (Math.Abs((headDateTime - now).TotalMinutes) > maxMinutes)
				return HmacResult.TimestampOutOfPeriod;

			var cacheUserData = WebApiCachingUserData.Data();

			var apiUser = cacheUserData.FirstOrDefault(x => x.PublicKey == headPublicKey);
			if (apiUser == null)
				return HmacResult.UserUnknown;

			if (!apiUser.Enabled)
				return HmacResult.UserDisabled;

			if (!cacheControllingData.NoRequestTimestampValidation && apiUser.LastRequest.HasValue && headDateTime <= apiUser.LastRequest.Value)
				return HmacResult.TimestampOlderThanLastRequest;

			var context = new WebApiRequestContext
			{
				HttpMethod = request.HttpMethod,
				HttpAcceptType = request.Headers["Accept"],
				PublicKey = headPublicKey,
				SecretKey = apiUser.SecretKey,
				Url = HttpUtility.UrlDecode(request.Url.AbsoluteUri.ToLower())
			};

			string contentMd5 = CreateContentMd5Hash(actionContext.Request);

			if (headContentMd5.HasValue() && headContentMd5 != contentMd5)
				return HmacResult.ContentMd5NotMatching;

			string messageRepresentation = _hmac.CreateMessageRepresentation(context, contentMd5, headTimestamp);

			if (string.IsNullOrEmpty(messageRepresentation))
				return HmacResult.MissingMessageRepresentationParameter;

			string signatureProvider = _hmac.CreateSignature(apiUser.SecretKey, messageRepresentation);

			if (signatureProvider != signatureConsumer)
				return HmacResult.InvalidSignature;

			customer = GetCustomer(apiUser.CustomerId);
			if (customer == null)
				return HmacResult.UserUnknown;

			if (!customer.Active || customer.Deleted)
				return HmacResult.UserIsInactive;

			if (!HasPermission(actionContext, customer))
				return HmacResult.UserHasNoPermission;

			//var headers = HttpContext.Current.Response.Headers;
			//headers.Add(ApiHeaderName.LastRequest, apiUser.LastRequest.HasValue ? apiUser.LastRequest.Value.ToString("o") : "");

			apiUser.LastRequest = headDateTime;

			return HmacResult.Success;
		}

		public override void OnAuthorization(HttpActionContext actionContext)
		{
			var result = HmacResult.FailedForUnknownReason;
			var cacheControllingData = WebApiCachingControllingData.Data();
			var now = DateTime.UtcNow;
			Customer customer = null;

			try
			{
				result = IsAuthenticated(actionContext, now, cacheControllingData, out customer);
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			if (result == HmacResult.Success)
			{
				// inform core about the authentication. note you cannot use IWorkContext.set_CurrentCustomer here.
				HttpContext.Current.User = new SmartStorePrincipal(customer, HmacAuthentication.Scheme1);

				var response = HttpContext.Current.Response;

				response.AddHeader(WebApiGlobal.Header.Version, cacheControllingData.Version);
				response.AddHeader(WebApiGlobal.Header.MaxTop, WebApiGlobal.MaxTop.ToString());
				response.AddHeader(WebApiGlobal.Header.Date, now.ToString("o"));
				response.AddHeader(WebApiGlobal.Header.CustomerId, customer.Id.ToString());

				response.Cache.SetCacheability(HttpCacheability.NoCache);
			}
			else
			{
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

				var headers = actionContext.Response.Headers;

				var scheme = _hmac.GetWwwAuthenticateScheme(actionContext.Request.Headers.Authorization.Scheme);
				headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(scheme));		// see RFC-2616

				headers.Add(WebApiGlobal.Header.Version, cacheControllingData.Version);
				headers.Add(WebApiGlobal.Header.MaxTop, WebApiGlobal.MaxTop.ToString());
				headers.Add(WebApiGlobal.Header.Date, now.ToString("o"));
				headers.Add(WebApiGlobal.Header.HmacResultId, ((int)result).ToString());
				headers.Add(WebApiGlobal.Header.HmacResultDescription, result.ToString());

				if (cacheControllingData.LogUnauthorized)
					LogUnauthorized(actionContext, result, customer);
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
