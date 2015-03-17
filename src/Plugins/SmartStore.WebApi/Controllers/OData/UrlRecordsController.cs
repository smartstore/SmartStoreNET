using System.Web.Http;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageMaintenance")]
	public class UrlRecordsController : WebApiEntityController<UrlRecord, IUrlRecordService>
	{
		protected override void Insert(UrlRecord entity)
		{
			throw this.ExceptionForbidden();
		}
		protected override void Update(UrlRecord entity)
		{
			throw this.ExceptionForbidden();
		}
		protected override void Delete(UrlRecord entity)
		{
			throw this.ExceptionForbidden();
		}

		[WebApiQueryable]
		public SingleResult<UrlRecord> GetUrlRecord(int key)
		{
			return GetSingleResult(key);
		}
	}
}
