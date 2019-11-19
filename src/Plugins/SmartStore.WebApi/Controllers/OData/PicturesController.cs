using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
	public class PicturesController : WebApiEntityController<Picture, IPictureService>
	{
		protected override void Insert(Picture entity)
		{
			throw this.ExceptionNotImplemented();
		}

        protected override void Update(Picture entity)
		{
			throw this.ExceptionNotImplemented();
		}

        protected override void Delete(Picture entity)
		{
			Service.DeletePicture(entity);
		}

		[WebApiQueryable]
        public SingleResult<Picture> GetPicture(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        public IQueryable<ProductPicture> GetProductPictures(int key)
		{
			return GetRelatedCollection(key, x => x.ProductPictures);
		}
	}
}
