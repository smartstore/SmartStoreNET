using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Core.Fakes
{
    public class FakeHttpRequest : HttpRequestBase
    {
        private readonly HttpCookieCollection _cookies;
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly NameValueCollection _serverVariables;
        private readonly string _relativeUrl;
        private readonly Uri _url;
        private readonly Uri _urlReferrer;
        private readonly string _httpMethod;
		private RequestContext _requestContext;

		public FakeHttpRequest(string relativeUrl, Uri url, Uri urlReferrer)
			: this(relativeUrl, HttpVerbs.Get.ToString("g"), url, urlReferrer, null, null, null, null)
		{
		}

        public FakeHttpRequest(string relativeUrl, 
			string method,
            NameValueCollection formParams,
			NameValueCollection queryStringParams,
            HttpCookieCollection cookies, 
			NameValueCollection serverVariables)
        {
            _httpMethod = method;
            _relativeUrl = relativeUrl;
            _formParams = formParams ?? new NameValueCollection();
            _queryStringParams = queryStringParams ?? new SmartStore.Collections.QueryString().FillFromString(relativeUrl);
            _cookies = cookies ?? new HttpCookieCollection();
            _serverVariables = serverVariables ?? new NameValueCollection();
        }


		public FakeHttpRequest(string relativeUrl,
			string method,
			Uri url,
			Uri urlReferrer,
			NameValueCollection formParams,
			NameValueCollection queryStringParams,
			HttpCookieCollection cookies,
			NameValueCollection serverVariables)
			: this(relativeUrl, method, formParams, queryStringParams, cookies, serverVariables)
		{
			_url = url;
			_urlReferrer = urlReferrer;
		}

        public override NameValueCollection ServerVariables
        {
            get
            {
                return _serverVariables;
            }
        }

        public override NameValueCollection Form
        {
            get { return _formParams; }
        }

        public override NameValueCollection QueryString
        {
            get { return _queryStringParams; }
        }

        public override HttpCookieCollection Cookies
        {
            get { return _cookies; }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return _relativeUrl; }
        }

        public override Uri Url
        {
            get
            {
                return _url ?? new Uri("http://tempuri.org");
            }
        }

        public override Uri UrlReferrer
        {
            get
            {
				return _urlReferrer ?? new Uri("http://tempuri.org");
            }
        }

        public override string PathInfo
        {
            get { return ""; }
        }

        public override string ApplicationPath
        {
            get
            {
                // We know that relative paths always start with ~/
                // ApplicationPath should start with /
                if (_relativeUrl != null && _relativeUrl.StartsWith("~/"))
                    return _relativeUrl.Remove(0, 1);
                return null;
            }
        }

        public override string HttpMethod
        {
            get
            {
                return _httpMethod;
            }
        }

        public override string UserHostAddress
        {
            get { return null; }
        }

		public override string RawUrl => this.ApplicationPath;
		public override bool IsSecureConnection => _url?.Scheme?.EmptyNull().StartsWith("https", StringComparison.OrdinalIgnoreCase) == true;
		public override bool IsAuthenticated => false;
		public override string[] UserLanguages => new string[] { };
		public override string UserAgent => "SmartStore.NET";
		public override bool IsLocal => false;

		public override RequestContext RequestContext
		{
			get
			{
				return _requestContext ?? new RequestContext();
			}

			set
			{
				_requestContext = value;
			}
		}
	}
}