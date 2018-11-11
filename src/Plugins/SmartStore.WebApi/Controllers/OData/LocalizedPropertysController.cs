using System.Web.Http;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageLanguages")]
	public class LocalizedPropertysController : WebApiEntityController<LocalizedProperty, ILocalizedEntityService>
	{
		protected override void Insert(LocalizedProperty entity)
		{
			Service.InsertLocalizedProperty(entity);
		}
		protected override void Update(LocalizedProperty entity)
		{
			Service.UpdateLocalizedProperty(entity);
		}
		protected override void Delete(LocalizedProperty entity)
		{
			Service.DeleteLocalizedProperty(entity);
		}

		[WebApiQueryable]
		public SingleResult<LocalizedProperty> GetLocalizedProperty(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Language> GetLanguage(int key)
		{
			return GetRelatedEntity(key, x => x.Language);
		}
	}
}
