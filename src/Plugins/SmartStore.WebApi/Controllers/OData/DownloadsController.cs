using System.Web.Http;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class DownloadsController : WebApiEntityController<Download, IDownloadService>
	{
        [WebApiAuthenticate(Permission = Permissions.Media.Download.Create)]
		protected override void Insert(Download entity)
		{
			Service.InsertDownload(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Media.Download.Update)]
        protected override void Update(Download entity)
		{
			Service.UpdateDownload(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Media.Download.Delete)]
        protected override void Delete(Download entity)
		{
			Service.DeleteDownload(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Media.Download.Read)]
        public SingleResult<Download> GetDownload(int key)
		{
			return GetSingleResult(key);
		}
	}
}
