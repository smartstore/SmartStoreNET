using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.WebApi.Configuration;
using System.Web.Http.OData.Routing.Conventions;
using SmartStore.Web.Framework.WebApi.OData;
using System.Web.Http.Cors;

namespace SmartStore.Web.Framework.WebApi
{   
    public class WebApiStartupTask : IStartupTask
    {
        public WebApiStartupTask()
        {
        }
        
        public void Execute()
        {
			var config = GlobalConfiguration.Configuration;

			var configBroadcaster = new WebApiConfigurationBroadcaster()
			{
				ModelBuilder = new ODataConventionModelBuilder(),
				RoutingConventions = ODataRoutingConventions.CreateDefault(),
				Routes = config.Routes
			};

			config.DependencyResolver = new AutofacWebApiDependencyResolver();

			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
			config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", "application/json"));
			config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "xml", "application/xml"));

			var queryAttribute = new WebApiQueryableAttribute()
			{
				MaxTop = WebApiGlobal.MaxTop
			};
			config.EnableQuerySupport(queryAttribute);

			var corsAttribute = new EnableCorsAttribute("*", "*", "*");
			config.EnableCors(corsAttribute);

			//config.EnableQuerySupport();
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

			var configPublisher = EngineContext.Current.Resolve<IWebApiConfigurationPublisher>();
			configPublisher.Configure(configBroadcaster);

			//config.Services.Insert(typeof(ModelBinderProvider), 0,
			//	new SimpleModelBinderProvider(typeof(Address), new AddressModelBinder()));


			config.Routes.MapHttpRoute(WebApiGlobal.RouteNameDefaultApi, "api/{version}/{controller}/{id}",
				new { version = "v1", controller = "Home", id = RouteParameter.Optional });

			config.Routes.MapODataRoute(WebApiGlobal.RouteNameDefaultOdata, WebApiGlobal.MostRecentOdataPath,
				configBroadcaster.ModelBuilder.GetEdmModel(), new DefaultODataPathHandler(), configBroadcaster.RoutingConventions);
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
