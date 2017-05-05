using AutoMapper;
using SmartStore.Admin.Infrastructure;
using NUnit.Framework;

namespace SmartStore.Web.MVC.Tests.Admin.Infrastructure
{
    [TestFixture]
    public class AutoMapperConfigurationTest
    {
        [Test]
        public void Configuration_is_valid()
        {
			var config = new MapperConfiguration(cfg => {
				cfg.AddProfile(new AutoMapperAdminProfile());
			});

			config.AssertConfigurationIsValid();
		}
    }
}
