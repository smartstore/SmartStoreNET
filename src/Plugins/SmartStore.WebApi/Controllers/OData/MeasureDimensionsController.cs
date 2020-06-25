using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class MeasureDimensionsController : WebApiEntityController<MeasureDimension, IMeasureService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Create)]
		protected override void Insert(MeasureDimension entity)
		{
			Service.InsertMeasureDimension(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
        protected override void Update(MeasureDimension entity)
		{
			Service.UpdateMeasureDimension(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Delete)]
        protected override void Delete(MeasureDimension entity)
		{
			Service.DeleteMeasureDimension(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public SingleResult<MeasureDimension> GetMeasureDimension(int key)
		{
			return GetSingleResult(key);
		}
	}
}
