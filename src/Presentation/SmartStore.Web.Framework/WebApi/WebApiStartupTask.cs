using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Newtonsoft.Json;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;

namespace SmartStore.Web.Framework.WebApi
{
    public class WebApiStartupTask : IApplicationStart
    {
		public int Order => 0;

		public void Start()
        {
			var config = GlobalConfiguration.Configuration;

			var configBroadcaster = new WebApiConfigurationBroadcaster
			{
				Configuration = config,
				ModelBuilder = new ODataConventionModelBuilder(),
				RoutingConventions = ODataRoutingConventions.CreateDefault()
			};

			config.DependencyResolver = new AutofacWebApiDependencyResolver();

			var odataFormatters = ODataMediaTypeFormatters.Create();
			config.Formatters.InsertRange(0, odataFormatters);

			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new WebApiContractResolver(config.Formatters.JsonFormatter);
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", "application/json"));
			config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "xml", "application/xml"));

			config.AddODataQueryFilter(new WebApiQueryableAttribute());

			var corsAttribute = new EnableCorsAttribute("*", "*", "*", WebApiGlobal.Header.CorsExposed);
			config.EnableCors(corsAttribute);
            
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

			var configPublisher = (IWebApiConfigurationPublisher)config.DependencyResolver.GetService(typeof(IWebApiConfigurationPublisher));
			configPublisher.Configure(configBroadcaster);

			//config.Services.Insert(typeof(ModelBinderProvider), 0,
			//	new SimpleModelBinderProvider(typeof(Address), new AddressModelBinder()));

			try
			{
				if (!config.Routes.ContainsKey(WebApiGlobal.RouteNameUploads))
				{
					config.Routes.MapHttpRoute(WebApiGlobal.RouteNameUploads, "api/{version}/Uploads/{action}/{id}",
						new { version = "v1", controller = "Uploads", action = "Index", id = RouteParameter.Optional });
				}

				if (!config.Routes.ContainsKey(WebApiGlobal.RouteNameDefaultApi))
				{
					config.Routes.MapHttpRoute(WebApiGlobal.RouteNameDefaultApi, "api/{version}/{controller}/{id}",
						new { version = "v1", controller = "Home", id = RouteParameter.Optional });
				}
			}
			catch { }

			try
			{
                if (!config.Routes.ContainsKey(WebApiGlobal.RouteNameDefaultOdata))
                {
                    config.MapODataServiceRoute(WebApiGlobal.RouteNameDefaultOdata, WebApiGlobal.MostRecentOdataPath,
                        configBroadcaster.ModelBuilder.GetEdmModel(), new DefaultODataPathHandler(), configBroadcaster.RoutingConventions);
                }
            }
			catch {	}
		}
    }
}
