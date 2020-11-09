using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Fakes;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.MVC.Tests.Framework.Controllers
{
    [TestFixture]
    public class RewriteUrlAttributeTests
    {
        [Test]
        public void Can_enforce_Ssl_Www()
        {
            var store = new Store
            {
                SslEnabled = true,
                ForceSslForAllPages = true,
                Url = "http://www.shop.com"
            };

            var attr = CreateRewriteUrlAttribute(store, "http://shop.com", CanonicalHostNameRule.RequireWww, out var context);

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsAssignableFrom<RedirectResult>(result);
            StringAssert.StartsWith("https://www.shop.com", result.Url);
        }

        [Test]
        public void Can_enforce_Ssl()
        {
            var store = new Store
            {
                SslEnabled = true,
                ForceSslForAllPages = true,
                Url = "http://www.shop.com"
            };

            var attr = CreateRewriteUrlAttribute(store, "http://shop.com", CanonicalHostNameRule.NoRule, out var context);

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsAssignableFrom<RedirectResult>(result);
            StringAssert.StartsWith("https://shop.com", result.Url);
        }

        [Test]
        public void Can_enforce_Www()
        {
            var store = new Store { Url = "http://www.shop.com" };

            var attr = CreateRewriteUrlAttribute(store, "http://shop.com", CanonicalHostNameRule.RequireWww, out var context);

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsAssignableFrom<RedirectResult>(result);
            StringAssert.StartsWith("http://www.shop.com", result.Url);
        }

        [Test]
        public void Can_omit_Www()
        {
            var store = new Store { Url = "http://www.shop.com" };

            var attr = CreateRewriteUrlAttribute(store, "http://www.shop.com", CanonicalHostNameRule.OmitWww, out var context);

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsAssignableFrom<RedirectResult>(result);
            StringAssert.StartsWith("http://shop.com", result.Url);
        }

        [Test]
        public void Can_enforce_NonSsl()
        {
            var store = new Store
            {
                SslEnabled = false,
                ForceSslForAllPages = false,
                Url = "http://www.shop.com"
            };

            var attr = CreateRewriteUrlAttribute(store, "https://www.shop.com", CanonicalHostNameRule.NoRule, out var context);
            attr.SslRequirement = SslRequirement.No;

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsAssignableFrom<RedirectResult>(result);
            StringAssert.StartsWith("http://www.shop.com", result.Url);
        }

        [Test]
        public void Can_retain_Ssl_Www()
        {
            var store = new Store
            {
                SslEnabled = true,
                ForceSslForAllPages = true,
                Url = "https://www.shop.com"
            };

            var attr = CreateRewriteUrlAttribute(store, "https://www.shop.com", CanonicalHostNameRule.RequireWww, out var context);

            attr.OnAuthorization(context);
            var result = context.Result as RedirectResult;

            Assert.IsNull(result);
        }

        private RewriteUrlAttribute CreateRewriteUrlAttribute(
            Store store,
            string requestUrl,
            CanonicalHostNameRule rule,
            out AuthorizationContext filterContext)
        {
            filterContext = null;

            var attr = new RewriteUrlAttribute(SslRequirement.Yes)
            {
                SeoSettings = new Lazy<SeoSettings>(() => new SeoSettings
                {
                    CanonicalHostNameRule = rule
                }),
                SecuritySettings = new Lazy<SecuritySettings>(() => new SecuritySettings
                {
                    UseSslOnLocalhost = true
                })
            };

            var storeContext = MockRepository.GenerateMock<IStoreContext>();
            storeContext.Expect(x => x.CurrentStore).Return(store);
            attr.StoreContext = new Lazy<IStoreContext>(() => storeContext);

            var workContext = MockRepository.GenerateMock<IWorkContext>();
            attr.WorkContext = new Lazy<IWorkContext>(() => workContext);

            var httpContext = new FakeHttpContext("~/", "GET");
            var httpRequest = new FakeHttpRequest("~/", new Uri(requestUrl), null);
            httpContext.SetRequest(httpRequest);

            var isHttps = httpRequest.IsSecureConnection;
            string secureUrl = isHttps ? requestUrl : requestUrl.Replace("http:", "https:");
            string nonSecureUrl = isHttps ? requestUrl.Replace("https:", "http:") : requestUrl;

            var webHelper = MockRepository.GenerateMock<IWebHelper>();
            webHelper.Expect(x => x.IsCurrentConnectionSecured()).Return(isHttps);
            webHelper.Expect(x => x.GetThisPageUrl(true, true)).Return(secureUrl);
            webHelper.Expect(x => x.GetThisPageUrl(true, false)).Return(nonSecureUrl);
            attr.WebHelper = new Lazy<IWebHelper>(() => webHelper);

            filterContext = new AuthorizationContext
            {
                HttpContext = httpContext
            };

            return attr;
        }
    }
}
