//Contributor:  Nicholas Mayne

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
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

        private FacebookOAuth2Client _facebookApplication;

        #endregion

        #region Ctor

        public FacebookProviderAuthorizer(IExternalAuthorizer authorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            HttpContextBase httpContext,
            ICommonServices services)
        {
            _authorizer = authorizer;
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _httpContext = httpContext;
            _services = services;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #endregion

        #region Utilities

        private FacebookOAuth2Client FacebookApplication
        {
            get
            {
                if (_facebookApplication == null)
                {
                    var settings = _services.Settings.LoadSetting<FacebookExternalAuthSettings>(_services.StoreContext.CurrentStore.Id);

                    _facebookApplication = new FacebookOAuth2Client(settings.ClientKeyIdentifier, settings.ClientSecret);
                }

                return _facebookApplication;
            }
        }

        private AuthorizeState VerifyAuthentication(string returnUrl)
        {
            string error = null;
            AuthenticationResult authResult = null;

            try
            {
                authResult = this.FacebookApplication.VerifyAuthentication(_httpContext, GenerateLocalCallbackUri());
            }
            catch (WebException wexc)
            {
                using (var response = wexc.Response as HttpWebResponse)
                {
                    error = response.StatusDescription;

                    var enc = Encoding.GetEncoding(response.CharacterSet);
                    using (var reader = new StreamReader(response.GetResponseStream(), enc))
                    {
                        var rawResponse = reader.ReadToEnd();
                        Logger.Log(LogLevel.Error, new Exception(rawResponse), response.StatusDescription, null);
                    }
                }
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                Logger.Log(LogLevel.Error, exception, null, null);
            }

            if (authResult != null && authResult.IsSuccessful)
            {
                if (!authResult.ExtraData.ContainsKey("id"))
                    throw new Exception("Authentication result does not contain id data");

                if (!authResult.ExtraData.ContainsKey("accesstoken"))
                    throw new Exception("Authentication result does not contain accesstoken data");

                var parameters = new OAuthAuthenticationParameters(FacebookExternalAuthMethod.SystemName)
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

            if (error.IsEmpty() && authResult != null && authResult.Error != null)
            {
                error = authResult.Error.Message;
            }
            if (error.IsEmpty())
            {
                error = _services.Localization.GetResource("Admin.Common.UnknownError");
            }

            var state = new AuthorizeState(returnUrl, OpenAuthenticationStatus.Error);
            state.AddError(error);

            return state;
        }

        private string GetEmailFromFacebook(string accessToken)
        {
            var result = "";
            var webRequest = WebRequest.Create("https://graph.facebook.com/me?fields=email&access_token=" + EscapeUriDataStringRfc3986(accessToken));

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var strResponse = reader.ReadToEnd();
                var info = JObject.Parse(strResponse);

                if (info["email"] != null)
                {
                    result = info["email"].ToString();
                }
            }
            return result;
        }

        private void ParseClaims(AuthenticationResult authenticationResult, OAuthAuthenticationParameters parameters)
        {
            var claims = new UserClaims();
            claims.Contact = new ContactClaims();

            if (authenticationResult.ExtraData.ContainsKey("username"))
            {
                claims.Contact.Email = authenticationResult.ExtraData["username"];
            }
            else
            {
                claims.Contact.Email = GetEmailFromFacebook(authenticationResult.ExtraData["accesstoken"]);
            }

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