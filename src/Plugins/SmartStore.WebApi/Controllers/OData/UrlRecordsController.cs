using System.Net;
using System.Web.Http;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
    [IEEE754Compatible]
    public class UrlRecordsController : WebApiEntityController<UrlRecord, IUrlRecordService>
    {
        [WebApiQueryable]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        public IHttpActionResult Post()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        [WebApiQueryable]
        public IHttpActionResult Put()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        [WebApiQueryable]
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
