using System.Linq;
using System.Net;
using System.Web.Http;
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

		public IHttpActionResult GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		public IHttpActionResult Post()
		{
			return StatusCode(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Put()
		{
			return StatusCode(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Patch()
		{
			return StatusCode(HttpStatusCode.Forbidden);
		}

		public IHttpActionResult Delete()
		{
			return StatusCode(HttpStatusCode.Forbidden);
		}
	}
}
