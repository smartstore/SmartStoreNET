//Contributor:  Nicholas Mayne

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;

namespace SmartStore.FacebookAuth.Core
{
    public class FacebookProviderAuthorizer : IOAuthProviderFacebookAuthorizer
	{
		#region Fields
		
		private readonly IExternalAuthorizer _authorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly HttpContextBase _httpContext;
		private readonly ICommonServices _services;

		private FacebookClient _facebookApplication;

		#endregion

		#region Ctor

        public FacebookProviderAuthorizer(IExternalAuthorizer authorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            HttpContextBase httpContext,
			ICommonServices services)
        {
            this._authorizer = authorizer;
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._httpContext = httpContext;
			this._services = services;
        }

		#endregion

		#region Utilities

		private FacebookClient FacebookApplication
        {
			get
			{
				if (_facebookApplication == null)
				{
					var settings = _services.Settings.LoadSetting<FacebookExternalAuthSettings>(_services.StoreContext.CurrentStore.Id);

					_facebookApplication = new FacebookClient(settings.ClientKeyIdentifier, settings.ClientSecret);
				}

				return _facebookApplication;
			}
        }

		private AuthorizeState VerifyAuthentication(string returnUrl)
        {
			var authResult = this.FacebookApplication.VerifyAuthentication(_httpContext, GenerateLocalCallbackUri());

			if (authResult.IsSuccessful)
			{
				if (!authResult.ExtraData.ContainsKey("id"))
					throw new Exception("Authentication result does not contain id data");

				if (!authResult.ExtraData.ContainsKey("accesstoken"))
					throw new Exception("Authentication result does not contain accesstoken data");

				var parameters = new OAuthAuthenticationParameters(Provider.SystemName)
				{
					ExternalIdentifier = authResult.ProviderUserId,
					OAuthToken = authResult.ExtraData["accesstoken"],
					OAuthAccessToken = authResult.ProviderUserId,
				};

				if (_externalAuthenticationSettings.AutoRegisterEnabled)
					ParseClaims(authResult, parameters);

				var result = _authorizer.Authorize(parameters);

				return new AuthorizeState(returnUrl, result);
			}

			var state = new AuthorizeState(returnUrl, OpenAuthenticationStatus.Error);
			var error = authResult.Error != null ? authResult.Error.Message : "Unknown error";
			state.AddError(error);
            return state;
        }

		private void ParseClaims(AuthenticationResult authenticationResult, OAuthAuthenticationParameters parameters)
        {
			var claims = new UserClaims();
			claims.Contact = new ContactClaims();
			
			if (authenticationResult.ExtraData.ContainsKey("username"))
				claims.Contact.Email = authenticationResult.ExtraData["username"];

			claims.Name = new NameClaims();

			if (authenticationResult.ExtraData.ContainsKey("name"))
			{
				var name = authenticationResult.ExtraData["name"];
				if (!String.IsNullOrEmpty(name))
				{
					var nameSplit = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (nameSplit.Length >= 2)
					{
						claims.Name.First = nameSplit[0];
						claims.Name.Last = nameSplit[1];
					}
					else
					{
						claims.Name.Last = nameSplit[0];
					}
				}
			}
            parameters.AddClaim(claims);
        }

		private AuthorizeState RequestAuthentication(string returnUrl)
        {
			var authUrl = GenerateServiceLoginUrl().AbsoluteUri;
			return new AuthorizeState("", OpenAuthenticationStatus.RequiresRedirect) { Result = new RedirectResult(authUrl) };
        }

		private Uri GenerateLocalCallbackUri()
		{
			string url = string.Format("{0}Plugins/SmartStore.FacebookAuth/logincallback/", _services.WebHelper.GetStoreLocation());
			return new Uri(url);
		}

		private Uri GenerateServiceLoginUrl()
		{
			//code copied from DotNetOpenAuth.AspNet.Clients.FacebookClient file
			var builder = new UriBuilder("https://www.facebook.com/dialog/oauth");
			var args = new Dictionary<string, string>();
			var settings = _services.Settings.LoadSetting<FacebookExternalAuthSettings>(_services.StoreContext.CurrentStore.Id);

			args.Add("client_id", settings.ClientKeyIdentifier);
			args.Add("redirect_uri", GenerateLocalCallbackUri().AbsoluteUri);
			args.Add("scope", "email");

			AppendQueryArgs(builder, args);

			return builder.Uri;
		}

		private void AppendQueryArgs(UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args)
		{
			if ((args != null) && (args.Count<KeyValuePair<string, string>>() > 0))
			{
				StringBuilder builder2 = new StringBuilder(50 + (args.Count<KeyValuePair<string, string>>() * 10));
				if (!string.IsNullOrEmpty(builder.Query))
				{
					builder2.Append(builder.Query.Substring(1));
					builder2.Append('&');
				}
				builder2.Append(CreateQueryString(args));
				builder.Query = builder2.ToString();
			}
		}

		private string CreateQueryString(IEnumerable<KeyValuePair<string, string>> args)
		{
			if (!args.Any<KeyValuePair<string, string>>())
			{
				return string.Empty;
			}
			StringBuilder builder = new StringBuilder(args.Count<KeyValuePair<string, string>>() * 10);
			foreach (KeyValuePair<string, string> pair in args)
			{
				builder.Append(EscapeUriDataStringRfc3986(pair.Key));
				builder.Append('=');
				builder.Append(EscapeUriDataStringRfc3986(pair.Value));
				builder.Append('&');
			}
			builder.Length--;
			return builder.ToString();
		}

		private readonly string[] UriRfc3986CharsToEscape = new string[] { "!", "*", "'", "(", ")" };

		private string EscapeUriDataStringRfc3986(string value)
		{
			StringBuilder builder = new StringBuilder(Uri.EscapeDataString(value));
			for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
			{
				builder.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
			}
			return builder.ToString();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Authorize response
		/// </summary>
		/// <param name="returnUrl">Return URL</param>
		/// <param name="verifyResponse">true - Verify response;false - request authentication;null - determine automatically</param>
		/// <returns>Authorize state</returns>
		public AuthorizeState Authorize(string returnUrl, bool? verifyResponse = null)
		{
			if (!verifyResponse.HasValue)
				throw new ArgumentException("Facebook plugin cannot automatically determine verifyResponse property");

			if (verifyResponse.Value)
			{
				return VerifyAuthentication(returnUrl);
			}
			else
			{
				return RequestAuthentication(returnUrl);
			}
		}

		#endregion
    }
}