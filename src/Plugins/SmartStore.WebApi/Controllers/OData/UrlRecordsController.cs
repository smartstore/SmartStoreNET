using System.Linq;
using System.Net;
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
			throw new HttpResponseException(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Put(int key, UrlRecord entity)
		{
			throw new HttpResponseException(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Patch(int key, Delta<UrlRecord> model)
		{
			throw new HttpResponseException(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Delete(int key)
		{
			throw new HttpResponseException(HttpStatusCode.Forbidden);
		}
	}
}
