using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class MeasureWeightsController : WebApiEntityController<MeasureWeight, IMeasureService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Create)]
		protected override void Insert(MeasureWeight entity)
		{
			Service.InsertMeasureWeight(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
        protected override void Update(MeasureWeight entity)
		{
			Service.UpdateMeasureWeight(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Delete)]
        protected override void Delete(MeasureWeight entity)
		{
			Service.DeleteMeasureWeight(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public SingleResult<MeasureWeight> GetMeasureWeight(int key)
		{
			return GetSingleResult(key);
		}
	}
}
