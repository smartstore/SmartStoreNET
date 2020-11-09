using System;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace SmartStore.WebApi.Services.Swagger
{
    public class SwaggerDefaultValueFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                return;

            //var actionParams = apiDescription.ActionDescriptor.GetParameters();
            var customAttributes = apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerDefaultValueAttribute>(true);

            foreach (var param in operation.parameters.Where(x => x.@default == null))
            {
                // set custom values specified via attribute
                var customAttribute = customAttributes.FirstOrDefault(p => p.ParameterName == param.name);
                if (customAttribute != null)
                {
                    param.@default = customAttribute.Value;
                }

                // set global default values (attribute decoration not necessary)
                if (param.@default == null)
                {
                    // set version number for non-odata endpoints
                    if (param.@in.IsCaseInsensitiveEqual("path") && param.name.IsCaseInsensitiveEqual("version"))
                    {
                        param.@default = "v1";
                    }
                }
            }
        }
    }


    public class SwaggerDefaultValueAttribute : Attribute
    {
        public SwaggerDefaultValueAttribute(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
        }

        public string ParameterName { get; private set; }
        public object Value { get; set; }
    }
}