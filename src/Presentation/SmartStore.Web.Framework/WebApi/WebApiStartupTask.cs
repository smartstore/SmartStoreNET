using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;

namespace SmartStore.Web.Framework.WebApi
{
	public class WebApiStartupTask : IStartupTask
    {
        
        public void Execute()
        {
			var config = GlobalConfiguration.Configuration;

			var configBroadcaster = new WebApiConfigurationBroadcaster
			{
				Configuration = config,
				ModelBuilder = new ODataConventionModelBuilder(),
				RoutingConventions = ODataRoutingConventions.CreateDefault()
			};

			config.DependencyResolver = new AutofacWebApiDependencyResolver();

			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
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
			catch (Exception) { }

			try
			{
				if (!config.Routes.ContainsKey(WebApiGlobal.RouteNameDefaultOdata))
				{
					config.Routes.MapODataServiceRoute(WebApiGlobal.RouteNameDefaultOdata, WebApiGlobal.MostRecentOdataPath,
						configBroadcaster.ModelBuilder.GetEdmModel(), new DefaultODataPathHandler(), configBroadcaster.RoutingConventions);
				}
			}
			catch (Exception) { }
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
