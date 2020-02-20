using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	public class ProductPicturesController : WebApiEntityController<ProductPicture, IProductService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        protected override void Insert(ProductPicture entity)
		{
            Service.InsertProductPicture(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        protected override void Update(ProductPicture entity)
		{
            Service.UpdateProductPicture(entity);
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        protected override void Delete(ProductPicture entity)
		{
			Service.DeleteProductPicture(entity);
		}

        // Navigation properties.

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Picture> GetPicture(int key)
        {
            return GetRelatedEntity(key, x => x.Picture);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
        {
            return GetRelatedEntity(key, x => x.Product);
        }
    }
}
