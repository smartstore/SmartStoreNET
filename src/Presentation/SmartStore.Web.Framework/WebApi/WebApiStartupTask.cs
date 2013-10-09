using System.Linq.Expressions;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using SmartStore.Core.Domain.Affiliates;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.WebApi.Routes;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.Web.Framework.WebApi
{
    
    public class WebApiStartupTask : IStartupTask
    {
        
        public WebApiStartupTask()
        {
        }
        
        public void Execute()
        {
            ConfigureWebApi(GlobalConfiguration.Configuration);
            ConfigureOData(GlobalConfiguration.Configuration);
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            //// set controller selector
            //config.Services.Replace(typeof(IHttpControllerSelector), new SmartStoreWebApiHttpControllerSelector(config));
            
            // set dependency resolver
            config.DependencyResolver = new AutofacWebApiDependencyResolver();

            // set json indentation
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            // config media type mappings
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", "application/json"));
            config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "xml", "application/xml"));

            // install permissions
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            permissionService.InstallPermissions(new WebApiPermissionProvider());

            // register custom web api routes (from plugins etc.)
            var routePublisher = EngineContext.Current.Resolve<IHttpRoutePublisher>();
            routePublisher.RegisterRoutes(config.Routes);

            // register default api route
			if (!config.Routes.ContainsKey("DefaultApi"))
			{
				config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "api/{controller}/{id}",
					defaults: new { id = RouteParameter.Optional }
				);
			}
        }

        private IEdmModel GetEdmModel()
        {
            //// TODO: EntitySets aus Plugins sammeln und hier registrieren
            //ODataModelBuilder modelBuilder = new ODataModelBuilder();
            //modelBuilder.EntitySet<Product>("Products");
            //modelBuilder.EntitySet<Order>("Orders");
            //modelBuilder.EntitySet<Affiliate>("Affiliates");
            //modelBuilder.EntitySet<OrderNote>("OrderNotes");
            //modelBuilder.EntitySet<Customer>("Customers");
            //modelBuilder.EntitySet<Address>("Addresses");
            //modelBuilder.EntitySet<Shipment>("Shipments");

            //var orders = modelBuilder.EntitySet<Order>("Orders").EntityType;
            //modelBuilder.EntitySet<Order>("Orders").HasRequiredBinding(x => x.Customer, modelBuilder.EntitySet<Customer>("Customers"));
            //orders.Ignore(x => x.RedeemedRewardPointsEntry);
            //orders.Ignore(x => x.DiscountUsageHistory);
            //orders.Ignore(x => x.GiftCardUsageHistory);
            //orders.Ignore(x => x.OrderProductVariants);
            ////order.Ignore(x => x.Shipments);

            ODataModelBuilder modelBuilder = new ODataModelBuilder();
            var orders = modelBuilder.EntitySet<Order>("Orders");
            orders.HasIdLink(entityContext => entityContext.Url.Link(
                "OData.GetById",
                new { controller = "Orders", id = entityContext.EntityInstance.Id }
            ), true);
            orders.HasEditLink(entityContext => entityContext.Url.Link(
                "OData.GetById",
                new { controller = "Customers", id = entityContext.EntityInstance.Id }
            ), true);
            var order = orders.EntityType;
            order.HasKey(p => p.Id);
            order.Property(p => p.OrderTotal);
            order.Property(p => p.CreatedOnUtc);
            order.Property(p => p.ShippingMethod);
            
            IEdmModel model = modelBuilder.GetEdmModel();

            return model;
        }

        private void ConfigureOData(HttpConfiguration config)
        {

            config.EnableQuerySupport();

            var model = GetEdmModel();

			if (!config.Routes.ContainsKey("ODataRoute.Default"))
			{
				config.Routes.MapODataRoute("ODataRoute.Default", "odata", model);
			}

            //// set json indentation
            //config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            //// config media type mappings
            //config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", "application/json"));
            //config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "xml", "application/xml"));
        }

        public int Order
        {
            get { return 0; }
        }
    }

}
