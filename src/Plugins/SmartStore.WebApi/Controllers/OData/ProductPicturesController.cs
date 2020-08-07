using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	public class ProductPicturesController : WebApiEntityController<ProductMediaFile, IProductService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
		public IQueryable<ProductMediaFile> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
		public SingleResult<ProductMediaFile> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
		public IHttpActionResult Post(ProductMediaFile entity)
		{
			var result = Insert(entity, () => Service.InsertProductPicture(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
		public async Task<IHttpActionResult> Put(int key, ProductMediaFile entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateProductPicture(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
		public async Task<IHttpActionResult> Patch(int key, Delta<ProductMediaFile> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateProductPicture(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteProductPicture(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<MediaFile> GetPicture(int key)
        {
            return GetRelatedEntity(key, x => x.MediaFile);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
        {
            return GetRelatedEntity(key, x => x.Product);
        }

        #endregion
    }
}
