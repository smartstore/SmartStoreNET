using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;
using Swashbuckle.Swagger;

namespace SmartStore.WebApi.Services.Swagger
{
    // https://github.com/domaindrivendev/Swashbuckle/issues/149
    public class SwaggerOdataDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.info.title = "Smartstore Web-API";

            var thisAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();

            var odataMethods = new[] { "Get", "Put", "Post", "Patch", "Delete" };
            var odataControllers = thisAssemblyTypes.Where(t => t.IsSubclassOf(typeof(ODataController))).ToList();
            var odataRoutes = GlobalConfiguration.Configuration.Routes.Where(a => a.GetType() == typeof(ODataRoute)).ToList();

            if (!odataRoutes.Any() || !odataControllers.Any())
                return;

            var route = odataRoutes.FirstOrDefault() as ODataRoute;

            foreach (var odataContoller in odataControllers.OrderBy(x => x.Name))
            {
                var methods = odataContoller.GetMethods().Where(a => odataMethods.Contains(a.Name)).ToList();
                if (!methods.Any())
                    continue;

                foreach (var method in methods)
                {
                    var path = "/" + route.RoutePrefix + "/" + odataContoller.Name.Replace("Controller", "");

                    if (swaggerDoc.paths.ContainsKey(path))
                        continue;

                    var odataPathItem = new PathItem();
                    var op = new Operation();

                    // This is assuming that all of the odata methods will be listed under a heading called OData in the swagger doc
                    op.tags = new List<string> { "OData" };
                    op.operationId = "OData_" + odataContoller.Name.Replace("Controller", "");

                    // This should probably be retrieved from XML code comments....
                    op.summary = "Summary for your method / data";
                    op.description = "Here is where we go deep into the description and options for the call.";

                    op.consumes = new List<string>();
                    op.produces = new List<string> { "application/json", "text/json", "application/xml", "text/xml" };
                    op.deprecated = false;

                    var response = new Response { description = "OK" };
                    response.schema = new Schema { type = "array", items = schemaRegistry.GetOrRegister(method.ReturnType) };
                    op.responses = new Dictionary<string, Response> { { "200", response } };

                    // this needs to be a switch based on the method name
                    odataPathItem.get = op;

                    swaggerDoc.paths.Add(path, odataPathItem);
                }
            }
        }
    }
}