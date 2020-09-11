using System.Web.Routing;
using NUnit.Framework;
using SmartStore.Core.Data;

namespace SmartStore.Web.MVC.Tests.Public.Infrastructure
{
    [TestFixture]
    public abstract class RoutesTestsBase
    {
        [SetUp]
        public void Setup()
        {
            //var typeFinder = new WebAppTypeFinder();
            //var routePublisher = new RoutePublisher(typeFinder);
            //routePublisher.RegisterRoutes(RouteTable.Routes);

            DataSettings.SetTestMode(true);

            new SmartStore.Web.Infrastructure.StoreRoutes().RegisterRoutes(RouteTable.Routes);
            new SmartStore.Web.Infrastructure.GeneralRoutes().RegisterRoutes(RouteTable.Routes);
            new SmartStore.Web.Infrastructure.SeoRoutes().RegisterRoutes(RouteTable.Routes);
        }

        [TearDown]
        public void TearDown()
        {
            RouteTable.Routes.Clear();
        }
    }
}
