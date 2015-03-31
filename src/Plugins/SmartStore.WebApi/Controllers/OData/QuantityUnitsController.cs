using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageMeasures")]
	public class QuantityUnitsController : WebApiEntityController<QuantityUnit, IQuantityUnitService>
	{
		protected override void Insert(QuantityUnit entity)
		{
			Service.InsertQuantityUnit(entity);
		}
		protected override void Update(QuantityUnit entity)
		{
			Service.UpdateQuantityUnit(entity);
		}
		protected override void Delete(QuantityUnit entity)
		{
			Service.DeleteQuantityUnit(entity);
		}

		[WebApiQueryable]
		public SingleResult<QuantityUnit> GetQuantityUnit(int key)
		{
			return GetSingleResult(key);
		}
	}
}
