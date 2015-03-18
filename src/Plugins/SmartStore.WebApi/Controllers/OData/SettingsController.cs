using System.Web.Http;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageSettings")]
	public class SettingsController : WebApiEntityController<Setting, ISettingService>
	{
		protected override void Insert(Setting entity)
		{
			Service.InsertSetting(entity);
		}
		protected override void Update(Setting entity)
		{
			Service.UpdateSetting(entity);
		}
		protected override void Delete(Setting entity)
		{
			Service.DeleteSetting(entity);
		}

		[WebApiQueryable]
		public SingleResult<Setting> GetSetting(int key)
		{
			return GetSingleResult(key);
		}
	}
}
