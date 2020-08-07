using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Security;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class LanguagesController : WebApiEntityController<Language, ILanguageService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Language.Read)]
		public IQueryable<Language> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Language.Read)]
        public SingleResult<Language> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Language.Create)]
		public IHttpActionResult Post(Language entity)
		{
			var result = Insert(entity, () => Service.InsertLanguage(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Language.Update)]
		public async Task<IHttpActionResult> Put(int key, Language entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateLanguage(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Language.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Language> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateLanguage(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Language.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteLanguage(entity));
			return result;
		}
	}
}
