using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
		public IQueryable<MeasureWeight> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public SingleResult<MeasureWeight> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
		public IHttpActionResult GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Create)]
		public IHttpActionResult Post(MeasureWeight entity)
		{
			var result = Insert(entity, () => Service.InsertMeasureWeight(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
		public async Task<IHttpActionResult> Put(int key, MeasureWeight entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateMeasureWeight(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<MeasureWeight> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateMeasureWeight(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteMeasureWeight(entity));
			return result;
		}
	}
}
