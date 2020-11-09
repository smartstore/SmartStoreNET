using System;
using Swashbuckle.Swagger;

namespace SmartStore.WebApi.Services.Swagger
{
    public class SwaggerSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            //if (type == typeof(Product))
            //{
            //	schema.example = new Product
            //	{
            //		Id = 123,
            //		Type = ProductType.Book,
            //		Description = "Treasure Island",
            //		UnitPrice = 10.0M
            //	};
            //}

            //if (schema.properties.ContainsKey("Version"))
            //{
            //	var version = schema.properties["Version"];
            //	if (version.@default == null)
            //		version.@default = "v1";
            //}
        }
    }
}