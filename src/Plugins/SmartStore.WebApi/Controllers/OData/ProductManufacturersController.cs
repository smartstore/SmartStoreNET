using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductManufacturersController : WebApiEntityController<ProductManufacturer, IManufacturerService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        protected override void Insert(ProductManufacturer entity)
		{
            Service.InsertProductManufacturer(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        protected override void Update(ProductManufacturer entity)
		{
            Service.UpdateProductManufacturer(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditManufacturer)]
        protected override void Delete(ProductManufacturer entity)
		{
			Service.DeleteProductManufacturer(entity);
		}

        // Navigation properties.

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public SingleResult<Manufacturer> GetManufacturer(int key)
        {
            return GetRelatedEntity(key, x => x.Manufacturer);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
        {
            return GetRelatedEntity(key, x => x.Product);
        }
    }
}
