using System.Web.Http;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class TaxCategoriesController : WebApiEntityController<TaxCategory, ITaxCategoryService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Tax.Create)]
        protected override void Insert(TaxCategory entity)
		{
			Service.InsertTaxCategory(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Tax.Update)]
        protected override void Update(TaxCategory entity)
		{
			Service.UpdateTaxCategory(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Tax.Delete)]
        protected override void Delete(TaxCategory entity)
		{
			Service.DeleteTaxCategory(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Tax.Read)]
        public SingleResult<TaxCategory> GetTaxCategory(int key)
		{
			return GetSingleResult(key);
		}
	}
}
