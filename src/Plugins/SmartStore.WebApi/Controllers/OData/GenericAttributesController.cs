using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
	public class GenericAttributesController : WebApiEntityController<GenericAttribute, IGenericAttributeService>
	{
		[WebApiQueryable]
		public IQueryable<GenericAttribute> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
		public SingleResult<GenericAttribute> Get(int key)
		{
			return GetSingleResult(key);
		}

		public IHttpActionResult Post(GenericAttribute entity)
		{
			var result = Insert(entity, () => Service.InsertAttribute(entity));
			return result;
		}

		public async Task<IHttpActionResult> Put(int key, GenericAttribute entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateAttribute(entity));
			return result;
		}

		public async Task<IHttpActionResult> Patch(int key, Delta<GenericAttribute> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateAttribute(entity));
			return result;
		}

		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteAttribute(entity));
			return result;
		}
	}
}
