using System.Web.Http;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Security;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class SettingsController : WebApiEntityController<Setting, ISettingService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Create)]
		protected override void Insert(Setting entity)
		{
			Service.InsertSetting(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Update)]
        protected override void Update(Setting entity)
		{
			Service.UpdateSetting(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Delete)]
        protected override void Delete(Setting entity)
		{
			Service.DeleteSetting(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Read)]
        public SingleResult<Setting> GetSetting(int key)
		{
			return GetSingleResult(key);
		}
	}
}
