using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductCategoriesController : WebApiEntityController<ProductCategory, ICategoryService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        protected override void Insert(ProductCategory entity)
		{
            Service.InsertProductCategory(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        protected override void Update(ProductCategory entity)
		{
            Service.UpdateProductCategory(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditCategory)]
        protected override void Delete(ProductCategory entity)
		{
			Service.DeleteProductCategory(entity);
		}

        // Navigation properties.

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public SingleResult<Category> GetCategory(int key)
        {
            return GetRelatedEntity(key, x => x.Category);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
        {
            return GetRelatedEntity(key, x => x.Product);
        }
    }
}
