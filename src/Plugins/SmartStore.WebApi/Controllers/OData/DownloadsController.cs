using System.Web.Http;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class DownloadsController : WebApiEntityController<Download, IDownloadService>
	{
		protected override void Insert(Download entity)
		{
			Service.InsertDownload(entity);
		}
		protected override void Update(Download entity)
		{
			Service.UpdateDownload(entity);
		}
		protected override void Delete(Download entity)
		{
			Service.DeleteDownload(entity);
		}

		[WebApiQueryable]
		public SingleResult<Download> GetDownload(int key)
		{
			return GetSingleResult(key);
		}
	}
}
