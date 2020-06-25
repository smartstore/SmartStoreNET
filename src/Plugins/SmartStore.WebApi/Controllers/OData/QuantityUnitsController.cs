using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class QuantityUnitsController : WebApiEntityController<QuantityUnit, IQuantityUnitService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Create)]
        protected override void Insert(QuantityUnit entity)
		{
			Service.InsertQuantityUnit(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
        protected override void Update(QuantityUnit entity)
		{
			Service.UpdateQuantityUnit(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Delete)]
        protected override void Delete(QuantityUnit entity)
		{
			Service.DeleteQuantityUnit(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public SingleResult<QuantityUnit> GetQuantityUnit(int key)
		{
			return GetSingleResult(key);
		}
	}
}
