using System;
using NUnit.Framework;
using SmartStore.Tests;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.Web.MVC.Tests.Framework.WebApi
{
    [TestFixture]
    public class WebApiTests
    {
        private HmacAuthentication _hmacAuth;

        [SetUp]
        public void SetUp()
        {
            _hmacAuth = new HmacAuthentication();
        }

        [TestCase("2013-11-09T11:37:21.1918793Z")]
        [TestCase("2013-11-09T11:37:21.191879Z")]
        [TestCase("2013-11-09T11:37:21.19187Z")]
        [TestCase("2013-11-09T11:37:21.1918Z")]
        [TestCase("2013-11-09T11:37:21.191Z")]
        [TestCase("2013-11-09T11:37:21.19Z")]
        [TestCase("2013-11-09T11:37:21.1Z")]
        [TestCase("2013-11-09T11:37:21Z")]
        public void Api_can_parse_timestamp(string timestamp)
        {
            DateTime dt;

            _hmacAuth.ParseTimestamp(timestamp, out dt).ShouldBeTrue();
        }
    }
}
