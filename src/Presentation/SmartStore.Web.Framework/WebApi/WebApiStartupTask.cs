using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
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
            //config.MapHttpAttributeRoutes();

            // Causes errors during XML serialization:
            //var oDataFormatters = ODataMediaTypeFormatters.Create();
            //config.Formatters.InsertRange(0, oDataFormatters);

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            json.SerializerSettings.ContractResolver = new WebApiContractResolver(config.Formatters.JsonFormatter);
            json.AddQueryStringMapping("$format", "json", "application/json");

            var xml = config.Formatters.XmlFormatter;
            xml.UseXmlSerializer = true;
            xml.Indent = true;
            xml.AddQueryStringMapping("$format", "xml", "application/xml");

            config.AddODataQueryFilter(new WebApiQueryableAttribute());

            var corsAttribute = new EnableCorsAttribute("*", "*", "*", WebApiGlobal.Header.CorsExposed);
            config.EnableCors(corsAttribute);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // OData V4 uses DateTimeOffset to represent DateTime. But converting between DateTimeOffset and DateTime will lose the time zone information.
            // Without UTC time zone configuration, OData would convert date values to the server's standard local time.
            // This would overwrite the UTC values in the database with local time values.
            // See https://docs.microsoft.com/en-us/odata/webapi/datetime-support
            config.SetTimeZoneInfo(TimeZoneInfo.Utc);

            // Allow OData actions and functions without the need for namespaces (OData V3 backward compatibility).
            // A namespace URL world be for example: /Products(123)/ProductService.FinalPrice
            // Note: the dot in this URL will cause IIS to return error 404. See ExtensionlessUrlHandler-Integrated-4.0.
            config.EnableUnqualifiedNameCall(true);

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
            catch { }
        }
    }
}
