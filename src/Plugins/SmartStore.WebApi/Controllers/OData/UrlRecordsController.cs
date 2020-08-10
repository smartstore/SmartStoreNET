using System.Linq;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
	public class UrlRecordsController : WebApiEntityController<UrlRecord, IUrlRecordService>
	{
		[WebApiQueryable]
		public IQueryable<UrlRecord> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
		public SingleResult<UrlRecord> Get(int key)
		{
			return GetSingleResult(key);
		}

		public IHttpActionResult Post(UrlRecord entity)
		{
			throw this.ExceptionForbidden();
		}

		public IHttpActionResult Put(int key, UrlRecord entity)
		{
			throw this.ExceptionForbidden();
		}

		public IHttpActionResult Patch(int key, Delta<UrlRecord> model)
		{
			throw this.ExceptionForbidden();
		}

		public IHttpActionResult Delete(int key)
		{
			throw this.ExceptionForbidden();
		}
	}
}
