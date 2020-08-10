using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Read)]
		public IQueryable<Setting> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Read)]
        public SingleResult<Setting> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Create)]
		public IHttpActionResult Post(Setting entity)
		{
			var result = Insert(entity, () => Service.InsertSetting(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Update)]
		public async Task<IHttpActionResult> Put(int key, Setting entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateSetting(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Setting> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateSetting(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Setting.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteSetting(entity));
			return result;
		}
	}
}
