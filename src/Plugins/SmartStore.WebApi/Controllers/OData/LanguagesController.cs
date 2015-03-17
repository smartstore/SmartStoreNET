using System.Web.Http;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageLanguages")]
	public class LanguagesController : WebApiEntityController<Language, ILanguageService>
	{
		protected override void Insert(Language entity)
		{
			Service.InsertLanguage(entity);
		}
		protected override void Update(Language entity)
		{
			Service.UpdateLanguage(entity);
		}
		protected override void Delete(Language entity)
		{
			Service.DeleteLanguage(entity);
		}

		[WebApiQueryable]
		public SingleResult<Language> GetLanguage(int key)
		{
			return GetSingleResult(key);
		}
	}
}
